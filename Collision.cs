using System;
using MCGalaxy;
using MCGalaxy.Blocks;
using MCGalaxy.Maths;
using MCGalaxy.Events.LevelEvents;
using BlockID = System.UInt16;

namespace NotAwesomeSurvival {

    public static class Collision {
        public static NasBlock[] nasBlocksIndexedByServerBlockID;
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
            nasBlocksIndexedByServerBlockID = new NasBlock[Block.ExtendedCount];
            for (BlockID blockID = 0; blockID < Block.ExtendedCount-1; blockID++) {
                nasBlocksIndexedByServerBlockID[blockID] = GetNasBlockAndFillInCollisionInformation(blockID, lvl);
            }
        }
        public static NasBlock GetNasBlockAndFillInCollisionInformation (BlockID serverBlockID, Level lvl) {
            //Player.Console.Message("on ID {0}", block);
            bool collides = true;
            AABB bounds;
            float fallDamageMultiplier = 1;
            
            BlockDefinition def = lvl.GetBlockDef(serverBlockID);
            if (def != null) {
                bounds = new AABB(def.MinX * 2, def.MinZ * 2, def.MinY * 2,
                                def.MaxX * 2, def.MaxZ * 2, def.MaxY * 2);
                
                switch (def.CollideType) {
                    case CollideType.ClimbRope:
                    case CollideType.LiquidWater:
                    case CollideType.SwimThrough:
                        bounds.Max.Y -= 2;
                        //Player.Console.Message("{0} should do no fall damage", def.Name);
                        fallDamageMultiplier = 0;
                        break;
                    case CollideType.WalkThrough:
                        //Player.Console.Message("collide for {0} is not solid", def.Name);
                        collides = false;
                        break;
                    default:
                    	break;
                }
            }
            else if (serverBlockID >= Block.Extended) {
                bounds = new AABB(0, 0, 0, 32, 32, 32);
            }
            else {
                BlockID core = Block.Convert(serverBlockID);
                bounds = new AABB(0, 0, 0, 32, DefaultSet.Height(core) * 2, 32);
            }
            NasBlock nb = NasBlock.Get(ConvertToClientBlockID(serverBlockID, lvl));
            nb.collides = collides;
            nb.bounds = bounds;
            if (nb.fallDamageMultiplier == -1) {
                nb.fallDamageMultiplier = fallDamageMultiplier;
            }
            //else {
            //    Player.Console.Message("already set fallDamageMultiplier for ID {0}", nb.selfID);
            //}
            return nb;
        }
        
        public static BlockID ConvertToClientBlockID(BlockID serverBlockID, Level lvl) {
            BlockID clientBlockID;
            if (serverBlockID >= Block.Extended) {
                clientBlockID = Block.ToRaw(serverBlockID);
            } else {
                clientBlockID = Block.Convert(serverBlockID);
                if (clientBlockID >= Block.CpeCount) clientBlockID = Block.Orange;
            }
            return clientBlockID;
        }
        
        
        
        
        
        
        
        public static bool TouchesGround(Level lvl, AABB entityAABB, Position entityPos, out float fallDamageMultiplier) {
            fallDamageMultiplier = 1;
            entityPos.X += entityAABB.Max.X;
            entityPos.Z += entityAABB.Max.Z;
            if (_TouchesGround(lvl, entityAABB, entityPos, out fallDamageMultiplier)) { return true; }
            entityPos.X += entityAABB.Min.X*2;
            if (_TouchesGround(lvl, entityAABB, entityPos, out fallDamageMultiplier)) { return true; }
            entityPos.Z += entityAABB.Min.Z*2;
            if (_TouchesGround(lvl, entityAABB, entityPos, out fallDamageMultiplier)) { return true; }
            entityPos.X += entityAABB.Max.X*2;
            if (_TouchesGround(lvl, entityAABB, entityPos, out fallDamageMultiplier)) { return true; }
            return false;
        }
        public static bool _TouchesGround(Level lvl, AABB entityAABB, Position entityPos, out float fallDamageMultiplier) {
            fallDamageMultiplier = 1;
            int x = entityPos.FeetBlockCoords.X;
            int y = entityPos.FeetBlockCoords.Y;
            int z = entityPos.FeetBlockCoords.Z;
            BlockID serverBlockID = lvl.GetBlock((ushort)x,
                                                 (ushort)y,
                                                 (ushort)z);
            if (serverBlockID == Block.Air) { return false; }
            NasBlock nasBlock = nasBlocksIndexedByServerBlockID[serverBlockID];
            if (!nasBlock.collides) { return false; }
            fallDamageMultiplier = nasBlock.fallDamageMultiplier;
            
            entityAABB = entityAABB.OffsetPosition(entityPos);
            AABB blockAABB = nasBlock.bounds.Offset(x * 32, y * 32, z * 32);
            if (AABB.Intersects(ref entityAABB, ref blockAABB)) {
                Player.Console.Message("nasblock ID is {0} and its fallDamageMultiplier is {1}",
                                       nasBlock.selfID,
                                       nasBlock.fallDamageMultiplier);
                return true;
            }
            return false;
        }
    }
    
}