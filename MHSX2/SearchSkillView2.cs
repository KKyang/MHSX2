using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace MHSX2
{
    class SearchSkillView2 : ListView
    {
        private NumericUpDown numericUpDown1;


        public SearchSkillView2()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // numericUpDown1
            // 
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

            base.OnMouseDown(e);
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

            Controls.Remove(numericUpDown1);

        }
    }
}
