using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml.Serialization;



namespace MHSX2
{
    public class Elemental
    {
        private int[] Value = new int[(int)ElementType.NumOfElementKind];


        public void Clear()
        {
            for (int i = 0; i < (int)ElementType.NumOfElementKind; i++)
                Value[i] = 0;
        }

        public int this[ElementType type]
        {
            set
            {
                Value[(int)type] = value;
            }
            get
            {
                return Value[(int)type];
            }
        }

        // overload operator +
        public static Elemental operator +(Elemental a, Elemental b)
        {
            Elemental ret = new Elemental();
            for (int i = 0; i < (int)ElementType.NumOfElementKind; i++)
            {
                ret.Value[i] = a.Value[i] + b.Value[i];
            }

            return ret;
        }
    }

    public class SkillOption:IComparable<SkillOption>
    {
        public int Point;
        public string Name;
        public SkillBase SBase;
        override public string ToString()
        {
            return Name;
        }


        #region IComparable<SkillOption> メンバ

        public int CompareTo(SkillOption other)
        {
            return other.Point - this.Point;
        }

        #endregion
    }


    public class SkillBase : IComparable
    {
        public uint SkillCategory;
        public uint SkillId;
        public string Name;
        public List<SkillOption> OptionTable = new List<SkillOption>();

        //ポイントがpointのときのオプションを返す
        public SkillOption GetOption(int point)
        {
            int min = int.MaxValue;
            SkillOption ret = null;
            int sign = (point >= 0) ? 1 : -1;

            foreach (SkillOption sopt in OptionTable)
            {
                if (sign >= 0 && sopt.Point < 0)
                    continue;
                else if (sign < 0 && sopt.Point >= 0)
                    continue;

                int dif = sign * (point - sopt.Point);

                if (dif < min && dif >= 0)
                {
                    min = dif;
                    ret = sopt;
                }
            }


            return ret;
        }

        public override string ToString()
        {
            return Name;
        }


        #region IComparable メンバ

        public int CompareTo(object obj)
        {
            SkillBase a = (SkillBase)obj;
            return (int)SkillId - (int)a.SkillId;
        }

        #endregion
    }


    public class SkillPoint : IComparable<SkillPoint>, ICloneable
    {
        public SkillBase SBase;
        public int Point;


        public SkillPoint()
        {
        }

        public SkillPoint(SkillPoint sp)
        {
            SBase = sp.SBase;
            Point = sp.Point;
        }

        public SkillPoint(SkillBase sb, int p)
        {
            SBase = sb;
            Point = p;
        }

        public override string ToString()
        {
            if (SBase == null)
                return "";

            string str;

            if (Point >= 0)
                str = SBase.Name + "+" + Point.ToString();
            else
                str = SBase.Name + Point.ToString();


            return str;
        }

        public SkillOption GetOption()
        {
            if (SBase == null)
                return null;
            else
                return SBase.GetOption(Point);


        }


        #region IComparable<SkillPoint> メンバ

        public int CompareTo(SkillPoint other)
        {
            SkillOption so = GetOption();
            SkillOption so2 = other.GetOption();

            if (so == null && so2 != null)
                return 1;
            else if (so != null && so2 == null)
                return -1;

            int dif = other.Point - Point;

            if (dif != 0)
                return dif;
            {
                return SBase.CompareTo(other.SBase);
            }

        }

        #endregion

        #region ICloneable メンバ

        public object Clone()
        {
            return new SkillPoint(this.SBase, this.Point);
        }

        #endregion
    }

    [Serializable()]
    public class SkillPointCondition : SkillPoint
    {
        public bool isIgnore = false;//除外指定スキルのポイントならture
        public int? UpperPoint = null;

        public SkillPointCondition()
        {
        }

        public SkillPointCondition(SkillBase b, int p)
        {
            SBase = b;
            Point = p;
        }

        public override string ToString()
        {
            string str = base.ToString();

            if (isIgnore)
                str += " 除外指定";


            if (UpperPoint != null)
            {
                str += " 上限" + UpperPoint.ToString();
            }

            return str;
        }
    }


    public class SkillPriority : IComparable
    {
        public SkillPriority()
        {
        }

        public SkillPriority(SkillBase sb, int p, bool Checked)
        {
            SBase = sb;
            Priority = p;
            this.Checked = Checked;
        }

        public bool Checked;
        public SkillBase SBase;
        public int Priority;
        public override string ToString()
        {
            if (SBase != null)
                return SBase.ToString();
            return "";
        }

        #region IComparable メンバ

        public int CompareTo(object obj)
        {
            SkillPriority b = (SkillPriority)obj;
            if (SBase != null)
                return SBase.CompareTo(b.SBase);
            else
                return 0;
        }

        #endregion
    }

    public class Job
    {
        public JobType type;

        public Job()
        {
        }

        public Job(JobType j)
        {
            type = j;
        }

        static public JobType TypeFromString(string name)
        {
            if (name == "共")
                return JobType.COMON;
            else if (name == "劍士")
                return JobType.KNIGHT;
            else if (name == "槍手")
                return JobType.GUNNER;
            else
                throw new Exception("職種読み取りエラー");
        }

        public override string ToString()
        {
            switch (type)
            {
                case JobType.COMON:
                    return "共用のみ";
                case JobType.KNIGHT:
                    return "劍士";
                case JobType.GUNNER:
                    return "槍手";
                default:
                    return "";
            }
        }


    }


    public class Sex
    {
        public SexType type;
        public Sex(SexType s)
        {
            type = s;
        }

        static public SexType TypeFromString(string name)
        {
            if (name == "共")
                return SexType.COMON;
            else if (name == "男")
                return SexType.MAN;
            else if (name == "女")
                return SexType.WOMAN;
            else
                throw new Exception("性別読み取りエラー");
        }

        public override string ToString()
        {
            switch (type)
            {
                case SexType.COMON:
                    return "共用";
                case SexType.MAN:
                    return "男";
                case SexType.WOMAN:
                    return "女";
                default:
                    return "";
            }
        }
    }

    public class Level
    {
        public int Def;

        public int Slot
        {
            set
            {
                if (value < 0 || value > 3)
                {
                    throw new Exception("スロットは1～7である必要があります。 value =" + value.ToString());
                }
                else
                {
                    _Slot = value;
                }

            }
            get
            {
                return _Slot;
            }
        }

        private int _Slot;

        public int Money;
        public Dictionary<Item, int> CostItems = new Dictionary<Item, int>();
        public int GetableHR = -1;


        public String GetableHRString
        {
            get
            {
                return BaseData.mDefine.GetRankKindString(GetableHR) + (GetableHR % 1000).ToString();
            }
        }

    }

    public class Item
    {
        public string name;
        public int HR;//入手可能HR
#if BETA
        public string ID;
#endif
        public override string ToString()
        {
            return name;
        }

        public String GetableHRString
        {
            get
            {
                return BaseData.mDefine.GetRankKindString(HR) + (HR % 1000).ToString();
            }
        }

    }

    public class DerivationSource
    {
        public string Source;
        public int level;

        public override string ToString()
        {
            return Source + "Lv" + level;
        }
    }


    public class ItemClass
    {
        public String Name = "";
        public String Text = "";

        public override string ToString()
        {
            if (Text != "")
                return Name + " ： " + Text;
            else
                return Name;
        }
    }

    public class ParentItemClass:ItemClass
    {
        public List<ItemClass> ChildClass = new List<ItemClass>();
    }

}