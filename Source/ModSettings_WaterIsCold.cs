using RimWorld;
using Verse;

namespace WaterIsCold
{
    public class ModSettings_WaterIsCold : ModSettings
    {
        public static bool coldWater = true;
        public static bool deepWater = true;
        public static bool noHeadgearIfSwimming = true;
        public static bool soakingWetIfCold = true;
        public static bool noSoakingWetIfShallowWarm = true;
        public static int wetInsFactor = 0;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref coldWater, "coldWater", true);
            Scribe_Values.Look(ref deepWater, "deepWater", true);
            Scribe_Values.Look(ref noHeadgearIfSwimming, "noHeadgearIfSwimming", true);
            Scribe_Values.Look(ref soakingWetIfCold, "soakingWetIfCold", true);
            Scribe_Values.Look(ref noSoakingWetIfShallowWarm, "noSoakingWetIfShallowWarm", true);
            Scribe_Values.Look(ref wetInsFactor, "wetInsFactor", 0);
            base.ExposeData();
        }
    }
}
