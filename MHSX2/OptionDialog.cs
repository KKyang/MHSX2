using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MHSX2
{
    public partial class OptionDialog : Form
    {
        private BaseData data;

        public OptionDialog(Settings setting,BaseData data)
        {
            this.data = data;
            InitializeComponent();
            //numericUpDown_threadnum.Maximum = ThreadNum;
            numericUpDown_threadnum.Value = setting.ThreadNum;
            numericUpDown_maxviewcount.Value = setting.StopSearchCount;

            checkBox_optimizeequip.Checked = setting.OptimizeEquip;
            checkBox_optimizeequip_CheckedChanged(null, null);
            //checkBox_ignore_def.Checked = setting.IgnoreDef;
            //checkBox_optimize_hard.Checked = setting.OptimizeHard;

            checkBox_usenetwork.Checked = setting.UseNetwork;
            checkBox_usenetwork_CheckedChanged(null, null);
            textBox_picture_server.Text = setting.PictureServer;

            textBox_sound.Text = setting.SoundFilePath;
            checkBox_sound.Checked = setting.PlaySound;

            checkBox_checkversion.Checked = setting.CheckNewVersion;
            //checkBox_optimize_hard.Checked = setting.OptimizeHard;
        }



        private void checkBox_optimizeequip_CheckedChanged(object sender, EventArgs e)
        {
            //if (checkBox_optimizeequip.Checked)
            //{
            //    checkBox_ignore_def.Enabled = true;
            //    //checkBox_optimize_hard.Enabled = true;
            //}
            //else
            //{
            //    checkBox_ignore_def.Enabled = false;
            //    //checkBox_optimize_hard.Enabled = false;
            //}
        }

        private void checkBox_usenetwork_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_usenetwork.Checked)
            {
                label_getfrom.Enabled = true;
                textBox_picture_server.Enabled = true;
            }
            else
            {
                label_getfrom.Enabled = false;
                textBox_picture_server.Enabled = false;
            }
        }

        private void button_sound_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox_sound.Text = openFileDialog1.FileName;
            }
        }

    }
}
