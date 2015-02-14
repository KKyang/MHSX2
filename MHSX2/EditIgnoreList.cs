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
    public partial class EditIgnoreList : Form
    {
        public enum Mode
        {
            Equip,
            Jewelry,
            Skill,
            Item,
            SKillCuff
        }

        Mode mode;
        Ignore ignore;
        BaseData basedata;
        bool initial = false;
        List<int> listCount;
        List<int> listSum;
        
        

        public EditIgnoreList(BaseData basedata, Ignore ignore, Mode mode)
        {
            this.mode = mode;
            this.ignore = ignore;
            this.basedata = basedata;
            listCount = new List<int>();
            listSum = new List<int>();
            InitializeComponent();

        }

        private void button_search_Click(object sender, EventArgs e)
        {
            Search();
        }

        private void initialSearch()
        {
            initial = false;

            listCount.Clear();
            listSum.Clear();
            listCount = new List<int>(checkedListBox1.Items.Count);
            listSum = new List<int>(checkedListBox1.Items.Count);
            for (int j = 0; j < checkedListBox1.Items.Count; j++)
            {
                listCount.Add(0);
                listSum.Add(0);
            }
            int i;
            switch (mode)
            {

                case Mode.Equip:
                    foreach (Dictionary<string, EquipmentData> table in basedata.EquipDataMap)
                    {
                        foreach (string name in table.Keys)
                        {
                            for (i = 0; i < basedata.ClassList.Count; i++)
                            { 
                                if (table[name].Class == basedata.ClassList[i].ToString())
                                {
                                    listSum[0]++;
                                    listSum[i + 1]++;
                                    break;
                                }
                            }
                            if(i == basedata.ClassList.Count)
                            {
                                listSum[0]++;
                                i = basedata.ClassList.Count - 1;
                                listSum[i + 1]++;
                            }

                            if (!ignore.Equip.Contains(name))
                            {
                               
                                listCount[0]++;
                                listCount[i + 1]++;
                            }

                        }
                    }
                    if (listBox_result.Items.Count < 1)
                    {
                        checkedListBox1.SetItemChecked(checkedListBox1.SelectedIndex, true);
                    }
                    break;

                case Mode.Jewelry:
                    foreach (JewelryData data in basedata.JewelryDataMap.Values)
                    {
                        for (i = 0; i < basedata.ClassList_Jewelry.Count; i++)
                        {
                            if (data.Class == basedata.ClassList_Jewelry[i].ToString())
                            {
                                listSum[0]++;
                                listSum[i + 1]++;
                                break;
                            }
                        }
                        if (i == basedata.ClassList_Jewelry.Count)
                        {
                            listSum[0]++;
                            i = basedata.ClassList_Jewelry.Count - 1;
                            listSum[i + 1]++;
                        }

                        if (!ignore.Jewelry.Contains(data.Name))
                        {
                            listCount[0]++;
                            listCount[i + 1]++;                   }

                    }
                    break;

                case Mode.SKillCuff:


                    foreach (SkillCuffData data in basedata.SkillCaffDataMap.Values)
                    {
                        for (i = 0; i < basedata.ClassList_Jewelry.Count; i++)
                        {
                            if (data.Class == basedata.ClassList_Jewelry[i].ToString())
                            {
                                listSum[0]++;
                                listSum[i + 1]++;
                                break;
                            }
                        }
                        if (i == basedata.ClassList_Jewelry.Count)
                        {
                            listSum[0]++;
                            i = basedata.ClassList_Jewelry.Count - 1;
                            listSum[i + 1]++;
                        }

                        if (!ignore.SkillCuff.Contains(data.Name))
                        {
                            listCount[0]++;
                            listCount[i + 1]++;
                        }
                    }
                    break;
            }
            for (int j = 0; j < checkedListBox1.Items.Count; j++)
            {
                Console.WriteLine(listCount[j]);
                if (listCount[j] == 0)
                {
                    checkedListBox1.SetItemChecked(j, true);
                }
                else
                {
                    checkedListBox1.SetItemChecked(j, false);
                }
            }
            initial = true;
        }

        private void Search()
        {
            String search = textBox1.Text;

            listBox_ignore.BeginUpdate();
            listBox_result.BeginUpdate();

            listBox_result.Items.Clear();
            listBox_ignore.Items.Clear();


            string classname = null;
            bool isClass = false;
            
            if(checkedListBox1.SelectedIndex != 0)
            {
                isClass = true;
                classname = (string)checkedListBox1.SelectedItem;
                if (classname == "無分類")
                {
                    classname = null;
                }
            }


            switch (mode)
            {

                case Mode.Equip:
                    foreach (Dictionary<string, EquipmentData> table in basedata.EquipDataMap)
                    {
                        foreach (string name in table.Keys)
                        {
                            if (isClass)
                            {
                                if (table[name].Class != classname)
                                    continue;
                            }

                            if (!name.Contains(search))
                                continue;

                            if (!ignore.Equip.Contains(name))
                            {
                                listBox_result.Items.Add(name);
                            }
                            else
                                listBox_ignore.Items.Add(name);

                        }
                    }
                    break;
                 
                case Mode.Jewelry:
                    foreach (JewelryData data in basedata.JewelryDataMap.Values)
                    {
                        if (isClass)
                        {
                  
                            if (data.Class != classname)
                                continue;
                        }


                        if (!data.Name.Contains(search))
                            continue;

                        if (!ignore.Jewelry.Contains(data.Name))
                        {
                            listBox_result.Items.Add(data.Name);
                        }
                        else
                            listBox_ignore.Items.Add(data.Name);

                    }
                    break;
                case Mode.Skill:
                    foreach (SkillBase sb in basedata.SkillBaseMap.Values)
                    {
                        foreach (SkillOption so in sb.OptionTable)
                        {
                            if (!so.Name.Contains(search))
                                continue;

                            if (ignore.skill.Contains(so.Name))
                            {
                                listBox_ignore.Items.Add(so.Name);
                            }
                            else
                            {
                                listBox_result.Items.Add(so.Name);
                            }
                        }
                    }
                    break;
                    
                case Mode.SKillCuff:


                    foreach (SkillCuffData data in basedata.SkillCaffDataMap.Values)
                    {
                        if (isClass)
                        {
                            if (data.Class != classname)
                                continue;
                        }


                        if (!data.Name.Contains(search))
                            continue;

                        if (!ignore.SkillCuff.Contains(data.Name))
                        {
                            listBox_result.Items.Add(data.Name);
                        }
                        else
                            listBox_ignore.Items.Add(data.Name);

                    }
                    break;

                case Mode.Item:
                    foreach (String name in basedata.ItemMap.Keys)
                    {
                        if (!name.Contains(search))
                            continue;

                        if (ignore.Item.Contains(name))
                        {
                            listBox_ignore.Items.Add(name);
                        }
                        else
                        {
                            listBox_result.Items.Add(name);
                        }
                    }

                    break;
            }

            listBox_ignore.EndUpdate();
            listBox_result.EndUpdate();
        }

        private bool SearchCheck(int i, string name)
        {
            switch (mode)
            {

                case Mode.Equip:
                    
                    foreach (Dictionary<string, EquipmentData> table in basedata.EquipDataMap)
                    {
                        if(table.ContainsKey(name))
                        {
                            if (table[name].Class == checkedListBox1.Items[i].ToString())
                            {
                                return true;
                            }
                            else if(table[name].Class == null)
                            {
                                return true;
                            }
                        }
                    }
                    break;
                case Mode.Jewelry:
                    foreach (JewelryData data in basedata.JewelryDataMap.Values)
                    {
                        if(data.Name == name)
                        {
                            if(data.Class == checkedListBox1.Items[i].ToString())
                            {
                                return true;
                            }
                            else if (data.Class == null)
                            {
                                return true;
                            }
                        }
                    }

                    break;

                case Mode.SKillCuff:


                    foreach (SkillCuffData data in basedata.SkillCaffDataMap.Values)
                    {
                        if (data.Name == name)
                        {
                            if (data.Class == checkedListBox1.Items[i].ToString())
                            {
                                return true;
                            }
                            else if (data.Class == null)
                            {
                                return true;
                            }
                        }
                    }


                    break;
            }
            return false;
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                button_search_Click(null, null);
            }
        }


        private void button_add_Click(object sender, EventArgs e)
        {
            List<string> delete = new List<string>();
            listBox_ignore.BeginUpdate();
            listBox_result.BeginUpdate();

            List<string> tgt = null;

            switch (mode)
            {
                case Mode.Equip:
                    tgt = ignore.Equip;
                    break;
                case Mode.Jewelry:
                    tgt = ignore.Jewelry;
                    break;
                case Mode.Skill:
                    tgt = ignore.skill;
                    break;
                case Mode.SKillCuff:
                    tgt = ignore.SkillCuff;
                    break;
                case Mode.Item:
                    tgt = ignore.Item;
                    break;
            }



            foreach (string name in listBox_result.SelectedItems)
            {
                delete.Add(name);
                tgt.Add(name);
                listBox_ignore.Items.Add(name);
            }


            foreach (string name in delete)
            {
                listBox_result.Items.Remove(name);
            }

            initial = false;
            if(checkedListBox1.SelectedIndex == 0)
            {
                for(int i = 1; i < checkedListBox1.Items.Count; i++)
                {
                    for(int j = 0; j < delete.Count; j++)
                    {
                        if (SearchCheck(i, delete[j]))
                        {
                            listCount[0]--;
                            listCount[i]--;
                        }
                    }
                }
            }
            else
            {
                listCount[0] -= delete.Count;
                listCount[checkedListBox1.SelectedIndex] -= delete.Count;
            }
            for (int i = 1; i < checkedListBox1.Items.Count; i++)
            {
                if(listCount[i] == 0)
                {
                    checkedListBox1.SetItemChecked(i, true);
                }
            }
            initial = true;

            listBox_ignore.EndUpdate();
            listBox_result.EndUpdate();
        }

        private void button_del_Click(object sender, EventArgs e)
        {
            List<string> delete = new List<string>();

            listBox_ignore.BeginUpdate();
            listBox_result.BeginUpdate();

            List<string> tgt = null;
            switch (mode)
            {
                case Mode.Equip:
                    tgt = ignore.Equip;
                    break;
                case Mode.Jewelry:
                    tgt = ignore.Jewelry;
                    break;
                case Mode.Skill:
                    tgt = ignore.skill;
                    break;
                case Mode.SKillCuff:
                    tgt = ignore.SkillCuff;
                    break;
                case Mode.Item:
                    tgt = ignore.Item;
                    break;
            }

            foreach (string name in listBox_ignore.SelectedItems)
            {
                tgt.Remove(name);
                delete.Add(name);
                listBox_result.Items.Add(name);
            }


            foreach (string name in delete)
            {
                listBox_ignore.Items.Remove(name);
            }

            initial = false;
            if (checkedListBox1.SelectedIndex == 0)
            {
                for (int i = 1; i < checkedListBox1.Items.Count; i++)
                {
                    for (int j = 0; j < delete.Count; j++)
                    {
                        if (SearchCheck(i, delete[j]))
                        {
                            listCount[0]++;
                            listCount[i]++;
                        }
                    }
                }
            }
            else
            {
                listCount[0] += delete.Count;
                listCount[checkedListBox1.SelectedIndex] += delete.Count;
            }
            for (int i = 1; i < checkedListBox1.Items.Count; i++)
            {
                if (listCount[i] != 0)
                {
                    checkedListBox1.SetItemChecked(i, false);
                }
            }
            initial = true;

            listBox_ignore.EndUpdate();
            listBox_result.EndUpdate();
        }

        private void EditIgnoreList_Load(object sender, EventArgs e)
        {
            checkedListBox1.Items.Add("全て");
            checkedListBox1.SelectedIndex = 0;           

            switch (mode)
            {
                case Mode.Equip:
                    checkedListBox1.Items.AddRange(basedata.ClassList.ToArray());
                    radioButton_equip.Checked = true;
                    break;
                case Mode.Jewelry:
                    checkedListBox1.Items.AddRange(basedata.ClassList_Jewelry.ToArray());
                    radioButton_jewelry.Checked = true;
                    break;
                case Mode.Skill:
                    radioButton_skill.Checked = true;
                    break;
            }
            initialSearch();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void listBox_result_DoubleClick(object sender, EventArgs e)
        {
            button_add_Click(null, null);
        }

        private void listBox_ignore_DoubleClick(object sender, EventArgs e)
        {
            button_del_Click(null, null);
        }

        private void listBox_ignore_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Delete)
            {
                button_del_Click(null, null);
            }
        }

        private void radioButton_equip_CheckedChanged(object sender, EventArgs e)
        {
            checkedListBox1.BeginUpdate();

            checkedListBox1.Items.Clear();
            checkedListBox1.Items.Add("全て");
            checkedListBox1.SelectedIndex = 0;
            checkedListBox1.Items.AddRange(basedata.ClassList.ToArray());
            checkedListBox1.EndUpdate();

            checkedListBox1.Enabled = true;
            mode = Mode.Equip;
            
            Search();
            initialSearch();
        }

        private void radioButton_jewelry_CheckedChanged(object sender, EventArgs e)
        {

            if (mode != Mode.Jewelry && mode != Mode.SKillCuff)
            {
                checkedListBox1.BeginUpdate();

                checkedListBox1.Items.Clear();
                checkedListBox1.Items.Add("全て");
                checkedListBox1.SelectedIndex = 0;
                checkedListBox1.Items.AddRange(basedata.ClassList_Jewelry.ToArray());
                checkedListBox1.EndUpdate();

                checkedListBox1.Enabled = true;

            }

            mode = Mode.Jewelry;
            
            Search();
            initialSearch();
        }

        private void radioButton_skill_CheckedChanged(object sender, EventArgs e)
        {
            checkedListBox1.Enabled = false;
            mode = Mode.Skill;
            Search();
        }

        private void radioButton_skillcuff_CheckedChanged(object sender, EventArgs e)
        {
            if (mode != Mode.Jewelry && mode != Mode.SKillCuff)
            {
                checkedListBox1.BeginUpdate();

                checkedListBox1.Items.Clear();
                checkedListBox1.Items.Add("全て");
                checkedListBox1.SelectedIndex = 0;
                checkedListBox1.Items.AddRange(basedata.ClassList_Jewelry.ToArray());
                checkedListBox1.EndUpdate();

                checkedListBox1.Enabled = true;

            }

            mode = Mode.SKillCuff;
            
            Search();
            initialSearch();
        }

        private void radioButton_item_CheckedChanged(object sender, EventArgs e)
        {
            mode = Mode.Item;
            checkedListBox1.Enabled = false;
            Search();

        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (!initial)
                return;

            initial = false;
            listBox_ignore.BeginUpdate();
            listBox_result.BeginUpdate();
            List<string> tgt = null;
            switch (mode)
            {
                case Mode.Equip:
                    tgt = ignore.Equip;
                    break;
                case Mode.Jewelry:
                    tgt = ignore.Jewelry;
                    break;
                case Mode.Skill:
                    tgt = ignore.skill;
                    break;
                case Mode.SKillCuff:
                    tgt = ignore.SkillCuff;
                    break;
                case Mode.Item:
                    tgt = ignore.Item;
                    break;
            }
            string name;
            if(e.CurrentValue != CheckState.Checked)
            {
                
                for(int i = 0; i < listBox_result.Items.Count; i++)
                {
                    name = listBox_result.Items[i].ToString();
                    tgt.Add(name);
                    listBox_ignore.Items.Add(name);
                }
                listBox_result.Items.Clear();
                listCount[checkedListBox1.SelectedIndex] = 0;


            }
            else
            {
                for (int i = 0; i < listBox_ignore.Items.Count; i++)
                {
                    name = listBox_ignore.Items[i].ToString();
                    tgt.Remove(name);
                    listBox_result.Items.Add(name);
                }
                listBox_ignore.Items.Clear();
                listCount[checkedListBox1.SelectedIndex] = listSum[checkedListBox1.SelectedIndex];
            }
            listBox_ignore.EndUpdate();
            listBox_result.EndUpdate();

            

            if (checkedListBox1.SelectedIndex == 0 && listBox_result.Items.Count == 0)
            {
                for (int i = 0; i < listCount.Count; i++)
                {
                    listCount[i] = 0;
                    checkedListBox1.SetItemChecked(i, true);
                }
            }
            else if (checkedListBox1.SelectedIndex == 0 && listBox_ignore.Items.Count == 0)
            {
                for (int i = 0; i < listCount.Count; i++)
                {
                    listCount[i] = listSum[i];
                    checkedListBox1.SetItemChecked(i, false);
                }
            }

            initial = true;
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Search();
        }
    }
}
