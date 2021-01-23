using System;
using RimWorld;
using Verse;
using HarmonyLib;
using UnityEngine;

namespace WaterIsCold.Swimming
{
    [HarmonyPatch]
    public class RenderSwimming
    {
        [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal", new Type[] { typeof(Vector3), typeof(float), typeof(bool), typeof(RotDrawMode), typeof(bool), typeof(bool) })]
        private static void Prefix(ref bool renderBody, Pawn ___pawn)
        {
            if (ModSettings_WaterIsCold.deepWater && renderBody)
            {
                TerrainDef terrain = ___pawn.Position.GetTerrain(___pawn.Map);
                if (terrain == TerrainDefOf.WaterMovingChestDeep || terrain == TerrainDefOf.WaterOceanDeep || terrain == TerrainDefOf.WaterDeep)
                {
                    renderBody = false;
                }
            }
        }
    }
}
