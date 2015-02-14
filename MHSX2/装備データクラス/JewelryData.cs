using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace MHSX2
{

    public enum JewelryType
    {
        Normal,
        SP,
        SkillCuff
    }

    public abstract class JewelryData_base
    {
        public string Name;
        public bool isIgnored;
        public int CostMoney;
        public int GetableHR;
        public string Class;

        public int Slot
        {
            set
            {
                if (value <= 0 || value > 3)
                {
                    MessageBox.Show("スロットの値は0から3でなければなりません。 " + Name);
                    Environment.Exit(-1);
                }
                else
                    _Slot = value;
            }
            get
            {
                return _Slot;
            }

        }
        private int _Slot;

        public int Rare
        {
            set
            {
                if (value < 1 || value > 7)
                {
                    MessageBox.Show("レア度は1～7である必要があります。 " + Name);
                }
                else
                    _Rare = value;
            }

            get
            {
                return _Rare;
            }
        }

        public String GetableHRString
        {
            get
            {
                return BaseData.mDefine.GetRankKindString(GetableHR) + (GetableHR % 1000).ToString();
            }
        }


        private int _Rare;


        public Dictionary<Item, int> CostItem = new Dictionary<Item, int>();//[Item]->int
        public JewelryType Type;

        public SkillPoint[] SkillList;//SKillPointArray

        public abstract string GetSlotString();
        public abstract string GetSkillString();
    }


    public class JewelryData : JewelryData_base
    {

        public JobType Job;

        public override string GetSlotString()
        {
            string SlotString = "";

            char slotchar;
            if (Type == JewelryType.SP)
                slotchar = '■';
            else
                slotchar = '●';

            for (int i = 0; i < Slot; i++)
                SlotString += slotchar;

            return SlotString;
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

        public override string ToString()
        {
            return Name + " " + GetSkillString();
        }


        public string GetName()
        {
            return Name;
        }

    }


}
