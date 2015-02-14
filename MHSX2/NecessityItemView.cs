using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;


namespace MHSX2
{
    public class NecessaryItemView : ListView
    {
        private EquipSet ESet_;
        BaseData basedata;
        public int money = 0;
        Settings setting = null;

        private Label CostMonyLabel = null;

        public void DataSet(BaseData data, Label label,Settings sets)
        {
            basedata = data;
            CostMonyLabel = label;
            setting = sets;
        }

        protected override void OnCreateControl()
        {
            MultiSelect = false;
            base.OnCreateControl();
        }

        public EquipSet ESet
        {
            get
            {
                return ESet_;
            }

            set
            {
                ESet_ = value;
                if (ESet != null)
                    Set(ESet_);
                else
                    Items.Clear();
            }
        }


        void Set(EquipSet eset)
        {


            Dictionary<Item, int> dict = eset.GetNecessaryItemList(basedata, ref money);


            BeginUpdate();
            Items.Clear();

            foreach (KeyValuePair<Item, int> pair in dict)
            {
                ListViewItem n = new ListViewItem();

                n.UseItemStyleForSubItems = false;
                n.Text = pair.Key.name;
                n.SubItems.Add(pair.Value.ToString());

                int hr = pair.Key.HR;
                n.SubItems.Add(BaseData.mDefine.GetRankKindString(hr) + (pair.Key.HR % 1000).ToString());

                n.Tag = pair.Key;


                Object tgt = null;

                if (basedata.JewelryDataMap.ContainsKey(pair.Key.name))
                {
                    tgt = basedata.JewelryDataMap[pair.Key.name];
                }else if(basedata.SkillCaffDataMap.ContainsKey(pair.Key.name))
                {
                    tgt = basedata.SkillCaffDataMap[pair.Key.name];
                }
                else
                {
                    for (int i = 0; i < (int)EquipKind.NumOfEquipKind; i++)
                    {
                        if (basedata.EquipDataMap[i].ContainsKey(pair.Key.name))
                        {
                            tgt = basedata.EquipDataMap[i][pair.Key.name];
                            break;
                        }
                    }
                }

                if (tgt != null)
                {
                    n.SubItems[0].Tag = tgt;
                    n.SubItems[0].ForeColor = System.Drawing.Color.Blue;
                    n.SubItems[0].Font = new System.Drawing.Font(n.Font, System.Drawing.FontStyle.Underline);

                }

#if BETA
                n.SubItems[0].Name = "Name";
                n.SubItems[0].ForeColor = System.Drawing.Color.Blue;
                n.SubItems[0].Font = new System.Drawing.Font(n.Font,System.Drawing.FontStyle.Underline);
#endif

                Items.Add(n);
            }

            Sorting = SortOrder.Descending;
            ListViewItemSorter = new Sorter(1, SortOrder.Descending);
            Sort();

            EndUpdate();

            if (CostMonyLabel != null)
                CostMonyLabel.Text = "費用：" + money.ToString("N0");

        }

#if BETA
        protected override void OnMouseMove(MouseEventArgs e)
        {
            ListViewHitTestInfo info = HitTest(e.Location);

            if (info.Item != null)
            {

                if (info.SubItem.Name == "Name")
                {
                    Cursor.Current = Cursors.Hand;
                }
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            ListViewHitTestInfo info = HitTest(e.Location);

            if (info.Item != null)
            {
                if (info.SubItem.Name == "Name")
                {
                    Item item = (Item)info.Item.Tag;

                    string URL = "http://homepage2.nifty.com/ferias/sozai/sozai.htm?" + item.ID;

                    System.Diagnostics.Process.Start(URL);
                }
            }

            base.OnMouseClick(e);
        }
#endif


        protected override void OnMouseClick(MouseEventArgs e)
        {
            ListViewItem item = this.GetItemAt(e.Location.X, e.Y);

            if (item != null && item.SubItems[0].Tag != null)
            {
                Object obj = item.SubItems[0].Tag;

                if (obj is EquipmentData)
                {
                    EquipmentData ed = (EquipmentData)obj;
                    EquipDataDetaileDialog dialog = new EquipDataDetaileDialog(basedata, setting, ed, 7, new Sex(setting.sex));

                    dialog.Show();
                }
                else if (obj is JewelryData_base)
                {
                    JewelryData_base jd = (JewelryData_base)obj;
                    EquipDataDetaileDialog dialog = new EquipDataDetaileDialog(basedata, setting, jd);

                    dialog.Show();
                }
            }

            base.OnMouseClick(e);
        }


        protected override void OnMouseMove(MouseEventArgs e)
        {
            ListViewItem item  = this.GetItemAt(e.X,e.Y);

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


        protected override void OnColumnClick(ColumnClickEventArgs e)
        {
            base.OnColumnClick(e);

            if (Sorting == SortOrder.Ascending)
            {
                Sorting = SortOrder.Descending;
            }
            else
                Sorting = SortOrder.Ascending;


            ListViewItemSorter = new Sorter(e.Column, Sorting);
            Sort();
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
                        ret = int.Parse(X.SubItems[Column].Text) - int.Parse(Y.SubItems[Column].Text);
                        break;
                    case 2://HR
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

        public int GetMoney()
        {
            return money;
        }

        public void ClipItems()
        {
            string CopyStr = "";

            foreach (ListViewItem item in Items)
            {
                CopyStr += item.Text + "×" + item.SubItems[1].Text + "\r\n";
            }

            CopyStr += "------------\r\n費用：" + money.ToString("N0");

            if (CopyStr != "")
                Clipboard.SetText(CopyStr);
        }
    }
}
