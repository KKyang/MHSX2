namespace MHSX2
{
    partial class SkillFilterDialog
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
            this.components = new System.ComponentModel.Container();
            this.listView_skilloption = new System.Windows.Forms.ListView();
            this.columnHeader46 = new System.Windows.Forms.ColumnHeader();
            this.button_add = new System.Windows.Forms.Button();
            this.button_delete = new System.Windows.Forms.Button();
            this.button_deleteall = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_cancel = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.削除ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.listView_SearchSkill = new MHSX2.SearchSkillView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
            this.treeView_skill = new MHSX2.SkillBaseTreeView();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView_skilloption
            // 
            this.listView_skilloption.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_skilloption.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader46});
            this.listView_skilloption.FullRowSelect = true;
            this.listView_skilloption.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listView_skilloption.HideSelection = false;
            this.listView_skilloption.Location = new System.Drawing.Point(176, 12);
            this.listView_skilloption.MultiSelect = false;
            this.listView_skilloption.Name = "listView_skilloption";
            this.listView_skilloption.ShowGroups = false;
            this.listView_skilloption.Size = new System.Drawing.Size(259, 65);
            this.listView_skilloption.TabIndex = 1;
            this.listView_skilloption.UseCompatibleStateImageBehavior = false;
            this.listView_skilloption.View = System.Windows.Forms.View.Details;
            this.listView_skilloption.DoubleClick += new System.EventHandler(this.listView_skilloption_DoubleClick);
            // 
            // columnHeader46
            // 
            this.columnHeader46.Width = 210;
            // 
            // button_add
            // 
            this.button_add.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_add.Location = new System.Drawing.Point(441, 12);
            this.button_add.Name = "button_add";
            this.button_add.Size = new System.Drawing.Size(38, 24);
            this.button_add.TabIndex = 2;
            this.button_add.Text = "追加";
            this.button_add.UseVisualStyleBackColor = true;
            this.button_add.Click += new System.EventHandler(this.button_add_Click);
            // 
            // button_delete
            // 
            this.button_delete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_delete.Location = new System.Drawing.Point(176, 347);
            this.button_delete.Name = "button_delete";
            this.button_delete.Size = new System.Drawing.Size(60, 21);
            this.button_delete.TabIndex = 4;
            this.button_delete.Text = "削除";
            this.button_delete.UseVisualStyleBackColor = true;
            this.button_delete.Click += new System.EventHandler(this.button_delete_Click);
            // 
            // button_deleteall
            // 
            this.button_deleteall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_deleteall.Location = new System.Drawing.Point(242, 347);
            this.button_deleteall.Name = "button_deleteall";
            this.button_deleteall.Size = new System.Drawing.Size(64, 21);
            this.button_deleteall.TabIndex = 5;
            this.button_deleteall.Text = "全て削除";
            this.button_deleteall.UseVisualStyleBackColor = true;
            this.button_deleteall.Click += new System.EventHandler(this.button_deleteall_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button_OK.Location = new System.Drawing.Point(349, 347);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(62, 21);
            this.button_OK.TabIndex = 6;
            this.button_OK.Text = "OK";
            this.button_OK.UseVisualStyleBackColor = true;
            // 
            // button_cancel
            // 
            this.button_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_cancel.Location = new System.Drawing.Point(417, 347);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(62, 21);
            this.button_cancel.TabIndex = 7;
            this.button_cancel.Text = "キャンセル";
            this.button_cancel.UseVisualStyleBackColor = true;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.削除ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(101, 26);
            // 
            // 削除ToolStripMenuItem
            // 
            this.削除ToolStripMenuItem.Name = "削除ToolStripMenuItem";
            this.削除ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.削除ToolStripMenuItem.Text = "削除";
            this.削除ToolStripMenuItem.Click += new System.EventHandler(this.削除ToolStripMenuItem_Click);
            // 
            // listView_SearchSkill
            // 
            this.listView_SearchSkill.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_SearchSkill.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader6,
            this.columnHeader5});
            this.listView_SearchSkill.ContextMenuStrip = this.contextMenuStrip1;
            this.listView_SearchSkill.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.listView_SearchSkill.FullRowSelect = true;
            this.listView_SearchSkill.GridLines = true;
            this.listView_SearchSkill.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listView_SearchSkill.Location = new System.Drawing.Point(176, 83);
            this.listView_SearchSkill.Name = "listView_SearchSkill";
            this.listView_SearchSkill.Size = new System.Drawing.Size(303, 258);
            this.listView_SearchSkill.TabIndex = 3;
            this.listView_SearchSkill.UseCompatibleStateImageBehavior = false;
            this.listView_SearchSkill.View = System.Windows.Forms.View.Details;
            this.listView_SearchSkill.SelectedIndexChanged += new System.EventHandler(this.listView_SearchSkill_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "スキル";
            this.columnHeader1.Width = 93;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "値";
            this.columnHeader6.Width = 34;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "発動スキル名";
            this.columnHeader5.Width = 132;
            // 
            // treeView_skill
            // 
            this.treeView_skill.AllowDrop = true;
            this.treeView_skill.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.treeView_skill.Location = new System.Drawing.Point(12, 12);
            this.treeView_skill.Name = "treeView_skill";
            this.treeView_skill.Size = new System.Drawing.Size(158, 356);
            this.treeView_skill.TabIndex = 0;
            this.treeView_skill.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView_skill_NodeMouseDoubleClick);
            this.treeView_skill.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_skill_AfterSelect);
            // 
            // SkillFilterDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_cancel;
            this.ClientSize = new System.Drawing.Size(491, 380);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.listView_skilloption);
            this.Controls.Add(this.button_add);
            this.Controls.Add(this.button_delete);
            this.Controls.Add(this.button_deleteall);
            this.Controls.Add(this.listView_SearchSkill);
            this.Controls.Add(this.treeView_skill);
            this.MinimumSize = new System.Drawing.Size(475, 269);
            this.Name = "SkillFilterDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "スキルフィルター";
            this.Load += new System.EventHandler(this.SkillFilterDialog_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView_skilloption;
        private System.Windows.Forms.ColumnHeader columnHeader46;
        private System.Windows.Forms.Button button_add;
        private System.Windows.Forms.Button button_delete;
        private System.Windows.Forms.Button button_deleteall;
        public SearchSkillView listView_SearchSkill;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private SkillBaseTreeView treeView_skill;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 削除ToolStripMenuItem;
    }
}