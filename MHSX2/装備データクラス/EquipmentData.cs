using System.Collections;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;

namespace MHSX2
{
    public abstract class EquipmentData_base
    {
        public int Slot
        {
            set
            {
                if (value < 0 || value > 3)
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

        public string Name = "";
        public bool isIgnored = false;
        public abstract string GetSlotString();
        public string Class;
        public string Type = "";

    }


    public class EquipmentData : EquipmentData_base
    {
        public EquipKind Kind;
        public SexType WearableSex;
        public JobType WearableJob;
        public int Rare
        {
            set
            {
                if (value < 1)
                {
                    MessageBox.Show("レア度は1以上である必要があります。 " + Name);
                    Environment.Exit(-1);
                }
                else
                    _Rare = value;

              

            }

            get
            {

                return _Rare;
            }
        }
        private int _Rare;



        public int Level;
        public int Def;
        public int Score;//評価値
        public Elemental Element = new Elemental();
        public SkillPoint[] SkillPointList;//SkillPoint配列
        public bool isSP;
        public List<Level> LevelList = new List<Level>();

        public DerivationSource DerivSource;
        public List<KeyValuePair<int, EquipmentData>> Derivation = new List<KeyValuePair<int, EquipmentData>>();
        public string Pict_M = "";
        public string Pict_W = "";
        public int RustaLv = 0;

        private string SkillString = null;


        public override string GetSlotString()
        {
            string SlotString = "";

            char slotchar;
            if (isSP)
                slotchar = '□';
            else
                slotchar = '○';

            for (int i = 0; i < Slot; i++)
                SlotString += slotchar;

            return SlotString;
        }

        public string GetSkillString()
        {
            if (SkillString == null)
            {
                SkillString = "";
                foreach (SkillPoint sp in SkillPointList)
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
            }


            return SkillString;
        }

        public override string ToString()
        {
            return Name;
        }


        public void GetNecessaryItemList(BaseData basedata, int level, Dictionary<Item, int> map, ref int Mony, List<EquipmentData> Checked)
        {
            for (int i = 0; i < level/* && i < data.LevelList.Count*/; i++)
            {
                Mony += LevelList[i].Money;

                foreach (KeyValuePair<Item, int> pair in LevelList[i].CostItems)
                {
                    if (map.ContainsKey(pair.Key))
                    {
                        map[pair.Key] += pair.Value;
                    }
                    else
                    {
                        map.Add(pair.Key, pair.Value);
                    }
                }


            }

            Checked.Add(this);

            if (DerivSource != null)
            {
                if (!Checked.Contains(basedata.EquipDataMap[(int)Kind][DerivSource.Source]))
                {
                    basedata.EquipDataMap[(int)Kind][DerivSource.Source].GetNecessaryItemList(basedata, DerivSource.level, map, ref Mony, Checked);
                }
            }
        }


    }
}
