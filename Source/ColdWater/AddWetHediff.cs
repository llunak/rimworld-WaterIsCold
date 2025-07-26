using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;
using HarmonyLib;

namespace WaterIsCold
{
    [HarmonyPatch]
    public class AddWetHediff
    {
        // Since this applies the hediff, this assumes that CanGainGainThoughtNow() is called
        // immediately before gaining the thought, which is the case (at least as of 1.6).
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pawn_MindState), nameof(CanGainGainThoughtNow))]
        public static bool CanGainGainThoughtNow(bool result, ThoughtDef thought, Pawn ___pawn)
        {
            if (thought != ThoughtDefOf.SoakingWet)
            {
                // I assume this is some kind of a modded equivalent of SoakingWet.
                if(ModLister.GetActiveModWithIdentifier("ReGrowth.BOTR.Core") == null)
                    return result;
                if(thought != DefDatabase<ThoughtDef>.GetNamedSilentFail("RG_Wet") && thought != DefDatabase<ThoughtDef>.GetNamedSilentFail("RG_ExtremelyWet"))
                {
                    return result;
                }
            }
            if (ModSettings_WaterIsCold.coldWater)
            {
                AddHediff(___pawn);
            }

            if( !ModSettings_WaterIsCold.soakingWetIfCold && !ModSettings_WaterIsCold.noSoakingWetIfShallowWarm )
                return result;
            float cellTemperature = GenTemperature.GetTemperatureForCell(___pawn.Position,___pawn.Map);
            if( ModSettings_WaterIsCold.noSoakingWetIfShallowWarm )
            {
                TerrainDef terrain = ___pawn.Position.GetTerrain(___pawn.Map);
                // Since the hediff raises the max comfy temperature, in this case use the original value (26C usually).
                // Walking in shallow water when warmer than this is fine.
                if( terrain.IsWater && Utility.IsShallowWater(terrain)
                    && cellTemperature > ___pawn.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax))
                {
                    return false;
                }
            }
            // Note that the comfortable temperature gets affected by the hediff, which is wanted (it'll quickly go from 16C to 21C
            // or worse). Make the pawn dislike the water if it's colder than that.
            if( ModSettings_WaterIsCold.soakingWetIfCold
                && cellTemperature < ___pawn.GetStatValue(StatDefOf.ComfyTemperatureMin))
            {
                return true;
            }
            return result;
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
                // Use the immune duration, so that stopping swimming and walking out of water still counts as swimming.
                if (pawn.Swimming || GenTicks.TicksGame - pawn.mindState.lastSwamTick < Pawn_MindState.SwimSoakingWetImmuneDuration)
                {
                    // The severity range for 'wet' is [0.26,0.51). Do not use the lowest value,
                    // so that the stage stays on for a while (until it's refreshed), but still
                    // use low so that it goes away quickly to just 'damp'.
                    firstHediffOfDef.Severity = 0.32f;
                }
                else if (!Utility.IsShallowWater(terrain))
                {
                    firstHediffOfDef.Severity = 1f;
                }
                else
                {
                    firstHediffOfDef.Severity = 0.5f;
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
