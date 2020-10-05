using System;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;
using MCGalaxy;
using MCGalaxy.Blocks;
using BlockID = System.UInt16;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Network;
using MCGalaxy.Tasks;

namespace NotAwesomeSurvival {

    public partial class NasPlayer {
        public static void Setup() {
            OnPlayerSpawningEvent.Register(OnPlayerSpawning, Priority.High);
        }
        public static void TakeDown() {
            OnPlayerSpawningEvent.Unregister(OnPlayerSpawning);
        }

        static void OnPlayerSpawning(Player p, ref Position pos, ref byte yaw, ref byte pitch, bool respawning) {
            //if (respawning) { return; }
            NasPlayer np = (NasPlayer)p.Extras[Nas.PlayerKey];
            np.SpawnPlayer(p.level, ref pos, ref yaw, ref pitch);
        }

        public void SpawnPlayer(Level level, ref Position spawnPos, ref byte yaw, ref byte pitch) {
            if (level.Config.Deletable && level.Config.Buildable) { return; } //not a nas map

            Logger.Log("SpawnPlayer");

            inventory.Setup();
            if (!hasBeenSpawned) { SpawnPlayerFirstTime(level, ref spawnPos, ref yaw, ref pitch); return; }

            if (transferInfo != null) {
                transferInfo.CalcNewPos();
                spawnPos = transferInfo.posBeforeMapChange;
                yaw = transferInfo.yawBeforeMapChange;
                pitch = transferInfo.pitchBeforeMapChange;

                p.Message("You got spawned");
                atBorder = true;
                transferInfo = null;
            }
        }
        public void SpawnPlayerFirstTime(Level level, ref Position spawnPos, ref byte yaw, ref byte pitch) {
            if (hasBeenSpawned) { return; }
            atBorder = true;
            if (p.Model != "|0.93023255813953488372093023255814") { Command.Find("model").Use(p, "|0.93023255813953488372093023255814"); }
            //p.Message("HP is {0}", HP);
            if (HP == 0) {
                //p.Message("resetting date they died");
                lastDeathDate = DateTime.UtcNow;
            }
            spawnPos = new Position(location.X, location.Y, location.Z);
            yaw = this.yaw;
            pitch = this.pitch;
            Logger.Log(LogType.Debug, "Teleporting " + p.name + "!");

            if (level.name != levelName) {
                //goto will call OnPlayerSpawning again to complete the spawn
                Command.Find("goto").Use(p, levelName);
                return;
            }
            hasBeenSpawned = true;
        }
        [JsonIgnore] int round = 0;
        public void DoMovement(Position next, byte yaw, byte pitch) {
            
            CheckGround();
            CheckMapCrossing(p.Pos);
            UpdateHeldBlock();
            UpdateCaveFog(next);
            round++;
        }
        void CheckGround() {
            Position below = p.Pos;
            below.Y-= 1;
            float fallDamageMultiplier = 1;
            if (Collision.TouchesGround(p.level, bounds, below, out fallDamageMultiplier)) {

                float fallHeight = lastGroundedLocation.Y - p.Pos.Y;
                if (fallHeight > 0) {
                    fallHeight /= 32f;
                    fallHeight-= 3;
                    
                    if (fallHeight > 0) {
                        float damage = (int)fallHeight * 2;
                        damage /= 4;
                        p.Message("damage is {0}", damage*fallDamageMultiplier);
                        TakeDamage(damage*fallDamageMultiplier, "falling");
                    }
                }
                lastGroundedLocation = new MCGalaxy.Maths.Vec3S32(p.Pos.X, p.Pos.Y, p.Pos.Z);
            }
        }
        [JsonIgnoreAttribute] bool atBorder = true;
        void CheckMapCrossing(Position next) {
            if (next.BlockX <= 0) {
                TryGoMapAt(-1, 0);
                return;
            }
            if (next.BlockX >= p.level.Width - 1) {
                TryGoMapAt(1, 0);
                return;
            }

            if (next.BlockZ <= 0) {
                TryGoMapAt(0, -1);
                return;
            }
            if (next.BlockZ >= p.level.Length - 1) {
                TryGoMapAt(0, 1);
                return;
            }
            atBorder = false;
        }
        void TryGoMapAt(int chunkOffsetX, int chunkOffsetZ) {
            if (atBorder) {
                //p.Message("Can't do it because already at border");
                return;
            }
            atBorder = true;
            int chunkX = 0, chunkZ = 0;
            string[] bits = p.level.name.Split(',');
            if (bits.Length >= 2) {
                if (!Int32.TryParse(bits[0], out chunkX)) { return; }
                if (!Int32.TryParse(bits[1], out chunkZ)) { return; }
            } else {
                return;
            }
            chunkX += chunkOffsetX;
            chunkZ += chunkOffsetZ;
            string mapName = chunkX + "," + chunkZ;
            if (File.Exists("levels/" + mapName + ".lvl")) {
                transferInfo = new TransferInfo(p, chunkOffsetX, chunkOffsetZ);
                Command.Find("goto").Use(p, mapName);
            } else {
                if (NasGen.currentlyGenerating) {
                    p.Message("A map is already generating! Please try again when it is finished.");
                    return;
                }
                GenInfo info = new GenInfo();
                info.p = p;
                info.mapName = mapName;
                SchedulerTask taskGenMap;
                taskGenMap = NasGen.genScheduler.QueueOnce(GenTask, info, TimeSpan.Zero);
            }
        }
        class GenInfo {
            public Player p;
            public string mapName;
        }
        static void GenTask(SchedulerTask task) {
            GenInfo info = (GenInfo)task.State;
            Command.Find("newlvl").Use(info.p, info.mapName + " " + NasGen.mapDims + " " + NasGen.mapDims + " " + NasGen.mapDims + " nasgen " + NasGen.seed);
        }
        [JsonIgnore] public TransferInfo transferInfo = null;
        public class TransferInfo {
            public TransferInfo(Player p, int chunkOffsetX, int chunkOffsetZ) {
                posBeforeMapChange = p.Pos;
                yawBeforeMapChange = p.Rot.RotY;
                pitchBeforeMapChange = p.Rot.HeadX;
                this.chunkOffsetX = chunkOffsetX;
                this.chunkOffsetZ = chunkOffsetZ;
            }
            public void CalcNewPos() {
                int xOffset = chunkOffsetX * NasGen.mapDims * 32;
                int zOffset = chunkOffsetZ * NasGen.mapDims * 32;
                posBeforeMapChange.X -= xOffset;
                posBeforeMapChange.Z -= zOffset;
            }
            [JsonIgnore] public Position posBeforeMapChange;
            [JsonIgnore] public byte yawBeforeMapChange;
            [JsonIgnore] public byte pitchBeforeMapChange;
            [JsonIgnore] public int chunkOffsetX, chunkOffsetZ;
        }

        public void UpdateCaveFog(Position next) {
            if (!NasLevel.all.ContainsKey(p.level.name)) { return; }

            const float change = 0.03125f;
            if (curRenderDistance > targetRenderDistance) {
                curRenderDistance *= 1 - change;
                if (curRenderDistance < targetRenderDistance) { curRenderDistance = targetRenderDistance; }
            } else if (curRenderDistance < targetRenderDistance) {
                curRenderDistance *= 1 + change;
                if (curRenderDistance > targetRenderDistance) { curRenderDistance = targetRenderDistance; }
            }
            curFogColor = ScaleColor(curFogColor, targetFogColor);

            p.Send(Packet.EnvMapProperty(EnvProp.MaxFog, (int)curRenderDistance));
            p.Send(Packet.EnvColor(2, curFogColor.R, curFogColor.G, curFogColor.B));

            NasLevel nl = NasLevel.all[p.level.name];
            int x = next.BlockX;
            int z = next.BlockZ;
            x = Utils.Clamp(x, 0, (ushort)(p.level.Width - 1));
            z = Utils.Clamp(z, 0, (ushort)(p.level.Length - 1));
            ushort height = nl.heightmap[x, z];

            if (next.BlockCoords == p.Pos.BlockCoords) { return; }

            if (height < NasGen.oceanHeight) { height = NasGen.oceanHeight; }


            int distanceBelow = height - next.BlockY;
            int expFog = 0;
            if (distanceBelow >= NasGen.diamondDepth) {
                targetRenderDistance = 16;
                targetFogColor = NasGen.diamondFogColor;
                expFog = 1;
            } else if (distanceBelow >= NasGen.goldDepth) {
                targetRenderDistance = 24;
                targetFogColor = NasGen.goldFogColor;
                expFog = 1;
            } else if (distanceBelow >= NasGen.ironDepth) {
                targetRenderDistance = 32;
                targetFogColor = NasGen.ironFogColor;
                expFog = 1;
            } else if (distanceBelow >= NasGen.coalDepth) {
                targetRenderDistance = 64;
                targetFogColor = NasGen.coalFogColor;
                expFog = 1;
            } else {
                targetRenderDistance = Server.Config.MaxFogDistance;
                targetFogColor = Color.White;
                expFog = 0;
            }
            p.Send(Packet.EnvMapProperty(EnvProp.ExpFog, expFog));
        }

        static Color ScaleColor(Color cur, Color goal) {
            byte R = ScaleChannel(cur.R, goal.R);
            byte G = ScaleChannel(cur.G, goal.G);
            byte B = ScaleChannel(cur.B, goal.B);
            return Color.FromArgb(R, G, B);
        }
        static byte ScaleChannel(byte curChannel, byte goalChannel) {
            if (curChannel > goalChannel) {
                curChannel--;
            } else if (curChannel < goalChannel) {
                curChannel++;
            }
            return curChannel;
        }
    }

}
