using System.Collections;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;

namespace oreprocessing
{
    public class CompDryable : ThingComp
    {
        private float dryProgressInt;

        public CompProperties_Dryable PropsDry => (CompProperties_Dryable)props;

        public float RotProgressPct => DryProgress / (float)PropsDry.TicksToDry;

        public float DryProgress
        {
            get
            {
                return dryProgressInt;
            }
            set
            {
                dryProgressInt = value;
            }
        }

        public int TicksUntilDryAtCurrentTempHumidity
        {
            get
            {
                float ambientHumidity = (Find.WorldGrid[parent.Tile].rainfall / 1000) + Find.WorldGrid[parent.Tile].swampiness;
                float ambientTemperature = parent.AmbientTemperature;
                ambientTemperature = Mathf.RoundToInt(ambientTemperature);
                return TicksUntilDryAtTempHumidity(ambientTemperature, ambientHumidity);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref dryProgressInt, "dryProg", 0f);
        }

        public override void CompTick()
        {
            Tick(1);
        }

        public override void CompTickRare()
        {
            Tick(250);
        }

        private static float DryRateAtTemperature(float temperature)
        {
            if (temperature < 0f)
            {
                return 0f;
            }
            if (temperature >= 10f)
            {
                return temperature / 10;
            }
            return (temperature - 0f) / 10f;
        }

        private void Tick(int interval)
        {
            if (!ParentIsNotContained(parent.ParentHolder)) return;

            float ambientHumidity = Find.WorldGrid[parent.Tile].rainfall / 1000 + Find.WorldGrid[parent.Tile].swampiness;
            float num = DryRateAtTemperature(parent.AmbientTemperature);

            if (ShouldGoWet())
            {
                DryProgress -= 6 * num * (float)interval / ambientHumidity;
                if (DryProgress < 0) DryProgress = 0;
            }
            else
            {
                DryProgress += num * (float)interval / ambientHumidity;
            }

            if (DryProgress > PropsDry.TicksToDry) TransformIntoSomething();
        }

        public bool ParentIsNotContained(IThingHolder holder)
        {
            if (holder is Map) return true;
            return false;
        }

        private void TransformIntoSomething()
        {
            float hitPercent = parent.HitPoints / (float)parent.MaxHitPoints;
            int count = parent.stackCount;
            Thing holder = parent.ParentHolder as Thing;
            bool forbidden = false;
            if (parent.TryGetComp<CompForbiddable>() is CompForbiddable comp)
            {
                forbidden = comp.Forbidden;
            }


            Map map = parent.Map;
            IntVec3 pos = parent.Position;
            parent.DeSpawn();
            parent.Destroy();
            Thing thing = ThingMaker.MakeThing(PropsDry.defDriesTo);
            thing.stackCount = count;

            GenPlace.TryPlaceThing(thing, pos, map, ThingPlaceMode.Direct, out Thing lastResultingThing);
            if (forbidden)
            {
                lastResultingThing.SetForbidden(value: true);
            }
        }

        private bool ShouldGoWet()
        {
            Thing thing = parent.ParentHolder as Thing;
            if (thing != null && thing.def.category == ThingCategory.Building && thing.def.building.preventDeteriorationInside)
            {
                return false;
            }
            else
            if (parent.Map != null)
            {
                if (parent.Map.weatherManager.RainRate > 0 && parent.Map.roofGrid.RoofAt(parent.Position) == null) return true;

                var terrain = parent.Map.terrainGrid.TerrainAt(parent.Position);
                if (terrain.takeSplashes && terrain.extraDeteriorationFactor > 0) return true;
            }
            return false;
        }

        public override void PreAbsorbStack(Thing otherStack, int count)
        {
            float t = (float)count / (float)(parent.stackCount + count);
            float dryProgress = ((ThingWithComps)otherStack).GetComp<CompDryable>().DryProgress;
            DryProgress = Mathf.Lerp(DryProgress, dryProgress, t);
        }

        public override void PostSplitOff(Thing piece)
        {
            ((ThingWithComps)piece).GetComp<CompDryable>().DryProgress = DryProgress;
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if ((float)PropsDry.TicksToDry - DryProgress > 0f)
            {
                int ticksUntilRotAtCurrentTemp = TicksUntilDryAtCurrentTempHumidity;
                stringBuilder.Append("TimeToDry".Translate(ticksUntilRotAtCurrentTemp.ToStringTicksToPeriod()) + ".");
            }
            return stringBuilder.ToString();
        }

        public int TicksUntilDryAtTempHumidity(float temp, float humidity)
        {
            float num = DryRateAtTemperature(temp);
            if (num <= 0f)
            {
                return (int)DryProgress;
            }
            float num2 = (float)PropsDry.TicksToDry - DryProgress;
            if (num2 <= 0f)
            {
                return 0;
            }
            return Mathf.RoundToInt((num2 / num) * humidity);
        }
    }


    public class CompProperties_Dryable : CompProperties
    {
        public float daysToDry = 2f;

        public ThingDef defDriesTo;

        public int TicksToDry => Mathf.RoundToInt(daysToDry * 60000f);

        public CompProperties_Dryable()
        {
            compClass = typeof(CompDryable);
        }

        public CompProperties_Dryable(float daysToDry)
        {
            this.daysToDry = daysToDry;
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (string item in base.ConfigErrors(parentDef))
            {
                yield return item;
            }
            if (parentDef.tickerType != TickerType.Normal && parentDef.tickerType != TickerType.Rare)
            {
                yield return "CompRottable needs tickerType " + TickerType.Rare + " or " + TickerType.Normal + ", has " + parentDef.tickerType;
            }
        }
    }

}
