using Verse;

namespace Doom2016
{
    public class CompProperties_SpawnDOOMOnDamaged : CompProperties
    {
        public CompProperties_SpawnDOOMOnDamaged()
        {
            this.compClass = typeof(CompSpawnDOOMOnDamaged);
        }
    }
}