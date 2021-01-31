using System;
using System.Collections.Generic;
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
			float num = 0;
			if (pawn.Map.mapTemperature.OutdoorTemp > 26f)
			{
				num = Math.Max(0, pawn.Map.mapTemperature.OutdoorTemp) * Math.Max(0,(.75f - pawn.needs.joy.CurLevel));
			}
			return base.GetChance(pawn) * num;
		}

		public override Job TryGiveJob(Pawn pawn)
		{
			Map mapHeld = pawn.MapHeld;
			if (mapHeld == null || !ModSettings_WaterIsCold.funWater || !JoyUtility.EnjoyableOutsideNow(mapHeld, null) || PawnUtility.WillSoonHaveBasicNeed(pawn))
			{
				return null;
			}
			//Log.Message(pawn.Name + "is looking for a swimming hole near " + pawn.Position.x + ", " + pawn.Position.z);
			IntVec3 swimSpot;
			IntVec3 shoreSpot;
			IntVec3 wadeSpot = FindRandomSwimmingHoleNear(pawn.Position, mapHeld, pawn, out swimSpot, out shoreSpot);
			if (shoreSpot == wadeSpot)
            {
				return null;
            }
			return JobMaker.MakeJob(this.def.jobDef, shoreSpot, wadeSpot, swimSpot);
		}

		private static IntVec3 FindRandomSwimmingHoleNear(IntVec3 root, Map map, Pawn swimmer, out IntVec3 swimSpot, out IntVec3 shoreSpot)
		{
			Predicate<IntVec3> swimValidator = delegate (IntVec3 c)
			{
				if (c.x < 12 || c.z < 12 || c.x > map.Size.x - 12 || c.z > map.Size.z - 12) //Avoid the edge
                {
					return false;
                }
				TerrainDef terrain = c.GetTerrain(map);
				if (c.IsForbidden(swimmer) || !terrain.HasModExtension<SwimmableWater>())
				{
					return false;
				}
				//Log.Message(swimmer.Name + " is checking if " + c.x.ToString() + ", " + c.z.ToString() + ", is swimmable");
				if (c.GetTerrain(map).IsRiver)
				{
					//Log.Message("Found a river");
					return true;
				}
				foreach (IntVec3 intVec3 in GenAdjFast.AdjacentCells8Way(c))
				{
					if (!intVec3.GetTerrain(map).HasModExtension<SwimmableWater>())
					{
						return false;
					}
				}
				//Log.Message(swimmer.Name + " found a swimming spot but needs a beach");
				return true;
			};
			if (!RCellFinder.TryFindRandomCellNearWith(root, swimValidator, map, out swimSpot, ModSettings_WaterIsCold.swimSearchArea / 2, ModSettings_WaterIsCold.swimSearchArea))
			{
				shoreSpot = root;
				return root;
			}
			shoreSpot = GetRandomShorelineNear(root, swimSpot, swimmer, map);
			return GetSecondSwimSpotNear(shoreSpot, swimSpot, swimmer, map);
		}

		public static IntVec3 GetRandomShorelineNear(IntVec3 root, IntVec3 swimSpot, Pawn swimmer, Map map)
        {
			CellRect cellRect = CellRect.FromLimits((root.x + swimSpot.x) / 2, (root.z + swimSpot.z) / 2, swimSpot.x, swimSpot.z);
			cellRect.ClipInsideMap(map);
			foreach(IntVec3 cell in cellRect)
            {
				if (!cell.IsForbidden(swimmer) && !cell.GetTerrain(map).IsWater && cell.Standable(map))
                {
					foreach(IntVec3 cell2 in GenAdjFast.AdjacentCellsCardinal(cell))
                    {
						if (cell2.GetTerrain(map).HasModExtension<SwimmableWater>())
                        {
							//Log.Message(swimmer.Name + " found shoreline at " + cell.x.ToString() + ", " + cell.z.ToString());
							return cell;
                        }
                    }
                }
            }
			return root;
        }

		public static IntVec3 GetSecondSwimSpotNear(IntVec3 root, IntVec3 swimSpot, Pawn swimmer, Map map)
		{
			int x = (root.x + swimSpot.x) / 2;
			int z = (root.z + swimSpot.z) / 2;
			if (root.x - swimSpot.x < 3 && root.x - swimSpot.x > 3)
            {
				x = swimSpot.x;
            }
			if (root.z - swimSpot.z < 3 && root.z - swimSpot.z > -3)
            {
				z = swimSpot.z;
            }
			CellRect cellRect = CellRect.FromLimits(x, z, root.x, root.z);
			cellRect.ClipInsideMap(map);
			foreach (IntVec3 cell in cellRect)
			{
				if (!cell.IsForbidden(swimmer) && cell.GetTerrain(map).HasModExtension<SwimmableWater>())
				{
					bool swimmable = true;
					foreach (IntVec3 cell2 in GenAdjFast.AdjacentCells8Way(cell))
					{
						if (!cell2.GetTerrain(map).HasModExtension<SwimmableWater>())
						{
							swimmable = false;
							break;
						}
					}
					if (swimmable)
                    {
						//Log.Message(swimmer.Name + " found second swim spot at " + cell.x.ToString() + ", " + cell.z.ToString());
						return cell;
					}
				}
			}
			if (swimSpot.GetTerrain(map).IsRiver)
            {
				return swimSpot;
            }
			return root;
		}
	}
}
