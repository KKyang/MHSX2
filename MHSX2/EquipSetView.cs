using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Drawing;

namespace MHSX2
{
    public class EquipSetView : ListView
    {
        private EquipSet ESet_value;
        private NumericUpDown numericUpDown1;

        public bool isInited = false;

        private NecessaryItemView ItemView = null;
        private EquipSetSkillView SkillView = null;

        String[] headname = new string[] { "武", "頭", "胴", "腕", "腰", "脚", "プ", "計" };


        public void SetViewers(EquipSetSkillView skill, NecessaryItemView item)
        {
            ItemView = item;
            SkillView = skill;
        }


        public EquipSetView()
        {
            InitializeComponent();
        }

        public EquipSet ESet
        {
            get
            {
                return ESet_value;
            }
            set
            {
                if (value != null)
                {
                    ESet_value = value;
                    isInited = true;
                }
                else
                {
                    ESet_value = null;
                    isInited = false;
                }

            }

        }


        public EquipSet CopyEquipSet(EquipSet set)
        {
            //ESet_value = new EquipSet();

            ESet_value = (EquipSet)set.Clone();

            //for (int i = 0; i < (int)EquipType.NumOfEquipKind; i++)
            //{
            //    if (set.Equips[i] == null)
            //        continue;

            //    ESet_value[(EquipType)i].EquipData = set.Equips[i].EquipData;
            //    ESet_value[(EquipType)i].Def = set.Equips[i].Def;
            //    ESet_value[(EquipType)i].Level = set.Equips[i].Level;
            //    ESet_value[(EquipType)i].Slot = set.Equips[i].Slot;

            //    ESet_value[(EquipType)i].jewelrys = set.Equips[i].jewelrys;
            //}




            return ESet_value;
        }


        protected override void OnItemCheck(ItemCheckEventArgs ice)
        {
            if (ice.CurrentValue == CheckState.Unchecked)
            {
                if (Items[ice.Index].Tag == null)
                {
                    ice.NewValue = CheckState.Unchecked;
                }
            }

            base.OnItemCheck(ice);
        }


        private void SetItemColor(ListViewItem Item)
        {
            if (Item.Checked == false && Item.Index != 7)
            {
                Item.UseItemStyleForSubItems = true;
                Item.ForeColor = System.Drawing.Color.DarkGray;
            }
            else
            {
                Item.UseItemStyleForSubItems = false;
                Item.ForeColor = System.Drawing.Color.Black;
            }
        }


        protected override void OnItemChecked(ItemCheckedEventArgs e)
        {

            SetItemColor(e.Item);

            if (e.Item.Tag != null)
            {
                if (0 <= e.Item.Index && e.Item.Index < 6)
                {
                    Equipment equip = (Equipment)e.Item.Tag;
                    equip.isChecked = e.Item.Checked;
                }
                else if (e.Item.Index == 6)
                {
                    PigClothes pc = (PigClothes)e.Item.Tag;
                    pc.isChecked = e.Item.Checked;
                }
            }

            UpdateTotalInfo();
            base.OnItemChecked(e);
        }

        public void UpdateTotalInfo()
        {
            ESet_value.UpdateData();

            ListViewItem ite = Items[7];
            ite.Checked = false;
            ite.SubItems.Clear();
            ite.Text = "計";

            for (int i = 1; i < 19; i++)
            {
                switch (i)
                {
                    case 4:
                        ite.SubItems.Add(ESet_value.TotalDef.ToString());
                        break;
                    case 5://HR
                        ite.SubItems.Add(ESet_value.TotalGettableHR.ToString());
                        break;
                    case 11:
                        ite.SubItems.Add(ESet.GetSlotString());
                        break;

                    case 14:
                    case 15:
                    case 16:
                    case 17:
                    case 18:
                        string str = ESet_value.TotalElement[(ElementType)(i - 14)].ToString();
                        if (int.Parse(str) > 0)
                            str = "+" + str;
                        ite.SubItems.Add(str);
                        break;
                    default:
                        ite.SubItems.Add("");
                        break;
                }

            }
        }

        public void UpdateData()
        {
            if (ESet_value == null)
            {
                BeginUpdate();
                Items.Clear();
                for (int i = 0; i < 8; i++)
                {
                    ListViewItem item = new ListViewItem(headname[i]);
                    Items.Add(item);
                }
                EndUpdate();
                return;
            }

            isInited = false;

            base.BeginUpdate();


            Dictionary<SkillBase, SkillPoint> InvokeSkill = ESet_value.GetInvokeSKillHashTable();

            for (EquipKind i = EquipKind.Weapon; i < EquipKind.NumOfEquipKind; i++)
            {
                ListViewItem item = Items[(int)i];//new ListViewItem(headname[i]);

                item.UseItemStyleForSubItems = false;
                item.SubItems.Clear();
                item.Text = headname[(int)i];
                Equipment e = ESet_value[i];


                if (e.EquipData != null)
                {
                    item.Tag = ESet_value[(EquipKind)i];//←順番注意↓
                    item.Checked = ESet_value[(EquipKind)i].isChecked;

                    item.SubItems.Add(e.EquipData.Name);
                    item.SubItems.Add(e.EquipData.Class);

                    item.SubItems.Add(e.Level.ToString());
                    item.SubItems[3].Name = "Level";

                    item.SubItems.Add(e.Def.ToString());


                    if (0 < e.Level && e.Level <= e.EquipData.LevelList.Count + 1)
                    {
                        int hr = e.EquipData.LevelList[e.Level - 1].GetableHR;

                        item.SubItems.Add(BaseData.mDefine.GetRankKindString(hr) + (hr % 1000).ToString());


                    }
                    else
                        item.SubItems.Add("");



                    for (int j = 0; j < 5; j++)
                    {
                        if (j < e.EquipData.SkillPointList.Length)
                        {
                            ListViewItem.ListViewSubItem sub = item.SubItems.Add(e.EquipData.SkillPointList[j].ToString());

                            if (InvokeSkill.ContainsKey(e.EquipData.SkillPointList[j].SBase))
                            {
                                if (e.EquipData.SkillPointList[j].Point >= 0)
                                {
                                    sub.ForeColor = Color.Blue;
                                }
                                else
                                {
                                    sub.ForeColor = Color.Red;
                                }
                            }
                        }
                        else
                        {
                            item.SubItems.Add("");
                        }
                    }


                    item.SubItems.Add(ESet_value[(EquipKind)i].GetSlotString());
                    item.SubItems.Add(ESet_value[(EquipKind)i].GetJewelryString());

                    item.SubItems.Add(e.EquipData.Rare.ToString());
                    for (ElementType type = ElementType.Fire; type != ElementType.NumOfElementKind; type++)
                    {
                        string str = e.EquipData.Element[type].ToString();
                        if (int.Parse(str) > 0)
                            str = "+" + str;
                        item.SubItems.Add(str);
                    }

                }
                else
                {
                    item.Tag = null;
                    item.Checked = false;
                }

                SetItemColor(item);
            }

            if (ESet.PigClothes.Clothes == null)
            {
                Items[6].SubItems.Clear();
                Items[6].Text = headname[6];
                Items[6].Tag = null;
                Items[6].Checked = false;
                SetItemColor(Items[6]);
            }
            else
            {
                ListViewItem item = Items[6];

                item.Tag = ESet.PigClothes;//←順番注意↓
                item.Checked = ESet.PigClothes.isChecked;


                item.UseItemStyleForSubItems = false;
                item.SubItems.Clear();
                item.Text = headname[6];

                item.SubItems.Add(ESet.PigClothes.Clothes.Name);
                item.SubItems.Add(ESet.PigClothes.Clothes.Class);
                for (int i = 0; i < 8; i++)
                {
                    item.SubItems.Add("");
                }

                item.SubItems.Add(ESet.PigClothes.GetSlotString());
                item.SubItems.Add(ESet.PigClothes.GetJewelryString());

                SetItemColor(item);
            }



            UpdateTotalInfo();


            base.EndUpdate();

            if (SkillView != null)
            {
                SkillView.ESet = ESet;
                SkillView.UpdateData();
            }

            if (ItemView != null)
            {
                ItemView.ESet = ESet;
            }

            isInited = true;
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
            this.numericUpDown1.Maximum = new decimal(new int[] {
            7,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(120, 19);
            this.numericUpDown1.TabIndex = 0;
            this.numericUpDown1.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
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
                if (info.SubItem.Name == "Level")
                {
                    Equipment equip = (Equipment)info.Item.Tag;

                    int point;
                    int.TryParse(info.SubItem.Text, out point);


                    numericUpDown1.Maximum = new decimal(equip.equipdata_value.LevelList.Count);

                    numericUpDown1.Value = point;
                    numericUpDown1.Location = info.SubItem.Bounds.Location;
                    numericUpDown1.Size = info.SubItem.Bounds.Size;
                    numericUpDown1.Tag = info;





                    Controls.Add(numericUpDown1);
                    numericUpDown1.Focus();
                    return;
                }
            }

            //base.OnMouseDown(e);
        }



        private void numericUpDown1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                numericUpDown1_Leave(sender, e);
        }

        private void numericUpDown1_Leave(object sender, EventArgs e)
        {
            Controls.Remove(numericUpDown1);
            ListViewHitTestInfo info = (ListViewHitTestInfo)numericUpDown1.Tag;

            info.SubItem.Text = numericUpDown1.Value.ToString();

            Equipment equip = (Equipment)info.Item.Tag;

            int FilledSlot = equip.GetFilledSlotNum();

            equip.Level = (int)numericUpDown1.Value;
            equip.Slot = equip.EquipData.LevelList[equip.Level - 1].Slot;
            equip.Def = equip.EquipData.LevelList[equip.Level - 1].Def;

            if (SkillView != null)
            {
                SkillView.ESet = ESet_value;
                SkillView.UpdateData();
            }

            if (ItemView != null)
            {
                ItemView.ESet = ESet_value;
            }



            UpdateData();
        }



    }
}
