using System;
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
        public static void Prefix(ThoughtDef mem, Pawn ___pawn)
        {
            if (!ModSettings_WaterIsCold.coldWater)
            {
                return;
            }
            if (mem != ThoughtDef.Named("SoakingWet"))
            {
                if (ModLister.GetActiveModWithIdentifier("ReGrowth.BOTR.Core") != null)
                {
                    if (mem == ThoughtDef.Named("RG_Wet") || mem == ThoughtDef.Named("RG_CompletelyWet"))
                    {
                        AddHediff(___pawn);
                    }
                }
                return;
            }
            AddHediff(___pawn);
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

            //Shortcut if went through water or clothed with single inner layer
            if (curSeverity > .75f)
            {
                firstHediffOfDef.Severity = 1f;
                return;
            }
            
            //Determine initial level
            TerrainDef terrain = pawn.Position.GetTerrain(pawn.Map);
            if (terrain.IsWater)
            {
                if (terrain == TerrainDefOf.WaterDeep || terrain == TerrainDefOf.WaterOceanDeep || terrain == TerrainDefOf.WaterMovingChestDeep)
                {
                    firstHediffOfDef.Severity = .75f;
                }
                if (pawn.apparel.AnyApparel)
                {
                    firstHediffOfDef.Severity += .25f;
                    for (int j = 0; j < pawn.apparel.WornApparelCount; j++)
                    {
                        Apparel clothing = pawn.apparel.WornApparel[j];
                        if (clothing.def.HasModExtension<WaterGear>() && clothing.def.GetModExtension<WaterGear>().wetSuit)
                        {
                            firstHediffOfDef.Severity -= .25f;
                            break;
                        }
                    }
                }
                return;
            }
            //Must be raining - determine level based on apparel
            bool outer = false;
            bool middle = false;
            bool skin = false;
            for (int i = 0; i < pawn.apparel.WornApparelCount; i++)
            {
                Apparel apparel = pawn.apparel.WornApparel[i];
                if (apparel.def.HasModExtension<WaterGear>() && apparel.def.GetModExtension<WaterGear>().rainSuit)
                {
                    firstHediffOfDef.Severity = Mathf.Max(curSeverity, .25f);
                    return;
                }
                if (apparel.def.apparel.layers.Contains(ApparelLayerDefOf.Shell))
                {
                    outer = true;
                }
                if (apparel.def.apparel.layers.Contains(ApparelLayerDefOf.Middle))
                {
                    middle = true;
                }
                if (apparel.def.apparel.layers.Contains(ApparelLayerDefOf.OnSkin))
                {
                    skin = true;
                }
            }
            if (outer)
            {
                if (middle && skin)
                    firstHediffOfDef.Severity = Mathf.Max(curSeverity, .25f);
                else if (middle || skin)
                    firstHediffOfDef.Severity = Mathf.Max(curSeverity, .5f);
                else
                    firstHediffOfDef.Severity = Mathf.Max(curSeverity, .75f);
            }
            else if (middle && skin)
                firstHediffOfDef.Severity = Mathf.Max(curSeverity, .75f);
            else if (middle || skin)
                firstHediffOfDef.Severity = 1f;
            else
                firstHediffOfDef.Severity = Mathf.Max(curSeverity, .5f);
        }
    }
}
