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
    
    public partial class ItemProp {
        
        public string name;
        public string color;
        public string character;
        
        public List<NasBlock.Material> materialsEffectiveAgainst;
        public int tier;
        public float percentageOfTimeSaved;
        public const int baseHPconst = 200;
        public float baseHP;
        public float damage;
        
        public static Dictionary<string, ItemProp> props = new Dictionary<string, ItemProp>();
        
        public ItemProp(string description, NasBlock.Material effectiveAgainst = NasBlock.Material.None, float percentageOfTimeSaved = 0, int tier = 1) {
            string[] descriptionBits = description.Split('|');
            this.name = descriptionBits[0];
            this.color = descriptionBits[1];
            this.character = descriptionBits[2];
            
            if (effectiveAgainst != NasBlock.Material.None) {
                this.materialsEffectiveAgainst = new List<NasBlock.Material>();
                this.materialsEffectiveAgainst.Add(effectiveAgainst);
            } else {
                this.materialsEffectiveAgainst = null;
            }
            //tier 0 is fists
            this.tier = tier;
            this.percentageOfTimeSaved = percentageOfTimeSaved;
            this.baseHP = baseHPconst;
            this.damage = 1;
            props.Add(this.name, this);
        }
    }
    
} 