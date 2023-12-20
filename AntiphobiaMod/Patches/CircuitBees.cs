using HarmonyLib;
using UnityEngine;

namespace AntiphobiaMod.Patches
{
    internal class CircuitBees
    {
        [HarmonyPatch(typeof(RedLocustBees), "Start")]
        [HarmonyPostfix]
        public static void OnRedLocustBeesStart(RedLocustBees __instance)
        {
            Plugin.Logger.LogInfo("--=== Circuit Bees Summoned ===--");

            Plugin.Logger.LogInfo("--=== Fixing Circuit Bee Hive... ===--");

            // if (Plugin.configMelissophobiaMode.Value)
            // {
            //     SetupMelissophobiaMode(__instance);
            // }

            if (Plugin.configTrypophobiaMode.Value)
            {
                SetupTrypophobiaMode(__instance);
            }
        }

        private static void SetupTrypophobiaMode(RedLocustBees theBees)
        {
            theBees.hive.gameObject.GetComponent<MeshRenderer>().material = Plugin.beeHiveMaterial;
        }

        // private static void SetupMelissophobiaMode(RedLocustBees theBees)
        // {
        // VisualEffect component -> VisualEffect.visualEffectAsset --> 
        // VFXRenderer -> .SetMaterials
        // }
    }
}
