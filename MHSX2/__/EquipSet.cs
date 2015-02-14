using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace MHSX2
{
    public class EquipSet : ICloneable
    {
        public Equipment[] Equips = new Equipment[(int)EquipKind.NumOfEquipKind];

        public int TotalSPSlotNum;
        public int TotalFilledSPSlotNum;
        public int TotalRestSPSlotNum;

        public int TotalSlotNum;
        public int TotalFilledSlotNum;
        public int TotalRestSlotNum;
        public int TotalDef;
        public int TotalGettableHR;
        public int ActiveSkillNum;
        public Elemental TotalElement = new Elemental();
        public PigClothes PigClothes = new PigClothes();


        public int No;//検索の時に見つかった順番

        public EquipSet()
        {
            for (int i = 0; i < 6; i++)
                Equips[i] = new Equipment();
        }


        //インデクサ
        public Equipment this[EquipKind index]
        {
            get
            {
                if (index < EquipKind.NumOfEquipKind)
                    return Equips[(int)index];
                else
                    throw new Exception("配列外アクセス");
            }
            set
            {
                if (index < EquipKind.NumOfEquipKind)
                    Equips[(int)index] = value;
                else
                    throw new Exception("配列外アクセス");
            }
        }


        public void UpdateTotalDef()
        {
            TotalDef = 0;
            foreach (Equipment e in Equips)
            {
                if (e.isChecked)
                {
                    if (e.EquipData.Kind != EquipKind.Weapon)
                    {
                        TotalDef += e.Def;
                    }
                }
            }
        }

        public void UpdateData()
        {
            UpdateTotalDef();

            TotalSPSlotNum = 0;
            TotalFilledSPSlotNum = 0;

            TotalSlotNum = 0;
            TotalFilledSlotNum = 0;
            TotalElement.Clear();
            TotalGettableHR = 0;

            foreach (Equipment e in Equips)
            {
                if (e.EquipData != null && e.isChecked)
                {
                    if (e.EquipData.Kind != EquipKind.Weapon)
                    {
                        TotalElement += e.EquipData.Element;
                    }

                    if (e.EquipData.isSP)
                    {
                        TotalSPSlotNum += e.Slot;
                        TotalFilledSPSlotNum += e.GetFilledSlotNum();
                    }
                    else
                    {
                        TotalSlotNum += e.Slot;
                        TotalFilledSlotNum += e.GetFilledSlotNum();
                    }

                    if (0 < e.Level && e.Level <= e.EquipData.LevelList.Count + 1)
                    {
                        int hr = e.EquipData.LevelList[e.Level - 1].GetableHR;
                        if (TotalGettableHR < hr)
                            TotalGettableHR = hr;
                    }
                }
            }

            TotalRestSlotNum = TotalSlotNum - TotalFilledSlotNum;
            TotalRestSPSlotNum = TotalSPSlotNum - TotalFilledSPSlotNum;


            List<SkillPoint> points = GetSkillPointArrayList();

            ActiveSkillNum = 0;
            foreach (SkillPoint sp in points)
            {
                SkillOption op = sp.SBase.GetOption(sp.Point);
                if (op != null)
                {
                    ActiveSkillNum++;
                }
            }

        }

        public Dictionary<SkillBase, SkillPoint> GetInvokeSKillHashTable()
        {
            List<SkillPoint> array = GetSkillPointArrayList();

            Dictionary<SkillBase, SkillPoint> ret = new Dictionary<SkillBase, SkillPoint>();
            foreach (SkillPoint sp in array)
            {
                SkillOption so = sp.SBase.GetOption(sp.Point);

                if (so != null)
                    ret[so.SBase] = sp;
            }


            return ret;
        }


        public List<SkillPoint> GetSkillPointArrayList()
        {
            List<SkillPoint> sptable = new List<SkillPoint>();
            foreach (Equipment equip in Equips)
            {
                if (equip.isChecked && equip.EquipData != null)//装備があったら
                {
                    foreach (SkillPoint sp in equip.EquipData.SkillPointList)
                    {
                        bool find = false;
                        foreach (SkillPoint sp2 in sptable)
                        {
                            if (sp2.SBase == sp.SBase)
                            {
                                sp2.Point += sp.Point;
                                find = true;
                                break;
                            }
                        }

                        if (find == false)
                        {
                            SkillPoint sp2 = new SkillPoint();
                            sp2.SBase = sp.SBase;
                            sp2.Point = sp.Point;
                            sptable.Add(sp2);
                        }

                    }

                    List<JewelryData_base> list = equip.GetFixedJewelys();

                    foreach (JewelryData j in list)
                    {
                        foreach (SkillPoint sp in j.SkillList)
                        {
                            bool find = false;
                            foreach (SkillPoint sp2 in sptable)
                            {
                                if (sp2.SBase == sp.SBase)
                                {
                                    sp2.Point += sp.Point;
                                    find = true;
                                    break;
                                }
                            }

                            if (find == false)
                            {
                                SkillPoint sp2 = new SkillPoint();
                                sp2.SBase = sp.SBase;
                                sp2.Point = sp.Point;
                                sptable.Add(sp2);
                            }
                        }
                    }

                }
            }


            if (PigClothes.Clothes != null && PigClothes.isChecked)
            {
                foreach (SkillCuffData scd in PigClothes.SkillCuffs)
                {
                    if (scd == null)
                        continue;

                    foreach (SkillPoint sp in scd.SkillList)
                    {
                        bool find = false;

                        foreach (SkillPoint tmp in sptable)
                        {
                            if (sp.SBase == tmp.SBase)
                            {
                                tmp.Point += sp.Point;
                                find = true;
                                break;
                            }
                        }


                        if (!find)
                        {
                            sptable.Add(new SkillPoint(sp.SBase, sp.Point));
                        }

                    }
                }

            }



            return sptable;
        }

        public string GetSlotString()
        {
            return "■" + TotalFilledSPSlotNum.ToString() + "□" + TotalRestSPSlotNum.ToString() +
                "●" + TotalFilledSlotNum.ToString() + "○" + TotalRestSlotNum.ToString();
        }

        public Dictionary<Item, int> GetNecessaryItemList(BaseData basedata, ref int money)
        {
            Dictionary<Item, int> dict = new Dictionary<Item, int>();

            money = 0;

            for (int i = 0; i < (int)EquipKind.NumOfEquipKind; i++)
            {
                Equipment equip = Equips[i];

                if (equip.EquipData == null)
                    continue;

                if (equip.isChecked == false)
                    continue;

                List<EquipmentData> list = new List<EquipmentData>();

                equip.EquipData.GetNecessaryItemList(basedata, equip.Level, dict, ref money, list);

                List<JewelryData_base> jdlist = equip.GetFixedJewelys();

                foreach (JewelryData_base jd in jdlist)
                {
                    money += jd.CostMoney;

                    foreach (KeyValuePair<Item, int> pair in jd.CostItem)
                    {

                        if (dict.ContainsKey(pair.Key))
                        {
                            dict[pair.Key] += pair.Value;
                        }
                        else
                            dict.Add(pair.Key, pair.Value);
                    }
                }
            }

            if (PigClothes != null)
            {
                if (PigClothes.isChecked)
                {
                    if (PigClothes.Clothes != null)
                    {
                        foreach (SkillCuffData sc in PigClothes.GetFixedJewelys())
                        {
                            money += sc.CostMoney;

                            foreach (Item item in sc.CostItem.Keys)
                            {
                                if (dict.ContainsKey(item))
                                {
                                    dict[item] += sc.CostItem[item];
                                }
                                else
                                    dict.Add(item, sc.CostItem[item]);
                            }
                        }
                    }
                }
            }


            return dict;

        }


        private int CacheItemCount = 0;
        private string CacheItemText = "";
        public int CountItemByText(string txt, BaseData basedata)
        {
            int ret = 0;
            int x = 0;

            if (CacheItemText == txt)
                return CacheItemCount;



            Dictionary<Item, int> list = GetNecessaryItemList(basedata, ref x);

            foreach (KeyValuePair<Item, int> pair in list)
            {
                if (pair.Key.name.Contains(txt))
                {
                    ret += pair.Value;
                }
            }

            CacheItemText = txt;
            CacheItemCount = ret;

            return ret;
        }

        private int CacheJewerlyCount = 0;
        private string CacheJewerlyText = "";

        public int CountJewelryByText(string txt)
        {

            if (CacheJewerlyText == txt)
                return CacheJewerlyCount;

            int ret = 0;
            foreach (Equipment equip in Equips)
            {
                if (equip == null)
                    continue;

                foreach (JewelryData jd in equip.jewelrys)
                {
                    if (jd == null)
                        continue;

                    if (jd.Name.Contains(txt))
                        ret++;
                }

            }

            if (PigClothes.Clothes != null)
            {
                foreach (SkillCuffData sc in PigClothes.SkillCuffs)
                {
                    if (sc == null)
                        continue;

                    if (sc.Name.Contains(txt))
                        ret++;
                }

            }


            CacheJewerlyText = txt;
            CacheJewerlyCount = ret;

            return ret;
        }

        private int CacheEquipCount = 0;
        private string CacheEquipText = "";
        public int CountEquipByText(string txt)
        {

            if (CacheEquipText == txt)
                return CacheEquipCount;

            int ret = 0;
            foreach (Equipment equip in Equips)
            {
                if (equip == null || equip.EquipData == null)
                    continue;

                if (equip.EquipData.Name.Contains(txt))
                    ret++;
            }

            if (PigClothes.Clothes != null)
            {
                if (PigClothes.Clothes.Name.Contains(txt))
                    ret++;
            }


            CacheEquipText = txt;
            CacheEquipCount = ret;


            return ret;
        }

        private int CacheClassCount = 0;
        private string CacheClassText = "";
        public int CountEquipByClass(string txt)
        {
            if (CacheClassText == txt)
            {
                return CacheClassCount;
            }

            int ret = 0;
            foreach (Equipment equip in Equips)
            {
                if (equip == null || equip.EquipData == null)
                    continue;

                if (equip.EquipData.Class != null && equip.EquipData.Class.Contains(txt))
                    ret++;
            }

            CacheClassText = txt;
            CacheClassCount = ret;

            return ret;

        }


        #region ICloneable メンバ

        public object Clone()
        {
            EquipSet ret = new EquipSet();
            for (int i = 0; i < 6; i++)
                ret.Equips[i] = (Equipment)this.Equips[i].Clone();

            ret.PigClothes = (PigClothes)PigClothes.Clone();

            return ret;
        }

        #endregion
    }
}
