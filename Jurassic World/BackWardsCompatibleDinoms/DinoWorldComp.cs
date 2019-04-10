//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using RimWorld;
//using Verse;
//using RimWorld.Planet;

//namespace BackWardsCompatibleDinoms
//{
//    class DinoWorldComp : WorldComponent
//    {
//        public bool alreadyDinofied = false;

//        public DinoWorldComp(World world) : base(world)
//        {
//        }

//        public override void ExposeData()
//        {
//            base.ExposeData();
//            Scribe_Values.Look(ref alreadyDinofied, "alreadyDinofied");
//        }
//    }
//}
