using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using LLBML.Math;
using LLBML.Settings;
using LLBML.States;
using LLHandlers;
using StageBackground;
using UnityEngine;

namespace NoStageEffects;

internal static class HarmonyPatches
{
    internal static void PatchAll()
    {
        Harmony harmony = new Harmony(Plugin.GUID);
        
        harmony.PatchAll(typeof(ScreenEffects));
        Plugin.LogGlobal.LogInfo("Screen effects patches applied");
        harmony.PatchAll(typeof(ForceGameSettings));
        Plugin.LogGlobal.LogInfo("Force game settings patches applied");
        Plugin.Instance.Config.SettingChanged += (sender, args) => ForceGameSettings.UpdateGameSettingsConfig();
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
        private static void SetState_Prefix(ref BGState state, ref bool stageReset)
        {
            if (!BGIntro)
            {
                BGIntro = true;
                return;
            }
            
            if ((state == BGState.ECLIPSE || stageReset) && !Configs.DoEclipseEffect.Value)
            {
                //Plugin.LogGlobal.LogInfo("Blocking attempt to activate BG state ECLIPSE");
                state = BGState.NORMAL;
                stageReset = false;
            }
            else if (state == BGState.HEAVEN && !Configs.DoHeavenEffect.Value)
            {
                //Plugin.LogGlobal.LogInfo("Blocking attempt to activate BG state HEAVEN");
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
            return Configs.DoKOCamera.Value;
        }

        [HarmonyPatch(typeof(OGONAGCFDPK), nameof(OGONAGCFDPK.FHHKKMAOEKF), MethodType.Enumerator)]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> KGameStart_MoveNext_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            /*
             * match
             * if (!this.skipTitleAnimations && !GameSettings.isOnline)
             */
            CodeMatcher cm = new CodeMatcher(instructions, il);
            cm.Start();
            cm.MatchForward(true, [
                new CodeMatch(OpCodes.Ldarg_0), // this
                new CodeMatch(OpCodes.Ldfld), // .skipTitleAnimations
                new CodeMatch(OpCodes.Call), // call skipTitleAnimations property getter
                new CodeMatch(OpCodes.Brtrue), // if true, break
                new CodeMatch(OpCodes.Call), // call GameSettings.isOnline property getter
                new CodeMatch(OpCodes.Brtrue) // if true, break
            ]);
            CodeInstruction brCopy = cm.Instruction;
            cm.Insert(
                Transpilers.EmitDelegate<Func<bool>>(() =>
                {
                    if (!Configs.DoStageIntros.Value) Plugin.LogGlobal.LogInfo("Skipping stage intro cutscene");
                    return !Configs.DoStageIntros.Value;
                }),
                new CodeInstruction(brCopy) // if true, break
            );
            return cm.InstructionEnumeration();
        }
    }

    private static class ForceGameSettings
    {
        [HarmonyPatch(typeof(JOMBNFKIHIC), nameof(JOMBNFKIHIC.LIDLDDLGBJJ))]
        [HarmonyPostfix]
        private static void UpdateToConfigAll_Postfix()
        {
            if (!Configs.AllowScreenShake.Value) JOMBNFKIHIC.KKLCEPHJMAM = false;
            if (!Configs.AllowWhiteFlashes.Value) JOMBNFKIHIC.AAJCGIGIFCD = false;
            if (!Configs.AllowMovingCamera.Value) JOMBNFKIHIC.DAIOEHBEJBC = false;
        }

        internal static void UpdateGameSettingsConfig()
        {
            JOMBNFKIHIC.LIDLDDLGBJJ();
        }
    }
    
    private static class StagePatches
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

        // manual override of animation clip matching due to incongruence between the intended clip and the actual clip first played
        private static bool blimp1Intro = false;
        private static bool blimp2Intro = false;
        [HarmonyPatch(typeof(BlimpScript), nameof(BlimpScript.Awake))]
        [HarmonyPrefix]
        private static void Pool_Awake_Prefix(BlimpScript __instance)
        {
            if (Configs.DoPoolBlimp.Value) return;
            
            if (__instance.gameObject.name == "Blimp_Animated 1") blimp1Intro = true;
            else if (__instance.gameObject.name == "Blimp_Animated 2") blimp2Intro = true;
            else Plugin.LogGlobal.LogInfo("Disabling Pool blimp animations path2 and path5");
        }
        [HarmonyPatch(typeof(BlimpScript), nameof(BlimpScript.Update))]
        [HarmonyPostfix]
        private static void Pool_Update_Postfix(BlimpScript __instance)
        {
            if (Configs.DoPoolBlimp.Value) return;
            if (!__instance.animationComponent.isPlaying) return;

            Transform airship = __instance.transform.Find("root/airship");
            if (airship == null)
            {
                Plugin.LogGlobal.LogWarning($"Could not find child 'root/airship' of pool blimp: {__instance.name}");
                return;
            }
            
            switch (__instance.name) 
            {
                case "Blimp_Animated 1":
                {
                    string clipName = __instance.animationClips[__instance.currentAnimIndex].name;
                    bool isBadClip = (clipName is "path2" or "path5") && !blimp1Intro;
                    
                    airship.gameObject.SetActive(!isBadClip);
                    if (blimp1Intro && __instance.currentAnimIndex != 0) blimp1Intro = false;
                    break;
                }
                case "Blimp_Animated 2":
                {
                    string clipName = __instance.animationClips[__instance.currentAnimIndex].name;
                    bool isBadClip = (clipName is "path2" or "path5") || blimp2Intro;
                    
                    airship.gameObject.SetActive(!isBadClip);
                    if (blimp2Intro && __instance.currentAnimIndex != 0) blimp2Intro = false;
                    break;
                }
                case "Blimp_Animated 3":
                {
                    string clipName = __instance.animationClips[__instance.currentAnimIndex].name;
                    bool isBadClip = clipName is "path2" or "path5";
                    
                    airship.gameObject.SetActive(!isBadClip);
                    break;
                }
            }
        }
        
        [HarmonyPatch(typeof(AssemblyScript), nameof(AssemblyScript.Start))]
        [HarmonyPostfix]
        private static void Factory_Start_Postfix(AssemblyScript __instance)
        {
            if (Configs.DoFactoryBuckets.Value) return;
            
            Plugin.LogGlobal.LogInfo("Disabling Factory bucket animations");
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