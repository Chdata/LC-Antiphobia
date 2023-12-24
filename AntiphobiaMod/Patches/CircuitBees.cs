using HarmonyLib;
using UnityEngine;

namespace AntiphobiaMod.Patches
{
    internal class CircuitBeesPatch
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

            switch (Plugin.configTrypophobiaMode.Value)
            {
                case 1:
                {
                    __instance.hive.gameObject.GetComponent<MeshRenderer>().material = Plugin.beeHiveMaterial;
                    break;
                }
                case 2:
                {
                    CreatePopcornAndParentTo(__instance.hive.transform);
                    __instance.hive.gameObject.GetComponent<MeshRenderer>().enabled = false;
                    break;
                }
            }
        }

        private static void CreatePopcornAndParentTo(Transform parentModel)
        {
            GameObject childObject = Object.Instantiate<GameObject>(Plugin.beeHivePopcorn);
            childObject.transform.SetParent(parentModel);
            childObject.transform.localPosition = Vector3.zero;
            childObject.transform.localRotation = Quaternion.identity;
        }

        // private static void SetupMelissophobiaMode(RedLocustBees theBees)
        // {
        // VisualEffect component -> VisualEffect.visualEffectAsset --> 
        // VFXRenderer -> .SetMaterials
        // }
    }
}
