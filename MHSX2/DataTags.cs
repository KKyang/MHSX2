using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace MHSX2
{
    public class SkillPointTag
    {
        public int index;
        public int Point;//求められるスキルの正の方向へのポイント
        public SkillBase sb;
        public override string ToString()
        {
            if (sb != null)
            {
                return sb.ToString() + Point.ToString();
            }
            return base.ToString();
        }
    }

    public class PlusJewelryDataTag : IComparable<PlusJewelryDataTag>
    {
        public JewelryDataTag jdt;
        public SkillPointTag SpecificPoint = null;
        public double Efficiency = 0;

        public PlusJewelryDataTag()
        {
        }

        public PlusJewelryDataTag(JewelryDataTag jdt, SkillPointTag pt)
        {
            this.jdt = jdt;
            SpecificPoint = pt;

            Efficiency = pt.Point / jdt.jd.Slot;

        }

        public override string ToString()
        {
            return jdt.jd.Name + " 効率" + Efficiency + " " + SpecificPoint.ToString() + " Slot" + Slot;
        }

        public int Slot
        {
            get
            {
                return jdt.jd.Slot;
            }
        }

        #region IComparable<SpecificJewelryDataTag> メンバ

        public int CompareTo(PlusJewelryDataTag other)
        {
            double diff;
            if (this.SpecificPoint.Point > 0)
            {
                if (other.SpecificPoint.Point > 0)
                {
                    diff = other.Efficiency - this.Efficiency;

                    if (diff != 0)
                        return diff > 0?1:-1;
                    else
                        return this.Slot - other.Slot;
                }
                else
                    return -1;
            }
            else
            {
                if (other.SpecificPoint.Point < 0)
                {
                    diff = this.Efficiency - other.Efficiency;

                    if (diff != 0)
                        return diff > 0 ? 1 : -1;
                    else
                        return this.Slot - other.Slot;

                }
                else
                    return 1;
            }

        }

        #endregion
    }


    public class SpecificJewelryDataTag : JewelryDataTag
    {
        public SkillPointTag SpecificPoint = new SkillPointTag();
    }

    public class JewelryDataTag
    {
        public JewelryData_base jd;
        public SkillPointTag[] SkillPointTags;

        public double GetEfficiency(int index)
        {
            foreach (SkillPointTag spt in SkillPointTags)
            {
                if (spt.index == index)
                {
                    return (double)spt.Point / jd.Slot;
                }
            }

            throw new Exception("装飾品エラー");
        }

        public SkillPointTag SearchSkill(int index)
        {
            foreach (SkillPointTag spt in SkillPointTags)
            {
                if (spt.index == index)
                {
                    return spt;
                }
            }

            return null;
        }

        public int? GetSkillPoint(SkillBase sb)//sbのポイントを持っていれば返す。なければnull
        {
            foreach (SkillPointTag spt in SkillPointTags)
            {
                if (spt.sb == sb)
                {
                    return spt.Point;
                }
            }

            return null;
        }

        public override string ToString()
        {
            if (jd != null)
            {
                string ret = jd.Name;


                foreach (SkillPointTag spt in SkillPointTags)
                    ret += " " + spt.ToString();

                ret += " " + jd.GetSlotString();

                return ret;
            }
            else
                return base.ToString();
        }

    }


    //public class JewelryDataTagSorter : IComparer
    //{
    //    private int target;
    //    public JewelryDataTagSorter(int index)
    //    {
    //        target = index;
    //    }

    //    #region IComparer メンバ

    //    public int Compare(object x, object y)
    //    {
    //        JewelryDataTag a = (JewelryDataTag)x, b = (JewelryDataTag)y;

    //        double va = a.GetEfficiency(target), vb = b.GetEfficiency(target);


    //        if (vb - va != 0)
    //        {
    //            if (vb - va > 0)
    //                return 1;
    //            else if (vb - va < 0)
    //                return -1;
    //            else
    //                return 0;
    //        }
    //        else
    //        {
    //            JewelryDataTagInfo info_a = a.GetInfo(target);
    //            JewelryDataTagInfo info_b = b.GetInfo(target);

    //            if (info_a == JewelryDataTagInfo.Flat || info_b == JewelryDataTagInfo.Flat)
    //                return a.jd.NecessarySlot - b.jd.NecessarySlot;

    //            switch (info_a)
    //            {
    //                case JewelryDataTagInfo.Plus:
    //                    switch (info_b)
    //                    {
    //                        case JewelryDataTagInfo.Plus:
    //                            return a.jd.NecessarySlot - b.jd.NecessarySlot;
    //                        case JewelryDataTagInfo.Solo:
    //                        case JewelryDataTagInfo.Minus:
    //                            return -1;
    //                    }
    //                    break;
    //                case JewelryDataTagInfo.Solo:
    //                    switch (info_b)
    //                    {
    //                        case JewelryDataTagInfo.Plus:
    //                            return 1;
    //                        case JewelryDataTagInfo.Solo:
    //                            return a.jd.NecessarySlot - b.jd.NecessarySlot;
    //                        case JewelryDataTagInfo.Minus:
    //                            return -1;
    //                    }
    //                    break;
    //                case JewelryDataTagInfo.Minus:
    //                    switch (info_b)
    //                    {
    //                        case JewelryDataTagInfo.Plus:
    //                        case JewelryDataTagInfo.Solo:
    //                            return 1;
    //                        case JewelryDataTagInfo.Minus:
    //                            return a.jd.NecessarySlot - b.jd.NecessarySlot;
    //                    }
    //                    break;
    //            }

    //            return 0;
    //        }
    //    }
    //    #endregion


    //}

    public class EquipmentDataTag
    {
        private EquipmentData equipdata_;
        public EquipmentData equipdata
        {
            get { return equipdata_; }
            set
            {
                if (value != null)
                {
                    equipdata_ = value;
                    Slot = value.Slot;
                    Def = value.Def;
                    Level = value.Level;
                }
                else
                {
                    equipdata_ = null;
                    Slot = -1;
                    Def = -1;
                    Level = -1;
                }
            }
        }

        public EquipmentDataTag[] BackwardEquips = new EquipmentDataTag[0];
        public List<EquipmentDataTag> BackwardEquipsTmp = new List<EquipmentDataTag>();

        //public EquipmentDataTag Next;
        //public EquipmentDataTag Parent;


        public SkillPointTag[] SkillPointTags;
        public int Level;
        public int Slot;
        public int Def;
        public override string ToString()
        {
            if (equipdata != null)
            {
                string ret = equipdata.Name + " スロット" + Slot.ToString();


                foreach (SkillPointTag spt in SkillPointTags)
                    ret += " " + spt.ToString();

                ret += " 防御" + Def.ToString();


                return ret;
            }
            else
                return base.ToString();
        }
    }

    public class PlusJewelryListTag
    {
        public SkillBase sb;
        public int SkillIndex;
        public PlusJewelryDataTag[] Jewelrys;
        public bool[] SlotExists = new bool[3];

        public int Length
        {
            get { return Jewelrys.Length; }
        }

        public PlusJewelryListTag(SkillBase SB, int index, PlusJewelryDataTag[] list)
        {
            this.sb = SB;
            SkillIndex = index;
            Jewelrys = list;

            //スロットごとに存在を記憶。1つでもそのスロットの装備があればtrue
            foreach(PlusJewelryDataTag jdt in list)
            {
                SlotExists[jdt.Slot - 1] = true;

            }

        }

        //インデクサ
        public PlusJewelryDataTag this[int index]
        {
            get
            {
                return Jewelrys[index];
            }
            set
            {
                Jewelrys[index] = value;
            }
        }

        public override string ToString()
        {
            string str = sb.ToString();

            foreach (PlusJewelryDataTag jdt in Jewelrys)
            {
                str += " " + jdt.ToString();
            }

            return str;
        }
    }

}

