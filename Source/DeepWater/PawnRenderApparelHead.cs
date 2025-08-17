using System;
using RimWorld;
using Verse;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace WaterIsCold
{
    [HarmonyPatch(typeof(PawnRenderNodeWorker_Apparel_Head))]
    public class PawnRenderNodeWorker_Apparel_Head_Patch
    {
        // Unfortunately this code disables rendering headger if PawnRenderFlags.Clothes is not set,
        // even if PawnRenderFlags.Headgear itself is set. Fix that. Since there's presumably code
        // that relies on this poor design of the flags being coupled, do this only for our use.
        // Since PawnDrawParms is a struct (and thus gets passed by value), simply set the flag
        // for it to nudge the code into allowing the headgear rendering.
        [HarmonyPrefix]
        [HarmonyPatch(nameof(CanDrawNow))]
        static public void CanDrawNow(ref PawnDrawParms parms)
        {
            // Is it the problematic case?
            if (!parms.flags.FlagSet(PawnRenderFlags.Clothes) && parms.flags.FlagSet(PawnRenderFlags.Headgear))
            {
                if( PawnRenderer_Patch.RenderAsInDeepWater( parms.pawn ))
                    parms.flags |= PawnRenderFlags.Clothes;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(HeadgearVisible))]
        static public void HeadgearVisible(ref PawnDrawParms parms)
        {
            // Is it the problematic case?
            if (!parms.flags.FlagSet(PawnRenderFlags.Clothes) && parms.flags.FlagSet(PawnRenderFlags.Headgear))
            {
                if( PawnRenderer_Patch.RenderAsInDeepWater( parms.pawn ))
                    parms.flags |= PawnRenderFlags.Clothes;
            }
        }

        // Hide heargear if swimming and configured so. Do this late (low priority) to override any mods
        // that possibly control drawing of hats (Hats Display Selection).
        [HarmonyPostfix]
        [HarmonyPatch(nameof(CanDrawNow))]
        [HarmonyPriority(Priority.Low)]
        public static bool CanDrawNow(bool result, PawnRenderNode n, PawnDrawParms parms)
        {
            if( !result )
                return result;
            if( parms.pawn.Swimming )
                return false;
            return result;
        }
    }
}
