using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using System.Net;

namespace MHSX2
{
    public partial class EquipDataDetaileDialog : Form
    {
        Settings setting;
        BaseData Base;
        EquipmentData Equip;
        JewelryData_base Jewelry;
        int Level_;
        List<Control> AddCtrlList = new List<Control>();
        Sex sex;
        int Column = -1;
        SortOrder order = SortOrder.None;

        public EquipDataDetaileDialog(BaseData basedata, Settings setting, EquipmentData data, int level, Sex sex)
        {
            this.sex = sex;
            this.setting = setting;
            Base = basedata;
            Equip = data;
            Level_ = level;
            InitializeComponent();
        }

        public EquipDataDetaileDialog(BaseData basedata, Settings setting, JewelryData_base data)
        {
            this.setting = setting;
            Base = basedata;
            Jewelry = data;
            InitializeComponent();
        }


        private void EquipDataDetaileDialog_Load(object sender, EventArgs e)
        {

            if (Equip != null)
            {

                numericUpDown1.Maximum = new decimal(Equip.LevelList.Count);

                Text = Equip.Name + "詳細";


                label_name.Text = Equip.Name;

                switch (Equip.WearableJob)
                {
                    case JobType.COMON:
                        label_job.Text = "共用";
                        break;
                    case JobType.KNIGHT:
                        label_job.Text = "剣士";
                        break;
                    case JobType.GUNNER:
                        label_job.Text = "ガンナー";
                        break;
                }

                switch (Equip.WearableSex)
                {
                    case SexType.COMON:
                        label_sex.Text = "共用";
                        break;

                    case SexType.MAN:
                        label_sex.Text = "男";
                        break;

                    case SexType.WOMAN:
                        label_sex.Text = "女";
                        break;
                }

                label_rare.Text = Equip.Rare.ToString();
                label_class.Text = Equip.Class;


                string[] str = new string[(int)ElementType.NumOfElementKind];

                for (int i = 0; i < (int)ElementType.NumOfElementKind; i++)
                {
                    if (Equip.Element[(ElementType)i] > 0)
                    {
                        str[i] = "+" + Equip.Element[(ElementType)i].ToString();
                    }
                    else
                        str[i] = Equip.Element[(ElementType)i].ToString();

                }

                label_element.Text = "火：" + str[(int)ElementType.Fire].ToString() +
                    "　水：" + str[(int)ElementType.Water].ToString() +
                    "　雷：" + str[(int)ElementType.Thunder].ToString() +
                    "　氷：" + str[(int)ElementType.Ice].ToString() +
                    "　龍：" + str[(int)ElementType.Dragon].ToString();

                foreach (Control l in AddCtrlList)
                {
                    splitContainer1.Panel1.Controls.Remove(l);
                }

                AddCtrlList.Clear();

                Point pt;

                if (Equip.DerivSource != null)
                {
                    pt = new Point(label_sourcelabel.Right + 9, label_sourcelabel.Top + 3);


                    EquipmentData data = Base.EquipDataMap[(int)Equip.Kind][Equip.DerivSource.Source];


                    LinkLabel link = new LinkLabel();

                    link.Location = new Point(pt.X, pt.Y);
                    link.MouseClick += new MouseEventHandler(link_MouseClick);
                    link.Tag = new KeyValuePair<EquipmentData, int>(data, Equip.DerivSource.level);
                    link.Font = new Font(link.Font.FontFamily, (float)12);

                    link.AutoSize = true;
                    link.Name = data.Name + "_label";
                    link.TabStop = true;
                    link.Text = Equip.DerivSource.Source + " Lv" + Equip.DerivSource.level.ToString();


                    AddCtrlList.Add(link);

                    splitContainer1.Panel1.Controls.Add(link);

                }



                pt = new Point(label_deriv.Right + 9, label_deriv.Top + 3);



                foreach (KeyValuePair<int, EquipmentData> pair in Equip.Derivation)
                {

                    Label label = new Label();
                    label.Location = pt;
                    label.Font = new Font(label.Font.FontFamily, (float)12);
                    label.AutoSize = true;

                    label.Text = "Lv" + pair.Key.ToString() + "　→";
                    AddCtrlList.Add(label);

                    splitContainer1.Panel1.Controls.Add(label);


                    LinkLabel link = new LinkLabel();

                    link.Location = new Point(pt.X + 60, pt.Y);
                    link.MouseClick += new MouseEventHandler(link_MouseClick);
                    link.Tag = new KeyValuePair<EquipmentData, int>(pair.Value, pair.Value.LevelList.Count);
                    link.Font = new Font(link.Font.FontFamily, (float)12);

                    link.AutoSize = true;
                    link.Name = pair.Value.Name + "_label";
                    link.TabStop = true;
                    link.Text = pair.Value.Name;


                    AddCtrlList.Add(link);

                    splitContainer1.Panel1.Controls.Add(link);


                    pt.Y += 24;
                }



                label_skill.Text = null;
                foreach (SkillPoint sp in Equip.SkillPointList)
                {
                    string point;
                    if (sp.Point > 0)
                        point = "+" + sp.Point.ToString();
                    else
                        point = sp.Point.ToString();

                    label_skill.Text += sp.SBase.Name + "：" + point + "\r\n";
                }

                Dictionary<Item, int> dict = new Dictionary<Item, int>();
                int SumMoney = 0;

                listView_level.Items.Clear();
                for (int i = 0; i < Equip.LevelList.Count; i++)
                {
                    Level level = Equip.LevelList[i];
                    if (level == null)
                        break;

                    SumMoney += level.Money;

                    ListViewItem item = new ListViewItem();
                    item.Text = (i + 1).ToString();
                    item.SubItems.Add(level.Def.ToString());


                    string slot = "";
                    for (int k = 0; k < level.Slot; k++)
                        slot += "○";

                    item.SubItems.Add(slot);
                    item.SubItems.Add(level.Money.ToString());

                    item.SubItems.Add(level.GetableHRString);


                    string items = "";

                    foreach (KeyValuePair<Item, int> pair in level.CostItems)
                    {
                        if (items != "")
                            items += "、";

                        items += pair.Key.name + "×" + pair.Value.ToString();


                        if (i < Level_)
                        {
                            if (dict.ContainsKey(pair.Key))
                            {
                                dict[pair.Key] += pair.Value;
                            }
                            else
                                dict.Add(pair.Key, pair.Value);
                        }
                    }

                    item.SubItems.Add(items);

                    listView_level.Items.Add(item);
                }


                numericUpDown1.Value = Level_;
                numericUpDown1_ValueChanged(null, null);

                if (Equip.Type != "")
                    label_type.Text = Equip.Type;
                else
                    label_type.Text = "なし";


                if (Equip.RustaLv == 0)
                {
                    label_Rusta.Text = "開放不可";
                }
                else
                {
                    label_Rusta.Text = Equip.RustaLv.ToString();
                }

                pictureBox1.Image = null;
                pictureBox1.LoadCompleted += new AsyncCompletedEventHandler(wc_DownloadFileCompleted);

                if (!Directory.Exists(MHSX2.Properties.Resources.DNAME_PICT))
                {
                    Directory.CreateDirectory(MHSX2.Properties.Resources.DNAME_PICT);
                }
                else
                {
                    string PictName = "";

                    switch (sex.type)
                    {
                        case SexType.MAN:
                            PictName = Equip.Pict_M;
                            break;
                        case SexType.WOMAN:
                            PictName = Equip.Pict_W;
                            break;
                        case SexType.COMON:
                            if (Equip.Pict_M != "")
                                PictName = Equip.Pict_M;
                            else
                                PictName = Equip.Pict_W;
                            break;
                    }

                    if (PictName != "")
                    {
                        string fpath = MHSX2.Properties.Resources.DNAME_PICT + "\\" + PictName;
                        pictureBox1.Tag = null;


                        if (File.Exists(fpath))
                        {
                            try
                            {
                                pictureBox1.Load(fpath);
                            }
                            catch (Exception)
                            {
                                File.Delete(fpath);

                                if (setting.UseNetwork)
                                {
                                    pictureBox1.Tag = fpath;
                                    pictureBox1.LoadAsync(setting.PictureServer + "/" + PictName);
                                }
                            }
                        }
                        else if (setting.UseNetwork)
                        {
                            pictureBox1.Tag = fpath;
                            pictureBox1.LoadAsync(setting.PictureServer + "/" + PictName);
                        }
                    }
                }
            }
            else if (Jewelry != null)
            {

                label_sourcelabel.Visible = false;
                label_deriv.Visible = false;
                label8.Visible = false;
                label_Rusta.Text = "";
                label6.Visible = false;
                label_element.Text = "";


                Text = Jewelry.Name + "詳細";


                label_name.Text = Jewelry.Name;


                if (Jewelry is JewelryData)
                {
                    JewelryData jd = (JewelryData)Jewelry;
                    switch (jd.Job)
                    {
                        case JobType.COMON:
                            label_job.Text = "共用";
                            break;
                        case JobType.KNIGHT:
                            label_job.Text = "剣士";
                            break;
                        case JobType.GUNNER:
                            label_job.Text = "ガンナー";
                            break;
                    }

                    label_class.Text = "";
                    label_rare.Text = jd.Rare.ToString();

                    if (jd.Type == JewelryType.SP)
                    {
                        label_type.Text = "SP装飾品";
                    }
                    else
                    {
                        label_type.Text = "装飾品";
                    }

                }
                else if (Jewelry is SkillCuffData)
                {
                    SkillCuffData sc = (SkillCuffData)Jewelry;

                    label_job.Text = "-";
                    label_rare.Text = "";


                    label_type.Text = "スキルカフ";

                }


                label_class.Text = Jewelry.Class;

                label_sex.Text = "-";





                foreach (Control l in AddCtrlList)
                {
                    splitContainer1.Panel1.Controls.Remove(l);
                }

                AddCtrlList.Clear();



                label_skill.Text = null;
                foreach (SkillPoint sp in Jewelry.SkillList)
                {
                    string point;
                    if (sp.Point > 0)
                        point = "+" + sp.Point.ToString();
                    else
                        point = sp.Point.ToString();

                    label_skill.Text += sp.SBase.Name + "：" + point + "\r\n";
                }


                Dictionary<Item, int> dict = new Dictionary<Item, int>();
                int SumMoney = 0;

                listView_level.Items.Clear();



                SumMoney = Jewelry.CostMoney;

                ListViewItem item = new ListViewItem();
                item.Text = "-";
                item.SubItems.Add("-");


                string slot = "";
                for (int k = 0; k < Jewelry.Slot; k++)
                    slot += "○";

                item.SubItems.Add(slot);
                item.SubItems.Add(Jewelry.CostMoney.ToString());

                item.SubItems.Add(Jewelry.GetableHRString);

                string items = "";

                foreach (KeyValuePair<Item, int> pair in Jewelry.CostItem)
                {
                    if (items != "")
                        items += "、";

                    items += pair.Key.name + "×" + pair.Value.ToString();


                    if (dict.ContainsKey(pair.Key))
                    {
                        dict[pair.Key] += pair.Value;
                    }
                    else
                        dict.Add(pair.Key, pair.Value);

                }

                item.SubItems.Add(items);

                listView_level.Items.Add(item);



                numericUpDown1.Enabled = false;
                numericUpDown1.Value = 1;


                pictureBox1.Image = null;
            }





        }

        void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                pictureBox1.Image.Save((string)pictureBox1.Tag);
            }
        }

        void link_MouseClick(object sender, MouseEventArgs e)
        {
            LinkLabel from = (LinkLabel)sender;
            KeyValuePair<EquipmentData, int> data = (KeyValuePair<EquipmentData, int>)from.Tag;

            Equip = data.Key;
            Level_ = data.Value;

            EquipDataDetaileDialog_Load(null, null);

        }


        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Level_ = (int)numericUpDown1.Value;

            Dictionary<Item, int> dict = new Dictionary<Item, int>();
            int SumMoney = 0;


            if (Equip != null)
            {

                for (int i = 0; i < Level_; i++)
                {
                    Level lv = Equip.LevelList[i];
                    SumMoney += lv.Money;

                    foreach (KeyValuePair<Item, int> pair in lv.CostItems)
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
            else if (Jewelry != null)
            {
                SumMoney += Jewelry.CostMoney;

                dict = Jewelry.CostItem;
            }


            listView_totalitems.Items.Clear();

            foreach (KeyValuePair<Item, int> pair in dict)
            {
                ListViewItem ite = new ListViewItem(pair.Key.name);

                ite.SubItems.Add(pair.Value.ToString());

                ite.SubItems.Add(pair.Key.GetableHRString);

                ite.Tag = pair.Key;

                Object tgt = null;

                if (Base.JewelryDataMap.ContainsKey(pair.Key.name))
                {
                    tgt = Base.JewelryDataMap[pair.Key.name];
                }
                else if (Base.SkillCaffDataMap.ContainsKey(pair.Key.name))
                {
                    tgt = Base.SkillCaffDataMap[pair.Key.name];
                }
                else
                {
                    for (int i = 0; i < (int)EquipKind.NumOfEquipKind; i++)
                    {
                        if (Base.EquipDataMap[i].ContainsKey(pair.Key.name))
                        {
                            tgt = Base.EquipDataMap[i][pair.Key.name];
                            break;
                        }
                    }
                }

                if (tgt != null)
                {

                    ite.UseItemStyleForSubItems = false;
                    ite.SubItems[0].Tag = tgt;
                    ite.SubItems[0].ForeColor = System.Drawing.Color.Blue;
                    ite.SubItems[0].Font = new System.Drawing.Font(ite.Font, System.Drawing.FontStyle.Underline);

                }


                listView_totalitems.Items.Add(ite);
            }

            Column = 1;
            order = SortOrder.Descending;
            listView_totalitems.ListViewItemSorter = new Sorter(Column, order);
            listView_totalitems.Sort();


            label_costmoney.Text = "費用：" + SumMoney.ToString("N0");
        }



        private class Sorter : System.Collections.IComparer
        {
            int Column;
            SortOrder order;

            public Sorter(int c, SortOrder o)
            {
                Column = c;
                order = o;
            }


            #region IComparer メンバ

            public int Compare(object x, object y)
            {
                int ret;
                ListViewItem X, Y;
                X = (ListViewItem)x;
                Y = (ListViewItem)y;
                Item ItemX = (Item)X.Tag;
                Item ItemY = (Item)Y.Tag;

                switch (Column)
                {
                    case 0:
                        ret = X.Text.CompareTo(Y.Text);
                        break;
                    case 1:
                        ret = int.Parse(X.SubItems[1].Text) - int.Parse(Y.SubItems[1].Text);
                        break;
                    case 2:
                        ret = ItemX.HR - ItemY.HR;
                        break;
                    default:
                        ret = 0;
                        break;
                }

                if (order == SortOrder.Ascending)
                    return ret;
                else
                    return -ret;

            }

            #endregion
        }

        private void listView_totalitems_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column != Column)
            {
                Column = e.Column;
                listView_totalitems.ListViewItemSorter = new Sorter(Column, SortOrder.Descending);
            }
            else
            {
                if (order == SortOrder.Descending)
                {
                    order = SortOrder.Ascending;
                }
                else
                    order = SortOrder.Descending;

                listView_totalitems.ListViewItemSorter = new Sorter(e.Column, order);
            }

            listView_totalitems.Sort();


        }

        private void button_clip_items_Click(object sender, EventArgs e)
        {
            string CopyStr = "";

            foreach (ListViewItem item in listView_totalitems.Items)
            {
                CopyStr += item.Text + "×" + item.SubItems[1].Text + "\r\n";
            }

            CopyStr += "------------\r\n" + label_costmoney.Text;

            if (CopyStr != "")
                Clipboard.SetText(CopyStr);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://fortune.adam.ne.jp/mhf_e_wiki/index.php");
        }

        private void listView_totalitems_MouseClick(object sender, MouseEventArgs e)
        {
            ListViewItem item = listView_totalitems.GetItemAt(e.X, e.Y);

            if (item != null && item.SubItems[0].Tag != null)
            {
                Object obj = item.SubItems[0].Tag;

                if (obj is EquipmentData)
                {
                    EquipmentData ed = (EquipmentData)obj;
                    EquipDataDetaileDialog dialog = new EquipDataDetaileDialog(Base, setting, ed, 7, new Sex(setting.sex));

                    dialog.Show();
                }
                else if (obj is JewelryData_base)
                {
                    JewelryData_base jd = (JewelryData_base)obj;
                    EquipDataDetaileDialog dialog = new EquipDataDetaileDialog(Base, setting, jd);

                    dialog.Show();
                }
            }

            base.OnMouseClick(e);
        }

        private void listView_totalitems_MouseMove(object sender, MouseEventArgs e)
        {
            ListViewItem item  = listView_totalitems.GetItemAt(e.X,e.Y);

            if (item != null && item.SubItems[0].Tag != null)
            {
                this.Cursor = System.Windows.Forms.Cursors.Hand;
            }
            else
            {
                this.Cursor = System.Windows.Forms.Cursors.Default;
            }

            base.OnMouseHover(e);
        
        }

        private void listView_totalitems_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = System.Windows.Forms.Cursors.Default;
        }



    }
}
