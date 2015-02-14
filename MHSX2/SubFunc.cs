using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Text;

namespace MHSX2
{
    public class Pair<K, V>
    {
        public K Key;
        public V Value;

        public Pair(K k, V v)
        {
            Key = k;
            Value = v;
        }

        public override string ToString()
        {
            return Key.ToString() + "," + Value.ToString();
        }
    }

    public class NumRangeOrder
    {
        public int Under;
        public int Upper;

        public NumRangeOrder(int under, int upper)
        {
            Under = under;
            Upper = upper;
        }

        public override string ToString()
        {
            return Under.ToString() + "～" + Upper.ToString();
        }

        //xが範囲内であればtrue
        public bool CheckNum(int x)
        {
            if (x < Under || x > Upper)
                return false;
            else
                return true;
        }
    }

    public class WindowStatus
    {
        public WindowStatus()
        {
        }
        public WindowStatus(int x, int y, int width, int height,FormWindowState state)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            State = state;
        }

        public WindowStatus(Form f)
        {
            switch (f.WindowState)
            {
                case FormWindowState.Normal:
                    X = f.Bounds.X;
                    Y = f.Bounds.Y;
                    Width = f.Bounds.Width;
                    Height = f.Bounds.Height;
                    State = FormWindowState.Normal;
                    break;
                case FormWindowState.Maximized:
                    X = f.RestoreBounds.X;
                    Y = f.RestoreBounds.Y;
                    Width = f.RestoreBounds.Width;
                    Height = f.RestoreBounds.Height;
                    State = FormWindowState.Maximized;
                    break;
                case FormWindowState.Minimized:
                    X = f.RestoreBounds.X;
                    Y = f.RestoreBounds.Y;
                    Width = f.RestoreBounds.Width;
                    Height = f.RestoreBounds.Height;
                    State = FormWindowState.Normal;
                    break;
            }

        }

        public Rectangle ToRectangle()
        {
            return new Rectangle(X, Y, Width, Height);
        }

        public int X;
        public int Y;
        public int Width;
        public int Height;
        public FormWindowState State = FormWindowState.Normal;
    }

    static class SubFunc
    {
        public static bool CompairSgin(int a, int b)
        {
            if (a >= 0 && b >= 0)
                return true;
            else if (a < 0 && b < 0)
                return true;
            else
                return false;
        }

        public static string MakeFilePath(string dir, string fname)
        {
            return dir + "/" + fname;
        }

        public static string FillSpace(string str, int len)
        {
            string ret = str;


            if (ret.EndsWith(" "))
            {
                ret = ret.Remove(ret.Length - 1);
            }

            Font font = new Font("ＭＳ Ｐゴシック", 12);
            int width = TextRenderer.MeasureText(ret, font).Width;

            int num = (len - width) / 11;
            for (int i = 0; i < num; i++)
            {
                ret += "　";
            }

            int nokori = len - TextRenderer.MeasureText(ret, font).Width;


            if (nokori > 8)
            {
                ret += "　";
            }
            else
            {
                ret += " ";
            }

            return ret;
        }

        //public static Rectangle GetShowBounds(Rectangle bounds)
        //{// 接続されているスクリーンのいずれかに入っているか判定する
        //    foreach (Screen sc in Screen.AllScreens)
        //    {
        //        // スクリーンの中にあるか
        //        Rectangle cross = sc.Bounds;
        //        cross.Intersect(bounds);
        //        if (cross.IsEmpty)
        //            continue;

        //        // スクリーンの作業エリアに入っているか
        //        Rectangle workCross = sc.WorkingArea;
        //        workCross.Inflate(-bounds.Width, -bounds.Height);
        //        workCross.Intersect(bounds);
        //        if (!workCross.IsEmpty)
        //            return bounds;

        //        // 入れる
        //        return NormalizeFormBounds(bounds, sc);
        //    }
        //    // どのスクリーンにも含まれていない。
        //    // たぶんマルチディスプレイじゃ無くなったか、
        //    // 画面の解像度が小さくなったかで、画面外に出てしまった。
        //    return NormalizeFormBounds(bounds, Screen.PrimaryScreen);
        //}


        ///// <summary>
        ///// 指定したスクリーンの作業エリアに全体が見えるように移動する
        ///// </summary>
        ///// <param name="bounds">現在の外形矩形</param>
        ///// <param name="screen">調整先のスクリーン</param>
        ///// <returns>調整結果の外形領域</returns>
        //private static Rectangle NormalizeFormBounds(Rectangle bounds, Screen screen)
        //{
        //    int width = bounds.Width;
        //    int height = bounds.Height;

        //    // 画面に入るように調整する
        //    if (bounds.Right < screen.WorkingArea.Left + width)
        //    {
        //        bounds.X = screen.WorkingArea.Left;
        //    }
        //    if (bounds.Left > screen.WorkingArea.Right - width)
        //    {
        //        bounds.X = screen.WorkingArea.Right - bounds.Width;
        //    }
        //    if (bounds.Y > screen.WorkingArea.Bottom - height)
        //    {
        //        bounds.Y = screen.WorkingArea.Bottom - bounds.Height;
        //    }
        //    if (bounds.Bottom < screen.WorkingArea.Top + height)
        //    {
        //        bounds.Y = screen.WorkingArea.Top;
        //    }
        //    return bounds;
        //}


    }
}
