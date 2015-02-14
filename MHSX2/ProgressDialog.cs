using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace MHSX2
{
    public partial class ProgressDialog : Form
    {

        public ProgressDialog()
        {
            InitializeComponent();
        }


        public void SetProgress(int value)
        {
            Invoke((MethodInvoker)delegate()
            {
                if (progressBar1.Maximum < value)
                {
                    value = progressBar1.Maximum;
                }

                progressBar1.Value = value;
            });
        }

    }
}
