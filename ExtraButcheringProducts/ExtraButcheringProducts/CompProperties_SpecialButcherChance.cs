using System.Collections.Generic;
using Verse;
using System.Xml;

namespace ExtraButcheringProducts
{


    public class CompProperties_SpecialButcherChance : CompProperties
    {
        public CompProperties_SpecialButcherChance()
        {
            this.compClass = typeof(CompSpecialButcherChance);
        }

        public List<ThingDefCountWithChanceClass> butcherProducts = new List<ThingDefCountWithChanceClass>();

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var item in base.ConfigErrors(parentDef))
                yield return item;


            if (butcherProducts.NullOrEmpty()) yield break;

            //doesn't seem to get picked up.
            //foreach (ThingDefCountWithChanceClass item in this.butcherProducts)
            //{
            //    if (item.chance < 1 || item.chance > 0)
            //        yield return "chance in butcherProducts must be less than 1 and greater than 0.";

            //    if (item.count >= 0)
            //        yield return "count in butcherProducts must be greater than 0";

            //    if (item.thingDef == null)
            //        yield return "thingDef in butcherProducts can't be null";
            //}

        }
    }

    public class CompSpecialButcherChance : ThingComp
    {
        public CompProperties_SpecialButcherChance Props
        {
            get
            {
                return (CompProperties_SpecialButcherChance)this.props;
            }
        }
    }

    public struct ThingDefCountWithChance
    {
    #pragma warning disable 169
        private ThingDef thingDef;
        private int count;
        private float chance;
    #pragma warning restore 169
    }

    public sealed class ThingDefCountWithChanceClass
    {
        public ThingDef thingDef;
        public int      count;
        public float    chance;

        public ThingDefCountWithChanceClass()
        {
        }

        public ThingDefCountWithChanceClass(ThingDef thingDef, int count, float chance)
        {
            this.thingDef = thingDef;
            this.count = count;
            this.chance = chance;
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            if (xmlRoot.ChildNodes.Count != 3)
            {
                Log.Error("Misconfigured ThingDefCountWithChance: " + xmlRoot.OuterXml);
                return;
            }
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thingDef", xmlRoot.FirstChild.InnerText);
            this.count  = (int)ParseHelper.FromString(xmlRoot.ChildNodes[1].InnerText,   typeof(int));
            this.chance = (float)ParseHelper.FromString(xmlRoot.ChildNodes[2].InnerText, typeof(float));
        }
    }
}
