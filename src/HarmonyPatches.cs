using HarmonyLib;
using StageBackground;
using UnityEngine;

namespace NoStageEffects;

internal static class HarmonyPatches
{
    internal static void PatchAll()
    {
        Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        
        harmony.PatchAll(typeof(ScreenEffects));
        Plugin.LogGlobal.LogInfo("Screen effects patches applied");
        harmony.PatchAll(typeof(StagePatches));
        Plugin.LogGlobal.LogInfo("Stage animation patches applied");
    }

    private static class ScreenEffects
    {
        private static bool BGIntro = false;
        [HarmonyPatch(typeof(BG), nameof(BG.StartUp))]
        [HarmonyPostfix]
        private static void StartUp_Postfix()
        {
            BGIntro = false;
        }
        [HarmonyPatch(typeof(BG), nameof(BG.SetState))]
        [HarmonyPrefix]
        private static void SetState_Prefix(ref BGState state)
        {
            if (!BGIntro)
            {
                BGIntro = true;
                return;
            }
            
            if (state == BGState.ECLIPSE && !Configs.DoEclipseEffect.Value)
            {
                Plugin.LogGlobal.LogInfo("Blocking attempt to activate BG state ECLIPSE");
                state = BGState.NORMAL;
            }
            else if (state == BGState.HEAVEN && !Configs.DoHeavenEffect.Value)
            {
                Plugin.LogGlobal.LogInfo("Blocking attempt to activate BG state HEAVEN");
                state = BGState.NORMAL;
            }
        }

        [HarmonyPatch(typeof(GameCamera), nameof(GameCamera.StartWave))]
        [HarmonyPrefix]
        private static bool StartWave_Prefix()
        {
            return Configs.DoShockwaveEffect.Value;
        }
        [HarmonyPatch(typeof(GameCamera), nameof(GameCamera.StartGlitchWave))]
        [HarmonyPrefix]
        private static bool StartGlitchWave_Prefix()
        {
            return Configs.DoShockwaveEffect.Value;
        }
        
        [HarmonyPatch(typeof(World), nameof(World.ActivateKOCamMode))]
        [HarmonyPrefix]
        private static bool ActivateKOCamMode_Prefix()
        {
            if (Configs.DoKOCamera.Value) return true;
            
            Plugin.LogGlobal.LogInfo("Blocking attempt to activate KO Camera");
            return false;
        }
        [HarmonyPatch(typeof(World), nameof(World.DeactivateKOCamMode))]
        [HarmonyPrefix]
        private static bool DeactivateKOCamMode_Prefix()
        {
            if (Configs.DoKOCamera.Value) return true;
            
            Plugin.LogGlobal.LogWarning("KO Camera mode deactivated (this shouldn't be possible)");
            return false;
        }
    }
    
    public static class StagePatches
    {
        [HarmonyPatch(typeof(ElevatorScript), nameof(ElevatorScript.SetState))]
        [HarmonyPrefix]
        private static void Elevator_SetState_Prefix(ref ElevatorScript.ElevatorState setState)
        {
            if (Configs.DoElevatorMove.Value) return;
            if (setState == ElevatorScript.ElevatorState.FALLING
                || setState == ElevatorScript.ElevatorState.OVERSHOOT_DOWN
                || setState == ElevatorScript.ElevatorState.WAITING_OPEN_DOORS) return;
            
            Plugin.LogGlobal.LogInfo("Stopping Elevator movement");
            setState = ElevatorScript.ElevatorState.STOPPED;
        }

        [HarmonyPatch(typeof(ElevatorScript), nameof(ElevatorScript.StopFalling))]
        [HarmonyPostfix]
        private static void Elevator_StopFalling_Postfix(ElevatorScript __instance)
        {
            if (Configs.DoElevatorMove.Value) return;

            __instance.currentFloor = 0;
            __instance.targetFloor = 2;
            __instance.worldTf.position = Vector3.up * __instance.floorPos[__instance.targetFloor];
        }

        private static bool SubwayIntro = false;
        [HarmonyPatch(typeof(TrainScript), nameof(TrainScript.Start))]
        [HarmonyPostfix]
        private static void Subway_Start_Postfix(TrainScript __instance)
        {
            if (Configs.DoSubwayMove.Value) return;
            
            SubwayIntro = false;
            __instance.SetState(TrainScript.TrainAnimationType.TRAIN_STATION_BIG_OUT);
            SubwayIntro = true;
            Plugin.LogGlobal.LogInfo("Disabling Subway train movement");
        }
        [HarmonyPatch(typeof(TrainScript), nameof(TrainScript.SetState))]
        [HarmonyPrefix]
        private static void Subway_SetState_Prefix(ref TrainScript.TrainAnimationType state)
        {
            if (!SubwayIntro || Configs.DoSubwayMove.Value) return;
            
            state = TrainScript.TrainAnimationType.TRAIN_STRAIGHT;
        }
        
        [HarmonyPatch(typeof(DroneScript), nameof(DroneScript.StartDroneSequence))]
        [HarmonyPrefix]
        private static bool Streets_StartDroneSequence_Prefix()
        {
            if (!Configs.DoStreetsDrones.Value) Plugin.LogGlobal.LogInfo("Disabling Streets drones");
            return Configs.DoStreetsDrones.Value;
        }
        
        [HarmonyPatch(typeof(BlimpScript), nameof(BlimpScript.Update))]
        [HarmonyPostfix]
        private static void Pool_Awake_Postfix(BlimpScript __instance)
        {
            if (!Configs.DoPoolBlimps.Value) Plugin.LogGlobal.LogInfo("Disabling Pool blimps");
            __instance.gameObject.SetActive(Configs.DoPoolBlimps.Value);
        }
        
        [HarmonyPatch(typeof(AssemblyScript), nameof(AssemblyScript.Start))]
        [HarmonyPostfix]
        private static void Factory_Start_Postfix(AssemblyScript __instance)
        {
            if (Configs.DoFactoryBuckets.Value) return;
            
            Plugin.LogGlobal.LogInfo("Disabling Factory buckets");
            __instance.animbucket1["ironBucketTrack"].enabled = false;
            __instance.animbucket2["ironBucketTrack"].enabled = false;
            __instance.animbucket3["ironBucketTrack"].enabled = false;
            __instance.animbucket4["ironBucketTrack"].enabled = false;
        }
        
        [HarmonyPatch(typeof(Stadium_ScreenCamController), nameof(Stadium_ScreenCamController.Awake))]
        [HarmonyPostfix]
        private static void Stadium_Awake_Postfix(Stadium_ScreenCamController __instance)
        {
            if (Configs.DoStadiumScreen.Value) return;
            
            Plugin.LogGlobal.LogInfo("Disabling Stadium screen");
            __instance.OnEclipse(true);
        }
        [HarmonyPatch(typeof(Stadium_ScreenCamController), nameof(Stadium_ScreenCamController.OnEclipse))]
        [HarmonyPrefix]
        private static void Stadium_OnEclipse_Prefix(ref bool pEclipseActive)
        {
            if (Configs.DoStadiumScreen.Value || pEclipseActive) return;
            
            Plugin.LogGlobal.LogInfo("Disabling Stadium screen");
            pEclipseActive = true;
        }
        
        [HarmonyPatch(typeof(SubmarineScript), nameof(SubmarineScript.StartAnim))]
        [HarmonyPrefix]
        private static bool Sewers_StartAnim_Prefix()
        {
            if (!Configs.DoSewersSubmarine.Value) Plugin.LogGlobal.LogInfo("Disabling Sewers submarine");
            return Configs.DoSewersSubmarine.Value;
        }
    }
}