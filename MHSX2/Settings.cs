using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

namespace MHSX2
{
    public class SkillSet
    {
        public string name;
        //public Job job = null;
        public List<string> list = new List<string>();
    }

    public class Settings
    {
        public Settings()
        {
            ThreadNum = Environment.ProcessorCount;
            if (ThreadNum > 8 || ThreadNum < 0)
                ThreadNum = 1;
        }

        public int ThreadNum;// = Environment.ProcessorCount;//CPUInfo.GetCPUCoreNum();
        public int StopSearchCount = 10000;
        public bool OptimizeEquip = false;
        //public bool IgnoreDef = true;
        //public bool OptimizeHard = false;
        public ProcessPriorityClass Priority = ProcessPriorityClass.BelowNormal;
        public bool IgnoreItem = false;
        public bool IgnoreSkill = false;
        public bool IgnoreClass = false;
        public List<string> FavoriteSkills = new List<string>();
        public List<SkillSet> SkillSets = new List<SkillSet>();
        public SexType sex = SexType.MAN;
        public int def_lower = 0;
        public int rare_lower = 1;
        public int rare_upper = 20;
        public int HRLimit = 3999;
        public string PictureServer = "http://fortune.adam.ne.jp/mhf/cbbs/file";
        public bool UseNetwork = true;
        //public int RustaLv = 0;
        public WindowStatus MainBounds = new WindowStatus(0, 0, 1024, 768,FormWindowState.Normal);
        public WindowStatus EditBounds = new WindowStatus(0, 0, 1024, 768,FormWindowState.Normal);
        public bool PlaySound = true;
        public string SoundFilePath = System.Environment.GetEnvironmentVariable("windir") + "\\Media\\notify.wav";
        public bool CheckNewVersion = true;
        public int LastVersionCheckDate = 0;
        public String PigClothes = "";

        public int RankLimitKind
        {
            get
            {
                return BaseData.mDefine.GetRankLimitKindIndex(HRLimit);

            }
        }
        
    

    }
}
