using System;
using System.Collections.Generic;
using System.IO;
using AntiphobiaMod.Patches;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LCSoundTool;
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
        public static ConfigEntry<bool> configHoplophobiaTurretMode;
        public static ConfigEntry<bool> configHoplophobiaShotgunMode;
        //public static ConfigEntry<bool> configMelissophobiaMode;
        public static ConfigEntry<int> configTrypophobiaMode;

        public static AssetBundle antiphobiaAssetBundle;

        public static GameObject shotgunTrumpetHorn;
        public static GameObject shotgunTrumpetBarrel;

        private static AudioClip soundTrumpetBlast;
        private static AudioClip soundTrumpetBlast2;
        private static AudioClip soundTrumpetBlastFail;

        public static GameObject turretBasscannon;

        private static AudioClip soundCannonBass;
        private static AudioClip soundCannonMuffle;
        private static AudioClip soundCannonWall;

        public static Material beeHiveMaterial;
        public static GameObject beeHivePopcorn;

        public static readonly Dictionary<ulong, TurretMode> turretModeLastFrameDict = [];
        public static readonly Dictionary<ulong, float> turretBerserkTimerDict = [];
        public static readonly Dictionary<ulong, bool> turretEnteringBerserkModeDict = [];

        private void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;
            //Logger = BepInEx.Logging.Logger.CreateLogSource(PluginInfo.PLUGIN_GUID);
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded, huzzah!");

            Logger.LogInfo("Hello 3");

            //configFixArachnophobiaMode = Config.Bind("Settings", "Fix Arachnophobia Mode", true, "If true, removes the floating fangs leftover in the game's official Arachnophobia Mode.");
            configHoplophobiaTurretMode = Config.Bind("Settings", "Hoplophobia Mode Turret", true, "If true, replaces Turret with Basscannon");
            configHoplophobiaShotgunMode = Config.Bind("Settings", "Hoplophobia Mode Shotgun", true, "If true, replaces Shotgun with Trumpet.");
            //configMelissophobiaMode = Config.Bind("Settings", "Melissophobia Mode", true, "If true, replaces the Circuit Bees with Circuit B's.");
            configTrypophobiaMode = Config.Bind("Settings", "Trypophobia Mode", 2, "0 = Disabled, 1 = replace texture, 2 = popcorn. Replaces the Circuit Bee Hive.");

            Logger.LogInfo("Hello 1");

            LoadAssets();

            Logger.LogInfo("Hello 4");

            //harmony.PatchAll(); // I tried this and it just causes none of my patches to apply

            harmony.PatchAll(typeof(Plugin));
            Logger.LogInfo("Olleh 5");

            harmony.PatchAll(typeof(BunkerSpiderPatch));
            Logger.LogInfo("Olleh 4");

            harmony.PatchAll(typeof(CircuitBeesPatch));
            Logger.LogInfo("Olleh 3");

            harmony.PatchAll(typeof(ShotgunPatch));
            Logger.LogInfo("Olleh 2");

            // "Olleh 1" is never getting printed to my console...
            harmony.PatchAll(typeof(TurretPatch));
            Logger.LogInfo("Olleh 1");

            Logger.LogInfo("Hello 5");
        }

        private void LoadAssets()
        {
            string path = Path.Combine(Path.GetDirectoryName(this.Info.Location), "antiphobia.assetbundle");

            Logger.LogInfo("Hello 2");
            Logger.LogInfo(path);

            antiphobiaAssetBundle = AssetBundle.LoadFromFile(path);
            if (antiphobiaAssetBundle == null)
            {
                Logger.LogError("Could not find Antiphobia Asset Bundle! File \"antiphobia.assetbundle\" needs to be moved to the same folder as the .dll");
            }

            beeHiveMaterial = antiphobiaAssetBundle.LoadAsset<Material>("Assets/Import/Antiphobia/mat_redlocusthive.mat");

            if (beeHiveMaterial == null)
            {
                Logger.LogError("Beehive material failed to load!");
            }

            beeHivePopcorn = antiphobiaAssetBundle.LoadAsset<GameObject>("Assets/Import/Antiphobia/popcorn.prefab");

            if (beeHivePopcorn == null)
            {
                Logger.LogError("Beehive popcorn failed to load!");
            }

            turretBasscannon = antiphobiaAssetBundle.LoadAsset<GameObject>("Assets/Import/Antiphobia/Basscannon.prefab");

            if (turretBasscannon == null)
            {
                Logger.LogError("Basscannon failed to load!");
            }

            shotgunTrumpetHorn = antiphobiaAssetBundle.LoadAsset<GameObject>("Assets/Import/Antiphobia/TrumpetHorn.prefab");

            if (shotgunTrumpetHorn == null)
            {
                Logger.LogError("Trumpet Horn failed to load!");
            }

            shotgunTrumpetBarrel = antiphobiaAssetBundle.LoadAsset<GameObject>("Assets/Import/Antiphobia/TrumpetBarrel.prefab");

            if (shotgunTrumpetBarrel == null)
            {
                Logger.LogError("Trumpet Barrel failed to load!");
            }

            Logger.LogInfo("Finished loading assets!");
        }
        private void Start()
        {
            Logger.LogInfo("--=== START ===--");
            Logger.LogInfo("--=== START ===--");
            Logger.LogInfo("--=== START ===--"); // These logs do get called... but the logs at the end don't get called.
            Logger.LogInfo("--=== START ===--");
            Logger.LogInfo("--=== START ===--");
            Logger.LogInfo("--=== START ===--");

            Logger.LogInfo("--=== Break 0 ===--");

            if (antiphobiaAssetBundle == null)
            {
                Logger.LogError("Could not find Antiphobia Asset Bundle! File \"antiphobia.assetbundle\" needs to be moved to the same folder as the .dll");
            }

            if (configHoplophobiaShotgunMode.Value)
            {
                soundTrumpetBlast = antiphobiaAssetBundle.LoadAsset<AudioClip>("Assets/Import/Antiphobia/sfx/TrumpetBlast.wav");
                soundTrumpetBlast2 = antiphobiaAssetBundle.LoadAsset<AudioClip>("Assets/Import/Antiphobia/sfx/TrumpetBlast2.wav");
                soundTrumpetBlastFail = antiphobiaAssetBundle.LoadAsset<AudioClip>("Assets/Import/Antiphobia/sfx/TrumpetBlastFail.wav");

                if (soundTrumpetBlastFail == null)
                {
                    Logger.LogError("Failed to load Trumpet sounds! File \"antiphobia.assetbundle\" needs to be moved to the same folder as the .dll");
                }

                Logger.LogInfo("--=== Break 4 ===--");

                SoundTool.ReplaceAudioClip("ShotgunBlast", soundTrumpetBlast);

                Logger.LogInfo("--=== Break 5 ===--");

                SoundTool.ReplaceAudioClip("ShotgunBlast2", soundTrumpetBlast2);

                Logger.LogInfo("--=== Break 6 ===--");

                SoundTool.ReplaceAudioClip("ShotgunBlastFail", soundTrumpetBlastFail);

                Logger.LogInfo("--=== Break 7 ===--");
            }

            if (configHoplophobiaTurretMode.Value)
            {
                soundCannonBass = antiphobiaAssetBundle.LoadAsset<AudioClip>("Assets/Import/Antiphobia/sfx/CannonBass.wav");

                Logger.LogInfo("--=== Break 8 ===--");

                soundCannonMuffle = antiphobiaAssetBundle.LoadAsset<AudioClip>("Assets/Import/Antiphobia/sfx/CannonMuffle.wav");

                Logger.LogInfo("--=== Break 9 ===--");
                soundCannonWall = antiphobiaAssetBundle.LoadAsset<AudioClip>("Assets/Import/Antiphobia/sfx/CannonWall.wav");

                Logger.LogInfo("--=== Break 10 ===--");

                if (soundCannonWall == null)
                {
                    Logger.LogError("Failed to load Basscannon sounds! File \"antiphobia.assetbundle\" needs to be moved to the same folder as the .dll");
                }

                Logger.LogInfo("--=== Break 11 ===--");

                SoundTool.ReplaceAudioClip("TurretFire", soundCannonBass);

                Logger.LogInfo("--=== Break 12 ===--");

                SoundTool.ReplaceAudioClip("TurretFireDistance", soundCannonMuffle);

                Logger.LogInfo("--=== Break 13 ===--");
                SoundTool.ReplaceAudioClip("TurretWallHits", soundCannonWall);

                Logger.LogInfo("--=== Break 14 ===--");
            }

            Logger.LogInfo("--=== All sounds are loaded! ===--");
            Logger.LogInfo("--=== All sounds are loaded! ===--");
            Logger.LogInfo("--=== All sounds are loaded! ===--");
            Logger.LogInfo("--=== All sounds are loaded! ===--");
            Logger.LogInfo("--=== All sounds are loaded! ===--");
            Logger.LogInfo("--=== All sounds are loaded! ===--");
        }
    }
}
