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
    class JobDriver2ElectricBoogaloocs : JobDriver
    {

        private int ticksToPickHit = -1000;
        private Effecter effecter;
        public const int BaseTicksBetweenPickHits = 100;


        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo targetA = this.job.targetA;
            LocalTargetInfo targetB = this.pawn;
            Job job = this.job;
            return pawn.Reserve(targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            CompMineShaft compMiningPlatform = this.job.targetA.Thing.TryGetComp<CompMineShaft>();
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            this.FailOnThingHavingDesignation(TargetIndex.A, DesignationDefOf.Deconstruct);
            this.FailOn(delegate ()
            {

                return !compMiningPlatform.CanMine();
            });

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            yield return MineStuffFromMine();
            if (compMiningPlatform.ProspectMode)
            {
                yield return ProspectFirst();
            }
            else
            {
                yield return MineShaftYieldStuff();
            }
            yield return ApplyHeDiff();
        }

        private Toil MineStuffFromMine()
        {
            return new Toil()
            {
                tickAction = delegate
                {

                    Building building = (Building)pawn.CurJob.targetA.Thing;
                    CompMineShaft comp = building.GetComp<CompMineShaft>();

                    if (ticksToPickHit < -100)
                    {
                        ResetTicksToPickHit();
                    }
                    if (pawn.skills != null)
                    {
                        pawn.skills.Learn(SkillDefOf.Mining, 0.11f, false);
                    }
                    ticksToPickHit--;

                    if (ticksToPickHit <= 0)
                    {
                        if (effecter == null)
                        {
                            effecter = EffecterDefOf.Mine.Spawn();
                        }
                        effecter.Trigger(pawn, building);

                        ResetTicksToPickHit();
                    }
                },
                defaultDuration = (int)Mathf.Clamp(OreSettingsHelper.ModSettings.WorkDuration / pawn.GetStatValue(StatDefOf.MiningSpeed, true), 800, 16000),
                defaultCompleteMode = ToilCompleteMode.Delay,
                handlingFacing = true
            }.WithProgressBarToilDelay(TargetIndex.B, false, -0.5f);
        }
 
        private Toil MineShaftYieldStuff()
        {
            return new Toil()
            {
                initAction = delegate
                {
                    Building building = (Building)pawn.CurJob.targetA.Thing;
                    CompMineShaft comp = building.GetComp<CompMineShaft>();
                    comp.MiningWorkDone(pawn);
                }
            };


        }

        private Toil ProspectFirst()
        {
            return new Toil()
            {
                initAction = delegate
                {
                    Building building = (Building)pawn.CurJob.targetA.Thing;
                    CompMineShaft comp = building.GetComp<CompMineShaft>();
                    comp.TryFinishProspecting();
                }
            };
        }

        private Toil ApplyHeDiff()
        {
            return new Toil()
            {
                initAction = delegate
                {
                    var hungerOnPawn = pawn?.health?.hediffSet?.GetFirstHediffOfDef(OreDefOf.MinersHunger);
                    if (hungerOnPawn == null)
                    {
                        Hediff hediff = HediffMaker.MakeHediff(OreDefOf.MinersHunger, pawn, null);
                        hediff.Severity = 0.25f;
                        pawn.health.AddHediff(hediff, null, null);
                    }
                    else
                    {
                        hungerOnPawn.Severity += 0.25f;
                    }
                }
            };
        }

        private void ResetTicksToPickHit()
        {
            float num = this.pawn.GetStatValue(StatDefOf.MiningSpeed, true);
            if (num < 0.6f && this.pawn.Faction != Faction.OfPlayer)
            {
                num = 0.6f;
            }
            this.ticksToPickHit = (int)Math.Round((double)(100f / num));
        }

    }

}
