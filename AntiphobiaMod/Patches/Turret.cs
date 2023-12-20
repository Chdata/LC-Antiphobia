using HarmonyLib;

namespace AntiphobiaMod.Patches
{
    internal class Turret
    {
        [HarmonyPatch(typeof(Turret), "Start")]
        [HarmonyPostfix]
        public static void OnTurretStart(Turret __instance)
        {
            Plugin.Logger.LogInfo("--=== Turret Summoned ===--");

            if (!Plugin.configHoplophobiaMode.Value)
            {
                return;
            }

            Plugin.Logger.LogInfo("--=== Fixing Turret... ===--");
        }
    }
}
