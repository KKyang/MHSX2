using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace MHSX2
{
    public class EquipSetSkillView : ListView
    {
        private enum SortType{SkillName = 0,Point,Actuate};

        EquipSet ESet_value;


        public EquipSetSkillView()
        {
            base.ListViewItemSorter = new ListView_EquipSkillViewComparer();

        }

 

        public EquipSet ESet
        {
            get
            {
                return ESet_value;
            }
            set
            {
                ESet_value = value;
            }
        }

        public void UpdateData()
        {
            if (ESet_value == null)
            {
                Items.Clear();
                return;
            }

            BeginUpdate();

            ListView_EquipSkillViewComparer comp = (ListView_EquipSkillViewComparer)base.ListViewItemSorter;

            comp.lower_skillid = uint.MaxValue;

            Items.Clear();

            List<ListViewItem> AddList = new List<ListViewItem>();

            List<SkillPoint> sppointlist = ESet.GetSkillPointArrayList();
            int count = 0;
            List<uint> list = new List<uint>();


            foreach (SkillPoint sp in sppointlist)
            {

                ListViewItem item = new ListViewItem();


                item.Tag = sp;

                item.Text = sp.SBase.Name;
                if (sp.Point > 0)
                    item.SubItems.Add("+" + sp.Point.ToString());
                else
                    item.SubItems.Add(sp.Point.ToString());


                SkillOption opt = sp.SBase.GetOption(sp.Point);

                if (opt != null)
                {
                    count++;
                    list.Add(sp.SBase.SkillId);

                    item.SubItems.Add(opt.Name);
                }
                else
                    item.SubItems.Add("");

                item.UseItemStyleForSubItems = false;

                if (sp.Point >= 0)
                {
                    item.SubItems[2].ForeColor = System.Drawing.Color.Black;
                }
                else
                {
                    item.SubItems[2].ForeColor = System.Drawing.Color.Red;
                }


                AddList.Add(item);
            }


            if (count > 10)
            {
                int numGRankEquip = 0;
                foreach (Equipment equip in ESet_value.Equips)
                {
                    if (equip.EquipData == null || equip.isChecked == false)
                        continue;


                    foreach (String grank in BaseData.mDefine.GRankEquipType)
                    {
                        if (equip.EquipData.Type.Equals(grank))
                        {
                            numGRankEquip++;
                            break;
                        }
                    }

                }

                int SkillNumUpeer = 10;
                switch (numGRankEquip)
                {
                    case 3:
                    case 4:
                        SkillNumUpeer = 11;
                        break;
                    case 5:
                        SkillNumUpeer = 12;
                        break;
                }

                if (count > SkillNumUpeer)
                {
                    list.Sort();

                    comp.lower_skillid = (uint)list[SkillNumUpeer];//最大発動数番目を記録
                }
            }


            foreach (ListViewItem item in AddList)
            {
                Items.Add(item);

                SkillPoint sp = (SkillPoint)item.Tag;

                if(sp.SBase.SkillId >= comp.lower_skillid)
                    item.SubItems[2].ForeColor = System.Drawing.Color.DarkGray;

            }


            EndUpdate();
        }

        protected override void OnColumnClick(ColumnClickEventArgs e)
        {
            ListView_EquipSkillViewComparer comp = (ListView_EquipSkillViewComparer)base.ListViewItemSorter;

            SortType type = (SortType)e.Column;

            
            if (comp.mSortType == type)
            {
                if (comp.mSortOrder == SortOrder.Ascending)
                    comp.mSortOrder = SortOrder.Descending;
                else
                    comp.mSortOrder = SortOrder.Ascending;
            }
            else
            {
                comp.mSortType = type;
                comp.mSortOrder = SortOrder.Ascending;
            }

            Sort();
        

            base.OnColumnClick(e);
        }


        private class ListView_EquipSkillViewComparer : IComparer
        {
            public uint lower_skillid = uint.MaxValue;//発動スキルIDの最大値。これより大きいスキルIDは発動しない
            public SortType mSortType = SortType.Actuate;
            public SortOrder mSortOrder = SortOrder.Ascending;

            public int Compare(object x, object y)
            {
                ListViewItem item1 = (ListViewItem)x;
                ListViewItem item2 = (ListViewItem)y;

                SkillPoint sp1 = (SkillPoint)item1.Tag;
                SkillPoint sp2 = (SkillPoint)item2.Tag;

                int ret = 0;

                switch (mSortType)
                {
                    case SortType.SkillName:
                        ret = sp1.SBase.SkillId.CompareTo(sp2.SBase.SkillId);
                        break;
                    case SortType.Actuate:
                    case SortType.Point:
                        if (item1.SubItems[2].Text != "" && item2.SubItems[2].Text == "")
                        {
                            ret = -1;
                            break;
                        }
                        else if (item1.SubItems[2].Text == "" && item2.SubItems[2].Text != "")
                        {
                            ret = 1;
                            break;
                        }

                        if (item1.SubItems[2].Text != "")
                        {
                            if (sp1.SBase.SkillId < lower_skillid && sp2.SBase.SkillId >= lower_skillid)
                            {
                                ret = -1;
                                break;
                            }
                            else if (sp1.SBase.SkillId >= lower_skillid && sp2.SBase.SkillId < lower_skillid)
                            {
                                ret = 1;
                                break;
                            }

                        }

                        if (mSortType == SortType.Actuate)
                            ret = sp1.SBase.SkillId.CompareTo(sp2.SBase.SkillId);
                        else
                            ret = ret = sp2.Point - sp1.Point;
                        break;
                        
                }

                if (mSortOrder == SortOrder.Ascending)
                    return ret;
                else
                    return -ret;
            }
        }

    }


}
