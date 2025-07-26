using RimWorld;
using Verse;

namespace WaterIsCold
{
    public class ModSettings_WaterIsCold : ModSettings
    {
        public static bool coldWater = true;
        public static bool deepWater = false;
        public static bool disableWetAlways = false;
        public static bool disableWetWarm = false;
        public static bool disableWetNever = false;
        public static int wetInsFactor = 0;
        //public static float warmWeather = 26f;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref coldWater, "coldWater", true);
            Scribe_Values.Look(ref deepWater, "deepWater", false);
            Scribe_Values.Look(ref disableWetAlways, "disableWetAlways", false);
            Scribe_Values.Look(ref disableWetWarm, "disableWetWarm", false);
            Scribe_Values.Look(ref disableWetNever, "disableWetNever", false);
            Scribe_Values.Look(ref wetInsFactor, "wetInsFactor", 0);
            base.ExposeData();
        }
    }
}
