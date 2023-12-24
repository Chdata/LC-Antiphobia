using HarmonyLib;
using UnityEngine;

namespace AntiphobiaMod.Patches
{
    internal class ShotgunPatch
    {
        [HarmonyPatch(typeof(ShotgunItem), "Start")]
        [HarmonyPostfix]
        public static void OnShotgunItemStart(ShotgunItem __instance)
        {
            Plugin.Logger.LogInfo("--=== Shotgun Summoned ===--");

            if (!Plugin.configHoplophobiaShotgunMode.Value)
            {
                return;
            }

            Plugin.Logger.LogInfo("--=== Fixing Shotgun... ===--");

            Transform gunMuzzle = __instance.gameObject.transform.Find("GunBarrel");
            Transform gunBarrel = __instance.gameObject.transform.Find("GunHandleLOD1");

            gunMuzzle.GetComponent<MeshRenderer>().enabled = false;
            gunMuzzle.Find("GunBarrelLOD1").GetComponent<MeshRenderer>().enabled = false;
            gunBarrel.GetComponent<MeshRenderer>().enabled = false;

            // Make the shotgun trails invisible, but still active (for collision)
            ParticleSystem gunParticles = gunMuzzle.Find("GunShootRayPoint").Find("BulletParticle").GetComponent<ParticleSystem>();
            var trailRenderer = gunParticles.trails;
            trailRenderer.enabled = false;

            // Hide the muzzle flash without deleting it (I don't know a better way)
            ParticleSystem flareParticles = gunMuzzle.Find("GunShootRayPoint").Find("BulletParticle").Find("BulletParticleFlare").GetComponent<ParticleSystem>();
            var flareRenderer = flareParticles.sizeOverLifetime;

            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0.0f, 0.0f);
            flareRenderer.size = new ParticleSystem.MinMaxCurve(0.0f, curve);

            CreateTrumpetHornAndParentTo(gunMuzzle);
            CreateTrumpetBarrelAndParentTo(gunBarrel);
        }

        private static void CreateTrumpetHornAndParentTo(Transform parentModel)
        {
            GameObject childObject = Object.Instantiate<GameObject>(Plugin.shotgunTrumpetHorn);
            childObject.transform.SetParent(parentModel);
            childObject.transform.localPosition = Vector3.zero;
            childObject.transform.localRotation = Quaternion.identity;
        }

        private static void CreateTrumpetBarrelAndParentTo(Transform parentModel)
        {
            GameObject childObject = Object.Instantiate<GameObject>(Plugin.shotgunTrumpetBarrel);
            childObject.transform.SetParent(parentModel);
            childObject.transform.localPosition = Vector3.zero;
            childObject.transform.localRotation = Quaternion.identity;
        }
    }
}
