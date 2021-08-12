using System.Collections.Generic;
using UnityEngine;

public class AI
{
    // client只能是1
    private GameInfo gameinfo;
    private Players players;
    private Player self;
    private List<int[]> buildingpriority;
    private List<int> researchpriority;
    private List<int> soldierpriority;
    public void Init(GameInfo info)
    {
        gameinfo = info;
        players = info.players;
        self = players.GetPlayer(1);
        InitBP();
        InitRP();
        InitSP();
    }

    private void InitBP()
    {
        // 初始化建筑优先级
        switch (gameinfo.gamemode)
        {
            case 1:
                buildingpriority = new List<int[]> {
                    new int[] { 3, 4 },
                    new int[] { 4, 2 },
                    new int[] { 1, 1 },
                    new int[] { 12, 2 },
                    new int[] { 18, 1 },
                    new int[] { 7, 1 },
                    new int[] { 15, 1 },
                    new int[] { 22, 1 },
                    new int[] { 25, 1 },
                };
                break;
        }
    }
    private void InitRP()
    {
        // 初始化科技优先级
        switch (gameinfo.gamemode)
        {
            case 1:
                researchpriority = new List<int> { 1, 21, 22, 23, 9, 10 };
                break;
        }
    }

    private void InitSP()
    {
        // 初始化兵种优先级
        switch (gameinfo.gamemode)
        {
            case 1:
                soldierpriority = new List<int> { 13, 12, 11 };
                break;
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

        int buid;
        if (!self.HasBuildings_AI(buildingpriority, out buid))
        {
            int lack;
            BuildingInfo info = gameinfo.buildingmap[buid];
            if (self.HasBuildings(info.pre_building, out lack))
            {
                if(self.food >= info.food && self.iron >= info.iron)
                {
                    for(int col = gameinfo.playercolumn - 1; col >= 0; col--)
                    {
                        for(int row = gameinfo.row - 1; row >= 0; row--)
                        {
                            Building building = self.buildings[row, col];
                            int foodleft, ironleft;
                            building.CheckFoodIronLeft(out foodleft, out ironleft);
                            if (info.collect_food > 0 && foodleft == 0) continue;
                            if (info.collect_iron > 0 && ironleft == 0) continue;
                            if (building.maxstatus == 0)
                            {
                                res = res + CreateSignal_AI(1, new Position(1, row, col), buid);
                                Debug.Log(string.Format("【AI】开始建造：{0}  建造地点 第{1}行 第{2}列)",
                                    info.name, row + 1, col + 1));
                                return res;
                            }
                        }
                    }
                }
            }
        }
        foreach(int reid in researchpriority)
        {
            ResearchInfo info = gameinfo.researchmap[reid];
            if(self.researches[reid].level < info.max)
            {
                if(self.researches[reid].status == info.time)
                {
                    int lack;
                    if(self.HasBuildings(info.pre_building, out lack)){
                        if(self.HasResearches(info.pre_research, out lack))
                        {
                            if (self.food >= info.food && self.iron >= info.iron)
                            {
                                foreach(Building building in self.FindBuildings_AI(info.building))
                                {
                                    if(building.status == building.maxstatus && building.researching == 0)
                                    {
                                        int row = building.row;
                                        int col = building.col;
                                        res = res + CreateSignal_AI(3, new Position(1, row, col), reid);
                                        Debug.Log(string.Format("【AI】开始研发：{0}({1}/{2}  研发地点 第{3}行 第{4}列)",
                                            info.name, self.researches[reid].level, info.max, row + 1, col + 1));
                                        return res;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        foreach(int soid in soldierpriority)
        {
            SoldierInfo info = gameinfo.soldiermap[soid];
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
                                res = res + CreateSignal_AI(5, new Position(1, row, col), soid);
                                Debug.Log(string.Format("【AI】开始招募：{0}  招募地点 第{1}行 第{2}列)",
                                    info.name, row + 1, col + 1));
                                return res;
                            }
                        }
                    }
                }
            }
        }

        return res;
    }
}
