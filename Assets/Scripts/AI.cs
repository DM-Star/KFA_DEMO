using System.Collections.Generic;
using UnityEngine;

public class AI
{
    // clientֻ����1
    private GameInfo gameinfo;
    private Players players;
    private Player self;
    public enum TACTICS {
        // ���ֿ칥
        SHAOLIN,
        // �䵱����
        WUDANG,
        // ȫ������
        QUANZHEN,
        // ���ز��
        KUNLUN, 
        // ��ü���
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
        plan.AddPlan(StagePlan.PlanType.BUILD, 3, 4, 0, 4, 4, 2, 5);   // 4��ũ�������½�
        plan.AddPlan(StagePlan.PlanType.BUILD, 4, 4, 1, 0, 0, 2, 5);   // 4���󳡷����Ͻ�
        plan.AddPlan(StagePlan.PlanType.BUILD, 8, 1, 2, 2, 2, 0, 0);   // 3·�����һ����ǽ
        startplan.AddPlan(StagePlan.PlanType.BUILD, 7, 1, 2, 1, 3, 4, 4);   // 2·��2��һ�����췿
        plan.AddPlan(StagePlan.PlanType.BUILD, 23, 1, 3, 1, 1, 1, 1);   // 2·��2·��1������
        plan.AddPlan(StagePlan.PlanType.BUILD, 23, 1, 3, 3, 3, 1, 1);   // 4·��2·��1������

        plan.AddPlan(StagePlan.PlanType.RESEARCH, 2, 1, 0);    // �����о�1����������

        switch (tactics)
        {
            case TACTICS.SHAOLIN:
                {
                    plan.AddPlan(StagePlan.PlanType.BUILD, 10, 2, 2, 1, 3, 5, 5);   // 2·��1��2������������
                    plan.AddPlan(StagePlan.PlanType.BUILD, 16, 1, 3, 1, 3, 4, 4);   // 2·��2��1���ؾ���
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 5, 8);   // ��6�����ֵ���
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 11, 1, 0);   // �޺�ȭ
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 12, 1, 1);   // ����ȭ
                    break;
                }
            case TACTICS.WUDANG:
                {
                    plan.AddPlan(StagePlan.PlanType.BUILD, 11, 2, 2, 1, 3, 5, 5);   // 2·��1��2���䵱������
                    plan.AddPlan(StagePlan.PlanType.BUILD, 17, 1, 3, 1, 3, 4, 4);   // 2·��2��1��̫�͹�
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 8, 5);   // ��5���䵱����
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 9, 2);   // ��2��̫������
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 1, 1, 0);   // ������
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 17, 1, 1);   // ̫������
                    break;
                }
            case TACTICS.QUANZHEN:
                {
                    plan.AddPlan(StagePlan.PlanType.BUILD, 12, 2, 2, 1, 3, 5, 5);   // 2·��1��2��ȫ��������
                    plan.AddPlan(StagePlan.PlanType.BUILD, 18, 1, 3, 1, 3, 4, 4);   // 2·��2��1��������
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 1, 1, 0);   // ������
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 11, 4);   // ��4��ȫ�����
                    break;
                }
            case TACTICS.KUNLUN:
                {
                    plan.AddPlan(StagePlan.PlanType.BUILD, 13, 2, 2, 1, 3, 5, 5);   // 2·��1��2������������
                    plan.AddPlan(StagePlan.PlanType.BUILD, 19, 1, 3, 1, 3, 4, 4);   // 2·��2��1����̶��
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 1, 1, 0);   // ������
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 15, 4);   // ��4�����ص���
                    break;
                }
            case TACTICS.EMEI:
                {
                    plan.AddPlan(StagePlan.PlanType.BUILD, 14, 2, 2, 1, 3, 5, 5);   // 2·��1��2����ü������
                    plan.AddPlan(StagePlan.PlanType.BUILD, 20, 1, 3, 1, 3, 4, 4);   // 2·��2��1��������
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 19, 2);   // ��2����ü������˼һ��
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 1, 1, 0);   // ������
                    break;
                }

        }
    }
    private void InitMiddlePlan()
    {
        StagePlan plan = middleplan;
        plan.AddPlan(StagePlan.PlanType.BUILD, 3, 2, 0, 4, 4, 0, 5);   // 5·��2ũ��
        plan.AddPlan(StagePlan.PlanType.BUILD, 15, 1, 0, 1, 3, 2, 5);   // ��һ��������
        plan.AddPlan(StagePlan.PlanType.BUILD, 27, 2, 1, 1, 3, 0, 0);   // 24·��1��������̳�ǽ
        plan.AddPlan(StagePlan.PlanType.BUILD, 22, 1, 1, 1, 3, 2, 5);   // ��һ������Ժ
        plan.AddPlan(StagePlan.PlanType.BUILD, 28, 1, 2, 2, 2, 1, 1);   // 3·��2��һ��ά��վ

        middleplan.AddPlan(StagePlan.PlanType.RESEARCH, 9, 1, 0);   // ������

        // ��ʼ���������ȼ�
        switch (tactics)
        {
            case TACTICS.SHAOLIN:
                {
                    plan.AddPlan(StagePlan.PlanType.BUILD, 10, 1, 0, 2, 2, 2, 5);   // 3·��һ��������
                    plan.AddPlan(StagePlan.PlanType.BUILD, 3, 2, 0, 0, 0, 0, 5);   // 1·��2ũ��
                    plan.AddPlan(StagePlan.PlanType.BUILD, 21, 1, 1, 1, 3, 2, 5);   // ��һ��ũ��Ժ
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 13, 1, 0);   // ������ȭ
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 14, 1, 1);   // �׽1��
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 6, 15);   // ��15��������ɮ

                    break;
                }
            case TACTICS.WUDANG:
                {
                    plan.AddPlan(StagePlan.PlanType.BUILD, 4, 2, 0, 0, 0, 0, 5);   // 1·��2��
                    plan.AddPlan(StagePlan.PlanType.BUILD, 17, 1, 0, 1, 3, 2, 5);   // ��һ��̫�͹�
                    plan.AddPlan(StagePlan.PlanType.BUILD, 21, 1, 0, 1, 3, 2, 5);   // ��һ��ũ��Ժ
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 6, 1, 0);   // ��ţ�̼���
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 15, 1, 0);   // �䵱������
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 16, 1, 0);   // �����޼���
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 18, 1, 0);   // ������1��
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 19, 1, 0);   // һ���߰���
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 9, 10);   // ��10��̫������
                    break;
                }
            case TACTICS.QUANZHEN:
                {
                    plan.AddPlan(StagePlan.PlanType.BUILD, 4, 2, 0, 0, 0, 0, 5);   // 1·��2��
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 23, 1, 0);   // һ�Ż�����
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 12, 10);   // ��10��ȫ����ʿ

                    break;
                }
            case TACTICS.KUNLUN:
                {
                    plan.AddPlan(StagePlan.PlanType.BUILD, 4, 2, 0, 0, 0, 0, 5);   // 1·��2��
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 25, 1, 0);   // Ыβ��
                    plan.AddPlan(StagePlan.PlanType.BUILD, 13, 3, 0, 0, 4, 0, 5);   // ������+3
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 26, 1, 1);   // Ѹ�׽���
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 27, 1, 2);   // �ɻ�
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 16, 5);   // ��5��������
                    break;
                }
            case TACTICS.EMEI:
                {
                    plan.AddPlan(StagePlan.PlanType.BUILD, 4, 2, 0, 0, 0, 0, 5);   // 1·��2��
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 30, 1, 0);   // ��а����
                    plan.AddPlan(StagePlan.PlanType.BUILD, 20, 1, 0, 1, 3, 2, 5);   // ��һ��������
                    break;
                }
        }
    }
    private void InitEndPlan()
    {
        StagePlan plan = endplan;
        plan.AddPlan(StagePlan.PlanType.BUILD, 25, 1, 0, 1, 3, 2, 5);   // ��һ��������¾�

        switch (tactics)
        {
            case TACTICS.SHAOLIN:
                {
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 7, 1, 0);   // �����漼��1��
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 37, 5, 0);   // ����ͻ��5��
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 14, 2, 0);   // �׽2��
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 5, 80);   // ��80�����ֵ���
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 6, 20);   // ��20��������ɮ
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 7, 10);   // ��10��ɨ��ɮ

                    break;
                }
            case TACTICS.WUDANG:
                {
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 18, 2, 0);   // ������2��
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 20, 1, 0);   // �����۶�1��
                    plan.AddPlan(StagePlan.PlanType.BUILD, 11, 3, 0, 0, 4, 0, 5);   // ������+3
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 9, 60);   // 60��̫������
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 10, 30);   // 30��������ʦ
                    break;
                }
            case TACTICS.QUANZHEN:
                {
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 21, 1, 0);   // ͬ�齣��
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 22, 1, 0);   // �󱱶���ʽ
                    plan.AddPlan(StagePlan.PlanType.BUILD, 12, 3, 0, 0, 4, 0, 5);   // ������+3
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 12, 40);   // 40����ʿ
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 13, 30);   // 30������
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 14, 20);   // 20����ʦ
                    break;
                }
            case TACTICS.KUNLUN:
                {
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 36, 1, 1);   // ��������
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 16, 40);   // 40��������
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 17, 30);   // 30��Ѹ��
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 18, 20);   // 20���ɻ�
                    break;
                }
            case TACTICS.EMEI:
                {
                    plan.AddPlan(StagePlan.PlanType.BUILD, 14, 3, 0, 0, 4, 0, 5);   // ������+3
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 31, 1, 0);   // �Ⱥ��ն�
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 28, 1, 1);   // ��ü������
                    plan.AddPlan(StagePlan.PlanType.RESEARCH, 29, 1, 1);   // �����Ʒ�
                    plan.AddPlan(StagePlan.PlanType.PRODUCE, 22, 50);   // ֱ�Ӻ��

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
        // typeָ���ź�����
        // 1���죬2ȡ�����죬3�з���4ȡ���з���5��ļ��6ȡ����ļ
        // ����idֻ��1�ֽڣ������������̫�࣬�Ʊ�Ҫ�ĳ�2�ֽڣ�����Parse��صĺ��������޸�
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
                                        Debug.Log(string.Format("��AI����ʼ���죺{0}  ����ص� ��{1}�� ��{2}��)",
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
                                            Debug.Log(string.Format("��AI����ʼ�з���{0}({1}/{2}  �з��ص� ��{3}�� ��{4}��)",
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
                                    Debug.Log(string.Format("��AI����ʼ��ļ��{0}  ��ļ�ص� ��{1}�� ��{2}��)",
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
