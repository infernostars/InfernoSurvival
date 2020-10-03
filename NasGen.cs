using System;
using System.Threading;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Text;

using LibNoise;


using MCGalaxy;
using MCGalaxy.Commands;
using MCGalaxy.Commands.Chatting;
using MCGalaxy.Config;
using MCGalaxy.Blocks;
using MCGalaxy.Events.ServerEvents;
using MCGalaxy.Events.LevelEvents;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Events.EntityEvents;
using BlockID = System.UInt16;

using MCGalaxy.Network;
using MCGalaxy.Maths;
using MCGalaxy.Tasks;

using MCGalaxy.Generator;
using MCGalaxy.Generator.Foliage;

namespace NotAwesomeSurvival {
    
    public static class NasGen {
        public const int mapDims = 256;
        public const string seed = "a";
        public const ushort oceanHeight = 64;
        public const ushort coalDepth = 4;
        public const ushort ironDepth = 16;
        public const ushort goldDepth = 50;
        public const ushort diamondDepth = 56;
        public const float coalChance = 1f;
        public const float ironChance = 0.25f;
        public const float goldChance = 0.125f;
        public const float diamondChance = goldChance*0.25f;
        public static Color coalFogColor;
        public static Color ironFogColor;
        public static Color goldFogColor;
        public static Color diamondFogColor;
        
        public static Scheduler genScheduler;
        
        public static void Setup() {
            if (genScheduler == null) genScheduler = new Scheduler("MapGenScheduler");
            MapGen.Register("nasGen", GenType.Advanced, Gen, "hello?");
			
            coalFogColor = System.Drawing.ColorTranslator.FromHtml("#BCC9E8");
            ironFogColor = System.Drawing.ColorTranslator.FromHtml("#A1A3A8");
            goldFogColor = System.Drawing.ColorTranslator.FromHtml("#7A706A");
            diamondFogColor = System.Drawing.ColorTranslator.FromHtml("#605854");
        }
        public static void TakeDown() {
            
        }
        public static bool currentlyGenerating = false;
        static bool Gen(Player p, Level lvl, string seed) {
            currentlyGenerating = true;
            int offsetX = 0, offsetZ = 0;
            string[] bits = lvl.name.Split(',');
            if (bits.Length >= 2) {
                Int32.TryParse(bits[0], out offsetX);
                Int32.TryParse(bits[1], out offsetZ);
                offsetX*=mapDims;
                offsetZ*=mapDims;
                //p.Message("offsetX is {0}, offsetZ is {1}", offsetX, offsetZ);
            }
            
            Perlin adjNoise = new Perlin();
            adjNoise.Seed   = MapGen.MakeInt(seed);
            Random r = new Random(adjNoise.Seed);
            DateTime dateStart = DateTime.UtcNow;
            
            GenTerrain(p, lvl, adjNoise, offsetX, offsetZ, r);
            NasLevel nl = new NasLevel();
            CalcHeightmap(p, lvl, ref nl);
            GenSoil(p, lvl, adjNoise, offsetX, offsetZ, r, seed);
            GenCaves(p, lvl, nl, adjNoise, offsetX, offsetZ, r, seed);
            GenPlants(p, lvl, adjNoise, offsetX, offsetZ, r, seed);
            GenOre(p, lvl, nl, r);

            
            lvl.Config.Deletable = false;
            lvl.Config.MOTD = "-hax +thirdperson";
            
            
            TimeSpan timeTaken = DateTime.UtcNow.Subtract(dateStart);
            p.Message("Done in {0}", timeTaken.Shorten(true, true));
            
            //GotoInfo info = new GotoInfo();
            //info.p = p;
            //info.levelName = lvl.name;
            //SchedulerTask task = Server.MainScheduler.QueueOnce(Goto, info, TimeSpan.FromMilliseconds(1500));
            currentlyGenerating = false;
            return true;
        }
        
        static void GenTerrain(Player p, Level lvl, Perlin adjNoise, int offsetX, int offsetZ, Random r) {
            //more frequency = smaller map scale
            adjNoise.Frequency = 0.75;
            adjNoise.OctaveCount = 6;
            DateTime dateStartLayer;
            int counter = 0;
            double width = lvl.Width, height = lvl.Height, length = lvl.Length;
            
            counter = 0;
            dateStartLayer = DateTime.UtcNow;
            for (double y = 0; y < height; y++) {
                //p.Message("Starting {0} layer.", ListicleNumber((int)(y+1)));
                for (double z = 0; z < length; ++z)
                    for (double x = 0; x < width; ++x)
                {
                    if (y == 0) {
                        lvl.SetTile((ushort)x, (ushort)(y), (ushort)z, Block.Bedrock);
                        continue;
                    }
                    double threshold = (((y+(oceanHeight-16))/(height))-0.5)*4.5;
                    if (threshold < -1.5) {
                        lvl.SetTile((ushort)x, (ushort)(y), (ushort)z, Block.Stone);
                        continue;
                    }
                    if (threshold > 1.5) { continue; }
                    
                    //divide y by less for more "layers"
                    double xVal = (x+offsetX)/200, yVal = y/140, zVal = (z+offsetZ)/200;
                    const double adj = 1;
                    xVal+= adj;
                    yVal+= adj;
                    zVal+= adj;
                    double value = adjNoise.GetValue(xVal, yVal, zVal);
                    //if (counter % (256*256) == 0) {
                    //    Thread.Sleep(10);
                    //}
                    //counter++;
                    
                    //multiply by more to more strictly follow halfway under = solid, above = air
    
                    
                    if (value > threshold) {
                        lvl.SetTile((ushort)x, (ushort)(y), (ushort)z, Block.Stone);
                    } else if (y < oceanHeight) {
                        lvl.SetTile((ushort)x, (ushort)(y), (ushort)z, Block.StillWater);
                    }
                }
                TimeSpan span = DateTime.UtcNow.Subtract(dateStartLayer);
                if (span > TimeSpan.FromSeconds(5)) {
                    p.Message("Initial gen {0}% complete.", (int)((y/height)*100) );
                    dateStartLayer = DateTime.UtcNow;
                }
            }
            p.Message("Initial gen 100% complete.");
            
            

        }
        static void CalcHeightmap(Player p, Level lvl, ref NasLevel nl) {
            p.Message("Calculating heightmap");
            nl.heightmap = new ushort[lvl.Width,lvl.Length];
            for (ushort z = 0; z < lvl.Length; ++z)
                for (ushort x = 0; x < lvl.Width; ++x)
            {
                //         skip bedrock
                for (ushort y = 1; y < lvl.Height; ++y) {
                    BlockID curBlock = lvl.FastGetBlock(x, y, z);
                    if (curBlock != Block.Stone) {
                        nl.heightmap[x,z] = (ushort)(y-1);
                        break;
                    }
                }
            }
            NasLevel.all.Add(lvl.name, nl);
        }
        static void GenSoil(Player p, Level lvl, Perlin adjNoise, int offsetX, int offsetZ, Random r, string seed) {
            int width = lvl.Width, height = lvl.Height, length = lvl.Length;
            p.Message("Now creating soil.");
            adjNoise.Seed   = MapGen.MakeInt(seed+"soil");
            adjNoise.Frequency = 1;
            adjNoise.OctaveCount = 6;
            
            for (int y = 0; y < height-1; y++)
                for (int z = 0; z < length; ++z)
                    for (int x = 0; x < width; ++x)
            {
                byte soil = Block.Dirt;
                
                if (lvl.FastGetBlock((ushort)x, (ushort)y, (ushort)z) == Block.Stone &&
                    lvl.FastGetBlock((ushort)x, (ushort)(y+1), (ushort)z) != Block.Stone) {
                    
                    if (y <= oceanHeight-12) {
                        soil = Block.Gravel;
                    }
                    else if (y <= oceanHeight+1) {
                        soil = Block.Sand;
                    }
                    
                    int startY = y;
                    for (int yCol = startY; yCol > startY-2 -r.Next(0, 2); yCol--) {
                        if (yCol < 0) { break; }
                        if (lvl.FastGetBlock((ushort)x, (ushort)(yCol), (ushort)z) == Block.Stone) {
                            lvl.SetTile((ushort)x, (ushort)(yCol), (ushort)z, soil);
                        }
                    }
                }
            }
        }
        static void GenCaves(Player p, Level lvl, NasLevel nl, Perlin adjNoise, int offsetX, int offsetZ, Random r, string seed) {
            int width = lvl.Width, height = lvl.Height, length = lvl.Length;

            p.Message("Now creating caves");
            adjNoise.Seed   = MapGen.MakeInt(seed+"cave");
            adjNoise.Frequency = 1; //more frequency = smaller map scale
            adjNoise.OctaveCount = 2;
            
            int counter = 0;
            DateTime dateStartLayer = DateTime.UtcNow;
            for (double y = 0; y < height; y++) {
                //p.Message("Starting {0} layer.", ListicleNumber((int)(y+1)));
                for (double z = 0; z < length; ++z)
                    for (double x = 0; x < width; ++x)
                {
                    double threshold = 0.55;
                    int caveHeight = nl.heightmap[(int)x,(int)z]-14;
                    if (y > caveHeight) {
                        threshold+= 0.05 * (y - (caveHeight));
                    }
                    if (threshold > 1.5) { continue; }
                    bool tryCave = false;
                    BlockID thisBlock = lvl.FastGetBlock((ushort)x, (ushort)(y), (ushort)z);
                    if (thisBlock == Block.Stone || thisBlock == Block.Dirt) { tryCave = true; }
                    if (!tryCave) {
                        continue;
                    }
                    
                    //divide y by less for more "layers"
                    double xVal = (x+offsetX)/15, yVal = y/7, zVal = (z+offsetZ)/15;
                    const double adj = 1;
                    xVal+= adj;
                    yVal+= adj;
                    zVal+= adj;
                    double value = adjNoise.GetValue(xVal, yVal, zVal);
                    
                    //if (counter % (256*256) == 0) {
                    //    Thread.Sleep(10);
                    //}
                    counter++;
                    
                    if (value > threshold) {
                        if (y <= 4) {
                            lvl.SetTile((ushort)x, (ushort)(y), (ushort)z, Block.StillLava);
                        } else {
                            lvl.SetTile((ushort)x, (ushort)(y), (ushort)z, Block.Air);
                        }
                    }
                }
                TimeSpan span = DateTime.UtcNow.Subtract(dateStartLayer);
                if (span > TimeSpan.FromSeconds(5)) {
                    p.Message("Cave gen {0}% complete.", (int)((y/height)*100) );
                    dateStartLayer = DateTime.UtcNow;
                }
            }
            p.Message("Cave gen 100% complete.");
        }
        static void GenPlants(Player p, Level lvl, Perlin adjNoise, int offsetX, int offsetZ, Random r, string seed) {
            p.Message("Now creating grass and trees.");
            adjNoise.Seed   = MapGen.MakeInt(seed+"tree");
            adjNoise.Frequency = 1;
            adjNoise.OctaveCount = 6;
            
            for (int y = 0; y < (ushort)(lvl.Height-1); y++)
                for (int z = 0; z < lvl.Length; ++z)
                    for (int x = 0; x < lvl.Width; ++x)
            {
                byte topSoil = Block.Grass;
                
                if (lvl.FastGetBlock((ushort)x, (ushort)y, (ushort)z) == Block.Dirt &&
                    lvl.FastGetBlock((ushort)x, (ushort)(y+1), (ushort)z) == Block.Air) {
                    
                    if (r.Next(0, 50) == 0) {
                        
                        double xVal = ((double)x+offsetX)/200, yVal = (double)y/130, zVal = ((double)z+offsetZ)/200;
                        const double adj = 1;
                        xVal+= adj;
                        yVal+= adj;
                        zVal+= adj;
                        double value = adjNoise.GetValue(xVal, yVal, zVal);
                        if (value > 0.0) {
                            topSoil = Block.Dirt;
                            OakTree tree = new OakTree();
                            tree.SetData(r, r.Next(0, 2));
                            tree.Generate((ushort)x, (ushort)(y+1), (ushort)z, (X,Y,Z,raw) => {
                                              if (lvl.IsAirAt(X,Y,Z) || lvl.GetBlock(X, Y, Z) == Block.Leaves) { lvl.SetTile(X,Y,Z, raw); }
                                          });
                        } else if (r.Next(0, 20) == 0) {
                            
                            topSoil = Block.Dirt;
                            OakTree tree = new OakTree();
                            tree.SetData(r, r.Next(0, 2));
                            tree.Generate((ushort)x, (ushort)(y+1), (ushort)z, (X,Y,Z,raw) => {
                                              if (lvl.IsAirAt(X,Y,Z) || lvl.GetBlock(X, Y, Z) == Block.Leaves) { lvl.SetTile(X,Y,Z, raw); }
                                          });
                        }
                    }

                    lvl.SetTile((ushort)x, (ushort)(y), (ushort)z, topSoil);
                }
            }
        }
        
        static void GenOre(Player p, Level lvl, NasLevel nl, Random r) {
            for (int y = 0; y < (ushort)lvl.Height-1; y++)
                for (int z = 0; z < lvl.Length; ++z)
                    for (int x = 0; x < lvl.Width; ++x)
            {
                BlockID curBlock = lvl.FastGetBlock((ushort)x, (ushort)(y), (ushort)z);
                if (curBlock != Block.Stone) { continue; }
                TryGenOre(r, lvl, nl, x, y, z, coalDepth, coalChance, 627);
                TryGenOre(r, lvl, nl, x, y, z, ironDepth, ironChance, 628);
                TryGenOre(r, lvl, nl, x, y, z, goldDepth, goldChance, 629);
                TryGenOre(r, lvl, nl, x, y, z, diamondDepth, diamondChance, 630);
            }
        }
        
        static bool TryGenOre(Random r, Level lvl, NasLevel nl, int x, int y, int z, int oreDepth, float oreChance, BlockID oreID) {
            double chance = (double)(oreChance/100);
            int height = nl.heightmap[x,z];
            if (height < oceanHeight) { height = oceanHeight; }
            int howManyBlocksYouHaveToTravelDownFromTopToReachHeight = lvl.Height-height;
            howManyBlocksYouHaveToTravelDownFromTopToReachHeight+= oreDepth;
            
            if (y <= lvl.Height-howManyBlocksYouHaveToTravelDownFromTopToReachHeight
                && r.NextDouble() <= chance
               ) {
                //if (r.NextDouble() > 0.5) {
                //    if (!BlockExposed(lvl, x, y, z)) { return false; }
                //}
                lvl.SetBlock((ushort)x, (ushort)y, (ushort)z, Block.FromRaw(oreID));
                return true;
            }
            return false;
        }
        static bool BlockExposed(Level lvl, int x, int y, int z) {
            if (lvl.IsAirAt((ushort)(x+1), (ushort)y, (ushort)z)) { return true; }
            if (lvl.IsAirAt((ushort)(x-2), (ushort)y, (ushort)z)) { return true; }
            if (lvl.IsAirAt((ushort)x, (ushort)(y+1), (ushort)z)) { return true; }
            if (lvl.IsAirAt((ushort)x, (ushort)(y-1), (ushort)z)) { return true; }
            if (lvl.IsAirAt((ushort)x, (ushort)y, (ushort)(z+1))) { return true; }
            if (lvl.IsAirAt((ushort)x, (ushort)y, (ushort)(z-1))) { return true; }
            return false;
        }
        
        class GotoInfo {
            public Player p;
            public string levelName;
        }
        static void Goto(SchedulerTask task) {
            GotoInfo info = (GotoInfo)task.State;
            Command.Find("goto").Use(info.p, info.levelName);
        }
    }
    
}