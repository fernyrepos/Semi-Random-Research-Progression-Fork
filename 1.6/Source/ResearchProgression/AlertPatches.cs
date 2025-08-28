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
    public static class Alert_NeedResearchProject_Patches
    {
        [HarmonyPatch(typeof(Alert_NeedResearchProject))]
        [HarmonyPatch("OnClick", MethodType.Normal)]
        public static class Alert_NeedResearchProject_OnClick
        {
            [HarmonyPrefix]
            public static bool Prefix()
            {
                if(SemiRandomResearchMod.settings.featureEnabled)
                {
                    Find.MainTabsRoot.SetCurrentTab(SemiRandomResearchDefOf.CM_Semi_Random_Research_MainButton_Next_Research);
                    return false;
                }
                return true;
            }
        }
    }
}
