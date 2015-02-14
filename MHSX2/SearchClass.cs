using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using System.Diagnostics;

namespace MHSX2
{
    public class SearchClass
    {
        public int ThreadNo;
        private Thread thread;
        private MHSX2Form parent;
        private SearchCondition condition;
        public SearchClass(MHSX2Form p, SearchCondition c)
        {
            parent = p;
            condition = c;
        }
        public EquipmentDataTag[][] PotentialEquipTagArrayArray;
        public JewelryDataTag[] SPJewelryDataTags;
        public PlusJewelryListTag[] PlusJewelyDataTags;
        public PlusJewelryDataTag[] SinglePlusJewelryDataTags;

        //public JewelryDataTag[][] PotentialJewelryTagArrayArray;//0にはSP　1～3にはスロットN必要な装飾品
        //public SpecificJewelryDataTag[] SpecificJewelryArrayArray;//ショートカットに利用可能な装飾品のTable

        public JewelryDataTag[][] PotentialSkillCuffTagArrayArray;


        Dictionary<SkillBase, int> OrderdEquipSkillPoint = new Dictionary<SkillBase, int>();


        public bool[] isOrderd;
        public volatile int[] SearchBeginPoint = new int[(int)EquipKind.NumOfEquipKind];
        public volatile int[] SearchEndPoint = new int[(int)EquipKind.NumOfEquipKind];
        public volatile int[] NowSearching = new int[(int)EquipKind.NumOfEquipKind];
        private volatile bool bStop = true;//ユーザーによって中断されたときtrue
        public volatile SearchClassState State = SearchClassState.Finished;//検索中ture 分配待ちのときfalse
        public volatile SearchClassRequestResult RequestResult = SearchClassRequestResult.None;
        public volatile object lock_State = new object();
        public volatile List<SearchClass> WaitingMeList = new List<SearchClass>();
        public volatile bool isUpdatetime = false;
        private SortedList<SkillBase, SkillPointCondition> RequirementSkillPointsSortedList = null;
        private int ADDCOUNT_LEN = 0;



        private int OrderdSkillNum = 0;//実際に指定されたスキル数。除外指定を除く。

        private int[] SinglePlusJewelryUseCount;
        private EquipmentDataTag[] EquipDataTags = new EquipmentDataTag[(int)EquipKind.NumOfEquipKind];
        private int[] SPJewwlryUseCount;
        private int[][] SkillCuffUseCount = new int[2][];

        public void Start()
        {
            thread = new Thread(Run);
            thread.Start();
        }

        public void Kill()
        {
            thread.Abort();
        }

        public void Stop()
        {
            bStop = true;
        }

        public void Resume()
        {
            bStop = false;
        }

        public bool isLive
        {
            get
            {
                return thread.IsAlive;
            }
        }

        private void Run()
        {

            bStop = false;
            lock (lock_State)
            {
                State = SearchClassState.Searching;
            }

            ulong CheckedNum = 0;


            bool isAdditionalSearching = false;

            bool isIgnoreDef = condition.isIgnoreDef;

            int[] SlotTypeNumArray = new int[4];//1-sp 1～3nomal
            int[] tmpSlotTypeNumArray1 = new int[3];
            int[] tmpSlotTypeNumArray2 = new int[3];
            int[] tmpSlotTypeNumArray3 = new int[3];
            int[] tmpSlotTypeNumArray = new int[3];

            int[] SKillCuffSlotArray = new int[2];//1or2
            int[] SKillCuffSlotArray2 = new int[2];


            Random rand = new Random();
            //満たすべきスキルポイント
            RequirementSkillPointsSortedList = condition.SkillPointConditionTable;

            //装備が決まった後あとどれだけスキルポイントが必要かをしめす配列
            SkillPoint[] RestPoint = new SkillPoint[RequirementSkillPointsSortedList.Count];
            SkillPoint[] tmpRestPoint = new SkillPoint[RequirementSkillPointsSortedList.Count];
            SkillPoint[] JewelrysSkillPoint = new SkillPoint[RequirementSkillPointsSortedList.Count];
            SkillPoint[] SPJewelrySkillPoint = new SkillPoint[RequirementSkillPointsSortedList.Count];


            int[] PlusJewelySlotCount = { 0, 0, 0 };//スロット別の装飾品数

            for (int i = 0; i < PlusJewelyDataTags.Length; i++)
            {
                int n = PlusJewelyDataTags[i].Length;
                if(ADDCOUNT_LEN < n)
                    ADDCOUNT_LEN = n;

                for (int j = 0; j < n; j++)
                {
                    PlusJewelySlotCount[PlusJewelyDataTags[i][j].Slot - 1]++;
                }
            }

            int[] tmpAddCount = new int[ADDCOUNT_LEN];
            int[] tmpRestSlot = new int[3];

            int[] SingleJewelrySlotCount = { 0, 0, 0 };

            //どの装飾品をどの数だけ使用するか

            SinglePlusJewelryUseCount = new int[SinglePlusJewelryDataTags.Length];

            for (int i = 0; i < SinglePlusJewelryDataTags.Length; i++)
            {
                if (SinglePlusJewelryDataTags[i].jdt != null)
                    SingleJewelrySlotCount[SinglePlusJewelryDataTags[i].Slot - 1]++;
            }


            SPJewwlryUseCount = new int[SPJewelryDataTags.Length];
           

            int[] EligibleSPJewelryUseCount = null;
            int[][] EligibleJewelryUseCount = null;
            int[][] EligibleSkillCuffUseCount = new int[2][];
            int[] EligibleSingleJewelryUseCount = null;
            //            int[] SpecificJewelryUseCount = new int[SpecificJewelryArrayArray.Length];


            foreach (SkillPointCondition spc in condition.SkillPointConditionTable.Values)
            {
                if (!spc.isIgnore)
                {
                    OrderdSkillNum++;
                }
            }

#if DEBUG

            //ulong CheckCount_amount = 0;
#endif



            #region　SKillCuffSlotArray構築
            if (condition.ESet.PigClothes.Clothes != null && condition.ESet.PigClothes.isChecked)
            {


                int x = condition.ESet.PigClothes.GetRestSlotNum();
                if (x > 0)
                    SKillCuffSlotArray[x - 1] = 1;


                for (int i = 1; i > 0; i--)//スロット2のスキルカフがなければもう分解しちゃう
                {
                    if (PotentialSkillCuffTagArrayArray[i].Length == 0 && SKillCuffSlotArray[i] > 0)
                    {
                        SKillCuffSlotArray[i - 1]++;
                        SKillCuffSlotArray[0]++;
                        SKillCuffSlotArray[i] = 0;
                    }
                }
            }

            #endregion

            for (int i = 0; i < 2; i++)
            {
                SkillCuffUseCount[i] = new int[PotentialSkillCuffTagArrayArray[i].Length];
            }


            for (int i = 0; i < RequirementSkillPointsSortedList.Count; i++)
            {
                RestPoint[i] = new SkillPoint();
                tmpRestPoint[i] = new SkillPoint();
                JewelrysSkillPoint[i] = new SkillPoint();
                SPJewelrySkillPoint[i] = new SkillPoint();

                RestPoint[i].SBase = tmpRestPoint[i].SBase = SPJewelrySkillPoint[i].SBase = JewelrysSkillPoint[i].SBase = (SkillBase)RequirementSkillPointsSortedList.Keys[i];


                SkillPointCondition sp = RequirementSkillPointsSortedList.Values[i];
                if (sp.isIgnore)
                {
                    RestPoint[i].Point = -Math.Abs(sp.Point);

                }
                else
                {
                    RestPoint[i].Point = Math.Abs(sp.Point);
                }
            }


            int RestDef = condition.defence_lower;//最低防御ラインに必要な選択した装備の防御力の合計
            int SelectedEquipDef = 0;
            int numSP = 0;
            int numBlankEquip = 0;
            int numGRankEquipNum_Orderd = 0;

            Dictionary<string, int> TypeUseCountTable = new Dictionary<string, int>();
            foreach (string str in condition.TypeNumOrder.Keys)
            {
                TypeUseCountTable.Add(str, 0);
            }

            SlotTypeNumArray[0] = 0;
            SlotTypeNumArray[1] = 0;
            SlotTypeNumArray[2] = 0;
            SlotTypeNumArray[3] = 0;

            Dictionary<EquipSetKey, EquipTagTreeNode>[] AdditionalSearchTreeDict = new Dictionary<EquipSetKey, EquipTagTreeNode>[(int)EquipKind.NumOfEquipKind];
            EquipTagTreeNode[] NowSearchTrees = new EquipTagTreeNode[(int)EquipKind.NumOfEquipKind];

            for (int i = 0; i < (int)EquipKind.NumOfEquipKind; i++)
            {

                AdditionalSearchTreeDict[i] = new Dictionary<EquipSetKey, EquipTagTreeNode>();//ついでに初期化


                if (isOrderd[i] != true)
                {
                    NowSearching[i] = SearchBeginPoint[i];
                    EquipDataTags[i] = PotentialEquipTagArrayArray[i][SearchBeginPoint[i]];
                    SelectedEquipDef += EquipDataTags[i].Def;

                    if (EquipDataTags[i].equipdata.Name == Properties.Resources.BLANK_EQUIP_NAME)
                    {
                        numBlankEquip++;
                    }

                    if (EquipDataTags[i].equipdata.isSP)
                    {
                        numSP++;
                        SlotTypeNumArray[0] += 1;
                    }
                    else
                    {
                        int num = EquipDataTags[i].Slot;
                        if (num > 0)
                            SlotTypeNumArray[num]++;
                    }

                    if (condition.isOrderTypeNum && TypeUseCountTable.ContainsKey(EquipDataTags[i].equipdata.Type))
                    {
                        TypeUseCountTable[EquipDataTags[i].equipdata.Type]++;
                    }



                    foreach (SkillPointTag spt in EquipDataTags[i].SkillPointTags)
                    {
                        RestPoint[spt.index].Point -= spt.Point;
                    }
                }
                else
                {
                    Equipment equip = condition.ESet[(EquipKind)i];
                    RestDef -= equip.Def;//目標防御力から固定装備をひく

                    if (equip.EquipData.Name == Properties.Resources.BLANK_EQUIP_NAME)
                    {
                        numBlankEquip++;
                    }

                    foreach (String grank in BaseData.mDefine.GRankEquipType)
                    {
                        if (equip.EquipData.Type.Equals(grank))
                        {
                            numGRankEquipNum_Orderd++;
                            break;
                        }
                    }


                    if (equip.EquipData.isSP)
                    {
                        numSP++;
                        SlotTypeNumArray[0] += equip.GetRestSlotNum();
                    }
                    else
                    {
                        int rest = equip.GetRestSlotNum();

                        if (rest > 0)
                            SlotTypeNumArray[rest]++;
                    }

                    if (condition.isOrderTypeNum && TypeUseCountTable.ContainsKey(equip.EquipData.Type))
                    {
                        TypeUseCountTable[equip.EquipData.Type]++;
                    }


                    #region 指定装備のスキルをRestPointから除算
                    foreach (SkillPoint sp in equip.EquipData.SkillPointList)
                    {
                        for (int j = 0; j < RestPoint.Length; j++)
                        {
                            if (sp.SBase == RestPoint[j].SBase)
                            {
                                SkillPointCondition cond_SP = RequirementSkillPointsSortedList.Values[j];//元の指定条件を取得

                                if (cond_SP.isIgnore)
                                {
                                    if (cond_SP.Point >= 0)
                                        RestPoint[j].Point += sp.Point;
                                    else
                                        RestPoint[j].Point -= sp.Point;
                                }
                                else
                                {
                                    if (cond_SP.Point >= 0)
                                        RestPoint[j].Point -= sp.Point;
                                    else
                                        RestPoint[j].Point += sp.Point;
                                }
                                break;
                            }
                        }

                        if (OrderdEquipSkillPoint.ContainsKey(sp.SBase))
                        {
                            OrderdEquipSkillPoint[sp.SBase] += sp.Point;
                        }
                        else
                        {
                            OrderdEquipSkillPoint[sp.SBase] = sp.Point;
                        }

                    }
                    #endregion

                    #region 指定装飾品のスキルポイントをRestPointから除算

                    List<JewelryData_base> jdlist = equip.GetFixedJewelys();

                    foreach (JewelryData jd in jdlist)
                    {
                        foreach (SkillPoint sp in jd.SkillList)
                        {
                            for (int k = 0; k < RestPoint.Length; k++)
                            {
                                if (sp.SBase == RestPoint[k].SBase)
                                {
                                    SkillPointCondition cond_SP = RequirementSkillPointsSortedList.Values[k];//元の指定条件を取得

                                    if (cond_SP.isIgnore)
                                    {
                                        if (cond_SP.Point >= 0)
                                            RestPoint[k].Point += sp.Point;
                                        else
                                            RestPoint[k].Point -= sp.Point;
                                    }
                                    else
                                    {
                                        if (cond_SP.Point >= 0)
                                            RestPoint[k].Point -= sp.Point;
                                        else
                                            RestPoint[k].Point += sp.Point;
                                    }
                                    break;

                                }
                            }

                            if (OrderdEquipSkillPoint.ContainsKey(sp.SBase))
                            {
                                OrderdEquipSkillPoint[sp.SBase] += sp.Point;
                            }
                            else
                            {
                                OrderdEquipSkillPoint[sp.SBase] = sp.Point;
                            }

                        }
                    }


                    #endregion
                }
            }


            if (condition.ESet.PigClothes.isChecked && condition.ESet.PigClothes.Clothes != null)
            {
                PigClothes clothes = condition.ESet.PigClothes;

                foreach (SkillCuffData scd in clothes.GetFixedJewelys())
                {
                    foreach (SkillPoint sp in scd.SkillList)
                    {
                        for (int k = 0; k < RestPoint.Length; k++)
                        {
                            if (sp.SBase == RestPoint[k].SBase)
                            {
                                SkillPointCondition cond_SP = RequirementSkillPointsSortedList.Values[k];//元の指定条件を取得

                                if (cond_SP.isIgnore)
                                {
                                    if (cond_SP.Point >= 0)
                                        RestPoint[k].Point += sp.Point;
                                    else
                                        RestPoint[k].Point -= sp.Point;
                                }
                                else
                                {
                                    if (cond_SP.Point >= 0)
                                        RestPoint[k].Point -= sp.Point;
                                    else
                                        RestPoint[k].Point += sp.Point;
                                }
                                break;

                            }
                        }

                        if (OrderdEquipSkillPoint.ContainsKey(sp.SBase))
                        {
                            OrderdEquipSkillPoint[sp.SBase] += sp.Point;
                        }
                        else
                        {
                            OrderdEquipSkillPoint[sp.SBase] = sp.Point;
                        }

                    }

                }
            }



            bool finish;
            EquipmentDataTag[] EligibleSet = new EquipmentDataTag[(int)EquipKind.NumOfEquipKind];
            EquipTagTreeNode TmpEquipTagTreeNode = null;
            bool[] EquipSetFindFlag = new bool[(int)EquipKind.NumOfEquipKind];
            Dictionary<SkillBase, int> EquipAndSkillCuffPointDict = null;
            int MaxSkillNum = 0;

            //再帰検索用の配列を確保しておく。途中でnewすると非常に遅い
            SkillPoint[] tmpPlusJewelrySkillPoint = new SkillPoint[RestPoint.Length];
            for (int i = 0; i < tmpPlusJewelrySkillPoint.Length; i++)
            {
                tmpPlusJewelrySkillPoint[i] = new SkillPoint(RestPoint[i].SBase, 0);
            }

            int[][] tmpPlusJewelryUseCount = new int[PlusJewelyDataTags.Length][];
            for (int i = 0; i < PlusJewelyDataTags.Length; i++)
            {
                tmpPlusJewelryUseCount[i] = new int[PlusJewelyDataTags[i].Length];
            }

            do
            {
                do
                {
                    /*
#if DEBUG
                    int[] testSlotTypeArray = new int[] { 0, 0, 0, 0 };
                    int tmpSPnum = 0;
                    for (int i = 0; i < (int)EquipType.NumOfEquipKind; i++)
                    {
                        if (isOrderd[i])
                        {
                            Equipment equip = condition.ESet[(EquipType)i];

                            if (equip.EquipData.isSP)
                            {
                                testSlotTypeArray[0] += equip.GetRestSlotNum();
                                tmpSPnum++;
                            }
                            else
                            {
                                int rest = equip.GetRestSlotNum();

                                if (rest > 0)
                                    testSlotTypeArray[rest]++;
                            }

                        }
                        else
                        {

                            EquipmentData equip = PotentialEquipTagArrayArray[i][NowSearching[i]].equipdata;

                            if (equip.isSP)
                            {
                                testSlotTypeArray[0] += equip.NumOfSlot;
                                tmpSPnum++;
                            }
                            else
                            {
                                if (equip.NumOfSlot > 0)
                                    testSlotTypeArray[equip.NumOfSlot]++;
                            }
                        }
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        if (SlotTypeArray[i] != testSlotTypeArray[i])
                        {
                            throw new Exception("エラー SlotyTypeが違う可能性大");
                        }
                    }

                    if (numSP != tmpSPnum)
                    {
                        //throw new Exception("エラー SPNumが違う可能性大");
                    }

#endif*/


                    bool UpeerBlocked = false;
                    bool isEligibleSetFind = false;
                    EquipAndSkillCuffPointDict = null;

                    #region SP箇所数と空き防具数をクリアしたら
                    if (condition.BlankEquipNum.CheckNum(numBlankEquip) && condition.SP_assign.CheckNum(numSP) && (isIgnoreDef || SelectedEquipDef >= RestDef))
                    {
                        //タイプ数指定をチェック
                        if (condition.isOrderTypeNum)
                        {
                            foreach (string key in condition.TypeNumOrder.Keys)
                            {
                                if (!condition.TypeNumOrder[key].CheckNum(TypeUseCountTable[key]))
                                {
                                    goto LABEL_NEXT;
                                }
                            }
                        }


                        #region 装飾品関連初期化

                        for (int i = 0; i < SinglePlusJewelryUseCount.Length; i++)
                            SinglePlusJewelryUseCount[i] = 0;
                        //Array.Clear(SinglePlusJewelryUseCount, 0, SinglePlusJewelryUseCount.Length);

                        for (int i = 0; i < SPJewwlryUseCount.Length; i++)
                        {
                            SPJewwlryUseCount[i] = 0;
                        }

                       // Array.Clear(SPJewwlryUseCount, 0, SPJewwlryUseCount.Length);


                        foreach (SkillPoint sp in SPJewelrySkillPoint)
                        {
                            sp.Point = 0;
                        }

                        int MinimumUsingSlot = int.MaxValue;
                        int MinimumUsingSPSlot = int.MaxValue;
                        int MinimumUsingCuffSlot = int.MaxValue;
                        MaxSkillNum = 0;
                        #endregion

                        #region スロット数がn必要な装飾品が無い場合、n個の列を分裂させる

                        tmpSlotTypeNumArray3[0] = SlotTypeNumArray[1];
                        tmpSlotTypeNumArray3[1] = SlotTypeNumArray[2];
                        tmpSlotTypeNumArray3[2] = SlotTypeNumArray[3];

                        if (PlusJewelySlotCount[2] == 0 && SingleJewelrySlotCount[2] == 0&& tmpSlotTypeNumArray3[2] > 0)//スロット3の装飾品が無ければ分解
                        {
                            tmpSlotTypeNumArray3[1] += tmpSlotTypeNumArray3[2];
                            tmpSlotTypeNumArray3[0] += tmpSlotTypeNumArray3[2];
                            tmpSlotTypeNumArray3[2] = 0;
                        }

                        if (PlusJewelySlotCount[1] == 0 && SingleJewelrySlotCount[1] == 0 && tmpSlotTypeNumArray3[1] > 0)//スロット2の装飾品が無ければ分解
                        {
                            tmpSlotTypeNumArray3[0] += tmpSlotTypeNumArray3[1] * 2;
                            tmpSlotTypeNumArray3[1] = 0;
                        }

                        #endregion


                        SKillCuffSlotArray2[0] = SKillCuffSlotArray[0];
                        SKillCuffSlotArray2[1] = SKillCuffSlotArray[1];


                        do
                        {//スキルカフスロット2

                            #region スキルカフを総当り

                            int depth2 = 0;

                            do
                            {
                                for (int x = 1 - depth2; x >= 0; x--)
                                {
                                    #region スロットxの装飾品候補リストの最後を限界まで追加
                                    if (PotentialSkillCuffTagArrayArray[x].Length > 0)
                                    {
                                        SkillCuffUseCount[x][PotentialSkillCuffTagArrayArray[x].Length - 1] = SKillCuffSlotArray2[x];

                                        if (SKillCuffSlotArray2[x] > 0)
                                        {
                                            foreach (SkillPointTag spt in PotentialSkillCuffTagArrayArray[x][PotentialSkillCuffTagArrayArray[x].Length - 1].SkillPointTags)//装飾品の持っているスキルポイントの全ての中から
                                            {
                                                SPJewelrySkillPoint[spt.index].Point += spt.Point * SKillCuffSlotArray2[x];
                                            }
                                        }
                                    }
                                    #endregion
                                }

                                depth2 = 2;
                                do
                                {
                                    #region 装飾品を総あたりする

                                    #region SPの装飾品候補リストの最後を限界まで追加
                                    if (SPJewelryDataTags.Length > 0)
                                    {
                                        SPJewwlryUseCount[SPJewelryDataTags.Length - 1] = SlotTypeNumArray[0];
                                        if (SlotTypeNumArray[0] > 0)
                                        {
                                            foreach (SkillPointTag spt in SPJewelryDataTags[SPJewelryDataTags.Length - 1].SkillPointTags)//装飾品の持っているスキルポイントの全ての中から
                                            {
                                                SPJewelrySkillPoint[spt.index].Point += spt.Point * SlotTypeNumArray[0];
                                            }
                                        }
                                    }
                                    #endregion

                                    do
                                    {
                                        for (int i = 0; i < JewelrysSkillPoint.Length; i++)
                                        {
                                            JewelrysSkillPoint[i].Point = SPJewelrySkillPoint[i].Point;
                                        }

                                        tmpSlotTypeNumArray2[0] = tmpSlotTypeNumArray3[0];
                                        tmpSlotTypeNumArray2[1] = tmpSlotTypeNumArray3[1];
                                        tmpSlotTypeNumArray2[2] = tmpSlotTypeNumArray3[2];

                                        #region 特別な装飾品があればもうはめたことにする

                                        int NumOfUseSlot1_SpecificJewelry = 0;

                                        for (int j = 0; j < SinglePlusJewelryDataTags.Length; j++)
                                        {
                                            PlusJewelryDataTag pjdt = SinglePlusJewelryDataTags[j];

                                            int index = pjdt.SpecificPoint.index;
                                            int need = RestPoint[index].Point - JewelrysSkillPoint[index].Point;

                                            if (need <= 0)
                                            {
                                                SinglePlusJewelryUseCount[j] = 0;
                                                continue;//足りているスキルはスキップ
                                            }

                                            if (pjdt.jdt == null)
                                            {
                                                goto LABEL_NEXT_SP_JEWELRY;//装飾品でこのスキルを補うことは出来ない
                                            }

                                            int num = need / pjdt.SpecificPoint.Point;
                                            if (need % pjdt.SpecificPoint.Point > 0)
                                                num++;


                                            if (tmpSlotTypeNumArray2[pjdt.Slot - 1] >= num)//スロット２、３についてはここで必要スロットを確保してしまう
                                            {
                                                tmpSlotTypeNumArray2[pjdt.Slot - 1] -= num;
                                            }
                                            else
                                            {
                                                if (pjdt.Slot == 3)
                                                {
                                                    goto LABEL_NEXT_SP_JEWELRY;//スロット３が無いと無理
                                                }
                                                else if (pjdt.Slot == 2)
                                                {//スロット２の場合
                                                    int rest = num - tmpSlotTypeNumArray2[pjdt.Slot - 1];

                                                    if (tmpSlotTypeNumArray2[2] < rest)//分解するスロット３が足りなければアウト
                                                    {
                                                        goto LABEL_NEXT_SP_JEWELRY;
                                                    }
                                                    tmpSlotTypeNumArray2[2] -= rest;//不足分をスロット3を分解し、補う。
                                                    tmpSlotTypeNumArray2[0] += rest;//分解したあまりを足す

                                                    tmpSlotTypeNumArray2[1] = 0;
                                                }
                                                else
                                                {
                                                    if (tmpSlotTypeNumArray2[pjdt.Slot - 1] != 0)
                                                    {
                                                        NumOfUseSlot1_SpecificJewelry += num - tmpSlotTypeNumArray2[pjdt.Slot - 1];
                                                        tmpSlotTypeNumArray2[pjdt.Slot - 1] = 0;
                                                    }
                                                    else
                                                        NumOfUseSlot1_SpecificJewelry += num;//足りないスロット１の数を記録しておく
                                                }
                                            }


                                            SinglePlusJewelryUseCount[j] = num;

                                            foreach (SkillPointTag spt in pjdt.jdt.SkillPointTags)//JewelrysSkillPointにポイントを加算
                                            {
                                                JewelrysSkillPoint[spt.index].Point += spt.Point * num;
                                            }
                                        }

                                        if (NumOfUseSlot1_SpecificJewelry > tmpSlotTypeNumArray2[2] * 3 + tmpSlotTypeNumArray2[1] * 2 + tmpSlotTypeNumArray2[0])
                                        {//もうどうがんばっても足りない
                                            goto LABEL_NEXT_SP_JEWELRY;
                                        }

                                        #endregion

                                        #region 無駄なスロットを分解
                                        if (PlusJewelySlotCount[2] == 0 && tmpSlotTypeNumArray2[2] > 0)//スロット3の装飾品が無ければ分解
                                        {
                                            tmpSlotTypeNumArray2[1] += tmpSlotTypeNumArray2[2];
                                            tmpSlotTypeNumArray2[0] += tmpSlotTypeNumArray2[2];
                                            tmpSlotTypeNumArray2[2] = 0;
                                        }

                                        if (PlusJewelySlotCount[1] == 0 && tmpSlotTypeNumArray2[1] > 0)//スロット2の装飾品が無ければ分解
                                        {
                                            tmpSlotTypeNumArray2[0] += tmpSlotTypeNumArray2[1] * 2;
                                            tmpSlotTypeNumArray2[1] = 0;
                                        }
                                        #endregion

                                        tmpSlotTypeNumArray1[0] = tmpSlotTypeNumArray2[0];
                                        tmpSlotTypeNumArray1[1] = tmpSlotTypeNumArray2[1];
                                        tmpSlotTypeNumArray1[2] = tmpSlotTypeNumArray2[2];


                                        #region 通常の装飾品に対して総当たり
                                        do
                                        {//装飾品をはめるパターンを変える（スロット３について）

                                            #region スロット1の特別な装飾品の分を引く

                                            if (tmpSlotTypeNumArray1[0] < NumOfUseSlot1_SpecificJewelry)
                                            {
                                                int rest = NumOfUseSlot1_SpecificJewelry - tmpSlotTypeNumArray1[0];
                                                if (rest > tmpSlotTypeNumArray1[1] * 2)//今のスロット２を全て分解しても足りない場合
                                                {
                                                    goto LABEL_SKIP_SLOT2;
                                                }
                                                else
                                                {//足りるまで一気に分解
                                                    int num = rest / 2;
                                                    if (rest % 2 > 0)
                                                        num++;

                                                    tmpSlotTypeNumArray1[1] -= num;
                                                    tmpSlotTypeNumArray1[0] += num * 2 - NumOfUseSlot1_SpecificJewelry;
                                                }
                                            }
                                            else
                                                tmpSlotTypeNumArray1[0] -= NumOfUseSlot1_SpecificJewelry;

                                            #endregion


                                            do
                                            {//装飾品をはめるパターンを変える（スロット２について)


                                                for (int i = 0; i < RestPoint.Length; i++)
                                                {
                                                    tmpRestPoint[i].Point = RestPoint[i].Point - JewelrysSkillPoint[i].Point;
                                                }

                                                tmpSlotTypeNumArray[0] = tmpSlotTypeNumArray1[0];
                                                tmpSlotTypeNumArray[1] = tmpSlotTypeNumArray1[1];
                                                tmpSlotTypeNumArray[2] = tmpSlotTypeNumArray1[2];


                                                //※※※再帰検索※※※
                                                int[][] FindUseCount = FindEligibleJewelryUseCount(tmpSlotTypeNumArray, tmpRestPoint, MinimumUsingSlot, ref UpeerBlocked, tmpPlusJewelrySkillPoint, tmpPlusJewelryUseCount,tmpAddCount,tmpRestSlot,null,ref EquipAndSkillCuffPointDict);
                                                //※※※※※※※※※※
                                                

                                                if (FindUseCount != null)
                                                {
                                                    #region 見つかった組み合わせをチェック。

                                                    #region 不要な装飾品はカット
                                                    bool changed;

                                                    SkillPoint[] tmpJewelrySkillPoint = new SkillPoint[JewelrysSkillPoint.Length];

                                                    for (int i = 0; i < JewelrysSkillPoint.Length; i++)
                                                    {
                                                        tmpJewelrySkillPoint[i] = (SkillPoint)JewelrysSkillPoint[i].Clone();
                                                    }

                                                    int[][] tmpJewelryUseCount = new int[FindUseCount.Length][];

                                                    for (int i = 0; i < tmpJewelryUseCount.Length; i++)//JewelryUseCountを変更しちゃうと次の探索のが正しくできないからコピーする
                                                    {
                                                        tmpJewelryUseCount[i] = (int[])FindUseCount[i].Clone();

                                                        for (int j = 0; j < FindUseCount[i].Length; j++)
                                                        {
                                                            if (FindUseCount[i][j] > 0)
                                                            {
                                                                foreach (SkillPointTag spt in PlusJewelyDataTags[i][j].jdt.SkillPointTags)
                                                                {
                                                                    tmpJewelrySkillPoint[spt.index].Point += spt.Point * FindUseCount[i][j];
                                                                }
                                                            }
                                                        }

                                                    }

                                                    int[] tmpSpJewelryUseCount = (int[])SPJewwlryUseCount.Clone();

                                                    int[][] tmpSkillCuffUseCount = new int[2][];
                                                    for (int i = 0; i < 2; i++)
                                                    {
                                                        tmpSkillCuffUseCount[i] = (int[])SkillCuffUseCount[i].Clone();
                                                    }

                                                    do
                                                    {
                                                        changed = false;
                                                        #region 装飾品に関して
                                                        for (int i = 0; i < tmpJewelryUseCount.Length; i++)
                                                        {
                                                            JewelryDataTag[] Jewelrys = new JewelryDataTag[PlusJewelyDataTags[i].Length];

                                                            for (int j = 0; j < Jewelrys.Length; j++)
                                                                Jewelrys[j] = PlusJewelyDataTags[i][j].jdt;

                                                            if (CutUnUseJewelry(RestPoint, tmpJewelrySkillPoint, Jewelrys, tmpJewelryUseCount[i]))
                                                                changed = true;
                                                        }

                                                        if (CutUnUseJewelry(RestPoint, tmpJewelrySkillPoint, SPJewelryDataTags, tmpSpJewelryUseCount))
                                                            changed = true;

                                                        #endregion

                                                        #region スキルカフに関して

                                                        for (int i = 0; i < 2; i++)
                                                        {
                                                            for (int j = 0; j < PotentialSkillCuffTagArrayArray[i].Length; j++)
                                                            {
                                                                if (tmpSkillCuffUseCount[i][j] > 0)
                                                                {
                                                                    bool flag = true;
                                                                    int min = int.MaxValue;
                                                                    foreach (SkillPointTag spt in PotentialSkillCuffTagArrayArray[i][j].SkillPointTags)
                                                                    {
                                                                        if (spt.Point < 0)
                                                                        {
                                                                            if (RequirementSkillPointsSortedList.Values[spt.index].UpperPoint != null)
                                                                            {
                                                                                int dif = Math.Abs((int)RequirementSkillPointsSortedList.Values[spt.index].UpperPoint - RequirementSkillPointsSortedList.Values[spt.index].Point)
                                                                                - tmpJewelrySkillPoint[spt.index].Point + RestPoint[spt.index].Point;


                                                                                if (dif < -spt.Point)
                                                                                {
                                                                                    flag = false;
                                                                                    break;
                                                                                }
                                                                                else
                                                                                {
                                                                                    if (min > dif / -spt.Point)
                                                                                        min = dif / -spt.Point;
                                                                                }
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            int x = tmpJewelrySkillPoint[spt.index].Point - RestPoint[spt.index].Point;
                                                                            if (x < spt.Point)
                                                                            {
                                                                                flag = false;
                                                                                break;
                                                                            }
                                                                            else
                                                                            {
                                                                                if (min > x / spt.Point)
                                                                                    min = x / spt.Point;
                                                                            }
                                                                        }
                                                                    }

                                                                    if (flag == true)
                                                                    {
                                                                        int cutnum;
                                                                        if (tmpSkillCuffUseCount[i][j] < min)
                                                                        {
                                                                            cutnum = tmpSkillCuffUseCount[i][j];
                                                                        }
                                                                        else
                                                                        {
                                                                            cutnum = min;
                                                                        }

                                                                        tmpSkillCuffUseCount[i][j] -= cutnum;

                                                                        foreach (SkillPointTag spt in PotentialSkillCuffTagArrayArray[i][j].SkillPointTags)
                                                                        {
                                                                            tmpJewelrySkillPoint[spt.index].Point -= spt.Point * cutnum;
                                                                        }
                                                                        changed = true;
                                                                    }
                                                                }
                                                            }
                                                        }


                                                        #endregion
                                                    } while (changed);

                                                    #endregion


                                                    EquipAndSkillCuffPointDict = MakeEquipAndSkillCuffPointDict(EquipDataTags, EquipAndSkillCuffPointDict, tmpSkillCuffUseCount);


                                                    //実際に発動するスキルから指定されたスキルを満たす数をカウント

                                                    int UseSlotsNum = 0;
                                                    int UseSPSlotsNum = 0;
                                                    int UseCuffSlotsNum = 0;
                                                    Dictionary<SkillBase, int> tmpSkillSum = new Dictionary<SkillBase, int>(EquipAndSkillCuffPointDict);

                                                    #region 使用するスロットの数を計算
                                                    for (int i = 0; i < tmpJewelryUseCount.Length; i++)
                                                    {
                                                        for (int j = 0; j < tmpJewelryUseCount[i].Length; j++)
                                                        {
                                                            UseSlotsNum += PlusJewelyDataTags[i][j].jdt.jd.Slot * tmpJewelryUseCount[i][j];

                                                            foreach (SkillPoint pt in PlusJewelyDataTags[i][j].jdt.jd.SkillList)
                                                            {
                                                                if (tmpSkillSum.ContainsKey(pt.SBase))
                                                                {
                                                                    tmpSkillSum[pt.SBase] += pt.Point * tmpJewelryUseCount[i][j];
                                                                }
                                                                else
                                                                {
                                                                    tmpSkillSum[pt.SBase] = pt.Point * tmpJewelryUseCount[i][j];
                                                                }
                                                            }
                                                        }

                                                    }

                                                    //スキルカフスロット数を算出
                                                    for (int i = 0; i < 2; i++)
                                                    {
                                                        int n = tmpSkillCuffUseCount[i].Length;
                                                        for (int j = 0; j < n; j++)
                                                        {
                                                            UseCuffSlotsNum += tmpSkillCuffUseCount[i][j] * PotentialSkillCuffTagArrayArray[i][j].jd.Slot;
                                                        }
                                                    }


                                                    //SP装飾品スロット数算出
                                                    for (int i = 0; i < tmpSpJewelryUseCount.Length; i++)
                                                    {
                                                        UseSPSlotsNum += tmpSpJewelryUseCount[i] * SPJewelryDataTags[i].jd.Slot;
                                                    }



                                                    for (int i = 0; i < SinglePlusJewelryUseCount.Length; i++)
                                                    {
                                                        if(SinglePlusJewelryDataTags[i].jdt == null)
                                                            continue;

                                                        if (SinglePlusJewelryUseCount[i] > 0)
                                                            UseSlotsNum += SinglePlusJewelryDataTags[i].Slot * SinglePlusJewelryUseCount[i];


                                                        foreach (SkillPoint pt in SinglePlusJewelryDataTags[i].jdt.jd.SkillList)
                                                        {
                                                            if (tmpSkillSum.ContainsKey(pt.SBase))
                                                            {
                                                                tmpSkillSum[pt.SBase] += pt.Point * SinglePlusJewelryUseCount[i];
                                                            }
                                                            else
                                                            {
                                                                tmpSkillSum[pt.SBase] = pt.Point * SinglePlusJewelryUseCount[i];
                                                            }
                                                        }
                                                    }


                                                    #endregion


                                                    #region 発動する順番に並べて、10までで有用なスキルをカウント
                                                    List<SkillBase> list = new List<SkillBase>();
                                                    foreach (KeyValuePair<SkillBase, int> pair in tmpSkillSum)
                                                    {
                                                        SkillOption so = pair.Key.GetOption(pair.Value);
                                                        if (null != so)
                                                        {
                                                            list.Add(pair.Key);
                                                        }
                                                    }

                                                    int SkillCount = 0;
                                                    if (list.Count > 10)
                                                    {
                                                        int SkillUpper = 10;

                                                        # region G級防具の数をカウントして発動スキル上限を算出

                                                        int GrankEquipNum = numGRankEquipNum_Orderd;

                                                        for (int i = 0; i < (int)EquipKind.NumOfEquipKind; i++)
                                                        {
                                                            if (isOrderd[i])
                                                                continue;


                                                            foreach (String grank in BaseData.mDefine.GRankEquipType)
                                                            {
                                                                if (EquipDataTags[i].equipdata.Type.Equals(grank))
                                                                {
                                                                    GrankEquipNum++;
                                                                    break;
                                                                }
                                                            }



                                                        }

                                                        switch (GrankEquipNum)
                                                        {
                                                            case 3:
                                                            case 4:
                                                                SkillUpper = 11;
                                                                break;
                                                            case 5:
                                                                SkillUpper = 12;
                                                                break;
                                                        }

                                                     

                                                        #endregion

                                                        list.Sort();

                                                        for (int i = 0; i < list.Count && i < SkillUpper; i++)
                                                        {
                                                            SkillBase sb = list[i];
                                                            foreach (SkillPointCondition spc in condition.SkillPointConditionTable.Values)
                                                            {
                                                                if (!spc.isIgnore && sb == spc.SBase)
                                                                {
                                                                    SkillCount++;
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        SkillCount = OrderdSkillNum;
                                                    }

                                                    #endregion

                                                    bool switchflag = false;

                                                    if (MaxSkillNum < SkillCount)
                                                    {
                                                        switchflag = true;
                                                    }
                                                    else if (MaxSkillNum == SkillCount)
                                                    {
                                                        if (MinimumUsingSlot > UseSlotsNum)
                                                        {
                                                            switchflag = true;
                                                        }
                                                        else if (MinimumUsingSlot == UseSlotsNum)
                                                        {
                                                            if (MinimumUsingSPSlot > UseSPSlotsNum)
                                                            {
                                                                switchflag = true;
                                                            }
                                                            else if (MinimumUsingSPSlot == UseSPSlotsNum)
                                                            {
                                                                if (MinimumUsingCuffSlot > UseCuffSlotsNum)
                                                                {
                                                                    switchflag = true;
                                                                }

                                                            }
                                                        }
                                                    }
                                                   

                                                    if (switchflag)
                                                    {
                                                        MinimumUsingSlot = UseSlotsNum;
                                                        MinimumUsingSPSlot = UseSPSlotsNum;
                                                        MinimumUsingCuffSlot = UseCuffSlotsNum;
                                                        MaxSkillNum = SkillCount;

                                                        for (int type = (int)EquipKind.Weapon; type < (int)EquipKind.NumOfEquipKind; type++)
                                                        {
                                                            if (!isOrderd[(int)type])
                                                            {
                                                                EligibleSet[type] = EquipDataTags[type];
                                                            }
                                                        }

                                                        EligibleJewelryUseCount = tmpJewelryUseCount;
                                                        EligibleSPJewelryUseCount = tmpSpJewelryUseCount;
                                                        EligibleSkillCuffUseCount = tmpSkillCuffUseCount;
                                                        EligibleSingleJewelryUseCount = (int[])SinglePlusJewelryUseCount.Clone();
                                                        isEligibleSetFind = true;

                                                    }

                                                    #endregion
                                                }


                                                #region 装飾品をはめるパターンを変える（スロット２について）
                                                if (tmpSlotTypeNumArray1[1] > 0)
                                                {
                                                    tmpSlotTypeNumArray1[1]--;
                                                    tmpSlotTypeNumArray1[0] += 2;
                                                }
                                                else
                                                    break;
                                                #endregion
                                            } while (true);

                                        LABEL_SKIP_SLOT2:

                                            #region 装飾品をはめるパターンを変える（スロット３について）
                                            if (tmpSlotTypeNumArray2[2] > 0)
                                            {
                                                tmpSlotTypeNumArray2[2]--;
                                                tmpSlotTypeNumArray2[1]++;
                                                tmpSlotTypeNumArray2[0]++;

                                                tmpSlotTypeNumArray1[0] = tmpSlotTypeNumArray2[0];
                                                tmpSlotTypeNumArray1[1] = tmpSlotTypeNumArray2[1];
                                                tmpSlotTypeNumArray1[2] = tmpSlotTypeNumArray2[2];
                                            }
                                            else
                                                break;
                                            #endregion
                                        } while (true);
                                        #endregion



                                    LABEL_NEXT_SP_JEWELRY:

                                        #region SP珠総当り

                                        finish = true;
                                        if (SlotTypeNumArray[0] > 0)
                                        {
                                            if (SPJewelryDataTags.Length == 1)
                                            {
                                                if (SPJewwlryUseCount[0] > 0)
                                                {
                                                    foreach (SkillPointTag spt in SPJewelryDataTags[0].SkillPointTags)//装飾品の持っているスキルポイントの全ての中から
                                                    {
                                                        SPJewelrySkillPoint[spt.index].Point -= spt.Point * SPJewwlryUseCount[0];

                                                    }
                                                    SPJewwlryUseCount[0] = 0;
                                                }
                                            }
                                            else
                                            {
                                                int sum = 0;
                                                for (int a = 1; a < SPJewelryDataTags.Length; a++)
                                                {
                                                    if (SPJewwlryUseCount[a] > 0)
                                                    {
                                                        foreach (SkillPointTag spt in SPJewelryDataTags[a].SkillPointTags)//装飾品の持っているスキルポイントの全ての中から
                                                        {
                                                            SPJewelrySkillPoint[spt.index].Point -= spt.Point;

                                                        }
                                                        SPJewwlryUseCount[a]--;
                                                        SPJewwlryUseCount[a - 1] += sum + 1;

                                                        foreach (SkillPointTag spt in SPJewelryDataTags[a - 1].SkillPointTags)//装飾品の持っているスキルポイントの全ての中から
                                                        {
                                                            SPJewelrySkillPoint[spt.index].Point += spt.Point * (sum + 1);

                                                        }
                                                        finish = false;
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        if (a == 1 && SPJewwlryUseCount[0] > 0)
                                                        {
                                                            foreach (SkillPointTag spt in SPJewelryDataTags[0].SkillPointTags)//装飾品の持っているスキルポイントの全ての中から
                                                            {
                                                                SPJewelrySkillPoint[spt.index].Point -= spt.Point * SPJewwlryUseCount[0];

                                                            }

                                                            sum += SPJewwlryUseCount[0];
                                                            SPJewwlryUseCount[0] = 0;
                                                        }
                                                    }

                                                }
                                            }
                                        }
                                        #endregion

                                    } while (!finish);


                                    #endregion

                                    #region スロットxのスキルカフを総当たり
                                    //finish = true;
                                    for (int x; depth2 > 0; depth2--)
                                    {
                                        x = 2 - depth2;
                                        if (SKillCuffSlotArray2[x] > 0)
                                        {
                                            if (PotentialSkillCuffTagArrayArray[x].Length == 1)
                                            {//一個の場合は装飾品を外すだけ
                                                if (SkillCuffUseCount[x][0] > 0)
                                                {
                                                    foreach (SkillPointTag spt in PotentialSkillCuffTagArrayArray[x][0].SkillPointTags)//装飾品の持っているスキルポイントの全ての中から
                                                    {
                                                        SPJewelrySkillPoint[spt.index].Point -= spt.Point * SkillCuffUseCount[x][0];

                                                    }
                                                    SkillCuffUseCount[x][0] = 0;
                                                }
                                            }
                                            else
                                            {
                                                int sum2 = 0;
                                                for (int a = 1; a < PotentialSkillCuffTagArrayArray[x].Length; a++)
                                                {
                                                    if (SkillCuffUseCount[x][a] > 0)
                                                    {
                                                        foreach (SkillPointTag spt in PotentialSkillCuffTagArrayArray[x][a].SkillPointTags)//装飾品の持っているスキルポイントの全ての中から
                                                        {
                                                            SPJewelrySkillPoint[spt.index].Point -= spt.Point;

                                                        }
                                                        SkillCuffUseCount[x][a]--;
                                                        SkillCuffUseCount[x][a - 1] += sum2 + 1;

                                                        foreach (SkillPointTag spt in PotentialSkillCuffTagArrayArray[x][a - 1].SkillPointTags)//装飾品の持っているスキルポイントの全ての中から
                                                        {
                                                            SPJewelrySkillPoint[spt.index].Point += spt.Point * (sum2 + 1);
                                                        }

                                                        //finish = false;
                                                        goto LABEL_BREAK2;
                                                    }
                                                    else if (a == 1 && SkillCuffUseCount[x][0] > 0)
                                                    {
                                                        foreach (SkillPointTag spt in PotentialSkillCuffTagArrayArray[x][0].SkillPointTags)//装飾品の持っているスキルポイントの全ての中から
                                                        {
                                                            SPJewelrySkillPoint[spt.index].Point -= spt.Point * SkillCuffUseCount[x][0];

                                                        }

                                                        sum2 += SkillCuffUseCount[x][0];
                                                        SkillCuffUseCount[x][0] = 0;
                                                    }
                                                }



                                            }
                                        }
                                    }

                                LABEL_BREAK2: ;
                                    #endregion

                                } while (depth2 == 2);


                            } while (depth2 > 0);


                            #region スキルカフスロットのパターンを変える（スロット2について）
                            if (SKillCuffSlotArray2[1] > 0)//スロット2があったら
                            {
                                SKillCuffSlotArray2[1]--;//分解
                                SKillCuffSlotArray2[0] += 2;
                            }
                            else
                                break;
                            #endregion

                            #endregion

                        } while (true);


                        if (isEligibleSetFind)
                        {
                            #region 防御力指定をクリアしていたら追加
                            if (SelectedEquipDef >= RestDef && SelectedEquipDef - RestDef <= condition.defence_upper - condition.defence_lower)
                            {
                                EquipSet set = new EquipSet();
                                for (int i = 0; i < (int)EquipKind.NumOfEquipKind; i++)
                                {
                                    if (isOrderd[i])
                                    {
                                        Equipment equip = condition.ESet[(EquipKind)i];
                                        set[(EquipKind)i].equipdata_value = equip.EquipData;//valueじゃないほうに代入すると無駄なコピーが発生するため
                                        set[(EquipKind)i].Def = equip.Def;
                                        set[(EquipKind)i].Slot = equip.Slot;
                                        set[(EquipKind)i].Level = equip.Level;
                                        set[(EquipKind)i].isChecked = true;

                                        List<JewelryData_base> jdlist = equip.GetFixedJewelys();

                                        foreach (JewelryData_base jd in jdlist)
                                        {
                                            set[(EquipKind)i].SetJewelry(jd);
                                        }


                                    }
                                    else
                                    {
                                        if (i == (int)EquipKind.Weapon)
                                        {
                                            if (EligibleSet[i].equipdata.Name == "武器スロットなし")
                                            {
                                                set[EquipKind.Weapon].EquipData = null;
                                            }
                                        }
                                        else
                                            set[(EquipKind)i].EquipData = EligibleSet[i].equipdata;

                                    }
                                }

                                if (condition.ESet.PigClothes != null && condition.ESet.PigClothes.isChecked)
                                {
                                    set.PigClothes = new PigClothes();

                                    set.PigClothes.isChecked = true;
                                    set.PigClothes.Clothes = condition.ESet.PigClothes.Clothes;
                                    set.PigClothes.SkillCuffs = (SkillCuffData[])condition.ESet.PigClothes.SkillCuffs.Clone();
                                }



                                #region 実際に装飾品をセット
                                for (int SlotSize = 3; SlotSize > 0; SlotSize--)//大きいものから入れないとダメ
                                {
                                    for (int i = 0; i < PlusJewelyDataTags.Length; i++)
                                    {
                                        //通常の装飾品をはめる
                                        for (int j = 0; j < PlusJewelyDataTags[i].Length; j++)
                                        {
                                            PlusJewelryDataTag pjdt = PlusJewelyDataTags[i][j];
                                            if (pjdt.jdt.jd.Slot != SlotSize)
                                                continue;

                                            if (EligibleJewelryUseCount[i][j] > 0)
                                            {
                                                int count = EligibleJewelryUseCount[i][j];
                                                SetJewelryToEquipSet(set, pjdt.jdt.jd, count);
                                            }
                                        }
                                    }

                                    for (int i = 0; i < SinglePlusJewelryDataTags.Length; i++)
                                    {
                                        if (SinglePlusJewelryDataTags[i].jdt == null)
                                            continue;

                                        JewelryData_base jd = SinglePlusJewelryDataTags[i].jdt.jd;
                                        if (jd.Slot != SlotSize)
                                            continue;

                                        if (EligibleSingleJewelryUseCount[i] > 0)
                                        {
                                            int count = EligibleSingleJewelryUseCount[i];
                                            SetJewelryToEquipSet(set, jd, count);
                                        }
                                    }

                                }


                                //通常の装飾品をはめる
                                for (int i = 0; i < SPJewelryDataTags.Length; i++)
                                {
                                    if (EligibleSPJewelryUseCount[i] > 0)
                                    {
                                        int count = EligibleSPJewelryUseCount[i];

                                        while (count > 0)
                                        {
                                            foreach (Equipment e in set.Equips)
                                            {
                                                if (e.EquipData == null)
                                                    continue;

                                                if (e.EquipData.isSP)
                                                {
                                                    if (e.GetRestSlotNum() >= SPJewelryDataTags[i].jd.Slot)
                                                    {
                                                        e.SetJewelry(SPJewelryDataTags[i].jd);
                                                        count--;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                for (int a = 1; a >= 0; a--)
                                {
                                    for (int b = 0; b < EligibleSkillCuffUseCount[a].Length; b++)
                                    {
                                        if (EligibleSkillCuffUseCount[a][b] > 0)
                                        {
                                            int count = EligibleSkillCuffUseCount[a][b];

                                            while (count > 0)//無限ループに陥る
                                            {
                                                if (set.PigClothes.GetRestSlotNum() >= PotentialSkillCuffTagArrayArray[a][b].jd.Slot)
                                                {
                                                    set.PigClothes.SetJewelry(PotentialSkillCuffTagArrayArray[a][b].jd);
                                                    count--;
                                                }
                                                else
                                                {
                                                    throw new Exception("想定外のエラー スキルカフ");
                                                }
                                            }
                                        }
                                    }
                                }


                                #endregion

                                lock (parent.AddEquipSetList)
                                {
                                    parent.AddEquipSetList.Add(set);
                                }//add処理

                            }

                            #endregion

                        }

                        //下限を満たす組み合わせが発見されていれば追加検索をかける
                        if (isEligibleSetFind || UpeerBlocked)
                        {
                            if (!isAdditionalSearching && !parent.setting.OptimizeEquip)
                            {
                                isAdditionalSearching = true;
                                for (int i = 1; i < (int)EquipKind.NumOfEquipKind; i++)
                                {
                                    if (isOrderd[i] == true)
                                        continue;

                                    EquipTagTreeNode Root = new EquipTagTreeNode(EquipDataTags[i]);

                                    EquipSetKey Key = new EquipSetKey(EquipDataTags, i);

                                    AdditionalSearchTreeDict[i].Add(Key, Root);

                                    NowSearchTrees[i] = Root;
                                }
                                //PrintTagTreeAll(NowSearchTrees);

                            }
                        }

                    }


                    #endregion



                ////////////////////
                //チェック終わり
                ////////////////////

                LABEL_NEXT:
                    #region 次の装備へ移行する
                    finish = true;


                    #region 追加案件があれば
                    if (isAdditionalSearching)
                    {
                        if (isEligibleSetFind || UpeerBlocked)
                        {
                            for (int i = 0; i < EquipSetFindFlag.Length; i++)
                            {
                                EquipSetFindFlag[i] = true;
                            }
                        }


                        for (int i = (int)EquipKind.NumOfEquipKind - 1; i > 0 && finish == true; i--)
                        {

                            if (isOrderd[i] == true)
                                continue;



                            TmpEquipTagTreeNode = null;

                            if (NowSearchTrees[i].Child != null && NowSearchTrees[i].Tag.equipdata.isSP != true)
                            {
                                if (EquipSetFindFlag[i])//条件をみたしている場合、下位装備に潜る
                                {//深く潜る
                                    TmpEquipTagTreeNode = NowSearchTrees[i].Child;
                                    finish = false;
                                }
                                else
                                {
                                    //NowSearchTrees[i].Child = null;//子を切断する。
                                }

                            }


                            if (NowSearchTrees[i].Next != null)//Nextがあったということは絶対にParentはある
                            {
                                if (EquipSetFindFlag[i] == false)
                                {//自分を線形リストから除外する
                                    NowSearchTrees[i].Next.Prev = NowSearchTrees[i].Prev;//nullだったらそのままnullになる

                                    if (NowSearchTrees[i].Prev != null)
                                    {//自分は真ん中だった場合
                                        NowSearchTrees[i].Prev.Next = NowSearchTrees[i].Next;
                                    }
                                    else
                                    {//自分が先頭だった場合
                                        NowSearchTrees[i].Parent.Child = NowSearchTrees[i].Next;
                                    }
                                }

                                if (finish == true)
                                {
                                    TmpEquipTagTreeNode = NowSearchTrees[i].Next;
                                    finish = false;
                                }
                            }
                            else
                            {
                                if (EquipSetFindFlag[i] == false)
                                {//自らを線形リストから除外する
                                    if (NowSearchTrees[i].Prev != null)
                                    {//同列の最後だった
                                        NowSearchTrees[i].Prev.Next = null;
                                    }
                                    else
                                    {//唯一の子だった
                                        if (NowSearchTrees[i].Parent != null)
                                        {//親から自分を削除

#if DEBUG
                                            if (NowSearchTrees[i].Parent.Child != NowSearchTrees[i])
                                            {
                                                throw new Exception("ありえない");
                                            }
#endif

                                            NowSearchTrees[i].Parent.Child = null;

                                        }
                                        else
                                        {
                                            //自身はRootしかありえない

                                            EquipSetKey key = new EquipSetKey(EquipDataTags, i);

                                            AdditionalSearchTreeDict[i].Remove(key);

                                            //AdditionalSearchTreeDict[i][key] = null;//絶対にDict中にあるはず

                                        }
                                    }
                                }
                            }


                            if (finish == true)
                            {
                                if (NowSearchTrees[i].Parent != null)
                                {
                                    EquipTagTreeNode tmp = NowSearchTrees[i];

                                    do
                                    {
                                        if (tmp.Parent.Next != null)
                                        {
                                            TmpEquipTagTreeNode = tmp.Parent.Next;
                                            finish = false;
                                            break;
                                        }
                                        else
                                            tmp = tmp.Parent;

                                    } while (tmp.Parent != null);//最上位にくるまでループ

                                }
                            }

                            if (finish == false)
                            {//今回で決まったと思われる場合

                                NowSearchTrees[i] = TmpEquipTagTreeNode;//今回決まった奴を反映
                                EquipSetFindFlag[i] = false;

                                for (int j = i; j < (int)EquipKind.NumOfEquipKind - 1; j++)
                                {
                                    if (isOrderd[j + 1] == true)
                                    {
                                        continue;
                                    }

                                    EquipmentDataTag[] ChildTree = (EquipmentDataTag[])EquipDataTags.Clone();

                                    ChildTree[i] = NowSearchTrees[i].Tag;

                                    for (int k = i + 1; k <= j; k++)
                                    {
                                        if (isOrderd[k] != true)
                                            ChildTree[k] = NowSearchTrees[k].Tag;
                                    }

                                    EquipSetKey ChildKey = new EquipSetKey(ChildTree, j + 1);

                                    EquipSetKey ZyouiTree = (EquipSetKey)ChildKey.Clone();


                                    ZyouiTree.Set[i] = NowSearchTrees[i].Parent.Tag;//今回決まった装備の一個親とする


                                    //上位のツリーをコピーする
                                    EquipTagTreeNode NewTree = new EquipTagTreeNode(AdditionalSearchTreeDict[j + 1][ZyouiTree]);


                                    AdditionalSearchTreeDict[j + 1].Add(ChildKey, NewTree);
                                    NowSearchTrees[j + 1] = NewTree;
                                    EquipSetFindFlag[j + 1] = false;


                                }

                                //PrintTagTreeAll(NowSearchTrees);


                                for (int j = i; j < (int)EquipKind.NumOfEquipKind; j++)
                                {
                                    if (isOrderd[j] == true)
                                        continue;

                                    if (EquipDataTags[j] != NowSearchTrees[j].Tag)
                                    {
                                        #region 今の奴を減算
                                        SelectedEquipDef -= EquipDataTags[j].Def;
                                        foreach (SkillPointTag spt in EquipDataTags[j].SkillPointTags)//取り除く装備のスキルポイントをRestPointへ足す
                                        {
                                            RestPoint[spt.index].Point += spt.Point;
                                        }

                                        if (EquipDataTags[j].equipdata.Name == Properties.Resources.BLANK_EQUIP_NAME)
                                        {
                                            numBlankEquip--;
                                        }

                                        if (condition.isOrderTypeNum && TypeUseCountTable.ContainsKey(EquipDataTags[j].equipdata.Type))
                                        {
                                            TypeUseCountTable[EquipDataTags[j].equipdata.Type]--;
                                        }

                                        if (EquipDataTags[j].equipdata.isSP)
                                        {
                                            numSP--;
                                            SlotTypeNumArray[0]--;
                                        }
                                        else
                                        {
                                            if (EquipDataTags[j].Slot > 0)
                                                SlotTypeNumArray[EquipDataTags[j].Slot]--;
                                        }



                                        #endregion

                                        EquipDataTags[j] = NowSearchTrees[j].Tag;

                                        #region 新しい奴を加算
                                        if (EquipDataTags[j].equipdata.Name == Properties.Resources.BLANK_EQUIP_NAME)
                                        {
                                            numBlankEquip++;
                                        }

                                        if (EquipDataTags[j].equipdata.isSP)
                                        {
                                            numSP++;
                                            SlotTypeNumArray[0]++;
                                        }
                                        else
                                        {
                                            if (EquipDataTags[j].Slot > 0)
                                                SlotTypeNumArray[EquipDataTags[j].Slot]++;
                                        }

                                        if (condition.isOrderTypeNum && TypeUseCountTable.ContainsKey(EquipDataTags[j].equipdata.Type))
                                        {
                                            TypeUseCountTable[EquipDataTags[j].equipdata.Type]++;
                                        }



                                        SelectedEquipDef += EquipDataTags[j].Def;
                                        foreach (SkillPointTag spt in EquipDataTags[j].SkillPointTags)//追加した装備のスキルポイントをRestからひく
                                        {
                                            RestPoint[spt.index].Point -= spt.Point;
                                        }
                                        #endregion
                                    }
                                }

                            }


                        }
                    }
                    #endregion


                    if (finish == true)
                    {
                        if (isAdditionalSearching)
                        {
                            isAdditionalSearching = false;

                            AdditionalSearchTreeDict[1].Clear();
                            AdditionalSearchTreeDict[2].Clear();
                            AdditionalSearchTreeDict[3].Clear();
                            AdditionalSearchTreeDict[4].Clear();
                            AdditionalSearchTreeDict[5].Clear();


                            #region ぐちゃぐちゃになったEquipDataTagsをもとに戻す


                            SlotTypeNumArray[0] = 0;
                            SlotTypeNumArray[1] = 0;
                            SlotTypeNumArray[2] = 0;
                            SlotTypeNumArray[3] = 0;

                            SelectedEquipDef = 0;
                            numSP = 0;
                            numBlankEquip = 0;
                            numGRankEquipNum_Orderd = 0;
                             

                            TypeUseCountTable.Clear();
                            foreach (string str in condition.TypeNumOrder.Keys)
                            {
                                TypeUseCountTable.Add(str, 0);
                            }

                            //装備セットを更新
                            for (int i = 0; i < (int)EquipKind.NumOfEquipKind; i++)
                            {
                                if (isOrderd[i] == true)
                                {
                                    Equipment data = condition.ESet[(EquipKind)i];

                                    if (data.EquipData.Name == Properties.Resources.BLANK_EQUIP_NAME)
                                    {
                                        numBlankEquip++;
                                    }

                                    foreach (String grank in BaseData.mDefine.GRankEquipType)
                                    {
                                        if (data.EquipData.Type.Equals(grank))
                                        {
                                            numGRankEquipNum_Orderd++;
                                            break;
                                        }
                                    }


                                    if (data.EquipData.isSP)
                                    {
                                        numSP++;
                                        SlotTypeNumArray[0] += data.GetRestSlotNum();
                                    }
                                    else
                                    {
                                        int rest = data.GetRestSlotNum();

                                        if (rest > 0)
                                        {
                                            SlotTypeNumArray[rest]++;
                                        }
                                    }

                                    if (condition.isOrderTypeNum && TypeUseCountTable.ContainsKey(data.EquipData.Type))
                                    {
                                        TypeUseCountTable[data.EquipData.Type]++;
                                    }



                                    continue;
                                }

                                foreach (SkillPointTag spt in EquipDataTags[i].SkillPointTags)
                                {
                                    RestPoint[spt.index].Point += spt.Point;
                                }


                                EquipDataTags[i] = PotentialEquipTagArrayArray[i][NowSearching[i]];//切り替え

                                foreach (SkillPointTag spt in EquipDataTags[i].SkillPointTags)
                                {
                                    RestPoint[spt.index].Point -= spt.Point;
                                }

                                SelectedEquipDef += EquipDataTags[i].Def;


                                if (EquipDataTags[i].equipdata.Name == Properties.Resources.BLANK_EQUIP_NAME)
                                {
                                    numBlankEquip++;
                                }

                                if (EquipDataTags[i].equipdata.isSP)
                                {
                                    numSP++;
                                    SlotTypeNumArray[0]++;
                                }
                                else if (EquipDataTags[i].Slot > 0)
                                {
                                    SlotTypeNumArray[EquipDataTags[i].Slot]++;
                                }

                                if (condition.isOrderTypeNum && TypeUseCountTable.ContainsKey(EquipDataTags[i].equipdata.Type))
                                {
                                    TypeUseCountTable[EquipDataTags[i].equipdata.Type]++;
                                }


                            }
                            #endregion


                        }


                        CheckedNum++;



                        for (int i = (int)EquipKind.NumOfEquipKind - 1; i >= 0 && finish == true; i--)
                        {
                            if (isOrderd[i] == true)
                                continue;

                            SelectedEquipDef -= EquipDataTags[i].Def;
                            foreach (SkillPointTag spt in EquipDataTags[i].SkillPointTags)//取り除く装備のスキルポイントをRestPointへ足す
                            {
                                RestPoint[spt.index].Point += spt.Point;
                            }

                            if (EquipDataTags[i].equipdata.Name == Properties.Resources.BLANK_EQUIP_NAME)
                            {
                                numBlankEquip--;
                            }

                            if (EquipDataTags[i].equipdata.isSP)
                            {
                                numSP--;
                                SlotTypeNumArray[0]--;
                            }
                            else
                            {
                                if (EquipDataTags[i].Slot > 0)
                                    SlotTypeNumArray[EquipDataTags[i].Slot]--;
                            }

                            if (condition.isOrderTypeNum && TypeUseCountTable.ContainsKey(EquipDataTags[i].equipdata.Type))
                            {
                                TypeUseCountTable[EquipDataTags[i].equipdata.Type]--;
                            }


                            if (NowSearching[i] + 1 < SearchEndPoint[i])//次の装備へ切り替える
                            {
                                NowSearching[i]++;
                                finish = false;
                                EquipDataTags[i] = PotentialEquipTagArrayArray[i][NowSearching[i]];

                            }
                            else
                            {

                                NowSearching[i] = SearchBeginPoint[i];
                                EquipDataTags[i] = PotentialEquipTagArrayArray[i][NowSearching[i]];
                            }

                            if (EquipDataTags[i].equipdata.Name == Properties.Resources.BLANK_EQUIP_NAME)
                            {
                                numBlankEquip++;
                            }

                            if (EquipDataTags[i].equipdata.isSP)
                            {
                                numSP++;
                                SlotTypeNumArray[0]++;
                            }
                            else
                            {
                                if (EquipDataTags[i].Slot > 0)
                                    SlotTypeNumArray[EquipDataTags[i].Slot]++;
                            }

                            if (condition.isOrderTypeNum && TypeUseCountTable.ContainsKey(EquipDataTags[i].equipdata.Type))
                            {
                                TypeUseCountTable[EquipDataTags[i].equipdata.Type]++;
                            }

                            SelectedEquipDef += EquipDataTags[i].Def;
                            foreach (SkillPointTag spt in EquipDataTags[i].SkillPointTags)//追加した装備のスキルポイントをRestからひく
                            {
                                RestPoint[spt.index].Point -= spt.Point;
                            }
                        }
                    }

                    #endregion

                    #region データ更新の時間になっていたら
                    if (isUpdatetime)
                    {
                        isUpdatetime = false;
                        lock (parent.LockNumOfCheckedPriorityEquipSet)
                        {
                            parent.NumOfCheckedPriorityEquipSet += CheckedNum;
                        }

                        //CheckCount_amount += CheckedNum;


                        CheckedNum = 0;

                        while (bStop)
                        {
                            Thread.Sleep(1000);
                        }

                        #region 検索領域の再配布
                        lock (WaitingMeList)
                        {
                            if (WaitingMeList.Count > 0)//自分を待っている人がいたら
                            {
                                #region 領域再配布


#if DEBUG
                                int ThisRest = CulcRestSearch(this);

                                int[] SearchBeginPoint_back = (int[])SearchBeginPoint.Clone();
                                int[] NowSearching_back = (int[])NowSearching.Clone();
                                int[] SearchEndPoint_back = (int[])SearchEndPoint.Clone();
#endif


                                int DivideCount = WaitingMeList.Count + 1;//待っている人＋自分


                                bool find = false;
                                for (int equiptype = 0; equiptype < (int)EquipKind.NumOfEquipKind; equiptype++)//分割は頭から見る
                                {
                                    if (isOrderd[equiptype] == true)
                                        continue;

                                    if (find == false && SearchEndPoint[equiptype] - NowSearching[equiptype] >= DivideCount)
                                    {
                                        find = true;
                                        int dif = SearchEndPoint[equiptype] - NowSearching[equiptype];
                                        int var = dif / DivideCount;
                                        int rest = dif % DivideCount;

                                        int begin;
                                        int end;
                                        if (rest > 0)
                                            end = NowSearching[equiptype] + var + 1;
                                        else
                                            end = NowSearching[equiptype] + var;


                                        SearchEndPoint[equiptype] = end;

                                        for (int i = 1; i < DivideCount; i++)
                                        {
                                            begin = end;
                                            if (rest > i)
                                                end = begin + var + 1;
                                            else
                                                end = begin + var;

                                            SearchClass sc = WaitingMeList[i - 1];
                                            sc.NowSearching[equiptype] = sc.SearchBeginPoint[equiptype] = begin;
                                            sc.SearchEndPoint[equiptype] = end;
                                        }
                                    }
                                    else
                                    {
                                        foreach (SearchClass sc in WaitingMeList)
                                        {
                                            if (find == false)
                                            {
                                                sc.NowSearching[equiptype] = NowSearching[equiptype];

                                            }
                                            else
                                            {
                                                sc.NowSearching[equiptype] = SearchBeginPoint[equiptype];

                                            }

                                            sc.SearchBeginPoint[equiptype] = SearchBeginPoint[equiptype];
                                            sc.SearchEndPoint[equiptype] = SearchEndPoint[equiptype];
                                        }
                                    }
                                }







                                if (find)
                                {
#if DEBUG
                                    int SpritedAll = CulcRestSearch(this);

                                    foreach (SearchClass sc in WaitingMeList)
                                    {
                                        SpritedAll += CulcRestSearch(sc);
                                    }

                                    if (ThisRest != SpritedAll)
                                    {
                                        System.Windows.Forms.MessageBox.Show("err");
                                    }
#endif

                                    foreach (SearchClass sc in WaitingMeList)
                                    {
                                        Debug.WriteLine("スレッド" + ThreadNo.ToString() + "からスレッド" + sc.ThreadNo.ToString() + "へStart");

                                        lock (sc.lock_State)
                                        {
                                            sc.State = SearchClassState.Searching;
                                        }

                                        sc.RequestResult = SearchClassRequestResult.Start;
                                    }
                                }
                                else
                                {
                                    foreach (SearchClass sc in WaitingMeList)
                                    {
                                        sc.RequestResult = SearchClassRequestResult.Sorry;
                                    }
                                }





                                WaitingMeList.Clear();


                                #endregion

                            }
                        }
                        #endregion

                    }
                    #endregion
                } while (finish == false);


                #region 自分の持っている分が終了したら
                lock (lock_State)
                {
                    State = SearchClassState.Waiting;
                }

                lock (parent.LockNumOfCheckedPriorityEquipSet)
                {
                    parent.NumOfCheckedPriorityEquipSet += CheckedNum;
                }
                // CheckCount_amount += CheckedNum;
                CheckedNum = 0;

                //もし待ってる人がいたらSorryを送る
                lock (WaitingMeList)
                {
                    foreach (SearchClass waiting in WaitingMeList)
                    {
                        Debug.WriteLine("スレッド" + ThreadNo.ToString() + "からスレッド" + waiting.ThreadNo.ToString() + "へSorry");
                        waiting.RequestResult = SearchClassRequestResult.Sorry;
                    }

                    WaitingMeList.Clear();
                }
                #endregion

                #region 自分が終わったので再配布をRequest
                do
                {
                    int? find = null;

                    int tnum = parent.SearchClasses.Length;
                    //手持ちの装備が全探索終了したら
                    for (int i = (this.ThreadNo + 1) % tnum; i != this.ThreadNo; i = (i + 1) % tnum)//自分の次の番号の人から探す
                    {
                        SearchClass sc = parent.SearchClasses[i];

                        lock (sc.lock_State)
                        {
                            if (sc.State == SearchClassState.Searching)
                            {//検索中の人発見

                                find = sc.ThreadNo;
                                lock (sc.WaitingMeList)
                                {
                                    Debug.WriteLine("スレッド" + ThreadNo.ToString() + "からスレッド" + sc.ThreadNo.ToString() + "へRequest");
                                    RequestResult = SearchClassRequestResult.None;
                                    sc.WaitingMeList.Add(this);
                                }
                                break;

                            }
                        }
                    }

                    if (find != null)
                    {
                        while (RequestResult == SearchClassRequestResult.None)//返答を待つ
                        {
                            //Debug.WriteLine("スレッド" + ThreadNo.ToString() + "返答待ち("+find.ToString()+")");
                            Thread.Sleep(1);
                        }


                        if (RequestResult == SearchClassRequestResult.Start)
                        {
                            Debug.WriteLine("スレッド" + ThreadNo.ToString() + "再開");

                            SlotTypeNumArray[0] = 0;
                            SlotTypeNumArray[1] = 0;
                            SlotTypeNumArray[2] = 0;
                            SlotTypeNumArray[3] = 0;

                            SelectedEquipDef = 0;
                            numSP = 0;
                            numBlankEquip = 0;
                            numGRankEquipNum_Orderd = 0;

                            TypeUseCountTable.Clear();
                            foreach (string str in condition.TypeNumOrder.Keys)
                            {
                                TypeUseCountTable.Add(str, 0);
                            }

                            //装備セットを更新
                            for (int i = 0; i < (int)EquipKind.NumOfEquipKind; i++)
                            {
                                if (isOrderd[i] == true)
                                {
                                    Equipment data = condition.ESet[(EquipKind)i];

                                    if (data.EquipData.Name == Properties.Resources.BLANK_EQUIP_NAME)
                                    {
                                        numBlankEquip++;
                                    }


                                    foreach (String grank in BaseData.mDefine.GRankEquipType)
                                    {
                                        if (data.EquipData.Type.Equals(grank))
                                        {
                                            numGRankEquipNum_Orderd++;
                                            break;
                                        }
                                    }

                                    

                                    if (data.EquipData.isSP)
                                    {
                                        numSP++;
                                        SlotTypeNumArray[0] += data.GetRestSlotNum();
                                    }
                                    else
                                    {
                                        int rest = data.GetRestSlotNum();

                                        if (rest > 0)
                                        {
                                            SlotTypeNumArray[rest]++;
                                        }
                                    }

                                    if (condition.isOrderTypeNum && TypeUseCountTable.ContainsKey(data.EquipData.Type))
                                    {
                                        TypeUseCountTable[data.EquipData.Type]++;
                                    }

                                    continue;
                                }

                                foreach (SkillPointTag spt in EquipDataTags[i].SkillPointTags)
                                {
                                    RestPoint[spt.index].Point += spt.Point;
                                }


                                EquipDataTags[i] = PotentialEquipTagArrayArray[i][NowSearching[i]];//切り替え

                                foreach (SkillPointTag spt in EquipDataTags[i].SkillPointTags)
                                {
                                    RestPoint[spt.index].Point -= spt.Point;
                                }

                                SelectedEquipDef += EquipDataTags[i].Def;


                                if (EquipDataTags[i].equipdata.Name == Properties.Resources.BLANK_EQUIP_NAME)
                                {
                                    numBlankEquip++;
                                }

                                if (EquipDataTags[i].equipdata.isSP)
                                {
                                    numSP++;
                                    SlotTypeNumArray[0]++;
                                }
                                else if (EquipDataTags[i].Slot > 0)
                                {
                                    SlotTypeNumArray[EquipDataTags[i].Slot]++;
                                }

                                if (condition.isOrderTypeNum && TypeUseCountTable.ContainsKey(EquipDataTags[i].equipdata.Type))
                                {
                                    TypeUseCountTable[EquipDataTags[i].equipdata.Type]++;
                                }


                            }
                            break;//検索再開
                        }
                        else if (RequestResult == SearchClassRequestResult.Sorry)
                        {
                            Debug.WriteLine("スレッド" + ThreadNo.ToString() + "Sorryを受諾");
                            Thread.Sleep(500);
                        }
                    }
                    else
                    {
                        //全てのスレッドが待機したので終了する
                        goto LABEL_FINISHED;
                    }
                } while (true);
                #endregion
            } while (true);


        LABEL_FINISHED:
            //Debug.WriteLine("スレッド" + ThreadNo.ToString() + "終了。申告合計:" + CheckCount_amount.ToString());

            lock (lock_State)
            {
                State = SearchClassState.Finished;
            }

        }///////////スレッド終了

        private Dictionary<SkillBase, int> MakeEquipAndSkillCuffPointDict(EquipmentDataTag[] EquipDataTags, Dictionary<SkillBase, int> EquipAndSkillCuffPointDict, int[][] tmpSkillCuffUseCount)
        {
            if (EquipAndSkillCuffPointDict == null)
            {
                EquipAndSkillCuffPointDict = new Dictionary<SkillBase, int>(OrderdEquipSkillPoint);

                #region 装備のポイント加算
                for (int i = 0; i < (int)EquipKind.NumOfEquipKind; i++)
                {
                    EquipmentData equip;
                    if (!isOrderd[i])
                    {
                        equip = EquipDataTags[i].equipdata;

                        foreach (SkillPoint pt in equip.SkillPointList)
                        {
                            if (EquipAndSkillCuffPointDict.ContainsKey(pt.SBase))
                            {
                                EquipAndSkillCuffPointDict[pt.SBase] += pt.Point;
                            }
                            else
                            {
                                EquipAndSkillCuffPointDict[pt.SBase] = pt.Point;
                            }
                        }
                    }

                }

                #endregion

                #region スキルカフのポイント加算

                for (int a = 0; a < 2; a++)
                {
                    for (int b = 0; b < tmpSkillCuffUseCount[a].Length; b++)
                    {
                        if (tmpSkillCuffUseCount[a][b] > 0)
                        {
                            int count = tmpSkillCuffUseCount[a][b];

                            foreach (SkillPoint pt in PotentialSkillCuffTagArrayArray[a][b].jd.SkillList)
                            {
                                if (EquipAndSkillCuffPointDict.ContainsKey(pt.SBase))
                                {
                                    EquipAndSkillCuffPointDict[pt.SBase] += pt.Point * count;
                                }
                                else
                                {
                                    EquipAndSkillCuffPointDict[pt.SBase] = pt.Point * count;
                                }
                            }
                        }
                    }
                }
                #endregion
            }
            else
            {

            }

            return EquipAndSkillCuffPointDict;
        }

        private static void SetJewelryToEquipSet(EquipSet set, JewelryData_base jd, int count)
        {
            while (count > 0)
            {
                foreach (Equipment e in set.Equips)
                {
                    if (e.EquipData == null)
                        continue;

                    if (!e.EquipData.isSP)
                    {
                        if (e.GetRestSlotNum() >= jd.Slot)
                        {
                            e.SetJewelry(jd);
                            count--;
                            break;
                        }
                    }
                }
            }
        }



        /// <summary>
        /// </summary>
        /// <param name="SlotTypeNumArray"></param>
        /// <param name="RestPoint"></param>
        /// <param name="minslots"></param>
        /// <param name="UpperBlocked"></param>
        /// <param name="isUpperCheck">true→上限を考慮して珠入れ</param>
        /// <param name="PlusJewelryFixedCount">既にはめられている装飾品のカウント累計</param>
        /// <returns></returns>
        private int[][] FindEligibleJewelryUseCount(int[] SlotTypeNumArray, SkillPoint[] RestPoint, int minslots,
            ref bool UpperBlocked, SkillPoint[] JewelrysSkillPoint, int[][] PlusJewelryUseCount, int[] tmpAddCount, int[] tmpRestSlot,
            int[][] PlusJewelryFixedCount,ref Dictionary<SkillBase, int> EquipAndSkillCuffPointDict)
        {
                                
            #region 過去の失敗リストを検索
            /*
            SortedDictionary<int, object> dict;
            object obj;
            List<KeyValuePair<SortedDictionary<int, Object>, int>> DeleteList = new List<KeyValuePair<SortedDictionary<int, object>, int>>();
            SortedDictionary<int, SortedDictionary<int, SortedDictionary<int, SortedDictionary<int,Object>>>> FailedList = parent.FailedList;

            foreach (int x in FailedList.Keys)
            {
                if (x > SlotTypeNumArray[0])
                    continue;

                SortedDictionary<int, SortedDictionary<int, SortedDictionary<int,Object>>> list2 = FailedList[x];

                foreach (int y in list2.Keys)
                {
                    if (y > SlotTypeNumArray[1])
                        continue;

                    SortedDictionary<int, SortedDictionary<int, Object>> list3 = list2[y];

                    foreach (int z in list3.Keys)
                    {
                        if (z > SlotTypeNumArray[2])
                            continue;


                        dict = list3[z];

                        int len = RestPoint.Length;
                        SortedDictionary<int, Object>.Enumerator[] enumerator = new SortedDictionary<int,object>.Enumerator[len];
                        SortedDictionary<int, Object>.Enumerator e;
                        int i = 0;
                        e = dict.GetEnumerator();

                        if (e.MoveNext())
                        {
                            while (true)
                            {
                                if (e.Current.Key > RestPoint[i].Point)
                                {
                                    if (e.MoveNext())
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        do
                                        {
                                            i--;
                                            if (i < 0)
                                            {//過去履歴になかった。チャレンジする。
                                                goto CHALLENGE_LABEL;
                                            }

                                            e = enumerator[i];
                                        } while (!e.MoveNext());

                                        continue;
                                    }
                                }

                                enumerator[i] = e;
                                i++;

                                if (i >= len)
                                {
                                    //過去によりゆるい条件でも失敗していた
                                    return null;
                                }

                                obj = e.Current.Value;

                                e = ((SortedDictionary<int, Object>)obj).GetEnumerator();
                                e.MoveNext();                                
                            }
                        }


                    CHALLENGE_LABEL: ;

                    }

                }


            }
            */

            #endregion


            int[][] ReturnUseCount = null;

            #region プラス装飾品をはめる。
            int index = 0;
            bool isDowning = true;

            int MaxSkillNum = 0;

            PlusJewelryListTag pjlt = null;

            int NeedSkillNum = PlusJewelryUseCount.Length;

            int[] EndPoint = new int[NeedSkillNum];

            do
            {
                while (index < NeedSkillNum && index >= 0)
                {
                    int beginpoint = 0;
                    pjlt = PlusJewelyDataTags[index];
                    bool[] SlotExists = pjlt.SlotExists;

                    if (!isDowning)
                    {
                        //下から上がってきた
                        #region 一個減らす装飾品を探索
                        bool flag = false;

                        int last = pjlt.Length - 1;

                        if (last < 0)
                        {
                            index--;
                            continue;
                        }

                        if (PlusJewelryUseCount[index][last] > 0)
                        {
                            SlotTypeNumArray[pjlt[last].Slot - 1] += PlusJewelryUseCount[index][last];

                            foreach (SkillPointTag spt in pjlt[last].jdt.SkillPointTags)
                            {
                                JewelrysSkillPoint[spt.index].Point -= spt.Point * PlusJewelryUseCount[index][last];
                            }

                            PlusJewelryUseCount[index][last] = 0;
                        }

                        for (int m = EndPoint[index]; m >= 0; m--)
                        {
                            if (PlusJewelryUseCount[index][m] > 0)
                            {
                                PlusJewelryUseCount[index][m]--;
                                SlotTypeNumArray[pjlt[m].Slot - 1]++;

                                foreach (SkillPointTag spt in pjlt[m].jdt.SkillPointTags)
                                {
                                    JewelrysSkillPoint[spt.index].Point -= spt.Point;
                                }

                                beginpoint = m + 1;
                                flag = true;
                                break;
                            }
                        }

                        if (flag == false)
                        {
                            index--;
                            continue;
                        }
                        #endregion
                    }

                    //この装飾品で満たすべき残りのポイント数
                    int rest = RestPoint[pjlt.SkillIndex].Point - JewelrysSkillPoint[pjlt.SkillIndex].Point;

                    if (rest <= 0)
                    {
                        EndPoint[index] = beginpoint;
                        index++;
                        continue;
                    }

                    SlotTypeNumArray.CopyTo(tmpRestSlot, 0);

                    int Length = pjlt.Length;
                    Array.Clear(tmpAddCount, 0, Length);

                    int endpoint;
                    for (endpoint = beginpoint; endpoint < Length; endpoint++)
                    {
                        #region 装飾品を順番に詰めていく
                        PlusJewelryDataTag pjdt = pjlt[endpoint];

                        if (!SubFunc.CompairSgin(pjdt.SpecificPoint.Point, rest))
                        {
                            continue;
                        }


                        int num = rest / pjdt.SpecificPoint.Point;

                        if (rest % pjdt.SpecificPoint.Point > 0)
                            num++;


                        if (tmpRestSlot[pjdt.jdt.jd.Slot - 1] < num)
                        {
                            num = tmpRestSlot[pjdt.jdt.jd.Slot - 1];
                            tmpRestSlot[pjdt.jdt.jd.Slot - 1] = 0;

                           
                            //スロットごとに空きがあるかチェック
                            if ((!SlotExists[0] || tmpRestSlot[0] == 0) && (!SlotExists[1] || tmpRestSlot[1] == 0) && (!SlotExists[2] || tmpRestSlot[2] == 0))
                            {
                                break;//もう嵌めこむ予知がないのでbreak;
                            }
                            

                        }
                        else
                        {
                            tmpRestSlot[pjdt.jdt.jd.Slot - 1] -= num;
                        }

                        tmpAddCount[endpoint] = num;
                        rest -= num * pjdt.SpecificPoint.Point;

                        if (rest <= 0)
                        {
                            break;
                        }


                        #endregion
                    }

                    if (rest <= 0)
                    {
                        #region indexのスキルを満たせたので次のスキルチェックに行く
                        tmpRestSlot.CopyTo(SlotTypeNumArray, 0);

                        for (int j = beginpoint; j <= endpoint; j++)
                        {
                            if (tmpAddCount[j] == 0)
                                continue;

                            PlusJewelryUseCount[index][j] += tmpAddCount[j];

                            PlusJewelryDataTag pjdt = pjlt[j];

                            foreach (SkillPointTag spt in pjdt.jdt.SkillPointTags)
                            {
                                JewelrysSkillPoint[spt.index].Point += spt.Point * tmpAddCount[j];
                            }

                        }

                        EndPoint[index] = endpoint;

                        isDowning = true;
                        index++;
                        #endregion
                    }
                    else
                    {

                        if (!isDowning)
                        {
                            continue;
                        }
                        else
                        {
                            isDowning = false;
                            index--;
                        }
                    }


                }

                //一番上まで抜けたのなら終了。
                if (!isDowning)
                    break;


                #region 条件を満たしていれば追加
                bool enough = true;
                bool Upper = false;
                for (int j = 0; j < RestPoint.Length; j++)
                {
                    int dif = RestPoint[j].Point - JewelrysSkillPoint[j].Point;
                    if (dif > 0)
                    {
                        enough = false;
                        break;
                    }

                    if (RequirementSkillPointsSortedList.Values[j].UpperPoint != null)
                    {
                        if (-dif > Math.Abs((int)RequirementSkillPointsSortedList.Values[j].UpperPoint - RequirementSkillPointsSortedList.Values[j].Point))//上限を超えていてもアウト
                        {
                            Upper = true;
                        }
                    }

                }

                int[][] tmpPlusJewelryUseCount = null;
                SkillPoint[] tmpJewelrySkillPoint = null;

                if (enough == true)
                {
                    if (Upper)
                    {
                        #region 下限は満たしたけど上限に引っかかった
                        UpperBlocked = true;


                        if (SlotTypeNumArray[0] > 0 || SlotTypeNumArray[1] > 0 || SlotTypeNumArray[2] > 0)
                        {
                            int[] RestSlot2 = (int[])SlotTypeNumArray.Clone();
                            SkillPoint[] RestPoint2 = new SkillPoint[RestPoint.Length];
                            for (int j = 0; j < RestPoint.Length; j++)
                            {
                                RestPoint2[j] = (SkillPoint)RestPoint[j].Clone();
                                RestPoint2[j].Point -= JewelrysSkillPoint[j].Point;
                            }


                            #region 上位から引き継ぐ使用する装飾品の数に加算
                            int[][] tmpPlusJewelryFixedCount = null;
                            if (PlusJewelryFixedCount != null)
                            {
                                tmpPlusJewelryFixedCount = new int[PlusJewelryFixedCount.Length][];

                                for (int i = 0; i < PlusJewelryFixedCount.Length; i++)
                                {
                                    tmpPlusJewelryFixedCount[i] = (int[])PlusJewelryFixedCount[i].Clone();

                                    for (int j = 0; j < tmpPlusJewelryFixedCount[i].Length; j++)
                                    {
                                        tmpPlusJewelryFixedCount[i][j] += PlusJewelryUseCount[i][j];
                                    }
                                }

                            }
                            #endregion


                            //再帰的に探索
                            int[][] UseCount = FindEligibleJewelryUseCount_UpperCheck(RestSlot2, RestPoint2, int.MaxValue,tmpPlusJewelryFixedCount,ref EquipAndSkillCuffPointDict);

                            if (UseCount != null)
                            {
                                #region 追加検索で見つけていたら

                                tmpJewelrySkillPoint = new SkillPoint[JewelrysSkillPoint.Length];

                                for (int i = 0; i < tmpJewelrySkillPoint.Length; i++)
                                {
                                    tmpJewelrySkillPoint[i] = new SkillPoint(JewelrysSkillPoint[i]);
                                }

                                tmpPlusJewelryUseCount = new int[PlusJewelryUseCount.Length][];
                                for (int i = 0; i < PlusJewelryUseCount.Length; i++)
                                {
                                    tmpPlusJewelryUseCount[i] = (int[])PlusJewelryUseCount[i].Clone();
                                    for (int j = 0; j < tmpPlusJewelryUseCount[i].Length; j++)
                                    {
                                        tmpPlusJewelryUseCount[i][j] += UseCount[i][j];

                                        if (UseCount[i][j] > 0)
                                        {
                                            JewelryDataTag jdt = PlusJewelyDataTags[i][j].jdt;

                                            foreach (SkillPointTag spt in jdt.SkillPointTags)
                                            {
                                                tmpJewelrySkillPoint[spt.index].Point += spt.Point * tmpPlusJewelryUseCount[i][j];
                                            }
                                        }

                                    }
                                }
                                #endregion
                            }

                        }
                        #endregion

                    }
                    else
                    {
                        #region  上限に引っかかっていなかった
                        tmpPlusJewelryUseCount = new int[PlusJewelryUseCount.Length][];
                        for (int i = 0; i < PlusJewelryUseCount.Length; i++)
                        {
                            tmpPlusJewelryUseCount[i] = (int[])PlusJewelryUseCount[i].Clone();
                        }

                        tmpJewelrySkillPoint = new SkillPoint[JewelrysSkillPoint.Length];

                        for (int i = 0; i < tmpJewelrySkillPoint.Length; i++)
                        {
                            tmpJewelrySkillPoint[i] = new SkillPoint(JewelrysSkillPoint[i]);
                        }
                        #endregion
                    }
                }
                else
                {

                    if (SlotTypeNumArray[0] > 0 || SlotTypeNumArray[1] > 0 || SlotTypeNumArray[2] > 0)
                    {
                        #region まだスロットが残っているようであれば再帰検索
                        int[] RestSlot2 = (int[])SlotTypeNumArray.Clone();
                        SkillPoint[] RestPoint2 = new SkillPoint[RestPoint.Length];
                        for (int j = 0; j < RestPoint.Length; j++)
                        {
                            RestPoint2[j] = (SkillPoint)RestPoint[j].Clone();
                            RestPoint2[j].Point -= JewelrysSkillPoint[j].Point;
                        }

                        //再帰的に探索
                        Upper = false;


                        SkillPoint[] tmpPlusJewelrySkillPoint = new SkillPoint[RestPoint.Length];
                        for (int i = 0; i < tmpPlusJewelrySkillPoint.Length; i++)
                        {
                            tmpPlusJewelrySkillPoint[i] = new SkillPoint(RestPoint[i].SBase, 0);
                        }

                        int[][] tmpcount = new int[PlusJewelyDataTags.Length][];
                        for (int i = 0; i < PlusJewelyDataTags.Length; i++)
                        {
                            tmpcount[i] = new int[PlusJewelyDataTags[i].Length];
                        }

                        #region 上位から引き継ぐ使用する装飾品の数に加算
                        int[][] tmpPlusJewelryFixedCount = null;
                        if (PlusJewelryFixedCount != null)
                        {
                            tmpPlusJewelryFixedCount = new int[PlusJewelryFixedCount.Length][];

                            for (int i = 0; i < PlusJewelryFixedCount.Length; i++)
                            {
                                tmpPlusJewelryFixedCount[i] = (int[])PlusJewelryFixedCount[i].Clone();

                                for (int j = 0; j < tmpPlusJewelryFixedCount[i].Length; j++)
                                {
                                    tmpPlusJewelryFixedCount[i][j] += PlusJewelryUseCount[i][j];
                                }
                            }

                        }
                        #endregion

                        int[][] UseCount = FindEligibleJewelryUseCount(RestSlot2, RestPoint2, int.MaxValue, ref Upper, tmpPlusJewelrySkillPoint, tmpcount, new int[ADDCOUNT_LEN], new int[3], tmpPlusJewelryFixedCount, ref EquipAndSkillCuffPointDict);

                        if (UseCount != null)
                        {
                            #region 追加検索で見つけていたら

                            tmpJewelrySkillPoint = new SkillPoint[JewelrysSkillPoint.Length];

                            for (int i = 0; i < tmpJewelrySkillPoint.Length; i++)
                            {
                                tmpJewelrySkillPoint[i] = new SkillPoint(JewelrysSkillPoint[i]);
                            }

                            tmpPlusJewelryUseCount = new int[PlusJewelryUseCount.Length][];
                            for (int i = 0; i < PlusJewelryUseCount.Length; i++)
                            {
                                tmpPlusJewelryUseCount[i] = (int[])PlusJewelryUseCount[i].Clone();
                                for (int j = 0; j < tmpPlusJewelryUseCount[i].Length; j++)
                                {
                                    tmpPlusJewelryUseCount[i][j] += UseCount[i][j];

                                    if (UseCount[i][j] > 0)
                                    {
                                        JewelryDataTag jdt = PlusJewelyDataTags[i][j].jdt;

                                        foreach (SkillPointTag spt in jdt.SkillPointTags)
                                        {
                                            tmpJewelrySkillPoint[spt.index].Point += spt.Point * UseCount[i][j];
                                        }
                                    }

                                }
                            }
                            #endregion
                        }
                        #endregion
                    }
                }


                if (tmpPlusJewelryUseCount != null)
                {
                   //下限も上限も全て満たしている
                    #region 不要な装飾品はカット
                    bool changed;
                    do
                    {
                        changed = false;
                        for (int i = 0; i < tmpPlusJewelryUseCount.Length; i++)
                        {
                            JewelryDataTag[] Jewelrys = new JewelryDataTag[PlusJewelyDataTags[i].Length];

                            for (int j = 0; j < Jewelrys.Length; j++)
                                Jewelrys[j] = PlusJewelyDataTags[i][j].jdt;

                            if (CutUnUseJewelry(RestPoint, tmpJewelrySkillPoint, Jewelrys, tmpPlusJewelryUseCount[i]))
                                changed = true;
                        }
                    } while (changed);
                    #endregion

                    #region 総スキル値の計算と仕様スロット数の算出
                    EquipAndSkillCuffPointDict = MakeEquipAndSkillCuffPointDict(EquipDataTags, EquipAndSkillCuffPointDict, SkillCuffUseCount);
                    Dictionary<SkillBase, int> tmpSkillSum = new Dictionary<SkillBase, int>(EquipAndSkillCuffPointDict);

                    int UseSlot = 0;

                    for (int i = 0; i < tmpPlusJewelryUseCount.Length; i++)
                    {
                        for (int j = 0; j < tmpPlusJewelryUseCount[i].Length; j++)
                        {
                            UseSlot += tmpPlusJewelryUseCount[i][j] * PlusJewelyDataTags[i][j].Slot;

                            foreach (SkillPoint pt in PlusJewelyDataTags[i][j].jdt.jd.SkillList)
                            {
                                if (tmpSkillSum.ContainsKey(pt.SBase))
                                {
                                    tmpSkillSum[pt.SBase] += pt.Point * tmpPlusJewelryUseCount[i][j];
                                }
                                else
                                {
                                    tmpSkillSum[pt.SBase] = pt.Point * tmpPlusJewelryUseCount[i][j];
                                }
                            }
                        }
                    }

                    if (PlusJewelryFixedCount != null)
                    {
                        for (int i = 0; i < PlusJewelryFixedCount.Length; i++)
                        {
                            for (int j = 0; j < PlusJewelryFixedCount[i].Length; j++)
                            {
                                foreach (SkillPoint pt in PlusJewelyDataTags[i][j].jdt.jd.SkillList)
                                {
                                    if (tmpSkillSum.ContainsKey(pt.SBase))
                                    {
                                        tmpSkillSum[pt.SBase] += pt.Point * PlusJewelryFixedCount[i][j];
                                    }
                                    else
                                    {
                                        tmpSkillSum[pt.SBase] = pt.Point * PlusJewelryFixedCount[i][j];
                                    }
                                }
                            }
                        }
                    }




                    for (int i = 0; i < SinglePlusJewelryUseCount.Length; i++)
                    {
                        if (SinglePlusJewelryDataTags[i].jdt == null)
                            continue;

                        foreach (SkillPoint pt in SinglePlusJewelryDataTags[i].jdt.jd.SkillList)
                        {
                            if (tmpSkillSum.ContainsKey(pt.SBase))
                            {
                                tmpSkillSum[pt.SBase] += pt.Point * SinglePlusJewelryUseCount[i];
                            }
                            else
                            {
                                tmpSkillSum[pt.SBase] = pt.Point * SinglePlusJewelryUseCount[i];
                            }
                        }
                    }
                    #endregion

                    #region 発動する順番に並べて、10までで有用なスキルをカウント
                    List<SkillBase> list = new List<SkillBase>();
                    foreach (KeyValuePair<SkillBase, int> pair in tmpSkillSum)
                    {
                        SkillOption so = pair.Key.GetOption(pair.Value);
                        if (null != so)
                        {
                            list.Add(pair.Key);
                        }
                    }

                    int SkillCount = 0;
                    if (list.Count > 10)
                    {
                        list.Sort();

                        for (int i = 0; i < list.Count && i < 10; i++)
                        {
                            SkillBase sb = list[i];
                            foreach (SkillPointCondition spc in condition.SkillPointConditionTable.Values)
                            {
                                if (!spc.isIgnore && sb == spc.SBase)
                                {
                                    SkillCount++;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        SkillCount = OrderdSkillNum;
                    }

                    #endregion

                    //より最適な結果であれば候補を差し替え
                    if ((minslots > UseSlot && MaxSkillNum == SkillCount) || MaxSkillNum < SkillCount)
                    {
                        if (MaxSkillNum != 0 && MaxSkillNum < SkillCount)
                        {
                        }

                        minslots = UseSlot;
                        MaxSkillNum = SkillCount;

                        ReturnUseCount = tmpPlusJewelryUseCount;
                    }


                }

                #endregion

                isDowning = false;
                index--;

            } while (true);
            #endregion


            #region 失敗リストに追加
            /*
            if (ReturnUseCount == null && UpperBlocked == false)
            {
                lock (FailedList)
                {
                    #region 過去の失敗リストから削除

                    foreach (int x in FailedList.Keys)
                    {
                        if (x > SlotTypeNumArray[0])
                            continue;

                        SortedDictionary<int, SortedDictionary<int, SortedDictionary<int, Object>>> list2 = FailedList[x];

                        foreach (int y in list2.Keys)
                        {
                            if (y > SlotTypeNumArray[1])
                                continue;

                            SortedDictionary<int, SortedDictionary<int, Object>> list3 = list2[y];

                            foreach (int z in list3.Keys)
                            {
                                if (z > SlotTypeNumArray[2])
                                    continue;

                                DeleteList.Clear();
                                int len = RestPoint.Length;
                                SortedDictionary<int, Object>.Enumerator[] enumerator = new SortedDictionary<int, object>.Enumerator[len];
                                SortedDictionary<int, Object>[] sorteddicts = new SortedDictionary<int, object>[len];
                                SortedDictionary<int, Object>.Enumerator e;
                                int i = 0;
                                dict = sorteddicts[0]= list3[z];
                                e = dict.GetEnumerator();

                                if (e.MoveNext())
                                {
                                    while (true)
                                    {
                                        if (e.Current.Key < RestPoint[i].Point)
                                        {
                                            if (e.MoveNext())
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                do
                                                {
                                                    i--;
                                                    if (i < 0)
                                                    {//過去履歴になかった。。
                                                        goto NEXT_LABEL;
                                                    }

                                                    e = enumerator[i];
                                                } while (!e.MoveNext());

                                                continue;
                                            }
                                        }

                                        enumerator[i] = e;
                                        i++;

                                        if (i >= len)
                                        {
                                            //削除する

                                            for (i = len - 1; i >= 0; i--)
                                            {
                                                if (sorteddicts[i].Count == 1)
                                                {
                                                    continue;
                                                }
                                                else
                                                {
                                                    e = enumerator[i];

                                                    //後で削除するリストに追加
                                                    DeleteList.Add(new KeyValuePair<SortedDictionary<int,Object>,int>(sorteddicts[i], e.Current.Key));
                                                    
                                                    while (!e.MoveNext())
                                                    {
                                                        i--;
                                                        if (i < 0)
                                                        {
                                                            goto NEXT_LABEL;

                                                        }

                                                        e = enumerator[i];
                                                    }
                                                    i++;
                                                    break;
                                                }
                                            }

                                        }

                                        obj = e.Current.Value;
                                        sorteddicts[i] = (SortedDictionary<int, Object>)obj;

                                        e = ((SortedDictionary<int, Object>)obj).GetEnumerator();
                                        e.MoveNext();
                                    }
                                }


                            NEXT_LABEL: ;

                                foreach (KeyValuePair<SortedDictionary<int, Object>,int> dic in DeleteList)
                                {
                                    dic.Key.Remove(dic.Value);
                                }

                            }

                        }


                    }


                    #endregion

                    #region 今回の失敗を追加
                    
                    SortedDictionary<int, SortedDictionary<int, SortedDictionary<int, Object>>> slot2;
                    SortedDictionary<int, SortedDictionary<int, Object>> slot3;

                    if (!FailedList.TryGetValue(SlotTypeNumArray[0], out slot2))
                    {
                        slot2 = new SortedDictionary<int, SortedDictionary<int, SortedDictionary<int, Object>>>();
                        FailedList.Add(SlotTypeNumArray[0], slot2);
                    }

                    if (!slot2.TryGetValue(SlotTypeNumArray[1], out slot3))
                    {
                        slot3 = new SortedDictionary<int, SortedDictionary<int, Object>>();
                        slot2.Add(SlotTypeNumArray[1], slot3);
                    }

                    if (!slot3.TryGetValue(SlotTypeNumArray[2], out dict))
                    {
                        dict = new SortedDictionary<int, Object>();
                        slot3.Add(SlotTypeNumArray[2], dict);
                    }

                    foreach (SkillPoint sp in RestPoint)
                    {
                        if (!dict.TryGetValue(sp.Point, out obj))
                        {
                            obj = new SortedDictionary<int, Object>();
                            dict.Add(sp.Point, obj);
                        }

                        dict = (SortedDictionary<int, object>)obj;

                    }

                    #endregion


                }
            }
            */
            #endregion

            return ReturnUseCount;

        }

        private int[][] FindEligibleJewelryUseCount_UpperCheck(int[] SlotTypeNumArray, SkillPoint[] RestPoint, int minslots,int[][] PlusJewelryFixedCount,ref Dictionary<SkillBase, int> EquipAndSkillCuffPointDict)
        {

            SkillPoint[] JewelrysSkillPoint = new SkillPoint[RestPoint.Length];
            for (int i = 0; i < JewelrysSkillPoint.Length; i++)
                JewelrysSkillPoint[i] = new SkillPoint(RestPoint[i].SBase, 0);

            int[][] PlusJewelryUseCount = new int[PlusJewelyDataTags.Length][];
            for (int i = 0; i < PlusJewelyDataTags.Length; i++)
                PlusJewelryUseCount[i] = new int[PlusJewelyDataTags[i].Length];

            int[][] ReturnUseCount = null;

            int MaxSkillNum = 0;

            PlusJewelryListTag pjlt = null;

            #region プラス装飾品をはめる。
            int index = 0;
            bool isDowning = true;
            do
            {
                while (index < PlusJewelryUseCount.Length && index >= 0)
                {
                    int beginpoint = 0;
                    pjlt = PlusJewelyDataTags[index];
                    bool[] SlotExists = pjlt.SlotExists;

                    if (!isDowning)
                    {
                        //下から上がってきた
                        #region 一個減らす装飾品を探索
                        bool flag = false;

                        int last = PlusJewelryUseCount[index].Length - 1;

                        if (last < 0)
                        {
                            index--;
                            continue;
                        }


                        if (PlusJewelryUseCount[index][last] > 0)
                        {
                            SlotTypeNumArray[PlusJewelyDataTags[index][last].Slot - 1] += PlusJewelryUseCount[index][last];

                            foreach (SkillPointTag spt in PlusJewelyDataTags[index][last].jdt.SkillPointTags)
                            {
                                JewelrysSkillPoint[spt.index].Point -= spt.Point * PlusJewelryUseCount[index][last];
                            }

                            PlusJewelryUseCount[index][last] = 0;
                        }

                        for (int m = last - 1; m >= 0; m--)
                        {
                            if (PlusJewelryUseCount[index][m] > 0)
                            {
                                PlusJewelryUseCount[index][m]--;
                                SlotTypeNumArray[PlusJewelyDataTags[index][m].Slot - 1]++;

                                foreach (SkillPointTag spt in PlusJewelyDataTags[index][m].jdt.SkillPointTags)
                                {
                                    JewelrysSkillPoint[spt.index].Point -= spt.Point;
                                }

                                beginpoint = m + 1;
                                flag = true;
                                break;
                            }
                        }

                        if (flag == false)
                        {
                            index--;
                            continue;
                        }
                        #endregion
                    }

                    int rest = RestPoint[PlusJewelyDataTags[index].SkillIndex].Point - JewelrysSkillPoint[PlusJewelyDataTags[index].SkillIndex].Point;

                    int SKillIndex = PlusJewelyDataTags[index].SkillIndex;
                    SkillPointCondition spc = RequirementSkillPointsSortedList.Values[SKillIndex];

                    int difmax = int.MaxValue;

                    if (spc.UpperPoint != null)
                    {
                        difmax = Math.Abs((int)spc.UpperPoint - spc.Point);

                        if (rest <= 0 && rest >= -difmax)
                        {
                            index++;
                            continue;
                        }
                    }
                    else if (rest <= 0)
                    {
                        index++;
                        continue;
                    }



                    int[] RestSlot = (int[])SlotTypeNumArray.Clone();
                    int[] AddCount = new int[PlusJewelyDataTags[index].Length];

                    for (int j = beginpoint; j < AddCount.Length; j++)
                    {
                        #region 装飾品を順番に詰めていく
                        PlusJewelryDataTag pjdt = PlusJewelyDataTags[index][j];

                        if (!SubFunc.CompairSgin(pjdt.SpecificPoint.Point, rest))
                        {
                            continue;
                        }

                        int num = 0;
                        if (rest > 0)
                        {
                            num = rest / pjdt.SpecificPoint.Point;

                            if (rest % pjdt.SpecificPoint.Point > 0)
                                num++;
                        }
                        else
                        {
                            num = (difmax + rest) / pjdt.SpecificPoint.Point;

                            if (-(difmax + rest) % -pjdt.SpecificPoint.Point > 0)
                                num++;
                        }

                        if (RestSlot[pjdt.jdt.jd.Slot - 1] < num)
                        {
                            num = RestSlot[pjdt.jdt.jd.Slot - 1];
                            RestSlot[pjdt.jdt.jd.Slot - 1] = 0;

                            //スロットごとに空きがあるかチェック
                            if ((!SlotExists[0] || RestSlot[0] == 0) && (!SlotExists[1] || RestSlot[1] == 0) && (!SlotExists[2] || RestSlot[2] == 0))
                            {
                                break;//もう嵌めこむ予知がないのでbreak;
                            }                          

                        }
                        else
                        {
                            RestSlot[pjdt.jdt.jd.Slot - 1] -= num;
                        }

                        AddCount[j] = num;
                        rest -= num * pjdt.SpecificPoint.Point;


                        if (rest <= 0 && rest >= -difmax)
                        {
                            break;
                        }

                        #endregion
                    }

                    if (rest <= 0 && rest >= -difmax)
                    {
                        RestSlot.CopyTo(SlotTypeNumArray, 0);

                        for (int j = beginpoint; j < AddCount.Length; j++)
                        {
                            if (AddCount[j] == 0)
                                continue;

                            PlusJewelryUseCount[index][j] += AddCount[j];

                            PlusJewelryDataTag pjdt = PlusJewelyDataTags[index][j];

                            foreach (SkillPointTag spt in pjdt.jdt.SkillPointTags)
                            {
                                JewelrysSkillPoint[spt.index].Point += spt.Point * AddCount[j];
                            }

                        }

                        isDowning = true;
                        index++;
                    }
                    else
                    {
                        if (!isDowning)
                        {
                            continue;
                        }
                        else
                        {
                            isDowning = false;
                            index--;
                        }
                    }


                }

                if (!isDowning)
                    break;


                #region 条件を満たしていれば追加
                bool enough = true;
                bool Upper = false;
                for (int j = 0; j < RestPoint.Length; j++)
                {
                    int dif = RestPoint[j].Point - JewelrysSkillPoint[j].Point;
                    if (dif > 0)
                    {
                        enough = false;
                        break;
                    }

                    if (RequirementSkillPointsSortedList.Values[j].UpperPoint != null)
                    {
                        if (-dif > Math.Abs((int)RequirementSkillPointsSortedList.Values[j].UpperPoint - RequirementSkillPointsSortedList.Values[j].Point))//上限を超えていてもアウト
                        {
                            Upper = true;
                        }
                    }

                }

                int[][] tmpPlusJewelryUseCount = null;
                SkillPoint[] tmpJewelrySkillPoint = null;

                if (enough == true)
                {
                    if (Upper)
                    {//下限は満たしたけど上限に引っかかった

                        if (SlotTypeNumArray[0] > 0 || SlotTypeNumArray[1] > 0 || SlotTypeNumArray[2] > 0)
                        {
                            int[] RestSlot2 = (int[])SlotTypeNumArray.Clone();
                            SkillPoint[] RestPoint2 = new SkillPoint[RestPoint.Length];
                            for (int j = 0; j < RestPoint.Length; j++)
                            {
                                RestPoint2[j] = (SkillPoint)RestPoint[j].Clone();
                                RestPoint2[j].Point -= JewelrysSkillPoint[j].Point;
                            }

                            #region 上位から引き継ぐ使用する装飾品の数に加算
                            int[][] tmpPlusJewelryFixedCount = null;
                            if (PlusJewelryFixedCount != null)
                            {
                                tmpPlusJewelryFixedCount = new int[PlusJewelryFixedCount.Length][];

                                for (int i = 0; i < PlusJewelryFixedCount.Length; i++)
                                {
                                    tmpPlusJewelryFixedCount[i] = (int[])PlusJewelryFixedCount[i].Clone();

                                    for (int j = 0; j < tmpPlusJewelryFixedCount[i].Length; j++)
                                    {
                                        tmpPlusJewelryFixedCount[i][j] += PlusJewelryUseCount[i][j];
                                    }
                                }

                            }
                            #endregion


                            //再帰的に探索
                            int[][] UseCount = FindEligibleJewelryUseCount_UpperCheck(RestSlot2, RestPoint2, int.MaxValue,tmpPlusJewelryFixedCount,ref EquipAndSkillCuffPointDict);

                            if (UseCount != null)
                            {
                                #region 追加検索で見つけていたら

                                tmpJewelrySkillPoint = new SkillPoint[JewelrysSkillPoint.Length];

                                for (int i = 0; i < tmpJewelrySkillPoint.Length; i++)
                                {
                                    tmpJewelrySkillPoint[i] = new SkillPoint(JewelrysSkillPoint[i]);
                                }

                                tmpPlusJewelryUseCount = new int[PlusJewelryUseCount.Length][];
                                for (int i = 0; i < PlusJewelryUseCount.Length; i++)
                                {
                                    tmpPlusJewelryUseCount[i] = (int[])PlusJewelryUseCount[i].Clone();
                                    for (int j = 0; j < tmpPlusJewelryUseCount[i].Length; j++)
                                    {
                                        tmpPlusJewelryUseCount[i][j] += UseCount[i][j];

                                        if (UseCount[i][j] > 0)
                                        {
                                            JewelryDataTag jdt = PlusJewelyDataTags[i][j].jdt;

                                            foreach (SkillPointTag spt in jdt.SkillPointTags)
                                            {
                                                tmpJewelrySkillPoint[spt.index].Point += spt.Point * UseCount[i][j];
                                            }
                                        }

                                    }
                                }
                                #endregion
                            }

                        }


                    }
                    else
                    {//上限に引っかかっていなかった
                        tmpPlusJewelryUseCount = new int[PlusJewelryUseCount.Length][];
                        for (int i = 0; i < PlusJewelryUseCount.Length; i++)
                        {
                            tmpPlusJewelryUseCount[i] = (int[])PlusJewelryUseCount[i].Clone();
                        }

                        tmpJewelrySkillPoint = new SkillPoint[JewelrysSkillPoint.Length];

                        for (int i = 0; i < tmpJewelrySkillPoint.Length; i++)
                        {
                            tmpJewelrySkillPoint[i] = new SkillPoint(JewelrysSkillPoint[i]);
                        }
                    }
                }
                else
                {

                    if (SlotTypeNumArray[0] > 0 || SlotTypeNumArray[1] > 0 || SlotTypeNumArray[2] > 0)
                    {
                        int[] RestSlot2 = (int[])SlotTypeNumArray.Clone();
                        SkillPoint[] RestPoint2 = new SkillPoint[RestPoint.Length];
                        for (int j = 0; j < RestPoint.Length; j++)
                        {
                            RestPoint2[j] = (SkillPoint)RestPoint[j].Clone();
                            RestPoint2[j].Point -= JewelrysSkillPoint[j].Point;
                        }

                        //再帰的に探索
                        Upper = false;

                        SkillPoint[] tmpPlusJewelrySkillPoint = new SkillPoint[RestPoint.Length];
                        for (int i = 0; i < tmpPlusJewelrySkillPoint.Length; i++)
                        {
                            tmpPlusJewelrySkillPoint[i] = new SkillPoint(RestPoint[i].SBase, 0);
                        }

                        int[][] tmpcount = new int[PlusJewelyDataTags.Length][];
                        for (int i = 0; i < PlusJewelyDataTags.Length; i++)
                        {
                            tmpcount[i] = new int[PlusJewelyDataTags[i].Length];
                        }

                        #region 上位から引き継ぐ使用する装飾品の数に加算
                        int[][] tmpPlusJewelryFixedCount = null;
                        if (PlusJewelryFixedCount != null)
                        {
                            tmpPlusJewelryFixedCount = new int[PlusJewelryFixedCount.Length][];

                            for (int i = 0; i < PlusJewelryFixedCount.Length; i++)
                            {
                                tmpPlusJewelryFixedCount[i] = (int[])PlusJewelryFixedCount[i].Clone();

                                for (int j = 0; j < tmpPlusJewelryFixedCount[i].Length; j++)
                                {
                                    tmpPlusJewelryFixedCount[i][j] += PlusJewelryUseCount[i][j];
                                }
                            }

                        }
                        #endregion


                        int[][] UseCount = FindEligibleJewelryUseCount(RestSlot2, RestPoint2, int.MaxValue, ref Upper,tmpPlusJewelrySkillPoint,tmpcount,
                            new int[ADDCOUNT_LEN],new int[3],tmpPlusJewelryFixedCount,ref EquipAndSkillCuffPointDict);

                        if (UseCount != null)
                        {
                            #region 追加検索で見つけていたら

                            tmpJewelrySkillPoint = new SkillPoint[JewelrysSkillPoint.Length];

                            for (int i = 0; i < tmpJewelrySkillPoint.Length; i++)
                            {
                                tmpJewelrySkillPoint[i] = new SkillPoint(JewelrysSkillPoint[i]);
                            }

                            tmpPlusJewelryUseCount = new int[PlusJewelryUseCount.Length][];
                            for (int i = 0; i < PlusJewelryUseCount.Length; i++)
                            {
                                tmpPlusJewelryUseCount[i] = (int[])PlusJewelryUseCount[i].Clone();
                                for (int j = 0; j < tmpPlusJewelryUseCount[i].Length; j++)
                                {
                                    tmpPlusJewelryUseCount[i][j] += UseCount[i][j];

                                    if (UseCount[i][j] > 0)
                                    {
                                        JewelryDataTag jdt = PlusJewelyDataTags[i][j].jdt;

                                        foreach (SkillPointTag spt in jdt.SkillPointTags)
                                        {
                                            tmpJewelrySkillPoint[spt.index].Point += spt.Point * tmpPlusJewelryUseCount[i][j];
                                        }
                                    }

                                }
                            }
                            #endregion
                        }

                    }
                }



                if (tmpPlusJewelryUseCount != null)
                {
                    //下限も上限も全て満たしている

                    #region 不要な装飾品はカット
                    bool changed;
                    do
                    {
                        changed = false;
                        for (int i = 0; i < tmpPlusJewelryUseCount.Length; i++)
                        {
                            JewelryDataTag[] Jewelrys = new JewelryDataTag[PlusJewelyDataTags[i].Length];

                            for (int j = 0; j < Jewelrys.Length; j++)
                                Jewelrys[j] = PlusJewelyDataTags[i][j].jdt;

                            if (CutUnUseJewelry(RestPoint, tmpJewelrySkillPoint, Jewelrys, tmpPlusJewelryUseCount[i]))
                                changed = true;
                        }
                    } while (changed);
                    #endregion

                    #region 総スキル値の計算と仕様スロット数の算出
                    EquipAndSkillCuffPointDict = MakeEquipAndSkillCuffPointDict(EquipDataTags, EquipAndSkillCuffPointDict, SkillCuffUseCount);
                    Dictionary<SkillBase, int> tmpSkillSum = new Dictionary<SkillBase, int>(EquipAndSkillCuffPointDict);

                    int UseSlot = 0;

                    for (int i = 0; i < tmpPlusJewelryUseCount.Length; i++)
                    {
                        for (int j = 0; j < tmpPlusJewelryUseCount[i].Length; j++)
                        {
                            UseSlot += tmpPlusJewelryUseCount[i][j] * PlusJewelyDataTags[i][j].Slot;

                            foreach (SkillPoint pt in PlusJewelyDataTags[i][j].jdt.jd.SkillList)
                            {
                                if (tmpSkillSum.ContainsKey(pt.SBase))
                                {
                                    tmpSkillSum[pt.SBase] += pt.Point * tmpPlusJewelryUseCount[i][j];
                                }
                                else
                                {
                                    tmpSkillSum[pt.SBase] = pt.Point * tmpPlusJewelryUseCount[i][j];
                                }
                            }
                        }
                    }

                    if (PlusJewelryFixedCount != null)
                    {
                        for (int i = 0; i < PlusJewelryFixedCount.Length; i++)
                        {
                            for (int j = 0; j < PlusJewelryFixedCount[i].Length; j++)
                            {
                                foreach (SkillPoint pt in PlusJewelyDataTags[i][j].jdt.jd.SkillList)
                                {
                                    if (tmpSkillSum.ContainsKey(pt.SBase))
                                    {
                                        tmpSkillSum[pt.SBase] += pt.Point * PlusJewelryFixedCount[i][j];
                                    }
                                    else
                                    {
                                        tmpSkillSum[pt.SBase] = pt.Point * PlusJewelryFixedCount[i][j];
                                    }
                                }
                            }
                        }
                    }


                    for (int i = 0; i < SinglePlusJewelryUseCount.Length; i++)
                    {
                        if (SinglePlusJewelryDataTags[i].jdt == null)
                            continue;

                        foreach (SkillPoint pt in SinglePlusJewelryDataTags[i].jdt.jd.SkillList)
                        {
                            if (tmpSkillSum.ContainsKey(pt.SBase))
                            {
                                tmpSkillSum[pt.SBase] += pt.Point * SinglePlusJewelryUseCount[i];
                            }
                            else
                            {
                                tmpSkillSum[pt.SBase] = pt.Point * SinglePlusJewelryUseCount[i];
                            }
                        }
                    }
                    #endregion

                    #region 発動する順番に並べて、10までで有用なスキルをカウント
                    List<SkillBase> list = new List<SkillBase>();
                    foreach (KeyValuePair<SkillBase, int> pair in tmpSkillSum)
                    {
                        SkillOption so = pair.Key.GetOption(pair.Value);
                        if (null != so)
                        {
                            list.Add(pair.Key);
                        }
                    }

                    int SkillCount = 0;
                    if (list.Count > 10)
                    {
                        list.Sort();

                        for (int i = 0; i < list.Count && i < 10; i++)
                        {
                            SkillBase sb = list[i];
                            foreach (SkillPointCondition spc in condition.SkillPointConditionTable.Values)
                            {
                                if (!spc.isIgnore && sb == spc.SBase)
                                {
                                    SkillCount++;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        SkillCount = OrderdSkillNum;
                    }

                    #endregion

                    if ((minslots > UseSlot && MaxSkillNum == SkillCount) || MaxSkillNum < SkillCount)
                    {
                        minslots = UseSlot;
                        MaxSkillNum = SkillCount;

                        ReturnUseCount = tmpPlusJewelryUseCount;
                    }

                }

                #endregion

                isDowning = false;
                index--;

            } while (true);
            #endregion


            return ReturnUseCount;

        }




        private bool CutUnUseJewelry(SkillPoint[] RestPoint, SkillPoint[] tmpJewelrySkillPoint, JewelryDataTag[] Jewelrys, int[] JewelrysUseCount)
        {
            bool changed = false;
            for (int j = 0; j < JewelrysUseCount.Length; j++)
            {
                if (JewelrysUseCount[j] > 0)
                {
                    bool flag = true;
                    int min = int.MaxValue;
                    foreach (SkillPointTag spt in Jewelrys[j].SkillPointTags)
                    {
                        if (spt.Point < 0)
                        {
                            if (RequirementSkillPointsSortedList.Values[spt.index].UpperPoint != null)
                            {
                                int dif = Math.Abs((int)RequirementSkillPointsSortedList.Values[spt.index].UpperPoint - RequirementSkillPointsSortedList.Values[spt.index].Point)
                                - tmpJewelrySkillPoint[spt.index].Point + RestPoint[spt.index].Point;

                                if (dif < -spt.Point)
                                {
                                    flag = false;
                                    break;
                                }
                                else
                                {
                                    if (min > dif / -spt.Point)
                                        min = dif / -spt.Point;
                                }
                            }
                        }
                        else
                        {
                            int x = tmpJewelrySkillPoint[spt.index].Point - RestPoint[spt.index].Point;
                            if (x < spt.Point)
                            {
                                flag = false;
                                break;
                            }
                            else
                            {
                                if (min > x / spt.Point)
                                    min = x / spt.Point;
                            }
                        }
                    }

                    if (flag == true)
                    {
                        int cutnum;
                        if (JewelrysUseCount[j] < min)
                        {
                            cutnum = JewelrysUseCount[j];
                        }
                        else
                        {
                            cutnum = min;
                        }

                        JewelrysUseCount[j] -= cutnum;

                        foreach (SkillPointTag spt in Jewelrys[j].SkillPointTags)
                        {
                            tmpJewelrySkillPoint[spt.index].Point -= spt.Point * cutnum;
                        }
                        changed = true;
                    }
                }
            }

            return changed;
        }



#if DEBUG
        private static int CulcRestSearch(SearchClass c)//そのスレッドの残り検索数を算出
        {
            int TESTSUM = 1;

            int bairitu = 1;

            for (int equiptype = (int)EquipKind.NumOfEquipKind - 1; equiptype >= 0; equiptype--)//分割
            {
                TESTSUM += (c.SearchEndPoint[equiptype] - c.NowSearching[equiptype] - 1) * bairitu;

                bairitu *= (c.SearchEndPoint[equiptype] - c.SearchBeginPoint[equiptype]);
            }

            return TESTSUM;
        }

        private void PrintEquipTagTree(EquipmentDataTag tag, string str, bool ExistNext)
        {

            if (ExistNext == true)
            {
                Debug.WriteLine(str + "├" + tag.ToString());

                str += "│";
            }
            else
            {
                Debug.WriteLine(str + "└" + tag.ToString());

                str += "　";
            }


            for (int i = 0; i < tag.BackwardEquips.Length; i++)
            {
                PrintEquipTagTree(tag.BackwardEquips[i], str, i + 1 < tag.BackwardEquips.Length);
            }
        }


        public static void PrintEquipTagTree(EquipTagTreeNode node, string str)
        {

            if (node.Next != null)
            {
                Debug.WriteLine(str + "├" + node.Tag.ToString());

                str += "│";
            }
            else
            {
                Debug.WriteLine(str + "└" + node.Tag.ToString());

                str += "　";
            }

            EquipTagTreeNode child = node.Child;

            for (; child != null; child = child.Next)
            {
                PrintEquipTagTree(child, str);
            }
        }

        public static void PrintTagTreeAll(EquipTagTreeNode[] NowSearchTrees)
        {
            Debug.WriteLine("---------------------------");
            PrintEquipTagTree(NowSearchTrees[1], "");
            PrintEquipTagTree(NowSearchTrees[2], "");
            PrintEquipTagTree(NowSearchTrees[3], "");
            PrintEquipTagTree(NowSearchTrees[4], "");
            PrintEquipTagTree(NowSearchTrees[5], "");
            Debug.WriteLine("---------------------------");
        }

#endif

    }

    class EquipSetKey : IEquatable<EquipSetKey>, ICloneable
    {
        public EquipmentDataTag[] Set = null;

        public EquipSetKey()
        {
        }

        public EquipSetKey(EquipmentDataTag[] tags, int depth)
        {
            Set = new EquipmentDataTag[depth];
            Array.Copy(tags, Set, depth);
        }

        public override int GetHashCode()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Set.Length; i++)
            {
                if (Set[i] == null)
                    continue;

                if (Set[i].equipdata == null)
                {
                    sb.Append("\t");
                }
                else
                    sb.Append(Set[i].equipdata.Name + "\t");
            }

            return sb.ToString().GetHashCode();
        }

        public override string ToString()
        {
            string str = "";

            for (int i = 0; i < Set.Length; i++)
            {
                if (Set[i] == null)
                    continue;

                if (Set[i].equipdata != null)
                    str += Set[i].equipdata.Name[0];
                else
                    str += "　";
            }

            return str;
        }

        #region IEquatable<EquipSetTag> メンバ

        public bool Equals(EquipSetKey other)
        {
            if (other.Set.Length != Set.Length)
            {
                throw new Exception("想定外の比較が発生");
            }

            for (int i = 0; i < Set.Length; i++)
            {
                if (Set[i] != other.Set[i])
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region ICloneable メンバ

        public object Clone()
        {
            EquipSetKey ret = new EquipSetKey();
            ret.Set = (EquipmentDataTag[])Set.Clone();
            return ret;
        }

        #endregion
    }

    public class EquipTagTreeNode
    {
        public EquipmentDataTag Tag = null;

        public EquipTagTreeNode Prev = null;
        public EquipTagTreeNode Next = null;
        public EquipTagTreeNode Parent = null;
        public EquipTagTreeNode Child = null;

        public EquipTagTreeNode(EquipTagTreeNode src)
        {
            Tag = src.Tag;

            if (src.Child != null)
            {
                Child = new EquipTagTreeNode(src.Child);
                Child.Parent = this;

                EquipTagTreeNode tmp = Child.Next;

                while (tmp != null)
                {
                    tmp.Parent = this;
                    tmp = tmp.Next;
                }
            }




            if (src.Next != null)
            {
                Next = new EquipTagTreeNode(src.Next);
                Next.Prev = this;
            }
        }

        public EquipTagTreeNode(EquipmentDataTag tag)
        {
            Tag = tag;

            if (tag.BackwardEquips.Length > 0)
            {
                Child = new EquipTagTreeNode(tag.BackwardEquips[0]);
                Child.Parent = this;

                EquipTagTreeNode prev = Child;

                for (int i = 1; i < tag.BackwardEquips.Length; i++)
                {
                    EquipTagTreeNode add = new EquipTagTreeNode(tag.BackwardEquips[i]);
                    add.Parent = this;
                    add.Prev = prev;
                    prev.Next = add;

                    prev = add;

                }

            }

        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (Tag != null)
                sb.Append(Tag.equipdata.Name);

            if (Prev != null)
            {
                if (Next != null)
                {
                    sb.Append(" 前:" + Prev.Tag.equipdata.Name + " 次:" + Next.Tag.equipdata.Name);
                }
                else
                {
                    sb.Append(" こいつが最後尾");
                }
            }
            else
            {
                if (Next != null)
                {
                    sb.Append(" こいつが先頭");
                }
                else
                {
                    if (Parent != null)
                    {
                        sb.Append(" こいつは単独");
                    }
                    else
                    {
                        sb.Append(" Root");
                    }
                }
            }

            return sb.ToString();

        }

    }

}

