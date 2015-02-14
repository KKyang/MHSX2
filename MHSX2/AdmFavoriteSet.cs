using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace MHSX2
{
    public class AdmFavoriteSet
    {
        static public void Save(EquipSet eset, Job job, Sex sex, string name)
        {
            if (eset == null)
                return;

            SaveFileNameDialog dialog = new SaveFileNameDialog();

            DateTime now = DateTime.Now;

            if (name == null)
            {
                name = job.ToString() +
                    "(" +
                    sex.ToString() +
                    ") - " +
                    now.Year.ToString("0000") + now.Month.ToString("00") + now.Day.ToString("00");

                dialog.textBox_filename.Text = name;

                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                name = dialog.textBox_filename.Text;


                foreach (char c in Path.GetInvalidFileNameChars())//使用できない文字を置き換え
                {
                    name = name.Replace(c, '_');
                }
            }

            string filepath = SubFunc.MakeFilePath(Properties.Resources.DNAME_FAVORITE, name) + "." + Properties.Resources.EXTENSION_EQUIPSET;

            if (!Directory.Exists(Properties.Resources.DNAME_FAVORITE))
            {
                Directory.CreateDirectory(Properties.Resources.DNAME_FAVORITE);
            }
            else if (File.Exists(filepath))
            {
                if (MessageBox.Show("既に存在しています。\r\n上書きしますか？", "重複確認", MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            }



            List<string> writestring = new List<string>();

            writestring.Add(job.ToString());
            writestring.Add(sex.ToString());

            for (int i = 0; i < (int)EquipKind.NumOfEquipKind; i++)
            {
                Equipment equip = eset.Equips[i];

                if (equip.EquipData == null)
                {
                    writestring.Add("");
                    continue;
                }
                else if (equip.EquipData.Name == "なし")
                {
                    writestring.Add("");
                    continue;
                }

                string line = equip.EquipData.Name;

                line += "," + equip.Level.ToString();

                List<JewelryData_base> list = equip.GetFixedJewelys();

                foreach (JewelryData_base jd in list)
                {
                    line += "," + jd.Name;
                }

                writestring.Add(line);
            }


            if (eset.PigClothes.Clothes != null)
            {
                string line = eset.PigClothes.Clothes.Name;

                for (int i = 0; i < eset.PigClothes.SkillCuffs.Length; i++)
                {
                    if (eset.PigClothes.SkillCuffs[i] != null)
                    {
                        line += "," + eset.PigClothes.SkillCuffs[i].Name;
                    }
                }

                writestring.Add(line);
            }
            else
                writestring.Add("");



            File.WriteAllLines(filepath, writestring.ToArray());


            MessageBox.Show("「" + name + "」で保存しました");

        }

        static public void Delete(string name)
        {
            string filepath = SubFunc.MakeFilePath(Properties.Resources.DNAME_FAVORITE, name) + "." + Properties.Resources.EXTENSION_EQUIPSET;

            File.Delete(filepath);
        }

        static public EquipSet Load(string name, BaseData basedata, out string job, out string sex)
        {
            bool errflag = false;
            List<string> notfound = new List<string>();

            EquipSet eset = new EquipSet();

            string filepath = SubFunc.MakeFilePath(Properties.Resources.DNAME_FAVORITE, name) + "." + Properties.Resources.EXTENSION_EQUIPSET;

            string[] lines = File.ReadAllLines(filepath);

            job = lines[0];
            sex = lines[1];

            for (int i = 0; i < (int)EquipKind.NumOfEquipKind; i++)
            {
                string line = lines[2 + i];

                string[] tokens = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length < 1)
                {
                    eset.Equips[i].EquipData = null;
                    continue;
                }

                if (basedata.EquipDataMap[i].ContainsKey(tokens[0]))
                    eset.Equips[i].EquipData = (EquipmentData)basedata.EquipDataMap[i][tokens[0]];
                else
                {
                    if (tokens[0] != "スロット3装備")
                    {
                        errflag = true;
                        notfound.Add(tokens[0]);
                    }

                    EquipmentData data = new EquipmentData();
                    data.Kind = (EquipKind)i;
                    data.Name = tokens[0];
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
                    eset.Equips[i].EquipData = data;
                }

                int begin = 1;



                eset.Equips[i].Level = eset.Equips[i].EquipData.LevelList.Count;


                if (tokens.Length > 1)
                {
                    int level;

                    if (int.TryParse(tokens[begin], out level))
                    {
                        begin++;
                        eset.Equips[i].Level = level;
                    }
                }

                if (eset.Equips[i].EquipData != null)
                {
                    int level = eset.Equips[i].Level;
                    List<Level> LevelList = eset.Equips[i].EquipData.LevelList;
                    eset.Equips[i].Def = LevelList[level - 1].Def;
                    eset.Equips[i].Slot = LevelList[level - 1].Slot;
                }


                for (int j = begin; j < tokens.Length; j++)
                {
                    if (basedata.JewelryDataMap.ContainsKey(tokens[j]))
                        eset.Equips[i].jewelrys[j - begin] = (JewelryData)basedata.JewelryDataMap[tokens[j]];
                    else
                    {
                        errflag = true;
                        notfound.Add(tokens[j]);
                    }
                }

            }

            if (lines.Length > 2 + (int)EquipKind.NumOfEquipKind)
            {
                string[] words = lines[2 + (int)EquipKind.NumOfEquipKind].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (words.Length > 1)
                {

                    if (basedata.ClothesDataMap.ContainsKey(words[0]))
                    {
                        eset.PigClothes.Clothes = basedata.ClothesDataMap[words[0]];


                        for (int i = 1; i < words.Length; i++)
                        {
                            if (basedata.SkillCaffDataMap.ContainsKey(words[i]))
                            {
                                eset.PigClothes.SetJewelry(basedata.SkillCaffDataMap[words[i]]);
                            }
                        }
                    }
                    else
                    {
                        ClothesData tmp = new ClothesData();
                        tmp.Name = words[0];
                        tmp.SetableCuffSeriesType = SkillCuffSeriesType.P;
                        tmp.Slot = 3;

                        notfound.Add(tmp.Name);
                    }
                }
            }


            if (errflag)
            {
                string str = "";
                foreach (string n in notfound)
                {
                    str += n + "\r\n";
                }

                str += "以上のデータが見つかりませんでした";

                MessageBox.Show(str);
            }

            return eset;
        }
    }
}
