using System.IO;
using AntiphobiaMod.Patches;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace AntiphobiaMod
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private readonly Harmony harmony = new(PluginInfo.PLUGIN_GUID);

        //public static ManualLogSource mls;

        internal static new ManualLogSource Logger;

        //public static ConfigEntry<bool> configFixArachnophobiaMode;
        public static ConfigEntry<bool> configHoplophobiaMode;
        //public static ConfigEntry<bool> configMelissophobiaMode;
        public static ConfigEntry<bool> configTrypophobiaMode;

        AssetBundle antiphobiaAssetBundle;
        public static Material beeHiveMaterial;

        private void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;
            //Logger = BepInEx.Logging.Logger.CreateLogSource(PluginInfo.PLUGIN_GUID);
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            //configFixArachnophobiaMode = Config.Bind("Settings", "Fix Arachnophobia Mode", true, "If true, removes the floating fangs leftover in the game's official Arachnophobia Mode.");
            configHoplophobiaMode = Config.Bind("Settings", "Hoplophobia Mode", true, "If true, implements replacements for turrets and shotguns.");
            //configMelissophobiaMode = Config.Bind("Settings", "Melissophobia Mode", true, "If true, replaces the Circuit Bees with Circuit B's.");
            configTrypophobiaMode = Config.Bind("Settings", "Trypophobia Mode", true, "If true, replaces the texture for the Circuit Bee Hive.");
            
            harmony.PatchAll(typeof(Plugin));
            harmony.PatchAll(typeof(BunkerSpider));
            harmony.PatchAll(typeof(CircuitBees));
            harmony.PatchAll(typeof(ShotgunItem));
            harmony.PatchAll(typeof(Turret));

            LoadAssets();
        }

        private void LoadAssets()
        {
            string path = Path.Combine(Path.GetDirectoryName(this.Info.Location), "antiphobia.assetbundle");

            Logger.LogDebug(path);

            AssetBundle antiphobiaAssetBundle = AssetBundle.LoadFromFile(path);
            if (antiphobiaAssetBundle == null)
            {
                Logger.LogError("Could not find Antiphobia Asset Bundle! File \"antiphobia.assetbundle\" needs to be moved to the same folder as the .dll");
            }

            beeHiveMaterial = antiphobiaAssetBundle.LoadAsset<Material>("Assets/Import/Antiphobia/mat_redlocusthive.mat");

            if (beeHiveMaterial == null)
            {
                Logger.LogError("Beehive material failed to load!");
            }

            Logger.LogInfo("Finished loading assets!");
        }
    }
}
