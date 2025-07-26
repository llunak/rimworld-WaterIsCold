using RimWorld;
using Verse;
using UnityEngine;

namespace WaterIsCold
{
    public class HediffComp_Evaporation : HediffComp_SeverityPerDay
    {
		private HediffCompProperties_Evaporation Props
		{
			get
			{
				return (HediffCompProperties_Evaporation)this.props;
			}
		}

		public override string CompTipStringExtra
		{
			get
			{
				if (this.props is HediffCompProperties_SeverityPerDay && this.Props.showDaysToRecover && this.SeverityChangePerDay() < 0f)
				{
					return "DaysToRecover".Translate((this.parent.Severity / Mathf.Abs(this.SeverityChangePerDay())).ToString("0.0"));
				}
				return null;
			}
		}

        public override float SeverityChangePerDay()
        {
            float heatFactor = 1f + (this.Pawn.AmbientTemperature - 21)/50;
            return base.SeverityChangePerDay() * Mathf.Max(heatFactor, 0f);
        }
    }
}
