using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CM_Semi_Random_Research
{
    [StaticConstructorOnStartup]
    public static class ResearchManager_Patches
    {
        [HarmonyPatch(typeof(ResearchManager))]
        [HarmonyPatch("FinishProject", MethodType.Normal)]
        public static class ResearchManager_FinishProject
        {

            [HarmonyPrefix]
            public static void HarmonyPrefix(ResearchProjectDef proj)
            {
                ResearchTracker researchTracker = Current.Game?.World?.GetComponent<ResearchTracker>();
                if (researchTracker != null)
                {
                    researchTracker.ConsiderProjectFinished(proj);
                }
            }
        }

        [HarmonyPatch(typeof(ResearchManager))]
        [HarmonyPatch("AddProgress", MethodType.Normal)]
        public static class ResearchManager_AddProgress
        {

            [HarmonyPrefix]
            public static void Prefix(ResearchProjectDef proj, float amount, Pawn source)
            {
                ResearchTracker researchTracker = Current.Game?.World?.GetComponent<ResearchTracker>();
                if (researchTracker != null &&
                    (proj.ProgressReal == 0 || SemiRandomResearchMod.settings.progressAddsChoice == ProgressAddsChoice.AddChoiceOnlyOnGain) &&
                    SemiRandomResearchMod.settings.progressAddsChoice != ProgressAddsChoice.Never &&
                    ! researchTracker.GetCurrentlyAvailableProjects().Contains(proj) &&
                    proj.CanStartNow)
                {
                    if(!researchTracker.CurrentProject.Any(x => x.knowledgeCategory == proj.knowledgeCategory) ||
                        SemiRandomResearchMod.settings.allowSwitchingResearch)
                    {
                        researchTracker.AddProjectToAvailableProjects(proj);
                    }
                }
            }
        }
    }
}
