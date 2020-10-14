using System;
using System.Drawing;
using LibNoise;
using MCGalaxy;
using BlockID = System.UInt16;
using MCGalaxy.Tasks;
using MCGalaxy.Generator;
using MCGalaxy.Generator.Foliage;

namespace NotAwesomeSurvival {

    public static class NasTree {
        public static void Setup() {
            
            
        }
        public static void GenOakTree(NasLevel nl, Random r, int x, int y, int z, bool broadcastChange = false) {
            Level lvl = nl.lvl;
            Tree tree;
            tree = new OakTree();
            tree.SetData(r, r.Next(0, 8));
            tree.Generate((ushort)x, (ushort)(y), (ushort)z, (X, Y, Z, raw) => {
                  if (NasBlock.CanPhysicsKillThis(lvl.GetBlock(X, Y, Z)) || lvl.GetBlock(X, Y, Z) == Block.Leaves) {
                      lvl.SetTile(X, Y, Z, raw);
                      if (broadcastChange) {
                          lvl.BroadcastChange(X, Y, Z, raw);
                      }
                  }
            });
        }
    }

}
