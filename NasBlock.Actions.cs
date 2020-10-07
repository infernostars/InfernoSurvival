using System;
using System.Collections.Generic;
using MCGalaxy;
using MCGalaxy.Blocks;
using MCGalaxy.Maths;
using BlockID = System.UInt16;
using NasBlockAction = System.Action<NotAwesomeSurvival.NasLevel, int, int, int>;

namespace NotAwesomeSurvival {

    public partial class NasBlock {
        static NasBlockAction FloodAction(BlockID serverBlockID) {
            return (nl,x,y,z) => {
                if (nl.GetBlock(x, y-1, z) == Block.Air) {
                    nl.SetBlock(x, y-1, z, serverBlockID);
                }
                if (nl.GetBlock(x+1, y, z) == Block.Air) {
                    nl.SetBlock(x+1, y, z, serverBlockID);
                }
                if (nl.GetBlock(x-1, y, z) == Block.Air) {
                    nl.SetBlock(x-1, y, z, serverBlockID);
                }
                if (nl.GetBlock(x, y, z+1) == Block.Air) {
                    nl.SetBlock(x, y, z+1, serverBlockID);
                }
                if (nl.GetBlock(x, y, z-1) == Block.Air) {
                    nl.SetBlock(x, y, z-1, serverBlockID);
                }
            };
        }
        
        static int LiquidInfiniteIndex = 0;
        static int LiquidSourceIndex = 1;
        static int LiquidWaterfallIndex = 2;
        /// <summary>
        /// First ID is the infinite-flood version of the liquid, second is the source, third is waterfall, the rest are heights from tallest to shortest
        /// </summary>
        static BlockID[] waterSet = new BlockID[] { 8, 9, Block.Extended|639,
            Block.Extended|632,
            Block.Extended|633,
            Block.Extended|634,
            Block.Extended|635,
            Block.Extended|636,
            Block.Extended|637,
            Block.Extended|638 };
        
        /// <summary>
        /// Check if the given block exists within the given set.
        /// </summary>
        /// <returns>The index of the set that the block is at
        /// or -1 if the block does not exist within the set.
        /// </returns>
        static int IsPartOfSet(BlockID[] set, BlockID block) {
            for (int i = 0; i < set.Length; i++) {
                if (set[i] == block) { return i; }
            }
            return -1;
        }
        static bool CanReplaceBlockAt(NasLevel nl, int x, int y, int z, BlockID[] set, int spreadIndex) {
            BlockID hereBlock = nl.GetBlock(x, y, z);
            if (hereBlock == Block.Air) { return true; }
            
            int hereIndex = IsPartOfSet(set, hereBlock);
            if (hereIndex == -1) { return false; }
            if (hereIndex <= spreadIndex) { return false; }
            return true;
        }
        static bool CanLiquidLive(NasLevel nl, BlockID[] set, int index, int x, int y, int z) {
            BlockID neighbor = nl.GetBlock(x, y, z);
            if (neighbor == set[index-1] ||
                neighbor == set[LiquidSourceIndex] ||
                neighbor == set[LiquidWaterfallIndex]
               ) {
                return true;
            }
            return false;
        }
        
        static NasBlockAction LimitedFloodAction(BlockID[] set, int index) {
            return (nl,x,y,z) => {
                //Step one -- Check if we need to drain
                if (index > LiquidSourceIndex) {
                    //it's not a source block
                    
                    if (index == LiquidWaterfallIndex) {
                        //it's a waterfall -- see if it needs to die
                        BlockID aboveHere = nl.GetBlock(x, y+1, z);
                        if (IsPartOfSet(set, aboveHere) == -1) {
                            //nl.lvl.Message("killing waterfall");
                            nl.SetBlock(x, y, z, Block.Air);
                            return;
                        }
                    } else {
                        //it's not a waterfall -- see if it needs to die
                        if (!(CanLiquidLive(nl, set, index, x+1, y, z) ||
                            CanLiquidLive(nl, set, index, x-1, y, z) ||
                            CanLiquidLive(nl, set, index, x, y, z+1) ||
                            CanLiquidLive(nl, set, index, x, y, z-1)) ) {
                            
                            //nl.lvl.Message("killing liquid");
                            nl.SetBlock(x, y, z, Block.Air);
                            return;
                        }
                    }
                }
                
                //Step two -- Do the actual flooding
                
                BlockID below = nl.GetBlock(x, y-1, z);
                int belowIndex = IsPartOfSet(set, below);
                if (below == Block.Air || belowIndex != -1) {
                    //don't override infinite source, source, or waterfall with a waterfall
                    if (below != Block.Air && belowIndex <= LiquidWaterfallIndex) { return; }
                    
                    //nl.lvl.Message("setting waterfall");
                    nl.SetBlock(x, y-1, z, set[LiquidWaterfallIndex]);
                    return;
                }
                
                if (index == set.Length-1) {
                    //it's the end of the stream -- no need to flood further
                    return;
                }
                
                int spreadIndex = (index < LiquidWaterfallIndex+1) ? LiquidWaterfallIndex+1 : index+1;
                BlockID spreadBlock = set[spreadIndex];
                if (CanReplaceBlockAt(nl, x+1, y, z, set, spreadIndex)) {
                    nl.SetBlock(x+1, y, z, spreadBlock);
                }
                if (CanReplaceBlockAt(nl, x-1, y, z, set, spreadIndex)) {
                    nl.SetBlock(x-1, y, z, spreadBlock);
                }
                if (CanReplaceBlockAt(nl, x, y, z+1, set, spreadIndex)) {
                    nl.SetBlock(x, y, z+1, spreadBlock);
                }
                if (CanReplaceBlockAt(nl, x, y, z-1, set, spreadIndex)) {
                    nl.SetBlock(x, y, z-1, spreadBlock);
                }
                
                
            };
        }
        
        static NasBlockAction FallingBlockAction(BlockID serverBlockID) {
            return (nl,x,y,z) => {
                BlockID blockUnder = nl.GetBlock(x, y-1, z);
                if (blockUnder == Block.Air || IsPartOfSet(waterSet, blockUnder) != -1) {
                    nl.SetBlock(x, y, z, Block.Air);
                    nl.SetBlock(x, y-1, z, serverBlockID);
                }
            };
        }
    }

}
