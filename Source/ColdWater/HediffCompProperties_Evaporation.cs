using RimWorld;
using Verse;

namespace WaterIsCold
{
    public class HediffCompProperties_Evaporation : HediffCompProperties_SeverityPerDay
    {      
        public HediffCompProperties_Evaporation()
        {
            this.compClass = typeof(HediffComp_Evaporation);
        }
    }
}
