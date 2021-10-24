using RimWorld;
using Verse;
using HarmonyLib;
using UnityEngine;

namespace WaterIsCold
{
    [HarmonyPatch(typeof(PawnRenderer), "RenderCache")]
    public class RenderSwimming
    {
        public static void Prefix (PawnRenderer __instance, ref bool renderBody, bool portrait, Pawn ___pawn)
        {
            //Log.Message("rendering " + ___pawn.LabelShort);
            if (portrait || !ModSettings_WaterIsCold.deepWater)
            {
                return;
            }
            if (___pawn?.Map == null || ___pawn.CarriedBy != null)
            {
                return;
            }
            if (___pawn.Dead || !___pawn.RaceProps.Humanlike)
            {
                return;
            }
            try
            {
                TerrainDef terrain = ___pawn.Position.GetTerrain(___pawn.Map);
                bool deep = terrain != null && (terrain == TerrainDefOf.WaterMovingChestDeep || terrain == TerrainDefOf.WaterOceanDeep || terrain == TerrainDefOf.WaterDeep);
                //Log.Message("deep is " + deep + ", and renderBody is " + renderBody + " for " + ___pawn.LabelShort);
                if (renderBody && deep)
                {
                    //.Message(___pawn.LabelShort + " should no longer have a body");
                    renderBody = false;
                }
            }
            catch
            {
                //ignore
            }
        }
    }
}
