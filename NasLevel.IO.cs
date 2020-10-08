using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using MCGalaxy;
using MCGalaxy.Events.LevelEvents;

namespace NotAwesomeSurvival {

    public partial class NasLevel {
        const string Path = Nas.Path + "leveldata/";
        const string Extension = ".json";

        public static void Setup() {
            OnLevelLoadedEvent.Register(OnLevelLoaded, Priority.High);
            OnLevelUnloadEvent.Register(OnLevelUnload, Priority.Low);
            OnLevelDeletedEvent.Register(OnLevelDeleted, Priority.Low);
            OnLevelRenamedEvent.Register(OnLevelRenamed, Priority.Low);
        }
        public static void TakeDown() {
            OnLevelLoadedEvent.Unregister(OnLevelLoaded);
            OnLevelUnloadEvent.Unregister(OnLevelUnload);
            OnLevelDeletedEvent.Unregister(OnLevelDeleted);
            OnLevelRenamedEvent.Unregister(OnLevelRenamed);
        }
        public static string GetFileName(string name) {
            return Path + name + Extension;
        }
        public static void Unload(string name, NasLevel nl) {
            nl.EndTickTask();
            string jsonString;
            jsonString = JsonConvert.SerializeObject(nl, Formatting.Indented);
            string fileName = GetFileName(name);
            File.WriteAllText(fileName, jsonString);
            Logger.Log(LogType.Debug, "Unloaded(saved) NasLevel " + fileName + "!");
            all.Remove(name);
        }
        static void OnLevelLoaded(Level lvl) {
            NasLevel nl = new NasLevel();
            string fileName = GetFileName(lvl.name);
            if (File.Exists(fileName)) {
                string jsonString = File.ReadAllText(fileName);
                nl = JsonConvert.DeserializeObject<NasLevel>(jsonString);
                nl.lvl = lvl;
                nl.BeginTickTask();
                all.Add(lvl.name, nl);
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
