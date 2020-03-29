using UnityEngine;
using Verse;
using System;

namespace oreprocessing
{
    internal static class OreSettingsHelper
    {
        public static OreModSettings ModSettings;
        public static void Reset()
        {
            ModSettings.Reset();
        }
    }

    public class SimpleOres : Mod
    {
        public SimpleOres(ModContentPack content) : base(content)
        {
            this.ModSettings = base.GetSettings<OreModSettings>();
            OreSettingsHelper.ModSettings = this.ModSettings;
        }




        public override void DoSettingsWindowContents(Rect inRect)
        {

            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);
            if(listing_Standard.ButtonText("Default Settings"))
            {
                ModSettings.Reset();
            }
            listing_Standard.Label(String.Format("Days for depleted mine to disappear {0}",ModSettings.DaysToDissappear).ToString());
            ModSettings.DaysToDissappear = (int)Mathf.Round(listing_Standard.Slider(ModSettings.DaysToDissappear,3,60)); 
            listing_Standard.Label(String.Format("Chance for mine to become abadoned after depletion ( every value below 80% might me unbalanced) {0}",ModSettings.ChanceToDissapear).ToString());
            ModSettings.ChanceToDissapear = (int)Mathf.Round(listing_Standard.Slider(ModSettings.ChanceToDissapear,0,100));
            listing_Standard.Label(String.Format("Work time duration, smaller it is the faster is the mining job {0}",(int)ModSettings.WorkDuration).ToString());
            ModSettings.WorkDuration = (int)Mathf.Round(listing_Standard.Slider(ModSettings.WorkDuration, 3000,10000));
            listing_Standard.CheckboxLabeled("[Debug] Should mod log chunk/ore chance?", ref ModSettings.ShouldLog);
            listing_Standard.End();
            ModSettings.Write();
        }

        public override string SettingsCategory()
        {
            return "Simple Ores";
        }


        private OreModSettings ModSettings;
    }
}
