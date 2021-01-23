using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;

namespace WaterIsCold
{
    public class JobDriver_Swim : JobDriver
    {
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

        protected override IEnumerable<Toil> MakeNewToils()
        {
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			Toil toil = new Toil();
			toil.tickAction = delegate ()
			{
				JoyUtility.JoyTickCheckEnd(this.pawn, JoyTickFullJoyAction.EndJob, 1f, null);
			};
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.defaultDuration = this.job.def.joyDuration;
			toil.FailOn(() => !this.pawn.Position.GetTerrain(this.pawn.Map).IsWater);
			toil.FailOn(() => this.pawn.Position.GetTerrain(this.pawn.Map) == TerrainDef.Named("Marsh"));
			toil.FailOn(() => this.pawn.Map.mapTemperature.OutdoorTemp < 21f);
			toil.FailOn(() => !JoyUtility.EnjoyableOutsideNow(this.pawn.Map, null));
			yield return toil;
			yield break;
		}


        public override string GetReport()
		{
			TerrainDef terrain = this.pawn.Position.GetTerrain(this.pawn.Map);
			if(terrain == TerrainDefOf.WaterShallow || terrain == TerrainDefOf.WaterMovingShallow || terrain == TerrainDefOf.WaterOceanShallow)
			{
				return "Wading".Translate();
            }
			if (terrain == TerrainDefOf.WaterOceanDeep)
            {
				return "Body surfing".Translate();
            }
			if (terrain == TerrainDefOf.WaterDeep)
            {
				return "Floating".Translate();
            }
			return "Swimming".Translate();
		}
	}
}
