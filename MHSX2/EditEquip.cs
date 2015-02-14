using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace MHSX2
{
    partial class EditEquipDialog : Form
    {
        private EquipSet ESet_value;
        private BaseData basedata;
        private SearchCondition DefaultCondition;
        private SearchCondition NowCondition;
        private Ignore ignore;
        private Settings setting;

        private bool ComboBox_rare_Lock = false;//編集中はこれでロックしないと無限ループに

        private ListView[] EquipViewArray = new ListView[(int)EquipKind.NumOfEquipKind];
        private Button[] AddJewelryBtnArray = new Button[(int)EquipKind.NumOfEquipKind + 1];

        private bool isDataInited = false;

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

        int HRLimit
        {
            get { return (int)numericUpDown_HRLimit.Value + comboBox_R_Kind.SelectedIndex * 1000; }
        }

        private EquipmentDataSorter EquipDataSorter = new EquipmentDataSorter(0, SortOrder.Ascending);

        List<int> EquipSortPriority = new List<int>();

        private List<EquipmentData>[] VirtualItems = new List<EquipmentData>[(int)EquipKind.NumOfEquipKind];
        private ListView.SelectedIndexCollection[] SelectedIndexs = new ListView.SelectedIndexCollection[(int)EquipKind.NumOfEquipKind];
        private ListView.ListViewItemCollection[] ItemCollection = new ListView.ListViewItemCollection[(int)EquipKind.NumOfEquipKind];

        private PigClothesSorter pig_sorter = new PigClothesSorter();


        //装備品リストと装飾品リストを全て更新
        private void UpdateLists()
        {
            if (!isDataInited)
                return;

            //検索名取得
            string EquipSearchName = textBox_searchtext.Text;

            string JewelySearchName = textBox_JewelySearch.Text;

            NowCondition = MakeSearchCondition();


            if (NowCondition != null)
            {


                //全てのviewに対して
                foreach (ListView view in EquipViewArray)
                {
                    UpdateEquipView(EquipSearchName, view);
                }

                UpdatePigClothesView(EquipSearchName);


                UpdateJewelryView(JewelySearchName);

            }

            UpdateAddJewelryBtnState();
        }


        //検索条件の取得
        public SearchCondition MakeSearchCondition()
        {
            //検索条件クラス作成
            SearchCondition cond = new SearchCondition();

            cond.ESet = ESet;

            //性別、職業等取得
            cond.sex = (Sex)comboBox_sex.SelectedItem;
            cond.job = (Job)comboBox_job.SelectedItem;
            int.TryParse((string)comboBox_rare_lower.SelectedItem, out cond.rare_lower);
            int.TryParse((string)comboBox_rare_upper.SelectedItem, out cond.rare_upper);

            cond.HR_Limit = (int)numericUpDown_HRLimit.Value + comboBox_R_Kind.SelectedIndex * 1000;



            cond.RustaLv = comboBox_Rusta.SelectedIndex;

            if (cond.rare_upper < cond.rare_lower)
            {
                MessageBox.Show("レア度指定が不正です");
                return null;
            }


            int.TryParse(textBox_def_lower.Text, out cond.defence_lower);

            if (textBox_def_upper.Text == "∞")
                cond.defence_upper = int.MaxValue;
            else
                int.TryParse(textBox_def_upper.Text, out cond.defence_upper);


            if (cond.defence_upper < cond.defence_lower)
            {
                //MessageBox.Show("防御力指定が不正です");
                textBox_def_upper.Text = "∞";
                cond.defence_upper = int.MaxValue;

                return null;
            }


            foreach (ListViewItem item in listView_searchskill.CheckedItems)
            {
                SkillPoint sp = (SkillPoint)item.Tag;
                SkillPointCondition spc = new SkillPointCondition();
                spc.SBase = sp.SBase;
                //sp.Point = int.Parse(item.SubItems["point"].Text);
                spc.Point = int.Parse(item.SubItems["point"].Text);
                cond.SkillPointConditionTable[sp.SBase] = spc;
            }

            cond.SlotPriority = (int)numericUpDown_slotpriority.Value;


            if (radioButton_search_or.Checked)
            {
                cond.skillSearchType = SkillSearchType.OR;
            }
            else
                cond.skillSearchType = SkillSearchType.AND;

            return cond;
        }
        //装飾品リストを更新
        private void UpdateJewelryView(string SearchName)
        {
            listView_jewelry.BeginUpdate();
            listView_jewelry.Items.Clear();

            int index = comboBox_JewelySearch.SelectedIndex;

            if (SearchName.Length != 0)
            {
                foreach (JewelryData_base jd in basedata.JewelryDataMap.Values)
                {
                    if (jd.Name.Contains(SearchName))
                    {
                        AddJewelryToView(jd);
                    }
                }

                foreach (JewelryData_base jd in basedata.SkillCaffDataMap.Values)
                {
                    if (jd.Name.Contains(SearchName))
                    {
                        AddJewelryToView(jd);
                    }
                }

                listView_jewelry.EndUpdate();
                return;
            }



            switch (index)
            {
                case 0://全て
                case 1://装飾品
                case 2://SP
                    List<JewelryData> AddList;

                    AddList = NowCondition.MakeJewelryArray(basedata, false, false);


                    foreach (JewelryData jd in AddList)
                    {
                        if (comboBox_class_jewelry.SelectedIndex != 0)
                        {
                            string Class = (string)comboBox_class_jewelry.SelectedItem;

                            if (Class == "無分類")
                                Class = null;

                            if (jd.Class != Class)
                                continue;
                        }

                        switch (comboBox_JewelySearch.SelectedIndex)
                        {
                            case 1:
                                if (jd.Type != JewelryType.Normal)
                                    continue;
                                break;
                            case 2:
                                if (jd.Type != JewelryType.SP)
                                    continue;
                                break;
                        }


                        AddJewelryToView(jd);

                    }
                    break;
            }



            switch (index)
            {
                case 0://全て
                case 3://スキルカフP
                case 4://スキルカフS
                    List<SkillCuffData> CuffList = NowCondition.MakeSkillCuffList(basedata, false, false, false);

                    foreach (SkillCuffData jd in CuffList)
                    {

                        if (comboBox_class_jewelry.SelectedIndex != 0)
                        {
                            string Class = (string)comboBox_class_jewelry.SelectedItem;

                            if (Class == "無分類")
                                Class = null;

                            if (jd.Class != Class)
                                continue;
                        }


                        switch (index)
                        {
                            case 3:
                                if (jd.SeriesType != SkillCuffSeriesType.P)
                                    continue;
                                break;

                            case 4:
                                if (jd.SeriesType != SkillCuffSeriesType.S)
                                    continue;
                                break;
                        }

                        AddJewelryToView(jd);
                    }
                    break;
            }

            listView_jewelry.EndUpdate();
        }
        //装飾品をリストに追加
        private void AddJewelryToView(JewelryData_base jd)
        {
            ListViewItem item = new ListViewItem();
            item.Tag = jd;
            item.Text = jd.Name;


            item.SubItems.Add(jd.GetSlotString());
            item.SubItems.Add(jd.GetSkillString());

            if (jd.isIgnored == true)
                item.ForeColor = Color.DarkGray;

            listView_jewelry.Items.Add(item);
        }
        //指定した装備候補リストを更新
        private void UpdateEquipView(string SearchName, ListView view)
        {
            view.BeginUpdate();

            view.VirtualListSize = 0;


            int Type = (int)view.Tag;
            VirtualItems[Type].Clear();



            List<EquipmentData> list;

            if ((EquipKind)Type == EquipKind.Weapon)
            {
                list = new List<EquipmentData>();
                foreach (EquipmentData data in basedata.EquipDataMap[0].Values)
                {
                    list.Add(data);
                }
            }
            else
            {

                if (SearchName.Length != 0)
                {
                    SortedList<SkillBase, SkillPointCondition> tmp = NowCondition.SkillPointConditionTable;
                    NowCondition.SkillPointConditionTable = new SortedList<SkillBase, SkillPointCondition>();

                    list = NowCondition.MakeEquipArray(basedata, (EquipKind)view.Tag, false, false, false);
                    NowCondition.SkillPointConditionTable = tmp;
                }
                else
                    list = NowCondition.MakeEquipArray(basedata, (EquipKind)view.Tag, false, false, false);
            }


            if (SearchName.Length != 0)
            {
                List<EquipmentData> newlist = new List<EquipmentData>();
                foreach (EquipmentData ed in list)
                {
                    if (ed.Name.Contains(SearchName))
                    {
                        newlist.Add(ed);
                    }
                }
                list = newlist;
            }

            //全て追加
            foreach (EquipmentData ed in list)
            {
                AddEquipToView(ed, view);
            }


            VirtualItems[Type].Sort(EquipDataSorter);


            view.EndUpdate();
        }

        private void UpdatePigClothesView(string SearchName)
        {
            listView_pig_clothes.Items.Clear();

            foreach (ClothesData clothes in basedata.ClothesDataMap.Values)
            {
                ListViewItem item = new ListViewItem(clothes.Name);
                item.SubItems.Add(clothes.Class);


                item.SubItems.Add(clothes.GetSlotString());

                item.Tag = clothes;

                listView_pig_clothes.Items.Add(item);
            }
        }

        private void UpdateAddJewelryBtnState()
        {
            if (!isDataInited || !listView_equip_condition.isInited)
                return;

            if (listView_jewelry.SelectedIndices.Count == 0)
            {
                foreach (Button btn in AddJewelryBtnArray)
                {
                    btn.Enabled = false;
                }
                return;
            }

            JewelryData_base tagdata = (JewelryData_base)listView_jewelry.SelectedItems[0].Tag;


            switch (tagdata.Type)
            {

                case JewelryType.Normal:
                case JewelryType.SP:
                    JewelryData jd = (JewelryData)listView_jewelry.SelectedItems[0].Tag;

                    foreach (Button btn in AddJewelryBtnArray)
                    {
                        if ((int)btn.Tag >= (int)EquipKind.NumOfEquipKind)
                            continue;

                        Equipment equip = (Equipment)listView_equip_condition.Items[(int)btn.Tag].Tag;
                        if (equip == null)
                        {
                            btn.Enabled = false;
                            continue;
                        }

                        if (equip.isChecked == false)
                        {
                            btn.Enabled = false;
                            continue;
                        }

                        if (equip.GetRestSlotNum() < jd.Slot)
                        {
                            btn.Enabled = false;
                            continue;
                        }

                        if (equip.EquipData.isSP != (jd.Type == JewelryType.SP))
                        {
                            btn.Enabled = false;
                            continue;
                        }
                        btn.Enabled = true;

                    }
                    break;
                case JewelryType.SkillCuff:
                    SkillCuffData sc = (SkillCuffData)tagdata;

                    PigClothes Clothes = (PigClothes)listView_equip_condition.Items[6].Tag;
                    if (Clothes == null)
                    {
                        button_addjewel_pig.Enabled = false;
                        return;
                    }

                    if (!Clothes.isChecked)
                    {
                        button_addjewel_pig.Enabled = false;
                        return;
                    }

                    if (Clothes.Clothes.SetableCuffSeriesType != SkillCuffSeriesType.P && sc.SeriesType == SkillCuffSeriesType.P)
                    {
                        button_addjewel_pig.Enabled = false;
                        return;
                    }

                    if (Clothes.GetRestSlotNum() < sc.Slot)
                    {
                        button_addjewel_pig.Enabled = false;
                        return;
                    }

                    button_addjewel_pig.Enabled = true;

                    break;
            }

        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="b"></param>
        /// <param name="e"></param>
        /// <param name="condition"></param>
        public EditEquipDialog(BaseData b, SearchCondition condition, Settings setting)
        {
            this.DefaultCondition = condition;
            this.setting = setting;

            InitializeComponent();
            basedata = b;
            ignore = condition.ignore;

            basedata.UpdateIgnoreInfo(ignore, setting);
            //condition.SkillPoints.Sort();

            foreach (SkillPoint skillpoint in condition.SkillPointConditionTable.Values)
            {

                ListViewItem item = new ListViewItem();
                item.Text = skillpoint.SBase.Name;
                item.Checked = true;
                ListViewItem.ListViewSubItem sub = new ListViewItem.ListViewSubItem();



                SkillPoint sp2;
                if (skillpoint.Point >= 0)
                    sp2 = new SkillPoint(skillpoint.SBase, 1);
                else
                    sp2 = new SkillPoint(skillpoint.SBase, -1);

                sub.Text = sp2.Point.ToString();
                sub.Name = "point";
                item.SubItems.Add(sub);

                item.Tag = sp2;

                listView_searchskill.Items.Add(item);
            }

            ESet = (EquipSet)condition.ESet.Clone();
            listView_equip_condition.ESet = ESet;
            listView_EquipSkill.ESet = ESet;


            //EquipViewArray構築
            listView_equip_weapon.Tag = EquipKind.Weapon;
            EquipViewArray[(int)EquipKind.Weapon] = listView_equip_weapon;
            listView_equip_head.Tag = EquipKind.Head;
            EquipViewArray[(int)EquipKind.Head] = listView_equip_head;
            listView_equip_body.Tag = EquipKind.Body;
            EquipViewArray[(int)EquipKind.Body] = listView_equip_body;
            listView_equip_arm.Tag = EquipKind.Arm;
            EquipViewArray[(int)EquipKind.Arm] = listView_equip_arm;
            listView_equip_waist.Tag = EquipKind.Waist;
            EquipViewArray[(int)EquipKind.Waist] = listView_equip_waist;
            listView_equip_leg.Tag = EquipKind.Leg;
            EquipViewArray[(int)EquipKind.Leg] = listView_equip_leg;

            for (int i = 0; i < (int)EquipKind.NumOfEquipKind; i++)
            {
                VirtualItems[i] = new List<EquipmentData>();
                SelectedIndexs[i] = new ListView.SelectedIndexCollection(EquipViewArray[i]);
                ItemCollection[i] = new ListView.ListViewItemCollection(EquipViewArray[i]);
            }


            //addjewelボタンにtag追加
            button_addjewel_weapon.Tag = EquipKind.Weapon;
            AddJewelryBtnArray[(int)EquipKind.Weapon] = button_addjewel_weapon;
            button_addjewel_head.Tag = EquipKind.Head;
            AddJewelryBtnArray[(int)EquipKind.Head] = button_addjewel_head;
            button_addjewel_body.Tag = EquipKind.Body;
            AddJewelryBtnArray[(int)EquipKind.Body] = button_addjewel_body;
            button_addjewel_arm.Tag = EquipKind.Arm;
            AddJewelryBtnArray[(int)EquipKind.Arm] = button_addjewel_arm;
            button_addjewel_waist.Tag = EquipKind.Waist;
            AddJewelryBtnArray[(int)EquipKind.Waist] = button_addjewel_waist;
            button_addjewel_leg.Tag = EquipKind.Leg;
            AddJewelryBtnArray[(int)EquipKind.Leg] = button_addjewel_leg;
            button_addjewel_pig.Tag = EquipKind.NumOfEquipKind;
            AddJewelryBtnArray[(int)EquipKind.NumOfEquipKind] = button_addjewel_pig;
        }

        //装備セットビューのチェックが更新されたら
        private void listView_equip_condition_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            listView_equip_condition.UpdateData();
            //listView_EquipSkill.UpdateData();
            UpdateAddJewelryBtnState();
        }

        private void EditEquipDialog_Load(object sender, EventArgs e)
        {
            comboBox_sex.Items.AddRange(new Sex[] { new Sex(SexType.MAN), new Sex(SexType.WOMAN) });
            comboBox_job.Items.AddRange(new Job[] { new Job(JobType.KNIGHT), new Job(JobType.GUNNER), new Job(JobType.COMON) });


            listView_necessity_items.DataSet(basedata, label_costmoney,setting);
            listView_equip_condition.SetViewers(listView_EquipSkill, listView_necessity_items);

            listView_pig_clothes.ListViewItemSorter = pig_sorter;

            comboBox_sex.SelectedIndex = (int)DefaultCondition.sex.type - 1;//こーするしかなかったんやーー

            switch (DefaultCondition.job.type)//ひじょーにやってはいけない実装
            {
                case JobType.COMON:
                    comboBox_job.SelectedIndex = 2;
                    break;
                default:
                    comboBox_job.SelectedIndex = (int)DefaultCondition.job.type - 1;
                    break;
            }



            comboBox_R_Kind.Items.AddRange(BaseData.mDefine.RankLimitKind);

            comboBox_R_Kind.SelectedIndex = DefaultCondition.HR_Limit / 1000;
            numericUpDown_HRLimit.Value = DefaultCondition.HR_Limit % 1000;

            ComboBox_rare_Lock = true;
            comboBox_rare_lower.Items.Add(DefaultCondition.rare_lower.ToString());
            comboBox_rare_upper.Items.Add(DefaultCondition.rare_upper.ToString());

            comboBox_rare_lower.SelectedItem = DefaultCondition.rare_lower.ToString();
            comboBox_rare_upper.SelectedItem = DefaultCondition.rare_upper.ToString();
            ComboBox_rare_Lock = false;
            comboBox_rare_lower_SelectedIndexChanged(null, null);
            comboBox_rare_upper_SelectedIndexChanged(null, null);



            comboBox_Rusta.Items.Clear();

            comboBox_Rusta.Items.Add("-");

            for (int i = 0; i < basedata.MaxRustaLebel; i++)//ラスタレベル追加
            {
                comboBox_Rusta.Items.Add((i + 1).ToString());
            }

            comboBox_Rusta.SelectedIndex = DefaultCondition.RustaLv;

            textBox_def_lower.Text = "0";
            textBox_def_upper.Text = "∞";


            radioButton_search_or.Checked = true;

            isDataInited = true;

            UpdateLists();
            listView_equip_condition.UpdateData();

            //おそらくバグで設定できないのでここで設定
            this.splitContainer2.Panel2MinSize = 458;

            LoadFavoriteEquipSetList();

            comboBox_JewelySearch.SelectedIndex = 0;//全て


            this.Bounds = setting.EditBounds.ToRectangle();
            this.WindowState = setting.EditBounds.State;

        }

        private void button_skill_conditionedit_Click(object sender, EventArgs e)
        {
            List<SkillPriority> list = new List<SkillPriority>();

            foreach (ListViewItem item in listView_searchskill.Items)
            {
                SkillPoint sp = (SkillPoint)item.Tag;
                int.TryParse(item.SubItems["point"].Text, out sp.Point);

                SkillPriority spri = new SkillPriority(sp.SBase, sp.Point, item.Checked);

                list.Add(spri);
            }

            SkillConditionEditDialog dialog = new SkillConditionEditDialog(basedata, list, setting);


            if (dialog.ShowDialog() == DialogResult.OK)
            {
                List<SkillPriority> result = dialog.GetSearchSkills();

                result.Sort();

                isDataInited = false;
                listView_searchskill.Items.Clear();
                foreach (SkillPriority spri in result)
                {
                    SkillPoint sp = new SkillPoint();
                    sp.Point = spri.Priority;
                    sp.SBase = spri.SBase;

                    ListViewItem item = new ListViewItem();
                    item.Tag = sp;
                    item.Text = sp.SBase.Name;
                    item.Checked = spri.Checked;
                    ListViewItem.ListViewSubItem sub = new ListViewItem.ListViewSubItem();
                    sub.Text = sp.Point.ToString();
                    sub.Name = "point";
                    item.SubItems.Add(sub);
                    listView_searchskill.Items.Add(item);
                }
                isDataInited = true;
                UpdateLists();
            }
        }

        private void 全てチェックToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isDataInited = false;
            foreach (ListViewItem item in listView_searchskill.Items)
            {
                item.Checked = true;
            }
            isDataInited = true;
            UpdateLists();
        }

        private void 全てチェックを外すToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isDataInited = false;
            foreach (ListViewItem item in listView_searchskill.Items)
            {
                item.Checked = false;
            }
            isDataInited = true;
            UpdateLists();
        }

        private void UpdateListInvent(object sender, EventArgs e)
        {
            UpdateLists();
        }

        private void UpdateGradeInvent(object sender, EventArgs e)
        {
            UpdateLists();
        }

        private void listView_searchskill_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            UpdateLists();
        }

        private void listView_searchskill_ControlRemoved(object sender, ControlEventArgs e)
        {
            UpdateLists();
        }

        private void AddEquipToView(EquipmentData ed, ListView view)
        {
            EquipKind type = (EquipKind)view.Tag;


            int v = ed.Slot * NowCondition.SlotPriority;//評価値のみ計算

            foreach (SkillPoint sp1 in NowCondition.SkillPointConditionTable.Values)
            {
                foreach (SkillPoint point in ed.SkillPointList)
                {
                    if (sp1.SBase == point.SBase)
                    {
                        v += sp1.Point * point.Point;
                    }
                }
            }
            ed.Score = v;


            VirtualItems[(int)type].Add(ed);
            EquipViewArray[(int)type].VirtualListSize++;

        }

        private void textBox_searchtext_TextChanged(object sender, EventArgs e)
        {
            if (!isDataInited)
                return;

            //検索名取得
            string SearchName = textBox_searchtext.Text;

            NowCondition = MakeSearchCondition();


            if (NowCondition != null)
            {
                //全てのviewに対して
                foreach (ListView view in EquipViewArray)
                {
                    UpdateEquipView(SearchName, view);
                }
            }
        }

        private class JewelListViewSorter : IComparer
        {
            int column = 0;
            SortOrder so;
            public JewelListViewSorter(int column, SortOrder o)
            {
                this.column = column;
                so = o;
            }

            public void SetColumn(int column)
            {
                if (this.column == column)
                {
                    if (so == SortOrder.Ascending)
                        so = SortOrder.Descending;
                    else
                        so = SortOrder.Ascending;
                }
                else
                {
                    this.column = column;
                    so = SortOrder.Descending;
                }
            }

            #region IComparer メンバ

            public int Compare(object x, object y)
            {
                ListViewItem X = (ListViewItem)x;
                ListViewItem Y = (ListViewItem)y;



                if (so == SortOrder.Ascending)
                    return X.SubItems[column].Text.CompareTo(Y.SubItems[column].Text);
                else
                    return Y.SubItems[column].Text.CompareTo(X.SubItems[column].Text);

            }

            #endregion
        }

        private void listView_equip_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListView view = (ListView)sender;

            if (view.Items.Count == 0)
                return;


            int type = (int)view.Tag;


            EquipDataSorter.Set(e.Column);

            VirtualItems[type].Sort(EquipDataSorter);

            view.RedrawItems(view.TopItem.Index, view.VirtualListSize - 1, true);
        }

        public class EquipmentDataSorter : IComparer<EquipmentData>
        {
            List<Pair<int, SortOrder>> SortPriority = new List<Pair<int, SortOrder>>();

            public EquipmentDataSorter(int column, SortOrder order)
            {
                SortPriority.Add(new Pair<int, SortOrder>(column, order));
            }




            public void Set(int Column)
            {

                if (SortPriority.Count == 0)
                {
                    SortPriority.Add(new Pair<int, SortOrder>(Column, SortOrder.Ascending));
                    return;
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
                        for (int i = 0; i < SortPriority.Count; i++)
                        {
                            if (SortPriority[i].Key == Column)
                            {
                                SortPriority.RemoveAt(i);
                            }
                        }

                        SortPriority.Add(new Pair<int, SortOrder>(Column, SortOrder.Ascending));
                    }
                }

            }

            #region IComparer<EquipmentData> メンバ

            public int Compare(EquipmentData x, EquipmentData y)
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
                        case 0://評価値
                            ret = y.Score - x.Score;
                            break;
                        case 1://名前
                            ret = y.Name.CompareTo(x.Name);
                            break;
                        case 2://分類
                            if (y.Class != null)
                            {
                                ret = y.Class.CompareTo(x.Class);
                            }
                            else if (x.Class != null)
                            {
                                ret = -x.Class.CompareTo(y.Class);
                            }
                            else
                                ret = 0;

                            break;
                        case 3://レベル
                            ret = y.Level - x.Level;
                            break;
                        case 4://防御値
                            ret = y.Def - x.Def;
                            break;
                        case 5://HR
                            ret = y.LevelList[y.Level - 1].GetableHR - x.LevelList[x.Level - 1].GetableHR;
                            break;
                        case 6:
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                            int index = Column - 6;

                            if (x.SkillPointList.Length > index)
                            {
                                if (y.SkillPointList.Length > index)
                                {
                                    ret = y.SkillPointList[index].SBase.Name.CompareTo(
                                        x.SkillPointList[index].SBase.Name);
                                }
                                else
                                {
                                    ret = 1;
                                }
                            }
                            else if (y.SkillPointList.Length > index)
                            {
                                ret = -1;
                            }
                            else
                                ret = 0;

                            break;
                        case 11:

                            ret = y.GetSlotString().CompareTo(x.GetSlotString());

                            break;
                        case 12:
                            ret = y.Rare - x.Rare;
                            break;
                        case 13:
                        case 14:
                        case 15:
                        case 16:
                        case 17:
                            int n = Column - 13;
                            ret = y.Element[(ElementType)n] - x.Element[(ElementType)n];

                            break;
                        default:
                            ret = 0;
                            break;
                    }
                }

                if (Order == SortOrder.Ascending)
                    return ret;
                else
                    return -ret;
            }

            #endregion
        }

        private void listView_equip_DoubleClick(object sender, EventArgs e)
        {
            ListView view = (ListView)sender;
            EquipSet ESet = (EquipSet)listView_equip_condition.ESet;

            int type = (int)view.Tag;

            ListViewItem item = ItemCollection[type][SelectedIndexs[type][0]];

            ESet.Equips[type].EquipData = (EquipmentData)item.Tag;


            listView_equip_condition.UpdateData();

            UpdateAddJewelryBtnState();
            //listView_EquipSkill.UpdateData();
        }

        private void button_unset_Click(object sender, EventArgs e)
        {
            foreach (int i in listView_equip_condition.SelectedIndices)
            {
                if (i >= (int)EquipKind.NumOfEquipKind + 1)
                {
                    continue;
                }


                if (i == 6)
                {
                    listView_equip_condition.ESet.PigClothes = new PigClothes();
                }
                else
                    listView_equip_condition.ESet[(EquipKind)i] = new Equipment();
            }


            listView_equip_condition.UpdateData();
        }

        private void comboBox_sex_SelectedIndexChanged(object sender, EventArgs e)
        {
            Sex sex = (Sex)comboBox_sex.SelectedItem;
            foreach (Equipment equip in ESet.Equips)
            {
                if (equip.EquipData != null)
                {
                    if (equip.EquipData.WearableSex != sex.type && equip.EquipData.WearableSex != SexType.COMON)
                    {
                        equip.EquipData = null;
                    }
                }
            }

            listView_equip_condition.UpdateData();
            UpdateLists();
        }

        private void comboBox_job_SelectedIndexChanged(object sender, EventArgs e)
        {

            Job job = (Job)comboBox_job.SelectedItem;
            foreach (Equipment equip in ESet.Equips)
            {
                if (equip.EquipData != null)
                {
                    if (job.type != JobType.COMON)
                    {
                        if (equip.EquipData.WearableJob != job.type && equip.EquipData.WearableJob != JobType.COMON)
                        {
                            equip.EquipData = null;
                        }
                    }
                    else
                    {
                        if (equip.EquipData.WearableJob != JobType.COMON)
                        {
                            equip.EquipData = null;
                        }
                    }

                }
            }
            listView_equip_condition.UpdateData();
            UpdateLists();
        }

        private void UpdateGradeInvent(object sender, ControlEventArgs e)
        {
            SearchCondition cond = MakeSearchCondition();
            if (cond != null)
            {
                UpdateLists();
            }
        }

        private void button_addjewel_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;


            if (listView_jewelry.SelectedItems.Count != 1)
                return;

            JewelryData_base jdb = (JewelryData_base)listView_jewelry.SelectedItems[0].Tag;


            Equipment_base equip;
            if ((EquipKind)btn.Tag != EquipKind.NumOfEquipKind)
                equip = listView_equip_condition.ESet[(EquipKind)btn.Tag];
            else
                equip = listView_equip_condition.ESet.PigClothes;


            equip.SetJewelry(jdb);

            listView_equip_condition.UpdateData();
            //listView_EquipSkill.UpdateData();
            UpdateAddJewelryBtnState();
        }

        private void listView_jewelry_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateAddJewelryBtnState();
        }

        private void button_unset_jewelry_Click(object sender, EventArgs e)
        {
            foreach (int i in listView_equip_condition.SelectedIndices)
            {
                if (i > (int)EquipKind.NumOfEquipKind)
                    continue;
                else if (i == (int)EquipKind.NumOfEquipKind)
                {
                    listView_equip_condition.ESet.PigClothes.SkillCuffs = new SkillCuffData[3];
                }
                else
                {
                    Equipment equip = listView_equip_condition.ESet[(EquipKind)i];

                    if (equip.EquipData == null)
                        continue;

                    equip.jewelrys = new JewelryData[3];

                }
            }

            listView_equip_condition.UpdateData();
            //listView_EquipSkill.UpdateData();
            UpdateAddJewelryBtnState();
        }

        private void listView_equip_condition_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (listView_equip_condition.SelectedIndices.Count == 1)
                tabControl_equip.SelectedIndex = listView_equip_condition.SelectedIndices[0];
        }

        private void listView_jewelry_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListView view = (ListView)sender;
            JewelListViewSorter sorter;
            if (view.ListViewItemSorter == null)
            {
                sorter = new JewelListViewSorter(e.Column, SortOrder.Descending);
                view.ListViewItemSorter = sorter;
            }
            else
            {
                sorter = (JewelListViewSorter)view.ListViewItemSorter;
                sorter.SetColumn(e.Column);
            }

            view.Sort();
        }

        private void listView_jewelry_DoubleClick(object sender, EventArgs e)
        {
            for (int i = 0; i < 7; i++)
            {
                if (AddJewelryBtnArray[i].Enabled)
                {
                    button_addjewel_Click(AddJewelryBtnArray[i], null);
                    break;
                }
            }
        }

        private void button_clip_Click(object sender, EventArgs e)
        {
            MHSX2.MHSX2Form.ClipEquipSet((Job)comboBox_job.SelectedItem, (Sex)comboBox_sex.SelectedItem, listView_equip_condition.ESet);
        }

        private void button_read_clip_Click(object sender, EventArgs e)
        {
            bool errflag = false;
            List<string> notfound = new List<string>();
            string clip = Clipboard.GetText();
            EquipSet tmpSet = new EquipSet();

            Job job = new Job(JobType.COMON);
            Sex sex = new Sex(SexType.COMON);


            string[] lines = clip.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length < 5)
            {
                MessageBox.Show("フォーマットが多分違います");
                return;
            }

            string[] words;

            bool find = false;
            int lineindex = 0;
            for (; lineindex < lines.Length - 5; lineindex++)
            {
                words = lines[lineindex].Split(new char[] { ' ', '　', ',', '.' }, StringSplitOptions.RemoveEmptyEntries);
                string name = words[0];

                if (name == "武器スロットなし")
                {
                    find = true;
                    break;
                }

                if (basedata.EquipDataMap[0].ContainsKey(name))
                {
                    find = true;
                    tmpSet[EquipKind.Weapon].EquipData = (EquipmentData)basedata.EquipDataMap[0][name];

                    if (tmpSet[EquipKind.Weapon].EquipData.WearableJob != job.type)
                    {
                        job.type = tmpSet[EquipKind.Weapon].EquipData.WearableJob;
                    }

                    if (tmpSet[EquipKind.Weapon].EquipData.WearableSex != sex.type)
                    {
                        job.type = tmpSet[EquipKind.Weapon].EquipData.WearableJob;
                    }


                    tmpSet[EquipKind.Weapon].Level = tmpSet[EquipKind.Weapon].EquipData.LevelList.Count;

                    if (words.Length > 2)//装飾品があるなら
                    {
                        int begin = 2;
                        int level;
                        if (words[1].StartsWith("Lv"))
                        {
                            int.TryParse(words[1].Replace("Lv", ""), out level);

                            if (level != 0)
                            {
                                begin += 1;
                                tmpSet[EquipKind.Weapon].Level = level;
                            }

                        }

                        if (words[2].StartsWith("Lv"))
                        {

                            int.TryParse(words[2].Replace("Lv", ""), out level);

                            if (level != 0)
                            {
                                begin += 2;
                                tmpSet[EquipKind.Weapon].Level = level;
                            }
                        }

                        int count = 0;
                        for (int j = 0; count < tmpSet[EquipKind.Weapon].Slot && j + begin < words.Length; j++)
                        {
                            string jewel = words[begin + j];
                            if (basedata.JewelryDataMap.ContainsKey(jewel))
                            {
                                JewelryData jd = (JewelryData)basedata.JewelryDataMap[jewel];
                                tmpSet[0].SetJewelry(jd);
                                count++;
                            }
                        }
                    }
                    break;
                }
            }

            if (!find)
            {
                MessageBox.Show("フォーマットが多分違います");
                return;
            }

            for (int i = 1; i < 6; i++)
            {
                words = lines[lineindex + i].Split(new char[] { ' ', '　', ',' }, StringSplitOptions.RemoveEmptyEntries);
                string name = words[0];

                if (name == "なし")
                {
                    continue;
                }

                if (basedata.EquipDataMap[i].ContainsKey(name))
                {
                    tmpSet[(EquipKind)i].EquipData = (EquipmentData)basedata.EquipDataMap[i][name];
                }
                else
                {
                    if (name != "スロット3装備")
                    {
                        errflag = true;
                        notfound.Add(name);
                    }


                    EquipmentData data = new EquipmentData();
                    data.Kind = (EquipKind)i;
                    data.Name = name;
                    data.Element.Clear();
                    data.Rare = 1;
                    data.WearableJob = JobType.COMON;
                    data.WearableSex = SexType.COMON;
                    data.SkillPointList = new SkillPoint[0];
                    data.Def = 0;
                    data.Slot = 3;
                    data.Level = 1;
                    data.LevelList.Add(new Level());
                    data.LevelList[0].Slot = 3;
                    data.LevelList[0].Money = 0;
                    data.LevelList[0].Def = 0;
                    data.LevelList[0].GetableHR = 0;


                    tmpSet[(EquipKind)i].EquipData = data;
                }


                if (tmpSet[(EquipKind)i].EquipData.WearableSex != sex.type)
                {
                    if (sex.type != SexType.COMON)
                    {
                        if (tmpSet[(EquipKind)i].EquipData.WearableSex != SexType.COMON)
                        {
                            MessageBox.Show("性別が入り乱れてます");
                            return;
                        }
                    }
                    else
                        sex.type = tmpSet[(EquipKind)i].EquipData.WearableSex;
                }

                if (tmpSet[(EquipKind)i].EquipData.WearableJob != job.type)
                {
                    if (job.type != JobType.COMON)
                    {
                        if (tmpSet[(EquipKind)i].EquipData.WearableJob != JobType.COMON)
                        {
                            MessageBox.Show("職種が入り乱れてます");
                            return;
                        }
                    }
                    else
                        job.type = tmpSet[(EquipKind)i].EquipData.WearableJob;
                }

                tmpSet[(EquipKind)i].Level = tmpSet[(EquipKind)i].EquipData.LevelList.Count;

                if (words.Length > 2)
                {
                    int begin = 2;
                    int level;
                    if (words[1].StartsWith("Lv"))
                    {
                        if (int.TryParse(words[1].Replace("Lv", ""), out level))
                        {
                            begin += 1;
                            tmpSet[(EquipKind)i].Level = level;
                        }

                    }
                    else if (words[2].StartsWith("Lv"))
                    {
                        if (int.TryParse(words[2].Replace("Lv", ""), out level))
                        {
                            begin += 2;
                            tmpSet[(EquipKind)i].Level = level;
                        }
                    }
                    else
                    {
                        tmpSet[(EquipKind)i].Level = tmpSet[(EquipKind)i].equipdata_value.LevelList.Count;
                    }


                    if (tmpSet[(EquipKind)i].Level > tmpSet[(EquipKind)i].equipdata_value.LevelList.Count)
                    {
                        tmpSet[(EquipKind)i].Level = tmpSet[(EquipKind)i].equipdata_value.LevelList.Count;
                    }



                    if (tmpSet[(EquipKind)i].EquipData != null)
                    {
                        level = tmpSet[(EquipKind)i].Level;
                        List<Level> LevelList = tmpSet[(EquipKind)i].EquipData.LevelList;

                        tmpSet[(EquipKind)i].Def = LevelList[level - 1].Def;
                        tmpSet[(EquipKind)i].Slot = LevelList[level - 1].Slot;
                    }


                    int count = 0;
                    for (int j = 0; count < tmpSet[(EquipKind)i].Slot && j + begin < words.Length; j++)
                    {
                        string jewel = words[begin + j];
                        if (basedata.JewelryDataMap.ContainsKey(jewel))
                        {
                            JewelryData jd = (JewelryData)basedata.JewelryDataMap[jewel];
                            tmpSet[(EquipKind)i].SetJewelry(jd);
                            count++;
                        }
                    }
                }

            }

            if (lines.Length > lineindex + 6)
            {
                words = lines[lineindex + 6].Split(new char[] { ' ', '　', ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (basedata.ClothesDataMap.ContainsKey(words[0]))
                {
                    tmpSet.PigClothes.Clothes = basedata.ClothesDataMap[words[0]];

                    for (int i = 1; i < words.Length; i++)
                    {
                        if (basedata.SkillCaffDataMap.ContainsKey(words[i]))
                        {
                            tmpSet.PigClothes.SetJewelry(basedata.SkillCaffDataMap[words[i]]);
                        }
                    }
                }
            }


            switch (sex.type)
            {
                case SexType.COMON:
                case SexType.MAN:
                    comboBox_sex.SelectedIndex = 0;
                    break;
                case SexType.WOMAN:
                    comboBox_sex.SelectedIndex = 1;
                    break;
            }

            switch (job.type)
            {
                case JobType.COMON:
                    comboBox_job.SelectedIndex = 2;
                    break;
                case JobType.KNIGHT:
                    comboBox_job.SelectedIndex = 0;
                    break;
                case JobType.GUNNER:
                    comboBox_job.SelectedIndex = 1;
                    break;
            }

            ESet = tmpSet;
            listView_equip_condition.ESet = tmpSet;
            listView_equip_condition.UpdateData();

            if (errflag)
            {
                string str = "";
                foreach (string n in notfound)
                {
                    str += n + "\r\n";
                }

                str += "以上のデータが見つかりませんでした";

                MessageBox.Show(str);
            }
            //listView_EquipSkill.ESet = tmpSet;
            //listView_EquipSkill.UpdateData();
        }

        private void button_search_text_clear_Click(object sender, EventArgs e)
        {
            textBox_searchtext.Clear();
        }

        private void 除外指定に追加ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListView list = (ListView)contextMenuStrip_equip_view.SourceControl;

            if (list.Equals(listView_pig_clothes))
                return;

            if (!list.Equals(listView_jewelry))
            {//装備
                int type = (int)list.Tag;
                foreach (int index in SelectedIndexs[type])
                {
                    ListViewItem item = ItemCollection[type][index];

                    if (item.Tag is EquipmentData)
                    {
                        EquipmentData data = (EquipmentData)item.Tag;
                        if (!ignore.Equip.Contains(data.Name))
                        {
                            data.isIgnored = true;
                            ignore.Equip.Add(data.Name);
                            item.UseItemStyleForSubItems = true;
                            item.ForeColor = Color.DarkGray;
                        }
                    }
                }

            }
            else
            {//装飾品
                foreach (ListViewItem item in list.SelectedItems)
                {
                    JewelryData_base data = (JewelryData_base)item.Tag;

                    if (data is JewelryData)
                    {
                        if (!ignore.Jewelry.Contains(data.Name))
                        {
                            ignore.Jewelry.Add(data.Name);
                            if (setting.IgnoreItem)
                            {
                                data.isIgnored = true;
                                item.ForeColor = Color.DarkGray;
                            }
                        }
                    }
                    else if (data is SkillCuffData)
                    {
                        if (!ignore.SkillCuff.Contains(data.Name))
                        {
                            ignore.SkillCuff.Add(data.Name);
                            if (setting.IgnoreItem)
                            {
                                data.isIgnored = true;
                                item.ForeColor = Color.DarkGray;
                            }
                        }
                    }
                }
            }

            //basedata.UpdateIgnoreInfo(ignore);
            //UpdateLists();
        }

        private void 除外指定を解除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListView list = (ListView)contextMenuStrip_equip_view.SourceControl;

            if (list.Equals(listView_pig_clothes))
                return;

            if (!list.Equals(listView_jewelry))
            {//装備
                int type = (int)list.Tag;
                foreach (int index in SelectedIndexs[type])
                {

                    ListViewItem item = ItemCollection[type][index];
                    if (item.Tag is EquipmentData)
                    {
                        EquipmentData data = (EquipmentData)item.Tag;

                        ignore.Equip.Remove(data.Name);
                        item.UseItemStyleForSubItems = true;

                        string str = data.Class;
                        if (str == null)
                            str = "無分類";

                        if ((setting.IgnoreItem && ignore.Equip.Contains(data.Name))
                            ||
                            (setting.IgnoreClass && ignore.Class.Contains(str)))
                        {
                            data.isIgnored = true;
                        }
                        else
                        {
                            data.isIgnored = false;
                        }
                    }
                }
            }
            else
            {//装飾品
                foreach (ListViewItem item in list.SelectedItems)
                {
                    JewelryData_base data = (JewelryData_base)item.Tag;

                    if (data is JewelryData)
                    {
                        ignore.Jewelry.Remove(data.Name);
                    }
                    else
                    {
                        ignore.SkillCuff.Remove(data.Name);
                    }
                    data.isIgnored = false;
                    item.ForeColor = Color.Black;
                }
            }



            //basedata.UpdateIgnoreInfo(ignore);
            //UpdateLists();
        }

        private void button_favorite_save_Click(object sender, EventArgs e)
        {
            if (comboBox_favorite_equipset.SelectedIndex == 0)//新規
            {
                AdmFavoriteSet.Save(ESet, (Job)comboBox_job.SelectedItem, (Sex)comboBox_sex.SelectedItem, null);

                LoadFavoriteEquipSetList();
            }
            else//上書き
            {

                AdmFavoriteSet.Save(ESet, (Job)comboBox_job.SelectedItem, (Sex)comboBox_sex.SelectedItem, comboBox_favorite_equipset.Text);


            }
        }

        private void LoadFavoriteEquipSetList()
        {
            if (!Directory.Exists(Properties.Resources.DNAME_FAVORITE))
            {
                Directory.CreateDirectory(Properties.Resources.DNAME_FAVORITE);
            }

            string[] list = Directory.GetFiles(Properties.Resources.DNAME_FAVORITE, "*.eset");

            for (int i = 0; i < list.Length; i++)
            {
                list[i] = Path.GetFileNameWithoutExtension(list[i]);
            }

            comboBox_favorite_equipset.Items.Clear();


            comboBox_favorite_equipset.Items.Add("新規");
            comboBox_favorite_equipset.Items.AddRange(list);

            comboBox_favorite_equipset.SelectedIndex = 0;
        }

        private void comboBox_favorite_equipset_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_favorite_equipset.SelectedIndex == 0)//新規
            {
                button_favorite_save.Text = "保存";
                button_favorite_delete.Enabled = false;
                button_load.Enabled = false;
            }
            else
            {
                button_favorite_save.Text = "上書き";
                button_favorite_delete.Enabled = true;
                button_load.Enabled = true;
            }
        }

        private void button_favorite_delete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("「" + comboBox_favorite_equipset.SelectedItem.ToString() + "」を削除します。よろしいですか？", "削除確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Cancel)
            {
                return;
            }


            AdmFavoriteSet.Delete(comboBox_favorite_equipset.SelectedItem.ToString());

            LoadFavoriteEquipSetList();
        }

        private void numericUpDown_HRLimit_ValueChanged(object sender, EventArgs e)
        {
            basedata.UpdateEquipLevel(HRLimit);

            UpdateLists();
        }

        private void listView_equip_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            ListView from = (ListView)sender;
            int type = (int)from.Tag;

            if (NowCondition == null)
            {
                NowCondition = MakeSearchCondition();
                if (NowCondition == null)
                {
                    return;
                }
            }



            EquipmentData ed = VirtualItems[type][e.ItemIndex];
            ListViewItem item = new ListViewItem();

            item.UseItemStyleForSubItems = false;
            item.Tag = ed;
            item.Text = ed.Score.ToString();
            item.SubItems.Add(ed.Name);
            item.SubItems.Add(ed.Class);
            item.SubItems.Add(ed.Level.ToString());
            item.SubItems.Add(ed.Def.ToString());



            if (0 < ed.Level && ed.Level <= ed.LevelList.Count + 1)
            {
                int hr = ed.LevelList[ed.Level - 1].GetableHR;

                item.SubItems.Add(BaseData.mDefine.GetRankKindString(hr) + (hr % 1000).ToString());
            }
            else
                item.SubItems.Add("");



            for (int i = 0; i < 5; i++)
            {
               if (ed.SkillPointList.Length > i)
                {
                    ListViewItem.ListViewSubItem sub = item.SubItems.Add(ed.SkillPointList[i].ToString());

                    if (NowCondition.SkillPointConditionTable.ContainsKey(ed.SkillPointList[i].SBase))
                    {
                        SkillPoint sp = (SkillPoint)NowCondition.SkillPointConditionTable[ed.SkillPointList[i].SBase];
                        if (SubFunc.CompairSgin(ed.SkillPointList[i].Point, sp.Point))
                        {
                            sub.ForeColor = Color.Blue;
                        }
                        else
                            sub.ForeColor = Color.Red;
                    }
                    else
                    {
                        sub.ForeColor = Color.Black;
                    }

                }
                else
                {
                    item.SubItems.Add("");
                }
            }



            item.SubItems.Add(ed.GetSlotString());

            item.SubItems.Add(ed.Rare.ToString());
            for (ElementType i = ElementType.Fire; i < ElementType.NumOfElementKind; i++)
            {
                if (ed.Element[i] > 0)
                    item.SubItems.Add("+" + ed.Element[i].ToString());
                else
                    item.SubItems.Add(ed.Element[i].ToString());
            }


            if (ed.isIgnored)
            {
                item.UseItemStyleForSubItems = true;
                item.ForeColor = Color.DarkGray;
            }

            e.Item = item;
        }

        private void button_copy_items_Click(object sender, EventArgs e)
        {
            listView_necessity_items.ClipItems();
        }

        private void tabControl_equip_SizeChanged(object sender, EventArgs e)
        {
            tabControl_equip.Refresh();
        }

        private void tabControl1_SizeChanged(object sender, EventArgs e)
        {
            tabControl1.Refresh();
        }

        private void 詳細ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListView list = (ListView)contextMenuStrip_equip_view.SourceControl;



            if (!list.Equals(listView_jewelry) && !list.Equals(listView_pig_clothes))
            {
                int type = (int)list.Tag;
                foreach (int index in SelectedIndexs[type])
                {
                    ListViewItem item = ItemCollection[type][index];
                    EquipmentData data = (EquipmentData)item.Tag;

                    EquipDataDetaileDialog dialog = new EquipDataDetaileDialog(basedata, setting, data, data.Level, (Sex)comboBox_sex.SelectedItem);
                    dialog.Show();
                }

            }
            else if (list.Equals(listView_jewelry))
            {
                foreach (ListViewItem item in listView_jewelry.SelectedItems)
                {
                    JewelryData_base data = (JewelryData_base)item.Tag;
                    EquipDataDetaileDialog dialog = new EquipDataDetaileDialog(basedata, setting, data);
                    dialog.Show();
                }
            }



        }

        private void button_load_Click(object sender, EventArgs e)
        {
            string job;
            string sex;

            ESet = AdmFavoriteSet.Load(comboBox_favorite_equipset.SelectedItem.ToString(), basedata, out job, out sex);

            if (job == "剣士")
                comboBox_job.SelectedIndex = 0;
            else if (job == "ガンナー")
                comboBox_job.SelectedIndex = 1;
            else if (job == "共用のみ")
                comboBox_job.SelectedIndex = 2;


            if (sex == "男")
                comboBox_sex.SelectedIndex = 0;
            else if (sex == "女")
                comboBox_sex.SelectedIndex = 1;

            listView_equip_condition.ESet = ESet;
            listView_equip_condition.UpdateData();
        }

        private void comboBox_rare_lower_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ComboBox_rare_Lock)
                return;

            ComboBox_rare_Lock = true;

            string nol = (string)comboBox_rare_lower.SelectedItem;


            int xl;
            if (int.TryParse(nol, out xl) == false)
            {
                xl = 1;
            }

            string nou = (string)comboBox_rare_upper.SelectedItem;

            comboBox_rare_upper.BeginUpdate();
            comboBox_rare_upper.Items.Clear();

            for (int i = xl; i <= 20; i++)
            {
                comboBox_rare_upper.Items.Add(i.ToString());
            }

            comboBox_rare_upper.SelectedItem = nou;
            comboBox_rare_upper.EndUpdate();
            ComboBox_rare_Lock = false;

            UpdateListInvent(sender, e);
        }

        private void comboBox_rare_upper_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ComboBox_rare_Lock)
                return;

            ComboBox_rare_Lock = true;

            string nou = (string)comboBox_rare_upper.SelectedItem;


            int xu;
            if (int.TryParse(nou, out xu) == false)
            {
                xu = 20;
            }

            string nol = (string)comboBox_rare_lower.SelectedItem;

            comboBox_rare_lower.BeginUpdate();
            comboBox_rare_lower.Items.Clear();

            for (int i = 1; i <= xu; i++)
            {
                comboBox_rare_lower.Items.Add(i.ToString());
            }

            comboBox_rare_lower.SelectedItem = nol;
            comboBox_rare_lower.EndUpdate();
            ComboBox_rare_Lock = false;

            UpdateListInvent(sender, e);
        }

        private void 詳細ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            EquipSet set = listView_equip_condition.ESet;

            foreach (ListViewItem item in listView_equip_condition.SelectedItems)
            {
                if (item.Tag is Equipment)
                {
                    Equipment equip = (Equipment)item.Tag;

                    if (equip == null)
                        continue;

                    if (equip.EquipData != null)
                    {
                        EquipDataDetaileDialog dialog = new EquipDataDetaileDialog(basedata, setting, equip.EquipData, equip.Level, (Sex)comboBox_sex.SelectedItem);
                        dialog.Show();
                    }
                }
            }

        }

        private void listView_equip_condition_DoubleClick(object sender, EventArgs e)
        {
            詳細ToolStripMenuItem1_Click(sender, e);
        }

        private void comboBox_Rusta_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateLists();
        }

        private void EditEquipDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            setting.EditBounds = new WindowStatus(this);
        }

        private void tabControl_equip_Selected(object sender, TabControlEventArgs e)
        {
            if (e.TabPageIndex >= EquipViewArray.Length)//豚服は除外
                return;

            ListView view = EquipViewArray[e.TabPageIndex];

            if (view.VirtualListSize == 0)
                return;

            VirtualItems[e.TabPageIndex].Sort(EquipDataSorter);

            view.RedrawItems(view.TopItem.Index, view.VirtualListSize - 1, true);
        }

        private void listView_pig_clothes_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (pig_sorter.c == e.Column)
            {
                if (pig_sorter.o == SortOrder.Ascending)
                {
                    pig_sorter.Set(e.Column, SortOrder.Descending);
                }
                else
                    pig_sorter.Set(e.Column, SortOrder.Ascending);
            }
            else
            {
                pig_sorter.Set(e.Column, SortOrder.Ascending);
            }


            listView_pig_clothes.Sort();
        }

        class PigClothesSorter : IComparer
        {
            public int c;
            public SortOrder o;

            public void Set(int c, SortOrder order)
            {
                this.c = c;
                this.o = order;
            }


            #region IComparer メンバ

            public int Compare(object x, object y)
            {
                ListViewItem a = (ListViewItem)x;
                ListViewItem b = (ListViewItem)y;

                int ret = 0;

                switch (c)
                {
                    case 0:
                    case 1:
                    case 2:
                        ret = b.SubItems[c].Text.CompareTo(a.SubItems[c].Text);
                        break;
                }

                if (o == SortOrder.Ascending)
                    return ret;
                else
                    return -ret;

            }

            #endregion
        }

        private void listView_pig_clothes_DoubleClick(object sender, EventArgs e)
        {
            if (listView_pig_clothes.SelectedItems.Count < 1)
                return;


            ClothesData clothes = (ClothesData)listView_pig_clothes.SelectedItems[0].Tag;


            ESet.PigClothes.Clothes = clothes;
            ESet.PigClothes.SkillCuffs = new SkillCuffData[3];
            ESet.PigClothes.isChecked = true;

            listView_equip_condition.UpdateData();

            UpdateAddJewelryBtnState();
        }

        private void textBox_JewelySearch_TextChanged(object sender, EventArgs e)
        {
            if (!isDataInited)
                return;

            //検索名取得
            string SearchName = textBox_JewelySearch.Text;

            if (SearchName.Length != 0)
            {
                comboBox_class_jewelry.Enabled = false;
                comboBox_JewelySearch.Enabled = false;
            }
            else
            {
                comboBox_class_jewelry.Enabled = true;
                comboBox_JewelySearch.Enabled = true;
            }


            NowCondition = MakeSearchCondition();

            if (NowCondition != null)
            {
                UpdateJewelryView(SearchName);
            }
        }

        private void comboBox_JewelySearch_SelectedIndexChanged(object sender, EventArgs e)
        {

            comboBox_class_jewelry.Items.Clear();
            comboBox_class_jewelry.Items.Add("全て");

            comboBox_class_jewelry.SelectedIndex = 0;//全てで検索がリスト更新が走る。


            List<string> ClassList = new List<string>();

            foreach (ListViewItem item in listView_jewelry.Items)
            {
                JewelryData_base jdb = (JewelryData_base)item.Tag;


                if (!ClassList.Contains(jdb.Class) && jdb.Class != null && jdb.Class != "")
                {
                    ClassList.Add(jdb.Class);
                }
            }

            ClassList.Sort();
            ClassList.Add("無分類");

            comboBox_class_jewelry.Items.AddRange(ClassList.ToArray());

        }

        private void comboBox_class_jewelry_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateJewelryView(textBox_JewelySearch.Text);
        }

    }

}
