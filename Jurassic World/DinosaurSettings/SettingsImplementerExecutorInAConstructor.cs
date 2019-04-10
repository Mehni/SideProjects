using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace DinosaurSettings
{
    [StaticConstructorOnStartup]
    static class SettingsImplementerExecutorInAConstructor
    {
        static SettingsImplementerExecutorInAConstructor()
        {
            if (!DinoSettings.dinosCanSpawnWild)
            {
                List<PawnKindDef> allDinos = DefDatabase<PawnKindDef>.AllDefsListForReading.FindAll(x => x.RaceProps.wildBiomes?.First() != null && x.RaceProps.wildBiomes.First().commonality != 0 && x.race.description.StartsWith(@"-- >"));

                foreach (PawnKindDef dino in allDinos)
                {
                    //Log.Message(dino.defName + " " + dino.race.description.StartsWith(@" -- >").ToString());
                    for (int i = 0; i < dino.RaceProps.wildBiomes.Count; i++)
                    {
                        dino.RaceProps.wildBiomes[i].commonality = 0f;
                    }
                }
            }

            if (!DinoSettings.dinosCanBeReconstructed)
            {
                ResearchProjectDef[] dinoResearchDefs = new[] { DefDatabase<ResearchProjectDef>.GetNamed("DNAReconstruction"), DefDatabase<ResearchProjectDef>.GetNamed("AmberExtraction") };
                IEnumerable<ThingDef> dinobuildingDefs = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => dinoResearchDefs.Contains(x.researchPrerequisites?.First()));

                foreach (var dbD in dinobuildingDefs)
                {
                    //is in fact pointless because designationCategories are cached.
                    dbD.designationCategory = null;
                }

                foreach (ResearchProjectDef rpd in dinoResearchDefs)
                {
                    DefDatabase<ResearchProjectDef>.AllDefsListForReading.Remove(rpd);
                }
            }
        }
    }
}
