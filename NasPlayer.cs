﻿using System;
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
    
    public partial class NasPlayer {
        [NonSerialized()] public Player p;
        [NonSerialized()] public NasBlock heldNasBlock = NasBlock.Default;
        [NonSerialized()] public ushort breakX = ushort.MaxValue, breakY = ushort.MaxValue, breakZ = ushort.MaxValue;
        [NonSerialized()] public int breakAttempt = 0;
        [NonSerialized()] public DateTime? lastAirClickDate = null;
        [NonSerialized()] public DateTime lastLeftClickReleaseDate = DateTime.MinValue;
        [JsonIgnoreAttribute] public byte craftingAreaID = 0;
        public void ResetBreaking() {
            breakX = breakY = breakZ = ushort.MaxValue;
            //NassEffect.UndefineEffect(p, NasBlockChange.BreakMeterID);
            if (p.Extras.Contains("nas_taskDisplayMeter")) {
                NasBlockChange.breakScheduler.Cancel((SchedulerTask)p.Extras["nas_taskDisplayMeter"]);
            }
        }
        public static NasPlayer GetNasPlayer(Player p) {
            if (!p.Extras.Contains(Nas.PlayerKey)) { return null; }
            return (NasPlayer)p.Extras[Nas.PlayerKey];
        }
        
        public float HP;
        const float maxHP = 10;
        public int Air;
        public Inventory inventory;
        public string levelName;
        public Vec3S32 location;
        public Vec3S32 lastGroundedLocation;
        public byte yaw;
        public byte pitch;
        public bool hasBeenSpawned;
        
        [JsonIgnoreAttribute] public Color targetFogColor = Color.White;
        [JsonIgnoreAttribute] public Color curFogColor = Color.White;
        [JsonIgnoreAttribute] public float targetRenderDistance = Server.Config.MaxFogDistance;
        [JsonIgnoreAttribute] public float curRenderDistance = Server.Config.MaxFogDistance;
        
        public NasPlayer(Player p) {
            this.p = p;
            HP = 10;
            Air = 10;
            inventory = new Inventory(p);
            hasBeenSpawned = false;
        }
        public void SetPlayer(Player p) {
            Logger.Log("setting player in inventory");
            this.p = p;
            inventory.SetPlayer(p);
        }
        public void HandleInteraction(MouseButton button, ushort x, ushort y, ushort z, byte entityID, TargetBlockFace face) {
            if (!(p.RawHeldBlock == 0)) { return; }
            
		    BlockID serverBlockID = p.level.GetBlock(x, y, z);
		    BlockID clientBlockID = p.ConvertBlock(serverBlockID);
		    NasBlock nasBlock = NasBlock.Get(clientBlockID);
		    if (nasBlock.station != null) {
		        lock (Crafting.locker) {
                    Crafting.Recipe recipe = Crafting.GetRecipe(p, x, y, z, nasBlock.station);
                    if (recipe == null) {
                        nasBlock.station.ShowArea(this, x, y, z, Color.Red, 500, 127);

                    }
                    else {
                        
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
                        inventory.GetDrop(dropClone, true);
                        nasBlock.station.ShowArea(this, x, y, z, Color.LightGreen, 500);
                        bool clearCraftingArea = button == MouseButton.Left;
                        var patternCost = recipe.patternCost;
                        foreach(KeyValuePair<BlockID, int> pair in patternCost) {
                            if (inventory.GetAmount(pair.Key) < pair.Value) {
                                clearCraftingArea = true; break;
                            }
                        }
                        if (clearCraftingArea) {
                            Crafting.ClearCraftingArea(p, x, y, z, nasBlock.station.ori);
                        } else {
                            foreach(KeyValuePair<BlockID, int> pair in patternCost) {
                                inventory.SetAmount(pair.Key, -pair.Value, false);
                            }
                        }
                    }
		        }
		    }
        }
        public static void SetLocation(NasPlayer np, string levelName, Position pos, Orientation rot) {
            np.levelName = levelName;
            np.location.X = pos.X;
            np.location.Y = pos.Y;
            np.location.Z = pos.Z;
            np.yaw = rot.RotY;
            np.pitch = rot.HeadX;
        }
        public void ChangeHealth(float diff) {
            //TODO threadsafe
            HP+= diff;
            DisplayHealth();
        }
        public void TakeDamage(float damage) {
            p.Send(Packet.VelocityControl(0, 0.5f, 0, 0, 0, 0));
            ChangeHealth(-damage);
            DisplayHealth("f", "&7[", "&7]");
			SchedulerTask taskDisplayRed;
			taskDisplayRed = Server.MainScheduler.QueueOnce(FinishTakeDamage, this, TimeSpan.FromMilliseconds(100));
        }
        static void FinishTakeDamage(SchedulerTask task) {
            NasPlayer np = (NasPlayer)task.State;
            np.DisplayHealth();
        }
        [NonSerialized()] public CpeMessageType whereHealthIsDisplayed = CpeMessageType.BottomRight2;
        public void DisplayHealth(string healthColor = "p", string prefix = "&7[", string suffix = "&7] &f") {
            p.SendCpeMessage(whereHealthIsDisplayed, prefix+HealthString(healthColor)+suffix);
        }
        private string HealthString(string healthColor) {
            StringBuilder builder = new StringBuilder("&8", (int)maxHP + 6);
			string final;
			float totalLostHealth = maxHP - HP;
			
			float lostHealthRemaining = totalLostHealth;
			for(int i = 0; i < totalLostHealth; ++i) {
			    if (lostHealthRemaining < 1) {
			        builder.Append("&"+healthColor+"╝"); //broken heart
			    } else {
			        builder.Append("♥"); //empty
			    }
			    lostHealthRemaining--;
			}
			
			builder.Append("&"+healthColor);
			for(int i = 0; i < (int)HP; ++i)
			{ builder.Append("♥"); }

			final = builder.ToString();
			return final;
        }
        
        public void UpdateHeldBlock() {
		    //p.RawHeldBlock is named wrong, it's actually serverBlockID, so we have to convert it to raw
		    BlockID clientBlockID = Block.ToRaw(p.RawHeldBlock);
		    NasBlock nasBlock = NasBlock.Get(clientBlockID);
		    
		    if (nasBlock.parentID != heldNasBlock.parentID) {
		        inventory.DisplayHeldBlock(nasBlock);
		    }

		    heldNasBlock = nasBlock;
        }
    }
    
}