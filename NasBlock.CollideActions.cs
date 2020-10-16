using System;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;
using MCGalaxy;
using MCGalaxy.Blocks;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Maths;
using MCGalaxy.Tasks;
using BlockID = System.UInt16;
using NasBlockCollideAction =
    System.Action<NotAwesomeSurvival.NasPlayer,
    NotAwesomeSurvival.NasBlock, bool, ushort, ushort, ushort>;

namespace NotAwesomeSurvival {

    public partial class NasBlock {
            
            
            static NasBlockCollideAction DefaultCollideAction() {
                return (np,nasBlock,headSurrounded,x,y,z) => {
                    if (headSurrounded) {
                        np.TakeDamage(0.5f, NasEntity.DamageSource.Suffocating);
                    }
                };
            }
            
        
    }

}
