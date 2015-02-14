using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MHSX2
{
    public partial class SkillFilterDialog : Form
    {
        private BaseData basedata;
        private Settings setting;
        private SortedList<SkillBase, SkillPointCondition> PointTable;

        public SkillFilterDialog(BaseData Data, Settings Setting, SortedList<SkillBase, SkillPointCondition> PointTable)
        {
            basedata = Data;
            setting = Setting;
            this.PointTable = PointTable;

            InitializeComponent();
        }

        private void SkillFilterDialog_Load(object sender, EventArgs e)
        {
            treeView_skill.LoadBaseData(basedata, setting);

            listView_SearchSkill.SetSkillOptionTable(PointTable);

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

                if (!Selected)
                {
                    Selected = true;
                    item.Selected = true;
                }


                listView_skilloption.Items.Add(item);
            }


            listView_skilloption.EndUpdate();
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

        private void AddSkill()
        {
            if (listView_skilloption.SelectedItems.Count == 0)
                return;


            SkillOption sopt = (SkillOption)listView_skilloption.SelectedItems[0].Tag;


            AddSkillOptionToSearchConditionView(sopt.SBase, sopt.Point);
        }

        private void AddSkillOptionToSearchConditionView(SkillBase sbase, int Point)
        {

            ListViewItem item = null;
            foreach (ListViewItem i in listView_SearchSkill.Items)
            {
                SkillBase tag = (SkillBase)i.Tag;
                if (tag == sbase)
                {
                    item = i;
                    break;
                }
            }

            if (item == null)
            {//新規登録
                item = new ListViewItem();

                item.Name = "skill";
                item.Tag = sbase;
                item.Text = sbase.Name;
                item.Checked = true;
                item.UseItemStyleForSubItems = false;

                ListViewItem.ListViewSubItem point = new ListViewItem.ListViewSubItem();
                point.Text = Point.ToString();
                point.Name = "point";
                item.SubItems.Add(point);

                ListViewItem.ListViewSubItem skillname = new ListViewItem.ListViewSubItem();

                SkillOption so = sbase.GetOption(Point);

                if (so != null)
                {
                    skillname.Text = sbase.GetOption(Point).Name;

                    if (Point < 0)
                        skillname.ForeColor = System.Drawing.Color.Red;
                    else
                        skillname.ForeColor = System.Drawing.Color.Black;
                }
                else
                    skillname.Text = "";

                skillname.Name = "skillname";
                item.SubItems.Add(skillname);



                listView_SearchSkill.Items.Add(item);
            }
            else
            {
                item.SubItems[1].Text = Point.ToString();
                listView_SearchSkill.UpdateLine(item.Index);
                item.Checked = true;
            }
        }


        private void listView_skilloption_DoubleClick(object sender, EventArgs e)
        {
            AddSkill();
        }

        private void button_add_Click(object sender, EventArgs e)
        {
            AddSkill();
        }

        private void button_delete_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView_SearchSkill.SelectedItems)
            {
                listView_SearchSkill.Items.Remove(item);
            }
        }

        private void button_deleteall_Click(object sender, EventArgs e)
        {
            listView_SearchSkill.Items.Clear();
        }

        private void 削除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView_SearchSkill.SelectedItems)
            {
                listView_SearchSkill.Items.Remove(item);
            }
        }

        private void listView_SearchSkill_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView_SearchSkill.SelectedItems.Count != 1)
                return;

            SkillBase sb = (SkillBase)listView_SearchSkill.SelectedItems[0].Tag;
            AddSkillOptionToView(sb.OptionTable);
        }

    }
}
