using BepInEx;
using BepInEx.Logging;

namespace NoStageEffects;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public static Plugin Instance { get; private set; }
    internal static ManualLogSource LogGlobal { get; private set; }

    private void Awake()
    {
        Instance = this;
        LogGlobal = base.Logger;
        LogGlobal.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        
        HarmonyPatches.PatchAll();
    }
}