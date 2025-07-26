using RimWorld;
using Verse;
using UnityEngine;
using HarmonyLib;

namespace WaterIsCold
{
    [HarmonyPatch]
    public class AddWetHediff
    {
        //public static List<ThoughtDef> wetThoughts = new List<ThoughtDef>() {ThoughtDef.Named("SoakingWet")};

        [HarmonyPatch(typeof(MemoryThoughtHandler), "TryGainMemoryFast")]
        public static bool Prefix(ThoughtDef mem, Pawn ___pawn)
        {
            if (mem != ThoughtDef.Named("SoakingWet"))
            {
                if (ModLister.GetActiveModWithIdentifier("ReGrowth.BOTR.Core") == null || (mem != DefDatabase<ThoughtDef>.GetNamedSilentFail("RG_Wet") && mem != DefDatabase<ThoughtDef>.GetNamedSilentFail("RG_ExtremelyWet")))
                {
                    return true;
                }
            }
            if (ModSettings_WaterIsCold.coldWater)
            {
                AddHediff(___pawn);
            }
            if (ModLister.GetActiveModWithIdentifier("VanillaExpanded.VanillaTraitsExpanded") != null)
            {
                if (___pawn.story.traits.HasTrait(DefDatabase<TraitDef>.GetNamedSilentFail("VTE_ChildOfSea"))) return true;
            }
            if (ModSettings_WaterIsCold.disableWetAlways) return false;
            if (ModSettings_WaterIsCold.disableWetNever) return true;
            if (ModSettings_WaterIsCold.disableWetWarm) return ___pawn.Map.mapTemperature.OutdoorTemp <= 26;
            return ___pawn.jobs.curJob?.def != DefOf_WaterIsCold.WIC_Swim;
        }

        public static void AddHediff(Pawn pawn)
        {
            HediffDef def = DefOf_WaterIsCold.WetCold;
            Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(def, false);
            if (firstHediffOfDef == null)
            {
                pawn.health.AddHediff(HediffMaker.MakeHediff(def, pawn, null), null, null, null);
                firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(def, false);
            }
            float curSeverity = firstHediffOfDef.Severity;

            //Determine level from traversing through water
            TerrainDef terrain = pawn.Position.GetTerrain(pawn.Map);
            if (terrain.IsWater)
            {
                if (terrain == TerrainDefOf.WaterDeep || terrain == TerrainDefOf.WaterOceanDeep || terrain == TerrainDefOf.WaterMovingChestDeep)
                {
                    firstHediffOfDef.Severity = 1f;
                }
                else
                {
                    firstHediffOfDef.Severity = .5f;
                }
                return;
            }
            //Apply rain
            float rainRate = pawn.Map.weatherManager.RainRate;
            if (pawn.apparel != null)
            {
                bool outer = false;
                bool middle = false;
                bool skin = false;
                float layers = 0f;
                for (int i = 0; i < pawn.apparel.WornApparelCount; i++)
                {
                    Apparel apparel = pawn.apparel.WornApparel[i];
                    if (apparel.def.apparel.layers.Contains(ApparelLayerDefOf.Shell) && !outer)
                    {
                        outer = true;
                        layers += .5f;
                    }
                    if (apparel.def.apparel.layers.Contains(ApparelLayerDefOf.Middle) && !middle)
                    {
                        middle = true;
                        layers += .25f;
                    }
                    if (apparel.def.apparel.layers.Contains(ApparelLayerDefOf.OnSkin) && !skin)
                    {
                        skin = true;
                        layers += .25f;
                    }
                }
                layers = 1.25f - Mathf.Max(.25f, layers);
                curSeverity += rainRate * layers / 10;
                firstHediffOfDef.Severity = Mathf.Min(curSeverity, rainRate * layers);
            }
        }
    }
}
