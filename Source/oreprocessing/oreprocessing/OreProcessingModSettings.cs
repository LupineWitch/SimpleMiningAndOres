using Verse;


namespace oreprocessing
{


        public class OreModSettings : ModSettings
        {
            public override void ExposeData()
            {
            base.ExposeData();
            Scribe_Values.Look<int>(ref DaysToDissappear, "DTD",45);
            Scribe_Values.Look<int>(ref ChanceToDissapear, "CTD",100);
            Scribe_Values.Look<float>(ref WorkDuration, "Work", 85000f); 
            Scribe_Values.Look<bool>(ref ShouldLog, "log", false);
            }

        #region Values
        public int DaysToDissappear = 45;
        public int ChanceToDissapear = 100;
        public float WorkDuration = 8000f;
        public bool ShouldLog = false;

        #endregion

        #region Default values
        private int DTDdef = 45;
        private int CTDdef = 100;
        private float WorkDurationDef = 8000f;
        #endregion
        public void Reset()
            {
            DaysToDissappear = DTDdef;
            ChanceToDissapear = CTDdef;
            WorkDuration = WorkDurationDef;
            }

        }
  
}
