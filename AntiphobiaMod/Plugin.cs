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
        public static ConfigEntry<bool> configEpilepsyMode;

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
        public static readonly Dictionary<ulong, ParticleSystem> basscannonParticleDict = [];

        private void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;
            //Logger = BepInEx.Logging.Logger.CreateLogSource(PluginInfo.PLUGIN_GUID);
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded, huzzah!");

            //configFixArachnophobiaMode = Config.Bind("Settings", "Fix Arachnophobia Mode", true, "If true, removes the floating fangs leftover in the game's official Arachnophobia Mode.");
            configHoplophobiaTurretMode = Config.Bind("Settings", "Hoplophobia Mode Turret", true, "If true, replaces Turret with Basscannon");
            configHoplophobiaShotgunMode = Config.Bind("Settings", "Hoplophobia Mode Shotgun", true, "If true, replaces Shotgun with Trumpet.");
            //configMelissophobiaMode = Config.Bind("Settings", "Melissophobia Mode", true, "If true, replaces the Circuit Bees with Circuit B's.");
            configTrypophobiaMode = Config.Bind("Settings", "Trypophobia Mode", 2, "0 = Disabled, 1 = replace texture, 2 = popcorn. Replaces the Circuit Bee Hive.");
            configEpilepsyMode = Config.Bind("Settings", "Epilepsy Mode", false, "If true, stops the fan from spinning in the Bunker Facility Start Room. It also disables the Basscannon particle effects.");

            LoadAssets();

            harmony.PatchAll(typeof(Plugin));
            harmony.PatchAll(typeof(BunkerSpiderPatch));
            harmony.PatchAll(typeof(CircuitBeesPatch));
            harmony.PatchAll(typeof(ShotgunPatch));
            harmony.PatchAll(typeof(TurretPatch));
        }

        private void LoadAssets()
        {
            string path = Path.Combine(Path.GetDirectoryName(this.Info.Location), "antiphobia.assetbundle");
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

                SoundTool.ReplaceAudioClip("ShotgunBlast", soundTrumpetBlast);
                SoundTool.ReplaceAudioClip("ShotgunBlast2", soundTrumpetBlast2);
                SoundTool.ReplaceAudioClip("ShotgunBlastFail", soundTrumpetBlastFail);
            }

            if (configHoplophobiaTurretMode.Value)
            {
                soundCannonBass = antiphobiaAssetBundle.LoadAsset<AudioClip>("Assets/Import/Antiphobia/sfx/CannonBass.wav");
                soundCannonMuffle = antiphobiaAssetBundle.LoadAsset<AudioClip>("Assets/Import/Antiphobia/sfx/CannonMuffle.wav");
                soundCannonWall = antiphobiaAssetBundle.LoadAsset<AudioClip>("Assets/Import/Antiphobia/sfx/CannonWall.wav");

                if (soundCannonWall == null)
                {
                    Logger.LogError("Failed to load Basscannon sounds! File \"antiphobia.assetbundle\" needs to be moved to the same folder as the .dll");
                }

                SoundTool.ReplaceAudioClip("TurretFire", soundCannonBass);
                SoundTool.ReplaceAudioClip("TurretFireDistance", soundCannonMuffle);
                SoundTool.ReplaceAudioClip("TurretWallHits", soundCannonWall);
            }

            Logger.LogInfo("--=== All sounds are loaded! ===--");
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.LoadNewLevel))]
        [HarmonyPostfix]
        static void OnLoadNewLevel() // ref SelectableLevel newLevel
        {
            Logger.LogInfo("Client is host: " + RoundManager.Instance.IsHost);

            turretModeLastFrameDict.Clear();
            turretBerserkTimerDict.Clear();
            turretEnteringBerserkModeDict.Clear();
            basscannonParticleDict.Clear();

            RoundManager.Instance.StartCoroutine(BunkerInterior.PatchStartRoom());
        }
    }
}
