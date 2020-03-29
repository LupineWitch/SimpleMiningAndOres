using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace oreprocessing
{
    public class OreNode : ICellBoolGiver, IExposable
    {
        private BoolGrid boolGrid;
        private CellBoolDrawer drawer;
        private List<IntVec3> Cells = null;
        private int Oreindex = -1;
        private Color Colour;
        private IntVec3 Size;
        private string RockName = String.Empty;

        public string GetRockName
        {
            get
            {
                return RockName;
            }
        }


        private void AssignStone()
        {
            List<string> list = new List<string>() { "Sandstone", "Limestone", "Granite", "Marble", "Slate" };

            int d = Rand.Range(1, 5) - 1;

            RockName = String.Format("Chunk{0}", list[d]);
        }

        public void ExposeData()
        {
            Scribe_Values.Look<int>(ref Oreindex, "OreOreIndex");
            Scribe_Values.Look<string>(ref RockName, "OreRockName");
            Scribe_Values.Look<IntVec3>(ref Size, "OreMapSize");
            Scribe_Values.Look<Color>(ref Colour, "OreColor");
            Scribe_Collections.Look(ref Cells, "OreCells",LookMode.Value);
            Scribe_Deep.Look<BoolGrid>(ref boolGrid, "OreBoolGrid");


        }

        public bool HasResource
        {
            get
            {
                if(Oreindex==-1)
                    return false;
                else
                    return true;
            }
        }


        public CompMineShaft GetComp
        {
            get
            {
                Thing mine = ThingMaker.MakeThing(ThingDef.Named("MiningPlatform"), OreDefOf.BlocksAdobe);
                CompMineShaft comp = mine.TryGetComp<CompMineShaft>();
                return comp;
            }
    }

        public int GetIndexMined
        {
            get
            {

                return Oreindex;
            }
        }



        System.Random rand = new System.Random();
        public Color Color => Colour; //new Color(rand.Next(0,20)*(float)rand.NextDouble(), rand.Next(0, 20) * (float)rand.NextDouble(), rand.Next(0,20)*(float)rand.NextDouble(), 0.75f);
        public bool GetCellBool(int index) => boolGrid[index];
        public bool GetCellBool(IntVec3 c) => boolGrid[c];
        public Color GetCellExtraColor(int index) => Color.white;

        public void RemoveFromGrid()
        {
            foreach (IntVec3 c in Cells)
            {
                boolGrid.Set(c, false);
            }
            Drawer.SetDirty();
        }


        public void MarkForDraw()
        {
            
            
                Drawer.CellBoolDrawerUpdate();
                Drawer.MarkForDraw();
                Drawer.CellBoolDrawerUpdate();
            
        }

        public void AddToGrid()
        {
            foreach (IntVec3 c in Cells)
            {
                boolGrid.Set(c, true);
            }
            Drawer.SetDirty();
        }

        private CellBoolDrawer Drawer
        {
            get
            {
                if (drawer == null)
                {
                    drawer = new CellBoolDrawer(this, Size.x, Size.z);
                }
                return drawer;
            }
        }



        public List<IntVec3> GetCells
        {
            get
            {
                return Cells;
            }
        }

        #region constructors
        public OreNode(IntVec3 TR, IntVec3 DL, Map m, Color C, bool WithResource)
        {
           Size = m.Size;
            Colour = C;
            
            
            boolGrid = new BoolGrid(m);

            Cells = CellRect.FromLimits(TR, DL).Cells.ToList<IntVec3>();



            if (WithResource)
            {
                this.AssignStone();
                this.AssignResource();
                this.AddToGrid();
            }
            else
                this.RemoveFromGrid();
            

        }
        

        public OreNode()
        { }
        #endregion 


        public void AssignResource()
        {

            Oreindex = ProportionalWheelSelection.SelectItem(GetComp.PropsMine.mineableList);


            

        }


    }

   public class OreMapComponent : MapComponent
    {
        private List<OreNode> Nodes = new List<OreNode>();

        public List<OreNode> GetNodes
        {
            get
            {
                return Nodes;
            }
        }
        
        public override void MapGenerated()
        {
          
            System.Random rand = new System.Random();


            int x = 0, y = this.map.Size.z;
            IntVec3 xy = new IntVec3(x,0,y);
            IntVec3 xy1 = new IntVec3(x + 24, 0, y + 24);
            int DebugCounter = 0;
            for (x = 0; x < this.map.Size.x-1; x += 25)
            {
                for (y = this.map.Size.z-1; y > 0 ; y -= 25)
                {
                    xy.x = x;
                    xy.z = y;
                    xy1.x = x + 24;
                    xy1.z = y - 24;
                    OreNode next = null;
                    bool WithResource = (rand.Next(0, 100) > 50);
                    switch (DebugCounter)
                    {
                        case 0 :
                            next = new OreNode(xy, xy1, this.map,Color.green, WithResource);
                            DebugCounter++;
                            break;
                        case 1 :
                            next = new OreNode(xy, xy1, this.map,Color.red, WithResource);
                            DebugCounter++;
                            break;
                        case 2:
                            next = new OreNode(xy, xy1, this.map,Color.blue, WithResource);
                            DebugCounter = 0;
                            break;

                    }
                    Nodes.Add(next);
                    
                }
            }
        }

        public OreMapComponent(Map map) : base (map)
        {
        }

        public override void ExposeData()
        {

            Scribe_Collections.Look(ref Nodes, "ResourceNodesOnMap", LookMode.Deep);
            base.ExposeData();
        }

        public void MarkForDraw()
        {
            foreach(OreNode Node in Nodes)
            {
                Node.MarkForDraw();
            }
        }

    }
}
