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
    NotAwesomeSurvival.NasBlock, ushort, ushort, ushort>;
using NasBlockExistAction =
    System.Action<NotAwesomeSurvival.NasPlayer,
    NotAwesomeSurvival.NasBlock, bool, ushort, ushort, ushort>;

namespace NotAwesomeSurvival {

    public partial class NasBlock {
            
            
            public class Entity {
                public static readonly object locker = new object();
                public ushort x, y, z;
                public Drop contents = null;
            }
            
            public class Container {
                public enum Type { Chest, Barrel, Crate }
                public Type type;
                public string name { get { return Type.GetName(typeof(Type), type); } }
                public Container() { }
                public Container(Container parent) {
                    type = parent.type;
                }
            }
            
            static NasBlockExistAction ContainerExistAction() {
                return (np,nasBlock,exists,x,y,z) => {
                    if (exists) {
                        np.p.Message("You placed a {0}!", nasBlock.container.name);
                        
                        return;
                    }
                    np.p.Message("You destroyed a {0}!", nasBlock.container.name);
                };
            }
            static NasBlockInteraction ContainerInteraction() {
                return (np,button,nasBlock,x,y,z) => {
                    lock (Entity.locker) {
                        np.p.Message("hi");
                    }
                };
            }
            
            static NasBlockInteraction CraftingInteraction() {
                return (np,button,nasBlock,x,y,z) => {
                    lock (Crafting.locker) {
                        Crafting.Recipe recipe = Crafting.GetRecipe(np.p, x, y, z, nasBlock.station);
                        if (recipe == null) {
                            nasBlock.station.ShowArea(np, x, y, z, Color.Red, 500, 127);
                            return;
                        }
                        Drop dropClone = new Drop(recipe.drop);
                        
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
            static NasBlockExistAction CraftingExistAction() {
                return (np,nasBlock,exists,x,y,z) => {
                    if (exists) {
                        np.p.Message("You placed a {0}!", nasBlock.station.name);
                        np.p.Message("Click it with nothing held (press G) to craft.");
                        np.p.Message("Right click to auto-replace recipe.");
                        np.p.Message("Left click for one-and-done.");
                        nasBlock.station.ShowArea(np, x, y, z, Color.White);
                        return;
                    }
                    np.p.Message("You destroyed a {0}!", nasBlock.station.name);
                };
            }
        
    }

}
