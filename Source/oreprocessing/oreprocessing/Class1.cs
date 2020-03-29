using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.AI;

namespace oreprocessing
{

   

    public class DryingSpot : Building
    {
        private CompDryingSpot DryingSpotComp;
        private int dryingTicks;
        private int TargetTicks
        {
            get
            {
                return this.DryingSpotComp.Props.dryingTicks;
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.dryingTicks, "dryingTicks", 0, false);
        }
        public override void SpawnSetup(Map currentGame, bool respawningAfterLoad)
        {
            base.SpawnSetup(currentGame, respawningAfterLoad);
            this.DryingSpotComp = base.GetComp<CompDryingSpot>();
        }
        public override void TickRare()
        {
            base.TickRare();
            if (this.dryingTicks < this.TargetTicks)
            {
                this.dryingTicks++;
            }
            if (this.dryingTicks >= this.TargetTicks)
            {
                this.PlaceProduct();
            }
        }
        private void PlaceProduct()
        {
            IntVec3 position = base.Position;
            Map map = base.Map;
            Thing product = ThingMaker.MakeThing(ThingDef.Named(this.DryingSpotComp.Props.efekt), null);
            product.stackCount = 9;
            GenPlace.TryPlaceThing(product, position, map, ThingPlaceMode.Near, null);
            GenPlace.TryPlaceThing(ThingMaker.MakeThing(ThingDef.Named("WoodLog"), null), position, map, ThingPlaceMode.Near, null);
            if (!this.Destroyed)
                this.Destroy();


        }
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            if (stringBuilder.Length != 0)
            {
                stringBuilder.AppendLine();
            }
            stringBuilder.AppendLine("Drying progress is" + " " + ((float)this.dryingTicks / (float)this.TargetTicks * 100f).ToString("#0.00") + "%");
            return stringBuilder.ToString().TrimEndNewlines();
        }



    }



    public class CompDryingSpot : ThingComp
    {
        public CompProperties_DryingSpot Props
        {
            get
            {
                return (CompProperties_DryingSpot)this.props;
            }
        }
    }

    public class CompProperties_DryingSpot : CompProperties
    {
        public string efekt;
        public int dryingTicks;
        public CompProperties_DryingSpot()
        {
            this.compClass = typeof(CompDryingSpot);
        }
    }


        public class AbadonedMineClass : Building
    {

        

        private readonly int daysInRareTicks = 240;
        private int collapsingTics = 0;
        public int TargetTicks
        {
            get
            {
                return OreSettingsHelper.ModSettings.DaysToDissappear * daysInRareTicks;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref collapsingTics, "tiks");
           
        }

        public override void TickRare()
        {
            base.TickRare();
            collapsingTics++;
            if (collapsingTics >= TargetTicks)
            {
                this.Destroy();
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            if (stringBuilder.Length != 0)
            {
                stringBuilder.AppendLine();
            }
            stringBuilder.AppendLine(String.Format("Days left:{0}",(int)(TargetTicks-collapsingTics)/daysInRareTicks).ToString());
            return stringBuilder.ToString().TrimEndNewlines();
        }



    }

}