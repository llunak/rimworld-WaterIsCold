using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using HarmonyLib;

namespace WaterIsCold
{
    [HarmonyPatch]
    public class WetClothes
    {
        [HarmonyPatch(typeof(StatPart_ApparelStatOffset), "TransformValue")]
        [HarmonyPrefix]
        public static bool IgnoreInsulationWhenWet(StatRequest req, ref float val, StatDef ___apparelStat, bool ___subtract)
        {
            if (___apparelStat != StatDefOf.Insulation_Cold || !___subtract)
            {
                return true;
            }
            if (req.HasThing && req.Thing != null)
            {
                Pawn pawn = req.Thing as Pawn;
                if (pawn != null)
                {
                    Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(DefOf_WaterIsCold.WetCold, false);
                    if (firstHediffOfDef == null)
                    {
                        return true;
                    }
                    float multiplier = Mathf.Clamp(1f - firstHediffOfDef.Severity, ModSettings_WaterIsCold.wetInsFactor / 100f, 1f);
                    if (pawn.apparel != null && multiplier > 0)
                    {
                        for (int i = 0; i < pawn.apparel.WornApparel.Count; i++)
                        {
                            Apparel apparel = pawn.apparel.WornApparel[i];
                            float num = apparel.GetStatValue(___apparelStat, true);
                            num += StatWorker.StatOffsetFromGear(apparel, ___apparelStat);
                            if (!apparel.def.HasModExtension<WaterGear>())
                            {
                                num *= multiplier;
                            }
                            val -= num;
                        }
                    }
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(StatPart_ApparelStatOffset), "ExplanationPart")]
        [HarmonyPrefix]
        public static bool IgnoreInsulationWhenWet(StatPart_ApparelStatOffset __instance, StatRequest req, ref string __result, StatDef ___apparelStat, bool ___subtract)
        {
            if (___apparelStat != StatDefOf.Insulation_Cold || !___subtract)
            {
                return true;
            }
            if (req.HasThing && req.Thing != null)
            {
                Pawn pawn = req.Thing as Pawn;
                if (pawn != null && PawnWearingRelevantGear(req, pawn, ___apparelStat))
                {
                    Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(DefOf_WaterIsCold.WetCold, false);
                    if (firstHediffOfDef == null)
                    {
                        return true;
                    }
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine("StatsReport_RelevantGear".Translate());
                    if (pawn.apparel != null)
                    {
                        for (int i = 0; i < pawn.apparel.WornApparel.Count; i++)
                        {
                            Apparel gear = pawn.apparel.WornApparel[i];
                            stringBuilder.AppendLine(InfoTextLineFrom(__instance, gear, firstHediffOfDef.Severity, ___apparelStat));
                        }
                    }
                    __result = stringBuilder.ToString();
                    return false;
                }
            }
            __result = null;
            return false;
        }

        private static bool PawnWearingRelevantGear(StatRequest req, Pawn pawn, StatDef apparelStat)
        {
            if (pawn.apparel != null)
            {
                for (int i = 0; i < pawn.apparel.WornApparel.Count; i++)
                {
                    Apparel apparel = pawn.apparel.WornApparel[i];
                    if (apparel.GetStatValue(apparelStat, true) != 0f)
                    {
                        return true;
                    }
                    if (StatWorker.StatOffsetFromGear(apparel, apparelStat) != 0f)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static string InfoTextLineFrom(StatPart_ApparelStatOffset instance, Thing gear, float heddifSeverity, StatDef apparelStat)
        {
            float num = gear.GetStatValue(apparelStat, true);
            num += StatWorker.StatOffsetFromGear(gear, apparelStat);
            num = -num;
            if (!gear.def.HasModExtension<WaterGear>())
            {
                num = num * Mathf.Clamp(1f - heddifSeverity, ModSettings_WaterIsCold.wetInsFactor, 1f);
                return "    " + gear.LabelCap + ": " + num.ToStringByStyle(instance.parentStat.toStringStyle, ToStringNumberSense.Offset) + " (wet)";
            }
            return "    " + gear.LabelCap + ": " + num.ToStringByStyle(instance.parentStat.toStringStyle, ToStringNumberSense.Offset) ;
        }

        [HarmonyPatch(typeof(Pawn_ApparelTracker), "Notify_ApparelRemoved")]
        [HarmonyPostfix]
        public static void RemoveWetClothes(Apparel apparel, Pawn ___pawn)
        {
            //Log.Message(___pawn.Name + " changed clothes");
            if (___pawn.Dead)
            {
                return;
            }
            Hediff firstHediffOfDef = ___pawn.health.hediffSet.GetFirstHediffOfDef(DefOf_WaterIsCold.WetCold, false);
            if (firstHediffOfDef == null)
            {
                return;
            }
            if (apparel.def.apparel.layers.Contains(ApparelLayerDefOf.OnSkin) || apparel.def.apparel.layers.Contains(ApparelLayerDefOf.Middle))
            {
                firstHediffOfDef.Severity = Mathf.Max(firstHediffOfDef.Severity * .75f, .25f);
            }
        }

        [HarmonyPatch(typeof(Pawn_ApparelTracker), "Notify_ApparelAdded")]
        [HarmonyPostfix]
        public static void AddDryClothes(Apparel apparel, Pawn ___pawn)
        {
            //Log.Message(___pawn.Name + " changed clothes");
            if (___pawn.Dead)
            {
                return;
            }
            Hediff firstHediffOfDef = ___pawn.health.hediffSet.GetFirstHediffOfDef(DefOf_WaterIsCold.WetCold, false);
            if (firstHediffOfDef == null)
            {
                return;
            }
            if (apparel.def.apparel.layers.Contains(ApparelLayerDefOf.OnSkin) || apparel.def.apparel.layers.Contains(ApparelLayerDefOf.Middle) || apparel.def.apparel.layers.Contains(ApparelLayerDefOf.Shell))
            {
                firstHediffOfDef.Severity *= .75f;
            }
        }
    }
}
