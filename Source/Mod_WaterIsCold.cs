using System.Reflection;
using UnityEngine;
using RimWorld;
using Verse;
using HarmonyLib;

namespace WaterIsCold
{
    public class Mod_WaterIsCold : Mod
    {
        Listing_Standard listingStandard = new Listing_Standard();

        public Mod_WaterIsCold(ModContentPack content) : base(content)
        {
            GetSettings<ModSettings_WaterIsCold>();
            Harmony harmony = new Harmony("rimworld.varietymattersfashion");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
        public override string SettingsCategory()
        {
            return "Water Is Cold";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect rect = new Rect(100f, 50f, inRect.width * .8f, inRect.height);

            listingStandard.Begin(rect);
            listingStandard.CheckboxLabeled("Water is cold: ", ref ModSettings_WaterIsCold.coldWater);
            listingStandard.CheckboxLabeled("Water is deep: ", ref ModSettings_WaterIsCold.deepWater);
            listingStandard.CheckboxLabeled("Water is fun: ", ref ModSettings_WaterIsCold.funWater);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }
    }
}
