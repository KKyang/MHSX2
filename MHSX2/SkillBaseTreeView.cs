using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;


namespace MHSX2
{
    class SkillBaseTreeView : TreeView
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, int colorRef);

        private const int TV_FIRST = 0x1100;
        private const int TVM_SETINSERTMARK = TV_FIRST + 26;
        private const int TVM_SETINSERTMARKCOLOR = TV_FIRST + 37;
        private const int TVM_GETINSERTMARKCOLOR = TV_FIRST + 38;

        /// <summary>TreeViewにInsertMarkを設定する</summary>
        /// <param name="node">InsertMarkを設定するTreeNode  nullで解除</param>
        /// <param name="isAfter">True:nodeの後ろにInsertMarkを表示する</param>
        public void SetInsertMark(TreeNode node, bool isAfter)
        {
            //IntPtr result = SendMessage(this.Handle , TVM_SETINSERTMARK 
            //                             , new IntPtr(isAfter ? -1 : 0) 
            //                             , (node == null ? IntPtr.Zero : node.Handle)  );
            Message msg = new Message();
            msg.HWnd = this.Handle;
            msg.Msg = TVM_SETINSERTMARK;
            msg.WParam = new IntPtr(isAfter ? -1 : 0);
            msg.LParam = (node == null ? IntPtr.Zero : node.Handle);
            this.WndProc(ref msg);
            if (msg.Result == IntPtr.Zero)
            {
                throw new ApplicationException();
            }
        }


        private ContextMenuStrip contextMenuStrip_add;
        private ToolStripMenuItem お気に入りスキルに追加ToolStripMenuItem;
        private System.ComponentModel.IContainer components;
        private ContextMenuStrip contextMenuStrip_delete;
        private ToolStripMenuItem toolStripMenuItem1;
        private List<string> FavoriteSkill;
        private List<SkillSet> SkillSets;

        public SkillBaseTreeView()
        {
            InitializeComponent();
        }

        public void LoadBaseData(BaseData basedata, Settings setting)
        {
            FavoriteSkill = setting.FavoriteSkills;
            SkillSets = setting.SkillSets;
            BeginUpdate();
            Nodes.Clear();


            TreeNode FavoriteParent = Nodes.Add("お気に入り");
            foreach (string skillname in FavoriteSkill)
            {
                SkillBase sb = null;

                if (basedata.SkillBaseMap.ContainsKey(skillname))
                {
                    sb = basedata.SkillBaseMap[skillname];
                }


                if (sb == null)
                {
                    throw new Exception("お気に入りスキル読み取りエラー");
                }

                TreeNode node = FavoriteParent.Nodes.Add(sb.Name);
                node.Tag = sb;
                node.ContextMenuStrip = contextMenuStrip_delete;
            }


            TreeNode SkillSetParent = Nodes.Add("スキルセット");
            foreach (SkillSet ss in SkillSets)
            {
                //List<SkillOption> list = new List<SkillOption>();

                //foreach (string str in ss.list)
                //{
                //    if (basedata.SkillOptionMap.ContainsKey(str))
                //    {
                //        list.Add(basedata.SkillOptionMap[str]);
                //    }
                //}


                TreeNode sub = new TreeNode();
                sub.Text = ss.name;
                sub.Tag = ss;//new Pair<Job,List<SkillOption>>(ss.job,list);
                sub.ContextMenuStrip = contextMenuStrip_delete;

                SkillSetParent.Nodes.Add(sub);

            }




            foreach (string typename in basedata.SkillCategoryList)
            {
                Nodes.Add(typename);
            }


            foreach (SkillBase sbase in basedata.SkillBaseMap.Values)
            {
                TreeNode node = Nodes[2 + (int)sbase.SkillCategory].Nodes.Add(sbase.Name);
                node.Tag = sbase;
                node.ContextMenuStrip = contextMenuStrip_add;
            }


            EndUpdate();
        }



        protected override void OnAfterSelect(TreeViewEventArgs e)
        {
            base.OnAfterSelect(e);

            switch (e.Node.Level)
            {
                case 0:
                    e.Node.Toggle();
                    SelectedNode = null;
                    break;
            }
        }


        protected override void OnBeforeExpand(TreeViewCancelEventArgs e)
        {

            base.OnBeforeExpand(e);
            //展開するのは常に一つだけ
            foreach (TreeNode t in Nodes)
                t.Collapse();
        }

        private void お気に入りスキルに追加ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedNode == null)
                return;

            if (!(SelectedNode.Tag is SkillBase))
                return;

            SkillBase sb = (SkillBase)SelectedNode.Tag;

            if (FavoriteSkill.Contains(sb.Name))
                return;

            FavoriteSkill.Add(sb.Name);
            TreeNode node = Nodes[0].Nodes.Add(sb.Name);
            node.Tag = sb;
            node.ContextMenuStrip = contextMenuStrip_delete;
        }

        private void 削除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedNode == null)
                return;



            switch (SelectedNode.Parent.Index)
            {
                case 0://お気に入り
                    if (!(SelectedNode.Tag is SkillBase))
                        return;

                    SkillBase sb = (SkillBase)SelectedNode.Tag;
                    FavoriteSkill.Remove(sb.Name);
                    Nodes[0].Nodes.Remove(SelectedNode);
                    break;
                case 1://スキルセット
                    SkillSets.RemoveAt(SelectedNode.Index);
                    Nodes[1].Nodes.Remove(SelectedNode);

                    break;
            }
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.contextMenuStrip_add = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.お気に入りスキルに追加ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip_delete = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip_add.SuspendLayout();
            this.contextMenuStrip_delete.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip_add
            // 
            this.contextMenuStrip_add.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.お気に入りスキルに追加ToolStripMenuItem});
            this.contextMenuStrip_add.Name = "contextMenuStrip_favorite";
            this.contextMenuStrip_add.Size = new System.Drawing.Size(184, 26);
            // 
            // お気に入りスキルに追加ToolStripMenuItem
            // 
            this.お気に入りスキルに追加ToolStripMenuItem.Name = "お気に入りスキルに追加ToolStripMenuItem";
            this.お気に入りスキルに追加ToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.お気に入りスキルに追加ToolStripMenuItem.Text = "お気に入りスキルに追加";
            this.お気に入りスキルに追加ToolStripMenuItem.Click += new System.EventHandler(this.お気に入りスキルに追加ToolStripMenuItem_Click);
            // 
            // contextMenuStrip_delete
            // 
            this.contextMenuStrip_delete.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1});
            this.contextMenuStrip_delete.Name = "contextMenuStrip_delete";
            this.contextMenuStrip_delete.Size = new System.Drawing.Size(164, 26);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(163, 22);
            this.toolStripMenuItem1.Text = "削除";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.削除ToolStripMenuItem_Click);
            // 
            // SkillBaseTreeView
            // 
            this.AllowDrop = true;
            this.LineColor = System.Drawing.Color.Black;
            this.DragLeave += new System.EventHandler(this.SkillBaseTreeView_DragLeave);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.SkillBaseTreeView_DragDrop);
            this.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.SkillBaseTreeView_ItemDrag);
            this.DragOver += new System.Windows.Forms.DragEventHandler(this.SkillBaseTreeView_DragOver);
            this.contextMenuStrip_add.ResumeLayout(false);
            this.contextMenuStrip_delete.ResumeLayout(false);
            this.ResumeLayout(false);

        }


        private void SkillBaseTreeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            TreeView tv = (TreeView)sender;

            TreeNode selected = (TreeNode)e.Item;

            if (selected.Parent == null)//トップノード（スキル分類）は移動不可
                return;

            if (selected.Parent.Index != 0 && selected.Parent.Index != 1)//お気に入り、スキルセット以外移動不可
                return;


            tv.SelectedNode = (TreeNode)e.Item;


            tv.Focus();
            //ノードのドラッグを開始する
            DragDropEffects dde = tv.DoDragDrop(e.Item, DragDropEffects.All);

            ////移動した時は、ドラッグしたノードを削除する
            //if ((dde & DragDropEffects.Move) == DragDropEffects.Move)
            //    tv.Nodes.Remove((TreeNode)e.Item);

        }

        private void SkillBaseTreeView_DragOver(object sender, DragEventArgs e)
        {
            //ドラッグされているデータがTreeNodeか調べる
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                if ((e.AllowedEffect & DragDropEffects.Move) ==
                    DragDropEffects.Move)
                    //何も押されていなければMove
                    e.Effect = DragDropEffects.Move;
                else
                    e.Effect = DragDropEffects.None;
            }
            else
            {
                //TreeNodeでなければ受け入れない
                e.Effect = DragDropEffects.None;
            }

            //マウス下のNodeを選択する
            if (e.Effect != DragDropEffects.None)
            {
                TreeView tv = (TreeView)sender;
                //マウスのあるNodeを取得する
                TreeNode target =
                    tv.GetNodeAt(tv.PointToClient(new Point(e.X, e.Y)));
                //ドラッグされているNodeを取得する
                TreeNode source =
                    (TreeNode)e.Data.GetData(typeof(TreeNode));



                //マウス下のNodeがドロップ先として適切か調べる
                if (target != null && target != source && !IsChildNode(source, target))
                {
                    if (target.Parent == null)
                    {
                        e.Effect = DragDropEffects.None;
                    }
                    else if (target.Parent.Index != source.Parent.Index)
                    {
                        e.Effect = DragDropEffects.None;
                    }
                    else
                    {
                        if ((target.Bounds.Top + target.Bounds.Bottom) / 2 < this.PointToClient(new Point(e.X, e.Y)).Y)
                        {
                            if (target.Index != source.Index - 1)
                                SetInsertMark(target, true);
                            else
                                e.Effect = DragDropEffects.None;
                        }
                        else
                        {
                            if (target.Index != source.Index + 1)
                                SetInsertMark(target, false);
                            else
                                e.Effect = DragDropEffects.None;
                        }
                    }
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }

            if (e.Effect == DragDropEffects.None)
            {
                SetInsertMark(null, true);
            }


        }

        private static bool IsChildNode(TreeNode parent, TreeNode child)
        {
            if (child.Parent == parent)
                return true;
            else if (child.Parent != null)
                return IsChildNode(parent, child.Parent);
            else
                return false;
        }

        private void SkillBaseTreeView_DragDrop(object sender, DragEventArgs e)
        {
            SetInsertMark(null, true);

            //ドロップされたデータがTreeNodeか調べる
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                TreeView tv = (TreeView)sender;
                //ドロップされたデータ(TreeNode)を取得
                TreeNode source =
                    (TreeNode)e.Data.GetData(typeof(TreeNode));
                //ドロップ先のTreeNodeを取得する
                TreeNode target =
                    tv.GetNodeAt(tv.PointToClient(new Point(e.X, e.Y)));
                //マウス下のNodeがドロップ先として適切か調べる

                if (target != null && target != source &&
                    !IsChildNode(source, target))
                {
                    if (target.Parent == null)
                        return;

                    TreeNode mother = target.Parent;

                    //ドロップされたNodeのコピーを作成


                    TreeNode cpy = (TreeNode)source.Clone();

                    BeginUpdate();

                    if ((target.Bounds.Top + target.Bounds.Bottom) / 2 < this.PointToClient(new Point(e.X, e.Y)).Y)
                        mother.Nodes.Insert(target.Index + 1, cpy);
                    else
                        mother.Nodes.Insert(target.Index, cpy);

                    mother.Nodes.Remove(source);

                    SelectedNode = cpy;

                    EndUpdate();

                    switch (target.Parent.Index)
                    {
                        case 0:
                            FavoriteSkill.Clear();
                            for (int i = 0; i < Nodes[0].Nodes.Count; i++)
                            {
                                FavoriteSkill.Add(Nodes[0].Nodes[i].Text);
                            }
                            break;
                        case 1:
                            SkillSets.Clear();
                            for (int i = 0; i < Nodes[1].Nodes.Count; i++)
                            {
                                if (Nodes[1].Nodes[i].Tag is SkillSet)
                                    SkillSets.Add((SkillSet)Nodes[1].Nodes[i].Tag);
                            }
                            break;
                    }
                }
                else
                    e.Effect = DragDropEffects.None;
            }
            else
                e.Effect = DragDropEffects.None;

        }

        private void SkillBaseTreeView_DragLeave(object sender, EventArgs e)
        {
            SetInsertMark(null, true);
        }

        public void AddSkillSet(string name, List<SkillOption> list, Job job)
        {
            SkillSet set = new SkillSet();

            set.name = name;
            //set.job = job;
            foreach (SkillOption so in list)
            {
                set.list.Add(so.Name);
            }

            SkillSets.Add(set);

            TreeNode sub = new TreeNode();
            sub.Text = name;
            sub.Tag = set;//new Pair<Job,List<SkillOption>>(job,list);
            sub.ContextMenuStrip = contextMenuStrip_delete;

            Nodes[1].Nodes.Add(sub);



        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                TreeViewHitTestInfo aaa = HitTest(e.X, e.Y);
                SelectedNode = aaa.Node;

            }

            base.OnMouseDown(e);
        }
    }
}
