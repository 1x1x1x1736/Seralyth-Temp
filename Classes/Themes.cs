using SeralythTemp.Menu;
using SeralythTemp.Notifications;
using System;
using System.Collections.Generic;
using UnityEngine;
using Valve.Newtonsoft.Json;
using static SeralythTemp.Settings;

namespace SeralythTemp.Classes
{
    public class Theme
    {
        public string name;
        public ExtGradient background;
        public ExtGradient[] buttons;
        public Color[] text;

        public Theme(string name, Color bgColor, Color btnDisabled, Color btnEnabled, Color textDisabled, Color textEnabled)
        {
            this.name = name;
            background = new ExtGradient { colors = ExtGradient.GetSolidGradient(bgColor) };
            buttons = new ExtGradient[]
            {
                new ExtGradient { colors = ExtGradient.GetSolidGradient(btnDisabled) },
                new ExtGradient { colors = ExtGradient.GetSolidGradient(btnEnabled) }
            };
            text = new Color[] { textDisabled, textEnabled };
        }

        public Theme(string name, ExtGradient bg, ExtGradient[] btns, Color[] txt)
        {
            this.name = name;
            background = bg;
            buttons = btns;
            text = txt;
        }
    }

    public class ThemeChanger
    {
        public static Theme[] themes = new Theme[]
        {
            new Theme("Default", Color.purple, Color.black, Color.red, Color.white, Color.green),
            new Theme("Ocean", new Color(0f, 0.2f, 0.4f), Color.black, new Color(0f, 0.5f, 0.8f), Color.white, new Color(0.2f, 0.8f, 1f)),
            new Theme("Forest", new Color(0f, 0.3f, 0.1f), Color.black, new Color(0f, 0.6f, 0.2f), Color.white, new Color(0.3f, 1f, 0.3f)),
            new Theme("Lava", new Color(0.4f, 0.1f, 0f), Color.black, new Color(0.8f, 0.2f, 0f), Color.white, new Color(1f, 0.5f, 0f)),
            new Theme("Midnight", new Color(0.05f, 0.05f, 0.1f), new Color(0.1f, 0.1f, 0.15f), new Color(0.15f, 0.15f, 0.3f), new Color(0.5f, 0.5f, 0.7f), new Color(0.6f, 0.6f, 1f)),
            new Theme("Cotton Candy", new Color(0.8f, 0.4f, 0.7f), Color.black, new Color(0.9f, 0.5f, 0.8f), Color.white, new Color(1f, 0.7f, 0.9f)),
            new Theme("Halloween", new Color(0.2f, 0.05f, 0f), Color.black, new Color(1f, 0.4f, 0f), Color.white, new Color(1f, 0.6f, 0f)),
            new Theme("Matrix", new Color(0f, 0.05f, 0f), Color.black, new Color(0f, 0.8f, 0f), new Color(0f, 0.5f, 0f), new Color(0f, 1f, 0f)),
            new Theme("Synthwave", new Color(0.1f, 0f, 0.2f), Color.black, new Color(1f, 0f, 0.5f), new Color(0.8f, 0.3f, 1f), new Color(1f, 0f, 0.8f)),
            new Theme("Arctic", new Color(0.85f, 0.92f, 0.98f), new Color(0.7f, 0.8f, 0.9f), new Color(0.5f, 0.7f, 0.95f), new Color(0.2f, 0.3f, 0.5f), new Color(0.1f, 0.2f, 0.6f)),
            new Theme("Sunset", new Color(0.6f, 0.2f, 0.3f), Color.black, new Color(1f, 0.5f, 0.2f), new Color(1f, 0.8f, 0.6f), new Color(1f, 0.3f, 0.1f)),
            new Theme("Cherry Blossom", new Color(0.15f, 0.05f, 0.1f), Color.black, new Color(0.9f, 0.4f, 0.6f), new Color(1f, 0.85f, 0.9f), new Color(1f, 0.6f, 0.8f)),
            new Theme("Toxic", new Color(0.05f, 0.1f, 0f), Color.black, new Color(0.5f, 1f, 0f), new Color(0.3f, 0.6f, 0f), new Color(0.7f, 1f, 0.2f)),
            new Theme("Blood Moon", new Color(0.15f, 0f, 0f), Color.black, new Color(0.7f, 0f, 0f), new Color(0.8f, 0.2f, 0.2f), new Color(1f, 0.1f, 0.1f)),
            new Theme("Royal", new Color(0.1f, 0f, 0.2f), Color.black, new Color(0.5f, 0f, 0.7f), new Color(0.9f, 0.8f, 0.5f), new Color(1f, 0.85f, 0f)),
            new Theme("Cyberpunk", new Color(0.05f, 0.05f, 0.1f), Color.black, new Color(0f, 1f, 1f), new Color(1f, 0f, 1f), new Color(0f, 1f, 1f)),
            new Theme("Peach", new Color(0.9f, 0.6f, 0.5f), Color.black, new Color(1f, 0.7f, 0.5f), Color.white, new Color(1f, 0.85f, 0.7f)),
            new Theme("Void", new Color(0.02f, 0.02f, 0.04f), Color.black, new Color(0.1f, 0.1f, 0.15f), new Color(0.3f, 0.3f, 0.4f), new Color(0.5f, 0.5f, 0.6f)),
            new Theme("Crimson", new Color(0.3f, 0f, 0.05f), Color.black, new Color(0.9f, 0.1f, 0.1f), new Color(1f, 0.9f, 0.9f), new Color(1f, 0.3f, 0.3f)),
            new Theme("Aurora", new Color(0f, 0.1f, 0.15f), Color.black, new Color(0f, 0.7f, 0.5f), new Color(0.4f, 0.9f, 0.7f), new Color(0.2f, 1f, 0.6f)),
            new Theme("Strawberry", new Color(0.5f, 0.1f, 0.15f), Color.black, new Color(0.9f, 0.2f, 0.3f), new Color(1f, 0.9f, 0.9f), new Color(1f, 0.5f, 0.6f)),
            new Theme("Stealth", new Color(0.08f, 0.08f, 0.08f), new Color(0.12f, 0.12f, 0.12f), new Color(0.2f, 0.2f, 0.2f), new Color(0.5f, 0.5f, 0.5f), new Color(0.8f, 0.8f, 0.8f)),
            new Theme("Candy Corn", new Color(0.1f, 0.05f, 0f), Color.black, new Color(1f, 0.7f, 0f), new Color(1f, 1f, 0.8f), new Color(1f, 0.6f, 0f)),
            new Theme("Ice Cream", new Color(0.95f, 0.85f, 0.9f), Color.black, new Color(0.8f, 0.5f, 0.7f), new Color(0.4f, 0.2f, 0.3f), new Color(0.7f, 0.3f, 0.5f)),
            new Theme("Galaxy", new Color(0.05f, 0f, 0.15f), Color.black, new Color(0.3f, 0.1f, 0.6f), new Color(0.6f, 0.4f, 0.9f), new Color(0.5f, 0.2f, 1f)),
            new Theme("Lemonade", new Color(0.9f, 0.85f, 0.2f), Color.black, new Color(1f, 0.95f, 0f), new Color(0.4f, 0.35f, 0f), new Color(0.7f, 0.65f, 0f)),
            new Theme("Volcano", new Color(0.2f, 0.05f, 0f), Color.black, new Color(1f, 0.3f, 0f), new Color(1f, 0.8f, 0.2f), new Color(1f, 0.5f, 0f)),
            new Theme("Coral Reef", new Color(0f, 0.15f, 0.2f), Color.black, new Color(0f, 0.6f, 0.7f), new Color(0.9f, 0.5f, 0.4f), new Color(0f, 0.8f, 0.9f)),
            new Theme("Sakura", new Color(0.2f, 0.05f, 0.1f), Color.black, new Color(0.95f, 0.5f, 0.65f), new Color(1f, 0.8f, 0.85f), new Color(1f, 0.6f, 0.75f)),
            new Theme("Nebula", new Color(0.08f, 0f, 0.12f), Color.black, new Color(0.4f, 0f, 0.8f), new Color(0.2f, 0.5f, 1f), new Color(0.6f, 0f, 1f)),
            new Theme("Mossy Stone", new Color(0.2f, 0.22f, 0.18f), Color.black, new Color(0.4f, 0.45f, 0.3f), new Color(0.6f, 0.65f, 0.5f), new Color(0.3f, 0.4f, 0.2f)),
            new Theme("Ember", new Color(0.15f, 0.03f, 0f), Color.black, new Color(0.9f, 0.2f, 0f), new Color(1f, 0.6f, 0.1f), new Color(1f, 0.35f, 0f)),
            new Theme("Frost", new Color(0.7f, 0.85f, 0.95f), new Color(0.5f, 0.7f, 0.85f), new Color(0.3f, 0.6f, 0.85f), new Color(0.1f, 0.15f, 0.3f), new Color(0f, 0.3f, 0.6f)),
            new Theme("Camo", new Color(0.15f, 0.18f, 0.1f), Color.black, new Color(0.3f, 0.35f, 0.2f), new Color(0.5f, 0.55f, 0.4f), new Color(0.25f, 0.3f, 0.15f)),
        };

        public static int currentThemeIndex = 0;

        public static void ApplyTheme(int index)
        {
            if (index < 0 || index >= themes.Length)
                return;

            currentThemeIndex = index;
            Theme theme = themes[index];

            backgroundColor = theme.background;
            buttonColors = theme.buttons;
            textColors = theme.text;

            if (HarmonyPatches.ThemeIndex != null)
                HarmonyPatches.ThemeIndex.Value = index;

            UpdateButtonText();
            Menu.Main.RecreateMenu();
            Preferences.AutoSave();
            NotifiLib.SendNotification("<color=grey>[</color><color=cyan>THEME</color><color=grey>]</color> " + theme.name);
        }

        public static void NextTheme()
        {
            ApplyTheme((currentThemeIndex + 1) % themes.Length);
        }

        public static void PrevTheme()
        {
            ApplyTheme((currentThemeIndex - 1 + themes.Length) % themes.Length);
        }

        public static void LoadSavedTheme()
        {
            if (HarmonyPatches.ThemeIndex != null)
                ApplyTheme(HarmonyPatches.ThemeIndex.Value);
        }

        private static void UpdateButtonText()
        {
            foreach (ButtonInfo[] category in Buttons.buttons)
            {
                for (int i = 0; i < category.Length; i++)
                {
                    if (category[i].buttonText.StartsWith("Theme:"))
                    {
                        string themeName = themes[currentThemeIndex].name;
                        category[i].buttonText = "Theme: " + themeName;
                        category[i].overlapText = "Theme: <color=grey>[</color><color=green>" + themeName + "</color><color=grey>]</color>";
                        return;
                    }
                }
            }
        }

        public static void SaveConfig()
        {
            Preferences.Save();
        }

        public static void LoadConfig()
        {
            Preferences.Load();
        }
    }
}
