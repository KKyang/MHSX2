using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;


namespace MHSX2
{
    public partial class SkillConditionEditDialog : Form
    {
        BaseData basedata;
        Settings setting;

        public SkillConditionEditDialog(BaseData b, List<SkillPriority> list, Settings setting)
        {
            basedata = b;
            this.setting = setting;
            InitializeComponent();

            foreach (SkillPriority sp in list)
            {
                listBox_searchskill.Items.Add(sp);
            }

        }

        public List<SkillPriority> GetSearchSkills()
        {
            List<SkillPriority> list = new List<SkillPriority>();

            foreach (SkillPriority sp in listBox_searchskill.Items)
            {
                list.Add(sp);
            }
            return list;
        }

        private void SkillConditionEditDialog_Load(object sender, EventArgs e)
        {
            skillBaseTreeView1.LoadBaseData(basedata, setting);
        }

        private void addskill()
        {
            if (skillBaseTreeView1.SelectedNode != null)
            {
                List<SkillBase> list = new List<SkillBase>();
                if (skillBaseTreeView1.SelectedNode.Tag is SkillBase)
                {
                    list.Add((SkillBase)skillBaseTreeView1.SelectedNode.Tag);
                }
                else if (skillBaseTreeView1.SelectedNode.Tag is SkillSet)
                {
                    SkillSet tmp = (SkillSet)skillBaseTreeView1.SelectedNode.Tag;

                    foreach (string str in tmp.list)
                    {
                        if (basedata.SkillOptionMap.ContainsKey(str))
                        {
                            list.Add(basedata.SkillOptionMap[str].SBase);
                        }
                    }

                }

                foreach (SkillBase sb in list)
                {
                    bool find = false;
                    foreach (SkillPriority sp in listBox_searchskill.Items)
                    {
                        if (sp.SBase == sb)
                        {
                            find = true;
                            break;
                        }
                    }

                    if (!find)
                    {
                        SkillPriority sp = new SkillPriority(sb, 1, true);
                        listBox_searchskill.Items.Add(sp);
                    }
                }


            }
        }

        private void deleteskill()
        {
            while (listBox_searchskill.SelectedIndex > -1)
            {
                listBox_searchskill.Items.RemoveAt(listBox_searchskill.SelectedIndex);
            }
        }

        private void button_add_Click(object sender, EventArgs e)
        {
            addskill();
        }

        private void button_dell_Click(object sender, EventArgs e)
        {
            deleteskill();

        }

        private void skillBaseTreeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Level == 1)
            {
                addskill();

            }
        }

        private void listBox_searchskill_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    deleteskill();
                    break;
            }
        }

        private void listBox_searchskill_DoubleClick(object sender, EventArgs e)
        {
            deleteskill();
        }

    }
}
