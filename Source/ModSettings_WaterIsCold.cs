using RimWorld;
using Verse;

namespace WaterIsCold
{
    public class ModSettings_WaterIsCold : ModSettings
    {
        public static bool coldWater = true;
        public static bool deepWater = true;
        public static bool funWater = true;
        public static bool disableWetAlways = false;
        public static bool disableWetWarm = false;
        public static bool disableWetNever = false;
        public static int wetInsFactor = 0;
        public static int swimSearchArea = 100;
        public static float warmWeather = 26f;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref coldWater, "coldWater", true);
            Scribe_Values.Look(ref deepWater, "deepWater", true);
            Scribe_Values.Look(ref funWater, "funWater", true);
            Scribe_Values.Look(ref disableWetAlways, "disableWetAlways", false);
            Scribe_Values.Look(ref disableWetWarm, "disableWetWarm", false);
            Scribe_Values.Look(ref disableWetNever, "disableWetNever", false);
            Scribe_Values.Look(ref wetInsFactor, "wetInsFactor", 0);
            Scribe_Values.Look(ref swimSearchArea, "swimSearchArea", 100);
            base.ExposeData();
        }
    }
}
