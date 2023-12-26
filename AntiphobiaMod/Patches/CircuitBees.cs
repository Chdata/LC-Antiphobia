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
                        Plugin.Logger.LogInfo("--=== Changed to Wood! ===--");
                        break;
                }
                case 2:
                {
                    CreatePopcornAndParentTo(__instance.hive.transform);
                    __instance.hive.gameObject.transform.GetComponent<MeshRenderer>().enabled = false;
                    Plugin.Logger.LogInfo("--=== Changed to Popcorn! ===--");
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

        [HarmonyPatch(typeof(GrabbableObject), "EquipItem")]
        [HarmonyPostfix]
        public static void OnBeehiveEquipItem(GrabbableObject __instance)
        {
            HideBeehiveGraphics(__instance);
        }

        [HarmonyPatch(typeof(GrabbableObject), "DiscardItem")]
        [HarmonyPostfix]
        public static void OnBeehiveDiscardItem(GrabbableObject __instance)
        {
            HideBeehiveGraphics(__instance);
        }

        private static void HideBeehiveGraphics(GrabbableObject theHive)
        {
            if (theHive.itemProperties.itemName != "Hive")
            {
                return;
            }

            theHive.gameObject.transform.GetComponent<MeshRenderer>().enabled = false;
        }
    }
}
