using BepInEx;
using BepInEx.Logging;

namespace NoDistractions;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(PluginDetails.DEPENDENCY_LLBML, BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(PluginDetails.DEPENDENCY_MODMENU, BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
    public static Plugin Instance { get; private set; }
    internal static ManualLogSource LogGlobal { get; private set; }
    internal bool IsGamePaused { get; set; } = false;

    private void Awake()
    {
        Instance = this;
        LogGlobal = base.Logger;
        LogGlobal.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        
        HarmonyPatches.PatchAll();
    }

    private void Start()
    {
        LLBML.Utils.ModDependenciesUtils.RegisterToModMenu(this.Info, PluginDetails.MODMENU_TEXT);
    }

    public float GetBGTimeScale()
    {
        return IsGamePaused ? 0f : 1f;
    }
}