namespace MHSX2
{
    partial class EditIgnoreList
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditIgnoreList));
            this.button_search = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.listBox_result = new System.Windows.Forms.ListBox();
            this.listBox_ignore = new System.Windows.Forms.ListBox();
            this.button_add = new System.Windows.Forms.Button();
            this.button_del = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.radioButton_equip = new System.Windows.Forms.RadioButton();
            this.radioButton_jewelry = new System.Windows.Forms.RadioButton();
            this.radioButton_skill = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.radioButton_skillcuff = new System.Windows.Forms.RadioButton();
            this.radioButton_item = new System.Windows.Forms.RadioButton();
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_search
            // 
            this.button_search.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_search.Location = new System.Drawing.Point(525, 38);
            this.button_search.Name = "button_search";
            this.button_search.Size = new System.Drawing.Size(39, 20);
            this.button_search.TabIndex = 6;
            this.button_search.Text = "検索";
            this.button_search.UseVisualStyleBackColor = true;
            this.button_search.Click += new System.EventHandler(this.button_search_Click);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(12, 38);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(507, 22);
            this.textBox1.TabIndex = 5;
            this.textBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
            // 
            // listBox_result
            // 
            this.listBox_result.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox_result.FormattingEnabled = true;
            this.listBox_result.ItemHeight = 12;
            this.listBox_result.Location = new System.Drawing.Point(6, 15);
            this.listBox_result.Name = "listBox_result";
            this.listBox_result.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBox_result.Size = new System.Drawing.Size(169, 376);
            this.listBox_result.Sorted = true;
            this.listBox_result.TabIndex = 0;
            this.listBox_result.DoubleClick += new System.EventHandler(this.listBox_result_DoubleClick);
            // 
            // listBox_ignore
            // 
            this.listBox_ignore.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox_ignore.FormattingEnabled = true;
            this.listBox_ignore.ItemHeight = 12;
            this.listBox_ignore.Location = new System.Drawing.Point(6, 15);
            this.listBox_ignore.Name = "listBox_ignore";
            this.listBox_ignore.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBox_ignore.Size = new System.Drawing.Size(182, 376);
            this.listBox_ignore.Sorted = true;
            this.listBox_ignore.TabIndex = 0;
            this.listBox_ignore.DoubleClick += new System.EventHandler(this.listBox_ignore_DoubleClick);
            this.listBox_ignore.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.listBox_ignore_KeyPress);
            // 
            // button_add
            // 
            this.button_add.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.button_add.Location = new System.Drawing.Point(318, 230);
            this.button_add.Name = "button_add";
            this.button_add.Size = new System.Drawing.Size(30, 26);
            this.button_add.TabIndex = 8;
            this.button_add.Text = "->";
            this.button_add.UseVisualStyleBackColor = true;
            this.button_add.Click += new System.EventHandler(this.button_add_Click);
            // 
            // button_del
            // 
            this.button_del.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.button_del.Location = new System.Drawing.Point(318, 262);
            this.button_del.Name = "button_del";
            this.button_del.Size = new System.Drawing.Size(30, 26);
            this.button_del.TabIndex = 9;
            this.button_del.Text = "<-";
            this.button_del.UseVisualStyleBackColor = true;
            this.button_del.Click += new System.EventHandler(this.button_del_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(490, 486);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 11;
            this.button_OK.Text = "OK";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // radioButton_equip
            // 
            this.radioButton_equip.AutoSize = true;
            this.radioButton_equip.Location = new System.Drawing.Point(12, 13);
            this.radioButton_equip.Name = "radioButton_equip";
            this.radioButton_equip.Size = new System.Drawing.Size(59, 16);
            this.radioButton_equip.TabIndex = 0;
            this.radioButton_equip.TabStop = true;
            this.radioButton_equip.Text = "装備品";
            this.radioButton_equip.UseVisualStyleBackColor = true;
            this.radioButton_equip.CheckedChanged += new System.EventHandler(this.radioButton_equip_CheckedChanged);
            // 
            // radioButton_jewelry
            // 
            this.radioButton_jewelry.AutoSize = true;
            this.radioButton_jewelry.Location = new System.Drawing.Point(77, 13);
            this.radioButton_jewelry.Name = "radioButton_jewelry";
            this.radioButton_jewelry.Size = new System.Drawing.Size(59, 16);
            this.radioButton_jewelry.TabIndex = 1;
            this.radioButton_jewelry.TabStop = true;
            this.radioButton_jewelry.Text = "装飾品";
            this.radioButton_jewelry.UseVisualStyleBackColor = true;
            this.radioButton_jewelry.CheckedChanged += new System.EventHandler(this.radioButton_jewelry_CheckedChanged);
            // 
            // radioButton_skill
            // 
            this.radioButton_skill.AutoSize = true;
            this.radioButton_skill.Location = new System.Drawing.Point(275, 13);
            this.radioButton_skill.Name = "radioButton_skill";
            this.radioButton_skill.Size = new System.Drawing.Size(59, 16);
            this.radioButton_skill.TabIndex = 3;
            this.radioButton_skill.TabStop = true;
            this.radioButton_skill.Text = "スキル";
            this.radioButton_skill.UseVisualStyleBackColor = true;
            this.radioButton_skill.CheckedChanged += new System.EventHandler(this.radioButton_skill_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.listBox_result);
            this.groupBox1.Location = new System.Drawing.Point(117, 74);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(181, 403);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "許可リスト";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.listBox_ignore);
            this.groupBox2.Location = new System.Drawing.Point(370, 74);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(194, 403);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "除外リスト";
            // 
            // radioButton_skillcuff
            // 
            this.radioButton_skillcuff.AutoSize = true;
            this.radioButton_skillcuff.Location = new System.Drawing.Point(142, 12);
            this.radioButton_skillcuff.Name = "radioButton_skillcuff";
            this.radioButton_skillcuff.Size = new System.Drawing.Size(83, 16);
            this.radioButton_skillcuff.TabIndex = 2;
            this.radioButton_skillcuff.TabStop = true;
            this.radioButton_skillcuff.Text = "スキルカフ";
            this.radioButton_skillcuff.UseVisualStyleBackColor = true;
            this.radioButton_skillcuff.CheckedChanged += new System.EventHandler(this.radioButton_skillcuff_CheckedChanged);
            // 
            // radioButton_item
            // 
            this.radioButton_item.AutoSize = true;
            this.radioButton_item.Location = new System.Drawing.Point(217, 12);
            this.radioButton_item.Name = "radioButton_item";
            this.radioButton_item.Size = new System.Drawing.Size(47, 16);
            this.radioButton_item.TabIndex = 12;
            this.radioButton_item.TabStop = true;
            this.radioButton_item.Text = "素材";
            this.radioButton_item.UseVisualStyleBackColor = true;
            this.radioButton_item.CheckedChanged += new System.EventHandler(this.radioButton_item_CheckedChanged);
            // 
            // checkedListBox1
            // 
            this.checkedListBox1.FormattingEnabled = true;
            this.checkedListBox1.Location = new System.Drawing.Point(12, 82);
            this.checkedListBox1.Name = "checkedListBox1";
            this.checkedListBox1.Size = new System.Drawing.Size(99, 395);
            this.checkedListBox1.TabIndex = 13;
            this.checkedListBox1.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBox1_ItemCheck);
            this.checkedListBox1.SelectedIndexChanged += new System.EventHandler(this.checkedListBox1_SelectedIndexChanged);
            // 
            // EditIgnoreList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(577, 521);
            this.Controls.Add(this.checkedListBox1);
            this.Controls.Add(this.radioButton_item);
            this.Controls.Add(this.radioButton_skillcuff);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.radioButton_skill);
            this.Controls.Add(this.radioButton_jewelry);
            this.Controls.Add(this.radioButton_equip);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.button_del);
            this.Controls.Add(this.button_add);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button_search);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EditIgnoreList";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "除外指定リスト編集";
            this.Load += new System.EventHandler(this.EditIgnoreList_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_search;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ListBox listBox_result;
        private System.Windows.Forms.ListBox listBox_ignore;
        private System.Windows.Forms.Button button_add;
        private System.Windows.Forms.Button button_del;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.RadioButton radioButton_equip;
        private System.Windows.Forms.RadioButton radioButton_jewelry;
        private System.Windows.Forms.RadioButton radioButton_skill;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton radioButton_skillcuff;
        private System.Windows.Forms.RadioButton radioButton_item;
        private System.Windows.Forms.CheckedListBox checkedListBox1;
    }
}