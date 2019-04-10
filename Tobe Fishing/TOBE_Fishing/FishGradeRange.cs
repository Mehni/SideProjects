using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Verse;

namespace TOBE_Fishing
{

    public class FishGradeRange : IEquatable<FishGradeRange>
    {
        public FishGrade min;
        public FishGrade max;

        public static FishGradeRange All => new FishGradeRange(FishGrade.F, FishGrade.S);

        public FishGradeRange()
        {
        }

        public FishGradeRange(FishGrade min, FishGrade max)
        {
            this.min = min;
            this.max = max;
        }

        public bool Includes(FishGrade p) => p >= this.min && p <= this.max;

        public static bool operator ==(FishGradeRange a, FishGradeRange b) => a.min == b.min && a.max == b.max;

        public static bool operator !=(FishGradeRange a, FishGradeRange b) => !(a == b);

        public override string ToString() => this.min.ToString() + "~" + this.max.ToString();

        public static FishGradeRange FromString(string s)
        {
            string[] array = s.Split('~');
            if (char.TryParse(array[0], out char min) && char.TryParse(array[1], out char max))
            {
                return new FishGradeRange(CharToGrade(min), CharToGrade(max));
            }
            StringBuilder fg = new StringBuilder();
            foreach (object variable in Enum.GetValues(typeof(FishGrade)))
            {
                fg.AppendWithComma(variable.ToString());
            }
            throw new Exception($"Can't parse FishGradeRange from {s}. Acceptable values are: {fg}. Values found were min: {array[0]}, max: {array[1]}");
        }

        private static FishGrade CharToGrade(char c)
        {
            switch (c)
            {
                case 'F':
                    return FishGrade.F;
                case 'D':
                    return FishGrade.D;
                case 'C':
                    return FishGrade.C;
                case 'B':
                    return FishGrade.B;
                case 'A':
                    return FishGrade.A;
                case 'S':
                    return FishGrade.S;
                default:
                    throw new ArgumentException($"FishGrade not stored as F, D, C, B, A or S but as {c}. Case-sensitive!");
            }
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            this.min = FishGradeRange.FromString(xmlRoot.FirstChild.Value).min;
            this.max = FishGradeRange.FromString(xmlRoot.FirstChild.Value).max;
        }

        public override int GetHashCode() => Verse.Gen.HashCombineStruct<FishGrade>(this.min.GetHashCode(), this.max);

        public override bool Equals(object obj)
        {
            if (!(obj is FishGradeRange))
            {
                return false;
            }
            FishGradeRange fishGradeRange = (FishGradeRange)obj;
            return fishGradeRange.min == this.min && fishGradeRange.max == this.max;
        }

        public bool Equals(FishGradeRange other) => other.min == this.min && other.max == this.max;
    }
}
