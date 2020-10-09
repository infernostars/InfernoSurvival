using System;
using System.Collections.Generic;
using System.Drawing;
using MCGalaxy;
using MCGalaxy.Blocks;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Maths;
using BlockID = System.UInt16;
using NasBlockInteraction =
    System.Action<NotAwesomeSurvival.NasPlayer, MCGalaxy.Events.PlayerEvents.MouseButton,
    NotAwesomeSurvival.NasBlock, bool, ushort, ushort, ushort>;

namespace NotAwesomeSurvival {

    public partial class NasBlock {
        
        static NasBlockInteraction CraftingInteraction() {
            return (np,button,nasBlock,justGotPlaced,x,y,z) => {
                    if (justGotPlaced) {
                        np.p.Message("You placed a {0}!", nasBlock.station.name);
                        np.p.Message("Click it with nothing held (press G) to craft.");
                        np.p.Message("Right click to auto-replace recipe.");
                        np.p.Message("Left click for one-and-done.");
                        nasBlock.station.ShowArea(np, x, y, z, Color.White);
                        return;
                    }
                    lock (Crafting.locker) {
                        Crafting.Recipe recipe = Crafting.GetRecipe(np.p, x, y, z, nasBlock.station);
                        if (recipe == null) {
                            nasBlock.station.ShowArea(np, x, y, z, Color.Red, 500, 127);
                            return;
                        }
                        
                        Drop dropClone = new Drop();
                        if (recipe.drop.blockStacks != null) {
                            dropClone.blockStacks = new List<BlockStack>();
                            foreach (BlockStack bs in recipe.drop.blockStacks) {
                                BlockStack bsClone = new BlockStack(bs.ID, bs.amount);
                                dropClone.blockStacks.Add(bsClone);
                            }
                        }
                        if (recipe.drop.items != null) {
                            dropClone.items = new List<Item>();
                            foreach (Item item in recipe.drop.items) {
                                Item itemClone = new Item(item.name);
                                dropClone.items.Add(itemClone);
                            }
                        }
                        np.inventory.GetDrop(dropClone, true);
                        nasBlock.station.ShowArea(np, x, y, z, Color.LightGreen, 500);
                        bool clearCraftingArea = button == MouseButton.Left;
                        var patternCost = recipe.patternCost;
                        foreach (KeyValuePair<BlockID, int> pair in patternCost) {
                            if (np.inventory.GetAmount(pair.Key) < pair.Value) {
                                clearCraftingArea = true; break;
                            }
                        }
                        if (clearCraftingArea) {
                            Crafting.ClearCraftingArea(np.p, x, y, z, nasBlock.station.ori);
                        } else {
                            foreach (KeyValuePair<BlockID, int> pair in patternCost) {
                                np.inventory.SetAmount(pair.Key, -pair.Value, false);
                            }
                        }
                    }
                
            };
        }
        
    }

}
