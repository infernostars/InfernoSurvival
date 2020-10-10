using System;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;
using MCGalaxy;
using MCGalaxy.Blocks;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Maths;
using BlockID = System.UInt16;
using NasBlockInteraction =
    System.Action<NotAwesomeSurvival.NasPlayer, MCGalaxy.Events.PlayerEvents.MouseButton, MCGalaxy.Events.PlayerEvents.MouseAction,
    NotAwesomeSurvival.NasBlock, ushort, ushort, ushort>;
using NasBlockExistAction =
    System.Action<NotAwesomeSurvival.NasPlayer,
    NotAwesomeSurvival.NasBlock, bool, ushort, ushort, ushort>;

namespace NotAwesomeSurvival {

    public partial class NasBlock {
            
            
            public class Entity {
                public bool CanAccess(Player p) {
                    return lockedBy.Length == 0 || lockedBy == p.name;
                }
                [JsonIgnore] public string FormattedNameOfLocker {
                    get {
                        if (lockedBy.Length == 0) { return "no one"; }
                        Player locker = PlayerInfo.FindExact(lockedBy);
                        return locker == null ? lockedBy : locker.ColoredName;
                    } }
                public string lockedBy = "";
                public Drop contents = null;
            }
            
            public class Container {
                public const int ToolLimit = 5;
                public static readonly object locker = new object();
                public enum Type { Chest, Barrel, Crate }
                public Type type;
                public string name { get { return Type.GetName(typeof(Type), type); } }
                public string description { get {
                        string desc = "";
                        switch (type) {
                            case NasBlock.Container.Type.Chest:
                                desc = name+"s can store "+ToolLimit+" tools each.";
                                break;
                            case NasBlock.Container.Type.Barrel:
                            case NasBlock.Container.Type.Crate:
                                desc = name+"s can store any amount of one type of block.";
                                break;
                            default:
                                throw new Exception("Invalid value for Type");
                        }
                        return desc;
                    } }
                public Container() { }
                public Container(Container parent) {
                    type = parent.type;
                }
            }
            
            static NasBlockExistAction ContainerExistAction() {
                return (np,nasBlock,exists,x,y,z) => {
                    lock (Container.locker) {
                        if (exists) {
                            Entity blockEntity = new Entity();
                            if (np.nl.blockEntities.ContainsKey(x+" "+y+" "+z)) {
                                //np.p.Message("You just overrode a spot that used to contain a chest.");
                                np.nl.blockEntities.Remove(x+" "+y+" "+z);
                            }
                            np.nl.blockEntities.Add(x+" "+y+" "+z, new Entity());
                            //np.p.Message("You placed a {0}!", nasBlock.container.name);
                            np.p.Message(nasBlock.container.description);
                            np.p.Message("To insert, select what you want to store, then left click.");
                            np.p.Message("To extract, right click.");
                            return;
                        }
                        
                        np.nl.blockEntities.Remove(x+" "+y+" "+z);
                        //np.p.Message("You destroyed a {0}!", nasBlock.container.name);
                    }
                };
            }
            
            static NasBlockInteraction ContainerInteraction() {
                return (np,button,action,nasBlock,x,y,z) => {
                    if (action == MouseAction.Pressed) { return; }
                    lock (Container.locker) {
                        if (np.nl.blockEntities.ContainsKey(x+" "+y+" "+z)) {
                            Entity blockEntity = np.nl.blockEntities[x+" "+y+" "+z];
                            
                            if (!blockEntity.CanAccess(np.p)) {
                                np.p.Message("This {0} is locked by {1}%S.", nasBlock.container.name.ToLower(), blockEntity.FormattedNameOfLocker);
                                return;
                            }
                            
                            //np.p.Message("There is a blockEntity here.");
                            if (button == MouseButton.Middle) {
                                CheckContents(np, nasBlock, blockEntity);
                                return;
                            }
                            
                            if (button == MouseButton.Left) {
                                if (np.inventory.HeldItem.name == "Key") {
                                    //it's already unlocked, lock it
                                    if (blockEntity.lockedBy.Length == 0) {
                                        blockEntity.lockedBy = np.p.name;
                                        np.p.Message("You %flock%S the {0}. Only you can access it now.", nasBlock.container.name.ToLower());
                                        return;
                                    }
                                }
                                
                                if (nasBlock.container.type == Container.Type.Chest) {
                                    AddTool(np, blockEntity);
                                    return;
                                }
                                return;
                            }
                            
                            if (button == MouseButton.Right) {
                                if (np.inventory.HeldItem.name == "Key") {
                                    //it's locked, unlock it
                                    if (blockEntity.lockedBy.Length > 0) {
                                        blockEntity.lockedBy = "";
                                        np.p.Message("You %funlock%S the {0}. Anyone can access it now.", nasBlock.container.name.ToLower());
                                        return;
                                    }
                                }
                                
                                if (nasBlock.container.type == Container.Type.Chest) {
                                    RemoveTool(np, blockEntity);
                                    return;
                                }
                                return;
                            }
                            return;
                        }
                        np.p.Message("Because the key is locked inside this chest, you can never open it. Just take it with you.");
                    }
                };
            }
            static void AddTool(NasPlayer np, Entity blockEntity) {
                if (blockEntity.contents != null && blockEntity.contents.items.Count >= Container.ToolLimit) {
                    np.p.Message("There can only be {0} tools at most in a chest.", Container.ToolLimit);
                    return;
                }
                if (np.inventory.items[np.inventory.selectedItemIndex] == null) {
                    np.p.Message("You need to select a tool to insert it.");
                    return;
                }
                if (blockEntity.contents == null) {
                    blockEntity.contents = new Drop(np.inventory.items[np.inventory.selectedItemIndex]);
                } else {
                    blockEntity.contents.items.Add(np.inventory.items[np.inventory.selectedItemIndex]);
                }
                np.p.Message("You put {0}%S in the chest.", np.inventory.items[np.inventory.selectedItemIndex].ColoredName);
                np.inventory.items[np.inventory.selectedItemIndex] = null;
                np.inventory.UpdateItemDisplay();
            }
            static void RemoveTool(NasPlayer np, Entity blockEntity) {
                if (blockEntity.contents == null) {
                    //np.p.Message("There's nothing to extract.");
                    return;
                }
                Drop taken = new Drop(blockEntity.contents.items[blockEntity.contents.items.Count-1]);
                blockEntity.contents.items.RemoveAt(blockEntity.contents.items.Count-1);
                np.inventory.GetDrop(taken, true);
                if (blockEntity.contents.items.Count == 0) {
                    blockEntity.contents = null;
                }
            }
            static void CheckContents(NasPlayer np, NasBlock nb, Entity blockEntity) {
                if (blockEntity.contents == null) {
                    np.p.Message("There's nothing inside.");
                }
                else if (blockEntity.contents.items != null) {
                    np.p.Message("There's {0} tool{1} inside.", blockEntity.contents.items.Count, blockEntity.contents.items.Count == 1 ? "" : "s");
                }
                
                np.p.Message("This {0} is %f{1}%S by you.", nb.container.name.ToLower(), blockEntity.lockedBy.Length > 0 ? "locked" : "not locked");
            }
            
            static NasBlockInteraction CraftingInteraction() {
                return (np,button,action,nasBlock,x,y,z) => {
                    if (action == MouseAction.Pressed) { return; }
                    lock (Crafting.locker) {
                        Crafting.Recipe recipe = Crafting.GetRecipe(np.p, x, y, z, nasBlock.station);
                        if (recipe == null) {
                            nasBlock.station.ShowArea(np, x, y, z, Color.Red, 500, 127);
                            return;
                        }
                        Drop dropClone = new Drop(recipe.drop);
                        
                        if (np.inventory.GetDrop(dropClone, true) != null) {
                            //non null means the player couldn't fit this drop in their inventory
                            return;
                        }
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
