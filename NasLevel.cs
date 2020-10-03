using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using MCGalaxy;
using MCGalaxy.Events.LevelEvents;

namespace NotAwesomeSurvival {

    public class NasLevel {
        public static void Setup() {
            OnLevelLoadEvent.Register(OnLevelLoad, Priority.High);
            OnLevelUnloadEvent.Register(OnLevelUnload, Priority.Low);
            OnLevelDeletedEvent.Register(OnLevelDeleted, Priority.Low);
            OnLevelRenamedEvent.Register(OnLevelRenamed, Priority.Low);
        }
        public static void TakeDown() {
            OnLevelLoadEvent.Unregister(OnLevelLoad);
            OnLevelUnloadEvent.Unregister(OnLevelUnload);
            OnLevelDeletedEvent.Unregister(OnLevelDeleted);
            OnLevelRenamedEvent.Unregister(OnLevelRenamed);
        }
        const string Path = Nas.Path + "leveldata/";
        const string Extension = ".json";
        [JsonIgnoreAttribute] public static Dictionary<string, NasLevel> all = new Dictionary<string, NasLevel>();
        //public string name;
        public static string GetFileName(string name) {
            return Path + name + Extension;
        }
        public ushort[,] heightmap;

        public static void Unload(string name, NasLevel nl) {
            string jsonString;
            jsonString = JsonConvert.SerializeObject(nl, Formatting.Indented);
            string fileName = GetFileName(name);
            File.WriteAllText(fileName, jsonString);
            Logger.Log(LogType.Debug, "Unloaded NasLevel " + fileName + "!");
            all.Remove(name);
        }


        static void OnLevelLoad(string name) {
            NasLevel nl = new NasLevel();
            string fileName = GetFileName(name);
            if (File.Exists(fileName)) {
                string jsonString = File.ReadAllText(fileName);
                nl = JsonConvert.DeserializeObject<NasLevel>(jsonString);
                all.Add(name, nl);
                Logger.Log(LogType.Debug, "Loaded NasLevel " + fileName + "!");
            }
        }
        static void OnLevelUnload(Level lvl) {
            if (!all.ContainsKey(lvl.name)) { return; }
            Unload(lvl.name, all[lvl.name]);
        }
        static void OnLevelDeleted(string name) {
            string fileName = Path + name + Extension;
            if (File.Exists(fileName)) {
                File.Delete(fileName);
                Logger.Log(LogType.Debug, "Deleted NasLevel " + fileName + "!");
            }
        }
        static void OnLevelRenamed(string srcMap, string dstMap) {
            string fileName = Path + srcMap + Extension;
            if (File.Exists(fileName)) {
                string newFileName = Path + dstMap + Extension;
                File.Move(fileName, newFileName);
                Logger.Log(LogType.Debug, "Renamed NasLevel " + fileName + " to " + newFileName + "!");
                //Unload(srcMap, all[srcMap]);
            }
        }
    }

}
