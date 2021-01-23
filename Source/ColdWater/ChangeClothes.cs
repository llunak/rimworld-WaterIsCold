using RimWorld;
using Verse;
using UnityEngine;
using HarmonyLib;

namespace WaterIsCold.WetHediff
{
    [HarmonyPatch(typeof(Pawn_ApparelTracker), "Notify_ApparelRemoved")]
    public class ChangeClothes
    {
        public static void Postfix(Apparel apparel, Pawn ___pawn)
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
                firstHediffOfDef.Severity = Mathf.Max(firstHediffOfDef.Severity * .5f, .25f);
                if (___pawn.needs.mood != null)
                {
                    ___pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDef.Named("SoakingWet"));
                }
            }
        }
    }
}
