using System;
using System.Threading;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Newtonsoft.Json;

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
using MCGalaxy.DB;

namespace NotAwesomeSurvival {
    
    //Stores information about a drop (from breaking a block or from a mob dying, or in a chest)
    public class Drop {
        public List<BlockStack> blockStacks = null;
        public List<Item> items = null;
        public Drop() {
            
        }
        public Drop(BlockID clientBlockID, int amount = 1) {
            BlockStack bs = new BlockStack(clientBlockID, amount);
            blockStacks = new List<BlockStack>();
            blockStacks.Add(bs);
        }
        public Drop(Item item) {
            items = new List<Item>();
            items.Add(item);
        }

    }
    public class BlockStack {
        public int amount;
        public BlockID ID;
        public BlockStack(BlockID ID, int amount = 1) {
            this.ID = ID; this.amount = amount;
        }
    }
    
} 