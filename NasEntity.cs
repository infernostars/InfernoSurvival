using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;
using BlockID = System.UInt16;
using MCGalaxy.Network;
using MCGalaxy.Maths;
using MCGalaxy.Tasks;

namespace NotAwesomeSurvival {

    public partial class NasEntity {
        
        public float HP;
        public const float maxHP = 10;
        public int Air;
        public string levelName;
        public Vec3S32 location;
        public Vec3S32 lastGroundedLocation;
        public byte yaw;
        public byte pitch;
        [JsonIgnoreAttribute] public AABB bounds = AABB.Make(new Vec3S32(0, 0, 0), new Vec3S32(16, 14*2, 16));


        public static void SetLocation(NasEntity ne, string levelName, Position pos, Orientation rot) {
            ne.levelName = levelName;
            ne.location.X = pos.X;
            ne.location.Y = pos.Y;
            ne.location.Z = pos.Z;
            ne.yaw = rot.RotY;
            ne.pitch = rot.HeadX;
        }
        public virtual void ChangeHealth(float diff) {
            //TODO threadsafe
            HP += diff;
            if (HP < 0) { HP = 0; }
        }
        public virtual void Die(string source) {
            
        }
    }

}
