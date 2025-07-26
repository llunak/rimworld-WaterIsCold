using Verse;

namespace WaterIsCold
{
    public static class Utility
    {
        // Assumes that terrain.IsWater is already true.
        public static bool IsShallowWater( TerrainDef terrain )
        {
            if( terrain.IsOcean )
                return false; // Consider even "shallow" ocean water to not be so shallow.
            // There does not seem to be a direct way to detect shallow water,
            // other than hacks like checking for "Shallow" in the name.
            // Since all shallow water is based on WaterShallowBase,
            // check for its "ShallowWater" affordance, which seems to be the last hackish way.
            if( terrain.affordances.Contains( DefOf_WaterIsCold.ShallowWater ))
                return true;
            return false;
        }
    }
}
