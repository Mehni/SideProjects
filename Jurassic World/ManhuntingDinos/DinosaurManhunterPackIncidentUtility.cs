using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace ManhuntingDinos
{
    public static class DinosaurManhunterPackIncidentUtility
    {
        //98% vanilla. Removal of "can arrive manhunter" check, addition of min combat power, removal of ByWeight
        public static bool TryFindManhunterAnimalKind(float points, int tile, out PawnKindDef animalKind)
        {
            IEnumerable<PawnKindDef> manhunterPool = from k in DefDatabase<PawnKindDef>.AllDefs
                                                       where k.RaceProps.Animal && (tile == -1 || Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(tile, k.race)) && k.combatPower > 89
                                                       select k;
            return manhunterPool.TryRandomElement(out animalKind);
        }

        public static List<Pawn> GenerateAnimals(PawnKindDef animalKind, int tile, float points)
        {
            int num = Mathf.Max(Mathf.RoundToInt(points / animalKind.combatPower), 1);
            List<Pawn> list = new List<Pawn>();
            for (int i = 0; i < num; i++)
            {
                PawnGenerationRequest request = new PawnGenerationRequest(animalKind, null, PawnGenerationContext.NonPlayer, tile);
                Pawn item = PawnGenerator.GeneratePawn(request);
                list.Add(item);
            }
            return list;
        }
    }
}
