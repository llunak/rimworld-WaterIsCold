using RimWorld;
using Verse;

namespace WaterIsCold
{
    public class ModSettings_WaterIsCold : ModSettings
    {
        public static bool coldWater = true;
        public static bool deepWater = true;
        public static bool funWater = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref coldWater, "coldWater", true);
            Scribe_Values.Look(ref deepWater, "deepWater", true);
            Scribe_Values.Look(ref funWater, "funWater", true);
            base.ExposeData();
        }
    }
}
