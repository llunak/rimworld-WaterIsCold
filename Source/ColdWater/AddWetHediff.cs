using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;
using HarmonyLib;
using System;

namespace WaterIsCold
{
    [HarmonyPatch]
    public class AddWetHediff
    {
        private static bool lastCanGainThought = false;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pawn_MindState), nameof(CanGainGainThoughtNow))]
        public static bool CanGainGainThoughtNow(bool result, ThoughtDef thought, Pawn ___pawn)
        {
            if( !IsSoakingWetThought( thought ))
                return result;
            // Here we have to force the ability to gain the thought, since we handle getting wet during applying
            // the thought (which first checks e.g. roof if the weather is causing the thought, and that check
            // is not yet done here). Remember the real result for use when applying the thought (this function
            // is always followed by applying the thought if it returns true for soaking wet).
            lastCanGainThought = result;
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MemoryThoughtHandler), nameof(TryGainMemoryFast),
            new Type[] { typeof( ThoughtDef ), typeof( Precept ) } )]
        public static bool TryGainMemoryFast(ThoughtDef mem, Pawn ___pawn)
        {
            if( !IsSoakingWetThought( mem ))
                return true; // Normal operation.
            return HandleSoakingWetThought( ___pawn );
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MemoryThoughtHandler), nameof(TryGainMemoryFast),
            new Type[] { typeof( ThoughtDef ), typeof( int ), typeof( Precept ) } )]
        public static bool TryGainMemoryFast(ThoughtDef mem, int stage, Pawn ___pawn)
        {
            if( !IsSoakingWetThought( mem ))
                return true; // Normal operation.
            return HandleSoakingWetThought( ___pawn );
        }

        private static bool IsSoakingWetThought(ThoughtDef thought)
        {
            if (thought == ThoughtDefOf.SoakingWet)
                return true;
            // I assume this is some kind of a modded equivalent of SoakingWet.
            if(ModLister.GetActiveModWithIdentifier("ReGrowth.BOTR.Core") == null)
                return false;
            return thought == DefDatabase<ThoughtDef>.GetNamedSilentFail("RG_Wet") || thought == DefDatabase<ThoughtDef>.GetNamedSilentFail("RG_ExtremelyWet");
        }

        // Apply the hediff on soaking wet thought (which is caused when the pawn is in water-related situation).
        // Only after that check if the thought itself should be applied (which is independent, as the pawn
        // may be in warm shallow water, which may be configured to not apply the thought, but still should cause the pawn to get somewhat wet).
        private static bool HandleSoakingWetThought( Pawn pawn )
        {
            if( ModSettings_WaterIsCold.coldWater )
                AddHediff(pawn);

            if( !ModSettings_WaterIsCold.soakingWetIfCold && !ModSettings_WaterIsCold.noSoakingWetIfShallowWarm )
                return lastCanGainThought; // We do not change whether the thought is gained.

            float cellTemperature = GenTemperature.GetTemperatureForCell(pawn.Position,pawn.Map);
            if( ModSettings_WaterIsCold.noSoakingWetIfShallowWarm )
            {
                TerrainDef terrain = pawn.Position.GetTerrain(pawn.Map);
                // Since the hediff raises the max comfy temperature, in this case use the original value (26C usually).
                // Walking in shallow water when warmer than this is fine.
                if( terrain.IsWater && Utility.IsShallowWater(terrain)
                    && cellTemperature > pawn.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax))
                {
                    return false; // Skip thought gaining.
                }
            }
            // Note that the comfortable temperature gets affected by the hediff, which is wanted (it'll quickly go from 16C to 21C
            // or worse). Make the pawn dislike the water if it's colder than that.
            if( ModSettings_WaterIsCold.soakingWetIfCold
                && cellTemperature < pawn.GetStatValue(StatDefOf.ComfyTemperatureMin))
            {
                // Only if not fully vacuum resistant.
                if( StatDefOf.VacuumResistance != null
                    && pawn.GetStatValue(StatDefOf.VacuumResistance, applyPostProcess: true, 5) < 1f )
                {
                    return true; // Gain the thought.
                }
            }
            return lastCanGainThought; // We do not change whether the thought is gained.
        }


        public static void AddHediff(Pawn pawn)
        {
            float vacuumResistance = 0;
            if( StatDefOf.VacuumResistance != null )
                vacuumResistance = pawn.GetStatValue(StatDefOf.VacuumResistance, applyPostProcess: true, 5);
            if( vacuumResistance >= 1f )
                return;

            HediffDef def = DefOf_WaterIsCold.WetCold;
            Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(def, false);
            if (firstHediffOfDef == null)
            {
                pawn.health.AddHediff(HediffMaker.MakeHediff(def, pawn, null), null, null, null);
                firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(def, false);
            }

            //Determine level from traversing through water
            TerrainDef terrain = pawn.Position.GetTerrain(pawn.Map);
            float waterSeverity = 0;
            if (terrain.IsWater)
            {
                // Use the immune duration, so that stopping swimming and walking out of water still counts as swimming.
                if (pawn.Swimming || GenTicks.TicksGame - pawn.mindState.lastSwamTick < Pawn_MindState.SwimSoakingWetImmuneDuration)
                {
                    // The severity range for 'wet' is [0.26,0.51). Do not use the lowest value,
                    // so that the stage stays on for a while (until it's refreshed), but still
                    // use low so that it goes away quickly to just 'damp'.
                    waterSeverity = 0.32f;
                }
                else if (Utility.IsShallowWater(terrain))
                    waterSeverity = 0.5f;
                else
                    waterSeverity = 1f;
            }
            waterSeverity *= ( 1 - vacuumResistance );
            //Apply rain
            float rainRate = pawn.Map.weatherManager.RainRate;
            rainRate *= ( 1 - vacuumResistance );
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
                float rainSeverity = firstHediffOfDef.Severity + rainRate * layers / 10;
                rainSeverity = Mathf.Min(rainSeverity, rainRate * layers);
                firstHediffOfDef.Severity = Mathf.Max( waterSeverity, rainSeverity, firstHediffOfDef.Severity );
            }
        }
    }
}
