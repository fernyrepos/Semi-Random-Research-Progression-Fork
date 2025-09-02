using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CM_Semi_Random_Research
{
    public static class SemiRandomResearchUtility
    {
        // This little gumdrop is to make my life easy with a transpiler patch for hiding the normal research button
        public static bool CanSelectNormalResearchNow(ResearchProjectDef rpd)
        {
            bool anomaly_enabled = Compatibility.IsAnomalyContent(rpd) && !SemiRandomResearchMod.settings.experimentalAnomalySupport;
            bool enabled = !SemiRandomResearchMod.settings.featureEnabled || anomaly_enabled;
            return enabled && rpd.CanStartNow;
        }
        public static bool IsCurrentProject(ResearchProjectDef rpd)
        {
            bool anomaly_enabled = Compatibility.IsAnomalyContent(rpd) && !SemiRandomResearchMod.settings.experimentalAnomalySupport;
            bool enabled = !SemiRandomResearchMod.settings.featureEnabled || anomaly_enabled;
            return enabled && Find.ResearchManager.IsCurrentProject(rpd);
        }
    }
}
