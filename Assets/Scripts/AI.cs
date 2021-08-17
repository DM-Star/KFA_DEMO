using System.Collections.Generic;
using UnityEngine;

public class AI
{
    // client只能是1
    private GameInfo gameinfo;
    private Players players;
    private Player self;
    public enum TACTICS {
        // 少林快攻
        SHAOLIN,
        // 武当后期
        WUDANG,
        // 全真中期
        QUANZHEN,
        // 昆仑拆家
        KUNLUN, 
        // 峨眉后悔
        EMEI
    }
    private TACTICS tactics;

    public enum STAGE
    {
        START, MIDDLE, END
    }
    private STAGE stage;
    public void Init(GameInfo info)
    {
        gameinfo = info;
        players = info.players;
        self = players.GetPlayer(1);
        tactics = (TACTICS)Random.Range(0, System.Enum.GetValues(typeof(TACTICS)).Length);
        startplan = new StagePlan();
        middleplan = new StagePlan();
        endplan = new StagePlan();
        stage = STAGE.START;
        InitStartPlan();
        InitMiddlePlan();
        InitEndPlan();
    }

    public class StagePlan
    {
        public List<List<Plan>> buildplans;
        public List<List<Plan>> researchplans;
        public List<Plan> produceplans;
        public int tocomplete;
        public enum PlanType
        {
            BUILD, RESEARCH, PRODUCE
        }
        public StagePlan()
        {
            buildplans = new List<List<Plan>>();
            researchplans = new List<List<Plan>>();
            produceplans = new List<Plan>();
            tocomplete = 0;
        }
        public void AddPlan(PlanType type, int id, int need, int priority = 0, int rowup = 0, int rowdown = 4, int colleft = 0, int colright = 5)
        {
            List<Plan> toadd = null;
            switch (type) {
                case PlanType.BUILD:
                    while (priority >= buildplans.Count) buildplans.Add(new List<Plan>());
                    toadd = buildplans[priority];
                    break;
                case PlanType.RESEARCH:
                    while (priority >= researchplans.Count) researchplans.Add(new List<Plan>());
                    toadd = researchplans[priority];
                    break;
                case PlanType.PRODUCE:
                    toadd = produceplans;
                    break;
            }
            toadd.Add(new Plan(id, need, rowup, rowdown, colleft, colright));
            tocomplete += need;
        }
    }
    private StagePlan startplan, middleplan, endplan;

    public class Plan
    {
        public int id;
        public int need;
        public int pos1r, pos1c, pos2r, pos2c;
        public Plan(int i, int need, int row1 = 0, int row2 = 4, int col1 = 0, int col2 = 5)
        {
            id = i;
            this.need = need;
            pos1r = row1;
            pos1c = col1;
            pos2r = row2;
            pos2c = col2;
        }
    }


    private void InitStartPlan()
    {
        StagePlan plan = startplan;
        plan.AddPlan(StagePlan.PlanType.BUILD, 3, 4, 0, 4, 4, 2, 5);   // 4个农场放右下角
        plan.AddPlan(StagePlan.PlanType.BUILD, 4, 4, 1, 0, 0, 2, 5);   // 4个矿场放右上角
        plan.AddPlan(StagePlan.PlanType.BUILD, 8, 1, 2, 2, 2, 0, 0);   // 3路最左放一个城墙
        startplan.AddPlan(StagePlan.PlanType.BUILD, 7, 1, 2, 1, 3, 4, 4);   // 2路右2放一个锻造房
        plan.AddPlan(StagePlan.PlanType.BUILD, 23, 1, 3, 1, 1, 1, 1);   // 2路左2路放1个箭塔
        plan.AddPlan(StagePlan.PlanType.BUILD, 23, 1, 3, 3, 3, 1, 1);   // 4路左2路放1个箭塔

        plan.AddPlan(StagePlan.PlanType.RESEARCH, 2, 1, 0);    // 优先研究1次锐器制造

        switch (tactics)
        {
            case TACTICS.SHAOLIN:
                {
                    plan.AddPlan(StagePlan.PlanType.BUILD, 10, 2, 2, 1, 3, 5, 5);   // 2路右1放2个少林练功房
                    plan.AddPlan(StagePlan.PlanType.BUILD, 16, 1, 3, 1, 3, 4, 4);   // 2路右2放1个藏经阁
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 5, 8);   // 造6个少林弟子
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 11, 1, 0);   // 罗汉拳
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 12, 1, 1);   // 七星拳
                    break;
                }
            case TACTICS.WUDANG:
                {
                    plan.AddPlan(StagePlan.PlanType.BUILD, 11, 2, 2, 1, 3, 5, 5);   // 2路右1放2个武当练功房
                    plan.AddPlan(StagePlan.PlanType.BUILD, 17, 1, 3, 1, 3, 4, 4);   // 2路右2放1个太和宫
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 8, 5);   // 造5个武当弟子
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 9, 2);   // 造2个太极剑客
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 1, 1, 0);   // 铸剑术
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 17, 1, 1);   // 太极剑法
                    break;
                }
            case TACTICS.QUANZHEN:
                {
                    plan.AddPlan(StagePlan.PlanType.BUILD, 12, 2, 2, 1, 3, 5, 5);   // 2路右1放2个全真练功房
                    plan.AddPlan(StagePlan.PlanType.BUILD, 18, 1, 3, 1, 3, 4, 4);   // 2路右2放1个重阳宫
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 1, 1, 0);   // 铸剑术
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 11, 4);   // 造4个全真弟子
                    break;
                }
            case TACTICS.KUNLUN:
                {
                    plan.AddPlan(StagePlan.PlanType.BUILD, 13, 2, 2, 1, 3, 5, 5);   // 2路右1放2个昆仑练功房
                    plan.AddPlan(StagePlan.PlanType.BUILD, 19, 1, 3, 1, 3, 4, 4);   // 2路右2放1个龙潭寺
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 1, 1, 0);   // 铸剑术
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 15, 4);   // 造4个昆仑弟子
                    break;
                }
            case TACTICS.EMEI:
                {
                    plan.AddPlan(StagePlan.PlanType.BUILD, 14, 2, 2, 1, 3, 5, 5);   // 2路右1放2个峨眉练功房
                    plan.AddPlan(StagePlan.PlanType.BUILD, 20, 1, 3, 1, 3, 4, 4);   // 2路右2放1个华藏寺
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 19, 2);   // 造2个峨眉弟子意思一下
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 1, 1, 0);   // 铸剑术
                    break;
                }

        }
    }
    private void InitMiddlePlan()
    {
        StagePlan plan = middleplan;
        plan.AddPlan(StagePlan.PlanType.BUILD, 3, 2, 0, 4, 4, 0, 5);   // 5路加2农场
        plan.AddPlan(StagePlan.PlanType.BUILD, 15, 1, 0, 1, 3, 2, 5);   // 造一个军机处
        plan.AddPlan(StagePlan.PlanType.BUILD, 27, 2, 1, 1, 3, 0, 0);   // 24路左1放两个尖刺城墙
        plan.AddPlan(StagePlan.PlanType.BUILD, 22, 1, 1, 1, 3, 2, 5);   // 造一个工科院
        plan.AddPlan(StagePlan.PlanType.BUILD, 28, 1, 2, 2, 2, 1, 1);   // 3路左2放一个维修站

        middleplan.AddPlan(StagePlan.PlanType.RESEARCH, 9, 1, 0);   // 减震技术

        // 初始化建筑优先级
        switch (tactics)
        {
            case TACTICS.SHAOLIN:
                {
                    plan.AddPlan(StagePlan.PlanType.BUILD, 10, 1, 0, 2, 2, 2, 5);   // 3路加一个练功房
                    plan.AddPlan(StagePlan.PlanType.BUILD, 3, 2, 0, 0, 0, 0, 5);   // 1路加2农场
                    plan.AddPlan(StagePlan.PlanType.BUILD, 21, 1, 1, 1, 3, 2, 5);   // 造一个农科院
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 13, 1, 0);   // 少林五拳
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 14, 1, 1);   // 易筋经1次
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 6, 15);   // 造15个少林武僧

                    break;
                }
            case TACTICS.WUDANG:
                {
                    plan.AddPlan(StagePlan.PlanType.BUILD, 4, 2, 0, 0, 0, 0, 5);   // 1路加2矿场
                    plan.AddPlan(StagePlan.PlanType.BUILD, 17, 1, 0, 1, 3, 2, 5);   // 加一个太和宫
                    plan.AddPlan(StagePlan.PlanType.BUILD, 21, 1, 0, 1, 3, 2, 5);   // 造一个农科院
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 6, 1, 0);   // 纯牛奶技术
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 15, 1, 0);   // 武当九阳功
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 16, 1, 0);   // 纯阳无极功
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 18, 1, 0);   // 梯云纵1次
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 19, 1, 0);   // 一手七暗器
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 9, 10);   // 造10个太极剑客
                    break;
                }
            case TACTICS.QUANZHEN:
                {
                    plan.AddPlan(StagePlan.PlanType.BUILD, 4, 2, 0, 0, 0, 0, 5);   // 1路加2矿场
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 23, 1, 0);   // 一炁化三清
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 12, 10);   // 造10个全真勇士

                    break;
                }
            case TACTICS.KUNLUN:
                {
                    plan.AddPlan(StagePlan.PlanType.BUILD, 4, 2, 0, 0, 0, 0, 5);   // 1路加2矿场
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 25, 1, 0);   // 蝎尾钩
                    plan.AddPlan(StagePlan.PlanType.BUILD, 13, 3, 0, 0, 4, 0, 5);   // 练功房+3
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 26, 1, 1);   // 迅雷剑法
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 27, 1, 2);   // 飞花
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 16, 5);   // 造5个暗器手
                    break;
                }
            case TACTICS.EMEI:
                {
                    plan.AddPlan(StagePlan.PlanType.BUILD, 4, 2, 0, 0, 0, 0, 5);   // 1路加2矿场
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 30, 1, 0);   // 辟邪剑法
                    plan.AddPlan(StagePlan.PlanType.BUILD, 20, 1, 0, 1, 3, 2, 5);   // 加一个华藏寺
                    break;
                }
        }
    }
    private void InitEndPlan()
    {
        StagePlan plan = endplan;
        plan.AddPlan(StagePlan.PlanType.BUILD, 25, 1, 0, 1, 3, 2, 5);   // 造一个中央军事局

        switch (tactics)
        {
            case TACTICS.SHAOLIN:
                {
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 7, 1, 0);   // 方便面技术1次
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 37, 5, 0);   // 基因突变5次
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 14, 2, 0);   // 易筋经2次
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 5, 80);   // 造80个少林弟子
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 6, 20);   // 造20个少林武僧
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 7, 10);   // 造10个扫地僧

                    break;
                }
            case TACTICS.WUDANG:
                {
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 18, 2, 0);   // 梯云纵2次
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 20, 1, 0);   // 三花聚顶1次
                    plan.AddPlan(StagePlan.PlanType.BUILD, 11, 3, 0, 0, 4, 0, 5);   // 练功房+3
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 9, 60);   // 60个太极剑客
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 10, 30);   // 30个暗器大师
                    break;
                }
            case TACTICS.QUANZHEN:
                {
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 21, 1, 0);   // 同归剑法
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 22, 1, 0);   // 大北斗七式
                    plan.AddPlan(StagePlan.PlanType.BUILD, 12, 3, 0, 0, 4, 0, 5);   // 练功房+3
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 12, 40);   // 40个勇士
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 13, 30);   // 30个剑客
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 14, 20);   // 20个宗师
                    break;
                }
            case TACTICS.KUNLUN:
                {
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 36, 1, 1);   // 浸毒暗器
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 16, 40);   // 40个暗器手
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 17, 30);   // 30个迅雷
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 18, 20);   // 20个飞花
                    break;
                }
            case TACTICS.EMEI:
                {
                    plan.AddPlan(StagePlan.PlanType.BUILD, 14, 3, 0, 0, 4, 0, 5);   // 练功房+3
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 31, 1, 0);   // 慈航普度
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 28, 1, 1);   // 峨眉九阳功
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 29, 1, 1);   // 伏虎掌法
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 22, 50);   // 直接后悔

                    break;
                }
        }
    }

    private char Int2Char(int i)
    {
        return (char)('A' + i);
    }
    private string CreateSignal_AI(int type, Position pos, int id = 0)
    {
        // type指明信号类型
        // 1建造，2取消建造，3研发，4取消研发，5招募，6取消招募
        // 这里id只有1字节，如果兵种数量太多，势必要改成2字节，这样Parse相关的函数都得修改
        string res = string.Format("{0}{1}{2}{3}{4}",
            Int2Char(type),
            Int2Char(pos.right),
            Int2Char(pos.row),
            Int2Char(pos.col),
            Int2Char(id));
        return res;
    }


    public string Operator()
    {
        string res = "000";
        StagePlan stageplan = null;
        switch (stage)
        {
            case STAGE.START:
                {
                    if(startplan.tocomplete > 0) stageplan = startplan;
                    else
                    {
                        stage = STAGE.MIDDLE;
                        stageplan = middleplan;
                    }
                    break;
                }
            case STAGE.MIDDLE:
                {
                    if(middleplan.tocomplete > 0) stageplan = middleplan;
                    else
                    {
                        stage = STAGE.END;
                        stageplan = endplan;
                    }
                    break;
                }
            case STAGE.END:
                {
                    stageplan = endplan;
                    break;
                }
        }

        foreach(List<Plan> plans in stageplan.buildplans)
        {
            if (plans.Count > 0)
            {
                Plan plan = plans[Random.Range(0, plans.Count)];
                BuildingInfo info = gameinfo.buildingmap[plan.id];
                int lack;
                if (self.HasBuildings(info.pre_building, out lack))
                {
                    if (self.HasResearches(info.pre_research, out lack))
                    {
                        if (self.food >= info.food && self.iron >= info.iron)
                        {
                            for (int col = plan.pos2c; col >= plan.pos1c; col--)
                            {
                                for (int row = plan.pos1r; row <= plan.pos2r; row++)
                                {
                                    Building building = self.buildings[row, col];
                                    if (building.maxstatus == 0)
                                    {
                                        res = res + CreateSignal_AI(1, new Position(1, row, col), plan.id);
                                        Debug.Log(string.Format("【AI】开始建造：{0}  建造地点 第{1}行 第{2}列)",
                                            info.name, row + 1, col + 1));
                                        stageplan.tocomplete--;
                                        plan.need--;
                                        if (plan.need == 0) 
                                            plans.Remove(plan);
                                        return res;
                                    }
                                }
                            }
                        }
                    }
                }
                break;
            }
        }

        foreach (List<Plan> plans in stageplan.researchplans)
        {
            if (plans.Count > 0)
            {
                Plan plan = plans[Random.Range(0, plans.Count)];
                ResearchInfo info = gameinfo.researchmap[plan.id];
                if (self.researches[plan.id].level < plan.need)
                {
                    if (self.researches[plan.id].status == info.time)
                    {
                        int lack;
                        if (self.HasBuildings(info.pre_building, out lack))
                        {
                            if (self.HasResearches(info.pre_research, out lack))
                            {
                                if (self.food >= info.food && self.iron >= info.iron)
                                {
                                    foreach (Building building in self.FindBuildings_AI(info.building))
                                    {
                                        if (building.status == building.maxstatus && building.researching == 0)
                                        {
                                            int row = building.row;
                                            int col = building.col;
                                            res = res + CreateSignal_AI(3, new Position(1, row, col), plan.id);
                                            Debug.Log(string.Format("【AI】开始研发：{0}({1}/{2}  研发地点 第{3}行 第{4}列)",
                                                info.name, self.researches[plan.id].level, info.max, row + 1, col + 1));
                                            stageplan.tocomplete--;
                                            plan.need--;
                                            if (plan.need == 0) plans.Remove(plan);
                                            return res;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                break;
            }
        }

        {
            List<Plan> plans = stageplan.produceplans;
            if (plans.Count > 0)
            {
                Plan plan = plans[Random.Range(0, plans.Count)];
                SoldierInfo info = gameinfo.soldiermap[plan.id];
                int lack;
                if (self.HasBuildings(info.pre_building, out lack))
                {
                    if (self.HasResearches(info.pre_research, out lack))
                    {
                        if (self.food >= info.food && self.iron >= info.iron)
                        {
                            foreach (Building building in self.FindBuildings_AI(info.building))
                            {
                                if (building.status == building.maxstatus && building.producing.Count == 0)
                                {
                                    int row = building.row;
                                    int col = building.col;
                                    res = res + CreateSignal_AI(5, new Position(1, row, col), plan.id);
                                    Debug.Log(string.Format("【AI】开始招募：{0}  招募地点 第{1}行 第{2}列)",
                                        info.name, row + 1, col + 1));
                                    stageplan.tocomplete--;
                                    plan.need--;
                                    if (plan.need == 0) plans.Remove(plan);
                                    return res;
                                }
                            }
                        }
                    }
                }
            }
        }
   

        return res;
    }
}
