using static SeralythTemp.Menu.Main;
using SeralythTemp.Classes;

namespace SeralythTemp.Mods.Settings
{
    public class Movement
    {
        public static int flySpeedIndex = 2;
        public static float flySpeed = 15f;

        public static void ChangeFlySpeed()
        {
            SetFlySpeedFromIndex(flySpeedIndex + 1);
        }

        public static void ChangeFlySpeedBack()
        {
            SetFlySpeedFromIndex(flySpeedIndex - 1);
        }

        public static void SetFlySpeedFromIndex(int index)
        {
            string[] speedNames = new string[] { "Very Slow", "Slow", "Normal", "Fast", "Very Fast", "Extreme" };
            float[] speedValues = new float[] { 5f, 10f, 15f, 20f, 30f, 50f };

            if (index < 0)
                index = speedNames.Length - 1;
            flySpeedIndex = index % speedNames.Length;
            flySpeed = speedValues[flySpeedIndex];

            GetIndex("Change Fly Speed").overlapText = $"Change Fly Speed [{speedNames[flySpeedIndex]}]";
            Preferences.AutoSave();
        }
    }
}
