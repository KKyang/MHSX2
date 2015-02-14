using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MHSX2
{
    public partial class TypeOrderListView : ListView
    {
        public const string NAME_UNDER = "UNDER";
        public const string NAME_UPPER = "UPPER";


        public TypeOrderListView()
        {
            InitializeComponent();


            Columns.Add("防具種").Width = 100;
            Columns.Add("下限").Width = 100;
            Columns.Add("上限").Width = 100;


        }

        private void TypeOrderListView_MouseClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo info = HitTest(e.Location);

            if (info.SubItem != null)
            {
                if (info.SubItem.Name == NAME_UNDER || info.SubItem.Name == NAME_UPPER)
                {
                    if (info.SubItem.Name == NAME_UNDER)
                    {
                        int sum = 0;
                        foreach (ListViewItem item in Items)
                        {
                            if (item != info.Item)
                            {
                                sum += (int)item.SubItems[NAME_UNDER].Tag;
                            }
                        }

                        numericUpDown1.Minimum = 0;

                        int upper = (int)info.Item.SubItems[NAME_UPPER].Tag;
                       
                        numericUpDown1.Maximum = (5 - sum < upper) ? 5 - sum : upper;

                    }
                    else if (info.SubItem.Name == NAME_UPPER)
                    {
                        numericUpDown1.Minimum = (int)info.Item.SubItems[NAME_UNDER].Tag;
                        numericUpDown1.Maximum = 5;
                    }


                    int point = (int)info.SubItem.Tag;

                    numericUpDown1.Value = point;
                    numericUpDown1.Location = info.SubItem.Bounds.Location;
                    numericUpDown1.Size = info.SubItem.Bounds.Size;
                    numericUpDown1.Tag = info;



                    Controls.Add(numericUpDown1);
                    numericUpDown1.Focus();
                    return;
                }
            }
        }

        private void numericUpDown1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                numericUpDown1_Leave(sender, e);
        }

        private void numericUpDown1_Leave(object sender, EventArgs e)
        {
            ListViewHitTestInfo info = (ListViewHitTestInfo)numericUpDown1.Tag;

            info.SubItem.Tag = (int)numericUpDown1.Value;

            if(info.SubItem.Name == NAME_UNDER)
                info.SubItem.Text = numericUpDown1.Value.ToString() + "箇所以上";
            else if(info .SubItem.Name == NAME_UPPER)
                info.SubItem.Text = numericUpDown1.Value.ToString() + "箇所以下";



            Controls.Remove(numericUpDown1);
        }



    }
}
