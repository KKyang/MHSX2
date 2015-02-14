using System;
using System.Windows.Forms;
using System.Collections.Generic;

using System.Text;
using System.Collections;

namespace MHSX2
{
    class EquipSetListView : ListView
    {
        public List<EquipSet> ShowItemList = new List<EquipSet>();//表示用
        public List<EquipSet> VirtualList = new List<EquipSet>();//オリジナルリスト

        public SelectedIndexCollection SelectedIndexs;
        public ListViewItemCollection ItemCollection;



        private int SortedColumn = 0;
        private SortOrder Order = SortOrder.Ascending;
        EquipSetSorter Sorter;


        public void Init(BaseData data)
        {
            Sorter = new EquipSetSorter(0, SortOrder.Ascending, data);
        }

        public void ClearItem()
        {
            Items.Clear();
            VirtualListSize = 0;
            ShowItemList.Clear();
            VirtualList.Clear();

        }

        protected override void OnCreateControl()
        {

            SelectedIndexs = new SelectedIndexCollection(this);
            ItemCollection = new ListViewItemCollection(this);
            base.OnCreateControl();
        }


        public void AddEquipSet(EquipSet ESet, bool flag/*trueでESet.Noへセット*/)
        {
            if (flag)
                ESet.No = VirtualListSize;

            VirtualListSize++;
            ESet.UpdateData();
            VirtualList.Add(ESet);
            ShowItemList.Add(ESet);

        }

        public void ResetFilter()
        {

            if (VirtualList.Count < 1)
                return;

            ShowItemList = new List<EquipSet>(VirtualList);

            VirtualListSize = ShowItemList.Count;

            ShowItemList.Sort(Sorter);
            RedrawItems(TopItem.Index, VirtualListSize - 1, true);

        }

        public List<EquipSet> SearchByEquipName(String name)
        {
            List<EquipSet> ret = new List<EquipSet>();


            foreach (EquipSet eset in ShowItemList)
            {
                foreach (Equipment e in eset.Equips)
                {
                    if (e.EquipData == null)
                        continue;

                    if (e.EquipData.Name.Contains(name))
                    {
                        ret.Add(eset);
                        goto LABEL_NEXT;
                    }
                }
                if (eset.PigClothes != null)
                    if (eset.PigClothes.Clothes != null)
                        if (eset.PigClothes.Clothes.Name.Contains(name))
                            ret.Add(eset);


            LABEL_NEXT: ;
            }


            return ret;
        }

        public List<EquipSet> SearchByJewelryName(String name)
        {
            List<EquipSet> ret = new List<EquipSet>();


            foreach (EquipSet eset in ShowItemList)
            {
                foreach (Equipment equip in eset.Equips)
                {
                    foreach (JewelryData jd in equip.jewelrys)
                    {
                        if (jd != null)
                            if (jd.Name.Contains(name))
                            {
                                ret.Add(eset);
                                goto LABEL_NEXT;
                            }
                    }
                }

                if (eset.PigClothes != null)
                {
                    foreach (SkillCuffData sc in eset.PigClothes.SkillCuffs)
                    {
                        if (sc != null)
                            if (sc.Name.Contains(name))
                            {
                                ret.Add(eset);

                                goto LABEL_NEXT;
                            }
                    }
                }

            LABEL_NEXT: ;

            }


            return ret;
        }

        public List<EquipSet> SearchByItemName(String name, BaseData basedata)
        {
            List<EquipSet> ret = new List<EquipSet>();


            foreach (EquipSet eset in ShowItemList)
            {
                int money = 0;
                Dictionary<Item, int> dict = eset.GetNecessaryItemList(basedata, ref money);

                foreach (Item item in dict.Keys)
                {
                    if (item.name.Contains(name))
                    {
                        ret.Add(eset);
                        break;
                    }
                }
            }


            return ret;
        }

        public List<EquipSet> SearchBySkillName(string name)
        {
            List<EquipSet> ret = new List<EquipSet>();


            foreach (EquipSet eset in ShowItemList)
            {
                Dictionary<SkillBase, SkillPoint> dict = eset.GetInvokeSKillHashTable();

                foreach (SkillPoint sp in dict.Values)
                {
                    if (sp.GetOption().Name.Contains(name))
                    {
                        ret.Add(eset);
                        break;
                    }
                }
            }


            return ret;
        }


        public void SortShowList(FilterDialog.IDList tgt, string txt, SortOrder order)
        {
            switch (tgt)
            {
                case FilterDialog.IDList.ByItem:
                    Sorter.Set_Text(txt, EquipSetSorter.SortType.BYITEM, order);
                    break;
                case FilterDialog.IDList.ByJewelry:
                    Sorter.Set_Text(txt, EquipSetSorter.SortType.BYJUELRY, order);
                    break;
                case FilterDialog.IDList.ByEquip:
                    Sorter.Set_Text(txt, EquipSetSorter.SortType.BYEQUIP, order);
                    break;
                case FilterDialog.IDList.ByClass:
                    Sorter.Set_Text(txt, EquipSetSorter.SortType.BYCLASS, order);
                    break;
            }

            if (ShowItemList.Count > 0)
            {
                ShowItemList.Sort(Sorter);
                RedrawItems(TopItem.Index, VirtualListSize - 1, true);
            }
        }

        public List<EquipSet> SearchByClassName(string name)
        {
            List<EquipSet> ret = new List<EquipSet>();


            foreach (EquipSet eset in ShowItemList)
            {
                foreach (Equipment equip in eset.Equips)
                {
                    if (equip.EquipData == null)
                        continue;

                    if (equip.EquipData.Class == null)
                        continue;

                    if (equip.EquipData.Class.Contains(name))
                    {
                        ret.Add(eset);
                        break;
                    }
                }
            }


            return ret;
        }


        public void RemoveFromShowList(List<EquipSet> list)
        {
            if (list == null)
                return;

            BeginUpdate();

            foreach (EquipSet e in list)
                ShowItemList.Remove(e);


            VirtualListSize = ShowItemList.Count;

            EndUpdate();
        }

        public void RemoveFromShowList_Selected()
        {
            BeginUpdate();

            List<EquipSet> list = new List<EquipSet>();

            ArrayList array = new ArrayList(SelectedIndices);

            array.Sort();


            for (int i = array.Count - 1; i >= 0; i--)
            {
                ShowItemList.RemoveAt((int)array[i]);
            }


            VirtualListSize = ShowItemList.Count;

            EndUpdate();
        }


        public void SetShowList(List<EquipSet> list)
        {
            if (list == null)
                return;

            BeginUpdate();

            ShowItemList = list;

            VirtualListSize = ShowItemList.Count;

            EndUpdate();
        }

        protected override void OnRetrieveVirtualItem(RetrieveVirtualItemEventArgs e)
        {
            if (e.ItemIndex < 0 || ShowItemList.Count <= e.ItemIndex)
            {
                e.Item = new ListViewItem();
            }
            else
            {
                EquipSet ESet = (EquipSet)ShowItemList[e.ItemIndex];

                ListViewItem item = new ListViewItem();
                item.Tag = ESet;
                item.Text = ESet.No.ToString();
                item.SubItems.Add(ESet.TotalDef.ToString());

                int hr =ESet.TotalGettableHR;

                item.SubItems.Add(BaseData.mDefine.GetRankKindString(hr) + (hr % 1000).ToString());

                item.SubItems.Add(ESet.TotalFilledSPSlotNum.ToString());
                item.SubItems.Add(ESet.TotalRestSPSlotNum.ToString());
                item.SubItems.Add(ESet.TotalFilledSlotNum.ToString());
                item.SubItems.Add(ESet.TotalRestSlotNum.ToString());
                item.SubItems.Add(ESet.ActiveSkillNum.ToString());
                Elemental ele = ESet.TotalElement;
                for (ElementType type = ElementType.Fire; type != ElementType.NumOfElementKind; type++)
                {
                    string str;
                    if (ele[type] > 0)
                        str = "+" + ele[type].ToString();
                    else
                        str = ele[type].ToString();
                    item.SubItems.Add(str);
                }

                e.Item = item;

            }

            base.OnRetrieveVirtualItem(e);


        }

        protected override void OnColumnClick(ColumnClickEventArgs e)
        {
            if (ShowItemList.Count < 1)
                return;

            if (SortedColumn != e.Column)
            {
                SortedColumn = e.Column;
                Order = SortOrder.Ascending;
            }
            else
            {
                if (Order == SortOrder.Ascending)
                    Order = SortOrder.Descending;
                else
                    Order = SortOrder.Ascending;
            }


            Sorter.Set(e.Column);

            ShowItemList.Sort(Sorter);

            RedrawItems(TopItem.Index, VirtualListSize - 1, true);

            base.OnColumnClick(e);
        }

        private class EquipSetSorter : IComparer<EquipSet>
        {
            List<Pair<int, SortOrder>> SortPriority = new List<Pair<int, SortOrder>>();
            string JewelryText;
            string ItemText;
            string EqiupText;
            string ClassText;
            BaseData basedata;

            public enum SortType { BYEQUIP = -10, BYITEM = -20, BYJUELRY = -30, BYCLASS = -40 };

            public EquipSetSorter(int Column, SortOrder order, BaseData basedata)
            {
                this.basedata = basedata;
                SortPriority.Add(new Pair<int, SortOrder>(Column, order));
            }

            public void Set_Text(string txt, SortType c, SortOrder order)
            {
                switch (c)
                {
                    case SortType.BYJUELRY://装飾品
                        JewelryText = txt;
                        break;
                    case SortType.BYITEM://素材
                        ItemText = txt;
                        break;
                    case SortType.BYEQUIP://装備
                        EqiupText = txt;
                        break;
                    case SortType.BYCLASS://分類
                        ClassText = txt;
                        break;

                }


                for (int i = SortPriority.Count - 1; i >= 0; i--)
                {
                    if (SortPriority[i].Key == (int)c)
                    {
                        SortPriority.RemoveAt(i);
                    }
                }

                SortPriority.Add(new Pair<int, SortOrder>((int)c, order));

                if (SortPriority.Count > 10)
                {
                    SortPriority.RemoveAt(0);
                }
            }

            public void Set(int Column)
            {

                if (SortPriority.Count == 0)
                {
                    SortPriority.Add(new Pair<int, SortOrder>(Column, SortOrder.Ascending));
                }
                else
                {
                    if (SortPriority[SortPriority.Count - 1].Key == Column)
                    {
                        if (SortPriority[SortPriority.Count - 1].Value == SortOrder.Ascending)
                            SortPriority[SortPriority.Count - 1].Value = SortOrder.Descending;
                        else
                            SortPriority[SortPriority.Count - 1].Value = SortOrder.Ascending;
                    }
                    else
                    {
                        for (int i = SortPriority.Count - 1; i >= 0; i--)
                        {
                            if (SortPriority[i].Key == Column)
                            {
                                SortPriority.RemoveAt(i);
                            }
                        }

                        SortPriority.Add(new Pair<int, SortOrder>(Column, SortOrder.Ascending));

                    }
                }


                if (SortPriority.Count > 10)
                {
                    SortPriority.RemoveAt(0);
                }

            }


            #region IComparer<EquipSet> メンバ


            public int Compare(EquipSet x, EquipSet y)
            {
                int ret = 0;
                int Column;
                SortOrder Order = SortOrder.None;

                for (int i = SortPriority.Count - 1; i >= 0 && ret == 0; i--)
                {
                    Column = SortPriority[i].Key;
                    Order = SortPriority[i].Value;

                    switch (Column)
                    {
                        case (int)SortType.BYJUELRY://装飾品
                            ret = x.CountJewelryByText(JewelryText) - y.CountJewelryByText(JewelryText);

                            break;

                        case (int)SortType.BYITEM://素材
                            ret = x.CountItemByText(ItemText, basedata) - y.CountItemByText(ItemText, basedata);
                            break;

                        case (int)SortType.BYEQUIP://装備
                            ret = x.CountEquipByText(EqiupText) - y.CountEquipByText(EqiupText);
                            break;

                        case (int)SortType.BYCLASS://分類
                            ret = x.CountEquipByClass(ClassText) - y.CountEquipByClass(ClassText);
                            break;

                        case 0://No
                            ret = x.No - y.No;
                            break;
                        case 1://防御
                            ret = y.TotalDef - x.TotalDef;
                            break;
                        case 2://HR
                            ret = y.TotalGettableHR - x.TotalGettableHR;
                            break;
                        case 3://■
                            ret = y.TotalFilledSPSlotNum - x.TotalFilledSPSlotNum;
                            break;
                        case 4://□
                            ret = y.TotalRestSPSlotNum - x.TotalRestSPSlotNum;
                            break;
                        case 5://●
                            ret = y.TotalFilledSlotNum - x.TotalFilledSlotNum;
                            break;
                        case 6://○
                            ret = y.TotalRestSlotNum - x.TotalRestSlotNum;
                            break;
                        case 7:
                            ret = y.ActiveSkillNum - x.ActiveSkillNum;
                            break;
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                        case 12:
                            ret = y.TotalElement[(ElementType)Column - 8] - x.TotalElement[(ElementType)Column - 8];
                            break;
                        default:
                            return 0;
                    }
                }


                if (Order == SortOrder.Ascending)
                    return ret;
                else
                    return -ret;
            }

            #endregion
        }


        internal List<EquipSet> SearchBySkillOrder(SortedList<SkillBase, SkillPointCondition> sortedList)
        {
            List<EquipSet> ret = new List<EquipSet>();


            foreach (EquipSet eset in ShowItemList)
            {
                Dictionary<SkillBase, SkillPoint> PointDict = eset.GetInvokeSKillHashTable();

                bool enough = true;
                foreach (KeyValuePair<SkillBase, SkillPointCondition> pair in sortedList)
                {
                    if (PointDict.ContainsKey(pair.Key))
                    {
                        if (pair.Value.Point >= 0)
                        {
                            if (PointDict[pair.Key].Point < pair.Value.Point)
                            {
                                enough = false;
                                break;
                            }
                        }
                        else
                        {
                            if (PointDict[pair.Key].Point > pair.Value.Point)
                            {
                                enough = false;
                                break;
                            }
                        }

                    }
                    else
                    {
                        enough = false;
                        break;
                    }
                }

                if (enough)
                {
                    ret.Add(eset);
                }


            }

            return ret;
        }
    }
}
