using BepInEx;
using BepInEx.Configuration;

namespace SeralythTemp
{
    [System.ComponentModel.Description(PluginInfo.Description)]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class HarmonyPatches : BaseUnityPlugin
    {
        public static ConfigEntry<int> ThemeIndex;
        public static ConfigEntry<string> SaveData;

        private void Awake()
        {
            ThemeIndex = Config.Bind("General", "ThemeIndex", 0, "Current theme index");
            SaveData = Config.Bind("General", "SaveData", "", "Saved mod config data");

            GorillaTagger.OnPlayerSpawned(OnPlayerSpawned);
        }

        public void OnPlayerSpawned()
        {
            Classes.Preferences.TryLoad();
            Patches.PatchHandler.PatchAll();
        }
    }
}
