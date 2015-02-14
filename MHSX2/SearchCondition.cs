using System;
using System.Collections.Generic;

using System.Text;
using System.Collections;


namespace MHSX2
{
    public class SearchCondition
    {
        public EquipSet ESet;
        public Sex sex;
        public Job job;
        public int rare_lower;
        public int rare_upper;
        public int defence_lower;
        public int defence_upper;
        public int HR_Limit;
        public SortedList<SkillBase, SkillPointCondition> SkillPointConditionTable = new SortedList<SkillBase, SkillPointCondition>();//[SkillBase]->SkillPoint//
        public int SlotPriority = 0;
        public SkillSearchType skillSearchType;//1-or 0-and
        public Ignore ignore = new Ignore();
        public NumRangeOrder SP_assign = null;//SP防具指定数
        public int RustaLv = 0;
        public NumRangeOrder BlankEquipNum = null;
        public Dictionary<string, NumRangeOrder> TypeNumOrder = new Dictionary<string, NumRangeOrder>();
        public bool isOrderTypeNum = false;

        
        public SearchCondition MemberwiseClone_()
        {
            return (SearchCondition)MemberwiseClone();
        }

        public List<EquipmentData> MakeEquipArray(BaseData basedata, EquipKind type, bool Mode, bool isIgnoreItem, bool isIgnoreClass)//mode = 1なら実際に検索するときに使う。0なら表示のときにつかう
        {

            if (Mode)
            {
                if (ESet[type].EquipData != null && ESet[type].isChecked == true)
                {
                    return new List<EquipmentData>();
                }
            }



            List<EquipmentData> ret = new List<EquipmentData>();

            foreach (EquipmentData ed in basedata.EquipDataMap[(int)type].Values)
            {
                if (ed.Level < 0)//HR制限で作成不可
                    continue;

                if (Mode)
                {
                    if (!ignore.IgnoreJudge_Equip(ed, isIgnoreItem, isIgnoreClass,basedata))
                    {
                        continue;
                    }
                }


                //ラスタ開放レベル制限
                if (RustaLv > 0)
                {
                    if (ed.RustaLv == 0 || ed.RustaLv > RustaLv)
                        continue;
                }


                //性別違いは無視
                if (ed.WearableSex != sex.type && ed.WearableSex != SexType.COMON)
                    continue;

                if (job.type == JobType.COMON)
                {
                    if (ed.WearableJob != JobType.COMON)
                        continue;
                }
                else if (ed.WearableJob != job.type && ed.WearableJob != JobType.COMON)
                    continue;

                if (ed.Rare < rare_lower || ed.Rare > rare_upper)
                    continue;


                if (!Mode && type == EquipKind.Weapon)
                {//表示の場合は武器は常に出す
                    ret.Add(ed);
                    continue;
                }


                if (!Mode)//モードによって防御力制限の意味が違う
                {
                    if (ed.Kind != EquipKind.Weapon)
                    {
                        if (ed.Def < defence_lower || ed.Def > defence_upper)
                            continue;
                    }
                }

                if (ed.isSP == true)//SPはスキル関係無しに候補へ
                {
                    ret.Add(ed);
                    continue;
                }


                //実際の検索で、個数指定が有効な防具はスキルに関係なしに追加
                if (Mode == true && isOrderTypeNum && TypeNumOrder.ContainsKey(ed.Type))
                {
                    ret.Add(ed);
                    continue;
                }



                if (SkillPointConditionTable.Count != 0)
                {
                    if (skillSearchType == SkillSearchType.OR)
                    {
                        //or検索
                        bool find = false;


                        foreach (SkillPoint sp in ed.SkillPointList)
                        {
                            if (SkillPointConditionTable.ContainsKey(sp.SBase))
                            {
                                SkillPointCondition skillpoint = SkillPointConditionTable[sp.SBase];
                                if (skillpoint.isIgnore)
                                {
                                    if (!SubFunc.CompairSgin(sp.Point, skillpoint.Point))
                                    {
                                        find = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    if (skillpoint.UpperPoint != null || SubFunc.CompairSgin(sp.Point, skillpoint.Point))
                                    {
                                        find = true;
                                        break;
                                    }
                                }
                            }

                        }


                        if (find == false)
                            continue;
                    }
                    else
                    {
                        //and検索
                        bool add = true;

                        foreach (SkillPointCondition skillpoint in SkillPointConditionTable.Values)
                        {
                            bool find = false;
                            foreach (SkillPoint sp in ed.SkillPointList)
                            {
                                if (skillpoint.SBase == sp.SBase)
                                {
                                    if (skillpoint.isIgnore)
                                    {
                                        if (!SubFunc.CompairSgin(sp.Point, skillpoint.Point))
                                        {
                                            find = true;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (SubFunc.CompairSgin(sp.Point, skillpoint.Point))
                                        {
                                            find = true;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (find == false)
                            {
                                add = false;
                                break;
                            }
                        }

                        if (add == false)
                            continue;
                    }
                }

                ret.Add(ed);//襲い掛かるcontinueの雨をかわしはれて追加
            }

            return ret;
        }

        public List<JewelryData> MakeJewelryArray(BaseData basedata, bool isIgnoreItem, bool isIgnoreClass)
        {
            List<JewelryData> AddList = new List<JewelryData>();

            foreach (JewelryData jd in basedata.JewelryDataMap.Values)
            {
                if (!ignore.IgnoreJudge_juerly(jd, isIgnoreItem, isIgnoreClass))
                {
                    continue;
                }

                if (job.type == JobType.COMON)
                {
                    if (jd.Job != JobType.COMON)
                        continue;
                }
                else
                {
                    if (jd.Job != job.type && jd.Job != JobType.COMON)
                        continue;
                }

                if (jd.GetableHR > HR_Limit)
                    continue;


                if (skillSearchType == SkillSearchType.OR)
                {
                    //or検索

                    bool find = false;
                    foreach (SkillPoint sp in jd.SkillList)
                    {
                        if (SkillPointConditionTable.ContainsKey(sp.SBase))
                        {
                            SkillPointCondition skillpoint = SkillPointConditionTable[sp.SBase];

                            if (skillpoint.isIgnore)
                            {
                                if (!SubFunc.CompairSgin(sp.Point, skillpoint.Point))
                                {
                                    find = true;
                                    break;
                                }
                            }
                            else
                            {
                                if (skillpoint.UpperPoint != null || SubFunc.CompairSgin(sp.Point, skillpoint.Point))
                                {
                                    find = true;
                                    break;
                                }
                            }

                        }

                    }


                    if (find == false && SkillPointConditionTable.Count != 0)
                        continue;

                }
                else
                {
                    //and検索
                    bool add = true;

                    foreach (SkillPointCondition skillpoint in SkillPointConditionTable.Values)
                    {
                        bool find = false;
                        foreach (SkillPoint sp in jd.SkillList)
                        {
                            if (skillpoint.SBase == sp.SBase)
                            {
                                if (skillpoint.isIgnore)
                                {
                                    if (!SubFunc.CompairSgin(sp.Point, skillpoint.Point))
                                    {
                                        find = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    if (SubFunc.CompairSgin(sp.Point, skillpoint.Point))
                                    {
                                        find = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (find == false)
                        {
                            add = false;
                            break;
                        }
                    }

                    if (add == false)
                        continue;
                }


                AddList.Add(jd);
            }

            return AddList;
        }

        public List<SkillCuffData> MakeSkillCuffList(BaseData basedata, bool isIgnoreItem, bool isSearching/*実際に検索するときtrue*/, bool isIgnoreClass)
        {
            List<SkillCuffData> ret = new List<SkillCuffData>();


            if (isSearching && ESet.PigClothes.Clothes == null)
            {
                throw new Exception("意図されない呼び出し MakeSkillCuffList");
            }

            foreach (SkillCuffData data in basedata.SkillCaffDataMap.Values)
            {

                if (!ignore.IgnoreJudge_SkillCuff(data, isIgnoreItem, isIgnoreClass))
                {
                    continue;
                }



                if (isSearching)
                {
                    if (data.SeriesType == SkillCuffSeriesType.P && ESet.PigClothes.Clothes.SetableCuffSeriesType != SkillCuffSeriesType.P)
                    {
                        continue;
                    }
                }


                if (data.GetableHR > HR_Limit)
                    continue;


                if (skillSearchType == SkillSearchType.OR)
                {
                    //or検索

                    bool find = false;
                    foreach (SkillPoint sp in data.SkillList)
                    {
                        if (SkillPointConditionTable.ContainsKey(sp.SBase))
                        {
                            SkillPointCondition skillpoint = SkillPointConditionTable[sp.SBase];

                            if (skillpoint.isIgnore)
                            {
                                if (!SubFunc.CompairSgin(sp.Point, skillpoint.Point))
                                {
                                    find = true;
                                    break;
                                }
                            }
                            else
                            {
                                if (skillpoint.UpperPoint != null || SubFunc.CompairSgin(sp.Point, skillpoint.Point))
                                {
                                    find = true;
                                    break;
                                }
                            }

                        }

                    }


                    if (find == false && SkillPointConditionTable.Count != 0)
                        continue;

                }
                else
                {
                    //and検索
                    bool add = true;

                    foreach (SkillPointCondition skillpoint in SkillPointConditionTable.Values)
                    {
                        bool find = false;
                        foreach (SkillPoint sp in data.SkillList)
                        {
                            if (skillpoint.SBase == sp.SBase)
                            {
                                if (skillpoint.isIgnore)
                                {
                                    if (!SubFunc.CompairSgin(sp.Point, skillpoint.Point))
                                    {
                                        find = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    if (SubFunc.CompairSgin(sp.Point, skillpoint.Point))
                                    {
                                        find = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (find == false)
                        {
                            add = false;
                            break;
                        }
                    }

                    if (add == false)
                        continue;
                }


                ret.Add(data);
            }

            return ret;
        }


        public bool isIgnoreDef
        {
            get
            {
                switch (job.type)
                {
                    case JobType.KNIGHT:
                        if (defence_lower < Defines.IgnoreDefThreshold_Kight)
                            return true;

                        break;
                    case JobType.GUNNER:
                    case JobType.COMON:
                        if (defence_lower < Defines.IgnoreDefThreshold_Gunner)
                            return true;
                        break;
                }



                int sum = 0;

                foreach (SkillPointCondition spt in SkillPointConditionTable.Values)
                {
                    sum += System.Math.Abs(spt.Point);
                }

                if (sum < Defines.SkillNumThreshold)
                {
                    return false;
                }
                else
                {
                    return true;
                }

            }
        }
    }
}
