using System.Reflection;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;
using HarmonyLib;

namespace WaterIsCold
{
    public class Mod_WaterIsCold : Mod
    {
        Listing_Standard listingStandard = new Listing_Standard();

        public Mod_WaterIsCold(ModContentPack content) : base(content)
        {
            GetSettings<ModSettings_WaterIsCold>();
            Harmony harmony = new Harmony("rimworld.cozarkian.wateriscold");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
        public override string SettingsCategory()
        {
            return "Water Is Cold";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect rect = new Rect(10f, 50f, inRect.width * .75f, inRect.height);
            listingStandard.Begin(rect);
            listingStandard.CheckboxLabeled("Water is cold: ", ref ModSettings_WaterIsCold.coldWater);
            listingStandard.CheckboxLabeled("Water is deep: ", ref ModSettings_WaterIsCold.deepWater);
            listingStandard.GapLine();
            //listingStandard.CheckboxLabeled("Always disable soaking wet thought:", ref ModSettings_WaterIsCold.disableWetAlways);
            //listingStandard.CheckboxLabeled("Disable soaking wet thought when warm (above 21C/70F):", ref ModSettings_WaterIsCold.disableWetWarm);
            if (listingStandard.RadioButton("Always disable soaking wet thought:", ModSettings_WaterIsCold.disableWetAlways))
            {
                ModSettings_WaterIsCold.disableWetAlways = true;
                ModSettings_WaterIsCold.disableWetWarm = false;
                ModSettings_WaterIsCold.disableWetNever = false;
            }
            if (listingStandard.RadioButton("Disable soaking wet when warm (above 26C/78.8F):", ModSettings_WaterIsCold.disableWetWarm))
            {
                ModSettings_WaterIsCold.disableWetAlways = false;
                ModSettings_WaterIsCold.disableWetWarm = true;
                ModSettings_WaterIsCold.disableWetNever = false;
            }
            if (listingStandard.RadioButton("Disable soaking wet when swimming:", !ModSettings_WaterIsCold.disableWetAlways && !ModSettings_WaterIsCold.disableWetWarm && !ModSettings_WaterIsCold.disableWetNever))
            {
                ModSettings_WaterIsCold.disableWetAlways = false;
                ModSettings_WaterIsCold.disableWetWarm = false;
                ModSettings_WaterIsCold.disableWetNever = false;
            }
            if (listingStandard.RadioButton("Swimming makes me angry:", ModSettings_WaterIsCold.disableWetNever))
            {
                ModSettings_WaterIsCold.disableWetAlways = false;
                ModSettings_WaterIsCold.disableWetWarm = false;
                ModSettings_WaterIsCold.disableWetNever = true;
            }
            listingStandard.GapLine();
            string wetInsulationLabel = "Minimum insulation value of clothing when wet (%):";
            string wetInsulationBuffer = ModSettings_WaterIsCold.wetInsFactor.ToString();
            LabeledIntEntry(listingStandard.GetRect(24f), wetInsulationLabel, ref ModSettings_WaterIsCold.wetInsFactor, ref wetInsulationBuffer, 1, 10, 0, 100);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        private void LabeledIntEntry(Rect rect, string label, ref int value, ref string editBuffer, int multiplier, int largeMultiplier, int min, int max)
        {
            float num = rect.width / 15f;
            Widgets.Label(rect, label);
            if (multiplier != largeMultiplier)
            {
                if (Widgets.ButtonText(new Rect(rect.xMax - num * 5f, rect.yMin, (float)num, rect.height), (-1 * largeMultiplier).ToString(), true, true, true))
                {
                    value -= largeMultiplier * GenUI.CurrentAdjustmentMultiplier();
                    editBuffer = value.ToString();
                    SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera(null);
                }
                if (Widgets.ButtonText(new Rect(rect.xMax - num, rect.yMin, num, rect.height), "+" + largeMultiplier.ToString(), true, true, true))
                {
                    value += largeMultiplier * GenUI.CurrentAdjustmentMultiplier();
                    editBuffer = value.ToString();
                    SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera(null);
                }
            }
            if (Widgets.ButtonText(new Rect(rect.xMax - num * 4f, rect.yMin, num, rect.height), (-1 * multiplier).ToString(), true, true, true))
            {
                value -= multiplier * GenUI.CurrentAdjustmentMultiplier();
                editBuffer = value.ToString();
                SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera(null);
            }
            if (Widgets.ButtonText(new Rect(rect.xMax - (num * 2f), rect.yMin, num, rect.height), "+" + multiplier.ToString(), true, true, true))
            {
                value += multiplier * GenUI.CurrentAdjustmentMultiplier();
                editBuffer = value.ToString();
                SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera(null);
            }
            Widgets.TextFieldNumeric<int>(new Rect(rect.xMax - (num * 3f), rect.yMin, num, rect.height), ref value, ref editBuffer, min, max);
        }
    }
}
