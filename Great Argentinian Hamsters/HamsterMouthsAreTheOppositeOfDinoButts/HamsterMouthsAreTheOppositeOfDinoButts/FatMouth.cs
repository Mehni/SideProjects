using Verse;

namespace HamsterMouthsAreTheOppositeOfDinoButts
{
    [StaticConstructorOnStartup]
    class FatMouth : DefModExtension
    {
        private Graphic _fatmouthedHamster;

        public Graphic FatMouthedHamster
        {
            get
            {
                if (_fatmouthedHamster == null)
                {
                    _fatmouthedHamster = fatMouthGraphicData.Graphic;
                }
                return _fatmouthedHamster;
            }
        }
#pragma warning disable 0649
        public GraphicData fatMouthGraphicData;
#pragma warning restore 0649
    }
}
