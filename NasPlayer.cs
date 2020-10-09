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

    public partial class NasPlayer : NasEntity {
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

        public Inventory inventory;
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
            Player.Console.Message("setting player in inventory");
            this.p = p;
            inventory.SetPlayer(p);
        }
        public void HandleInteraction(MouseButton button, ushort x, ushort y, ushort z, byte entityID, TargetBlockFace face) {
            if (!(p.RawHeldBlock == 0)) { return; }

            BlockID serverBlockID = p.level.GetBlock(x, y, z);
            BlockID clientBlockID = p.ConvertBlock(serverBlockID);
            NasBlock nasBlock = NasBlock.Get(clientBlockID);
            if (nasBlock.interaction != null) {
                nasBlock.interaction(this, button, nasBlock, false, x, y, z);
            }
        }
        public override void ChangeHealth(float diff) {
            base.ChangeHealth(diff);
            DisplayHealth();
        }
        /// <summary>
        /// returns true if dead
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public bool TakeDamage(float damage, string source = "unknown causes") {
            if (p.invincible) { return false; }
            TimeSpan timeSinceDeath = DateTime.UtcNow.Subtract(lastDeathDate);
            if (timeSinceDeath.TotalMilliseconds < 2000) {
                //p.Message("You cannot take damage after dying until 2 seconds have passed");
                return false;
            }
            if (damage == 0) { return false; }
            ChangeHealth(-damage);
            DisplayHealth("f", "&7[", "&7]");
            if (HP == 0) {
                Die(source);
                return true;
            }
            p.Send(Packet.VelocityControl(0, 0.25f, 0, 0, 0, 0));
            SchedulerTask taskDisplayRed;
            taskDisplayRed = Server.MainScheduler.QueueOnce(FinishTakeDamage, this, TimeSpan.FromMilliseconds(100));
            
            return false;
        }
        static void FinishTakeDamage(SchedulerTask task) {
            NasPlayer np = (NasPlayer)task.State;
            np.DisplayHealth();
        }
        [JsonIgnoreAttribute] DateTime lastDeathDate = DateTime.MinValue;
        public override void Die(string source) {
            lastDeathDate = DateTime.UtcNow;
            hasBeenSpawned = false;
            Orientation rot = new Orientation(Server.mainLevel.rotx, Server.mainLevel.roty);
            NasEntity.SetLocation(this, Server.mainLevel.name, Server.mainLevel.SpawnPos, rot);
            p.HandleDeath(Block.Stone, p.ColoredName+"%c died%S from "+source+"%S.", false, true);
            HP = maxHP;
            //inventory = new Inventory(p);
            //inventory.Setup();
            DisplayHealth();
        }
        [NonSerialized()] public CpeMessageType whereHealthIsDisplayed = CpeMessageType.BottomRight2;
        public void DisplayHealth(string healthColor = "p", string prefix = "&7[", string suffix = "&7]¼") {
            p.SendCpeMessage(whereHealthIsDisplayed, prefix + HealthString(healthColor) + suffix);
        }
        private string HealthString(string healthColor) {
            StringBuilder builder = new StringBuilder("&8", (int)maxHP + 6);
            string final;
            float totalLostHealth = maxHP - HP;

            float lostHealthRemaining = totalLostHealth;
            for (int i = 0; i < totalLostHealth; ++i) {
                if (lostHealthRemaining < 1) {
                    builder.Append("&" + healthColor + "╝"); //broken heart
                } else {
                    builder.Append("♥"); //empty
                }
                lostHealthRemaining--;
            }

            builder.Append("&" + healthColor);
            for (int i = 0; i < (int)HP; ++i) { builder.Append("♥"); }

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
