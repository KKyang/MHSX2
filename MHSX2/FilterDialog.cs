using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MHSX2
{
    public partial class FilterDialog : Form
    {
        private BaseData basedata = null;
        private Settings setting = null;

        public SortedList<SkillBase, SkillPointCondition> PointTable = new SortedList<SkillBase, SkillPointCondition>();


        public enum IDList { ByEquip, BySkill, ByItem, ByJewelry, ByClass };
        public class MyComboBoxItem
        {
            private IDList m_id;
            private string m_name = "";

            //コンストラクタ
            public MyComboBoxItem(IDList id, string name)
            {
                m_id = id;
                m_name = name;
            }



            //実際の値
            public IDList Id
            {
                get
                {
                    return m_id;
                }
            }

            //表示名称
            //(このプロパティはこのサンプルでは使わないのでなくても良い)
            public string Name
            {
                get
                {
                    return m_name;
                }
            }

            //オーバーライドしたメソッド
            //これがコンボボックスに表示される
            public override string ToString()
            {
                return m_name;
            }
        }

        public FilterDialog(BaseData basedata, Settings setting)
        {
            this.basedata = basedata;
            this.setting = setting;
            InitializeComponent();


            comboBox1.Items.Add(new MyComboBoxItem(IDList.ByEquip, "使用装備"));
            comboBox1.Items.Add(new MyComboBoxItem(IDList.ByJewelry, "使用装飾品"));
            comboBox1.Items.Add(new MyComboBoxItem(IDList.BySkill, "発動スキル"));
            comboBox1.Items.Add(new MyComboBoxItem(IDList.ByItem, "使用素材"));
            comboBox1.Items.Add(new MyComboBoxItem(IDList.ByClass, "装備分類"));

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;

            comboBox5.SelectedIndex = 0;



            comboBox3.Items.Add(new MyComboBoxItem(IDList.ByItem, "使用素材"));
            comboBox3.Items.Add(new MyComboBoxItem(IDList.ByJewelry, "使用装飾品"));
            comboBox3.Items.Add(new MyComboBoxItem(IDList.ByEquip, "使用装備"));
            comboBox3.Items.Add(new MyComboBoxItem(IDList.ByClass, "装備分類"));

            comboBox3.SelectedIndex = 0;
            comboBox4.SelectedIndex = 0;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            MyComboBoxItem item = (MyComboBoxItem)comboBox1.SelectedItem;

            textBox1.AutoCompleteCustomSource.Clear();

            SetTextBoxAutoComplete(item, textBox1);

        }

        private void SetTextBoxAutoComplete(MyComboBoxItem item, TextBox tbox)
        {
            switch (item.Id)
            {
                case IDList.ByEquip:
                    for (int i = basedata.EquipDataMap.Length - 1; i >= 0; i--)
                        tbox.AutoCompleteCustomSource.AddRange(new List<string>(basedata.EquipDataMap[i].Keys).ToArray());
                    break;
                case IDList.ByItem:
                    tbox.AutoCompleteCustomSource.AddRange(new List<string>(basedata.ItemMap.Keys).ToArray());

                    break;
                case IDList.BySkill:
                    tbox.AutoCompleteCustomSource.AddRange(new List<string>(basedata.SkillOptionMap.Keys).ToArray());

                    break;
                case IDList.ByJewelry:
                    tbox.AutoCompleteCustomSource.AddRange(new List<string>(basedata.JewelryDataMap.Keys).ToArray());
                    tbox.AutoCompleteCustomSource.AddRange(new List<string>(basedata.SkillCaffDataMap.Keys).ToArray());

                    break;

                case IDList.ByClass:
                    List<string> list = new List<string>();
                    foreach (string name in basedata.ClassList)
                    {
                        list.Add(name.Replace("(", "").Replace(")", ""));
                    }

                    tbox.AutoCompleteCustomSource.AddRange(list.ToArray());
                    break;
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            MyComboBoxItem item = (MyComboBoxItem)comboBox3.SelectedItem;

            textBox2.AutoCompleteCustomSource.Clear();

            SetTextBoxAutoComplete(item, textBox2);

            //switch (item.Id)
            //{
            //    case IDList.ByItem:
            //        textBox2.AutoCompleteCustomSource.AddRange(new List<string>(basedata.ItemMap.Keys).ToArray());
            //        break;
            //    case IDList.ByJewelry:
            //        textBox2.AutoCompleteCustomSource.AddRange(new List<string>(basedata.JewelryDataMap.Keys).ToArray());
            //        textBox2.AutoCompleteCustomSource.AddRange(new List<string>(basedata.SkillCaffDataMap.Keys).ToArray());


            //        break;
            //    case IDList.ByEquip:
            //        for (int i = basedata.EquipDataMap.Length - 1; i >= 0; i--)
            //            textBox2.AutoCompleteCustomSource.AddRange(new List<string>(basedata.EquipDataMap[i].Keys).ToArray());
            //        break;
            //    case IDList.ByClass:
            //        List<string> list = new List<string>();
            //        foreach (string name in basedata.ClassList)
            //        {
            //            list.Add(name.Replace("(", "").Replace(")", ""));
            //        }

            //        textBox2.AutoCompleteCustomSource.AddRange(list.ToArray());
            //        break;
            //}
        }

        private void button_skill_order_Click(object sender, EventArgs e)
        {
            SkillFilterDialog dialog = new SkillFilterDialog(basedata, setting, PointTable);

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                PointTable = dialog.listView_SearchSkill.GetSkillOptionTable();
            }

        }




    }
}
