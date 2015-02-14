using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace MHSX2
{
    public class AdmSearchHistory
    {
        public static void Save(List<EquipSet> list, string fname, SearchCondition cond)
        {
            if (fname == null)
            {
                SaveFileNameDialog dialog = new SaveFileNameDialog();
                dialog.ShowDialog();
            }



            HistFile hist = new HistFile();

            hist.sex = cond.sex.type;
            hist.job = cond.job.type;

            hist.ignore = cond.ignore;


            foreach (KeyValuePair<SkillBase, SkillPointCondition> pair in cond.SkillPointConditionTable)
            {
                if (!pair.Value.isIgnore)
                    hist.SkillOrder.Add(pair.Key.Name, pair.Value.Point);
            }


            hist.EquipOrder = CreateSetSaveData(hist, cond.ESet);



            foreach (EquipSet set in list)
            {
                SetSaveData setdata = CreateSetSaveData(hist, set);

                if (setdata == null)
                {
                    MessageBox.Show("この履歴を保存する事に失敗しました");
                    return;
                }

                hist.Sets.Add(setdata);
            }

            string fpath = SubFunc.MakeFilePath(MHSX2.Properties.Resources.DNAME_HISTORY, fname);

            FileStream fs = new FileStream(fpath, FileMode.Create, FileAccess.Write);

            BinaryFormatter bf = new BinaryFormatter();
            //シリアル化して書き込む
            bf.Serialize(fs, hist);
            fs.Close();
        }

        private static SetSaveData CreateSetSaveData(HistFile hist, EquipSet set)
        {
            SetSaveData setdata = new SetSaveData();

            setdata.No = (uint)set.No;

            //各部位の装備品をインデックス保存
            for (int i = 0; i < (int)EquipKind.NumOfEquipKind; i++)
            {
                EquipSaveData data = new EquipSaveData();
                Equipment equip = set.Equips[i];

                if (equip.isChecked == false)
                {
                    continue;
                }


                if (equip.EquipData == null)
                {
                    setdata.Sets[i] = null;
                }
                else
                {
                    data.level = (byte)equip.Level;

                    if (hist.UseEquipList.Contains(equip.EquipData.Name))
                    {
                        data.EquipIndex = (ushort)hist.UseEquipList.IndexOf(equip.EquipData.Name);
                    }
                    else
                    {
                        data.EquipIndex = (ushort)hist.UseEquipList.Count;

                        if (data.EquipIndex == ushort.MaxValue)
                        {
                            return null;
                        }

                        hist.UseEquipList.Add(equip.EquipData.Name);
                    }


                    //装飾品のインデックス保存
                    List<ushort> jewelrys = new List<ushort>();

                    for (int j = 0; j < set.Equips[i].jewelrys.Length; j++)
                    {
                        JewelryData jd = set.Equips[i].jewelrys[j];
                        if (jd == null)
                            continue;

                        if (hist.UseJewelyList.Contains(jd.Name))
                        {
                            jewelrys.Add((ushort)hist.UseJewelyList.IndexOf(jd.Name));
                        }
                        else
                        {
                            jewelrys.Add((ushort)hist.UseJewelyList.Count);
                            hist.UseJewelyList.Add(jd.Name);
                        }
                    }


                    data.Jewely = jewelrys.ToArray();
                    setdata.Sets[i] = data;
                }
            }

            if (set.PigClothes.Clothes != null)
            {
                PigClothesSaveData savedata = new PigClothesSaveData();

                ushort index;
                if (!hist.UsePigClothesList.Contains(set.PigClothes.Clothes.Name))
                {
                    index = (ushort)hist.UsePigClothesList.Count;
                    hist.UsePigClothesList.Add(set.PigClothes.Clothes.Name);
                }
                else
                    index = (ushort)hist.UsePigClothesList.IndexOf(set.PigClothes.Clothes.Name);

                savedata.ClothesIndex = index;

                List<ushort> SkillCuffList = new List<ushort>();

                for (int i = 0; i < set.PigClothes.SkillCuffs.Length; i++)
                {
                    if (set.PigClothes.SkillCuffs[i] != null)
                    {
                        if (!hist.UseSkillCuffList.Contains(set.PigClothes.SkillCuffs[i].Name))
                        {
                            index = (ushort)hist.UseSkillCuffList.Count;
                            hist.UseSkillCuffList.Add(set.PigClothes.SkillCuffs[i].Name);
                        }
                        else
                        {
                            index = (ushort)hist.UseSkillCuffList.IndexOf(set.PigClothes.SkillCuffs[i].Name);
                        }
                        SkillCuffList.Add(index);
                    }
                }

                savedata.SkillCuf = SkillCuffList.ToArray();

                setdata.PigClothes = savedata;
            }
            return setdata;
        }

        public static List<EquipSet> Load(string fname, BaseData basedata, out SearchCondition cond)
        {
            Dictionary<string, EquipmentData_base> NotFoundEquips = new Dictionary<string, EquipmentData_base>();
            List<string> NotFoundJewelrys = new List<string>();

            string fpath = SubFunc.MakeFilePath(MHSX2.Properties.Resources.DNAME_HISTORY, fname);

            FileStream fs = new FileStream(fpath, FileMode.Open, FileAccess.Read);
            BinaryFormatter bf = new BinaryFormatter();
            HistFile hist = (HistFile)bf.Deserialize(fs);
            fs.Close();

            cond = new SearchCondition();

            cond.sex = new Sex(hist.sex);
            cond.job = new Job(hist.job);
            cond.ignore = hist.ignore;


            EquipmentData Notthing = new EquipmentData();
            Notthing.Kind = EquipKind.Weapon;
            Notthing.Name = "なし";
            Notthing.Element.Clear();
            Notthing.Rare = 1;
            Notthing.WearableJob = JobType.COMON;
            Notthing.WearableSex = SexType.COMON;
            Notthing.SkillPointList = new SkillPoint[0];


            Notthing.LevelList.Add(new Level());
            Notthing.LevelList[0].Slot = 0;
            Notthing.LevelList[0].Money = 0;
            Notthing.LevelList[0].Def = 0;
            Notthing.LevelList[0].GetableHR = 0;


            //スロット3のみの装備を追加
            EquipmentData[] Slot3s = new EquipmentData[(int)EquipKind.NumOfEquipKind];

            for (int i = 0; i < (int)EquipKind.NumOfEquipKind; i++)
            {
                EquipmentData data = new EquipmentData();
                data.Kind = (EquipKind)i;
                data.Name = "スロット3装備";
                data.Element.Clear();
                data.Rare = 1;
                data.WearableJob = JobType.COMON;
                data.WearableSex = SexType.COMON;
                data.SkillPointList = new SkillPoint[0];
                data.Def = 0;
                data.Slot = 3;
                data.Level = 1;
                data.LevelList.Add(new Level());
                data.LevelList[0].Slot = 3;
                data.LevelList[0].Money = 0;
                data.LevelList[0].Def = 0;
                data.LevelList[0].GetableHR = 0;
                Slot3s[i] = data;
            }



            List<EquipSet> list = new List<EquipSet>();

            bool errflag = false;


            //指定装備をロード
            if (hist.EquipOrder != null)
                cond.ESet = CreateESetFromSetSaveData(basedata, NotFoundEquips, NotFoundJewelrys, hist, Notthing, Slot3s, ref errflag, hist.EquipOrder);
            else
                cond.ESet = new EquipSet();


            if (hist.SkillOrder != null)
            {
                foreach (KeyValuePair<string, int> pair in hist.SkillOrder)
                {
                    if (!basedata.SkillBaseMap.ContainsKey(pair.Key))
                        continue;

                    SkillBase sb = basedata.SkillBaseMap[pair.Key];

                    SkillPointCondition spcond = new SkillPointCondition(sb, pair.Value);

                    cond.SkillPointConditionTable.Add(sb, spcond);
                }

            }



            for (int i = 0; i < hist.Sets.Count; i++)
            {
                SetSaveData SetData = hist.Sets[i];

                EquipSet eset = CreateESetFromSetSaveData(basedata, NotFoundEquips, NotFoundJewelrys, hist, Notthing, Slot3s, ref errflag, SetData);

                list.Add(eset);
            }

            if (errflag)
            {
                string str = "次の装備がdatから見つからなかったので適当にこさえました。\r\n";

                foreach (string name in NotFoundEquips.Keys)
                {
                    str += name + "\r\n";
                }

                foreach (string name in NotFoundJewelrys)
                {
                    str += name + "\r\n";
                }

                MessageBox.Show(str);
            }

            return list;
        }

        private static EquipSet CreateESetFromSetSaveData(BaseData basedata, Dictionary<string, EquipmentData_base> NotFoundEquips, List<string> NotFoundJewelrys, HistFile hist, EquipmentData Notthing, EquipmentData[] Slot3s, ref bool errflag, SetSaveData SetData)
        {
            EquipSet eset = new EquipSet();
            eset.No = (int)SetData.No;

            for (int j = 0; j < SetData.Sets.Length; j++)
            {
                EquipSaveData esdata = SetData.Sets[j];
                Equipment equip = new Equipment();

                if (esdata == null)
                    continue;

                string ename = hist.UseEquipList[esdata.EquipIndex];

                if (!basedata.EquipDataMap[j].ContainsKey(ename))
                {
                    if (ename == "なし")
                    {
                        equip.EquipData = Notthing;
                    }
                    else if (ename == "スロット3装備")
                    {
                        equip.EquipData = Slot3s[j];
                    }
                    else
                    {
                        //NOTFOUND
                        errflag = true;
                        if (!NotFoundEquips.ContainsKey(ename))
                        {
                            EquipmentData data = new EquipmentData();
                            data.Kind = (EquipKind)j;
                            data.Name = ename + "(仮)";
                            data.Element.Clear();
                            data.Rare = 1;
                            data.WearableJob = JobType.COMON;
                            data.WearableSex = SexType.COMON;
                            data.SkillPointList = new SkillPoint[0];
                            data.Def = 0;
                            data.Slot = 3;
                            data.Level = 1;
                            data.LevelList.Add(new Level());
                            data.LevelList[0].Slot = 3;
                            data.LevelList[0].Money = 0;
                            data.LevelList[0].Def = 0;
                            data.LevelList[0].GetableHR = 0;
                            NotFoundEquips.Add(ename, data);
                        }
                        else
                        {
                            equip.EquipData = (EquipmentData)NotFoundEquips[ename];
                        }
                    }
                }
                else
                {
                    equip.EquipData = basedata.EquipDataMap[j][ename];

                    equip.Level = (int)esdata.level;

                    equip.Def = equip.EquipData.LevelList[equip.Level - 1].Def;
                    equip.Slot = equip.EquipData.LevelList[equip.Level - 1].Slot;
                }


                List<JewelryData> jlist = new List<JewelryData>();

                for (int k = 0; k < esdata.Jewely.Length; k++)
                {
                    string jname = hist.UseJewelyList[esdata.Jewely[k]];

                    if (!basedata.JewelryDataMap.ContainsKey(jname))
                    {
                        errflag = true;
                        if (!NotFoundJewelrys.Contains(jname))
                        {
                            NotFoundJewelrys.Add(jname);
                        }

                        continue;
                    }

                    JewelryData jd = basedata.JewelryDataMap[jname];

                    jlist.Add(jd);
                }

                equip.jewelrys = new JewelryData[3];

                for (int i = 0; i < jlist.Count && i < equip.jewelrys.Length; i++)
                {
                    equip.jewelrys[i] = jlist[i];
                }


                eset.Equips[j] = equip;
            }


            if (SetData.PigClothes != null)
            {
                string name = hist.UsePigClothesList[SetData.PigClothes.ClothesIndex];

                eset.PigClothes = new PigClothes();
                if (basedata.ClothesDataMap.ContainsKey(name))
                {
                    eset.PigClothes.Clothes = basedata.ClothesDataMap[name];
                }
                else
                {
                    errflag = true;
                    if (NotFoundEquips.ContainsKey(name))
                    {//装備なし
                        eset.PigClothes.Clothes = new ClothesData();
                        eset.PigClothes.Clothes.Name = name + "(仮)";
                        eset.PigClothes.Clothes.Slot = 2;
                        eset.PigClothes.Clothes.SetableCuffSeriesType = SkillCuffSeriesType.P;

                        NotFoundEquips.Add(name, eset.PigClothes.Clothes);
                    }
                    else
                        eset.PigClothes.Clothes = (ClothesData)NotFoundEquips[name];
                }


                int addSkillCufCount = 0;
                foreach (ushort scindex in SetData.PigClothes.SkillCuf)
                {
                    name = hist.UseSkillCuffList[scindex];

                    if (basedata.SkillCaffDataMap.ContainsKey(name))
                    {
                        eset.PigClothes.SkillCuffs[addSkillCufCount] = basedata.SkillCaffDataMap[name];
                    }
                    else
                    {//datに装備なし
                        errflag = true;
                        if (!NotFoundJewelrys.Contains(name))
                        {
                            NotFoundJewelrys.Add(name);
                        }

                        eset.PigClothes.SkillCuffs[addSkillCufCount] = new SkillCuffData();
                        eset.PigClothes.SkillCuffs[addSkillCufCount].Name = name + "(仮)";
                        eset.PigClothes.SkillCuffs[addSkillCufCount].Slot = 1;
                        eset.PigClothes.SkillCuffs[addSkillCufCount].Type = JewelryType.SkillCuff;
                        eset.PigClothes.SkillCuffs[addSkillCufCount].SeriesType = SkillCuffSeriesType.S;
                        eset.PigClothes.SkillCuffs[addSkillCufCount].SkillList = new SkillPoint[0];

                    }

                    addSkillCufCount++;
                }

            }
            return eset;
        }

        public static void Delete(string fname)
        {
            string fpath = SubFunc.MakeFilePath(MHSX2.Properties.Resources.DNAME_HISTORY, fname);
            File.Delete(fpath);
        }


        [Serializable()]
        class EquipSaveData
        {
            public byte level;
            public ushort EquipIndex;
            public ushort[] Jewely;
        }

        [Serializable()]
        class PigClothesSaveData
        {
            public ushort ClothesIndex;
            public ushort[] SkillCuf;
        }

        [Serializable()]
        class SetSaveData
        {
            public uint No;
            public EquipSaveData[] Sets = new EquipSaveData[(int)EquipKind.NumOfEquipKind];
            public PigClothesSaveData PigClothes;
        }


        [Serializable()]
        class HistFile
        {
            public SexType sex;
            public JobType job;
            public Dictionary<string, int> SkillOrder = new Dictionary<string, int>();
            public SetSaveData EquipOrder = new SetSaveData();

            public List<string> UseEquipList = new List<string>();
            public List<string> UseJewelyList = new List<string>();
            public List<string> UsePigClothesList = new List<string>();
            public List<string> UseSkillCuffList = new List<string>();
            public List<SetSaveData> Sets = new List<SetSaveData>();
            public Ignore ignore = new Ignore();
        }
    }
}
