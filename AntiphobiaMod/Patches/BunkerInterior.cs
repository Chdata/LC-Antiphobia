using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AntiphobiaMod.Patches
{
    internal class BunkerInterior
    {
        public static IEnumerator PatchStartRoom()
        {
            // Wait a bit for the level to be fully spawned
            yield return new WaitForSeconds(5.0f);

            if (!Plugin.configEpilepsyMode.Value)
            {
                yield break;
            }

            Plugin.Logger.LogInfo("Searching for Industrial Fan");

            foreach (GameObject gameObject in UnityEngine.Object.FindObjectsOfType<GameObject>())
            {
                //Plugin.Logger.LogInfo($"Found {gameObject.name}");

                if (!gameObject.name.StartsWith("IndustrialFan"))
                {
                    continue;
                }

                Plugin.Logger.LogInfo("Found Industrial Fan and turned it off!");

                Animator theAnimator = gameObject.GetComponent<Animator>();

                if (theAnimator != null)
                {
                    theAnimator.enabled = false;
                }
            }
        }
    }
}
