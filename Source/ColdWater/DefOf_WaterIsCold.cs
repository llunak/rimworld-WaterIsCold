using RimWorld;
using Verse;

namespace WaterIsCold
{
    [DefOf]
    public class DefOf_WaterIsCold
    {
        public static HediffDef WetCold;

        static DefOf_WaterIsCold()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(DefOf_WaterIsCold));
        }
    }
}
