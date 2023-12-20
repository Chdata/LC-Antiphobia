using BepInEx;

namespace AntiphobiaMod
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Chdata's {PluginInfo.PLUGIN_GUID} mod is loaded!");
        }
    }
}
