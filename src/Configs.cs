using BepInEx.Configuration;
using LLBML.Utils;

namespace NoStageEffects;

internal static class Configs
{
    private static ConfigFile config;
    
    internal static ConfigEntry<bool> DoShockwaveEffect { get; private set; }
    internal static ConfigEntry<bool> DoEclipseEffect { get; private set; }
    internal static ConfigEntry<bool> DoHeavenEffect { get; private set; }
    internal static ConfigEntry<bool> DoKOCamera { get; private set; }
    
    internal static ConfigEntry<bool> DoElevatorMove { get; private set; }
    internal static ConfigEntry<bool> DoSubwayMove { get; private set; }
    internal static ConfigEntry<bool> DoStadiumScreen { get; private set; }
    internal static ConfigEntry<bool> DoStreetsDrones { get; private set; }
    internal static ConfigEntry<bool> DoPoolBlimps { get; private set; }
    internal static ConfigEntry<bool> DoFactoryBuckets { get; private set; }
    internal static ConfigEntry<bool> DoSewersSubmarine { get; private set; }
    
    internal static void Init()
    {
        ModDependenciesUtils.RegisterToModMenu(Plugin.Instance.Info, [
            "This mod allows you to disable distracting stage effects and animations",
            "All effects are disabled by default"
        ]);

        config = Plugin.Instance.Config;

        config.Bind("General Effects", "mm_header_general", "General Effects", new ConfigDescription("", null, "modmenu_header"));

        DoShockwaveEffect = config.Bind<bool>("Toggles", "ShockwaveEffect", false, "Camera glitch and impact wave effect when the ball is hit");
        DoEclipseEffect = config.Bind<bool>("Toggles", "EclipseEffect", false, "Stage transition at 250 ball speed");
        DoHeavenEffect = config.Bind<bool>("Toggles", "HeavenEffect", false, "Pure white stage effect at 1,000,000 ball speed");
        DoKOCamera = config.Bind<bool>("Toggles", "KOCamera", false, "Cinematic camera on kill in local games");

        config.Bind("gap", "mm_header_gap", 50, new ConfigDescription("", null, "modmenu_gap"));
        
        config.Bind("Stage Animations", "mm_header_stages", "Stage Animations", new ConfigDescription("", null, "modmenu_header"));
        
        DoElevatorMove = config.Bind<bool>("Toggles", "ElevatorMove", false, "Elevator moves between floors");
        DoSubwayMove = config.Bind<bool>("Toggles", "SubwayMove", false, "Subway moves between stations and makes turns");
        DoStadiumScreen = config.Bind<bool>("Toggles", "StadiumScreen", false, "Stadium screen shows players and ball");
        DoStreetsDrones = config.Bind<bool>("Toggles", "StreetsDrones", false, "Drones appear on Streets");
        DoPoolBlimps = config.Bind<bool>("Toggles", "PoolBlimps", false, "Blimps appear on Pool");
        DoFactoryBuckets = config.Bind<bool>("Toggles", "FactoryBuckets", false, "Iron buckets appear on Factory");
        DoSewersSubmarine = config.Bind<bool>("Toggles", "SewersSubmarine", false, "Submarine appears on Sewers");
    }
}