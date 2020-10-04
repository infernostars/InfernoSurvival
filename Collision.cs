using System;
using MCGalaxy;
using MCGalaxy.Blocks;
using MCGalaxy.Maths;
using MCGalaxy.Events.LevelEvents;
using BlockID = System.UInt16;

namespace NotAwesomeSurvival {
    
    public static class Collision {
        public static AABB[] blockBounds;
        public static void Setup() {
            OnLevelLoadedEvent.Register(OnLevelLoaded, Priority.Low);
        }
        static void OnLevelLoaded(Level lvl) {
            if (lvl.name == Server.Config.MainLevel) {
                SetupBlockBounds(lvl);
                OnLevelLoadedEvent.Unregister(OnLevelLoaded);
            }
        }
        public static void SetupBlockBounds(Level lvl) {
            blockBounds = new AABB[Block.ExtendedCount];
            for (BlockID blockID = 0; blockID < Block.ExtendedCount-1; blockID++) {
                //Player.Console.Message("Setting up block AABB {0}", blockID);
                blockBounds[blockID] = Block.BlockAABB(blockID, lvl);
            }
        }
        public static bool TouchesGround(Level lvl, AABB entityAABB, Position entityPos) {
            entityPos.X += entityAABB.Max.X;
            entityPos.Z += entityAABB.Max.Z;
            if (_TouchesGround(lvl, entityAABB, entityPos)) { return true; }
            entityPos.X += entityAABB.Min.X*2;
            if (_TouchesGround(lvl, entityAABB, entityPos)) { return true; }
            entityPos.Z += entityAABB.Min.Z*2;
            if (_TouchesGround(lvl, entityAABB, entityPos)) { return true; }
            entityPos.X += entityAABB.Max.X*2;
            if (_TouchesGround(lvl, entityAABB, entityPos)) { return true; }
            return false;
        }
        public static bool _TouchesGround(Level lvl, AABB entityAABB, Position entityPos) {
            int x = entityPos.FeetBlockCoords.X;
            int y = entityPos.FeetBlockCoords.Y;
            int z = entityPos.FeetBlockCoords.Z;
            BlockID serverBlockID = lvl.GetBlock((ushort)x,
                                                 (ushort)y,
                                                 (ushort)z);
            if (serverBlockID == Block.Air) { return false; }
            AABB blockAABB = blockBounds[serverBlockID];
            
            entityAABB = entityAABB.OffsetPosition(entityPos);
            blockAABB = blockAABB.Offset(x * 32, y * 32, z * 32);
            if (AABB.Intersects(ref entityAABB, ref blockAABB)) {
                return true;
            }
            return false;
        }
    }
    
}