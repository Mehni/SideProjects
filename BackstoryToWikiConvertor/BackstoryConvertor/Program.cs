using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;

namespace BackstoryConvertor
{
    public static class Extension
    {
        public static string CheckOrCoss(this IEnumerable<XElement> spawnCategories, string spawnCat)
            => spawnCategories.Descendants("li").Any(x => x.Value == spawnCat) ? "check" : "cross";

        public static string CapitaliseFirst(this string str)
            => char.ToUpper(str[0]) + str.Substring(1);
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string workingDir = Directory.GetCurrentDirectory();

                List<XElement> adulthoods = new List<XElement>();
                List<XElement> childhoods = new List<XElement>();

                adulthoods = PopulateHoods(workingDir, adulthoods, "*_Adult.xml");
                childhoods = PopulateHoods(workingDir, childhoods, "*_Child.xml");

                TurnBackStoriesIntoTables(adulthoods, workingDir + Path.DirectorySeparatorChar + "adulthoods");
                TurnBackStoriesIntoTables(childhoods, workingDir + Path.DirectorySeparatorChar + "childhoods");

                List<XElement> adulthoodBacker = new List<XElement>();
                List<XElement> childhoodBacker = new List<XElement>();

                adulthoodBacker = PopulateHoodsBackers(workingDir, "Adulthood");
                childhoodBacker = PopulateHoodsBackers(workingDir, "Childhood");

                TurnBackStoriesIntoTables(adulthoodBacker, workingDir + Path.DirectorySeparatorChar + "adulthoodbacker");
                TurnBackStoriesIntoTables(childhoodBacker, workingDir + Path.DirectorySeparatorChar + "childhoodbacker");

                Console.Read();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.Read();
            }
        }

        private static List<XElement> PopulateHoods(string workingDir, List<XElement> hoods, string filter)
        {
            foreach (string file in Directory.GetFiles(workingDir, filter))
            {
                Console.WriteLine("found " + file);
                hoods.AddRange(XElement.Load(file).Elements());
            }
            return hoods;
        }

        private static List<XElement> PopulateHoodsBackers(string workingDir, string backstoryType)
            => XElement.Load(workingDir + Path.DirectorySeparatorChar + "backstories.xml").Descendants("PawnBio").Descendants(backstoryType).Concat(
               XElement.Load(workingDir + Path.DirectorySeparatorChar + "TynanCustom.xml").Descendants("PawnBio").Descendants(backstoryType))
                .ToList();

        private static void TurnBackStoriesIntoTables(IEnumerable<XElement> item, string saveLocation)
        {
            List<string> backstories = new List<string>();

            foreach (XElement backstory in item)
            {
                backstories.Add(ParseBackstory(backstory));
            }
            backstories.Sort();

            Console.WriteLine(backstories.Count() + " backstories for " + saveLocation + ".txt");

            StringBuilder sb = new StringBuilder();
            sb.Append(CreateHeader());

            foreach (string backstory in backstories)
            {
                sb.Append(backstory);
            }

            sb.Append(CreateFooter());

            File.WriteAllText(saveLocation + ".txt", sb.ToString());
        }

        private static string CreateHeader()
            => "<noinclude>&larr; <small>[[Backstories]]</small></noinclude>" + Environment.NewLine +
            "{| {{STDT| sortable c_17}}" + Environment.NewLine +
            "!Title <br/> (Short Title) !!Description !!Skill Modifications !!Incapable of !!Civil !!Raider !!Slave !!Trader !!Traveler !!Tribal" + Environment.NewLine +
            "|-" + Environment.NewLine;

        private static string CreateFooter()
            => "|}";

        private static string ParseBackstory(XElement xElement)
        {
            string workDisables = WorkDisables(xElement);

            return "! " + Title(xElement) + "{{br}}" + "(" + TitleShort(xElement) + ")" + Environment.NewLine +
                   "| " + BaseDesc(xElement) + Environment.NewLine +
                   "| " + SkillGains(xElement) + Environment.NewLine +
                   "| " + (string.IsNullOrEmpty(workDisables) ? "None" : WorkDisables(xElement)) + Environment.NewLine +
                   "| " + SpawnCategories(xElement) + Environment.NewLine +
                   "|-" + Environment.NewLine;
        }

        private static string SpawnCategories(XElement xElement)
        {
            IEnumerable<XElement> spwn = xElement.Descendants("SpawnCategories");
            return "{{" + spwn.CheckOrCoss("Civil") + "}} || {{" + spwn.CheckOrCoss("Raider") + "}} || {{" + spwn.CheckOrCoss("Slave") + "}} " +
                    "|| {{" + spwn.CheckOrCoss("Trader") + "}} || {{" + spwn.CheckOrCoss("Traveler") + "}} || {{" + spwn.CheckOrCoss("Tribal") + "}}";
        }

        private static string WorkDisables(XElement xElement)
        {
            string result = "";
            IEnumerable<XElement> str = xElement.Descendants("WorkDisables");
            foreach (XElement item in str.Descendants())
            {
                result += item.Value + "{{br}}";
            }
            return result;
        }

        private static string SkillGains(XElement xElement)
        {
            StringBuilder sb = new StringBuilder();
            foreach (XElement item in (xElement.Descendants("SkillGains").Concat(xElement.Descendants("skillGains"))).Descendants())
            {
                sb.Append(parseKeyValuePair(item));
            }

            string parseKeyValuePair(XElement li)
            {
                if (li.Descendants("key")?.FirstOrDefault()?.Value == null)
                    return "";
                if (li.Descendants("value")?.FirstOrDefault()?.Value == null)
                    return "";

                return li.Descendants("key").FirstOrDefault().Value.Trim() + ": {{" + addPlusOrMinus(li.Descendants("value").FirstOrDefault().Value.Trim()) + "}}</br>";
            }

            string addPlusOrMinus(string valu)
            {
                string result = "";
                if (valu.Contains('-'))
                {
                    result += "--";
                }
                else
                {
                    result += "+";
                }

                return result += "|" + new string(valu.Where(x => char.IsDigit(x)).ToArray());
            }

            return sb.ToString().Substring(0, sb.Length - 5);
        }

        private static string BaseDesc(XElement xElement)
            => xElement.Element("BaseDesc")?.Value?.Replace(@"\n\n", "{{br}}").Replace(Environment.NewLine, "{{br}}")
            ?? xElement.Element("baseDesc")?.Value?.Replace(@"\n\n", "{{br}}");

        private static string TitleShort(XElement xElement)
            => xElement.Element("titleShort")?.Value?.CapitaliseFirst()
            ?? xElement.Element("TitleShort")?.Value?.CapitaliseFirst();

        private static string Title(XElement xElement)
            => xElement.Element("Title")?.Value?.CapitaliseFirst()
            ?? xElement.Element("title")?.Value?.CapitaliseFirst();
    }
}
