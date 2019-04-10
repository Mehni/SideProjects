using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Doom2016
{
    //Credit to Yaluzan. I am delighted to be able to finally use the below snippet
    public class DeathActionWorker_Advanced : DeathActionWorker
    {
        public override RulePackDef DeathRules => RulePackDefOf.Transition_DiedExplosive;

        public override void PawnDied(Corpse corpse)
        {
            //explode, then do 99 deterioration damage and set a large fire on the corpse
            DoExplosion(corpse.Map, corpse.Position);
            corpse.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, 99));
            Fire fire = (Fire)ThingMaker.MakeThing(ThingDefOf.Fire, null);
            fire.fireSize = 1f;
            GenSpawn.Spawn(fire, corpse.Position, corpse.Map);
        }

        void DoExplosion(Map map, IntVec3 intVec3)
        {
            int[,] design = new int[,]
                {
                    {0,0,0,0,0,0,0,0,0 },
                    {0,1,1,1,1,1,1,1,0 },
                    {0,1,0,0,0,0,0,1,0 },
                    {0,1,0,1,0,1,0,1,0 },
                    {0,1,0,0,0,0,0,1,0 },
                    {0,1,1,0,0,0,1,1,0 },
                    {0,0,1,1,1,1,1,0,0 },
                    {0,0,1,0,0,0,1,0,0 },
                    {0,0,1,1,1,1,1,0,0 },
                };

            for (int x = 0; x < design.GetLength(0); x += 1)
            {
                for (int y = 0; y < design.GetLength(1); y += 1)
                {
                    if (design[x, y] == 1)
                    {
                        IntVec3 newExplosion = new IntVec3(intVec3.x - y + 4, intVec3.y, intVec3.z - x + 4);

                        if (newExplosion.InBounds(map))
                            GenExplosion.DoExplosion(newExplosion, map, 0.2f, DamageDefOf.Flame, null);
                    }
                }
            }
        }
    }
}
