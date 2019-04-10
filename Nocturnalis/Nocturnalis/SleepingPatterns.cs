using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace Nocturnalis
{
    public class SleepingPatterns : DefModExtension
    {
        public Dictionary<SleepingHabits, Pair<int, int>> napTime
            = new Dictionary<SleepingHabits, Pair<int, int>>
              {
                  {SleepingHabits.Diurnal,    new Pair<int, int>(7 ,21)}, //wake at 7, sleep at 21 == 14 waking hours (or 10 sleeping hrs)
                  {SleepingHabits.Nocturnal,  new Pair<int, int>(20, 7)},
                  {SleepingHabits.Matutinal,  new Pair<int, int>(5, 13)},
                  {SleepingHabits.Vespertine, new Pair<int, int>(17, 1)}
              };

        public SleepingHabits sleepingHabit;
    }

    public enum SleepingHabits
    {
        // ReSharper disable InconsistentNaming
        Diurnal,
        Nocturnal,
        Matutinal,
        Vespertine,
        // ReSharper restore InconsistentNaming
    }
}
