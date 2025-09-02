using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Verse;

namespace CM_Semi_Random_Research
{
    [StaticConstructorOnStartup]
    public static class Dialog_ResearchComplete_Patches
    {
        static Dialog_ResearchComplete_Patches()
        {
            try
            {
                var harmony = new Harmony("CM_Semi_Random_Research.Dialog_ResearchComplete_Patches");
                
                // Get the specific FinishProject method we want to patch
                var finishProjectMethod = typeof(ResearchManager).GetMethod("FinishProject", 
                    new Type[] { typeof(ResearchProjectDef), typeof(bool), typeof(Pawn), typeof(bool) });
                
                if (finishProjectMethod != null)
                {
                    harmony.Patch(finishProjectMethod,
                        prefix: new HarmonyMethod(typeof(Dialog_ResearchComplete_Patches), nameof(FinishProject_Prefix)));
                    
                    Log.Message("[Semi Random Research] Successfully patched research completion dialog");
                }
                else
                {
                    Log.Error("[Semi Random Research] Could not find ResearchManager.FinishProject method");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Semi Random Research] Error while patching: {ex}");
            }
        }

        public static void FinishProject_Prefix(ResearchProjectDef proj, ref bool doCompletionDialog, Pawn researcher, ref bool doCompletionLetter)
        {
            try
            {
                if (!SemiRandomResearchMod.settings.featureEnabled)
                    return;

                // Force vanilla notifications to be skipped
                doCompletionDialog = false;
                doCompletionLetter = false;

                // Skip during world generation or special cases
                if (Verse.GenScene.InEntryScene || 
                    Current.Game == null || 
                    Current.Game.World == null ||
                    Current.Game.World.worldObjects == null ||
                    LongEventHandler.AnyEventNowOrWaiting)
                    return;

                // Get research rate information
                var rateTracker = Current.Game.World.GetComponent<ResearchRateTracker>();
                var rateInfo = rateTracker?.GetResearchRateInfo(proj);

                // Create letter text
                StringBuilder letterText = new StringBuilder();
                letterText.AppendLine($"Research completed: {proj.LabelCap}");
                
                if (rateInfo != null && rateInfo.TotalSamples > 0)
                {
                    letterText.AppendLine();
                    letterText.AppendLine($"Average rate: {rateInfo.AverageRateFormatted}");
                }

                if (researcher != null)
                {
                    letterText.AppendLine();
                    letterText.AppendLine($"Completed by: {researcher.LabelShort}");
                }

                // Create and queue the letter
                var letter = LetterMaker.MakeLetter(
                    $"Research Complete: {proj.LabelCap}", 
                    letterText.ToString(), 
                    LetterDefOf.PositiveEvent,
                    researcher != null ? new LookTargets(researcher) : null);

                // Force the letter to be added immediately
                Find.LetterStack.ReceiveLetter(letter);

                // Queue the UI update for the research window
                LongEventHandler.ExecuteWhenFinished(() =>
                {
                    try
                    {
                        // Double check UI is still valid
                        if (Find.UIRoot == null || !(Find.UIRoot is UIRoot_Play))
                            return;

                        // Pause the game
                        Find.TickManager?.Pause();

                        // Open our research window
                        MainButtonDef researchButton = SemiRandomResearchDefOf.CM_Semi_Random_Research_MainButton_Next_Research;
                        if (researchButton != null && Find.MainTabsRoot != null)
                        {
                            Find.MainTabsRoot.SetCurrentTab(researchButton);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[Semi Random Research] Error in queued UI update: {ex}");
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Error($"[Semi Random Research] Error in FinishProject_Prefix: {ex}");
            }
        }
    }
} 