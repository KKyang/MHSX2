using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Xml.Serialization;

namespace MHSX2
{
    [Serializable()]
    public class Ignore :ICloneable
    {
        public List<string> Equip = new List<string>();
        public List<string> Jewelry = new List<string>();
        public List<string> skill = new List<string>();
        public List<string> Class = new List<string>();
        public List<string> Class_Jewelry = new List<string>();
        public List<string> SkillCuff = new List<string>();
        public List<string> Item = new List<string>();

        #region ICloneable メンバ

        public object Clone()
        {
            Ignore ret = new Ignore();

            ret.Equip = new List<string>(this.Equip);
            ret.Jewelry = new List<string>(this.Jewelry);
            ret.skill = new List<string>(this.skill);
            ret.Class = new List<string>(this.Class);
            ret.Class_Jewelry = new List<string>(this.Class_Jewelry);
            ret.SkillCuff = new List<string>(this.SkillCuff);
            ret.Item = new List<string>(this.Item);

            return ret;
        }

        #endregion

        //false ・・無視アイテムに該当　　true・・該当なし
        public bool IgnoreJudge_juerly(JewelryData jd, bool IgnoreItem, bool IgnoreClass)
        {
            if (IgnoreItem)
            {
                if (Jewelry.Contains(jd.Name))//除外指定されてたらダメ
                    return false;

                foreach (Item item in jd.CostItem.Keys)
                {
                    if (Item.Contains(item.name))
                    {
                        return false;
                    }
                }
            }

            if (IgnoreClass)
            {
                if (jd.Class != null)
                {
                    if (Class_Jewelry.Contains(jd.Class))
                        return false;
                }
                else
                {
                    if (Class_Jewelry.Contains("無分類"))
                        return false;
                }
            }

            return true;
        }

        public bool IgnoreJudge_Equip(EquipmentData ed, bool IgnoreItem, bool IgnoreClass,BaseData basedata)
        {
            if (IgnoreItem)//除外指定されてたらダメ
            {
                if(Equip.Contains(ed.Name))
                    return false;

                int level = ed.Level;
                EquipmentData source = ed;
               
                while (source != null)
                {
                    for (int i = 0; i < level; i++)
                    {
                        foreach (Item item in source.LevelList[i].CostItems.Keys)
                        {
                            if (Item.Contains(item.name))
                            {
                                return false;
                            }
                        }
                    }

                    if (source.DerivSource != null)
                    {
                        level = source.DerivSource.level;
                        source = basedata.EquipDataMap[(int)source.Kind][source.DerivSource.Source];
                    }
                    else
                        source = null;

                }
         
            }

            if (IgnoreClass)
            {
                if (ed.Class != null)
                {
                    if (Class.Contains(ed.Class))
                        return false;
                }
                else
                {
                    if (Class.Contains("無分類"))
                        return false;
                }
            }

            return true;
        }

        public bool IgnoreJudge_SkillCuff(SkillCuffData cuff, bool IgnoreItem, bool IgnoreClass)
        {
            if (IgnoreItem)
            {
                if (SkillCuff.Contains(cuff.Name))
                    return false;

                foreach (Item item in cuff.CostItem.Keys)
                {
                    if (this.Item.Contains(item.name))
                        return false;
                }

            }

            if (IgnoreClass)
            {
                if (cuff.Class != null)
                {
                    if (this.Class_Jewelry.Contains(cuff.Class))
                        return false;
                }
                else
                {
                    if (this.Class_Jewelry.Contains("無分類"))
                        return false;
                }
            }


            return true;
        }

    }
}
