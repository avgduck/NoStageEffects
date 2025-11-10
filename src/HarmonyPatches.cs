using HarmonyLib;
using LLScreen;
using StageBackground;

namespace NoDistractions;

public static class HarmonyPatches
{
    public static void PatchAll()
    {
        Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll(typeof(GamePause));
        harmony.PatchAll(typeof(BackgroundAnimations));
        harmony.PatchAll(typeof(ScreenEffects));
    }

    private static class GamePause
    {
        [HarmonyPatch(typeof(OGONAGCFDPK), nameof(OGONAGCFDPK.PFDNCNGCEDB))]
        [HarmonyPostfix]
        private static void SetTimePause_Postfix(bool LHMJELLIJDP)
        {
            Plugin.Instance.IsGamePaused = LHMJELLIJDP;
        }
    }
    
    private static class BackgroundAnimations
    {
        [HarmonyPatch(typeof(BG), nameof(BG.Update))]
        [HarmonyPrefix]
        private static void Update_Prefix()
        {
            BG.timeScale = 0f;
        }
        
        [HarmonyPatch(typeof(ScreenGameHud), nameof(ScreenGameHud.CShowSpeedChange), MethodType.Enumerator)]
        [HarmonyPrefix]
        private static void CShowSpeedChange_Prefix()
        {
            BG.timeScale = Plugin.Instance.GetBGTimeScale();
        }
        
        [HarmonyPatch(typeof(ScreenGameHud), nameof(ScreenGameHud.CShowSpeedChange), MethodType.Enumerator)]
        [HarmonyPostfix]
        private static void CShowSpeedChange_Postfix()
        {
            BG.timeScale = 0f;
        }

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
    }

    private static class ScreenEffects
    {
        [HarmonyPatch(typeof(PostFXCircle), nameof(PostFXCircle.CStartWave), MethodType.Enumerator)]
        [HarmonyPrefix]
        private static void CStartWave_Prefix()
        {
            BG.timeScale = Plugin.Instance.GetBGTimeScale();
        }
        
        [HarmonyPatch(typeof(PostFXCircle), nameof(PostFXCircle.CStartWave), MethodType.Enumerator)]
        [HarmonyPostfix]
        private static void CStartWave_Postfix()
        {
            BG.timeScale = 0f;
        }
    }
}