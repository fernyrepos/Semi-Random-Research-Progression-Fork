using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Sound;

namespace CM_Semi_Random_Research
{
    [StaticConstructorOnStartup]
    public static class MainTabWindow_Research_Patches
    {


        private static readonly Texture2D NextResearchButtonIcon = ContentFinder<Texture2D>.Get("UI/Buttons/MainButtons/CM_Semi_Random_Research_Random");

        [HarmonyPatch(typeof(MainTabWindow_Research))]
        [HarmonyPatch("DrawLeftRect", MethodType.Normal)]
        public static class MainTabWindow_Research_DrawLeftRect
        {

            [HarmonyPostfix]
            public static void Postfix(ResearchProjectDef __instance, Rect leftOutRect)
            {
                float buttonSize = 32.0f;
                Rect buttonRect = new Rect(leftOutRect.xMax - buttonSize, leftOutRect.yMin, buttonSize, buttonSize);

                // I'm just going to check both buttons in case either snatches up the event
                bool pressedButton1 = Widgets.ButtonTextSubtle(buttonRect, "");
                bool pressedButton2 = Widgets.ButtonImage(buttonRect, NextResearchButtonIcon);

                if (pressedButton1 || pressedButton2)
                {
                    SoundDefOf.ResearchStart.PlayOneShotOnCamera();

                    MainTabWindow currentWindow = Find.WindowStack.WindowOfType<MainTabWindow>();
                    MainTabWindow newWindow = SemiRandomResearchDefOf.CM_Semi_Random_Research_MainButton_Next_Research.TabWindow;

                    //Log.Message(string.Format("Has currentWindow {0}, has newWindow {1}", (currentWindow != null).ToString(), (newWindow != null).ToString()));
                    
                    if (currentWindow != null && newWindow != null)
                    {
                        Find.WindowStack.TryRemove(currentWindow, false);
                        Find.WindowStack.Add(newWindow);
                        SoundDefOf.TabOpen.PlayOneShotOnCamera();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(MainTabWindow_Research))]
        [HarmonyPatch("DrawStartButton", MethodType.Normal)]
        public static class MainTabWindow_Research_DrawStartButton
        {
            
            [HarmonyPrefix]
            public static void Prefix(List<string> ___lockedReasons, ResearchTabDef ___curTabInt)
            {
                ___lockedReasons.Clear();
                if(SemiRandomResearchMod.settings.featureEnabled)
                {
                    ___lockedReasons.Add("Semi Random Research is active.");
                }
                SemiRandomResearchUtility.is_anomaly_tab = ___curTabInt == ResearchTabDefOf.Anomaly;
            }

            //Ugly alternative prefix that avoids the need for the Transpiler below. Used to patch mods that yoinked the MainTabWindow_Research class.
            public static bool PrefixSkip(List<string> ___lockedReasons, ResearchTabDef ___curTabInt, ResearchProjectDef ___selectedProject)
            {
                Prefix(___lockedReasons, ___curTabInt);
                return ___selectedProject == null || !SemiRandomResearchMod.settings.featureEnabled || ! ___selectedProject.CanStartNow;
            }

            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                // if (selectedProject.CanStartNow && !Find.ResearchManager.IsCurrentProject(selectedProject))
                //IL_0000: ldarg.0
                //IL_0001: ldfld class Verse.ResearchProjectDef RimWorld.MainTabWindow_Research::selectedProject
                //IL_0006: callvirt instance bool Verse.ResearchProjectDef::get_CanStartNow() <- This needs replacing
                //IL_000b: brfalse.s IL_004f

                FieldInfo selectedProjectFieldInfo = typeof(RimWorld.MainTabWindow_Research).GetField("selectedProject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                MethodInfo canStartNowMethodInfo = AccessTools.Method(typeof(Verse.ResearchProjectDef), "get_CanStartNow");
                MethodInfo replacementCanStartCheck = AccessTools.Method(typeof(SemiRandomResearchUtility), nameof(SemiRandomResearchUtility.CanSelectNormalResearchNow));
                MethodInfo isCurrentProjectMethodInfo = AccessTools.Method(typeof(ResearchManager), "IsCurrentProject");
                MethodInfo replacementIsCurrentProject = AccessTools.Method(typeof(SemiRandomResearchUtility), nameof(SemiRandomResearchUtility.IsCurrentProject));
                
                MethodInfo clearListMethodInfo = AccessTools.Method(new List<string>().GetType(), "Clear");
                FieldInfo lockedReasonsFieldInfo = typeof(RimWorld.MainTabWindow_Research).GetField("lockedReasons", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);


                List<CodeInstruction> instructionList = instructions.ToList();

                for (int i = 2; i < instructionList.Count; ++i)
                {
                    // Verify everything we are replacing to make sure this hasn't already been tampered with
                    if (instructionList[i - 2].IsLdarg() &&
                        instructionList[i - 1].LoadsField(selectedProjectFieldInfo) &&
                        instructionList[i - 0].Calls(canStartNowMethodInfo))
                    {
                        Log.Message("[CM_Semi_Random_Research] - patching to conditionally hide normal start research button.");
                        instructionList[i - 0] = new CodeInstruction(OpCodes.Call, replacementCanStartCheck);
                    }
                    
                    if(i>5)
                    {
                        if (instructionList[i - 5].IsLdarg() &&
                            instructionList[i - 4].LoadsField(selectedProjectFieldInfo) &&
                            instructionList[i - 3].Calls(isCurrentProjectMethodInfo) &&
                            instructionList[i - 0].LoadsConstant("StopResearch"))
                        {
                            instructionList[i - 6].opcode = OpCodes.Nop;
                            Log.Message("[CM_Semi_Random_Research] - patching to conditionally hide stop research button at instruction.");
                            instructionList[i - 3] = new CodeInstruction(OpCodes.Call, replacementIsCurrentProject);
                        }
                    }

                    // Remove lockedReasons.Clear(); See prefix above.
                    if (
                        instructionList[i - 1].LoadsField(lockedReasonsFieldInfo) &&
                        instructionList[i - 0].Calls(clearListMethodInfo))
                    {
                        instructionList[i - 1].opcode = OpCodes.Nop;
                        instructionList[i - 0].opcode = OpCodes.Nop;
                    }

                }

                foreach (CodeInstruction instruction in instructionList)
                {
                    yield return instruction;
                }
            }
        
        }

    }
}
