using HarmonyLib;

namespace AntiphobiaMod.Patches
{
    internal class Shotgun
    {
        [HarmonyPatch(typeof(ShotgunItem), "Start")]
        [HarmonyPostfix]
        public static void OnShotgunItemStart(ShotgunItem __instance)
        {
            Plugin.Logger.LogInfo("--=== Shotgun Summoned ===--");

            if (!Plugin.configHoplophobiaMode.Value)
            {
                return;
            }

            Plugin.Logger.LogInfo("--=== Fixing Shotgun... ===--");
        }
    }
}
