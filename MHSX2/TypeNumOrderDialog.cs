using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MHSX2
{
    public partial class TypeNumOrderDialog : Form
    {
        public TypeNumOrderDialog(NumRangeOrder SP, NumRangeOrder Blank, Dictionary<string, NumRangeOrder> TypeOrder)
        {
            InitializeComponent();

            item_SP.SubItems.Add("").Name = "UNDER";
            item_blank.SubItems.Add("").Name = "UNDER";
            item_SP.SubItems.Add("").Name = "UPPER";
            item_blank.SubItems.Add("").Name = "UPPER";



            typeOrderListView1.Items.Add(item_blank);
            typeOrderListView1.Items.Add(item_SP);


            SPNumOrder = SP;
            BlankNumOrder = Blank;
            TypeOrderTable = TypeOrder;

        }

        public Dictionary<string, NumRangeOrder> TypeOrderTable
        {
            set
            {
                foreach (KeyValuePair<string, NumRangeOrder> pair in value)
                {
                    ListViewItem item = new ListViewItem(pair.Key);

                    item.Tag = "Type";
                    item.SubItems.Add(pair.Value.Under.ToString() + "箇所以上").Name = "UNDER";
                    item.SubItems["UNDER"].Tag = pair.Value.Under;

                    item.SubItems.Add(pair.Value.Upper.ToString() + "箇所以下").Name = "UPPER";
                    item.SubItems[2].Tag = pair.Value.Upper;

                    typeOrderListView1.Items.Add(item);
                }

            }

            get
            {

                Dictionary<string, NumRangeOrder> ret = new Dictionary<string, NumRangeOrder>();

                foreach (ListViewItem item in typeOrderListView1.Items)
                {
                    if ((string)item.Tag != "Type")
                        continue;

                    ret.Add(item.Text,  new NumRangeOrder((int)item.SubItems[1].Tag,(int)item.SubItems[2].Tag));
                }

                return ret;
            }

        }

        private ListViewItem item_SP = new ListViewItem("SP防具");

        public NumRangeOrder SPNumOrder
        {
            get
            {
                return new NumRangeOrder((int)item_SP.SubItems[TypeOrderListView.NAME_UNDER].Tag, (int)item_SP.SubItems[TypeOrderListView.NAME_UPPER].Tag);

            }
            set
            {
                item_SP.SubItems[1].Tag = value.Under;
                item_SP.SubItems[1].Text = value.Under.ToString() + "箇所以上";

                item_SP.SubItems[2].Tag = value.Upper;
                item_SP.SubItems[2].Text = value.Upper.ToString() + "箇所以下";
            }
        }

        private ListViewItem item_blank = new ListViewItem(Properties.Resources.BLANK_EQUIP_NAME);

        public NumRangeOrder BlankNumOrder
        {
            get
            {
                return new NumRangeOrder((int)item_blank.SubItems[TypeOrderListView.NAME_UNDER].Tag, (int)item_blank.SubItems[TypeOrderListView.NAME_UPPER].Tag);


            }
            set
            {
                item_blank.SubItems[1].Tag = value.Under;
                item_blank.SubItems[1].Text = value.Under.ToString() + "箇所以上";

                item_blank.SubItems[2].Tag = value.Upper;
                item_blank.SubItems[2].Text = value.Upper.ToString() + "箇所以下";
            }
        }

    }
}
