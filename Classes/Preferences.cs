using SeralythTemp.Menu;
using SeralythTemp.Notifications;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Valve.Newtonsoft.Json;

namespace SeralythTemp.Classes
{
    public class Preferences
    {
        private static string FilePath => Path.Combine("SeralythTemp", "SeralythTemp.json");

        public static void Save()
        {
            AutoSave();
            NotifiLib.SendNotification("<color=grey>[</color><color=green>SAVED</color><color=grey>]</color> Preferences saved.");
        }

        public static void AutoSave()
        {
            try
            {
                if (!Directory.Exists("SeralythTemp"))
                    Directory.CreateDirectory("SeralythTemp");

                var data = new Dictionary<string, object>();
                data["themeIndex"] = ThemeChanger.currentThemeIndex;
                data["buttonSoundIndex"] = Main.change16;
                data["flySpeedIndex"] = Mods.Settings.Movement.flySpeedIndex;

                var mods = new Dictionary<string, bool>();
                foreach (ButtonInfo[] category in Buttons.buttons)
                {
                    foreach (ButtonInfo button in category)
                    {
                        mods[button.buttonText] = button.enabled;
                    }
                }
                data["mods"] = mods;

                string json = JsonConvert.SerializeObject(data);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception exc)
            {
                Debug.LogError(string.Format("{0} // Error saving preferences: {1}", PluginInfo.Name, exc));
            }
        }

        public static void Load()
        {
            if (!TryLoad())
                NotifiLib.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> No preferences file found.");
        }

        public static bool TryLoad()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    AutoSave();
                    return true;
                }

                string json = File.ReadAllText(FilePath);
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (data == null)
                    return false;

                if (data.ContainsKey("themeIndex"))
                    ThemeChanger.ApplyTheme(Convert.ToInt32(data["themeIndex"]));

                if (data.ContainsKey("buttonSoundIndex"))
                    Main.SetButtonSound(Convert.ToInt32(data["buttonSoundIndex"]));

                if (data.ContainsKey("flySpeedIndex"))
                    Mods.Settings.Movement.SetFlySpeedFromIndex(Convert.ToInt32(data["flySpeedIndex"]));

                if (data.ContainsKey("mods"))
                {
                    var mods = JsonConvert.DeserializeObject<Dictionary<string, bool>>(data["mods"].ToString());
                    foreach (ButtonInfo[] category in Buttons.buttons)
                    {
                        foreach (ButtonInfo button in category)
                        {
                            if (mods.ContainsKey(button.buttonText))
                                button.enabled = mods[button.buttonText];
                        }
                    }
                }
                return true;
            }
            catch (Exception exc)
            {
                Debug.LogError(string.Format("{0} // Error loading preferences: {1}", PluginInfo.Name, exc));
                return false;
            }
        }
    }
}