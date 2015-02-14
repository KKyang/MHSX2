using System;
using System.Collections.Generic;

using System.Text;
using System.Windows.Forms;


namespace MHSX2
{

    public class SearchSkillView : System.Windows.Forms.ListView
    {
        private NumericUpDown numericUpDown1;

        public SearchSkillView()
        {
            base.UseCompatibleStateImageBehavior = false;
            InitializeComponent();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            ListViewHitTestInfo info = HitTest(e.Location);

            if (info.SubItem != null)
            {
                if (info.SubItem.Name == "point")
                {
                    int point = int.Parse(info.SubItem.Text);
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

        private void InitializeComponent()
        {
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.numericUpDown1.Location = new System.Drawing.Point(0, 0);
            this.numericUpDown1.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(120, 19);
            this.numericUpDown1.TabIndex = 0;
            this.numericUpDown1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDown1.Leave += new System.EventHandler(this.numericUpDown1_Leave);
            this.numericUpDown1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.numericUpDown1_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);

        }



        public void UpdateLine(int n)
        {
            ListViewItem item = Items[n];
            SkillBase sbase = (SkillBase)item.Tag;
            int point = int.Parse(item.SubItems[1].Text);

            SkillOption so = sbase.GetOption(point);

            if (so != null)
                item.SubItems[2].Text = so.Name;
            else
                item.SubItems[2].Text = "";

            if (point < 0)
                item.SubItems[2].ForeColor = System.Drawing.Color.Red;
            else
                item.SubItems[2].ForeColor = System.Drawing.Color.Black;
        }

        private void numericUpDown1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                numericUpDown1_Leave(sender, e);
        }

        private void numericUpDown1_Leave(object sender, EventArgs e)
        {
            ListViewHitTestInfo info = (ListViewHitTestInfo)numericUpDown1.Tag;

            info.SubItem.Text = numericUpDown1.Value.ToString();

            UpdateLine(info.Item.Index);
            Controls.Remove(numericUpDown1);
        }

        public SortedList<SkillBase, SkillPointCondition> GetSkillOptionTable()
        {
            SortedList<SkillBase, SkillPointCondition> table = new SortedList<SkillBase, SkillPointCondition>();
            foreach (ListViewItem item in Items)
            {
                if (item.Checked)
                {
                    SkillPointCondition point = new SkillPointCondition();
                    point.Point = int.Parse(item.SubItems[1].Text);
                    point.SBase = (SkillBase)item.Tag;
                    table[point.SBase] = point;
                }
            }

            return table;
        }

        public void SetSkillOptionTable(SortedList<SkillBase, SkillPointCondition> value)
        {
            foreach (KeyValuePair<SkillBase, SkillPointCondition> pair in value)
            {
                AddSkillOption(pair.Key, pair.Value.Point);
            }
        }



        public void AddSkillOption(SkillBase sbase, int Point)
        {
            ListViewItem item = null;
            foreach (ListViewItem i in Items)
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



                Items.Add(item);
            }
            else
            {
                item.SubItems[1].Text = Point.ToString();
                UpdateLine(item.Index);
                item.Checked = true;
            }
        }




    }
}
