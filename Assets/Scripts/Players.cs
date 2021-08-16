using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Research
{
    public int id;
    // 当前研发状态，小于maxstatus研发中，=maxstatus不在研发
    public double status;
    public int maxstatus;
    // 完成研发的次数
    public int level;
    // 如果正在研发，标识出正在哪个建筑中研发
    public int row, col;
    public int food;
    public int iron;
    public Player player;
    public Buff basebuff;   // 全局buff
    public Research(ResearchInfo info, Player p)
    {
        id = info.id;
        maxstatus = info.time;
        status = maxstatus;
        level = 0;
        row = col = -1;
        food = info.food;
        iron = info.iron;
        player = p;
        basebuff = new Buff();
    }
    public void Move(GameInfo gameinfo)
    {
        if (status < maxstatus)
        {
            status += 100.0 / (100 + basebuff.time);
            if (status >= maxstatus)
            {
                player.FinishResearch(id);
                gameinfo.infobar.statuses.Enqueue(new StatusMsg(6, new Position(player.isclient, row, col), id));
                if (player.isclient == gameinfo.client)
                {
                    gameinfo.canvas.ShowMsg(string.Format("{0} 研发完成。", gameinfo.researchmap[id].name));
                }
            }
        }
    }
}
public class Produce
{
    public int id;
    public int status;
    public int maxstatus;
    public int food;
    public int iron;
    public Produce(SoldierInfo info, int needfood, int neediron)
    {
        id = info.id;
        status = 0;
        maxstatus = info.time;
        food = needfood;
        iron = neediron;
    }
    public bool Move()
    {
        // 招募完成后返回true，否则返回false
        if (status < maxstatus)
        {
            status++;
            if (status == maxstatus)
            {
                return true;
            }
        }
        return false;
    }
}
public class Player
{
    public class BuildingStatus
    {
        public int buildingcount;   // 在建
        public int count;           // 拥有
        public BuildingStatus()
        {
            buildingcount = 1;
            count = 0;
        }
    }
    // 区分主客，0主机，1客户端（对应0左边，1右边）
    public int isclient;
    public int food;
    public int iron;
    public Building[,] buildings;
    private SortedDictionary<int, BuildingStatus> buildingcount;  // < 建筑编号, 建筑状态 >
    private Players players;
    private SortedDictionary<int, List<Building>> bumap;          // < 建筑编号, 所有该建筑的坐标列表 >
    public SortedDictionary<int, Buff> buildingbuff;  // 当前建筑的初始buff信息
    public SortedDictionary<int, Buff> soldierbuff;   // 当前士兵的初始buff信息
    public Dictionary<Transform, Unit> units;

    public SortedDictionary<int, Research> researches;
    public Player()
    {
        buildingcount = new SortedDictionary<int, BuildingStatus>();
        researches = new SortedDictionary<int, Research>();
        bumap = new SortedDictionary<int, List<Building>>();
        buildingbuff = new SortedDictionary<int, Buff>();
        soldierbuff = new SortedDictionary<int, Buff>();
        units = new Dictionary<Transform, Unit>();
    }
    public void Init(GameInfo gameinfo, int client)
    {
        int gamemode = gameinfo.gamemode;
        players = gameinfo.players;
        switch (gamemode)
        {
            case 1:
                isclient = client;
                food = 200;
                iron = 200;
                int row = gameinfo.row;
                int col = gameinfo.playercolumn;
                buildings = new Building[row, col];
                for (int r = 0; r < row; r++)
                {
                    for (int c = 0; c < col; c++)
                    {
                        buildings[r, c] = gameinfo.background.buildings[client, r, c];
                        buildings[r, c].player = this;
                    }
                }

                foreach (KeyValuePair<int, ResearchInfo> p in gameinfo.researchmap)
                {
                    researches.Add(p.Key, new Research(p.Value, this));
                }
                break;
        }

        foreach (KeyValuePair<int, BuildingInfo> pair in gameinfo.buildingmap)
        {
            buildingbuff.Add(pair.Key, new Buff(pair.Value.unitinfo));
        }
        foreach (KeyValuePair<int, SoldierInfo> pair in gameinfo.soldiermap)
        {
            soldierbuff.Add(pair.Key, new Buff(pair.Value.unitinfo));
        }

        
    }
    public void GameStart(GameInfo gameinfo)
    {
        if (isclient == 0) BuildFirst(2, 0, gameinfo.buildingmap[1]);
        else if (isclient == 1) BuildFirst(2, gameinfo.playercolumn - 1, gameinfo.buildingmap[1]);
    }
    private void BuildFirst(int row, int col, BuildingInfo info)
    {
        Building building = buildings[row, col];
        building.player = this;
        int id = info.id;
        building.Build(row, col, info, true);
        buildingcount.Add(id, new BuildingStatus());
        buildingcount[id].count = 1;
        buildingcount[id].buildingcount = 0;
        bumap.Add(id, new List<Building>());
        bumap[id].Add(building);
        players.units[row].Add(building.unit);
    }
    public void StartBuild(int row, int col, BuildingInfo info)
    {
        Building building = buildings[row, col];
        building.player = this;
        building.Build(row, col, info);

        food -= (int)(info.food * (1 + buildingbuff[info.id].food / 100f));
        iron -= (int)(info.iron * (1 + buildingbuff[info.id].iron / 100f));

        int id = info.id;
        if (buildingcount.ContainsKey(id))
        {
            buildingcount[id].buildingcount++;
            bumap[id].Add(building);
        }
        else
        {
            buildingcount.Add(id, new BuildingStatus());
            bumap.Add(id, new List<Building>());
            bumap[id].Add(building);
        }
        players.units[row].Add(building.unit);
    }
    public void FinishBuild(int id)
    {
        if (id != 0)
        {
            if (buildingcount.ContainsKey(id))
            {
                buildingcount[id].count++;
                buildingcount[id].buildingcount--;
            }
            else
            {
                Debug.Log("有什么bug出现了(Players.cs  line 302)");
            }
        }

    }
    public void CancelBuild(int row, int col, BuildingInfo info)
    {
        Building building = buildings[row, col];
        int id = building.id;

        // 待测试：升级时被攻击
        double health = (double)building.unit.hp / building.unit.maxhp;
        int getfood = System.Convert.ToInt32(health * (info.food * (1 + buildingbuff[info.id].food / 100f)));
        int getiron = System.Convert.ToInt32(health * (info.iron * (1 + buildingbuff[info.id].iron / 100f)));
        food += getfood;
        iron += getiron;
        if(building.status == building.maxstatus)
        {
            buildingcount[id].count--;
        }
        else
        {
            buildingcount[id].buildingcount--;
        }
        for(int i = 0; i < bumap[id].Count; i++) { 
            if(bumap[id][i].Equals(building))
            {
                bumap[id].RemoveAt(i);
                break;
            }
        }
        players.units[row].Remove(building.unit);
        building.Destroy();
    }
    public void LossBuilding(Building building, GameInfo gameinfo)
    {
        int row = building.row;
        int col = building.col;
        int id = building.id;

        // 待测试：升级时被攻击
        if (building.status == building.maxstatus)
        {
            buildingcount[id].count--;
            // 在研究的科技全部取消
            if(building.researching != 0)
            {
                Research research = researches[building.researching];
                research.status = research.maxstatus;
            }
        }
        else
        {
            buildingcount[id].buildingcount--;
        }
        for (int i = 0; i < bumap[id].Count; i++)
        {
            if (bumap[id][i].Equals(building))
            {
                bumap[id].RemoveAt(i);
                break;
            }
        }
        players.units[row].Remove(building.unit);
        building.Destroy();
        gameinfo.infobar.statuses.Enqueue(new StatusMsg(4, new Position(isclient, row, col), id));
    }
    public void StartResearch(int row, int col, ResearchInfo info)
    {
        // 科技所属建筑应为当前格子建筑
        // 当前建筑未在研发科技
        // 当前科技未在研究
        // 当前科技未达研发次数上限
        int id = info.id;
        researches[id].status = 0;
        researches[id].row = row;
        researches[id].col = col;
        food -= info.food;
        iron -= info.iron;
        buildings[row, col].researching = id;
    }
    public void FinishResearch(int id)
    {
        Building building = buildings[researches[id].row, researches[id].col];
        building.researching = 0;
        researches[id].level++;
        ResearchInfo reinfo = players.gameinfo.researchmap[id];
        List<int> targets;
        List<Unit> immediate = new List<Unit>();
        for (int i = 0; i < reinfo.buffnum; i++)
        {
            ResearchInfo.BuffInfo buff = reinfo.buffinfos[i];
            switch (buff.type)
            {
                case 1: // 建筑
                    {
                        targets = buff.targets;
                        foreach (int target in targets)
                        {
                            foreach (KeyValuePair<int, Buff> pair in buildingbuff)
                            {
                                int buid = pair.Key;
                                if (target == -1 || buid == target) pair.Value.AddBuff(buff);
                            }
                            foreach (KeyValuePair<Transform, Unit> pair in units)
                            {
                                Unit unit = pair.Value;
                                if (unit.type == Unit.Type.BUILDING
                                    && (target == -1 || unit.id == target)) immediate.Add(pair.Value);
                            }
                        }
                        break;
                    }
                case 2: // 兵种
                    {
                        targets = buff.targets;
                        foreach (int target in targets)
                        {
                            foreach(KeyValuePair<int, Buff> pair in soldierbuff)
                            {
                                int soid = pair.Key;
                                if(target == -1 || soid == target) pair.Value.AddBuff(buff);
                            }
                            foreach (KeyValuePair<Transform, Unit> pair in units)
                            {
                                Unit unit = pair.Value;
                                if (unit.type == Unit.Type.SOLDIER
                                    && (target == -1 || unit.id == target)) immediate.Add(pair.Value);
                            }
                        }
                        break;
                    }
                case 3: // 科技
                    {
                        targets = buff.targets;
                        if (targets[0] != -1)
                        {
                            foreach (int target in targets)
                            {
                                researches[target].basebuff.AddBuff(buff);
                            }
                        }
                        else if (targets[0] == -1)
                        {
                            foreach (KeyValuePair<int, Research> pair in researches)
                            {
                                pair.Value.basebuff.AddBuff(buff);
                            }
                        }
                        break;
                    }
                case 4: // 英雄
                    break;
                case 5: // 建筑类型
                    {
                        targets = buff.targets;
                        foreach (int target in targets)
                        {
                            foreach (KeyValuePair<int, Buff> pair in buildingbuff)
                            {
                                BuildingInfo buinfo = players.gameinfo.buildingmap[pair.Key];
                                if (target == -1 || buinfo.type == target)
                                {
                                    pair.Value.AddBuff(buff);
                                }
                            }
                            foreach (KeyValuePair<Transform, Unit> pair in units)
                            {
                                Unit unit = pair.Value;
                                if (unit.type == Unit.Type.BUILDING
                                    && (target == -1 || unit.building.buinfo.type == target)) immediate.Add(pair.Value);
                            }
                        }
                        break;
                    }
                case 6: // 门派
                    {
                        targets = buff.targets;
                        foreach (int target in targets)
                        {
                            foreach (KeyValuePair<int, Buff> pair in soldierbuff)
                            {
                                SoldierInfo soinfo = players.gameinfo.soldiermap[pair.Key];
                                if (target == -1 || soinfo.job == target)
                                {
                                    pair.Value.AddBuff(buff);
                                }
                            }
                            foreach (KeyValuePair<Transform, Unit> pair in units)
                            {
                                Unit unit = pair.Value;
                                if (unit.type == Unit.Type.SOLDIER
                                    && (target == -1 || unit.soldier.soinfo.job == target)) immediate.Add(pair.Value);
                            }
                        }
                        break;
                    }
            }
            for (int e = 0; e < buff.effects.Count; e++)
            {
                foreach (Unit unit in immediate)
                {
                    switch (buff.effects[e])
                    {
                        case 12:    // 加血上限，即时生效
                            {
                                unit.maxhp += buff.values[e];
                                unit.hp += buff.values[e];
                                if (unit.type == Unit.Type.SOLDIER) unit.soldier.SetHpBar();

                                break;
                            }
                        case 33:    // 加技能，更新按技能找单位表
                            {
                                foreach (string skill in buff.skills) players.AddUnitToSkill(skill, unit);
                                break;
                            }
                    }
                }
            }
        }
        Debug.Log("research complete");
        players.gameinfo.skills.TriggerSkills(building.unit, EVENT.FINISH_RESEARCH, reinfo);
    }
    public void CancelResearch(int id)
    {
        Research research = researches[id];
        food += research.food;
        iron += research.iron;
        buildings[research.row, research.col].researching = 0;
        research.status = research.maxstatus;
    }
    public int StartProduce(Building building, SoldierInfo info)
    {
        // 返回开始后的建造队列长度
        int needfood = (int)(info.food * (1 + soldierbuff[info.id].food / 100f));
        int neediron = (int)(info.iron * (1 + soldierbuff[info.id].iron / 100f));
        food -= needfood;
        iron -= neediron;
        return building.AddProduce(info, needfood, neediron);
    }
    public void FinishProduce(Building building, int id)
    {
        players.CreateSoldier(building, id, isclient);
    }
    public int CancelProduce(Building building)
    {
        // 返回招募队列剩余士兵数
        // food iron的恢复在building中执行
        return building.CancelProduce();
    }
    public void Move(GameInfo gameinfo)
    {
        foreach (Building building in buildings)
        {
            if (building.id != 0)
            {
                building.Move(gameinfo);
            }
        }
    }
    public bool HasBuildings(List<int> prebuildings, out int lack)
    {
        lack = 0;
        foreach (int id in prebuildings)
        {
            if (id != 0)
            {
                if (!buildingcount.ContainsKey(id) || buildingcount[id].count == 0)
                {
                    lack = id;
                    return false;
                }
            }
        }
        return true;
    }
    public bool HasBuilding(int prebuilding)
    {
        if (prebuilding != 0)
        {
            if (!buildingcount.ContainsKey(prebuilding) || buildingcount[prebuilding].count == 0)
            {
                return false;
            }
        }
        return true;
    }
    public int GetBuildingsNum(List<int> ids)
    {
        int num = 0;
        foreach(int id in ids)
        {
            if(id != 0)
            {
                if (buildingcount.ContainsKey(id))
                {
                    num += buildingcount[id].count;
                }
            }
        }
        return num;
    }
    public bool HasBuildings_AI(List<int[]> prebuildings, out int lack)
    {
        // AI专用函数，判断是否拥有某座建筑或者在造某座建筑
        lack = 0;
        foreach (int[] tobuild in prebuildings)
        {
            int id = tobuild[0];
            int need = tobuild[1];
            if (id != 0)
            {
                if (!buildingcount.ContainsKey(id) || (buildingcount[id].count + buildingcount[id].buildingcount) < need)
                {
                    lack = id;
                    return false;
                }
            }
        }
        return true;
    }
    public List<Building> FindBuildings_AI(int id)
    {
        if (bumap.ContainsKey(id))
        {
            return bumap[id];
        }
        return null;
    }
    public bool HasResearches(List<int> preresearched, out int lack)
    {
        lack = 0;
        foreach (int id in preresearched)
        {
            if (id != 0)
            {
                if (!researches.ContainsKey(id) || researches[id].level <= 0)
                {
                    lack = id;
                    return false;
                }
            }
        }
        return true;
    }
    public bool HasResearch(int preresearch)
    {
        if (preresearch != 0)
        {
            if (!researches.ContainsKey(preresearch) || researches[preresearch].level <= 0)
            {
                return false;
            }
        }
        return true;
    }
}

public class Players : MonoBehaviour
{
    public int gamemode;
    private Player[] players;
    public List<List<Unit>> units;
    public List<Projection> projections;
    public GameInfo gameinfo;
    // 士兵实例
    public Soldier soldierins;
    public SortedDictionary<string, List<Unit>> skillunit;
    private void Awake()
    {
        projections = new List<Projection>();
        skillunit = new SortedDictionary<string, List<Unit>>();
    }
    // 0返回主机玩家，1返回客户端玩家
    public Player GetPlayer(int client)
    {
        return players[client];
    }
    public void Init(GameInfo info)
    {
        gameinfo = info;
        gamemode = gameinfo.gamemode;
        units = new List<List<Unit>>(5);
        for(int i = 0; i < 5; i++)
        {
            units.Add(new List<Unit>());
        }

        // 多人游戏时可能更多
        players = new Player[2];
        for (int i = 0; i < 2; i++)
        {
            players[i] = new Player();
            players[i].Init(gameinfo, i);
        }
    }
    // 返回输家
    public int Move()
    {
        for (int cl = 0; cl < 2; cl++)
        {
            Player player = players[cl];
            player.Move(gameinfo);
        }
        foreach (List<Unit> line in units)
        {
            foreach(Unit unit in line)
            {
                unit.Move();
            }
            line.Sort();

            // 处理出界的单位
            while(line.Count > 0 && line[0].transform.position.x < -90)
            {
                Destroy(line[0].transform.gameObject);
                line.RemoveAt(0);
            }
            while (line.Count > 0 && line[line.Count - 1].transform.position.x > 90)
            {
                Destroy(line[line.Count - 1].transform.gameObject);
                line.RemoveAt(line.Count - 1);
            }
        }
        // 所有飞行物飞行
        if (projections.Count > 0)
        {
            List<Projection> todestroy = new List<Projection>();
            foreach (Projection projection in projections)
            {
                if (projection.crossnum > 0 && projection.distance > 0)
                {
                    projection.Move();
                }
                else
                {
                    todestroy.Add(projection);
                }
            }
            foreach (Projection projection in todestroy)
            {
                projections.Remove(projection);
                projection.DestroySelf();
            }
        }
        // 死亡判断
        foreach (List<Unit> line in units)
        {
            List<Building> todestroy = new List<Building>();
            List<Soldier> tokill = new List<Soldier>();
            foreach(Unit unit in line)
            {
                if(unit.hp <= 0)
                {
                    if(unit.type == Unit.Type.BUILDING)
                    {
                        todestroy.Add(unit.building);
                    }
                    else if(unit.type == Unit.Type.SOLDIER)
                    {
                        tokill.Add(unit.soldier);
                    }
                }
            }
            foreach (Building building in todestroy)
            {
                // 插入时机：建筑被敌人摧毁
                gameinfo.skills.TriggerSkills(building.unit, EVENT.DEATH);
                RemoveUnitFromSkill(building.unit);
                DesrtoyBuilding(building);
                if(building.player.GetBuildingsNum(gameinfo.corelist) <= 0)
                {
                    return building.player.isclient;
                }
            }
            foreach (Soldier solider in tokill)
            {
                // 插入时机：士兵被敌人杀死
                gameinfo.skills.TriggerSkills(solider.unit, EVENT.DEATH);
                line.Remove(solider.unit);
                solider.unit.alive = false;
                RemoveUnitFromSkill(solider.unit);
                Destroy(solider.gameObject);
            }
        }

        gameinfo.background.UpdateMist(players[gameinfo.client].units);

        return -1;
    }
    public void AddUnitToSkill(string skill, Unit unit)
    {
        if (!skillunit.ContainsKey(skill)) skillunit.Add(skill, new List<Unit>());
        skillunit[skill].Add(unit);
    }
    private void RemoveUnitFromSkill(Unit unit)
    {
        foreach(string skill in unit.basebuff.skills) skillunit[skill].Remove(unit);
        foreach (string skill in unit.buff.skills) skillunit[skill].Remove(unit);
    }
    public Building GetBuilding(Position pos)
    {
        if (pos.right != -1)
        {
            return players[pos.right].buildings[pos.row, pos.col];
        }
        else return null;
    }
    public void CreateSoldier(Building building, int id, int client)
    {
        SoldierInfo info = gameinfo.soldiermap[id];
        // public static Object Instantiate(Object original, Vector3 position, Quaternion rotation, Transform parent);
        int r = building.row;
        int c = building.col;
        Soldier s = Instantiate(soldierins, 
            gameinfo.background.Pos2Pix(new Vector3(client, r, c)), 
            new Quaternion(), transform);
        s.Init(gameinfo, id, client, r);  // 之后要改，根据兵种id查询投射物。
        units[r].Add(s.unit);
    }
    public void DesrtoyBuilding(Building building)
    {
        Player owner = building.player;
        owner.LossBuilding(building, gameinfo);
    }
    public int GetCoreNum(int client)
    {
        Player player = players[client];
        return player.GetBuildingsNum(gameinfo.corelist);
    }
    public void GameStart(GameInfo gameinfo)
    {
        for(int i = 0; i < 2; i++)
        {
            players[i].GameStart(gameinfo);
        }
    }
}
