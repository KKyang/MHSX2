using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.Xml;
using System.Threading;
using System.IO;

namespace MHSX2
{
    public class BaseData
    {
        public List<string> SkillCategoryList = new List<string>();
        public Dictionary<string, SkillBase> SkillBaseMap = new Dictionary<string, SkillBase>();//[Name]->SkillBase
        public Dictionary<string, EquipmentData>[] EquipDataMap = new Dictionary<string, EquipmentData>[(int)EquipKind.NumOfEquipKind];//[Name]->EquipData///
        public Dictionary<string, JewelryData> JewelryDataMap = new Dictionary<string, JewelryData>();//[Name]->JewelryData
        public Dictionary<string, SkillCuffData> SkillCaffDataMap = new Dictionary<string, SkillCuffData>();
        public Dictionary<string, ClothesData> ClothesDataMap = new Dictionary<string, ClothesData>();

        public Dictionary<string, SkillOption> SkillOptionMap = new Dictionary<string, SkillOption>();//[オプション名]->SkillOption//
        public Dictionary<string, Item> ItemMap = new Dictionary<string, Item>();//[アイテム名]->Item
        public List<string> ClassList = new List<string>();
        public List<string> ClassList_Jewelry = new List<string>();
        public List<string> TypeList = new List<string>();
        public List<ItemClass> ItemClassList = new List<ItemClass>();
        public List<ItemClass> ItemClassList_Jewelry = new List<ItemClass>();
        public int MaxRustaLebel = 0;

        public static Define mDefine;

        private List<String> tmpItemList = new List<string>();
        private List<String> tmpSkillList = new List<string>();
        private List<String> tmpClassList = new List<string>();
        private List<String> tmpClassListJewelry = new List<string>();

        public BaseData()
        {
            for (int i = 0; i < 6; i++)
                EquipDataMap[i] = new Dictionary<string, EquipmentData>();

        }

        public void Load()
        {
            LoadFromXML();

            ClassList.Sort();
            ClassList_Jewelry.Sort();
            TypeList.Sort();
        }


        private void LoadFromXML()
        {
            LoadItem(SubFunc.MakeFilePath(Properties.Resources.DNAME_DAT, Properties.Resources.FNAME_ITEM));

            LoadSkillBase(SubFunc.MakeFilePath(Properties.Resources.DNAME_DAT, Properties.Resources.FNAME_SKILLBASE));
            LoadJewelry(SubFunc.MakeFilePath(Properties.Resources.DNAME_DAT, Properties.Resources.FNAME_JEWEL));
            LoadSkillCuff(SubFunc.MakeFilePath(Properties.Resources.DNAME_DAT, Properties.Resources.FNAME_SKILLCUFF));
            LoadClasses(SubFunc.MakeFilePath(Properties.Resources.DNAME_DAT, Properties.Resources.FNAME_CLASSES));
            LoadDefine(SubFunc.MakeFilePath(Properties.Resources.DNAME_DAT, Properties.Resources.FNAME_DEFINE));

            LoadEquip(EquipKind.Weapon, SubFunc.MakeFilePath(Properties.Resources.DNAME_DAT, Properties.Resources.FNAME_WEAPON));

            LoadEquip(EquipKind.Head, SubFunc.MakeFilePath(Properties.Resources.DNAME_DAT, Properties.Resources.FNAME_EQUIPHEAD));
            LoadEquip(EquipKind.Body, SubFunc.MakeFilePath(Properties.Resources.DNAME_DAT, Properties.Resources.FNAME_EQUIPBODY));
            LoadEquip(EquipKind.Arm, SubFunc.MakeFilePath(Properties.Resources.DNAME_DAT, Properties.Resources.FNAME_EQUIPARM));
            LoadEquip(EquipKind.Waist, SubFunc.MakeFilePath(Properties.Resources.DNAME_DAT, Properties.Resources.FNAME_EQUIPWST));
            LoadEquip(EquipKind.Leg, SubFunc.MakeFilePath(Properties.Resources.DNAME_DAT, Properties.Resources.FNAME_EQUIPLEG));

            LoadClothes(SubFunc.MakeFilePath(Properties.Resources.DNAME_DAT, Properties.Resources.FNAME_CLOTHES));
        }

        public class Define
        {
            public String[] GRankEquipType;
            public String[] RankLimitKind;

            public int GetRankLimitKindIndex(int hr)
            {
                int x = hr/1000;

                if(RankLimitKind.Length > x)
                {
                    return x;
                }
                else
                    return 0;

            }

            public String GetRankKindString(int hr)
            {
                int index = GetRankLimitKindIndex(hr);

                if (RankLimitKind.Length > index)
                    return RankLimitKind[index];
                else 
                    return "HR";
            }

        }

        private void LoadDefine(string fname)
        {
            try
            {
                FileStream fs = File.Open(fname, FileMode.Open);
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Define));

                BaseData.mDefine = (Define)serializer.Deserialize(fs);
                fs.Close();
            }
            catch (Exception)
            {
                mDefine = new Define();
                mDefine.GRankEquipType = new String[] { };
                mDefine.RankLimitKind = new String[] {"HR"};
            }


        }

        public void LoadClasses(String fname)
        {
            XmlDocument xdoc = new XmlDocument();

            try
            {
                xdoc.Load(fname);
            }
            catch (Exception)
            {
                return;
            }


            XmlElement root = xdoc.DocumentElement;

            for (XmlNode node = root.FirstChild; node != null; node = node.NextSibling)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    ItemClassType type;
                    if(node.Name == "Equip")
                        type = ItemClassType.Equip;
                    else if (node.Name == "Jewelry")
                        type = ItemClassType.Jewelry;
                    else continue;


                    for (XmlNode child = node.FirstChild; child != null; child = child.NextSibling)
                    {
                        if (node.NodeType == XmlNodeType.Element)
                        {
                            ReadItemClass((XmlElement)child, null,type);
                        }
                    }
                }
            }

        }

        private void ReadItemClass(XmlElement node,ParentItemClass parent,ItemClassType type)
        {
            ItemClass addClass = null;

            if (node.Name == "ParentClass")
            {
                ParentItemClass ParentClass = new ParentItemClass();

                ParentClass.Name = node.GetAttribute("Name");

                if(node.HasAttribute("Text"))
                    ParentClass.Text = node.GetAttribute("Text");

                foreach(XmlNode n in node.ChildNodes)
                {
                    if(n.NodeType == XmlNodeType.Element)
                        ReadItemClass((XmlElement)n,ParentClass,type);
                }

                addClass = ParentClass;

            }
            else if(node.Name == "Class")
            {
                addClass = new ItemClass();
                addClass.Name = node.InnerText;

                if (node.HasAttribute("Text"))
                    addClass.Text = node.GetAttribute("Text");
            }

            if (parent != null)
            {
                parent.ChildClass.Add(addClass);
            }
            else
            {
                switch (type)
                {
                    case ItemClassType.Equip:

                        ItemClassList.Add(addClass);
                        break;
                    case ItemClassType.Jewelry:
                        ItemClassList_Jewelry.Add(addClass);
                        break;
                }
            }

        }



        public void LoadItem(String fname)
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(fname);

            XmlElement root = xdoc.DocumentElement;


            foreach (XmlElement data in root.ChildNodes)
            {
                Item item = new Item();
                item.name = data.InnerText;
                if (false == int.TryParse(data.GetAttribute("HR"), out item.HR))
                {
                    throw new Exception(item.name + "を読み込み中にエラー発生");
                }

#if BETA
                item.ID = data.GetAttribute("ID");
#endif
                ItemMap.Add(item.name, item);

            }


        }

        public void UpdateIgnoreInfo(Ignore ignore, Settings setting)
        {
            foreach (Dictionary<string, EquipmentData> table in EquipDataMap)
            {
                foreach (EquipmentData data in table.Values)
                {
                   

                    if (ignore.IgnoreJudge_Equip(data, setting.IgnoreItem, setting.IgnoreClass, this))
                    {
                        data.isIgnored = false;
                    }
                    else
                        data.isIgnored = true;

                }
            }

            foreach (JewelryData data in JewelryDataMap.Values)
            {
                //string str = data.Class;
                //if (str == "")
                //    str = "無分類";


                //if (
                //    (setting.IgnoreItem && ignore.Jewelry.Contains(data.Name))
                //    ||
                //    (setting.IgnoreClass && ignore.Class_Jewelry.Contains(str))
                //    )
                //{
                //    data.isIgnored = true;
                //}
                //else
                //    data.isIgnored = false;

                if (ignore.IgnoreJudge_juerly(data, setting.IgnoreItem, setting.IgnoreClass))
                {
                    data.isIgnored = false;
                }
                else
                    data.isIgnored = true;

            }

            foreach (SkillCuffData sc in SkillCaffDataMap.Values)
            {
                //string str = sc.Class;
                //if (str == "")
                //    str = "無分類";

                //if (
                //    (setting.IgnoreItem && ignore.SkillCuff.Contains(sc.Name))
                //    ||
                //    (setting.IgnoreClass && ignore.Class_Jewelry.Contains(str))
                //    )
                //    sc.isIgnored = true;
                //else
                //    sc.isIgnored = false;

                if (ignore.IgnoreJudge_SkillCuff(sc, setting.IgnoreItem, setting.IgnoreClass))
                {
                    sc.isIgnored = false;
                }
                else
                    sc.isIgnored = true;

            }

        }

        public void UpdateEquipLevel(int UpperLevel)
        {
            for (int i = 0; i < (int)EquipKind.NumOfEquipKind; i++)
            {
                foreach (EquipmentData equip in EquipDataMap[i].Values)
                {
                    int n;
                    for (n = 0; n < equip.LevelList.Count; n++)
                    {
                        if (equip.LevelList[n] == null)
                            break;

                        if (equip.LevelList[n].GetableHR > UpperLevel)
                            break;
                    }

                    if (n < 1)
                    {
                        //作成不可能
                        equip.Level = -1;
                        equip.Slot = 0;
                        equip.Def = 0;
                    }
                    else
                    {
                        equip.Level = n;
                        equip.Slot = equip.LevelList[n - 1].Slot;
                        equip.Def = equip.LevelList[n - 1].Def;
                    }
                }
            }
        }

        private void LoadJewelry(string fname)
        {
            XmlDocument xdoc = new XmlDocument();

            xdoc.Load(fname);
            XmlElement root = xdoc.DocumentElement;


            //Jewel.xmlの情報取得
            for (XmlNode node = root.FirstChild; node != null; node = node.NextSibling)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    XmlElement JewelType = (XmlElement)node;
                    JewelryType isSP;
                    if (JewelType.Name == "SP")
                        isSP = JewelryType.SP;
                    else
                        isSP = JewelryType.Normal;

                    for (XmlNode node2 = JewelType.FirstChild; node2 != null; node2 = node2.NextSibling)
                    {
                        if (node2.NodeType != XmlNodeType.Element)
                            continue;

                        XmlElement data = (XmlElement)node2;

                        JewelryData jewelry = new JewelryData();
                        jewelry.Type = isSP;

                        //名前取得
                        jewelry.Name = data.GetAttribute("Name");

                        //レア度取得
                        jewelry.Rare = int.Parse(data.GetAttribute("Rare"));

                        //職種取得
                        jewelry.Job = Job.TypeFromString(data.GetAttribute("Job"));

                        //必要スロット数取得
                        jewelry.Slot = int.Parse(data.GetAttribute("Slot"));


                        if(data.HasAttribute("Class"))
                            jewelry.Class = data.GetAttribute("Class");


                        string tmp = jewelry.Class;
                        if (tmp == null)
                            tmp = "無分類";

                        if (!ClassList_Jewelry.Contains(tmp))
                        {
                            ClassList_Jewelry.Add(tmp);
                        }



                        List<SkillPoint> SkillList = new List<SkillPoint>();


                        for (XmlNode node3 = data.FirstChild; node3 != null; node3 = node3.NextSibling)
                        {
                            if (node3.NodeType != XmlNodeType.Element)
                                continue;

                            if (node3.Name == "Skills")
                            {
                                foreach (XmlElement sp in node3.ChildNodes)
                                {
                                    SkillPoint skill = new SkillPoint();

                                    if (!SkillBaseMap.ContainsKey(sp.InnerText))
                                    {
                                        throw new Exception(jewelry.Name + " のスキル " + sp.InnerText + "が見つかりません");
                                    }
                                    //Point取得
                                    skill.Point = int.Parse(sp.GetAttribute("Point"));
                                    //SBase登録
                                    skill.SBase = (SkillBase)SkillBaseMap[sp.InnerText];
                                    //skillbaseに追加
                                    SkillList.Add(skill);
                                }
                            }
                            else if (node3.Name == "Cost")
                            {
                                XmlElement Cost = (XmlElement)node3;
                                jewelry.CostMoney = int.Parse(Cost.GetAttribute("Money"));

                                foreach (XmlElement item in Cost.ChildNodes)
                                {
                                    if (ItemMap.ContainsKey(item.InnerText))
                                        jewelry.CostItem.Add(ItemMap[item.InnerText], int.Parse(item.GetAttribute("Num")));
                                    else
                                    {
                                        throw new Exception(jewelry.Name + "の素材「" + item.InnerText + "」に誤りがあります");
                                    }
                                }


                            }
                        }

                        jewelry.SkillList = SkillList.ToArray();
                        //BaseMapに追加
                        JewelryDataMap[jewelry.Name] = jewelry;
                    }
                }
            }


            #region 入手可能HR情報計算

            foreach (JewelryData jd in JewelryDataMap.Values)
            {
                int MaxHR = 0;

                foreach (Item item in jd.CostItem.Keys)
                {
                    if (item.HR > MaxHR)
                        MaxHR = item.HR;
                }

                jd.GetableHR = MaxHR;
            }
            #endregion
        }

        private void LoadSkillCuff(string fname)
        {
            XmlDocument xdoc = new XmlDocument();

            xdoc.Load(fname);
            XmlElement root = xdoc.DocumentElement;

            foreach (XmlNode node in root.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue;

                XmlElement Series = (XmlElement)node;

                SkillCuffSeriesType type;

                if (Series.Name == "P")
                {
                    type = SkillCuffSeriesType.P;
                }
                else if (Series.Name == "S")
                {
                    type = SkillCuffSeriesType.S;
                }
                else
                {
                    throw new Exception(Series.Name + "というシリーズはありません");
                }

                foreach (XmlNode node2 in Series.ChildNodes)
                {
                    if (node2.NodeType != XmlNodeType.Element)
                        continue;

                    XmlElement DataNode = (XmlElement)node2;

                    SkillCuffData data = new SkillCuffData();

                    data.SeriesType = type;
                    data.Name = DataNode.GetAttribute("Name");
                    data.Slot = int.Parse(DataNode.GetAttribute("Slot"));

                    if(DataNode.HasAttribute("Class"))
                        data.Class = DataNode.GetAttribute("Class");


                    data.Rare = int.Parse(DataNode.GetAttribute("Rare"));

                    string tmp = data.Class;
                    if (tmp == "" || tmp == null)
                        tmp = "無分類";

                    if (!ClassList_Jewelry.Contains(tmp))
                    {
                        ClassList_Jewelry.Add(tmp);
                    }



                    foreach (XmlNode node3 in DataNode.ChildNodes)
                    {
                        if (node3.NodeType != XmlNodeType.Element)
                            continue;


                        if (node3.Name == "Skills")
                        {
                            XmlElement SkillsNode = (XmlElement)node3;

                            List<SkillPoint> list = new List<SkillPoint>();
                            foreach (XmlNode node4 in SkillsNode.ChildNodes)
                            {
                                if (node4.NodeType != XmlNodeType.Element)
                                    continue;

                                XmlElement skillnode = (XmlElement)node4;

                                SkillPoint sp = new SkillPoint();

                                string SkillName = skillnode.InnerText;
                                if (!SkillBaseMap.ContainsKey(SkillName))
                                {
                                    throw new Exception(SkillName + "というスキルは登録されていません");
                                }

                                sp.SBase = SkillBaseMap[SkillName];

                                sp.Point = int.Parse(skillnode.GetAttribute("Point"));

                                list.Add(sp);
                            }

                            data.SkillList = list.ToArray();

                        }
                        else if (node3.Name == "Cost")
                        {
                            XmlElement CostNode = (XmlElement)node3;

                            data.CostMoney = int.Parse(CostNode.GetAttribute("Money"));


                            foreach (XmlNode node4 in CostNode.ChildNodes)
                            {
                                if (node4.NodeType != XmlNodeType.Element)
                                    continue;

                                XmlElement ItemNode = (XmlElement)node4;


                                string ItemName = ItemNode.InnerText;
                                if (!ItemMap.ContainsKey(ItemName))
                                {
                                    throw new Exception(ItemName + "というアイテムは登録されていません");
                                }

                                if (data.CostItem.ContainsKey(ItemMap[ItemName]))
                                    data.CostItem[ItemMap[ItemName]] += int.Parse(ItemNode.GetAttribute("Num"));
                                else
                                    data.CostItem[ItemMap[ItemName]] = int.Parse(ItemNode.GetAttribute("Num"));

                            }

                        }

                    }

                    SkillCaffDataMap[data.Name] = data;

                }

            }



            #region 入手可能HR情報計算

            foreach (SkillCuffData sc in SkillCaffDataMap.Values)
            {
                int MaxHR = 0;

                foreach (Item item in sc.CostItem.Keys)
                {
                    if (item.HR > MaxHR)
                        MaxHR = item.HR;
                }

                sc.GetableHR = MaxHR;
            }
            #endregion

        }

        private void LoadClothes(string fname)
        {
            XmlDocument xdoc = new XmlDocument();

            xdoc.Load(fname);
            XmlElement root = xdoc.DocumentElement;

            foreach (XmlNode node in root.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue;

                XmlElement ClothesNode = (XmlElement)node;

                ClothesData data = new ClothesData();

                data.Name = ClothesNode.InnerText;
                data.Slot = int.Parse(ClothesNode.GetAttribute("Slot"));

                if (ClothesNode.HasAttribute("Class"))
                {
                    data.Class = ClothesNode.GetAttribute("Class");

                    //if (!ClassList.Contains(data.Class))
                    //{
                    //    ClassList.Add(data.Class);
                    //}
                }

                string type = ClothesNode.GetAttribute("Type");

                if (type == "P")
                {
                    data.SetableCuffSeriesType = SkillCuffSeriesType.P;
                }
                else if (type == "S")
                    data.SetableCuffSeriesType = SkillCuffSeriesType.S;
                else
                    throw new Exception(type + "という服タイプはありません");

                ClothesDataMap[data.Name] = data;
            }
        }

        private void LoadSkillBase(string fname)//スキルベース取得
        {
            uint STypeIndex = 0;

            XmlDocument xdoc = new XmlDocument();

            xdoc.Load(fname);
            XmlElement root = xdoc.DocumentElement;

            List<uint> IDList = new List<uint>();

            //SKillBase.xmlの情報取得
            for (XmlNode node = root.FirstChild; node != null; node = node.NextSibling)
            {
                //OutputDebugString(systemStringToChar(node.Name));
                //OutputDebugString("\n");
                if (node.NodeType == XmlNodeType.Element)
                {
                    XmlElement SkillType = (XmlElement)node;
                    SkillCategoryList.Add(SkillType.GetAttribute("TypeName"));

                    for (XmlNode node2 = SkillType.FirstChild; node2 != null; node2 = node2.NextSibling)
                    {

                        SkillBase skillbase = new SkillBase();
                        XmlElement data = (XmlElement)node2;
                        //OutputDebugString(systemStringToChar(data.Name));
                        //OutputDebugString("\n");

                        skillbase.SkillCategory = STypeIndex;

                        //SkillID取得
                        try
                        {
                            skillbase.SkillId = uint.Parse(data.GetAttribute("ID"));
                        }
                        catch (Exception)
                        {
                            throw new Exception(data.GetAttribute("Name") + " スキルIDの取得に失敗しました。");
                        }

                        if (!IDList.Contains(skillbase.SkillId))
                            IDList.Add(skillbase.SkillId);
                        else
                        {
                            throw new Exception("skillbase スキルID:" + skillbase.SkillId.ToString() + "が重複しています");
                        }

                        //CString str;
                        //str.Format("%d",id);
                        //OutputDebugString(str);

                        //SkillName取得
                        skillbase.Name = data.GetAttribute("Name");

                        //OutputDebugString(SkillName);


                        //XmlNode ^option = 

                        for (XmlNode node3 = data.FirstChild; node3 != null; node3 = node3.NextSibling)
                        {
                            if (node3.NodeType == XmlNodeType.Element)
                            {
                                SkillOption skilloption = new SkillOption();
                                XmlElement option = (XmlElement)node3;

                                //スキルIDをコピー
                                skilloption.SBase = skillbase;
                                //Point取得
                                skilloption.Point = int.Parse(option.GetAttribute("Point"));
                                //発生スキル名取得
                                skilloption.Name = option.InnerText;

                                //skillbaseに追加
                                skillbase.OptionTable.Add(skilloption);

                                //SkillOptionMapに追加
                                SkillOptionMap[skilloption.Name] = skilloption;
                            }
                        }

                        //BaseMapに追加
                        SkillBaseMap[skillbase.Name] = skillbase;
                    }

                    STypeIndex++;
                }
            }
        }

        private void LoadEquip(EquipKind type, string fname)
        {
            XmlDocument xdoc = new XmlDocument();

            xdoc.Load(fname);
            XmlElement root = xdoc.DocumentElement;

            Dictionary<string, EquipmentData> List = EquipDataMap[(int)type];
            //保存先解決



            //Equip***.xmlの情報取得
            for (XmlNode node = root.FirstChild; node != null; node = node.NextSibling)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue;

                XmlElement Element = (XmlElement)node;
                bool isSP = false;
                if (Element.Name == "SP")
                    isSP = true;

                for (XmlNode node2 = Element.FirstChild; node2 != null; node2 = node2.NextSibling)
                {
                    if (node2.NodeType != XmlNodeType.Element)
                        continue;

                    XmlElement data = (XmlElement)node2;
                    EquipmentData equip = new EquipmentData();
                    equip.Kind = type;
                    equip.isSP = isSP;


                    //名前取得
                    equip.Name = data.GetAttribute("Name");



                    //Sex取得
                    equip.WearableSex = Sex.TypeFromString(data.GetAttribute("Sex"));
                    //Job取得
                    equip.WearableJob = Job.TypeFromString(data.GetAttribute("Job"));
                    //レア度取得
                    equip.Rare = int.Parse(data.GetAttribute("Rare"));


                    //クラス取得
                    if (data.HasAttribute("Class"))
                        equip.Class = data.GetAttribute("Class");
                    else
                        equip.Class = null;

                    if (data.HasAttribute("Type"))
                    {
                        equip.Type = data.GetAttribute("Type");

                        if (!TypeList.Contains(equip.Type))
                        {
                            TypeList.Add(equip.Type);
                        }

                    }
                    else
                        equip.Type = "";



                    //ラスタ開放レベル取得
                    if (data.HasAttribute("Rusta"))
                    {
                        int.TryParse(data.GetAttribute("Rusta"), out equip.RustaLv);
                    }
                    else
                        equip.RustaLv = 0;

                    if (MaxRustaLebel < equip.RustaLv)
                        MaxRustaLebel = equip.RustaLv;


                    //画像ファイル名取得
                    if (data.HasAttribute("Pict_M"))
                    {
                        equip.Pict_M = data.GetAttribute("Pict_M");
                    }

                    if (data.HasAttribute("Pict_W"))
                    {
                        equip.Pict_W = data.GetAttribute("Pict_W");
                    }

                    string tmp = equip.Class;
                    if (tmp == null)
                        tmp = "無分類";

                    if (!ClassList.Contains(tmp))
                    {
                        ClassList.Add(tmp);
                    }



                    List<SkillPoint> SkillPointList = new List<SkillPoint>();

                    for (XmlNode node3 = data.FirstChild; node3 != null; node3 = node3.NextSibling)
                    {
                        if (node3.NodeType != XmlNodeType.Element)
                            continue;

                        XmlElement option = (XmlElement)node3;

                        if (node3.Name == "Elemental")
                        {
                            equip.Element[ElementType.Dragon] = int.Parse(option.GetAttribute("Dragon"));
                            equip.Element[ElementType.Fire] = int.Parse(option.GetAttribute("Fire"));
                            equip.Element[ElementType.Ice] = int.Parse(option.GetAttribute("Ice"));
                            equip.Element[ElementType.Thunder] = int.Parse(option.GetAttribute("Thunder"));
                            equip.Element[ElementType.Water] = int.Parse(option.GetAttribute("Water"));
                        }
                        else if (node3.Name == "Skills")
                        {
                            foreach (XmlElement child in node3.ChildNodes)
                            {
                                if (child.NodeType != XmlNodeType.Element)
                                    continue;

                                SkillPoint sp = new SkillPoint();
                                sp.Point = int.Parse(child.GetAttribute("Point"));

                                if (!SkillBaseMap.ContainsKey(child.InnerText))
                                {
                                    throw new Exception(equip.Name + "のスキル" + child.InnerText + "が見つかりません");
                                }
                                else
                                    sp.SBase = (SkillBase)SkillBaseMap[child.InnerText];

                                SkillPointList.Add(sp);
                            }
                        }
                        else if (node3.Name == "Level")
                        {
                            foreach (XmlElement child in node3.ChildNodes)
                            {
                                Level level = new Level();
                                if (type == EquipKind.Weapon)
                                {
                                    level.Def = int.Parse(child.GetAttribute("Atk"));
                                }
                                else
                                {
                                    level.Def = int.Parse(child.GetAttribute("Def"));
                                }

                                level.Slot = int.Parse(child.GetAttribute("Slot"));


                                XmlElement CostEle = (XmlElement)child.FirstChild;



                                level.Money = int.Parse(CostEle.GetAttribute("Money"));

                                foreach (XmlElement item in CostEle.ChildNodes)
                                {
                                    if (!level.CostItems.ContainsKey(ItemMap[item.InnerText]))
                                        level.CostItems.Add(ItemMap[item.InnerText], int.Parse(item.GetAttribute("Num")));
                                    else
                                        level.CostItems[ItemMap[item.InnerText]] += int.Parse(item.GetAttribute("Num"));
                                }



                                int l = int.Parse(child.Name[1].ToString());//L○のとりだし

                                while (equip.LevelList.Count < l)
                                {
                                    equip.LevelList.Add(new Level());
                                }

                                equip.LevelList[l - 1] = level;
                            }
                        }
                        else if (node3.Name == "Source")
                        {
                            XmlElement SourceEle = (XmlElement)node3;
                            equip.DerivSource = new DerivationSource();
                            equip.DerivSource.level = int.Parse(SourceEle.GetAttribute("Level"));
                            equip.DerivSource.Source = SourceEle.InnerText;

                        }
                        else
                        {
                            throw new Exception("装備データロードエラー");
                        }

                    }

                    equip.SkillPointList = SkillPointList.ToArray();


                    if (List.ContainsKey(equip.Name))
                    {
                        throw new Exception(equip.Name + "が重複しています");
                    }

                    List[equip.Name] = equip;
                }


            }

            if (type != EquipKind.Weapon)
            {
                //「空き」追加
                EquipmentData BlankEquip = new EquipmentData();
                BlankEquip.Kind = type;
                BlankEquip.Name = Properties.Resources.BLANK_EQUIP_NAME;
                BlankEquip.Element.Clear();
                BlankEquip.Rare = 1;
                BlankEquip.WearableJob = JobType.COMON;
                BlankEquip.WearableSex = SexType.COMON;
                BlankEquip.SkillPointList = new SkillPoint[0];
                BlankEquip.Def = 0;
                BlankEquip.Slot = 0;
                BlankEquip.Level = 1;
                BlankEquip.LevelList.Add(new Level());
                BlankEquip.LevelList[0].Slot = 0;
                BlankEquip.LevelList[0].Money = 0;
                BlankEquip.LevelList[0].Def = 0;
                BlankEquip.LevelList[0].GetableHR = 0;
                BlankEquip.Class = null;

                List[BlankEquip.Name] = BlankEquip;
            }


            foreach (EquipmentData e in List.Values)
            {
                CalcGetableHR(e, List, new List<EquipmentData>());

                if (e.DerivSource != null)
                {
                    if (!List.ContainsKey(e.DerivSource.Source))
                    {
                        throw new Exception("「" + e.Name + "」の派生元「" + e.DerivSource.Source + "」が見つかりません");
                    }

                    EquipmentData Source = (EquipmentData)List[e.DerivSource.Source];
                    Source.Derivation.Add(new KeyValuePair<int, EquipmentData>(e.DerivSource.level, e));
                }
            }



        }

        private static void CalcGetableHR(EquipmentData equip, Dictionary<string, EquipmentData> list, List<EquipmentData> CheckingList)
        {
            if (equip.LevelList[0].GetableHR != -1)
                return;

            int MaxGainHR = 0;
            if (equip.DerivSource != null)
            {
                if (!list.ContainsKey(equip.DerivSource.Source))
                {
                    throw new Exception("「" + equip.Name + "」の派生元「" + equip.DerivSource.Source + "」が見つかりません");
                }

                EquipmentData Source = (EquipmentData)list[equip.DerivSource.Source];


                if (!CheckingList.Contains(equip))//∞ループ回避
                {
                    CheckingList.Add(equip);
                    CalcGetableHR(Source, list, CheckingList);
                    MaxGainHR = Source.LevelList[equip.DerivSource.level - 1].GetableHR;
                }
            }


            #region 作成可能HRの計算

            for (int i = 0; i < equip.LevelList.Count; i++)
            {
                if (equip.LevelList[i] == null)
                    break;

                Level level = equip.LevelList[i];

                foreach (Item item in level.CostItems.Keys)
                {
                    if (item.HR > MaxGainHR)
                        MaxGainHR = item.HR;
                }

                level.GetableHR = MaxGainHR;
            }
            #endregion
        }



       

    }


}
