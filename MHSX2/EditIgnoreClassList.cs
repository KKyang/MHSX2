using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MHSX2
{
    public partial class EditIgnoreClassList : Form
    {
        List<string> ClassList;
        List<string> IgnoreClassList;
        List<ItemClass> ItemClassList;
        List<ItemClass> ItemClassList_Jewelry;

        List<string> ClassList_Jewelry;
        List<string> IgnoreClassList_jewelry;

        List<string> AddedList = new List<string>();

        public EditIgnoreClassList(BaseData basedata, Ignore ignore)
        {
            ClassList = basedata.ClassList;
            ClassList_Jewelry = basedata.ClassList_Jewelry;
            ItemClassList = basedata.ItemClassList;
            ItemClassList_Jewelry = basedata.ItemClassList_Jewelry;

            this.IgnoreClassList = ignore.Class;
            this.IgnoreClassList_jewelry = ignore.Class_Jewelry;

            InitializeComponent();


            //foreach (string str in ClassList)
            //{
            //    checkedListBox1.Items.Add(str, !IgnoreClassList.Contains(str));
            //}

            //foreach (string str in ClassList_Jewelry)
            //{
            //    checkedListBox2.Items.Add(str, !IgnoreClassList_jewelry.Contains(str));
            //}

            //Equip分類
            AddedList.Clear();
            foreach (ItemClass itemClass in ItemClassList)
            {
                TreeNode Add = CreateNode(null, itemClass, AddedList);
                treeView_equip.Nodes.Add(Add);
            }

            foreach (string str in ClassList)
            {
                if (!AddedList.Contains(str))
                {
                    TreeNode add = new TreeNode(str);
                    add.Tag = str;
                    treeView_equip.Nodes.Add(add);
                    AddedList.Add(str);
                }
            }

            foreach (TreeNode node in treeView_equip.Nodes)
            {
                JudgeCheck(node,ItemClassType.Equip);
                node.Expand();
            }



            //Jewelry分類
            AddedList.Clear();
            foreach (ItemClass itemClass in ItemClassList_Jewelry)
            {
                TreeNode Add = CreateNode(null, itemClass, AddedList);
                treeView_Jewelry.Nodes.Add(Add);
            }

            foreach (string str in ClassList_Jewelry)
            {
                if (!AddedList.Contains(str))
                {
                    TreeNode add = new TreeNode(str);
                    add.Tag = str;
                    treeView_Jewelry.Nodes.Add(add);
                    AddedList.Add(str);
                }
            }

            foreach (TreeNode node in treeView_Jewelry.Nodes)
            {
                JudgeCheck(node,ItemClassType.Jewelry);
                node.Expand();
            }



        }

        private void JudgeCheck(TreeNode node,ItemClassType type)
        {
            if (node.Nodes.Count == 0)
            {
                String name = (String)node.Tag;
                switch (type)
                {
                    case ItemClassType.Equip:
                        node.Checked = !IgnoreClassList.Contains(name);
                        break;
                    case ItemClassType.Jewelry:
                        node.Checked = !IgnoreClassList_jewelry.Contains(name);
                        break;
                }
            }
            else
            {
                foreach (TreeNode child in node.Nodes)
                {
                    JudgeCheck(child,type);
                }
            }

        }


        public EditIgnoreClassList()
        {
            InitializeComponent();
        }




        private TreeNode CreateNode(TreeNode parent, ItemClass child,List<string> AddedList)
        {
            TreeNode AddNode = new TreeNode();

            if (child is ParentItemClass)
            {
                ParentItemClass node = (ParentItemClass)child;

                AddNode.Text = node.ToString();

                foreach (ItemClass c in node.ChildClass)
                {
                    AddNode.Nodes.Add(CreateNode(AddNode, c,AddedList));
                }

            }
            else
            {
                AddNode.Text = child.ToString();
                AddNode.Tag = child.Name;
                AddedList.Add(child.Name);

            }

            return AddNode;
        }


        private void button_OK_Click(object sender, EventArgs e)
        {
            IgnoreClassList.Clear();

            //foreach (string str in ClassList)
            //{
            //    if (!checkedListBox1.CheckedItems.Contains(str))
            //        IgnoreClassList.Add(str);
            //}

            foreach (TreeNode node in treeView_equip.Nodes)
            {
                LookCheckState(IgnoreClassList, node);
            }


            IgnoreClassList_jewelry.Clear();

            //foreach (string str in ClassList_Jewelry)
            //{
            //    if (!treeView_Jewelry.CheckedItems.Contains(str))
            //        IgnoreClassList_jewelry.Add(str);
            //}

            foreach (TreeNode node in treeView_Jewelry.Nodes)
            {
                LookCheckState(IgnoreClassList_jewelry, node);
            }

        }

        private void LookCheckState(List<string> iglist,TreeNode node)
        {
            if (node.Nodes.Count == 0)
            {
                if (node.Checked == false)
                {
                    String name = (String)node.Tag;
                    iglist.Add(name);
                }
            }
            else
            {
                foreach (TreeNode child in node.Nodes)
                {
                    LookCheckState(iglist,child);
                }
            }
        }


        private void 全てチェックToolStripMenuItem_Click(object sender, EventArgs e)
        {

            TriStateTreeView tgt = null;
            switch (tabControl1.SelectedIndex)
            {
                case 0:
                    tgt = treeView_equip;
                    break;
                case 1:
                    tgt = treeView_Jewelry;
                    break;
            }


            Stack<TreeNode> stack = new Stack<TreeNode>();

            foreach (TreeNode node in tgt.Nodes)
                stack.Push(node);

            do
            {
                TreeNode node = stack.Pop();
                node.Checked = true;

                foreach (TreeNode n in node.Nodes)
                    stack.Push(n);

            } while (stack.Count > 0);


        }

        private void 全てチェックを外すToolStripMenuItem_Click(object sender, EventArgs e)
        {

            TriStateTreeView tgt = null;
            switch (tabControl1.SelectedIndex)
            {
                case 0:
                    tgt = treeView_equip;
                    break;
                case 1:
                    tgt = treeView_Jewelry;
                    break;
            }


            Stack<TreeNode> stack = new Stack<TreeNode>();

            foreach (TreeNode node in tgt.Nodes)
                stack.Push(node);

            do
            {
                TreeNode node = stack.Pop();
                node.Checked = false;

                foreach (TreeNode n in node.Nodes)
                    stack.Push(n);

            } while (stack.Count > 0);


        }

    }
}
