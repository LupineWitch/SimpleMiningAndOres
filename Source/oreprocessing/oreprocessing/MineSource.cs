using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine; 


namespace oreprocessing
{
    public class PlaceWorker_MiningSource : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            List<OreNode> Nodes = map.GetComponent<OreMapComponent>().GetNodes;
            List<IntVec3> CellsToCheck = null;

            foreach (OreNode Node in Nodes)
            {
                if (Node.GetCells.Contains(loc) && Node.HasResource)
                {
                    CellsToCheck = Node.GetCells;
                    break;
                }
            }

            if (CellsToCheck == null)
                return "There is no resource here!";

            for (int i = 0; i < CellsToCheck.Count; i++)
            {
                List<Thing> ThingsOnC = CellsToCheck[i].GetThingList(map);
                for (int j = 0; j < ThingsOnC.Count; j++)
                {
                    Thing rzecz = ThingsOnC[j];
                    ThingDef thingDef = GenConstruct.BuiltDefOf(rzecz.def) as ThingDef;

                    if (thingDef != null && thingDef.building != null)
                    {
                        if (thingDef == ThingDef.Named("MiningPlatform") || thingDef == ThingDef.Named("Blueprint_MiningPlatform") || thingDef == ThingDef.Named("Frame_MiningPlatform"))
                        {
                            return "There is aleady mine on that node!";

                        }
                    }

                }
            }

            return true;
        }

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            base.DrawGhost(def, center, rot, ghostCol);
            Find.CurrentMap.GetComponent<OreMapComponent>().MarkForDraw();
        }
    }


    
}
