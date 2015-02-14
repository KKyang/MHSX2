using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MHSX2
{

    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MHSX2Form());
            }
            catch (Exception e)
            {
                MessageBox.Show("エラーの為終了します。\r\nエラー内容:\r\n" + e.Message);
            }
        }
    }


}
