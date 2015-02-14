

namespace MHSX2
{
    partial class OptionDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDown_threadnum = new System.Windows.Forms.NumericUpDown();
            this.button_ok = new System.Windows.Forms.Button();
            this.button_cancel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBox_checkversion = new System.Windows.Forms.CheckBox();
            this.button_sound = new System.Windows.Forms.Button();
            this.textBox_sound = new System.Windows.Forms.TextBox();
            this.label_getfrom = new System.Windows.Forms.Label();
            this.textBox_picture_server = new System.Windows.Forms.TextBox();
            this.checkBox_sound = new System.Windows.Forms.CheckBox();
            this.checkBox_usenetwork = new System.Windows.Forms.CheckBox();
            this.checkBox_optimizeequip = new System.Windows.Forms.CheckBox();
            this.numericUpDown_maxviewcount = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_threadnum)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_maxviewcount)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(73, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "検索スレッド数";
            // 
            // numericUpDown_threadnum
            // 
            this.numericUpDown_threadnum.Location = new System.Drawing.Point(154, 18);
            this.numericUpDown_threadnum.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown_threadnum.Name = "numericUpDown_threadnum";
            this.numericUpDown_threadnum.Size = new System.Drawing.Size(43, 19);
            this.numericUpDown_threadnum.TabIndex = 1;
            this.numericUpDown_threadnum.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericUpDown_threadnum.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // button_ok
            // 
            this.button_ok.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button_ok.Location = new System.Drawing.Point(34, 261);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(77, 23);
            this.button_ok.TabIndex = 1;
            this.button_ok.Text = "OK";
            this.button_ok.UseVisualStyleBackColor = true;
            // 
            // button_cancel
            // 
            this.button_cancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_cancel.Location = new System.Drawing.Point(115, 261);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(77, 23);
            this.button_cancel.TabIndex = 2;
            this.button_cancel.Text = "キャンセル";
            this.button_cancel.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            //this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.checkBox_checkversion);
            this.groupBox1.Controls.Add(this.button_sound);
            this.groupBox1.Controls.Add(this.textBox_sound);
            this.groupBox1.Controls.Add(this.label_getfrom);
            this.groupBox1.Controls.Add(this.textBox_picture_server);
            this.groupBox1.Controls.Add(this.checkBox_sound);
            this.groupBox1.Controls.Add(this.checkBox_usenetwork);
            this.groupBox1.Controls.Add(this.checkBox_optimizeequip);
            this.groupBox1.Controls.Add(this.numericUpDown_maxviewcount);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.numericUpDown_threadnum);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(207, 243);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "設定";
            // 
            // checkBox_checkversion
            // 
            this.checkBox_checkversion.AutoSize = true;
            this.checkBox_checkversion.Location = new System.Drawing.Point(18, 187);
            this.checkBox_checkversion.Name = "checkBox_checkversion";
            this.checkBox_checkversion.Size = new System.Drawing.Size(148, 16);
            this.checkBox_checkversion.TabIndex = 11;
            this.checkBox_checkversion.Text = "起動時に本体更新チェック";
            this.checkBox_checkversion.UseVisualStyleBackColor = true;
            // 
            // button_sound
            // 
            this.button_sound.Location = new System.Drawing.Point(169, 162);
            this.button_sound.Name = "button_sound";
            this.button_sound.Size = new System.Drawing.Size(25, 19);
            this.button_sound.TabIndex = 10;
            this.button_sound.Text = "...";
            this.button_sound.UseVisualStyleBackColor = true;
            this.button_sound.Click += new System.EventHandler(this.button_sound_Click);
            // 
            // textBox_sound
            // 
            this.textBox_sound.Location = new System.Drawing.Point(18, 162);
            this.textBox_sound.Name = "textBox_sound";
            this.textBox_sound.Size = new System.Drawing.Size(145, 19);
            this.textBox_sound.TabIndex = 9;
            // 
            // label_getfrom
            // 
            this.label_getfrom.AutoSize = true;
            this.label_getfrom.Location = new System.Drawing.Point(22, 118);
            this.label_getfrom.Name = "label_getfrom";
            this.label_getfrom.Size = new System.Drawing.Size(41, 12);
            this.label_getfrom.TabIndex = 6;
            this.label_getfrom.Text = "取得先";
            // 
            // textBox_picture_server
            // 
            this.textBox_picture_server.Location = new System.Drawing.Point(66, 115);
            this.textBox_picture_server.Name = "textBox_picture_server";
            this.textBox_picture_server.Size = new System.Drawing.Size(131, 19);
            this.textBox_picture_server.TabIndex = 7;
            // 
            // checkBox_sound
            // 
            this.checkBox_sound.AutoSize = true;
            this.checkBox_sound.Location = new System.Drawing.Point(18, 140);
            this.checkBox_sound.Name = "checkBox_sound";
            this.checkBox_sound.Size = new System.Drawing.Size(129, 16);
            this.checkBox_sound.TabIndex = 8;
            this.checkBox_sound.Text = "検索終了時に音再生";
            this.checkBox_sound.UseVisualStyleBackColor = true;
            this.checkBox_sound.CheckedChanged += new System.EventHandler(this.checkBox_usenetwork_CheckedChanged);
            // 
            // checkBox_usenetwork
            // 
            this.checkBox_usenetwork.AutoSize = true;
            this.checkBox_usenetwork.Location = new System.Drawing.Point(18, 90);
            this.checkBox_usenetwork.Name = "checkBox_usenetwork";
            this.checkBox_usenetwork.Size = new System.Drawing.Size(179, 16);
            this.checkBox_usenetwork.TabIndex = 5;
            this.checkBox_usenetwork.Text = "インターネットから画像を取得する";
            this.checkBox_usenetwork.UseVisualStyleBackColor = true;
            this.checkBox_usenetwork.CheckedChanged += new System.EventHandler(this.checkBox_usenetwork_CheckedChanged);
            // 
            // checkBox_optimizeequip
            // 
            this.checkBox_optimizeequip.AutoSize = true;
            this.checkBox_optimizeequip.Location = new System.Drawing.Point(18, 68);
            this.checkBox_optimizeequip.Name = "checkBox_optimizeequip";
            this.checkBox_optimizeequip.Size = new System.Drawing.Size(176, 16);
            this.checkBox_optimizeequip.TabIndex = 4;
            this.checkBox_optimizeequip.Text = "上位互換の存在する装備を省く";
            this.checkBox_optimizeequip.UseVisualStyleBackColor = true;
            this.checkBox_optimizeequip.CheckedChanged += new System.EventHandler(this.checkBox_optimizeequip_CheckedChanged);
            // 
            // numericUpDown_maxviewcount
            // 
            this.numericUpDown_maxviewcount.Increment = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDown_maxviewcount.Location = new System.Drawing.Point(130, 43);
            this.numericUpDown_maxviewcount.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDown_maxviewcount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown_maxviewcount.Name = "numericUpDown_maxviewcount";
            this.numericUpDown_maxviewcount.Size = new System.Drawing.Size(67, 19);
            this.numericUpDown_maxviewcount.TabIndex = 3;
            this.numericUpDown_maxviewcount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericUpDown_maxviewcount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(47, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "最大表示件数";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "サウンドファイル|*.wav";

            // 
            // OptionDialog
            // 
            this.AcceptButton = this.button_ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_cancel;
            this.ClientSize = new System.Drawing.Size(227, 296);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_ok);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "設定";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_threadnum)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_maxviewcount)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.NumericUpDown numericUpDown_threadnum;
        private System.Windows.Forms.Button button_ok;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.NumericUpDown numericUpDown_maxviewcount;
        public System.Windows.Forms.CheckBox checkBox_optimizeequip;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label label_getfrom;
        public System.Windows.Forms.TextBox textBox_picture_server;
        public System.Windows.Forms.CheckBox checkBox_usenetwork;
        private System.Windows.Forms.Button button_sound;
        public System.Windows.Forms.CheckBox checkBox_sound;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        public System.Windows.Forms.TextBox textBox_sound;
        public System.Windows.Forms.CheckBox checkBox_checkversion;
        private System.Windows.Forms.Button button1;
    }
}