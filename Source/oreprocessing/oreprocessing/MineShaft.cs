using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.Sound;

namespace oreprocessing
{



    public class CompMineShaft : ThingComp
    {

        public CompProperties_MineShaft PropsMine
        {
            get
            {
                return (CompProperties_MineShaft)this.props;
            }
        }

        
        public bool ProspectMode = true;
        private int lastUsedTick = -99999;
        private int OreIndex = -1;
        public string defaultTexPath;
        private int OreIn = 0;
        private int  OreDug = 0;
        private string RockName = String.Empty;


        private bool Confirm = false;




        public void DepleteMine()
        {
            OreDug = OreIn;
        }


        public void PlaceAbadonedMine()
        {
            Thing abadoned = ThingMaker.MakeThing(ThingDef.Named("AbadonedPlatform"),this.parent.Stuff);
            Map temp = this.parent.Map;
            IntVec3 temV = this.parent.Position;
            this.parent.Destroy();
            
            GenPlace.TryPlaceThing(abadoned, temV, temp, ThingPlaceMode.Direct, null, null);
        }



        #region Job Related methods

        public void MiningWorkDone(Pawn Miner)
        {            
            this.lastUsedTick = Find.TickManager.TicksGame;
            OreDug++;
            TryfinishMining(Miner.GetStatValue(StatDefOf.MiningYield, true));  

        }

        private void TryfinishMining(float TotalYield)
        {  

            GenPlace.TryPlaceThing(ThingMaker.MakeThing(ThingDefOf.Filth_RubbleRock), this.parent.InteractionCell.RandomAdjacentCell8Way(), this.parent.Map, ThingPlaceMode.Near);
                                 
            //Roll if chunk is to be mined
            System.Random rand = new System.Random();
            double k = rand.NextDouble();
            if (OreSettingsHelper.ModSettings.ShouldLog)
            {
                Log.Message(String.Format("Chunk Chance was: {0} Ajusted TotalYieldChance is {1}", k, TotalYield - 0.4d));
            }
            if (k > ((double)TotalYield-0.4d))
            {
                Thing chunk = ThingMaker.MakeThing(ThingDef.Named(RockName));
                GenPlace.TryPlaceThing(chunk, this.parent.InteractionCell, this.parent.Map, ThingPlaceMode.Near, null, null);
            }
            else
            {
                ThingDef thingDef = PropsMine.mineableList[OreIndex].thingDef;
                Thing thing = ThingMaker.MakeThing(thingDef, null);
                thing.stackCount = (int)(PropsMine.mineableList[OreIndex].yield * TotalYield);
                GenPlace.TryPlaceThing(thing, this.parent.InteractionCell, this.parent.Map, ThingPlaceMode.Near, null, null);
            }

            if (OreDug >= OreIn)
            {
                if (rand.Next(0, 100) > OreSettingsHelper.ModSettings.ChanceToDissapear)
                    ProspectMode = true;
                else
                {
                    Messages.Message("Mine depleted!", new TargetInfo(parent.InteractionCell, parent.Map, false), MessageTypeDefOf.NegativeEvent);
                    PlaceAbadonedMine();
                }
            }


        }
            
        public void TryFinishProspecting()
        {
            List<OreNode> Nodes = this.parent.Map.GetComponent<OreMapComponent>().GetNodes;
            foreach(OreNode N in Nodes)
                {
                if (N.GetCells.Contains(this.parent.Position))  
                    {
                    OreIndex = N.GetIndexMined;
                    RockName = N.GetRockName;
                    OreIn = PropsMine.mineableList[OreIndex].OreLevel;
                    }
                }
            
            OreDug = 0;
            ProspectMode = false;
            Messages.Message(String.Format("New ore was found! It is {0}.", PropsMine.mineableList[OreIndex].thingDef.label.ToString()), new TargetInfo(parent.InteractionCell, parent.Map, false), MessageTypeDefOf.PositiveEvent);
        }

        #endregion



        //Saving
        public override void PostExposeData()
        {
            Scribe_Values.Look<bool>(ref ProspectMode, "ProspectModeone", false);
            Scribe_Values.Look<int>(ref lastUsedTick, "lastusedTick", -99999);
            Scribe_Values.Look<int>(ref OreIndex, "Ore_currentResource", 0);
            Scribe_Values.Look<int>(ref this.OreDug, "Ore_depletionLevel", 0);
            Scribe_Values.Look<int>(ref this.OreIn, "Ore_depletionMax", 450);
            Scribe_Values.Look<string>(ref this.RockName, "Stone", "ChunkGranite");
        }




        //BUTTONS
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo c in base.CompGetGizmosExtra())
            {
                yield return c;
            }

            if (!Confirm)
            {
                Command_Action AbadonMine = new Command_Action()
                {
                    defaultLabel = "Abadon",
                    defaultDesc = "Abadon mine if you wish to deconstruct it",
                    activateSound = SoundDef.Named("Click"),
                    icon = Utilities.GetIcon(),
                    action = () => { Confirm = true; },
                };
                yield return AbadonMine;
            }
            else
            {
                Command_Action AbadonMine2 = new Command_Action()
                {
                    defaultLabel = "Are you sure?",
                    defaultDesc = "Yes, I want  to abadon mine",
                    activateSound = SoundDef.Named("Click"),
                    icon = Utilities.GetIcon(),
                    action = () => { PlaceAbadonedMine(); },
                };
                yield return AbadonMine2;  
                
                Command_Action iDont= new Command_Action()
                {
                    defaultLabel = "No, go back",
                    defaultDesc = "What did I even click?",
                    activateSound = SoundDef.Named("Click"),
                    icon = Utilities.GetIcon(),
                    action = () => { Confirm = false; },
                };
                yield return iDont;
            }


            if (Prefs.DevMode)
            {
                
                
                Command_Action EmptyMine = new Command_Action()
                {
                    defaultLabel = "Empty Mine",
                    activateSound = SoundDef.Named("Click"),
                    action = () => { DepleteMine(); },
                };
                yield return EmptyMine;     
                
                Command_Action UpdateMine = new Command_Action()
                {
                    defaultLabel = "Update mine",
                    defaultDesc = "Sets mined resource to coal, WARING! USE ONLY TO UPDATE MINES BROKEN BY COAL UPDATE!",
                    activateSound = SoundDef.Named("Click"),
                    action = () => { DepleteMine(); },
                };
                yield return EmptyMine;
            }
            
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder builder = new StringBuilder();
            if (ProspectMode)
            {
                builder.AppendLine("Mine is in prospecting mode");
            }
            else
            {
                builder.AppendLine("Mine is in mining mode");
            }
            if (OreIndex == -1)
                builder.AppendLine("No ore has been yet found!");
            else
            {
                builder.AppendFormat("Dug ore is {0}\n", PropsMine.mineableList[OreIndex].thingDef.label);
                builder.Append(OreDug + "/" + OreIn);
                builder.AppendLine(" of mining operations is depleted.");
            }
            return builder.ToString().TrimEndNewlines();
        }

        public bool CanMine()
        {
            if (this.OreDug < this.OreIn || ProspectMode )
                return true;
            else
             return false;
            
        }

        public override void PostDeSpawn(Map map)
        {
            this.lastUsedTick = -99999;
        }
        public bool UsedLastTick()
        {
            return this.lastUsedTick >= Find.TickManager.TicksGame - 1;
        }
    }

    public class CompProperties_MineShaft : CompProperties
    {

        public List<thingMined> mineableList = new List<thingMined>();

        public CompProperties_MineShaft()
        {
            this.compClass = typeof(CompMineShaft);
        }
        public override void ResolveReferences(ThingDef parentDef)
        {
            base.ResolveReferences(parentDef);
        }

    }



}
