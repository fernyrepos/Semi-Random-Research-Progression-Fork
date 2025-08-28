//using AlienRace;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CM_Semi_Random_Research
{
    [StaticConstructorOnStartup]
    static class Compatibility
    {
        public static bool enabled_AlienRaces = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "erdelf.HumanoidAlienRaces");
        public static bool enabled_SoS2 = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "kentington.saveourship2");
        public static bool enabled_CE = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "CETeam.CombatExtended");
        public static bool enabled_BRT = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "andery233xj.mod.BetterResearchTabs");

        //Some mods need patching later, to avoid errors.

        static Compatibility() 
        {
            var harmony = new Harmony("CM_Semi_Random_Research");
            //if (enabled_BRT)
            //{
            //    PatchBetterResearchTab(harmony);
            //}
        }
        
        public static bool DoCompatibilityChecks(ResearchProjectDef rpd) 
        {
            return SatisfiesAlienRaceRestriction(rpd) &&
                ! rpd.IsDummyResearch() &&
                (SemiRandomResearchMod.settings.experimentalAnomalySupport || !IsAnomalyContent(rpd));
        }

        public static bool IsAnomalyContent(ResearchProjectDef rpd)
        {
            if(rpd == null)
            {
                return false;
            }
            return rpd.baseCost <= 0 || rpd.knowledgeCost > 0;
        }

        public static bool IsHiddenResearch(ResearchProjectDef rpd)
        {
            if (rpd == null)
            {
                return false;
            }

            if(rpd.IsHidden)
            {
                return true;
            }

            if(enabled_SoS2 && rpd.tab.defName  == "ResearchTabArchotech")
            {
                return !SaveOurShip2ArchotechUplinkUnlocked(rpd);
            }

            return false;
        }

        public static bool SatisfiesAlienRaceRestriction(ResearchProjectDef rpd)
        {
            if (rpd!=null && enabled_AlienRaces)
            {
                //return DoRaceCheck(rpd);
                return true;
            }
            else
            {
                return true;
            }
        }

        public static bool IsDummyResearch(this ResearchProjectDef rpd)
        {
            if(rpd == null)
            {
                return false;
            }
            if (enabled_CE && rpd.defName == "VFES_Artillery_Debug")
            {
                return true;
            }
            if(rpd.Cost == 0)
            {
                return true;
            }
            if(rpd.prerequisites != null && rpd.prerequisites.Contains(rpd))
            {
                return true;
            }

            return false;
        }

        private static bool SaveOurShip2ArchotechUplinkUnlocked(ResearchProjectDef rpd)
        {
            return SaveOurShip2.ShipInteriorMod2.WorldComp.Unlocks.Contains("ArchotechUplink");
        }

        //private static bool DoRaceCheck(ResearchProjectDef rpd)
        //{
        //    if (!RaceRestrictionSettings.researchRestrictionDict.ContainsKey(rpd))
        //    {
        //        return true;
        //    }
        //    HashSet<ThingDef> colonistRaces = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoSuspended.Where(x => !x.WorkTagIsDisabled(WorkTags.Intellectual)).Select(x => x.def).ToHashSet();
        //    return RaceRestrictionSettings.CanResearch(colonistRaces, rpd);
        //}


        //private static void PatchBetterResearchTab(Harmony harmony)
        //{
        //    Log.Message("[CM_Semi_Random_Research] - Patching Better Research Tab to remove the start button. This is not as nice looking than in the base-game research tab, but I'm not writing a transpiler for a mod that could change without notice.");
        //    harmony.Patch(typeof(TowersBetterResearchTabs.MainTabWindow_Research).GetMethod("DrawStartButton", BindingFlags.NonPublic | BindingFlags.Instance),
        //    typeof(MainTabWindow_Research_Patches.MainTabWindow_Research_DrawStartButton).GetMethod("PrefixSkip"),
        //    null,
        //    null);

        //    harmony.Patch(typeof(TowersBetterResearchTabs.MainTabWindow_Research).GetMethod("DrawLeftRect", BindingFlags.NonPublic | BindingFlags.Instance),
        //        null, typeof(MainTabWindow_Research_Patches.MainTabWindow_Research_DrawLeftRect).GetMethod("Postfix"));
        //}
    }
}
