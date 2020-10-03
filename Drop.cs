using System.Collections.Generic;
using BlockID = System.UInt16;

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
