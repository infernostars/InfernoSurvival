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
    
    public class DynamicColor {
        static SchedulerTask task;
        public static ColorDesc[] defaultColors     ;
        public static ColorDesc[] fullHealthColors  ;
        public static ColorDesc[] mediumHealthColors;
        public static ColorDesc[] lowHealthColors   ;
        public static ColorDesc[] direHealthColors  ;
        
        public static void Setup() {
            Bitmap colorImage;
            colorImage = new Bitmap(Nas.Path+"selectorColors.png");
            
            defaultColors      = new ColorDesc[colorImage.Width];
            fullHealthColors   = new ColorDesc[colorImage.Width];
            mediumHealthColors = new ColorDesc[colorImage.Width];
            lowHealthColors    = new ColorDesc[colorImage.Width];
            direHealthColors   = new ColorDesc[colorImage.Width];
            
            int index = 0;
            SetupDescs(index++, colorImage, ref defaultColors);
            SetupDescs(index++, colorImage, ref fullHealthColors);
            SetupDescs(index++, colorImage, ref mediumHealthColors);
            SetupDescs(index++, colorImage, ref lowHealthColors);
            SetupDescs(index++, colorImage, ref direHealthColors);
            colorImage.Dispose();
            
            task = Server.MainScheduler.QueueRepeat(Update, null, TimeSpan.FromMilliseconds(100));
        }
        static void SetupDescs(int yOffset, Bitmap colorImage, ref ColorDesc[] colorDescs) {
            for (int i = 0; i < colorImage.Width; i++) {
                Color color = colorImage.GetPixel(i, yOffset);
                colorDescs[i].R = color.R;
                colorDescs[i].G = color.G;
                colorDescs[i].B = color.B;
                colorDescs[i].A = 255;
                colorDescs[i].Code = 'h';
                colorDescs[i].Fallback = 'f';
            }
        }
        public static void TakeDown() {
        	if (task == null) return;
        	Server.MainScheduler.Cancel(task);
        }
        
        static int index;
        static void Update(SchedulerTask task) {
        	index = (index + 1) % defaultColors.Length;
        	Player[] players = PlayerInfo.Online.Items;
            
        	foreach (Player p in players) {
        		if (!p.Supports(CpeExt.TextColors)) continue;
        		NasPlayer np = NasPlayer.GetNasPlayer(p);
        		if (np == null) {
        		    //p.Message("your NP is null");
        		    continue;
        		}
        		ColorDesc desc = np.inventory.selectorColors[index];
        		//p.Message("Sending the color desc {0} {1} {2} {3}", desc.R, desc.G, desc.B, desc.Code);
        		p.Send(Packet.SetTextColor(desc));
        	}
        }
    }
    
    public static class ColorCleanUp {
		public const char colorCode = '&';
		public static string CleanedString(string message) {
			if (message.IndexOf(colorCode) == -1) return message;
			
			char lastColor = colorCode; //default to a character that can never be a color
			int lastColorIndex = -1;
			bool newColorAllowed = true;
			StringBuilder cleaned = new StringBuilder(message.Length);
			bool isStandard;
			// @7went@7 to @2 @e@4  @5 ae_palais_saxonne@e@4__@e
			// test
			for (int i = 0; i < message.Length; i++) {
				if (IsColorCode(message, i, out isStandard)) {
					if (i+2 >= message.Length) { break; }
					if (lastColor == (isStandard ? Char.ToLower(message[i+1]) : message[i+1]) ) {
						//Logger.Log(LogType.Debug, "SKIPPED POINTLESS COLOR CODE");
						i++ ; continue;
					}
					if (!newColorAllowed && lastColorIndex != -1) {
						cleaned.Remove(lastColorIndex, 2);
						//Logger.Log(LogType.Debug, "REMOVED POINTLESS COLOR CODE");
						//Logger.Log(LogType.Debug, "cleaned: \"{0}\"", cleaned.ToString());
					}
					
					lastColor = isStandard ? Char.ToLower(message[i+1]) : message[i+1];
					cleaned.Append(message[i]);
					cleaned.Append(lastColor);
					lastColorIndex = cleaned.Length-2;
					i++;
					newColorAllowed = false;
					//Logger.Log(LogType.Debug, "add color code: \"{0}\"", cleaned.ToString());
					continue;
				}
				if (message[i] != ' ') { newColorAllowed = true; }
				cleaned.Append(message[i]);
				//Logger.Log(LogType.Debug, "cleaned: \"{0}\"", cleaned.ToString());
			}
			//Logger.Log(LogType.Debug, "newColorAllowed is {0}", newColorAllowed);
			if (!newColorAllowed) {
				cleaned.Remove(lastColorIndex, 2);
				//Logger.Log(LogType.Debug, "REMOVED FINAL POINTLESS COLOR CODE");
			}
			return cleaned.ToString();
		}
		
		//assumes the character you're checking is the first character in a color-code pair
		static bool IsColorCode(string message, int i, out bool isStandard) {
			isStandard = false;
			if (message[i] == colorCode) {
				if (i+1 > message.Length-1) { return false; }
				if (Colors.IsStandard(message[i+1])) { isStandard = true; return true; }
				if (Colors.IsDefined(message[i+1])) { return true; }
				return false;
			}
			//the code below checks assuming message[i] isn't an ampersand
			//if (message[i-1] < 0) { return false; }
			//if (message[i-1] != colorCode) { return false; }
			//if (Colors.IsStandard(message[i]) || Colors.IsDefined(message[i])) { return true; }
			return false;
		}
    }
    
} 