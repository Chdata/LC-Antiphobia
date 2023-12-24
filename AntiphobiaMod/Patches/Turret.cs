using HarmonyLib;
using UnityEngine;

namespace AntiphobiaMod.Patches
{
    internal class TurretPatch
    {
        [HarmonyPatch(typeof(Turret), "Start")]
        [HarmonyPostfix]
        public static void OnTurretStart(Turret __instance)
        {
            Plugin.Logger.LogInfo("--=== Turret Summoned ===--");

            if (!Plugin.configHoplophobiaTurretMode.Value)
            {
                return;
            }

            // var t1 = __instance;
            // var t2 = __instance.gameObject;
            // var t3 = __instance.gameObject.transform;
            // 
            // Plugin.Logger.LogInfo($"--=== Fixing Turret 1 - " +
            //     $"\n1: {t1} " +
            //     $"\n2: {t2} " +
            //     $"\n3: {t3} " +
            //     $"\n4: {__instance.gameObject.transform.Find("MeshContainer")} " +
            //     $"\n5: {__instance.gameObject.transform.parent}" +
            //     $"\n5: {__instance.gameObject.transform.parent.Find("MeshContainer")}" +
            //     $"\n0: The end " +
            //     $"... ===--");

            Plugin.turretModeLastFrameDict.Add(__instance.NetworkObjectId, TurretMode.Detection);
            Plugin.turretBerserkTimerDict.Add(__instance.NetworkObjectId, 0f);
            Plugin.turretEnteringBerserkModeDict.Add(__instance.NetworkObjectId, false);

            Transform turretGunBody = __instance.gameObject.transform.parent.Find("MeshContainer").Find("RotatingRodContainer").Find("Rod").Find("GunBody");

            if (turretGunBody == null)
            {
                Plugin.Logger.LogInfo("--=== Failed Turret... ===--");
            }

            turretGunBody.GetComponent<MeshRenderer>().enabled = false;
            turretGunBody.Find("Magazine").GetComponent<MeshRenderer>().enabled = false;

            // Make the shotgun trails invisible, but still active (for collision)
            ParticleSystem gunParticles = turretGunBody.Find("GunBarrelPos").Find("BulletParticle").GetComponent<ParticleSystem>();

            if (gunParticles == null)
            {
                Plugin.Logger.LogInfo("--=== Failed Turret Particles... ===--");
            }

            var trailRenderer = gunParticles.trails;
            trailRenderer.enabled = false;

            // Hide the muzzle flash without deleting it (I don't know a better way)
            ParticleSystem flareParticles = turretGunBody.Find("GunBarrelPos").Find("BulletParticle").Find("BulletParticleFlare").GetComponent<ParticleSystem>();

            if (flareParticles == null)
            {
                Plugin.Logger.LogInfo("--=== Failed Flared Particles... ===--");
            }
             
            var flareRenderer = flareParticles.sizeOverLifetime;

            AnimationCurve curve = new();
            curve.AddKey(0.0f, 0.0f);
            flareRenderer.size = new ParticleSystem.MinMaxCurve(0.0f, curve);

            CreateBasscannonAndParentTo(__instance, turretGunBody);

            Plugin.Logger.LogInfo("--=== Reformed Turret... ===--");
        }

        private static void CreateBasscannonAndParentTo(Turret __instance, Transform parentModel)
        {
            GameObject childObject = Object.Instantiate<GameObject>(Plugin.turretBasscannon);
            childObject.transform.SetParent(parentModel);
            childObject.transform.localPosition = Vector3.zero;
            childObject.transform.localRotation = Quaternion.Euler(-90f, -90f, 0f);
            childObject.transform.Find("roar").transform.localRotation = Quaternion.Euler(0f, -3.75f, 0f);

            Plugin.basscannonParticleDict.Add(__instance.NetworkObjectId, childObject.transform.Find("roar").GetComponent<ParticleSystem>());

            Plugin.basscannonParticleDict[__instance.NetworkObjectId].transform.SetParent(parentModel.parent, worldPositionStays: true);

            //Plugin.Logger.LogInfo($"--=== Basscannon: {childObject.transform.Find("roar").GetComponent<ParticleSystem>()} - ");
        }

        [HarmonyPatch(typeof(Turret), "Update")]
        [HarmonyPostfix]
        public static void OnTurretUpdate(Turret __instance)
        {
            if (!Plugin.configHoplophobiaTurretMode.Value)
            {
                return;
            }

            if (!__instance.turretActive)
            {
                return;
            }

            switch (__instance.turretMode)
            {
                case TurretMode.Detection:
                    if (Plugin.turretModeLastFrameDict[__instance.NetworkObjectId] != TurretMode.Detection)
                    {
                        Plugin.Logger.LogInfo("--=== Turret Detection ===--");
                        Plugin.turretModeLastFrameDict[__instance.NetworkObjectId] = TurretMode.Detection;
                        //GetCustomParticleSystem(__instance).Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
                        GetCustomParticleSystem(__instance).gameObject.SetActive(false);
                        Plugin.Logger.LogInfo("--=== Turret Detected ===--");
                    }
                    break;
                case TurretMode.Firing:
                    if (Plugin.turretModeLastFrameDict[__instance.NetworkObjectId] != TurretMode.Firing)
                    {
                        Plugin.Logger.LogInfo("--=== Turret Firing ===--");
                        Plugin.turretModeLastFrameDict[__instance.NetworkObjectId] = TurretMode.Firing;

                        Plugin.Logger.LogInfo("--=== Turret Fire Test ===--");

                        GetCustomParticleSystem(__instance).gameObject.SetActive(true);
                        //GetCustomParticleSystem(__instance).Play(withChildren: true);
                        Plugin.Logger.LogInfo("--=== Turret Fired ===--");
                    }
                    break;
                case TurretMode.Berserk:
                    //Plugin.Logger.LogInfo($"--=== Turret Berserk 1 Time: {Plugin.turretBerserkTimerDict[__instance.NetworkObjectId]} ===--");
                    //Plugin.Logger.LogInfo($"--=== Turret Berserk 1 Mode: {Plugin.turretEnteringBerserkModeDict[__instance.NetworkObjectId]} ===--");

                    if (Plugin.turretModeLastFrameDict[__instance.NetworkObjectId] != TurretMode.Berserk)
                    {
                        Plugin.Logger.LogInfo("--=== Turret Berserk ===--");
                        Plugin.turretModeLastFrameDict[__instance.NetworkObjectId] = TurretMode.Berserk;

                        Plugin.turretBerserkTimerDict[__instance.NetworkObjectId] = 1.3f;
                        Plugin.turretEnteringBerserkModeDict[__instance.NetworkObjectId] = true;
                    }
                    if (Plugin.turretEnteringBerserkModeDict[__instance.NetworkObjectId])
                    {
                        Plugin.turretBerserkTimerDict[__instance.NetworkObjectId] -= Time.deltaTime;
                        Plugin.turretBerserkTimerDict[__instance.NetworkObjectId] -= Time.deltaTime;
                        if (Plugin.turretBerserkTimerDict[__instance.NetworkObjectId] <= 0f)
                        {
                            Plugin.turretEnteringBerserkModeDict[__instance.NetworkObjectId] = false;
                            Plugin.turretBerserkTimerDict[__instance.NetworkObjectId] = 9f;
                            //GetCustomParticleSystem(__instance).Play(withChildren: true);
                            GetCustomParticleSystem(__instance).gameObject.SetActive(true);
                        }
                        break;
                    }
                    if (__instance.IsServer)
                    {
                        Plugin.Logger.LogInfo("--=== Turret Server ===--");
                        Plugin.turretBerserkTimerDict[__instance.NetworkObjectId] -= Time.deltaTime;
                    }
                    break;
            }
        }

        public static ParticleSystem GetCustomParticleSystem(Turret theTurret)
        {
            //Plugin.Logger.LogInfo("--=== Turret GetCustomParticleSystem() ===--");

            //Transform turretGunBody = theTurret.gameObject.transform.parent.Find("MeshContainer").Find("RotatingRodContainer").Find("Rod").Find("GunBody");

            //Plugin.Logger.LogInfo($"--=== Shooting Turret - ");
            //Plugin.Logger.LogInfo($"1: {Plugin.basscannonParticleDict[theTurret.NetworkObjectId]} ");
            //Plugin.Logger.LogInfo($"0: The end ");
            //Plugin.Logger.LogInfo($"... ===--");

            return Plugin.basscannonParticleDict[theTurret.NetworkObjectId];

            //return theTurret.gameObject.transform.parent.Find("MeshContainer").Find("RotatingRodContainer").Find("Rod").Find("GunBody").Find("Basscannon").Find("roar").GetComponent<ParticleSystem>();
        }

        //[HarmonyPatch(typeof(Turret), "OnDestroy")]
        //[HarmonyPrefix]
        //public static void OnTurretDestroy(Turret __instance)
        //{
        //    Plugin.Logger.LogInfo("--=== Turret Deleted ===--");
        //
        //    Plugin.turretModeLastFrameDict.Remove(__instance.NetworkObjectId);
        //    Plugin.turretBerserkTimerDict.Remove(__instance.NetworkObjectId);
        //    Plugin.turretEnteringBerserkModeDict.Remove(__instance.NetworkObjectId);
        //}
    }
}
