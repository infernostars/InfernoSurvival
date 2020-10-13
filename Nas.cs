using System.IO;
using Newtonsoft.Json;
using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;
using BlockID = System.UInt16;
using MCGalaxy.Network;

//unknownshadow200: well player ids go from 0 up to 255. normal bots go from 127 down to 64, then 254 down to 127, then finally 63 down to 0.

//UnknownShadow200: FromRaw adds 256 if the block id is >= 66, and ToRaw subtracts 256 if the block id is >= 66
//"raw" is MCGalaxy's name for clientBlockID
///model |0.93023255813953488372093023255814

//gravestone drops upon death that contains your inventory
//different types of crafting stations
//furnace for smelting-style recipes



namespace NotAwesomeSurvival {

    public sealed class Nas : Plugin {
        public override string name { get { return "nas"; } }
        public override string MCGalaxy_Version { get { return "1.9.2.5"; } }
        public override string creator { get { return "goodly"; } }

        const string KeyPrefix = "nas_";
        public const string PlayerKey = KeyPrefix + "NasPlayer";
        public const string Path = "plugins/nas/";
        public const string SavePath = Path + "playerdata/";
        public static string GetSavePath(Player p) {
            return SavePath + p.name + ".json";
        }

        public override void Load(bool startup) {
            NasPlayer.Setup();
            NasBlock.Setup();
            NassEffect.Setup();
            NasBlockChange.Setup();
            ItemProp.Setup();
            Crafting.Setup();
            DynamicColor.Setup();
            Collision.Setup();

            OnPlayerConnectEvent.Register(OnPlayerConnect, Priority.High);
            OnJoinedLevelEvent.Register(OnJoinedLevel, Priority.High);
            OnPlayerClickEvent.Register(OnPlayerClick, Priority.High);
            OnBlockChangingEvent.Register(OnBlockChanging, Priority.High);
            OnBlockChangedEvent.Register(OnBlockChanged, Priority.High);
            OnPlayerMoveEvent.Register(OnPlayerMove, Priority.High);
            OnPlayerDisconnectEvent.Register(OnPlayerDisconnect, Priority.Low);
            OnPlayerCommandEvent.Register(OnPlayerCommand, Priority.High);
            NasGen.Setup();
            NasLevel.Setup();
        }

        public override void Unload(bool shutdown) {
            NasPlayer.TakeDown();
            DynamicColor.TakeDown();
            OnPlayerConnectEvent.Unregister(OnPlayerConnect);
            OnJoinedLevelEvent.Unregister(OnJoinedLevel);
            OnPlayerClickEvent.Unregister(OnPlayerClick);
            OnBlockChangingEvent.Unregister(OnBlockChanging);
            OnBlockChangedEvent.Unregister(OnBlockChanged);
            OnPlayerMoveEvent.Unregister(OnPlayerMove);
            OnPlayerDisconnectEvent.Unregister(OnPlayerDisconnect);
            OnPlayerCommandEvent.Unregister(OnPlayerCommand);
            NasLevel.TakeDown();
        }

        static void OnPlayerConnect(Player p) {
            Player.Console.Message("OnPlayerConnect");
            string path = GetSavePath(p);
            NasPlayer np;
            if (File.Exists(path)) {
                string jsonString = File.ReadAllText(path);
                np = JsonConvert.DeserializeObject<NasPlayer>(jsonString);
                np.SetPlayer(p);
                p.Extras[PlayerKey] = np;
                Logger.Log(LogType.Debug, "Loaded save file " + path + "!");
            } else {
                np = new NasPlayer(p);
                Orientation rot = new Orientation(Server.mainLevel.rotx, Server.mainLevel.roty);
                NasEntity.SetLocation(np, Server.mainLevel.name, Server.mainLevel.SpawnPos, rot);
                p.Extras[PlayerKey] = np;
                Logger.Log(LogType.Debug, "Created new save file for " + p.name + "!");
            }
            np.DisplayHealth();
            np.inventory.ClearHotbar();
            np.inventory.DisplayHeldBlock(NasBlock.Default);

            //Q and E
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar left◙", 16, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar right◙", 18, 0, true));
            //arrow keys
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar up◙", 200, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar down◙", 208, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar left◙", 203, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar right◙", 205, 0, true));

            //WASD (lol)
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar bagopen up◙", 17, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar bagopen down◙", 31, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar bagopen left◙", 30, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar bagopen right◙", 32, 0, true));

            //M and I
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar move◙", 50, 0, true)); //was 50 (M) was 42 (shift)
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar inv◙", 19, 0, true)); //was 23 (i)

            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar delete◙", 45, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar confirmdelete◙", 25, 0, true));



        }
        static void OnPlayerCommand(Player p, string cmd, string message, CommandData data) {
            //if (cmd.CaselessEq("setall")) {
            //    foreach (Command _cmd in Command.allCmds) {
            //        //p.Message("name {0}", _cmd.name);
            //        Command.Find("cmdset").Use(p, _cmd.name + " Operator");
            //
            //    }
            //    p.cancelcommand = true;
            //    return;
            //}


            if (!cmd.CaselessEq("nas")) { return; }
            p.cancelcommand = true;
            NasPlayer np = (NasPlayer)p.Extras[PlayerKey];
            string[] words = message.Split(' ');

            if (words.Length > 1 && words[0] == "hotbar") {
                string hotbarFunc = words[1];
                if (words.Length > 2) {
                    string func2 = words[2];
                    if (hotbarFunc == "bagopen") {
                        if (!np.inventory.bagOpen) { return; }
                        if (func2 == "left") { np.inventory.MoveItemBarSelection(-1); return; }
                        if (func2 == "right") { np.inventory.MoveItemBarSelection(1); return; }
                        if (func2 == "up") { np.inventory.MoveItemBarSelection(-Inventory.itemBarLength); return; }
                        if (func2 == "down") { np.inventory.MoveItemBarSelection(Inventory.itemBarLength); return; }
                    }
                    return;
                }

                if (hotbarFunc == "left") { np.inventory.MoveItemBarSelection(-1); return; }
                if (hotbarFunc == "right") { np.inventory.MoveItemBarSelection(1); return; }

                if (hotbarFunc == "up") { np.inventory.MoveItemBarSelection(-Inventory.itemBarLength); return; }
                if (hotbarFunc == "down") { np.inventory.MoveItemBarSelection(Inventory.itemBarLength); return; }

                if (hotbarFunc == "move") { np.inventory.DoItemMove(); return; }
                if (hotbarFunc == "inv") { np.inventory.ToggleBagOpen(); return; }

                if (hotbarFunc == "delete") { np.inventory.DeleteItem(); return; }
                if (hotbarFunc == "confirmdelete") { np.inventory.DeleteItem(true); return; }


                return;
            }
        }
        static void OnJoinedLevel(Player p, Level prevLevel, Level level, ref bool announce) {
            NasPlayer np = (NasPlayer)p.Extras[PlayerKey];
            //np.OnJoinedLevel(prevLevel, level);
            np.nl = NasLevel.Get(level.name);
        }
        static void OnPlayerDisconnect(Player p, string reason) {
            NasPlayer np = (NasPlayer)p.Extras[PlayerKey];
            NasPlayer.SetLocation(np, p.level.name, p.Pos, p.Rot);
            np.hasBeenSpawned = false;
            string jsonString;
            jsonString = JsonConvert.SerializeObject(np, Formatting.Indented);
            File.WriteAllText(GetSavePath(p), jsonString);
        }

        static void OnPlayerClick
        (Player p,
        MouseButton button, MouseAction action,
        ushort yaw, ushort pitch,
        byte entity, ushort x, ushort y, ushort z,
        TargetBlockFace face) {
            if (p.level.Config.Deletable && p.level.Config.Buildable) { return; }
            
            

            if (button == MouseButton.Left) { NasBlockChange.HandleLeftClick(p, button, action, yaw, pitch, entity, x, y, z, face); }
            
            
            
            NasPlayer np = NasPlayer.GetNasPlayer(p);
            
            
            if (button == MouseButton.Middle && action == MouseAction.Pressed) {
                //NasPlayer np = (NasPlayer)p.Extras[PlayerKey];
                //np.ChangeHealth(0.5f);
                int dist;
                Player.Console.Message("Found {0} holes at distance {1}", NasBlock.HolesInRange(np.nl, x, y+1, z, 4, NasBlock.waterSet, out dist).Count, dist);
            }
            if (button == MouseButton.Right && action == MouseAction.Pressed) {
                //NasPlayer np = (NasPlayer)p.Extras[PlayerKey];
                //np.TakeDamage(0.5f);
            }
            
            
            
            
            if (!np.justBrokeOrPlaced) {
                np.HandleInteraction(button, action, x, y, z, entity, face);
            }
            
            if (action == MouseAction.Released) {
                np.justBrokeOrPlaced = false;
            }

        }

        static void OnBlockChanging(Player p, ushort x, ushort y, ushort z, BlockID block, bool placing, ref bool cancel) {
            NasBlockChange.PlaceBlock(p, x, y, z, block, placing, ref cancel);
        }
        static void OnBlockChanged(Player p, ushort x, ushort y, ushort z, ChangeResult result) {
            NasBlockChange.OnBlockChanged(p, x, y, z, result);
        }
        static void OnPlayerMove(Player p, Position next, byte yaw, byte pitch) {
            NasPlayer np = (NasPlayer)p.Extras[PlayerKey];
            np.DoMovement(next, yaw, pitch);
        }

    }

}
