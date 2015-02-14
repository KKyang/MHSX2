using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using Microsoft.VisualBasic;
using System.IO;
using System.Xml;


namespace MHSX2
{
    public partial class MHSX2Form : Form
    {
        public BaseData basedata = new BaseData();
        private EquipSet ESet_value = new EquipSet();

        bool isSearching = false;
        public SearchClass[] SearchClasses;
        private ulong NumOfPriorityEquipSet;//可能性のある装備セットの数
        public ulong NumOfCheckedPriorityEquipSet;//走査の終わった装備セットの数
        //private long SearchStartTime;//検索開始した時刻
        Stopwatch SearchingTime = new Stopwatch();//検索している時間
        public Object LockNumOfCheckedPriorityEquipSet = new object();
        private SearchCondition SearchedCondition;
        //private Job SearchedJob;
        //private Sex SearchedSex;
        private Ignore ignore;//除外設定
        public Settings setting;

        public List<EquipSet> AddEquipSetList = new List<EquipSet>();
        private List<EquipSet> SecondAddEquipSetList = new List<EquipSet>();
        //public SortedDictionary<int, SortedDictionary<int, SortedDictionary<int, SortedDictionary<int,Object>>>> FailedList;

        private bool ComboBox_rare_Lock = false;//ComboBox_rareを変更するときにかけるロック

        private NumRangeOrder SPNumOrder = new NumRangeOrder(0, 5);
        private NumRangeOrder BlankEquipNumOrder = new NumRangeOrder(0, 5);
        private Dictionary<string, NumRangeOrder> TypeNumOrderTable = new Dictionary<string, NumRangeOrder>();

        [System.Runtime.InteropServices.DllImport("winmm.dll")]
        private static extern int mciSendString(String command,
           StringBuilder buffer, int bufferSize, IntPtr hwndCallback);


        public EquipSet ESet
        {
            get
            {
                return ESet_value;
            }
            set
            {
                ESet_value = value;
                EquipSetView_condition.ESet = ESet;
                equipSetSkillView_Equip_Condition.ESet = ESet;
            }
        }


        private bool CheckNewVersion()
        {
            DateTime dt = DateTime.Today;
            int now = dt.Year * 10000 + dt.Month * 100 + dt.Day;
            bool ret = false;

            if (now > setting.LastVersionCheckDate)
            {
                try
                {
                    //ダウンロードするファイル
                    string url = Properties.Resources.URL_HOME_PAGE + "/" + Properties.Resources.VERSION_FILE_NAME;

                    //WebRequestの作成
                    System.Net.HttpWebRequest webreq =
                        (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);

                    //サーバーからの応答を受信するためのWebResponseを取得
                    System.Net.HttpWebResponse webres =
                        (System.Net.HttpWebResponse)webreq.GetResponse();

                    //応答データを受信するためのStreamを取得
                    StreamReader sr = new StreamReader(webres.GetResponseStream());

                    int LatestVersion = int.Parse(sr.ReadLine());

                    sr.Close();
                    webres.Close();

                    if (LatestVersion > Properties.Settings.Default.ThisVersion)
                    {
                        UpdateCheckDialog dialog = new UpdateCheckDialog();

                        if (dialog.ShowDialog() == DialogResult.Yes)
                        {
                            System.Diagnostics.Process.Start(Properties.Resources.URL_HOME_PAGE);
                            ret = true;
                        }

                        if (dialog.checkBox1.Checked)
                        {
                            setting.CheckNewVersion = false;
                        }

                    }

                    setting.LastVersionCheckDate = now;

                }
                catch (Exception)
                {
                    return false;
                }
            }

            return ret;
        }

        public MHSX2Form()
        {
            try
            {
                LoadSettings();

                if (setting.CheckNewVersion)
                {
                    if (CheckNewVersion())
                    {
                        string fname = SubFunc.MakeFilePath(Properties.Resources.DNAME_SETTING, Properties.Resources.FNAME_SETTING);
                        FileStream fs = File.Open(fname, FileMode.Create);
                        System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Settings));
                        serializer.Serialize(fs, setting);
                        fs.Close();

                        System.Environment.Exit(0);
                    }
                }

                basedata.Load();


                Process thisProcess = System.Diagnostics.Process.GetCurrentProcess();
                thisProcess.PriorityClass = setting.Priority;
                basedata.UpdateIgnoreInfo(ignore, setting);

                foreach (string str in basedata.TypeList)
                {
                    TypeNumOrderTable.Add(str, new NumRangeOrder(0, 5));
                }

            }
            catch (Exception excep)
            {
                System.Windows.Forms.MessageBox.Show(excep.Message);
                System.Environment.Exit(-1);
            }


            InitializeComponent();





        }

        private void MHSX2Form_Load(object sender, EventArgs e)
        {

            equipSetListView_result.Init(basedata);
            this.Text = "MHSX2 ver " + Properties.Settings.Default.ThisVersion.ToString() + " (Revised by KKyang)";
            comboBox_sex.Items.AddRange(new Sex[] { new Sex(SexType.MAN), new Sex(SexType.WOMAN) });
            switch (setting.sex)
            {
                case SexType.MAN:
                    comboBox_sex.SelectedIndex = 0;
                    break;
                case SexType.WOMAN:
                    comboBox_sex.SelectedIndex = 1;
                    break;
            }


            comboBox_job.Items.AddRange(new Job[] { new Job(JobType.KNIGHT), new Job(JobType.GUNNER), new Job(JobType.COMON) });

            this.comboBox_job.SelectedIndex = 0;


            ComboBox_rare_Lock = true;

            this.comboBox_rare_lower.Items.Add(setting.rare_lower.ToString());
            this.comboBox_rare_upper.Items.Add(setting.rare_upper.ToString());
            this.comboBox_rare_lower.SelectedItem = setting.rare_lower.ToString();
            this.comboBox_rare_upper.SelectedItem = setting.rare_upper.ToString();

            ComboBox_rare_Lock = false;
            comboBox_rare_lower_SelectedIndexChanged(null, null);
            comboBox_rare_upper_SelectedIndexChanged(null, null);

            textBox_def_lower.Text = setting.def_lower.ToString();

            //if (setting.def_upper == int.MaxValue)
            //{
            //    textBox_def_upper.Text = "∞";
            //}
            //else
            //{
            //    textBox_def_upper.Text = setting.def_upper.ToString();
            //}


            checkBox_ignore_item.Checked = setting.IgnoreItem;
            checkBox_ignore_skill.Checked = setting.IgnoreSkill;
            checkBox_ignore_class.Checked = setting.IgnoreClass;

            treeView_skill.LoadBaseData(basedata, setting);

            EquipSetView_condition.ESet = ESet;
            EquipSetView_condition.UpdateData();
            //equipSetSkillView_Equip_Condition.ESet = ESet;
            //equipSetSkillView_Equip_Condition.UpdateData();


            comboBox_priority.Items.Add(System.Diagnostics.ProcessPriorityClass.AboveNormal);
            comboBox_priority.Items.Add(System.Diagnostics.ProcessPriorityClass.Normal);
            comboBox_priority.Items.Add(System.Diagnostics.ProcessPriorityClass.BelowNormal);
            comboBox_priority.Items.Add(System.Diagnostics.ProcessPriorityClass.Idle);

            comboBox_priority.SelectedItem = setting.Priority;


            comboBox_R_Kind.Items.AddRange(BaseData.mDefine.RankLimitKind);
            int kind = setting.RankLimitKind;
            comboBox_R_Kind.SelectedIndex = (int)kind;

            numericUpDown_HRLimit.Value = setting.HRLimit % 1000;
            numericUpDown_HRLimit_ValueChanged(null, null);

            listView_necessity_items.DataSet(basedata, label_costmoney,setting);

            equipSetView_search_result.SetViewers(equipSetSkillView_search_result, listView_necessity_items);
            EquipSetView_condition.SetViewers(equipSetSkillView_Equip_Condition, null);


            comboBox_Rusta.Items.Clear();

            comboBox_Rusta.Items.Add("-");

            for (int i = 0; i < basedata.MaxRustaLebel; i++)//ラスタレベル追加
            {
                comboBox_Rusta.Items.Add((i + 1).ToString());
            }

            comboBox_Rusta.SelectedIndex = 0;
            //comboBox_blank_equip_num.SelectedIndex = 0;

            //comboBox_num_SP.SelectedIndex = 0;

            //前回終了時の豚服を保持
            if (basedata.ClothesDataMap.ContainsKey(setting.PigClothes))
            {
                ESet.PigClothes.Clothes = basedata.ClothesDataMap[setting.PigClothes];
                ESet.PigClothes.isChecked = false;
                EquipSetView_condition.UpdateData();
            }




            LoadSearchHistoryList();

        }

        private void treeView_skill_AfterSelect(object sender, TreeViewEventArgs e)
        {
            switch (e.Node.Level)
            {
                case 1:
                    if (e.Node.Tag.GetType() == typeof(SkillBase))
                    {
                        SkillBase sbase = (SkillBase)e.Node.Tag;
                        AddSkillOptionToView(sbase.OptionTable);
                    }
                    else if (e.Node.Tag.GetType() == typeof(SkillSet)/*Pair<Job, List<SkillOption>>)*/)
                    {
                        SkillSet ss = (SkillSet)e.Node.Tag;
                        //Pair<Job, List<SkillOption>> pair = (Pair<Job, List<SkillOption>>)e.Node.Tag;

                        List<SkillOption> list = new List<SkillOption>();

                        foreach (string str in ss.list)
                        {
                            if (basedata.SkillOptionMap.ContainsKey(str))
                            {
                                list.Add(basedata.SkillOptionMap[str]);
                            }
                        }

                        AddSkillOptionToView(list);
                    }

                    break;
            }
        }

        private void AddSkillOptionToView(List<SkillOption> OptionTable)
        {
            listView_skilloption.BeginUpdate();
            listView_skilloption.Items.Clear();

            bool Selected = false;

            foreach (SkillOption o in OptionTable)
            {
                ListViewItem item = new ListViewItem();
                item.Tag = o;
                item.Text = o.Name;

                if (o.Point < 0)
                {
                    item.ForeColor = Color.Red;
                }


                if (checkBox_ignore_skill.Checked && ignore.skill.Contains(o.Name))
                {
                    item.ForeColor = Color.DarkGray;
                }
                else if (!Selected)
                {
                    Selected = true;
                    item.Selected = true;
                }



                listView_skilloption.Items.Add(item);
            }


            listView_skilloption.EndUpdate();
        }


        private void button_skilladd_Click(object sender, EventArgs e)
        {
            AddSkill();
        }

        private void AddSkill()
        {
            if (listView_skilloption.SelectedItems.Count == 0)
                return;


            SkillOption sopt = (SkillOption)listView_skilloption.SelectedItems[0].Tag;


            AddSkillOptionToSearchConditionView(sopt.SBase, sopt.Point);
        }

        private void AddSkillOptionToSearchConditionView(SkillBase sbase, int Point)
        {
            SkillOption so = sbase.GetOption(Point);

            if (checkBox_ignore_skill.Checked && so != null &&ignore.skill.Contains(so.Name))
                return;

            listView_SearchSkill.AddSkillOption(sbase, Point);

        }

        private void listBox_skill_DoubleClick(object sender, EventArgs e)
        {
            //ダブルクリックは追加と同等
            AddSkill();
        }

        private void button_skill_alldelete_Click(object sender, EventArgs e)
        {
            listView_SearchSkill.Items.Clear();
        }

        private void button_skill_delete_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView_SearchSkill.SelectedItems)
            {
                listView_SearchSkill.Items.Remove(item);
            }

        }

        private void treeView_skill_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Level == 1)
            {
                if (e.Node.Tag.GetType() == typeof(SkillBase))
                {
                    //ダブルクリックは追加と同等
                    AddSkill();
                }
                else if (e.Node.Tag.GetType() == typeof(SkillSet))
                {
                    SkillSet ss = (SkillSet)e.Node.Tag;


                    //if (ss.job != null)
                    //{
                    //    if (ss.job.type == JobType.COMON)
                    //    {
                    //        comboBox_job.SelectedIndex = 2;
                    //    }
                    //    else
                    //        comboBox_job.SelectedIndex = (int)ss.job.type - 1;
                    //}


                    foreach (string str in ss.list)
                    {
                        if (basedata.SkillOptionMap.ContainsKey(str))
                        {
                            SkillOption so = basedata.SkillOptionMap[str];
                            AddSkillOptionToSearchConditionView(so.SBase, so.Point);
                        }
                    }

                }


            }
        }

        private void button_add_Click(object sender, EventArgs e)
        {
            AddSkill();
        }


        private void button_edit_equip_Click(object sender, EventArgs e)
        {
            SearchCondition condition = MakeSearchCondition();

            if (condition == null)
                return;

            EditEquipDialog EditDialog = new EditEquipDialog(basedata, condition, setting);

            if (EditDialog.ShowDialog() == DialogResult.OK)
            {
                SearchCondition cond = EditDialog.MakeSearchCondition();
                ESet = cond.ESet;

                comboBox_sex.SelectedIndex = (int)cond.sex.type - 1;

                comboBox_R_Kind.SelectedIndex = cond.HR_Limit / 1000;
                numericUpDown_HRLimit.Value = cond.HR_Limit % 1000;

                
                comboBox_Rusta.SelectedIndex = cond.RustaLv;

                if (cond.job.type == JobType.COMON)
                {
                    comboBox_job.SelectedIndex = 2;
                }
                else
                    comboBox_job.SelectedIndex = (int)cond.job.type - 1;

                EquipSetView_condition.UpdateData();
                //equipSetSkillView_Equip_Condition.UpdateData();


            }
            else
            {
                basedata.UpdateEquipLevel(HRLimit);
            }

            treeView_skill.LoadBaseData(basedata, setting);
            SaveSettings();
        }

        private SearchCondition MakeSearchCondition()
        {
            SearchCondition condition = new SearchCondition();

            //if (setting.IgnoreClass)
            //{
            //    condition.ignore.Class = new List<string>(ignore.Class);
            //    condition.ignore.Class_Jewelry = new List<string>(ignore.Class_Jewelry);
            //}

            //if (setting.IgnoreItem)
            //{
            //    condition.ignore.Equip = new List<string>(ignore.Equip);
            //    condition.ignore.Jewelry = new List<string>(ignore.Jewelry);
            //    condition.ignore.SkillCuff = new List<string>(ignore.SkillCuff);
            //    condition.ignore.Item = new List<string>(ignore.Item);
            //}

            //if (setting.IgnoreSkill)
            //{
            //    condition.ignore.skill = new List<string>(ignore.skill);
            //}

            condition.ignore = this.ignore;


            condition.ESet = ESet;

            condition.sex = (Sex)comboBox_sex.SelectedItem;
            condition.job = (Job)comboBox_job.SelectedItem;


            condition.HR_Limit = HRLimit;


            condition.isOrderTypeNum = checkBox_TypeNumOrder.Checked;
            if (condition.isOrderTypeNum)
            {

                condition.SP_assign = SPNumOrder;
                condition.BlankEquipNum = BlankEquipNumOrder;


                foreach (KeyValuePair<string, NumRangeOrder> pair in this.TypeNumOrderTable)
                {
                    if (pair.Value.Under > 0 || pair.Value.Upper < 5)
                    {
                        condition.TypeNumOrder.Add(pair.Key, pair.Value);
                    }
                }
            }
            else
            {
                condition.SP_assign = new NumRangeOrder(0, 5);
                condition.BlankEquipNum = new NumRangeOrder(0, 5);

            }



            int.TryParse((string)comboBox_rare_lower.SelectedItem, out condition.rare_lower);
            int.TryParse((string)comboBox_rare_upper.SelectedItem, out condition.rare_upper);

            condition.RustaLv = comboBox_Rusta.SelectedIndex;

            if (condition.rare_upper < condition.rare_lower)
            {
                MessageBox.Show("レア度指定が不正です");
                return null;
            }


            int.TryParse(textBox_def_lower.Text, out condition.defence_lower);

            if (textBox_def_upper.Text == "∞")
                condition.defence_upper = int.MaxValue;
            else
                int.TryParse(textBox_def_upper.Text, out condition.defence_upper);


            if (condition.defence_upper < condition.defence_lower)
            {
                MessageBox.Show("防御力指定が不正です");
                return null;
            }

            condition.SkillPointConditionTable = listView_SearchSkill.GetSkillOptionTable();

            //foreach (ListViewItem item in listView_SearchSkill.Items)
            //{
            //    if (item.Checked)
            //    {
            //        SkillPointCondition point = new SkillPointCondition();
            //        point.Point = int.Parse(item.SubItems[1].Text);
            //        point.SBase = (SkillBase)item.Tag;
            //        condition.SkillPointConditionTable[point.SBase] = point;
            //    }
            //}

            return condition;
        }

        private void listView_equip_condition_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            EquipSetView_condition.UpdateData();
            //equipSetSkillView_Equip_Condition.UpdateData();
        }

        private void 削除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView_SearchSkill.SelectedItems)
            {
                listView_SearchSkill.Items.Remove(item);
            }
        }

        private void 全てチェックToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView_SearchSkill.Items)
            {
                item.Checked = true;
            }
        }

        private void 全てチェックをはずすToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView_SearchSkill.Items)
            {
                item.Checked = false;
            }
        }

        private void comboBox_sex_SelectionChangeCommitted(object sender, EventArgs e)
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
            EquipSetView_condition.UpdateData();
        }

        private void comboBox_job_SelectionChangeCommitted(object sender, EventArgs e)
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
            EquipSetView_condition.UpdateData();
        }


        private void EndedSearch()
        {
            UpdateStatusBar();
            timer1.Enabled = false;
            SearchingTime.Reset();
            groupBox_searchCondition.Enabled = true;
            button_search.Text = "検索開始";
            isSearching = false;
            button_search_stop.Enabled = false;
//            menuStrip1.Enabled = true;
            button_setting.Enabled = true;
            panel_search_history.Enabled = true;

        }

        private void StopSearching()
        {
            //全てのスレッドを強制終了
            foreach (SearchClass t in SearchClasses)
            {
                t.Kill();
            }

            EndedSearch();

            toolTip1.SetToolTip(label_search, "");
            progressBar_search.Value = 0;
        }


        private JewelryDataTag ComperJewelry(JewelryDataTag a, JewelryDataTag b, SortedList<SkillBase, SkillPointCondition> spcond)//aとbと完全下位のものを返す
        {
            if (a.jd.Type != b.jd.Type)
                return null;

            JewelryDataTag bigger = a;
            JewelryDataTag smaller = b;

            for (int i = 0; i < 2; i++)
            {
                if (bigger.jd.Slot > smaller.jd.Slot)//必要スロット数が多い方は上位互換なりえない
                {
                    goto LABEL_CHANGE;
                }

                foreach (SkillPointTag sspt in smaller.SkillPointTags)//ちいさいものが持っているものに対して
                {
                    SkillPointTag bspt = bigger.SearchSkill(sspt.index);

                    if (bspt == null)
                    {
                        if (sspt.Point > 0 || spcond[sspt.sb].UpperPoint != null)//小さい方が大きいものにない有益なものを持っていたらアウト
                        {
                            goto LABEL_CHANGE;
                        }

                        continue;
                    }

                    double dif = (double)sspt.Point / smaller.jd.Slot - (double)bspt.Point / bigger.jd.Slot;

                    if (dif > 0)
                    {
                        goto LABEL_CHANGE;//小さい方が効率の良いものもっててもアウト
                    }

                    if (spcond[bspt.sb].UpperPoint != null && dif != 0)//上限の指定されているもので、値が等しく無い場合は上位互換ではない
                    {
                        goto LABEL_CHANGE;
                    }

                }


                foreach (SkillPointTag bspt in bigger.SkillPointTags)//大きい方がもっているものに対して
                {
                    SkillPointTag sspt = smaller.SearchSkill(bspt.index);

                    if (sspt == null)
                    {
                        if (bspt.Point < 0 || spcond[bspt.sb].UpperPoint != null)//大きい方が小さいものにない有害なものを持っていたらアウト
                        {
                            goto LABEL_CHANGE;
                        }


                        continue;
                    }

                    //大きいものにあるやつはもう上でヒットしてるはずだからいらないはず。。。
                    //if ((double)sspt.Point / smaller.jd.NecessarySlot > (double)bspt.Point / bigger.jd.NecessarySlot)
                    //{
                    //    goto LABEL_CHANGE;//小さい方が効率の良いものもっててもアウト
                    //}
                }




                return smaller;
            LABEL_CHANGE:
                bigger = b;
                smaller = a;
            }

            return null;
        }

        private void BeginSearch()
        {

            ////////////////////////////////////////////////////////
            //デバッグ用
#if DEBUG
            //setting.ThreadNum = 1;
#endif
            ///////////////////////////////////////////////////////

            SearchCondition cond = MakeSearchCondition();

            if (cond == null)
            {
                return;
            }

            if (cond.SkillPointConditionTable.Count == 0)
            {
                MessageBox.Show("スキル条件を入力してください");
                return;
            }


            #region 除外スキルが適正か判断
            if (checkBox_ignore_skill.Checked)
            {
                foreach (string igname in ignore.skill)
                {

                    if (!basedata.SkillOptionMap.ContainsKey(igname))
                    {
                        MessageBox.Show(igname + "という発動スキルは登録されていません。");
                        return;
                    }

                    SkillOption ignoreOpt = (SkillOption)basedata.SkillOptionMap[igname];


                    if (cond.SkillPointConditionTable.ContainsKey(ignoreOpt.SBase))
                    {
                        SkillPointCondition exist = cond.SkillPointConditionTable[ignoreOpt.SBase];
                        if (exist.isIgnore)
                        {
                            if (!SubFunc.CompairSgin(exist.Point, ignoreOpt.Point))
                            {
                                MessageBox.Show("スキル\"" + ignoreOpt.SBase.Name + "\"の除外スキル指定に不備があります");
                                return;
                            }

                            if (Math.Abs(exist.Point) > Math.Abs(ignoreOpt.Point) - 1)//絶対値が小さい方を採用
                            {
                                if (ignoreOpt.Point >= 0)
                                    exist.Point = ignoreOpt.Point - 1;
                                else
                                    exist.Point = ignoreOpt.Point + 1;
                            }
                        }
                        else
                        {
                            if (Math.Abs(exist.Point) < Math.Abs(ignoreOpt.Point) && SubFunc.CompairSgin(exist.Point, ignoreOpt.Point))
                            {
                                if (exist.UpperPoint == null)
                                {
                                    if (ignoreOpt.Point >= 0)
                                        exist.UpperPoint = ignoreOpt.Point - 1;
                                    else
                                        exist.UpperPoint = ignoreOpt.Point + 1;
                                }
                                else
                                {
                                    if (Math.Abs((int)exist.UpperPoint) > Math.Abs(ignoreOpt.Point) - 1)
                                    {
                                        if (ignoreOpt.Point >= 0)
                                            exist.UpperPoint = ignoreOpt.Point - 1;
                                        else
                                            exist.UpperPoint = ignoreOpt.Point + 1;
                                    }
                                }

                            }
                        }
                    }
                    else
                    {
                        SkillPointCondition sp = new SkillPointCondition(ignoreOpt.SBase, ignoreOpt.Point);


                        if (sp.Point >= 0)
                            sp.Point--;
                        else
                            sp.Point++;

                        sp.isIgnore = true;

                        cond.SkillPointConditionTable.Add(sp.SBase, sp);
                    }

                }
            }
            #endregion


            #region GUI変更
            isSearching = true;

            SearchedCondition = cond;

            equipSetView_search_result.ESet = null;
            equipSetView_search_result.UpdateData();
            //equipSetSkillView_search_result.ESet = null;
            //equipSetSkillView_search_result.UpdateData();
            //button_clip.Enabled = false;
            //button_save_favorite.Enabled = false;
            //button_edit_result_equip.Enabled = false;

            panel6.Enabled = false;

            groupBox_searchCondition.Enabled = false;



            panel_search_history.Enabled = false;
            //menuStrip1.Enabled = false;
            button_setting.Enabled = false;
            button_search.Text = "検索中止";
            button_search_stop.Text = "一時中断";
            button_search_stop.Enabled = true;


            equipSetListView_result.ClearItem();

            equipSetSkillView_search_result.Items.Clear();

            listView_necessity_items.Items.Clear();
            label_costmoney.Text = "";

            AddEquipSetList.Clear();
            SecondAddEquipSetList.Clear();

            #endregion


            cond.skillSearchType = SkillSearchType.OR;//常にor検索

            bool[] isOrderd = new bool[(int)EquipKind.NumOfEquipKind];



            #region 装飾品リスト作成
            //可能性のある装飾品を全て取得
            List<JewelryData> PotentialJewelrys = cond.MakeJewelryArray(basedata, setting.IgnoreItem, setting.IgnoreClass);

            List<JewelryDataTag> JewelryDataTags = MakeJewelyDataTagList(cond, PotentialJewelrys.ToArray());

            //JewelryDataTagsから完全下位の装飾品を除外
            RemoveInferiorJewely(JewelryDataTags, cond.SkillPointConditionTable);


            //#region listをソート
            //foreach (DictionaryEntry pair in PlusJewelrys)
            //{
            //    ArrayList[] lists = (ArrayList[])pair.Value;
            //    lists[0].Sort(new JewelryDataTagSorter((int)pair.Key));
            //    lists[1].Sort(new JewelryDataTagSorter((int)pair.Key));
            //}
            //#endregion

            #endregion


            #region SP珠の取り出し

            List<JewelryDataTag> SPJewelyDataTags = new List<JewelryDataTag>();
            for (int i = JewelryDataTags.Count - 1; i >= 0; i--)
            {
                if (JewelryDataTags[i].jd.Type == JewelryType.SP)
                {
                    SPJewelyDataTags.Add(JewelryDataTags[i]);
                    JewelryDataTags.RemoveAt(i);
                }
            }

            #endregion


            #region 必然な装飾品の割り出し

            Dictionary<SkillBase, List<PlusJewelryDataTag>> PlusJewelrys = new Dictionary<SkillBase, List<PlusJewelryDataTag>>();
            List<PlusJewelryDataTag> SingleJewelrys = new List<PlusJewelryDataTag>();

            //listに目的のスキルが+のものを含む装飾品を選出
            foreach (SkillPointCondition sp in cond.SkillPointConditionTable.Values)
            {
                List<PlusJewelryDataTag> Pluslist = new List<PlusJewelryDataTag>();

                bool BeDownedFlag = false;

                foreach (JewelryDataTag jdt in JewelryDataTags)
                {
                    int? pt = jdt.GetSkillPoint(sp.SBase);
                    

                    if (pt != null)
                    {
                        if (pt > 0 || sp.UpperPoint != null)
                        {
                            SkillPointTag spt = new SkillPointTag();
                            spt.Point = (int)pt;
                            spt.sb = sp.SBase;
                            spt.index = cond.SkillPointConditionTable.IndexOfKey(sp.SBase);

                            Pluslist.Add(new PlusJewelryDataTag(jdt, spt));
                        }
                        else if (pt < 0)
                        {
                            BeDownedFlag = true;
                        }
                    }
                }

                Pluslist.Sort();

                switch (Pluslist.Count)
                {
                    case 0://nullを入れておく
                        PlusJewelryDataTag pjdt = new PlusJewelryDataTag(null,new SkillPointTag());
                        pjdt.SpecificPoint.index = cond.SkillPointConditionTable.IndexOfKey(sp.SBase);
                        pjdt.SpecificPoint.sb = sp.SBase;
                        SingleJewelrys.Add(pjdt);

                        PlusJewelrys[sp.SBase] = Pluslist;
                        break;
                    case 1:
                        SingleJewelrys.Add(Pluslist[0]);
                        if (BeDownedFlag)
                            PlusJewelrys[sp.SBase] = Pluslist;
                        break;
                    default:
                        PlusJewelrys[sp.SBase] = Pluslist;
                        break;
                }

            }



            #endregion


            #region PlusJewelryArray作成
            List<PlusJewelryListTag> PlusJewelrysList = new List<PlusJewelryListTag>();

            foreach (KeyValuePair<SkillBase, List<PlusJewelryDataTag>> p in PlusJewelrys)
            {
                PlusJewelrysList.Add(new PlusJewelryListTag(p.Key, cond.SkillPointConditionTable.IndexOfKey(p.Key), p.Value.ToArray()));
            }

            PlusJewelrysList.Sort(delegate(PlusJewelryListTag x, PlusJewelryListTag y)//プラスの装飾品の数が多い順にソート
            {
                return y.Jewelrys.Length - x.Jewelrys.Length;
            });

            PlusJewelryListTag[] PlusJewelrysArray = PlusJewelrysList.ToArray();

            PlusJewelryDataTag[] SinglePlusJewelryArray = SingleJewelrys.ToArray();

            #endregion


            #region スキルカフリスト作成
            JewelryDataTag[][] PotentialSkillCuffTagArrayArray = null;
            if (cond.ESet.PigClothes.Clothes != null && cond.ESet.PigClothes.isChecked)
            {

                List<SkillCuffData> CuffList = cond.MakeSkillCuffList(basedata, setting.IgnoreItem, true, setting.IgnoreClass);

                List<JewelryDataTag> SkillCuffDataTags = MakeJewelyDataTagList(cond, CuffList.ToArray());

                //完全下位カフ削除
                RemoveInferiorJewely(SkillCuffDataTags, cond.SkillPointConditionTable);

                PotentialSkillCuffTagArrayArray = MakeSplitedJewelyDataTagArrays(SkillCuffDataTags, JewelryType.SkillCuff);

            }
            else
                PotentialSkillCuffTagArrayArray = new JewelryDataTag[2][] { new JewelryDataTag[0], new JewelryDataTag[0] };
            #endregion


            #region 装備品リスト作成
            EquipmentDataTag[][] PotentialEquipTagArrayArray = new EquipmentDataTag[(int)EquipKind.NumOfEquipKind][];

            Dictionary<SkillBase, List<PlusJewelryDataTag>> tmpDict = new Dictionary<SkillBase, List<PlusJewelryDataTag>>(PlusJewelrys);
            foreach (PlusJewelryDataTag pjdt in SingleJewelrys)
            {
                if (!tmpDict.ContainsKey(pjdt.SpecificPoint.sb))
                {
                    List<PlusJewelryDataTag> list = new List<PlusJewelryDataTag>();
                    list.Add(pjdt);
                    tmpDict.Add(pjdt.SpecificPoint.sb, list);
                }
            }


            for (EquipKind i = EquipKind.Weapon; i != EquipKind.NumOfEquipKind; i++)
            {
                isOrderd[(int)i] = cond.ESet[i].isChecked && cond.ESet[i].EquipData != null;
                List<EquipmentData> tmplist = cond.MakeEquipArray(basedata, i, true, setting.IgnoreItem, setting.IgnoreClass);

                if (isOrderd[(int)i] == false)//指定が無いならば
                {
                    EquipmentData data = new EquipmentData();
                    data.Kind = i;
                    data.Element.Clear();
                    data.Rare = 1;
                    data.WearableJob = JobType.COMON;
                    data.WearableSex = SexType.COMON;
                    data.SkillPointList = new SkillPoint[0];
                    data.Def = 0;
                    data.Level = 1;
                    data.LevelList.Add(new Level());
                    data.LevelList[0].Money = 0;
                    data.LevelList[0].Def = 0;
                    data.LevelList[0].GetableHR = 0;
                    if (i == EquipKind.Weapon)
                    {
                        data.Name = "武器スロットなし";
                        data.Slot = 0;
                        data.LevelList[0].Slot = 0;
                    }
                    else
                    {
                        //スロット3のみの装備を追加
                        data.Name = "スロット3装備";
                        data.Slot = 3;
                        data.LevelList[0].Slot = 3;
                    }
                    tmplist.Add(data);
                }


                int count = tmplist.Count;

                List<EquipmentDataTag> taglist = new List<EquipmentDataTag>();

                for (int j = 0; j < count; j++)
                {
                    EquipmentDataTag tag = new EquipmentDataTag();

                    tag.equipdata = (EquipmentData)tmplist[j];

                    taglist.Add(tag);

                    List<SkillPointTag> list = new List<SkillPointTag>();

                    foreach (SkillPoint sp in tag.equipdata.SkillPointList)
                    {
                        if (cond.SkillPointConditionTable.ContainsKey(sp.SBase))
                        {
                            SkillPointTag spt = new SkillPointTag();
                            spt.index = cond.SkillPointConditionTable.IndexOfKey(sp.SBase);
                            SkillPointCondition skillpoint = cond.SkillPointConditionTable[sp.SBase];

                            if (skillpoint.isIgnore)
                            {
                                if (skillpoint.Point >= 0)//不要なスキルの負方向へ正規化
                                    spt.Point = -sp.Point;
                                else
                                    spt.Point = +sp.Point;
                            }
                            else
                            {
                                if (skillpoint.Point >= 0)//必要なスキルの正方向へ正規化
                                    spt.Point = sp.Point;
                                else
                                    spt.Point = -sp.Point;
                            }
                            spt.sb = sp.SBase;
                            list.Add(spt);
                        }



                    }


                    tag.SkillPointTags = (SkillPointTag[])list.ToArray();
                }




                #region 無駄なequipを取り除く

                OptimizeEquipList(taglist, cond.SkillPointConditionTable, tmpDict, cond);


                //装備品の互換ツリーをデバッグ出力。
                //foreach (EquipmentDataTag tag in taglist)
                //{
                //    Debug.WriteLine("-----------------------------------------");
                //    SearchClass.PrintEquipTagTree(new EquipTagTreeNode(tag), "");
                //    Debug.WriteLine("-----------------------------------------");
                //}



                #endregion

                count = taglist.Count;

                if (cond.BlankEquipNum.Under > 0 && i != EquipKind.Weapon)
                {
                    EquipmentDataTag tag = new EquipmentDataTag();
                    tag.equipdata = basedata.EquipDataMap[(int)i][Properties.Resources.BLANK_EQUIP_NAME];
                    tag.Def = 0;
                    tag.Level = 1;
                    tag.Slot = 0;
                    tag.SkillPointTags = new SkillPointTag[0];
                    taglist.Add(tag);
                    count++;
                }

                PotentialEquipTagArrayArray[(int)i] = new EquipmentDataTag[count];//配列化
                for (int j = 0; j < count; j++)
                {
                    PotentialEquipTagArrayArray[(int)i][j] = taglist[j];
                }

            }
            #endregion


            #region 検索スレッドの作成開始

            //FailedList = new SortedDictionary<int, SortedDictionary<int, SortedDictionary<int, SortedDictionary<int,Object>>>>();
            NumOfCheckedPriorityEquipSet = 0;
            NumOfPriorityEquipSet = 1;
            //総パターン数を算出
            for (int i = 0; i < (int)EquipKind.NumOfEquipKind; i++)
            {
                if (isOrderd[i] == false)//指定無しなら
                {
                    int count = PotentialEquipTagArrayArray[i].Length;
                    if (count != 0)
                        NumOfPriorityEquipSet *= (uint)count;
                }
            }


            SearchClasses = new SearchClass[setting.ThreadNum];
            for (int i = 0; i < setting.ThreadNum; i++)
            {
                SearchClasses[i] = new SearchClass(this, cond);

                SearchClasses[i].ThreadNo = i;
                SearchClasses[i].isOrderd = isOrderd;
                SearchClasses[i].PotentialEquipTagArrayArray = PotentialEquipTagArrayArray;
                SearchClasses[i].PotentialSkillCuffTagArrayArray = PotentialSkillCuffTagArrayArray;
                SearchClasses[i].SPJewelryDataTags = SPJewelyDataTags.ToArray();
                SearchClasses[i].PlusJewelyDataTags = PlusJewelrysArray;
                SearchClasses[i].SinglePlusJewelryDataTags = SinglePlusJewelryArray;

            }


            bool Sepalated = false;
            for (EquipKind i = EquipKind.Weapon; i < EquipKind.NumOfEquipKind; i++)
            {
                if (isOrderd[(int)i] != false)
                    continue;

                if (Sepalated == false && PotentialEquipTagArrayArray[(int)i].Length >= setting.ThreadNum)
                {
                    Sepalated = true;
                    int rest = (int)(PotentialEquipTagArrayArray[(int)i].Length % setting.ThreadNum);
                    int var = (int)(PotentialEquipTagArrayArray[(int)i].Length / setting.ThreadNum);
                    int begin = 0;
                    int end = var;

                    for (int j = 0; j < setting.ThreadNum; j++)
                    {
                        if (rest > j)//出来るだけ均等に分配
                            end += 1;

                        SearchClasses[j].SearchBeginPoint[(int)i] = begin;
                        SearchClasses[j].SearchEndPoint[(int)i] = end;

                        begin = end;
                        end = begin + var;
                    }
                }
                else
                {
                    for (int j = 0; j < setting.ThreadNum; j++)
                    {
                        SearchClasses[j].SearchBeginPoint[(int)i] = 0;
                        SearchClasses[j].SearchEndPoint[(int)i] = PotentialEquipTagArrayArray[(int)i].Length;
                    }
                }
            }



            if (Sepalated == false)
            {
                //分割する必要なくねってことで
                SearchClasses = new SearchClass[] { SearchClasses[0] };
            }

            foreach (SearchClass sc in SearchClasses)
            {
                sc.Start();
            }
            #endregion

            //SearchStartTime = DateTime.Now.Ticks;

            SearchingTime.Reset();
            SearchingTime.Start();

            timer1.Start();
        }

        //private static void SetParentAndNext(EquipmentDataTag tag)
        //{
        //    for (int k = 0; k < tag.BackwardEquips.Length; k++)
        //    {
        //        tag.BackwardEquips[k].Parent = tag;

        //        if (k != tag.BackwardEquips.Length - 1)
        //            tag.BackwardEquips[k].Next = tag.BackwardEquips[k + 1];

        //        SetParentAndNext(tag.BackwardEquips[k]);

        //    }
        //}

        private static JewelryDataTag[][] MakeSplitedJewelyDataTagArrays(List<JewelryDataTag> JewelryDataTags, JewelryType type)
        {
            List<JewelryDataTag>[] PotentialJewelryTagArrayList;

            if (type == JewelryType.Normal)
            {
                PotentialJewelryTagArrayList = new List<JewelryDataTag>[] { new List<JewelryDataTag>(), new List<JewelryDataTag>(), new List<JewelryDataTag>(), new List<JewelryDataTag>() };//0-sp 1～3-nスロット必要な装飾品
                #region スロット数別に区分け
                foreach (JewelryDataTag jdt in JewelryDataTags)
                {
                    if (jdt.jd.Type == JewelryType.SP)
                    {
                        PotentialJewelryTagArrayList[0].Add(jdt);
                    }
                    else
                    {
                        PotentialJewelryTagArrayList[jdt.jd.Slot].Add(jdt);
                    }
                }
                #endregion

                JewelryDataTag[][] PotentialJewelryTagArrayArray = new JewelryDataTag[4][];

                for (int i = 0; i < 4; i++)
                {
                    PotentialJewelryTagArrayArray[i] = PotentialJewelryTagArrayList[i].ToArray();
                }
                return PotentialJewelryTagArrayArray;
            }
            else if (type == JewelryType.SkillCuff)
            {
                #region スロット数別に区分け
                PotentialJewelryTagArrayList = new List<JewelryDataTag>[] { new List<JewelryDataTag>(), new List<JewelryDataTag>() };
                foreach (JewelryDataTag jdt in JewelryDataTags)
                {
                    PotentialJewelryTagArrayList[jdt.jd.Slot - 1].Add(jdt);
                }
                #endregion

                JewelryDataTag[][] PotentialJewelryTagArrayArray = new JewelryDataTag[2][];

                for (int i = 0; i < 2; i++)
                {
                    PotentialJewelryTagArrayArray[i] = PotentialJewelryTagArrayList[i].ToArray();
                }
                return PotentialJewelryTagArrayArray;
            }
            else
                throw new Exception("予期しない呼び出し MakeSplitedJewelyDataTagArrays");


        }

        private void RemoveInferiorJewely(List<JewelryDataTag> JewelryDataTags, SortedList<SkillBase, SkillPointCondition> spcond)
        {
            for (int i = 0; i < JewelryDataTags.Count - 1; )
            {
                for (int j = i + 1; j < JewelryDataTags.Count; )
                {
                    JewelryDataTag a = (JewelryDataTag)JewelryDataTags[i];
                    JewelryDataTag b = (JewelryDataTag)JewelryDataTags[j];
                    JewelryDataTag delete = ComperJewelry(a, b, spcond);

                    if (delete != null)
                    {
                        JewelryDataTags.Remove(delete);
                        if (delete == a)
                        {//i番目を削除
                            goto LABEL_NEXT;
                        }
                        else
                        {//j番目を削除
                            continue;
                        }
                    }
                    else
                        j++;
                }
                i++;
            LABEL_NEXT: ;
            }

            Dictionary<SkillBase, List<PlusJewelryDataTag>> PlusJewelrys = new Dictionary<SkillBase, List<PlusJewelryDataTag>>();

            #region 暫定のスキル種別のリストを作成
            foreach (SkillPointCondition sp in spcond.Values)
            {
                List<PlusJewelryDataTag> Pluslist = new List<PlusJewelryDataTag>();


                foreach (JewelryDataTag jdt in JewelryDataTags)
                {
                    if (jdt.jd.Type != JewelryType.Normal)
                        continue;


                    int? pt = jdt.GetSkillPoint(sp.SBase);

                    if (pt != null)
                    {
                        if (pt > 0 || sp.UpperPoint != null)
                        {
                            SkillPointTag spt = new SkillPointTag();
                            spt.Point = (int)pt;
                            spt.sb = sp.SBase;
                            spt.index = spcond.IndexOfKey(sp.SBase);

                            Pluslist.Add(new PlusJewelryDataTag(jdt, spt));
                        }
                    }
                }

                Pluslist.Sort();

                PlusJewelrys[sp.SBase] = Pluslist;


            }

            #endregion


            for (int i = 0; i < JewelryDataTags.Count; )
            {
                JewelryDataTag tag = JewelryDataTags[i];

                if (tag.jd.Type != JewelryType.Normal)
                {
                    i++;
                    continue;
                }

                if(tag.jd.Slot > 1)
                {
                    int slotdiff = tag.jd.Slot;

                    int[] Pointdiff = new int[spcond.Count];

                    foreach (SkillPointTag ptag in tag.SkillPointTags)
                    {
                        Pointdiff[ptag.index] -= ptag.Point;
                    }

                    if (JudgeUpwardUsingJewelry(slotdiff, Pointdiff, spcond.Values, PlusJewelrys, true,tag.jd))
                    {//この装飾品は他の２つ以上の装飾品によって代替する事ができる。
                        JewelryDataTags.RemoveAt(i);
                        continue;
                    }

                }

                i++;
            }

        }



        private static List<JewelryDataTag> MakeJewelyDataTagList(SearchCondition cond, JewelryData_base[] PotentialJewelrys)
        {
            List<JewelryDataTag> JewelryDataTags = new List<JewelryDataTag>();

            #region JewelryDataTagsの構築
            foreach (JewelryData_base jd in PotentialJewelrys)
            {
                JewelryDataTag jdt = new JewelryDataTag();
                jdt.jd = jd;

                List<SkillPointTag> list = new List<SkillPointTag>();

                foreach (SkillPoint sp in jd.SkillList)
                {
                    if (cond.SkillPointConditionTable.ContainsKey(sp.SBase))
                    {
                        SkillPointTag tag = new SkillPointTag();
                        tag.index = cond.SkillPointConditionTable.IndexOfKey(sp.SBase);

                        SkillPointCondition skillpoint = cond.SkillPointConditionTable[sp.SBase];
                        if (skillpoint.isIgnore)
                        {
                            if (skillpoint.Point >= 0)//-方向に正規化
                                tag.Point = -sp.Point;
                            else
                                tag.Point = sp.Point;
                        }
                        else
                        {
                            if (skillpoint.Point >= 0)//+方向に正規化
                                tag.Point = sp.Point;
                            else
                                tag.Point = -sp.Point;
                        }
                        tag.sb = sp.SBase;
                        list.Add(tag);
                    }


                }

                jdt.SkillPointTags = list.ToArray();

                JewelryDataTags.Add(jdt);
            }


            #endregion
            return JewelryDataTags;
        }



        //装備リストの最適化
        private void OptimizeEquipList(List<EquipmentDataTag> list, SortedList<SkillBase, SkillPointCondition> spcond, Dictionary<SkillBase, List<PlusJewelryDataTag>> PlusJewelrys, SearchCondition cond)
        {
            bool isIgnoreDef = cond.isIgnoreDef;

            for (int j = 0; j < list.Count - 1; j++)
            {
                EquipmentDataTag a = (EquipmentDataTag)list[j];


                for (int k = j + 1; k < list.Count; )
                {
                    EquipmentDataTag b = (EquipmentDataTag)list[k];

                    if (a.equipdata.isSP != b.equipdata.isSP)//SPはSP同士の比較のみ
                    {
                        k++;
                        continue;
                    }

                    if (cond.isOrderTypeNum && a.equipdata.Type != b.equipdata.Type)
                    {
                        if (cond.TypeNumOrder.ContainsKey(a.equipdata.Type) || cond.TypeNumOrder.ContainsKey(b.equipdata.Type))
                        {
                            k++;
                            continue;
                        }
                    }


                    EquipmentDataTag bigger = null, smaller = null;


                    if (a.Slot > b.Slot)
                    {
                        bigger = a;
                        smaller = b;
                    }
                    else if (b.Slot > a.Slot)
                    {

                        bigger = b;
                        smaller = a;
                    }

                    bool retry = false;

                    if (bigger == null)
                    {//スロット数では上下がきまらない場合

                        if (isIgnoreDef)
                            retry = true;//防御値を無視する場合ひっくり返してもう一回やる


                        if (a.Def > b.Def)
                        {
                            bigger = a;
                            smaller = b;
                        }
                        else if (a.Def < b.Def)
                        {
                            bigger = b;
                            smaller = a;
                        }
                        else
                        {//スロットも防御値も一緒
                            bigger = a;
                            smaller = b;
                            retry = true;
                        }
                    }
                    else
                    {
                        if (!isIgnoreDef)
                        {
                            if (bigger.Def < smaller.Def)
                            {
                                k++;
                                continue;
                            }
                        }
                    }



                    int[] PointDiff = new int[spcond.Count];


                    foreach (SkillPointTag tag in bigger.SkillPointTags)
                    {
                        PointDiff[tag.index] += tag.Point;
                    }

                    foreach (SkillPointTag tag in smaller.SkillPointTags)
                    {
                        PointDiff[tag.index] -= tag.Point;
                    }

                    bool flag = true;
                    for (int i = 0; i < PointDiff.Length; i++)
                    {
                        //if (spcond.Values[i].UpperPoint == null)
                        //{
                            if (PointDiff[i] < 0)
                            {
                                flag = false;
                                break;
                            }
                        //}
                        //else
                        //{
                        //    if (PointDiff[i] != 0)
                        //    {
                        //        flag = false;
                        //        break;
                        //    }
                        //}
                    }

                    if (flag == true)
                    {
                        if (smaller == a)
                        {
                            list.RemoveAt(j);
                            k = j + 1;
                            a = (EquipmentDataTag)list[j];
                        }
                        else
                        {
                            list.RemoveAt(k);
                        }

                        //下位を上位の配下に
                        bigger.BackwardEquipsTmp.Add(smaller);

                        goto LABELE_B;
                    }



                    if (retry)
                    {//smal big 逆の可能性
                        flag = true;
                        for (int i = 0; i < PointDiff.Length; i++)//すべてマイナスand 上限指定の場合は等しいならよし
                        {
                            //if (spcond.Values[i].UpperPoint == null)
                            //{
                                if (PointDiff[i] > 0)
                                {
                                    flag = false;
                                    break;
                                }
                            //}
                            //else
                            //{
                            //    if (PointDiff[i] != 0)
                            //    {
                            //        flag = false;
                            //        break;
                            //    }
                            //}
                        }


                        if (flag == true)
                        {
                            if (bigger == a)//bigger見てることに注意
                            {
                                list.RemoveAt(j);
                                k = j + 1;
                                a = (EquipmentDataTag)list[j];
                            }
                            else
                            {
                                list.RemoveAt(k);
                            }


                            //下位を上位の配下に
                            smaller.BackwardEquipsTmp.Add(bigger);


                            goto LABELE_B;
                        }
                    }

                    #region 強度の絞り込み
                    //ここに来た時点で少なくともbigger.slot>=smal.slot

                    if (bigger.Slot > smaller.Slot)
                    {
                        int slotdiff = bigger.Slot - smaller.Slot;

                        Boolean isBiger = false;

                        isBiger = JudgeUpwardUsingJewelry(slotdiff,PointDiff,spcond.Values,PlusJewelrys,false,null);
                        

                        if (isBiger == true)
                        {
                            if (smaller == a)
                            {
                                list.RemoveAt(j);
                                k = j + 1;
                                a = (EquipmentDataTag)list[j];
                            }
                            else
                            {
                                list.RemoveAt(k);
                            }

                            //下位を上位の配下に
                            bigger.BackwardEquipsTmp.Add(smaller);

                           // Debug.WriteLine(bigger.ToString() + " > " + smaller.ToString() + " diff " + slotdiff.ToString());

                            goto LABELE_B;

                        }

                    }

                    #endregion

                    k++;
                LABELE_B: ;
                }
            }


            foreach (EquipmentDataTag tag in list)
            {
                //int x = countbackwardtmp(tag);
                OptimizeEquipList(tag.BackwardEquipsTmp, spcond, PlusJewelrys, cond);
                tag.BackwardEquips = tag.BackwardEquipsTmp.ToArray();

                //x = countbackward(tag);

                tag.BackwardEquipsTmp = null;
            }

        }

        //他の装飾品を考慮して不要な装飾品を省く。
        /* 例えば
         * 食事スキルと達人スキルのみが要求されている場合
         * ・レウスヘルム スロット2 達人4 
         * ・ボーンヘルム スロット3 食事4
         * を比べた時
         * 仙人珠G(達人5)をボーンヘルムにはめ込むと
         * レウスヘルムの上位装備となり得るため
         * レウスヘルムはボーンヘルムよりも完全下位の装備と見ることが出来る
         */
        private bool JudgeUpwardUsingJewelry(int slotdiff,int[] PointDiff,  IList<SkillPointCondition> spcond, Dictionary<SkillBase, List<PlusJewelryDataTag>> PlusJewelrys,bool checkUpper,JewelryData_base ignoreJewelry)
        {
            List<JewelryDataTag>[] tmpUseJewerlys = new List<JewelryDataTag>[3];
            JewelryDataTag[][] UseJewerlys = new JewelryDataTag[3][];

            for (int i = 0; i < 3; i++)
            {
                tmpUseJewerlys[i] = new List<JewelryDataTag>();
            }


            for (int i = 0; i < PointDiff.Length; i++)
            {
                SkillBase sb = spcond[i].SBase;
                foreach (PlusJewelryDataTag pjdt in PlusJewelrys[sb])
                {
                    if (pjdt.jdt == null || pjdt.jdt.jd == ignoreJewelry)
                        continue;

                    if (pjdt.jdt.jd.Slot <= slotdiff && !tmpUseJewerlys[pjdt.jdt.jd.Slot - 1].Contains(pjdt.jdt))
                    {
                        tmpUseJewerlys[pjdt.jdt.jd.Slot - 1].Add(pjdt.jdt);
                    }
                }
            }


            for (int i = 0; i < 3; i++)
            {
                UseJewerlys[i] = tmpUseJewerlys[i].ToArray();
            }


            #region 装飾品を考慮して上位互換をチェックする


            for (int hoge = slotdiff; hoge > 0; hoge--)
            {
                int[] SlotTypeArray = new int[3];
                int[] tmpSlotTypeArray = new int[3];
                int[][] JewelryUseCount = new int[3][];

                for (int i = 0; i < 3; i++)
                    JewelryUseCount[i] = new int[UseJewerlys[i].Length];


                SlotTypeArray[hoge - 1] = 1;


                #region スロット数がn必要な装飾品が無い場合、n個の列を分裂させる


                if (UseJewerlys[2].Length == 0 && SlotTypeArray[2] > 0)//スロット3の装飾品が無ければ分解
                {
                    SlotTypeArray[1] += SlotTypeArray[2];
                    SlotTypeArray[0] += SlotTypeArray[2];
                    SlotTypeArray[2] = 0;
                }

                if (UseJewerlys[1].Length == 0 && SlotTypeArray[1] > 0)//スロット2の装飾品が無ければ分解
                {
                    SlotTypeArray[0] += SlotTypeArray[1] * 2;
                    SlotTypeArray[1] = 0;
                }

                #endregion


                #region 装飾品を総あたりする

                int[] JewelrysSkillPoint = new int[spcond.Count];
                int[] RestPoint = new int[spcond.Count];

                for (int i = 0; i < RestPoint.Length; i++)
                {
                    RestPoint[i] = -PointDiff[i];
                }



                tmpSlotTypeArray[0] = SlotTypeArray[0];
                tmpSlotTypeArray[1] = SlotTypeArray[1];
                tmpSlotTypeArray[2] = SlotTypeArray[2];


                #region 通常の装飾品に対して総当たり
                do
                {//装飾品をはめるパターンを変える（スロット３について）


                    do
                    {

                        #region 古い検索コード (早い）

                        #region スロットが固定されたので総当たり
                        #region スロット3の装飾品候補リストの最後を限界まで追加
                        if (UseJewerlys[2].Length > 0)
                        {
                            JewelryUseCount[2][UseJewerlys[2].Length - 1] = tmpSlotTypeArray[2];
                            foreach (SkillPointTag spt in UseJewerlys[2][UseJewerlys[2].Length - 1].SkillPointTags)//装飾品の持っているスキルポイントの全ての中から
                            {
                                JewelrysSkillPoint[spt.index] += spt.Point * tmpSlotTypeArray[2];

                            }
                        }

                        bool finish;
                        int sum;

                        #endregion
                        do//スロット３
                        {
                            #region スロット2の装飾品候補リストの最後を限界まで追加
                            if (UseJewerlys[1].Length > 0)
                            {
                                JewelryUseCount[1][UseJewerlys[1].Length - 1] = tmpSlotTypeArray[1];
                                foreach (SkillPointTag spt in UseJewerlys[1][UseJewerlys[1].Length - 1].SkillPointTags)//装飾品の持っているスキルポイントの全ての中から
                                {
                                    JewelrysSkillPoint[spt.index] += spt.Point * tmpSlotTypeArray[1];

                                }

                            }

                            #endregion
                            do
                            {//スロット２

                                #region スロット1の装飾品候補リストの最後を限界まで追加
                                if (UseJewerlys[0].Length > 0)
                                {
                                    JewelryUseCount[0][UseJewerlys[0].Length - 1] = tmpSlotTypeArray[0];
                                    foreach (SkillPointTag spt in UseJewerlys[0][UseJewerlys[0].Length - 1].SkillPointTags)//装飾品の持っているスキルポイントの全ての中から
                                    {
                                        JewelrysSkillPoint[spt.index] += spt.Point * tmpSlotTypeArray[0];

                                    }
                                }
                                #endregion

                                do
                                {//スロット１

                                    #region 条件を満たしていれば追加
                                    bool enough = true;
                                    for (int i = 0; i < RestPoint.Length; i++)
                                    {
                                        int dif = RestPoint[i] - JewelrysSkillPoint[i];


                                        if (checkUpper == false)
                                        {//上限を考慮しない上位判定
                                            if (dif > 0)
                                            {
                                                enough = false;
                                                break;
                                            }
                                        }
                                        else
                                        {//上限を考慮する場合は、上限指定されているポイントは同値でなければならない

                                            if (spcond[i].UpperPoint == null)
                                            {
                                                if (dif > 0)
                                                {
                                                    enough = false;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                if (dif != 0)
                                                {
                                                    enough = false;
                                                    break;
                                                }
                                            }
                                        }


                                    }

                                    if (enough == true)
                                    {
                                        return true;
                                    }
                                    #endregion


                                    #region スロット１の装飾品を総当たり
                                    finish = true;
                                    if (tmpSlotTypeArray[0] > 0)
                                    {
                                        if (UseJewerlys[0].Length == 1)
                                        {//一個の場合は装飾品を外すだけ
                                            if (JewelryUseCount[0][0] > 0)
                                            {
                                                foreach (SkillPointTag spt in UseJewerlys[0][0].SkillPointTags)//装飾品の持っているスキルポイントの全ての中から
                                                {
                                                    JewelrysSkillPoint[spt.index] -= spt.Point * JewelryUseCount[0][0];

                                                }
                                                JewelryUseCount[0][0] = 0;
                                            }
                                        }
                                        else
                                        {
                                            sum = 0;
                                            for (int c = 1; c < UseJewerlys[0].Length; c++)
                                            {
                                                if (JewelryUseCount[0][c] > 0)
                                                {
                                                    foreach (SkillPointTag spt in UseJewerlys[0][c].SkillPointTags)//装飾品の持っているスキルポイントの全ての中から
                                                    {
                                                        JewelrysSkillPoint[spt.index] -= spt.Point;

                                                    }
                                                    JewelryUseCount[0][c]--;
                                                    JewelryUseCount[0][c - 1] += sum + 1;

                                                    foreach (SkillPointTag spt in UseJewerlys[0][c - 1].SkillPointTags)//装飾品の持っているスキルポイントの全ての中から
                                                    {
                                                        JewelrysSkillPoint[spt.index] += spt.Point * (sum + 1);
                                                    }
                                                    finish = false;
                                                    break;
                                                }
                                                else if (c == 1 && JewelryUseCount[0][0] > 0)
                                                {
                                                    foreach (SkillPointTag spt in UseJewerlys[0][0].SkillPointTags)//装飾品の持っているスキルポイントの全ての中から
                                                    {
                                                        JewelrysSkillPoint[spt.index] -= spt.Point * JewelryUseCount[0][0];

                                                    }

                                                    sum += JewelryUseCount[0][0];
                                                    JewelryUseCount[0][0] = 0;
                                                }
                                            }
                                        }
                                    }
                                    #endregion


                                } while (!finish);


                                #region スロット2の装飾品を総当たり
                                finish = true;
                                if (tmpSlotTypeArray[1] > 0)
                                {

                                    if (UseJewerlys[1].Length == 1)
                                    {
                                        if (JewelryUseCount[1][0] > 0)
                                        {//一個の場合は装飾品を外すだけ
                                            foreach (SkillPointTag spt in UseJewerlys[1][0].SkillPointTags)//装飾品の持っているスキルポイントの全ての中から
                                            {
                                                JewelrysSkillPoint[spt.index] -= spt.Point * JewelryUseCount[1][0];

                                            }

                                            JewelryUseCount[1][0] = 0;
                                        }
                                    }
                                    else
                                    {
                                        sum = 0;
                                        for (int c = 1; c < UseJewerlys[1].Length; c++)
                                        {
                                            if (JewelryUseCount[1][c] > 0)
                                            {
                                                foreach (SkillPointTag spt in UseJewerlys[1][c].SkillPointTags)//装飾品の持っているスキルポイントの全ての中から
                                                {
                                                    JewelrysSkillPoint[spt.index] -= spt.Point;

                                                }
                                                JewelryUseCount[1][c]--;
                                                JewelryUseCount[1][c - 1] += sum + 1;

                                                foreach (SkillPointTag spt in UseJewerlys[1][c - 1].SkillPointTags)//装飾品の持っているスキルポイントの全ての中から
                                                {
                                                    JewelrysSkillPoint[spt.index] += spt.Point * (sum + 1);
                                                }
                                                finish = false;
                                                break;
                                            }
                                            else if (c == 1 && JewelryUseCount[1][0] > 0)
                                            {
                                                foreach (SkillPointTag spt in UseJewerlys[1][0].SkillPointTags)//装飾品の持っているスキルポイントの全ての中から
                                                {
                                                    JewelrysSkillPoint[spt.index] -= spt.Point * JewelryUseCount[1][0];

                                                }

                                                sum += JewelryUseCount[1][0];
                                                JewelryUseCount[1][0] = 0;
                                            }
                                        }
                                    }
                                }
                                #endregion

                            } while (!finish);

                            #region スロット3の装飾品を総当たり
                            finish = true;
                            if (tmpSlotTypeArray[2] > 0)
                            {

                                if (UseJewerlys[2].Length == 1)
                                {//一個の場合は装飾品を外すだけ
                                    if (JewelryUseCount[2][0] > 0)
                                    {
                                        foreach (SkillPointTag spt in UseJewerlys[2][0].SkillPointTags)//装飾品の持っているスキルポイントの全ての中から
                                        {
                                            JewelrysSkillPoint[spt.index] -= spt.Point * JewelryUseCount[2][0];

                                        }

                                        JewelryUseCount[2][0] = 0;
                                    }
                                }
                                else
                                {
                                    sum = 0;
                                    for (int c = 1; c < UseJewerlys[2].Length; c++)
                                    {
                                        if (JewelryUseCount[2][c] > 0)
                                        {
                                            foreach (SkillPointTag spt in UseJewerlys[2][c].SkillPointTags)//装飾品の持っているスキルポイントの全ての中から
                                            {
                                                JewelrysSkillPoint[spt.index] -= spt.Point;

                                            }
                                            JewelryUseCount[2][c]--;
                                            JewelryUseCount[2][c - 1] += sum + 1;

                                            foreach (SkillPointTag spt in UseJewerlys[2][c - 1].SkillPointTags)//装飾品の持っているスキルポイントの全ての中から
                                            {
                                                JewelrysSkillPoint[spt.index] += spt.Point * (sum + 1);
                                            }
                                            finish = false;
                                            break;
                                        }
                                        else if (c == 1 && JewelryUseCount[2][0] > 0)
                                        {
                                            foreach (SkillPointTag spt in UseJewerlys[2][0].SkillPointTags)//装飾品の持っているスキルポイントの全ての中から
                                            {
                                                JewelrysSkillPoint[spt.index] -= spt.Point * JewelryUseCount[2][0];

                                            }

                                            sum += JewelryUseCount[2][0];
                                            JewelryUseCount[2][0] = 0;
                                        }
                                    }
                                }
                            }
                            #endregion

                        } while (!finish);

                        #endregion

                        #endregion

                        #region 装飾品をはめるパターンを変える（スロット２について）
                        if (tmpSlotTypeArray[1] > 0)
                        {
                            tmpSlotTypeArray[1]--;
                            tmpSlotTypeArray[0] += 2;
                        }
                        else
                            break;
                        #endregion
                    } while (true);

                    #region 装飾品をはめるパターンを変える（スロット３について）
                    if (SlotTypeArray[2] > 0)
                    {
                        SlotTypeArray[2]--;
                        SlotTypeArray[1]++;
                        SlotTypeArray[0]++;

                        tmpSlotTypeArray[0] = SlotTypeArray[0];
                        tmpSlotTypeArray[1] = SlotTypeArray[1];
                        tmpSlotTypeArray[2] = SlotTypeArray[2];
                    }
                    else
                        break;
                    #endregion
                } while (true);
                #endregion

                #endregion

            }

            #endregion

            return false;
        }

#if DEBUG
        int countbackward(EquipmentDataTag tag)
        {
            int x = tag.BackwardEquips.Length;

            foreach (EquipmentDataTag t in tag.BackwardEquips)
            {
                x += countbackward(t);
            }

            return x;
        }

        int countbackwardtmp(EquipmentDataTag tag)
        {
            int x = tag.BackwardEquipsTmp.Count;

            foreach (EquipmentDataTag t in tag.BackwardEquipsTmp)
            {
                x += countbackwardtmp(t);
            }

            return x;
        }
#endif

        private void button_search_Click(object sender, EventArgs e)
        {
            if (!isSearching)
            {
                BeginSearch();
            }
            else
            {
                if (MessageBox.Show("検索を中止しますか？", "中止確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    StopSearching();
                }
            }
        }

        private void UpdateStatusBar()
        {

            ulong pasent = 100 * NumOfCheckedPriorityEquipSet / NumOfPriorityEquipSet;


            long dif = SearchingTime.ElapsedMilliseconds / 1000;



            if (dif == 0)
                dif = 1;

            ulong eps = NumOfCheckedPriorityEquipSet / (ulong)dif;


            if (eps == 0)
                eps = 1;
            uint rest = (uint)((NumOfPriorityEquipSet - NumOfCheckedPriorityEquipSet) / eps);

            string restString = "";
            if (rest > 3600)
            {
                restString += (rest / 3600).ToString() + "時間";
                rest %= 3600;
            }
            if (rest > 60)
            {
                restString += (rest / 60).ToString() + "分";
                rest %= 60;
            }

            restString += rest.ToString() + "秒";


            label_search.Text = "残り" + restString + " " + pasent.ToString() + "%";
            toolTip1.SetToolTip(label_search, NumOfCheckedPriorityEquipSet.ToString("N0") + " / " + NumOfPriorityEquipSet.ToString("N0"));


            if (pasent > 100)
                pasent = 100;


            progressBar_search.Value = (int)pasent;
        }

        private void timer1_Tick(object sender, EventArgs e)//現在の検索状況を更新
        {
            timer1.Stop();//停止

            UpdateStatusBar();//ステータスバー更新

            List<EquipSet> AddList = AddEquipSetList;
            lock (AddEquipSetList)//セカンドと切り替える
            {
                AddEquipSetList = SecondAddEquipSetList;
            }
            SecondAddEquipSetList = AddList;


            if (SecondAddEquipSetList.Count > 0)
            {
                equipSetListView_result.BeginUpdate();
                foreach (EquipSet set in SecondAddEquipSetList)
                {
                    equipSetListView_result.AddEquipSet(set, true);

                    if (equipSetListView_result.Items.Count >= setting.StopSearchCount)
                    {
                        equipSetListView_result.EndUpdate();
                        StopSearching();
                        PlayNotifySound();
                        MessageBox.Show("検索結果が" + setting.StopSearchCount.ToString() + "件を超えたので検索を打ち切ります。");

                        return;
                    }
                }

                equipSetListView_result.EndUpdate();
                SecondAddEquipSetList.Clear();
            }


            foreach (SearchClass sc in SearchClasses)
                sc.isUpdatetime = true;

            bool Finish = true;
            foreach (SearchClass sc in SearchClasses)
            {

                if (sc.isLive == true)
                {
                    Finish = false;
                    break;
                }
            }

            if (Finish)
            {
                equipSetListView_result.BeginUpdate();

                foreach (EquipSet set in AddEquipSetList)//サブもすべて空にする
                    equipSetListView_result.AddEquipSet(set, true);


                equipSetListView_result.EndUpdate();


                if (NumOfPriorityEquipSet != NumOfCheckedPriorityEquipSet)
                {
                    MessageBox.Show("エラー発生。検索が正しく終了しませんでした。");
                    //   MessageBox.Show("Debug エラーはっせー 下の値と共に開発者にエラー再現方法を知らせると吉\r\n" + NumOfPriorityEquipSet.ToString() + "　" + NumOfCheckedPriorityEquipSet.ToString());
                }


                EndedSearch();

                PlayNotifySound();

                return;
            }

            timer1.Start();//再開
        }

        private void PlayNotifySound()
        {
            try
            {
                if (setting.PlaySound)
                {
                    string fpath = setting.SoundFilePath;

                    if (File.Exists(fpath))
                    {
                        System.Media.SoundPlayer Sound = new System.Media.SoundPlayer(fpath);
                        Sound.Play();
                    }
                }
            }
            catch (Exception) {
            
            }

        }

        private void MHSX2Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isSearching)
            {
                if (MessageBox.Show("終了しますか？",
                    "終了確認",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    StopSearching();
                }
                else
                {
                    e.Cancel = true;
                    return;
                }
            }


            setting.MainBounds = new WindowStatus(this);


            SaveSettings();
        }




        private void button_search_stop_Click(object sender, EventArgs e)
        {
            if ((bool)button_search_stop.Tag == false)
            {
                foreach (SearchClass sclass in SearchClasses)
                {
                    sclass.Stop();
                    sclass.isUpdatetime = true;
                }

                button_search_stop.Tag = true;
                button_search_stop.Text = "再開";

                timer1.Stop();
                SearchingTime.Stop();
            }
            else
            {
                foreach (SearchClass sclass in SearchClasses)
                {
                    sclass.Resume();
                }

                button_search_stop.Tag = false;
                button_search_stop.Text = "一時中断";
                timer1.Start();
                SearchingTime.Start();
            }
        }

        private void equipSetListView_result_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (equipSetListView_result.SelectedIndexs.Count < 1)
                return;

            ListViewItem item = equipSetListView_result.ItemCollection[equipSetListView_result.SelectedIndexs[0]];

            EquipSet set = equipSetView_search_result.CopyEquipSet((EquipSet)item.Tag);
            equipSetView_search_result.UpdateData();
            //equipSetSkillView_search_result.ESet = set;
            //equipSetSkillView_search_result.UpdateData();

            panel6.Enabled = true;

           //button_clip.Enabled = true;
  //          button_save_favorite.Enabled = true;
           //button_edit_result_equip.Enabled = true;

        }



        private void button_clip_Click(object sender, EventArgs e)
        {
            ClipEquipSet(SearchedCondition.job, SearchedCondition.sex, equipSetView_search_result.ESet);
        }

        public static void ClipEquipSet(Job job, Sex sex, EquipSet eset)
        {
            String CopyString = "MHSX2 装備クリップ";

            int count = 0;
            CopyString += " " + job.ToString() + "(" + sex.ToString() + ")" + "\r\n\r\n";



            for (int i = 0; i < (int)EquipKind.NumOfEquipKind; i++)
            {
                string line = "";

                Equipment equip = eset[(EquipKind)i];

                string name;

                if (equip.EquipData != null && equip.isChecked)
                {
                    name = equip.equipdata_value.Name;
                }
                else
                {
                    if (i == (int)EquipKind.Weapon)
                    {
                        name = "武器スロットなし";
                    }
                    else
                        name = "なし";

                    CopyString += name + "\r\n";
                    continue;
                }


                line += name;
                line = SubFunc.FillSpace(line, 170);
                line += equip.EquipData.Class;



                //line.PadRight(30 - new ASCIIEncoding().GetByteCount(line), ' ');
                line = SubFunc.FillSpace(line, 215);


                line += "Lv" + equip.Level + "　";//レベル出力

                line = SubFunc.FillSpace(line, 240);

                if (i != (int)EquipKind.Weapon)
                {
                    string def = equip.Def.ToString();


                    Font font = new Font("ＭＳ Ｐゴシック", 12);
                    int rest = 35 - TextRenderer.MeasureText(def, font).Width;

                    while (rest >= 10)
                    {
                        def = "　" + def;
                        rest -= 10;
                    }

                    rest = 35 - TextRenderer.MeasureText(def, font).Width;
                    if (rest >= 4)
                    {
                        if (line.EndsWith(" "))
                        {
                            line.Remove(line.Length - 1);
                            def = "　" + def;

                        }
                        else
                            def += " ";
                    }

                    rest = 35 - TextRenderer.MeasureText(def, font).Width;



                    line += def + "　";
                }
                else
                    line += "　　　";


                line = SubFunc.FillSpace(line, 300);

                line += equip.GetSlotString();

                line = SubFunc.FillSpace(line, 330);

                line += equip.GetJewelryString();

                CopyString += line += "\r\n";
            }


            if (eset.PigClothes.Clothes != null)
            {
                string line = eset.PigClothes.Clothes.Name;

                line = SubFunc.FillSpace(line, 310);

                line += eset.PigClothes.GetSlotString();
                line = SubFunc.FillSpace(line, 360);
                line += eset.PigClothes.GetJewelryString();

                CopyString += line + "\r\n";
            }
            else
            {
                CopyString += "プーギー服なし\r\n";
            }


            CopyString += "\r\n";
            CopyString += "防御値:" + eset.TotalDef.ToString() + " ";
            CopyString += "ｽﾛｯﾄ:" + eset.GetSlotString() + " ";

            string[] elements = new string[] { "火", "水", "雷", "氷", "龍" };


            for (int i = 0; i < (int)ElementType.NumOfElementKind; i++)
            {
                if (i != 0)
                    CopyString += " ";
                CopyString += elements[i] + ":" + eset.TotalElement[(ElementType)i].ToString();
            }

            CopyString += "\r\n\r\n";

            count = 0;

            Dictionary<SkillBase, SkillPoint> Dict = eset.GetInvokeSKillHashTable();
            List<SkillPoint> list = new List<SkillPoint>(Dict.Values);

            uint lower_skillid = uint.MaxValue;

            if (list.Count > 10)
            {
                List<uint> skillidlist = new List<uint>();
                foreach (SkillPoint sp in list)
                {
                    skillidlist.Add(sp.SBase.SkillId);
                }

                skillidlist.Sort();

                lower_skillid = skillidlist[9];
            }

            List<SkillPoint> NotInvokeList = new List<SkillPoint>();

            int index = 0;
            while (index < list.Count)
            {
                if (list[index].SBase.SkillId > lower_skillid)
                {
                    NotInvokeList.Add(list[index]);
                    list.RemoveAt(index);
                }
                else
                    index++;
            }

            list.Sort();
            NotInvokeList.Sort();

            count = 0;
            foreach (SkillPoint sp in list)
            {
                if (count > 4)
                {
                    CopyString += "\r\n";
                    count = 0;
                }
                else if (count != 0)
                    CopyString += ",";

                CopyString += sp.GetOption().Name;
                count++;
            }

            foreach (SkillPoint sp in NotInvokeList)
            {
                if (count > 4)
                {
                    CopyString += "\r\n";
                    count = 0;
                }
                else if (count != 0)
                    CopyString += ",";

                CopyString += sp.GetOption().Name;
                count++;


            }


            try
            {
                Clipboard.SetText(CopyString);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void button_edit_result_equip_Click(object sender, EventArgs e)
        {
            SearchCondition condition = MakeSearchCondition();

            if (condition == null)
                return;

            basedata.UpdateEquipLevel((int)numericUpDown_HRLimit.Value);

            condition.ESet = equipSetView_search_result.ESet;

            EditEquipDialog EditDialog = new EditEquipDialog(basedata, condition, setting);

            EditDialog.ShowDialog();


        }

        private void listView_SearchSkill_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView_SearchSkill.SelectedItems.Count != 1)
                return;

            SkillBase sb = (SkillBase)listView_SearchSkill.SelectedItems[0].Tag;
            AddSkillOptionToView(sb.OptionTable);

        }

        private void LoadSettings()
        {
            if (!Directory.Exists(Properties.Resources.DNAME_SETTING))
            {
                Directory.CreateDirectory(Properties.Resources.DNAME_SETTING);
            }

            string fname = SubFunc.MakeFilePath(Properties.Resources.DNAME_SETTING, Properties.Resources.FNAME_IGNORE);

            if (!File.Exists(fname))
            {
                ignore = new Ignore();
            }
            else
            {
                FileStream fs = File.Open(fname, FileMode.Open);

                try
                {
                    System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Ignore));

                    ignore = (Ignore)serializer.Deserialize(fs);
                    fs.Close();
                }
                catch (Exception)
                {
                    fs.Close();
                    if (MessageBox.Show("設定ファイル読み込みエラー 設定ファイルを削除しますか？", "エラー", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        File.Delete(fname);

                    ignore = new Ignore();
                }
            }

            fname = SubFunc.MakeFilePath(Properties.Resources.DNAME_SETTING, Properties.Resources.FNAME_SETTING);

            if (!File.Exists(fname))
            {
                setting = new Settings();
            }
            else
            {
                FileStream fs = File.Open(fname, FileMode.Open);

                try
                {
                    System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Settings));

                    setting = (Settings)serializer.Deserialize(fs);
                    fs.Close();
                }
                catch (Exception)
                {
                    fs.Close();
                    if (MessageBox.Show("設定ファイル読み込みエラー 設定ファイルを削除しますか？", "エラー", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        File.Delete(fname);

                    setting = new Settings();
                }
            }

            return;
        }

        private void SaveSettings()
        {
            if (!Directory.Exists(Properties.Resources.DNAME_SETTING))
            {
                Directory.CreateDirectory(Properties.Resources.DNAME_SETTING);
            }

            string fname = Properties.Resources.DNAME_SETTING + "/" + Properties.Resources.FNAME_IGNORE;

            FileStream fs = File.Open(fname, FileMode.Create);

            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Ignore));


            serializer.Serialize(fs, ignore);
            fs.Close();


            Sex sex = (Sex)comboBox_sex.SelectedItem;
            setting.sex = sex.type;

            int.TryParse(textBox_def_lower.Text, out setting.def_lower);

            //if (textBox_def_upper.Text == "∞")
            //{
            //    setting.def_upper = int.MaxValue;
            //}
            //else
            //{
            //    int.TryParse(textBox_def_upper.Text, out setting.def_upper);
            //}

            setting.rare_lower = int.Parse((string)comboBox_rare_lower.SelectedItem);
            setting.rare_upper = int.Parse((string)comboBox_rare_upper.SelectedItem);


            setting.HRLimit = HRLimit;
             

            //豚服保存
            if (ESet.PigClothes.Clothes != null)
            {
                setting.PigClothes = ESet.PigClothes.Clothes.Name;
            }
            else
                setting.PigClothes = "";



            fname = SubFunc.MakeFilePath(Properties.Resources.DNAME_SETTING, Properties.Resources.FNAME_SETTING);
            fs = File.Open(fname, FileMode.Create);
            serializer = new System.Xml.Serialization.XmlSerializer(typeof(Settings));
            serializer.Serialize(fs, setting);


            fs.Close();


        }

        private int HRLimit
        {
            get
            {
                return (int)numericUpDown_HRLimit.Value + comboBox_R_Kind.SelectedIndex * 1000;
            }
        }

        private void 除外装備へ追加ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListView list = (ListView)contextMenuStrip_searchresult.SourceControl;

            foreach (ListViewItem item in list.SelectedItems)
            {
                if (item.Tag is Equipment)
                {
                    Equipment data = (Equipment)item.Tag;
                    if (data != null)
                    {
                        if (!ignore.Equip.Contains(data.EquipData.Name))
                            ignore.Equip.Add(data.EquipData.Name);
                    }
                }
            }

            //basedata.UpdateIgnoreInfo(ignore);
        }

        private void 除外指定を解除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListView list = (ListView)contextMenuStrip_searchresult.SourceControl;

            foreach (ListViewItem item in list.SelectedItems)
            {
                if (item.Tag is Equipment)
                {
                    Equipment data = (Equipment)item.Tag;
                    if (data != null)
                    {
                        ignore.Equip.Remove(data.EquipData.Name);
                    }
                }
            }

            //basedata.UpdateIgnoreInfo(ignore);
        }

        private void checkBox_ignore_item_CheckedChanged(object sender, EventArgs e)
        {
            setting.IgnoreItem = checkBox_ignore_item.Checked;
        }

        private void button_edit_ignore_item_Click(object sender, EventArgs e)
        {
            EditIgnoreList dialog = new EditIgnoreList(basedata, ignore, EditIgnoreList.Mode.Equip);
            dialog.ShowDialog();
            SaveSettings();
        }

        private void comboBox_priority_SelectedIndexChanged(object sender, EventArgs e)
        {
            setting.Priority = (ProcessPriorityClass)comboBox_priority.SelectedItem;
            Process thisProcess = System.Diagnostics.Process.GetCurrentProcess();
            thisProcess.PriorityClass = setting.Priority;
        }

        private void 除外指定スキルに追加ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SkillOption so = null;
            if (contextMenuStrip_ignore_skill.SourceControl == listView_skilloption)
            {
                if (listView_skilloption.SelectedIndices.Count > 0)
                    so = (SkillOption)listView_skilloption.SelectedItems[0].Tag;
            }
            else if (contextMenuStrip_ignore_skill.SourceControl == equipSetSkillView_search_result)
            {
                if (equipSetSkillView_search_result.SelectedIndices.Count > 0)
                {
                    SkillPoint sp = (SkillPoint)equipSetSkillView_search_result.SelectedItems[0].Tag;
                    so = sp.SBase.GetOption(sp.Point);
                }
            }

            if (so == null)
                return;

            if (!ignore.skill.Contains(so.Name))
                ignore.skill.Add(so.Name);


            AddSkillOptionToView(so.SBase.OptionTable);
        }

        private void button_edit_ignore_skill_Click(object sender, EventArgs e)
        {
            EditIgnoreList dialog = new EditIgnoreList(basedata, ignore, EditIgnoreList.Mode.Skill);
            dialog.ShowDialog();
            SaveSettings();
        }

        private void listView_skilloption_DoubleClick(object sender, EventArgs e)
        {
            AddSkill();
        }

        private void 除外指定を解除ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SkillOption so = null;
            if (contextMenuStrip_ignore_skill.SourceControl == listView_skilloption)
            {
                if (listView_skilloption.SelectedIndices.Count > 0)
                    so = (SkillOption)listView_skilloption.SelectedItems[0].Tag;
            }
            else if (contextMenuStrip_ignore_skill.SourceControl == equipSetSkillView_search_result)
            {
                if (equipSetSkillView_search_result.SelectedIndices.Count > 0)
                {
                    SkillPoint sp = (SkillPoint)equipSetSkillView_search_result.SelectedItems[0].Tag;
                    so = sp.SBase.GetOption(sp.Point);
                }
            }

            if (so == null)
                return;

            ignore.skill.Remove(so.Name);

            AddSkillOptionToView(so.SBase.OptionTable);
        }

        private void checkBox_ignore_skill_CheckedChanged(object sender, EventArgs e)
        {
            setting.IgnoreSkill = checkBox_ignore_skill.Checked;
        }

        private void button_save_favorite_Click(object sender, EventArgs e)
        {
            AdmFavoriteSet.Save(equipSetView_search_result.ESet, SearchedCondition.job, SearchedCondition.sex, null);
        }

        private void numericUpDown_HRLimit_ValueChanged(object sender, EventArgs e)
        {
            basedata.UpdateEquipLevel(HRLimit);
        }

        private void button_edit_ignore_class_Click(object sender, EventArgs e)
        {
            EditIgnoreClassList dialog = new EditIgnoreClassList(basedata, ignore);
            dialog.ShowDialog();
        }

        private void checkBox_ignore_class_CheckedChanged(object sender, EventArgs e)
        {
            setting.IgnoreClass = checkBox_ignore_class.Checked;
        }

        private void button_copy_items_Click(object sender, EventArgs e)
        {
            listView_necessity_items.ClipItems();

        }

        private void equipSetView_search_result_DoubleClick(object sender, EventArgs e)
        {
            詳細ToolStripMenuItem_Click(sender, e);
        }

        private void 詳細ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (equipSetView_search_result.SelectedItems.Count < 1)
                return;

            ListViewItem item = equipSetView_search_result.SelectedItems[0];

            if (item.Tag is Equipment)
            {
                Equipment equip = (Equipment)item.Tag;


                EquipDataDetaileDialog dialog = new EquipDataDetaileDialog(basedata, setting, equip.EquipData, equip.Level, (Sex)comboBox_sex.SelectedItem);

                dialog.ShowDialog();
            }
        }

        private void comboBox_search_history_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_search_history.SelectedIndex == 0)//新規
            {
                button_search_history_save.Text = "保存";
                button_search_history_delete.Enabled = false;
                button_search_history_load.Enabled = false;
            }
            else
            {
                button_search_history_save.Text = "上書";
                button_search_history_delete.Enabled = true;
                button_search_history_load.Enabled = true;
            }
        }

        private void LoadSearchHistoryList()
        {
            comboBox_search_history.Items.Clear();

            comboBox_search_history.Items.Add("新規");
            comboBox_search_history.SelectedIndex = 0;

            if (!Directory.Exists(MHSX2.Properties.Resources.DNAME_HISTORY))
            {
                Directory.CreateDirectory(MHSX2.Properties.Resources.DNAME_HISTORY);
            }
            else
            {
                string[] list = Directory.GetFiles(Properties.Resources.DNAME_HISTORY, "*.hist");

                for (int i = 0; i < list.Length; i++)
                {
                    list[i] = Path.GetFileNameWithoutExtension(list[i]);
                }

                comboBox_search_history.Items.AddRange(list);
            }


        }

        private void button_search_history_save_Click(object sender, EventArgs e)
        {
            if (equipSetListView_result.Items.Count < 1)
                return;


            if (SearchedCondition == null)
                return;


            List<EquipSet> list = new List<EquipSet>(equipSetListView_result.ShowItemList);


            //for (int i = 0; i < equipSetListView_result.Items.Count; i++)
            //{
            //    ListViewItem item = equipSetListView_result.Items[i];

            //    EquipSet set = (EquipSet)item.Tag;

            //    list.Add(set);
            //}


            if (comboBox_search_history.SelectedIndex == 0)
            {
                //新規

                SaveFileNameDialog dialog = new SaveFileNameDialog();

                DateTime now = DateTime.Now;


                Job job = SearchedCondition.job;
                Sex sex = SearchedCondition.sex;

                string name = job.ToString() +
                    "(" +
                    sex.ToString() +
                    ") - " +
                    now.Year.ToString("0000") + now.Month.ToString("00") + now.Day.ToString("00");

                dialog.textBox_filename.Text = name;

                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                name = dialog.textBox_filename.Text;


                foreach (char c in Path.GetInvalidFileNameChars())//使用できない文字を置き換え
                {
                    name = name.Replace(c, '_');
                }



                AdmSearchHistory.Save(list, name + ".hist", SearchedCondition);

                LoadSearchHistoryList();

                MessageBox.Show("「" + name + "」で保存しました");
            }
            else
            {
                //上書き

                if (MessageBox.Show("上書きします。よろしいですか？", "上書き確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                    return;

                string name = comboBox_search_history.Text;
                AdmSearchHistory.Save(list, name + ".hist", SearchedCondition);

                LoadSearchHistoryList();

                MessageBox.Show("「" + name + "」で上書き保存しました");
            }
        }

        private void button_search_history_delete_Click(object sender, EventArgs e)
        {
            string name = comboBox_search_history.Text;

            if (MessageBox.Show("「" + name + "」を削除します。よろしいですか？", "削除確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Cancel)
            {
                return;
            }


            AdmSearchHistory.Delete(name + ".hist");
            LoadSearchHistoryList();
        }

        private void button_search_history_load_Click(object sender, EventArgs e)
        {
            string name = comboBox_search_history.Text;


            List<EquipSet> list = AdmSearchHistory.Load(name + ".hist", basedata, out SearchedCondition);


            ESet = SearchedCondition.ESet;
            EquipSetView_condition.UpdateData();


            listView_SearchSkill.BeginUpdate();
            listView_SearchSkill.Items.Clear();

            foreach (KeyValuePair<SkillBase, SkillPointCondition> pair in SearchedCondition.SkillPointConditionTable)
            {
                AddSkillOptionToSearchConditionView(pair.Key, pair.Value.Point);
            }

            listView_SearchSkill.EndUpdate();


            equipSetListView_result.BeginUpdate();

            equipSetListView_result.ClearItem();


            foreach (EquipSet set in list)//
                equipSetListView_result.AddEquipSet(set, false);


            equipSetListView_result.EndUpdate();




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
                xu = 7;
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
        }

        private void 詳細ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            EquipSet set = EquipSetView_condition.ESet;

            foreach (ListViewItem item in EquipSetView_condition.SelectedItems)
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


        private void button_add_skillset_Click(object sender, EventArgs e)
        {
            SaveFileNameDialog dialog = new SaveFileNameDialog();

            dialog.textBox_filename.Text = "スキルセット";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string savename = dialog.textBox_filename.Text;

                List<SkillOption> list = new List<SkillOption>();
                foreach (ListViewItem item in listView_SearchSkill.Items)
                {
                    if (item.Checked && item.SubItems[2].Text != string.Empty)
                    {
                        if (basedata.SkillOptionMap.ContainsKey(item.SubItems[2].Text))
                        {
                            list.Add(basedata.SkillOptionMap[item.SubItems[2].Text]);
                        }
                    }
                }


                treeView_skill.AddSkillSet(savename, list, (Job)comboBox_job.SelectedItem);


            }
        }

        private void button_filter_reset_Click(object sender, EventArgs e)
        {
            equipSetListView_result.ResetFilter();
        }

        private void button_filter_Click(object sender, EventArgs e)
        {
            //重複チェック用
            //List<EquipSet> list = equipSetListView_result.ShowItemList;
            //for (int i = 0; i < list.Count; i++)
            //{
            //    for (int j = i + 1; j < list.Count; j++)
            //    {
            //        if (list[i].Equips[0].EquipData == list[j].Equips[0].EquipData &&
            //            list[i].Equips[1].EquipData == list[j].Equips[1].EquipData &&
            //            list[i].Equips[2].EquipData == list[j].Equips[2].EquipData &&
            //            list[i].Equips[3].EquipData == list[j].Equips[3].EquipData &&
            //            list[i].Equips[4].EquipData == list[j].Equips[4].EquipData &&
            //            list[i].Equips[5].EquipData == list[j].Equips[5].EquipData)
            //        {

            //        }
            //    }
            //}



            FilterDialog FilterDlg = new FilterDialog(basedata, setting);
            if (FilterDlg.ShowDialog() == DialogResult.OK)
            {

                FilterDialog.MyComboBoxItem item = FilterDlg.comboBox1.SelectedItem as FilterDialog.MyComboBoxItem;

                List<EquipSet> PickUpList = null;

                string text = FilterDlg.textBox1.Text;//絞り込み

                if (text != "")
                {

                    switch (item.Id)
                    {
                        case FilterDialog.IDList.ByEquip:
                            PickUpList = equipSetListView_result.SearchByEquipName(text);
                            break;
                        case FilterDialog.IDList.ByItem:
                            PickUpList = equipSetListView_result.SearchByItemName(text, basedata);
                            break;
                        case FilterDialog.IDList.ByJewelry:
                            PickUpList = equipSetListView_result.SearchByJewelryName(text);
                            break;
                        case FilterDialog.IDList.BySkill:
                            PickUpList = equipSetListView_result.SearchBySkillName(text);
                            break;
                        case FilterDialog.IDList.ByClass:
                            PickUpList = equipSetListView_result.SearchByClassName(text);
                            break;
                    }


                    switch (FilterDlg.comboBox2.SelectedIndex)
                    {
                        case 0://取り除く
                            equipSetListView_result.RemoveFromShowList(PickUpList);
                            break;
                        case 1://残す
                            equipSetListView_result.SetShowList(PickUpList);

                            break;
                    }
                }

                if (FilterDlg.PointTable.Count != 0)
                {
                    PickUpList = equipSetListView_result.SearchBySkillOrder(FilterDlg.PointTable);


                    switch (FilterDlg.comboBox5.SelectedIndex)
                    {
                        case 0://残す
                            equipSetListView_result.SetShowList(PickUpList);
                            break;
                        case 1://取り除く
                            equipSetListView_result.RemoveFromShowList(PickUpList);
                            break;
                    }
                }



                item = FilterDlg.comboBox3.SelectedItem as FilterDialog.MyComboBoxItem;
                text = FilterDlg.textBox2.Text;//ソート順

                if (text != "")
                {
                    switch (FilterDlg.comboBox4.SelectedIndex)
                    {
                        case 0:
                            equipSetListView_result.SortShowList(item.Id, text, SortOrder.Ascending);
                            break;
                        case 1:
                            equipSetListView_result.SortShowList(item.Id, text, SortOrder.Descending);
                            break;
                    }
                }

            }
        }



        private void 取り除くToolStripMenuItem_Click(object sender, EventArgs e)
        {
            equipSetListView_result.RemoveFromShowList_Selected();
        }

        private void button_edit_numorder_Click(object sender, EventArgs e)
        {
            TypeNumOrderDialog dialog = new TypeNumOrderDialog(SPNumOrder, BlankEquipNumOrder, TypeNumOrderTable);

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SPNumOrder = dialog.SPNumOrder;
                BlankEquipNumOrder = dialog.BlankNumOrder;
                TypeNumOrderTable = dialog.TypeOrderTable;

                checkBox_TypeNumOrder.Checked = true;

            }
        }

        private void クリップToolStripMenuItem_Click(object sender, EventArgs e)
        {

           if( equipSetListView_result.SelectedIndexs.Count < 1)
               return;

           EquipSet set = equipSetListView_result.ShowItemList[equipSetListView_result.SelectedIndexs[0]];

            ClipEquipSet(SearchedCondition.job, SearchedCondition.sex, set);
        }

        private void button_setting_Click(object sender, EventArgs e)
        {
            OptionDialog dialog = new OptionDialog(setting,basedata);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                setting.ThreadNum = (int)dialog.numericUpDown_threadnum.Value;
                setting.StopSearchCount = (int)dialog.numericUpDown_maxviewcount.Value;
                setting.OptimizeEquip = dialog.checkBox_optimizeequip.Checked;
                //setting.IgnoreDef = dialog.checkBox_ignore_def.Checked;
                setting.UseNetwork = dialog.checkBox_usenetwork.Checked;
                setting.PictureServer = dialog.textBox_picture_server.Text;
                //setting.OptimizeHard = dialog.checkBox_optimize_hard.Checked;
                setting.PlaySound = dialog.checkBox_sound.Checked;
                setting.SoundFilePath = dialog.textBox_sound.Text;
                setting.CheckNewVersion = dialog.checkBox_checkversion.Checked;
            }
        }

        private void MHSX2Form_Shown(object sender, EventArgs e)
        {
            this.Bounds = setting.MainBounds.ToRectangle();
            this.WindowState = setting.MainBounds.State;
        }




       

        private void equipSetListView_result_DoubleClick(object sender, EventArgs e)
        {
            if (equipSetListView_result.SelectedIndexs.Count == 1)
            {
                ListViewItem item = equipSetListView_result.Items[equipSetListView_result.SelectedIndexs[0]];

                if (item.Tag != null && item.Tag is EquipSet)
                {
                    EquipSet eset = (EquipSet)item.Tag;

                    if (SearchedCondition == null)
                        return;

                    SearchCondition condition = SearchedCondition.MemberwiseClone_();

                    condition.ESet = eset;

                    EditEquipDialog EditDialog = new EditEquipDialog(basedata, condition, setting);

                    //装備セット編集を開く
                    EditDialog.ShowDialog();

                }
            }
        }


      

 

    }

}
