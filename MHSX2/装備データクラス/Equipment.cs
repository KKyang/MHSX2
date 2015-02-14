using System;
using System.Collections.Generic;

using System.Text;

namespace MHSX2
{
    public abstract class Equipment_base
    {
        public abstract List<JewelryData_base> GetFixedJewelys();
        public abstract string GetSlotString();
        public abstract string GetJewelryString();
        public abstract int GetFilledSlotNum();
        public abstract int GetRestSlotNum();
        public abstract void SetJewelry(JewelryData_base jd);
    }


    public class Equipment : Equipment_base, ICloneable
    {
        public JewelryData[] jewelrys = new JewelryData[3];
        public bool isChecked = false;
        public int Level;
        public int Slot;
        public int Def;
        public EquipmentData equipdata_value = null;

        public Equipment(EquipmentData e)
        {
            EquipData = e;
        }

        public Equipment()
        {
        }

        public override List<JewelryData_base> GetFixedJewelys()
        {
            int count = 0;

            List<JewelryData_base> list = new List<JewelryData_base>();

            for (int i = 0; i < jewelrys.Length; i++)
            {
                if (jewelrys[i] != null)
                {
                    if (count + jewelrys[i].Slot <= Slot)
                    {
                        list.Add(jewelrys[i]);
                        count += jewelrys[i].Slot;
                    }
                }
            }

            return list;
        }

        public override string GetSlotString()
        {
            if (equipdata_value == null)
                return "";

            char fillchar;
            char restchar;
            if (equipdata_value.isSP)
            {
                fillchar = '■';
                restchar = '□';
            }
            else
            {
                fillchar = '●';
                restchar = '○';
            }



            string SlotString = "";


            List<JewelryData_base> list = GetFixedJewelys();


            int pushed = 0;
            foreach (JewelryData_base jd in list)
            {
                for (int k = 0; k < jd.Slot; k++)
                {
                    SlotString += fillchar;
                }
                pushed += jd.Slot;
            }

            while (pushed < Slot)
            {
                SlotString += restchar;
                pushed++;
            }



            return SlotString;
        }

        public override string GetJewelryString()
        {
            if (equipdata_value == null)
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

        public EquipmentData EquipData
        {
            get { return equipdata_value; }
            set
            {
                if (value != null)
                {
                    equipdata_value = value;
                    isChecked = true;
                    Level = value.Level;
                    Def = value.Def;
                    Slot = value.Slot;
                }
                else
                {
                    equipdata_value = null;
                    isChecked = false;
                    Level = -1;
                    Def = -1;
                    Slot = -1;
                }

                for (int i = 0; i < jewelrys.Length; i++)
                    jewelrys[i] = null;
            }
        }

        public override int GetFilledSlotNum()
        {
            if (equipdata_value == null)
                return 0;

            int ret = 0;

            List<JewelryData_base> list = GetFixedJewelys();

            foreach (JewelryData_base jd in list)
            {
                ret += jd.Slot;
            }

            return ret;
        }

        public override int GetRestSlotNum()
        {
            if (equipdata_value == null)
                return 0;

            return Slot - GetFilledSlotNum();
        }


        public override void SetJewelry(JewelryData_base jd)
        {
            if (equipdata_value == null)
                return;

            if (!(jd is JewelryData))
            {
                return;
            }

            //SPはSPにしかだめー
            if (equipdata_value.isSP != (jd.Type == JewelryType.SP))
                return;

            int rest = GetRestSlotNum();

            if (jd.Slot > rest)
                return;

            for (int i = 0; i < Slot && i < jewelrys.Length; i++)
            {
                if (jewelrys[i] == null)
                {
                    jewelrys[i] = (JewelryData)jd;
                    break;
                }
            }

        }

        #region ICloneable メンバ

        public object Clone()
        {
            Equipment ret = (Equipment)this.MemberwiseClone();

            if (jewelrys != null)
                ret.jewelrys = (JewelryData[])this.jewelrys.Clone();
            return ret;
        }

        #endregion

    }

}
