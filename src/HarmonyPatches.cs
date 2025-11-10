using HarmonyLib;
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
    
    public static class StagePatches
    {
        [HarmonyPatch(typeof(ElevatorScript), nameof(ElevatorScript.SetState))]
        [HarmonyPrefix]
        private static void Elevator_SetState_Prefix(ref ElevatorScript.ElevatorState setState)
        {
            setState = ElevatorScript.ElevatorState.STOPPED;
        }

        private static bool SubwayIntro = false;
        [HarmonyPatch(typeof(TrainScript), nameof(TrainScript.Start))]
        [HarmonyPostfix]
        private static void Subway_Start_Postfix(TrainScript __instance)
        {
            SubwayIntro = false;
            __instance.SetState(TrainScript.TrainAnimationType.TRAIN_STATION_BIG_OUT);
            SubwayIntro = true;
        }
        [HarmonyPatch(typeof(TrainScript), nameof(TrainScript.SetState))]
        [HarmonyPrefix]
        private static void Subway_SetState_Prefix(ref TrainScript.TrainAnimationType state)
        {
            if (!SubwayIntro) return;
            
            state = TrainScript.TrainAnimationType.TRAIN_STRAIGHT;
        }
        
        [HarmonyPatch(typeof(DroneScript), nameof(DroneScript.StartDroneSequence))]
        [HarmonyPrefix]
        private static bool Streets_StartDroneSequence_Prefix()
        {
            return false;
        }
        
        [HarmonyPatch(typeof(BlimpScript), nameof(BlimpScript.Update))]
        [HarmonyPrefix]
        private static bool Pool_Update_Prefix()
        {
            return false;
        }
        
        [HarmonyPatch(typeof(AssemblyScript), nameof(AssemblyScript.Start))]
        [HarmonyPostfix]
        private static void Factory_Start_Postfix(AssemblyScript __instance)
        {
            __instance.animbucket1["ironBucketTrack"].enabled = false;
            __instance.animbucket2["ironBucketTrack"].enabled = false;
            __instance.animbucket3["ironBucketTrack"].enabled = false;
            __instance.animbucket4["ironBucketTrack"].enabled = false;
        }
        
        [HarmonyPatch(typeof(Stadium_ScreenCamController), nameof(Stadium_ScreenCamController.Awake))]
        [HarmonyPostfix]
        private static void Stadium_Awake_Postfix(Stadium_ScreenCamController __instance)
        {
            __instance._cam.enabled = false;
        }
        
        [HarmonyPatch(typeof(SubmarineScript), nameof(SubmarineScript.StartAnim))]
        [HarmonyPrefix]
        private static bool Sewers_StartAnim_Prefix()
        {
            return false;
        }
    }
}