using System;
using System.Collections.Generic;
using System.Text;

namespace MHSX2
{

    public class ClothesData : EquipmentData_base
    {
        public SkillCuffSeriesType SetableCuffSeriesType;

        public override string GetSlotString()
        {
            string mark;
            if (SetableCuffSeriesType == SkillCuffSeriesType.P)
                mark = "☆";
            else
                mark = "◇";

            string ret = "";
            for (int i = 0; i < Slot; i++)
            {
                ret += mark;
            }

            return ret;
        }
    }


    public class PigClothes : Equipment_base,ICloneable
    {
        public ClothesData Clothes = null;
        public SkillCuffData[] SkillCuffs = new SkillCuffData[3];
        public bool isChecked = true;


        public override string GetSlotString()
        {
            if (Clothes == null)
                return "";


            string SlotString = "";


            int UseCount = 0;
            foreach (SkillCuffData data in SkillCuffs)
            {
                if (data != null)
                    UseCount += data.Slot;
            }

            string mark;

            if (Clothes.SetableCuffSeriesType == SkillCuffSeriesType.P)
                mark = "★";
            else
                mark = "◆";

            for (int i = 0; i < UseCount && i < Clothes.Slot; i++)
            {
                SlotString += mark;
            }


            if (Clothes.SetableCuffSeriesType == SkillCuffSeriesType.P)
                mark = "☆";
            else
                mark = "◇";

            for (int i = UseCount; i < Clothes.Slot; i++)
            {
                SlotString += mark;
            }


            return SlotString;
        }

        public override int GetRestSlotNum()
        {
            if (Clothes == null)
                return 0;

            return Clothes.Slot - GetFilledSlotNum();
        }


        public override int GetFilledSlotNum()
        {
            if (Clothes == null)
                return 0;

            int ret = 0;

            List<JewelryData_base> list = GetFixedJewelys();

            foreach (JewelryData_base jd in list)
            {
                ret += jd.Slot;
            }

            return ret;
        }


        public override List<JewelryData_base> GetFixedJewelys()
        {
            int count = 0;

            List<JewelryData_base> list = new List<JewelryData_base>();

            foreach (SkillCuffData sc in SkillCuffs)
            {
                if (sc == null)
                    continue;

                if (count + sc.Slot <= Clothes.Slot)
                {
                    list.Add(sc);
                    count += sc.Slot;
                }
            }

            return list;
        }


        public override string GetJewelryString()
        {
            if (Clothes == null)
                return "";

            string JewelryString = "";


            List<JewelryData_base> list = GetFixedJewelys();

            bool flag = false;
            foreach (JewelryData_base j in list)
            {
                if (flag)
                {
                    JewelryString += ", ";
                }
                else
                    flag = true;

                JewelryString += j.Name;

            }

            return JewelryString;



        }

        public override void SetJewelry(JewelryData_base jd)
        {
            if (!(jd is SkillCuffData))
                return;

            SkillCuffData sc = (SkillCuffData)jd;


            if (sc.SeriesType == SkillCuffSeriesType.P && Clothes.SetableCuffSeriesType != SkillCuffSeriesType.P)
            {
                return;
            }

            if (GetRestSlotNum() < sc.Slot)
                return;

            for (int i = 0; i < SkillCuffs.Length; i++)
            {
                if (SkillCuffs[i] != null)
                    continue;

                SkillCuffs[i] = sc;
                break;
            }



        }


        #region ICloneable メンバ

        public object Clone()
        {
            PigClothes ret = new PigClothes();


            ret.Clothes = Clothes;
            ret.SkillCuffs = (SkillCuffData[])SkillCuffs.Clone();
            ret.isChecked = isChecked;

            return ret;
        }

        #endregion
    }

}
