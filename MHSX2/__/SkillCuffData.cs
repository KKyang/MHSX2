using System;
using System.Collections.Generic;
using System.Text;

namespace MHSX2
{
    public enum SkillCuffSeriesType
    {
        P = 0,
        S
    }


    public class SkillCuffData : JewelryData_base
    {
        public SkillCuffSeriesType SeriesType;


        public SkillCuffData()
        {
            Type = JewelryType.SkillCuff;
        }

        public override string ToString()
        {
            return Name + GetSlotString();
        }

        #region JewelryInterface メンバ


        public override string GetSlotString()
        {
            string ret = "";

            string Symbol = "";
            switch (SeriesType)
            {
                case SkillCuffSeriesType.P:
                    Symbol = "★";
                    break;
                case SkillCuffSeriesType.S:
                    Symbol = "◆";
                    break;
            }

            for (int i = 0; i < Slot; i++)
            {
                ret += Symbol;
            }

            return ret;
        }


        public override string GetSkillString()
        {
            string SkillString = "";
            foreach (SkillPoint sp in SkillList)
            {
                if (SkillString.Length == 0)
                {
                    SkillString = sp.ToString();
                }
                else
                {

                    SkillString += ", " + sp.ToString();
                }
            }

            return SkillString;
        }

        #endregion
    }



}
