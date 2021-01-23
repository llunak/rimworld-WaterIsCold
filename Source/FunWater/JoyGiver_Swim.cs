using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace WaterIsCold
{
    public class JoyGiver_Swim : JoyGiver
    {
		public override float GetChance(Pawn pawn)
		{
			if (!ModSettings_WaterIsCold.funWater)
            {
				return 0f;
            }
			float num = Math.Max(0, pawn.Map.mapTemperature.OutdoorTemp - 21f);
			return base.GetChance(pawn) * num;
		}

		public override Job TryGiveJob(Pawn pawn)
		{
			Map mapHeld = pawn.MapHeld;
			if (mapHeld == null || !ModSettings_WaterIsCold.funWater)
            {
				return null;
            }
			if (!JoyUtility.EnjoyableOutsideNow(mapHeld, null))
			{
				return null;
			}
			if (PawnUtility.WillSoonHaveBasicNeed(pawn))
			{
				return null;
			}

			IntVec3 c;
			if (!RCellFinder.TryFindRandomCellNearWith(pawn.Position, (IntVec3 pos) => pos.GetTerrain(pawn.Map).HasModExtension<SwimmableWater>(), pawn.Map, out c, 5, 100))
			{
				return null;
			}
			return JobMaker.MakeJob(this.def.jobDef, c);
		}
	}
}
