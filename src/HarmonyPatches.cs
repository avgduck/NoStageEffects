using HarmonyLib;
using LLScreen;
using StageBackground;

namespace NoStageEffects;

public static class HarmonyPatches
{
    public static void PatchAll()
    {
        Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        
        harmony.PatchAll(typeof(ScreenEffects));
        harmony.PatchAll(typeof(StagePatches));
    }

    private static class ScreenEffects
    {
        [HarmonyPatch(typeof(BG), nameof(BG.SetState))]
        [HarmonyPrefix]
        private static bool SetState_Prefix(BGState state)
        {
            if (state == BGState.ECLIPSE || state == BGState.HEAVEN)
            {
                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(GameCamera), nameof(GameCamera.StartWave))]
        [HarmonyPrefix]
        private static bool StartWave_Prefix()
        {
            return false;
        }
        [HarmonyPatch(typeof(GameCamera), nameof(GameCamera.StartGlitchWave))]
        [HarmonyPrefix]
        private static bool StartGlitchWave_Prefix()
        {
            return false;
        }
    }
}