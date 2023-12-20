using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace AntiphobiaMod.Patches
{
    internal class BunkerSpider
    {
        [HarmonyPatch(typeof(SandSpiderAI), "Start")]
        [HarmonyPostfix]
        public static void OnBunkerSpiderStart(SandSpiderAI __instance)
        {
            Plugin.Logger.LogInfo("--=== Bunker Spider Summoned ===--");

            // if (!Plugin.configFixArachnophobiaMode.Value)
            // {
            //     return;
            // }

            Plugin.Logger.LogInfo("--=== Fixing Bunker Spider... ===--");

            Transform BunkerSpiderModelObject = __instance.transform.Find("MeshContainer");

            if (BunkerSpiderModelObject == null)
            {
                Plugin.Logger.LogError("--=== NOT FOUND ===--"); // This code should never be reached in-game.
            }
            else
            {
                Plugin.Logger.LogInfo("--=== FOUND ===--");
            }

            // Make the mesh invisible by disabling its mesh renderer.
            BunkerSpiderModelObject.Find("AnimContainer").Find("Armature").Find("Head").Find("RightFang").gameObject.GetComponent<MeshRenderer>().enabled = false;
            BunkerSpiderModelObject.Find("AnimContainer").Find("Armature").Find("Head").Find("LeftFang").gameObject.GetComponent<MeshRenderer>().enabled = false;
        }


        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.LoadNewLevel))]
        [HarmonyPrefix]
        static bool OnLoadNewLevel(ref SelectableLevel newLevel)
        {
            Plugin.Logger.LogInfo("Client is host: " + RoundManager.Instance.IsHost);

            if (!RoundManager.Instance.IsHost) return true;

            if (newLevel.levelID == 3)
            {
                Plugin.Logger.LogInfo("Level is company, skipping");
                return true;
            }

            var nl = newLevel;

            var enemyComponentRarity = new Dictionary<Type, int>();
            enemyComponentRarity.Clear();

            nl.maxEnemyPowerCount = 100;
            enemyComponentRarity.Add(typeof(SandSpiderAI), 99999);
            UpdateRarity(newLevel.Enemies, enemyComponentRarity);

            Plugin.Logger.LogInfo("Increased Max Power Counter and Bunker Spider Spawns");

            newLevel = nl;

            return true;
        }

        private static void UpdateRarity(List<SpawnableEnemyWithRarity> enemies, Dictionary<Type, int> componentRarity)
        {
            if (componentRarity.Count <= 0) return;

            foreach (var unit in enemies)
            {
                foreach (var componentRarityPair in componentRarity)
                {
                    if (unit.enemyType.enemyPrefab.GetComponent(componentRarityPair.Key) == null)
                        continue;
                    unit.rarity = componentRarityPair.Value;
                    componentRarity.Remove(componentRarityPair.Key);
                    break;
                }
            }
        }
    }
}
