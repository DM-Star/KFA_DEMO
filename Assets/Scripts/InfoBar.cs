using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClickMsg
{
    // 按钮类型：1建筑类型按钮 2建筑按钮 3科技按钮 4格子 5兵种按钮
    public int type;
    public int id;
    public Position pos;
    public ClickMsg(int btntype, int i)
    {
        type = btntype;
        id = i;
    }
    public ClickMsg(Position p)
    {
        type = 4;
        pos = p;
    }
}
public class StatusMsg
{
    // 接受建筑、科技状态更迭信息
    // 消息类型：1开始建造 2建造完毕 3取消建造 4建筑被毁
    // 5开始研发 6研发完毕 7中止研发
    // 8开始招募（之前招募队列为空） 9招募队列全部招募完毕 10招募队列完全清空
    public int msg; 
    public Position pos;
    public int id;  // 在某些情况需要用，例如拆除时，要记录被拆除的编号
    public StatusMsg(int m, Position p)
    {
        msg = m;
        pos = new Position(p);
    }
    public StatusMsg(int m, Position p, int i)
    {
        msg = m;
        pos = new Position(p);
        id = i;
    }
}
public class BtnStatus
{
    // 按钮状态
    public int buildingtype;// 玩家当前选定的建筑类型按钮对应的建筑类型id，0表示未选中
    public int building;    // 玩家当前选定的建筑按钮对应的建筑id，0表示未选中
    public int research;    // 玩家当前选定的科技按钮对应的科技id，0表示未选中
    public int soldier;     // 玩家当前选定的士兵按钮对应的科技id，0表示未选中
    public Position grid;   // 玩家当前选中的格子，right = -1表示未选中
    public BtnStatus()
    {
        buildingtype = 0;
        building = 0;
        research = 0;
        soldier = 0;
        grid = new Position(-1, -1, -1);
    }
}
public class InfoBar : MonoBehaviour
{
    public GameInfo gameinfo;
    public BackGround background;
    public Cvs canvas;
    public Sot socket;
    // 用于构造实例
    public BtnBuildingType btnbuildingtype;
    public BtnBuilding btnbuilding;
    public BtnBuilding btnresearch;
    public BtnBuilding btnsoldier;

    // 按下按钮的队列
    public Queue<ClickMsg> clicked;
    // 建筑、科技状态更迭
    public Queue<StatusMsg> statuses;

    // 1核心 2资源 3军团 4功能 5研发
    // 未来可能会有第六种建筑类别，很遥远
    private List<BtnBuildingType> btnbuildingtypes;
    private SortedDictionary<int, List<BtnBuilding>> btnbuildings;
    private SortedDictionary<int, List<BtnBuilding>> btnresearchs;
    private SortedDictionary<int, List<BtnBuilding>> btnsoldiers;
    // 科技id映射到科技按钮，用于在按钮上展示科研进度
    private SortedDictionary<int, BtnBuilding> btnresearchmap;
    private SortedDictionary<int, BtnBuilding> btnsoldiermap;

    private Image avatar;
    public Bar buildbar;
    public Bar hpbar;
    public Text buildbartext;
    public Text hpbartext;

    private GameObject operationbar;
    private GameObject progressbar;
    private GameObject resourcebar;
    private GameObject btnconfirm;
    private GameObject btncancel;

    // Start is called before the first frame update
    void Awake()
    {
        clicked = new Queue<ClickMsg>();
        statuses = new Queue<StatusMsg>();
        opbarpre = new GameObject[2];
    }
    // Update is called once per frame
    void Update()
    {
        // 处理建筑类型按钮按下事件
        BtnClicked();
        // 处理建筑、科技状态更迭
        StatusChange();
        UpdateProgressBar();
        ShowResourceInfo();
        ShowAvatar();
    }
    // 接收网络帧信号
    public void RecvSignal(int type, Position pos, int id)
    {
        Player player = gameinfo.players.GetPlayer(pos.right);
        Building target = gameinfo.players.GetBuilding(pos);
        int row = pos.row;
        int col = pos.col;
        bool self = (pos.right == gameinfo.client);
        switch (type)
        {
            case 1:     // 建造
                {
                    BuildingInfo tobuild = gameinfo.buildingmap[id];
                    player.StartBuild(row, col, tobuild);
                    statuses.Enqueue(new StatusMsg(1, pos, id));
                    if (self)
                    {
                        Log(string.Format("开始建造：{0}，预计耗时：{1}秒。", tobuild.name, tobuild.time / 50));
                    }
                    break;
                }
            case 2:     // 取消建造
                {
                    if (target.status < target.maxstatus)
                    {
                        // 取消建造
                        statuses.Enqueue(new StatusMsg(3, pos, id));
                        BuildingInfo buildingInfo = gameinfo.buildingmap[id];
                        player.CancelBuild(row, col, buildingInfo);
                        if (self)
                        {
                            Log(string.Format("取消建造：{0}。", buildingInfo.name));
                        }
                    }
                    else
                    {
                        // 摧毁建筑
                        statuses.Enqueue(new StatusMsg(4, pos, id));
                        BuildingInfo buildingInfo = gameinfo.buildingmap[id];
                        player.CancelBuild(row, col, buildingInfo);
                        if (self)
                        {
                            Log(string.Format("拆除建筑：{0}。", buildingInfo.name));
                        }
                    }
                    break;
                }
            case 3:     // 研发
                {
                    ResearchInfo toresearch = gameinfo.researchmap[id];
                    player.StartResearch(row, col, toresearch);
                    statuses.Enqueue(new StatusMsg(5, pos, id));
                    if (self)
                    {
                        Log(string.Format("开始研发：{0}，预计耗时：{1}秒。", toresearch.name,
                            (toresearch.time * (100 + player.researches[id].basebuff.time) / 100f / 50).ToString("0.0"))
                            );
                    }
                    break;
                }
            case 4:     // 取消研发
                {
                    statuses.Enqueue(new StatusMsg(7, pos, id));
                    player.CancelResearch(id);
                    if (self)
                    {
                        Log(string.Format("取消研发：{0}。", gameinfo.researchmap[id].name));
                    }
                    break;
                }
            case 5:     // 招募
                {
                    SoldierInfo toproduce = gameinfo.soldiermap[id];
                    statuses.Enqueue(new StatusMsg(11, pos, id));
                    int qcount = player.StartProduce(target, toproduce);
                    if (qcount == 1)
                    {
                        // 如果当前队列只有1个招募士兵，则发送消息8
                        // 其他的表现形式待定
                        statuses.Enqueue(new StatusMsg(8, pos, id));
                        if (self)
                        {
                            Log(string.Format("开始招募：{0}，预计耗时：{1}秒。", toproduce.name, toproduce.time / 50));
                        }
                    }
                    else
                    {
                        if (self)
                        {
                            Log(string.Format("{0} 加入招募队列，队列前方还有 {1} 个招募任务。", toproduce.name, qcount - 1));
                        }
                    }
                    break;
                }
            case 6:     // 取消招募
                {
                    string name = gameinfo.soldiermap[target.producing[target.producing.Count - 1].id].name;
                    statuses.Enqueue(new StatusMsg(12, pos, id));
                    int qcount = player.CancelProduce(target);
                    if (qcount == 0)
                    {
                        // 如果招募队列清空，发送信息10
                        statuses.Enqueue(new StatusMsg(10, pos));
                        if (self)
                        {
                            Log(string.Format("取消招募：{0}。", name, qcount));
                        }
                    }
                    else
                    {
                        if (self)
                        {
                            Log(string.Format("取消招募：{0}，队列中还有 {1} 个招募任务。", name, qcount));
                        }
                    }
                    break;
                }
        }
    }
    private void StatusChange()
    {
        // 建筑完成/科技完成也会影响其他建造的opbar，可能需要改变前驱颜色
        while (statuses.Count > 0)
        {
            StatusMsg msg = statuses.Dequeue();
            int type = msg.msg;
            Position pos = msg.pos;
            Building building = gameinfo.players.GetBuilding(pos);
            int buid = curbtn.building;
            switch (type)
            {
                case 1:     //开始建造
                    {
                        if (curbtn.grid.Equals(pos))
                        {
                            // 如果消息位置是点击位置
                            // 熄灭建筑按钮和建筑类型按钮
                            if (pos.right == gameinfo.client)
                            {
                                foreach (BtnBuilding btn in btnbuildings[curbtn.buildingtype])
                                {
                                    btn.gameObject.SetActive(false);
                                }
                                for (int i = 0; i < 5; i++)
                                {
                                    btnbuildingtypes[i].gameObject.SetActive(false);
                                }
                                // 熄灭建造按钮，点亮取消按钮
                                btnconfirm.SetActive(false);
                                btncancel.GetComponentInChildren<Text>().text = "取消";
                                btncancel.SetActive(true);
                            }
                            else
                            {
                                operationbar.SetActive(true);
                                ShowEnemyBuilding(gameinfo.buildingmap[msg.id]);
                            }
                            // 展示进度条
                            progressbar.SetActive(true);
                        }

                        break;
                    }
                case 2:     // 建造完毕
                    {
                        Building finish = gameinfo.players.GetBuilding(curbtn.grid);
                        if (finish != null)
                        {
                            buid = finish.id;
                        }
                        else
                        {
                            buid = 0;
                        }
                        if (pos.right == gameinfo.client)
                        {
                            // 自己的建筑
                            if (curbtn.grid.Equals(pos))
                            {
                                // 如果消息位置是点击位置
                                if (btnresearchs.ContainsKey(buid))
                                {
                                    foreach (BtnBuilding btn in btnresearchs[buid])
                                    {
                                        btn.gameObject.SetActive(true);
                                    }
                                }
                                if (btnsoldiers.ContainsKey(buid))
                                {
                                    foreach (BtnBuilding btn in btnsoldiers[buid])
                                    {
                                        btn.UpdateSoProgreeText(0);
                                        btn.gameObject.SetActive(true);
                                    }
                                    int soid = curbtn.soldier;
                                    if (soid != 0)
                                    {
                                        SoldierInfo soinfo = gameinfo.soldiermap[soid];
                                        if (soinfo.building == buid)
                                        {
                                            ShowSoldierInfo(soinfo);
                                            btnconfirm.GetComponentInChildren<Text>().text = "招募";
                                            btnconfirm.SetActive(true);
                                        }
                                    }
                                }
                                btncancel.GetComponentInChildren<Text>().text = "拆除";
                            }
                            else
                            {
                                // 消息非点击位置
                                if (buid != 0)
                                {
                                    // 当前位置存在建筑
                                    int reid = curbtn.research;
                                    int soid = curbtn.soldier;
                                    if (reid != 0)
                                    {
                                        ShowResearchInfo(gameinfo.researchmap[reid]);
                                    }
                                    else if (soid != 0)
                                    {
                                        SoldierInfo soinfo = gameinfo.soldiermap[soid];
                                        if (soinfo.building == buid)
                                        {
                                            ShowSoldierInfo(soinfo);
                                        }
                                    }
                                    else
                                    {
                                        ShowBuildingInfo(gameinfo.buildingmap[buid]);
                                    }
                                }
                                else
                                {
                                    buid = curbtn.building;
                                    // 当前位置不存在建筑，但是玩家点击了建筑按钮
                                    if (buid != 0)
                                    {
                                        ShowBuildingInfo(gameinfo.buildingmap[buid]);
                                    }
                                }
                            }
                        }
                        break;
                    }
                case 3:     // 取消建造
                    {
                        // 展示建筑按钮和建筑类型按钮
                        if (curbtn.grid.Equals(pos))
                        {
                            if (pos.right == gameinfo.client)
                            {
                                // 自己取消建造自己的建筑
                                foreach (BtnBuilding btn in btnbuildings[curbtn.buildingtype])
                                {
                                    btn.gameObject.SetActive(true);
                                }
                                for (int i = 0; i < 5; i++)
                                {
                                    btnbuildingtypes[i].gameObject.SetActive(true);
                                }
                                // 熄灭科技按钮
                                if (btnresearchs.ContainsKey(msg.id))
                                {
                                    foreach (BtnBuilding btn in btnresearchs[msg.id])
                                    {
                                        btn.gameObject.SetActive(false);
                                    }
                                }
                                if (btnsoldiers.ContainsKey(msg.id))
                                {
                                    foreach (BtnBuilding btn in btnsoldiers[msg.id])
                                    {
                                        btn.gameObject.SetActive(false);
                                    }
                                }
                                // 熄灭进度条
                                progressbar.SetActive(false);
                                // 点亮建造按钮，熄灭取消按钮
                                btnconfirm.GetComponentInChildren<Text>().text = "建造";
                                btnconfirm.SetActive(true);
                                btncancel.SetActive(false);
                                ShowBuildingInfo(gameinfo.buildingmap[buid]);
                            }
                            else
                            {
                                //敌人取消了自己的建筑
                                // 熄灭进度条
                                progressbar.SetActive(false);
                                // 熄灭操作条
                                operationbar.SetActive(false);
                            }
                        }
                        break;
                    }
                case 4:     // 建筑被毁
                    {
                        if (curbtn.grid.Equals(pos))
                        {
                            if (pos.right == gameinfo.client)
                            {
                                // 被毁的是自己的建筑
                                foreach (BtnBuilding btn in btnbuildings[curbtn.buildingtype])
                                {
                                    btn.gameObject.SetActive(true);
                                }
                                for (int i = 0; i < 5; i++)
                                {
                                    btnbuildingtypes[i].gameObject.SetActive(true);
                                }
                                // 熄灭科技按钮
                                if (btnresearchs.ContainsKey(msg.id))
                                {
                                    foreach (BtnBuilding btn in btnresearchs[msg.id])
                                    {
                                        btn.gameObject.SetActive(false);
                                    }
                                }
                                if (btnsoldiers.ContainsKey(msg.id))
                                {
                                    foreach (BtnBuilding btn in btnsoldiers[msg.id])
                                    {
                                        btn.gameObject.SetActive(false);
                                    }
                                }
                                // 熄灭进度条
                                progressbar.SetActive(false);
                                // 点亮建造按钮，熄灭取消按钮
                                btnconfirm.GetComponentInChildren<Text>().text = "建造";
                                btnconfirm.SetActive(true);
                                btncancel.SetActive(false);
                                ShowBuildingInfo(gameinfo.buildingmap[buid]);
                            }
                            else
                            {
                                // 被毁的是敌人的建筑
                                // 熄灭进度条
                                progressbar.SetActive(false);
                                // 熄灭操作条
                                operationbar.SetActive(false);
                            }
                        }
                        else
                        {
                            buid = gameinfo.players.GetBuilding(curbtn.grid).id;
                            // 可能影响当前格子的前驱
                            if (buid != 0)
                            {
                                int reid = curbtn.research;
                                int soid = curbtn.soldier;
                                if (reid != 0)
                                {
                                    ShowResearchInfo(gameinfo.researchmap[reid]);
                                }
                                else if (soid != 0)
                                {
                                    SoldierInfo soinfo = gameinfo.soldiermap[soid];
                                    if (buid == soinfo.building)
                                    {
                                        ShowSoldierInfo(soinfo);
                                    }
                                }
                                else
                                {
                                    ShowBuildingInfo(gameinfo.buildingmap[buid]);
                                }
                            }
                        }
                        break;
                    }
                case 5:     // 开始研发
                    {
                        if (pos.right == gameinfo.client)
                        {
                            foreach (BtnBuilding btn in btnresearchs[building.id])
                            {
                                btn.gameObject.SetActive(false);
                            }
                            // 展示进度条
                            progressbar.SetActive(true);
                            // 熄灭建造按钮，点亮取消按钮
                            btnconfirm.SetActive(false);
                            btncancel.GetComponentInChildren<Text>().text = "取消";
                            btncancel.SetActive(true);
                        }
                        break;
                    }
                case 6:     // 研发完成
                    {
                        if (pos.right == gameinfo.client)
                        {
                            Building finish = gameinfo.players.GetBuilding(curbtn.grid);
                            if (finish != null)
                            {
                                buid = finish.id;
                            }
                            else
                            {
                                buid = 0;
                            }
                            if (curbtn.grid.Equals(pos))
                            {

                                if (btnresearchs.ContainsKey(buid))
                                {
                                    foreach (BtnBuilding btn in btnresearchs[buid])
                                    {
                                        btn.gameObject.SetActive(true);
                                    }
                                }
                                curbtn.research = 0;
                                // 熄灭建造按钮，点亮取消按钮
                                // btnconfirm.GetComponentInChildren<Text>().text = "研发";
                                // btnconfirm.SetActive(true); // 如果次数不够了，也不能点亮建造按钮
                                ShowBuildingInfo(gameinfo.buildingmap[buid]);
                                btncancel.GetComponentInChildren<Text>().text = "拆除";
                                btncancel.SetActive(true);

                            }
                            else
                            {
                                // 消息非点击位置
                                if (buid != 0)
                                {
                                    int reid = curbtn.research;
                                    int soid = curbtn.soldier;
                                    if (reid != 0)
                                    {
                                        ShowResearchInfo(gameinfo.researchmap[reid]);
                                    }
                                    else if (soid != 0)
                                    {
                                        SoldierInfo soinfo = gameinfo.soldiermap[soid];
                                        if (buid == soinfo.building)
                                        {
                                            ShowSoldierInfo(soinfo);
                                        }
                                    }
                                    else
                                    {
                                        ShowBuildingInfo(gameinfo.buildingmap[buid]);
                                    }
                                }
                            }

                            btnresearchmap[msg.id].UpdateReProgressText();
                        }
                        break;
                    }
                case 7:     // 中止研发
                    {
                        if (pos.right == gameinfo.client)
                        {
                            Building finish = gameinfo.players.GetBuilding(curbtn.grid);
                            if (finish != null)
                            {
                                buid = finish.id;
                            }
                            else
                            {
                                buid = 0;
                            }
                            if (curbtn.grid.Equals(pos))
                            {
                                curbtn.research = msg.id;
                                if (curbtn.research == 0)
                                {
                                    btnconfirm.SetActive(false);
                                    ShowBuildingInfo(gameinfo.buildingmap[buid]);
                                }
                                else
                                {
                                    btnconfirm.GetComponentInChildren<Text>().text = "研发";
                                    btnconfirm.SetActive(true);
                                    ShowResearchInfo(gameinfo.researchmap[curbtn.research]);
                                }

                                if (btnresearchs.ContainsKey(buid))
                                {
                                    foreach (BtnBuilding btn in btnresearchs[buid])
                                    {
                                        btn.gameObject.SetActive(true);
                                    }
                                }

                                btncancel.GetComponentInChildren<Text>().text = "拆除";
                                btncancel.SetActive(true);
                            }
                        }
                        break;
                    }
                case 8:     // 开始招募（之前招募队列为空）
                    {
                        if (pos.right == gameinfo.client)
                        {
                            // 展示进度条
                            progressbar.SetActive(true);
                            // 点亮取消按钮
                            btncancel.GetComponentInChildren<Text>().text = "取消";
                            btncancel.SetActive(true);
                        }
                        break;
                    }
                case 9:     // 招募队列全部招募完毕
                    {
                        if (curbtn.grid.Equals(pos))
                        {
                            if (pos.right == gameinfo.client)
                            {
                                btncancel.GetComponentInChildren<Text>().text = "拆除";
                                btncancel.SetActive(true);
                            }
                        }
                        break;
                    }
                case 10:    // 招募队列完全清空
                    {
                        if (curbtn.grid.Equals(pos))
                        {
                            if (pos.right == gameinfo.client)
                            {
                                btncancel.GetComponentInChildren<Text>().text = "拆除";
                                btncancel.SetActive(true);
                            }
                        }
                        break;
                    }
                case 11:    // 增加一个招募士兵
                    {
                        if (pos.Equals(curbtn.grid))
                        {
                            if(pos.right == gameinfo.client)
                            {
                                btnsoldiermap[msg.id].UpdateSoProgreeText(1, 1);
                            }
                        }
                        break;
                    }
                case 12:    // 减少一个招募士兵
                    {
                        if (pos.Equals(curbtn.grid))
                        {
                            if (pos.right == gameinfo.client)
                            {
                                btnsoldiermap[msg.id].UpdateSoProgreeText(1, -1);
                            }
                        }
                        break;
                    }
            }
        }
    }
    // 这个处理各种按键请求，实时更新curbtn，并且进行适当的SetAction(false)操作
    private BtnStatus curbtn = new BtnStatus();
    // operatorbar上面的各个文本组件
    private GameObject opbarname, opbardesc, opbarneed;
    public FoodIronbar opbarfoodiron;
    public FoodIronbar rebarfoodiron;
    private GameObject[] opbarpre;
    private void ShowBuildingInfo(BuildingInfo info)
    {
        // 只管opbarname opbardesc opbarfoodiron opbarpre
        // 建造按钮和拆除按钮不归这个函数管
        Player player = gameinfo.players.GetPlayer(gameinfo.client);
        opbarname.GetComponent<Text>().text = info.name;
        opbardesc.GetComponent<Text>().text = info.desc;
        opbarname.SetActive(true);
        opbardesc.SetActive(true);
        int needfood = (int)(info.food * (1 + player.buildingbuff[info.id].food / 100f));
        int neediron = (int)(info.iron * (1 + player.buildingbuff[info.id].iron / 100f));
        opbarfoodiron.SetNum(needfood, neediron);
        opbarfoodiron.SetActive(true);
        // 描述：需要建筑/需要科技
        // 当其他建筑建成时，或是其他科技完成时，需要检查当前的InfoBar，可能需要变色
        int prenum = 0;
        foreach (int i in info.pre_building)
        {
            if (i != 0)
            {
                Text name = opbarpre[prenum].GetComponent<Text>();
                name.text = gameinfo.buildingmap[i].name;
                if (player.HasBuilding(i))
                {
                    name.color = Color.green;
                }
                else
                {
                    name.color = Color.red;
                }
                prenum++;
            }
        }
        foreach (int i in info.pre_research)
        {
            if (i != 0)
            {
                Text name = opbarpre[prenum].GetComponent<Text>();
                name.text = gameinfo.researchmap[i].name;
                if (player.HasResearch(i))
                {
                    name.color = Color.green;
                }
                else
                {
                    name.color = Color.red;
                }
                prenum++;
            }
        }
        if (prenum == 0)
        {
            opbarneed.GetComponent<Text>().text = "";
        }
        else
        {
            opbarneed.GetComponent<Text>().text = "需要";
        }
        while (prenum < 2)
        {
            opbarpre[prenum].GetComponent<Text>().text = "";
            prenum++;
        }
    }
    private void ShowEnemyBuilding(BuildingInfo info)
    {
        opbarname.GetComponent<Text>().text = info.name;
        opbardesc.GetComponent<Text>().text = info.desc;
        opbarneed.GetComponent<Text>().text = "";
        opbarname.SetActive(true);
        opbardesc.SetActive(true);
        opbarfoodiron.SetActive(false);
        for(int i = 0; i < 2; i++)
        {
            opbarpre[i].GetComponent<Text>().text = "";
        }
    }
    private void ShowResearchInfo(ResearchInfo info)
    {
        // 只管opbarname opbardesc opbarfoodiron opbarpre
        // confirm按钮和cancel按钮不归这个函数管
        Player player = gameinfo.players.GetPlayer(gameinfo.client);
        opbarname.GetComponent<Text>().text = info.name;
        opbardesc.GetComponent<Text>().text = info.desc;
        opbarfoodiron.SetNum(info.food, info.iron);
        opbarname.SetActive(true);
        opbardesc.SetActive(true);
        opbarfoodiron.SetActive(true);
        // 描述：需要建筑/需要科技
        // 当其他建筑建成时，或是其他科技完成时，需要检查当前的InfoBar，可能需要变色
        int prenum = 0;
        foreach (int i in info.pre_building)
        {
            if (i != 0)
            {
                Text name = opbarpre[prenum].GetComponent<Text>();
                name.text = gameinfo.buildingmap[i].name;
                if (player.HasBuilding(i))
                {
                    name.color = Color.green;
                }
                else
                {
                    name.color = Color.red;
                }
                prenum++;
            }
        }
        foreach (int i in info.pre_research)
        {
            if (i != 0)
            {
                Text name = opbarpre[prenum].GetComponent<Text>();
                name.text = gameinfo.researchmap[i].name;
                if (player.HasResearch(i))
                {
                    name.color = Color.green;
                }
                else
                {
                    name.color = Color.red;
                }
                prenum++;
            }
        }
        if(prenum == 0)
        {
            opbarneed.GetComponent<Text>().text = "";
        }
        else
        {
            opbarneed.GetComponent<Text>().text = "需要";
        }
        while (prenum < 2)
        {
            opbarpre[prenum].GetComponent<Text>().text = "";
            prenum++;
        }
    }
    private void ShowSoldierInfo(SoldierInfo info)
    {
        // 只管opbarname opbardesc opbarfoodiron opbarpre
        // confirm按钮和cancel按钮不归这个函数管
        Player player = gameinfo.players.GetPlayer(gameinfo.client);
        opbarname.GetComponent<Text>().text = info.name;
        opbardesc.GetComponent<Text>().text = info.desc;
        int needfood = (int)(info.food * (1 + player.soldierbuff[info.id].food / 100f));
        int neediron = (int)(info.iron * (1 + player.soldierbuff[info.id].iron / 100f));
        opbarfoodiron.SetNum(needfood, neediron);
        opbarname.SetActive(true);
        opbardesc.SetActive(true);
        opbarfoodiron.SetActive(true);
        // 描述：需要建筑/需要科技
        // 当其他建筑建成时，或是其他科技完成时，需要检查当前的InfoBar，可能需要变色
        int prenum = 0;
        foreach (int i in info.pre_building)
        {
            if (i != 0)
            {
                Text name = opbarpre[prenum].GetComponent<Text>();
                name.text = gameinfo.buildingmap[i].name;
                if (player.HasBuilding(i))
                {
                    name.color = Color.green;
                }
                else
                {
                    name.color = Color.red;
                }
                prenum++;
            }
        }
        foreach (int i in info.pre_research)
        {
            if (i != 0)
            {
                Text name = opbarpre[prenum].GetComponent<Text>();
                name.text = gameinfo.researchmap[i].name;
                if (player.HasResearch(i))
                {
                    name.color = Color.green;
                }
                else
                {
                    name.color = Color.red;
                }
                prenum++;
            }
        }
        if (prenum == 0)
        {
            opbarneed.GetComponent<Text>().text = "";
        }
        else
        {
            opbarneed.GetComponent<Text>().text = "需要";
        }
        while (prenum < 2)
        {
            opbarpre[prenum].GetComponent<Text>().text = "";
            prenum++;
        }
    }
    private void ShowResourceInfo()
    {

        int right = curbtn.grid.right;
        if (right != -1)
        {
            int row = curbtn.grid.row;
            int col = curbtn.grid.col;
            Player player = gameinfo.players.GetPlayer(right);
            Building building = player.buildings[row, col];
            if (building.id != -1)
            {
                int food, iron;
                building.CheckFoodIronLeft(out food, out iron);
                rebarfoodiron.SetNum(food, iron);
            }
        }
    }
    private void BtnClicked()
    {
        // 可以进行的修改：未达到建造条件的建筑和科技不显示confirm按钮
        while(clicked.Count > 0)
        {
            ClickMsg msg = clicked.Dequeue();
            int id = msg.id;
            int type = msg.type;
            int client = gameinfo.client;
            Player player;
            switch (type)
            {
                case 1: // 建筑类型按钮
                    {
                        if (id != curbtn.buildingtype)
                        {
                            foreach (BtnBuilding btn in btnbuildings[curbtn.buildingtype])
                            {
                                btn.gameObject.SetActive(false);
                            }
                            foreach (BtnBuilding btn in btnbuildings[id])
                            {
                                btn.gameObject.SetActive(true);
                            }
                            curbtn.buildingtype = id;
                        }
                        break;
                    }
                case 2: // 建筑按钮
                    {
                        curbtn.building = id;
                        btncancel.SetActive(false);
                        btnconfirm.GetComponentInChildren<Text>().text = "建造";
                        btnconfirm.SetActive(true);
                        operationbar.SetActive(true);
                        ShowBuildingInfo(gameinfo.buildingmap[id]);
                        break;
                    }
                case 3: // 科技按钮
                    {
                        curbtn.research = id;
                        btncancel.GetComponentInChildren<Text>().text = "拆除";
                        btncancel.SetActive(true);
                        btnconfirm.GetComponentInChildren<Text>().text = "研发";
                        btnconfirm.SetActive(true);
                        operationbar.SetActive(true);
                        ShowResearchInfo(gameinfo.researchmap[id]);
                        break;
                    }
                case 4: // 格子
                    {
                        if (!curbtn.grid.Equals(msg.pos))
                        {
                            int right = msg.pos.right;
                            int row = msg.pos.row;
                            int col = msg.pos.col;
                            player = gameinfo.players.GetPlayer(right);
                            Building building = player.buildings[row, col];

                            if (curbtn.grid.right != -1)
                            {
                                // 先把旧的科技按钮熄灭
                                Building oldbuilding = gameinfo.players.GetPlayer(curbtn.grid.right).buildings[curbtn.grid.row, curbtn.grid.col];
                                if (btnresearchs.ContainsKey(oldbuilding.id))
                                {
                                    foreach (BtnBuilding btn in btnresearchs[oldbuilding.id])
                                    {
                                        btn.gameObject.SetActive(false);
                                    }
                                }
                                // 把旧的兵种按钮熄灭
                                if (btnsoldiers.ContainsKey(oldbuilding.id))
                                {
                                    foreach (BtnBuilding btn in btnsoldiers[oldbuilding.id])
                                    {
                                        btn.gameObject.SetActive(false);
                                    }
                                }
                            }

                            if (building.id == 0)
                            {
                                // 空地
                                if (right == client)
                                {
                                    foreach (BtnBuildingType btn in btnbuildingtypes)
                                    {
                                        btn.gameObject.SetActive(true);
                                    }
                                    foreach (BtnBuilding btn in btnbuildings[curbtn.buildingtype])
                                    {
                                        btn.gameObject.SetActive(true);
                                    }
                                    if (curbtn.building != 0)
                                    {
                                        btncancel.SetActive(false);
                                        btnconfirm.GetComponentInChildren<Text>().text = "建造";
                                        btnconfirm.SetActive(true);
                                        operationbar.SetActive(true);
                                        ShowBuildingInfo(gameinfo.buildingmap[curbtn.building]);
                                    }
                                    else
                                    {
                                        operationbar.SetActive(false);
                                    }
                                }
                                else
                                {
                                    foreach (BtnBuildingType btn in btnbuildingtypes)
                                    {
                                        btn.gameObject.SetActive(false);
                                    }
                                    foreach (BtnBuilding btn in btnbuildings[curbtn.buildingtype])
                                    {
                                        btn.gameObject.SetActive(false);
                                    }
                                    operationbar.SetActive(false);
                                }
                                progressbar.SetActive(false);
                            }
                            else
                            {
                                // 存在建筑
                                foreach (BtnBuildingType btn in btnbuildingtypes)
                                {
                                    btn.gameObject.SetActive(false);
                                }
                                foreach (BtnBuilding btn in btnbuildings[curbtn.buildingtype])
                                {
                                    btn.gameObject.SetActive(false);
                                }
                                if (right == client)
                                {
                                    // 是自己的建筑
                                    // 已经建造完毕
                                    operationbar.SetActive(true);
                                    if (building.status == building.maxstatus)
                                    {
                                        if (btnresearchs.ContainsKey(building.id))
                                        {
                                            // 如果没有在研发，展示建筑信息 
                                            if (building.researching == 0)
                                            {
                                                foreach (BtnBuilding btn in btnresearchs[building.id])
                                                {
                                                    btn.gameObject.SetActive(true);
                                                }
                                                btncancel.GetComponentInChildren<Text>().text = "拆除";
                                                btnconfirm.SetActive(false);
                                                ShowBuildingInfo(gameinfo.buildingmap[building.id]);
                                            }
                                            else
                                            {
                                                // 正在研发，则展示取消按钮
                                                int reid = building.researching;
                                                ShowResearchInfo(gameinfo.researchmap[reid]);
                                                btncancel.GetComponentInChildren<Text>().text = "取消";
                                                btnconfirm.SetActive(false);
                                            }
                                        }
                                        else if (btnsoldiers.ContainsKey(building.id))
                                        {
                                            // 当前是军团建筑
                                            foreach (BtnBuilding btn in btnsoldiers[building.id])
                                            {
                                                btn.UpdateSoProgreeText(0);
                                                btn.gameObject.SetActive(true);
                                            }
                                            // 更新士兵按钮招募数字
                                            foreach (Produce produce in building.producing)
                                            {
                                                int soid = produce.id;
                                                btnsoldiermap[soid].UpdateSoProgreeText(1, 1);
                                            }
                                            if (building.producing.Count > 0)
                                            {
                                                // 正在招募，则展示取消
                                                btncancel.GetComponentInChildren<Text>().text = "取消";
                                            }
                                            else
                                            {
                                                // 未在招募，则展示拆除
                                                btncancel.GetComponentInChildren<Text>().text = "拆除";
                                            }

                                            if (gameinfo.soldiermap.ContainsKey(curbtn.soldier))
                                            {
                                                SoldierInfo soinfo = gameinfo.soldiermap[curbtn.soldier];
                                                if (soinfo.building == building.id)
                                                {
                                                    ShowSoldierInfo(soinfo);
                                                    btnconfirm.GetComponentInChildren<Text>().text = "招募";
                                                    btnconfirm.SetActive(true);
                                                }
                                                else
                                                {
                                                    btnconfirm.SetActive(false);
                                                    ShowBuildingInfo(gameinfo.buildingmap[building.id]);
                                                }
                                            }
                                            else
                                            {
                                                ShowBuildingInfo(gameinfo.buildingmap[building.id]);
                                                btnconfirm.SetActive(false);
                                            }

                                        }
                                        else
                                        {
                                            btncancel.GetComponentInChildren<Text>().text = "拆除";
                                            ShowBuildingInfo(gameinfo.buildingmap[building.id]);
                                            btnconfirm.SetActive(false);
                                        }
                                    }
                                    else
                                    {
                                        // 还在建造
                                        btncancel.GetComponentInChildren<Text>().text = "取消";
                                        ShowBuildingInfo(gameinfo.buildingmap[building.id]);
                                        btnconfirm.SetActive(false);
                                    }
                                    btncancel.SetActive(true);
                                }
                                else if (right != client)
                                {
                                    // 是别人的建筑
                                    operationbar.SetActive(true);
                                    ShowEnemyBuilding(gameinfo.buildingmap[building.id]);
                                    btnconfirm.SetActive(false);
                                    btncancel.SetActive(false);
                                }
                                progressbar.SetActive(true);
                            }

                            if ((building.foodpoint || building.ironpoint) && right == client)
                            {
                                resourcebar.SetActive(true);
                            }
                            else
                            {
                                resourcebar.SetActive(false);
                            }

                            curbtn.research = 0;
                            // curbtn.soldier = 0;
                            curbtn.grid.set(msg.pos);
                        }

                        break;
                    }
                case 5: // 兵种按钮
                    {
                        curbtn.soldier = id;
                        btncancel.GetComponentInChildren<Text>().text = "拆除";
                        btncancel.SetActive(true);
                        btnconfirm.GetComponentInChildren<Text>().text = "招募";
                        btnconfirm.SetActive(true);
                        operationbar.SetActive(true);
                        ShowSoldierInfo(gameinfo.soldiermap[id]);
                        break;
                    }
            }
        }
    }
    private void initbtnbuilding()
    {
        btnbuildingtypes = new List<BtnBuildingType>(5);
        string[] typestring = {"核心", "资源", "军团", "功能", "研发" };
        Transform father = GameObject.Find("Background").transform;
        for (int i = 0; i < 5; i++)
        {
            btnbuildingtypes.Add(null);
            btnbuildingtypes[i] = Instantiate(btnbuildingtype, father);
            btnbuildingtypes[i].name = "btnbuildingtype" + i.ToString();
            btnbuildingtypes[i].text.text = typestring[i];
            btnbuildingtypes[i].Init(i, this);
            btnbuildingtypes[i].gameObject.SetActive(false);
        }
        btnbuildingtype.gameObject.SetActive(false);

        btnbuildings = new SortedDictionary<int, List<BtnBuilding>>();
        btnbuildings.Add(0, new List<BtnBuilding>());
        foreach (KeyValuePair<int, BuildingInfo> pair in gameinfo.buildingmap)
        {
            int type = pair.Value.type;
            if (!btnbuildings.ContainsKey(type))
            {
                btnbuildings.Add(type, new List<BtnBuilding>());
            }
            int btnid = btnbuildings[type].Count;
            btnbuildings[type].Add(null);
            btnbuildings[type][btnid] = Instantiate(btnbuilding, father);
            string path = pair.Value.path;
            btnbuildings[type][btnid].InitB(btnid, pair.Key, this, path);
        }

        btnbuilding.gameObject.SetActive(false);
    }
    private void initbtnresearch()
    {
        btnresearchs = new SortedDictionary<int, List<BtnBuilding>>();
        btnresearchmap = new SortedDictionary<int, BtnBuilding>();
        btnresearchs.Add(0, new List<BtnBuilding>());
        Transform father = GameObject.Find("Background").transform;
        foreach (KeyValuePair<int, ResearchInfo> pair in gameinfo.researchmap)
        {
            int buid = pair.Value.building;
            if (!btnresearchs.ContainsKey(buid))
            {
                btnresearchs.Add(buid, new List<BtnBuilding>());
            }
            int btnid = btnresearchs[buid].Count;
            BtnBuilding btnbuilding = Instantiate(btnresearch, father);
            string path = pair.Value.path;
            btnbuilding.InitR(btnid, pair.Key, this, path);
            btnresearchmap.Add(pair.Key, btnbuilding);
            btnresearchs[buid].Add(btnbuilding);
        }
        btnresearch.gameObject.SetActive(false);
    }
    private void initbtnsoldier()
    {
        btnsoldiers = new SortedDictionary<int, List<BtnBuilding>>();
        btnsoldiermap = new SortedDictionary<int, BtnBuilding>();
        btnsoldiers.Add(0, new List<BtnBuilding>());
        Transform father = GameObject.Find("Background").transform;
        foreach (KeyValuePair<int, SoldierInfo> pair in gameinfo.soldiermap)
        {
            int buid = pair.Value.building;
            if (!btnsoldiers.ContainsKey(buid))
            {
                btnsoldiers.Add(buid, new List<BtnBuilding>());
            }
            int btnid = btnsoldiers[buid].Count;
            BtnBuilding btnbuilding = Instantiate(btnsoldier, father);
            string path = pair.Value.path;
            btnbuilding.InitS(btnid, pair.Key, this, path);
            btnsoldiermap.Add(pair.Key, btnbuilding);
            btnsoldiers[buid].Add(btnbuilding);
        }
        btnsoldier.gameObject.SetActive(false);
    }
    private void Build()
    {
        int client = gameinfo.client;
        int right = curbtn.grid.right;
        Player player = gameinfo.players.GetPlayer(client);
        // 目标位置上当前的状况
        Building target = gameinfo.players.GetBuilding(curbtn.grid);
        // 待建建筑的信息
        BuildingInfo tobuild = gameinfo.buildingmap[curbtn.building];

        // 判断失效后，可以增加一个消息盒

        // 先判断玩家点击的建筑是不是自己的建筑
        if (right == client)
        {
            // 不能在已有的建筑上造新建筑
            if (target.id == 0)
            {
                if (tobuild.collect_food <= 0 || target.foodpoint)
                {
                    if (tobuild.collect_iron <= 0 || target.ironpoint)
                    {
                        // 需要足够的资源
                        int needfood = (int)(tobuild.food * (1 + player.buildingbuff[tobuild.id].food / 100f));
                        if (player.food >= needfood)
                        {
                            int neediron = (int)(tobuild.iron * (1 + player.buildingbuff[tobuild.id].iron / 100f));
                            if (player.iron >= neediron)
                            {
                                // 前驱科技和前驱建筑需满足
                                int lack;
                                if (player.HasBuildings(tobuild.pre_building, out lack))
                                {
                                    if (player.HasResearches(tobuild.pre_research, out lack))
                                    {
                                        // 发送建造信号
                                        socket.CreateSignal(1, curbtn.grid, tobuild.id);
                                    }
                                    else
                                    {
                                        Log(string.Format("请先研发：{0}", gameinfo.researchmap[lack].name));
                                    }
                                }
                                else
                                {
                                    Log(string.Format("请先建造：{0}", gameinfo.buildingmap[lack].name));
                                }
                            }
                            else
                            {
                                Log(string.Format("钢铁不够"));
                            }
                        }
                        else
                        {
                            Log(string.Format("粮食不够"));
                        }
                    }
                    else
                    {
                        Log(string.Format("{0} 必须建在矿山前", tobuild.name));
                    }
                }
                else
                {
                    Log(string.Format("{0} 必须建在土壤上", tobuild.name));
                }
            }
        }
    }
    private void Research()
    {
        int client = gameinfo.client;
        Player player = gameinfo.players.GetPlayer(client);
        // 目标位置上当前的状况
        Building target = gameinfo.players.GetBuilding(curbtn.grid);
        int id = curbtn.research;
        // 待研发科技的信息
        ResearchInfo toresearch = gameinfo.researchmap[id];

        // 科技所属建筑应为当前格子建筑
        if (target.id == toresearch.building)
        {
            // 当前建筑未在研发科技
            if (target.researching == 0)
            {
                // 当前科技未在研究
                if (player.researches[id].status == player.researches[id].maxstatus)
                {
                    // 当前科技未达研发次数上限
                    if (player.researches[id].level < toresearch.max || toresearch.max == -1)
                    {
                        // 需要足够的资源
                        int needfood = toresearch.food;
                        if (player.food >= needfood)
                        {
                            int neediron = toresearch.iron;
                            if (player.iron >= neediron)
                            {
                                // 前驱科技和前驱建筑需满足
                                int lack;
                                if (player.HasBuildings(toresearch.pre_building, out lack))
                                {
                                    if (player.HasResearches(toresearch.pre_research, out lack))
                                    {
                                        // 发送研发信号
                                        socket.CreateSignal(3, curbtn.grid, id);
                                    }
                                    else
                                    {
                                        Log(string.Format("请先研发：{0}", gameinfo.researchmap[lack].name));
                                    }
                                }
                                else
                                {
                                    Log(string.Format("请先建造：{0}", gameinfo.buildingmap[lack].name));
                                }
                            }
                            else
                            {
                                Log(string.Format("钢铁不够"));
                            }
                        }
                        else
                        {
                            Log(string.Format("粮食不够"));
                        }
                    }
                    else
                    {
                        Log(string.Format("{0} 已达到等级上限。", toresearch.name));
                    }
                }
                else
                {
                    Log(string.Format("{0} 已经在研究了。", toresearch.name));
                }
            }
        }
    }
    private void Produce()
    {
        int client = gameinfo.client;
        Player player = gameinfo.players.GetPlayer(client);
        // 目标位置上当前的状况
        Building target = gameinfo.players.GetBuilding(curbtn.grid);
        int id = curbtn.soldier;
        // 待招募兵种的信息
        SoldierInfo toproduce = gameinfo.soldiermap[id];

        // 兵种所属建筑应为当前格子建筑
        if (target.id == toproduce.building)
        {
            // 需要足够的资源
            int needfood = (int)(toproduce.food * (1 + player.soldierbuff[toproduce.id].food / 100f));
            if (player.food >= needfood)
            {
                int neediron = (int)(toproduce.iron * (1 + player.soldierbuff[toproduce.id].iron / 100f));
                if (player.iron >= neediron)
                {
                    // 前驱科技和前驱建筑需满足
                    int lack;
                    if (player.HasBuildings(toproduce.pre_building, out lack))
                    {
                        if (player.HasResearches(toproduce.pre_research, out lack))
                        {
                            // 发送招募信号
                            socket.CreateSignal(5, curbtn.grid, id);
                        }
                        else
                        {
                            Log(string.Format("请先研发：{0}", gameinfo.researchmap[lack].name));
                        }
                    }
                    else
                    {
                        Log(string.Format("请先建造：{0}", gameinfo.buildingmap[lack].name));
                    }
                }
                else
                {
                    Log(string.Format("钢铁不够"));
                }
            }
            else
            {
                Log(string.Format("粮食不够"));
            }
        }
    }
    private void DeBuild()
    {
        int client = gameinfo.client;
        int right = curbtn.grid.right;
        // 目标位置上当前的状况
        Building target = gameinfo.players.GetBuilding(curbtn.grid);
        int buid = target.id;
        BuildingInfo todebuild = gameinfo.buildingmap[buid];
        // 先判断玩家点击的建筑是不是自己的建筑
        if (right == client)
        {
            // 必须存在建筑
            if (target.id != 0)
            {
                if (todebuild.type != 1 || gameinfo.players.GetCoreNum(client) > 1)
                {
                    // 发送拆除信号
                    socket.CreateSignal(2, curbtn.grid, buid);
                }
                else
                {
                    Log(string.Format("你不能拆除你唯一的核心建筑"));
                }
            }
        }
    }
    private void DeResearch()
    {
        int client = gameinfo.client;
        int right = curbtn.grid.right;
        Building building = gameinfo.players.GetBuilding(curbtn.grid);
        int reid = building.researching;
        // 先判断玩家点击的建筑是不是自己的建筑
        if (right == client)
        {
            // 必须存在科技
            if (reid != 0)
            {
                // 发送取消信号
                socket.CreateSignal(4, curbtn.grid, reid);
            }
        }
    }
    private void DeProduce()
    {
        int client = gameinfo.client;
        int right = curbtn.grid.right;
        Building building = gameinfo.players.GetBuilding(curbtn.grid);
        // 先判断玩家点击的建筑是不是自己的建筑
        if (right == client)
        {
            // 必须存在招募
            if (building.producing.Count > 0)
            {
                int soid = building.producing[building.producing.Count - 1].id;
                // 发送取消信号
                socket.CreateSignal(6, curbtn.grid, soid);

                // 收到取消信号后再取消
                
            }
        }
    }
    private void Confirm()
    {
        // 根据不同情况，Confirm具有不同的含义
        Building building = gameinfo.players.GetBuilding(curbtn.grid);
        int id = building.id;
        if (id == 0)
        {
            // 空地建房
            Build();
        }
        else if(building.id != 0){
            // 不是空地，根据建筑类型决定
            int type = gameinfo.buildingmap[id].type;
            switch (type)
            {
                case 5:
                    // 研发型建筑
                    Research();
                    break;
                case 3:
                    // 军团型建筑
                    Produce();
                    break;
            }
        }
    }
    private void Cancel()
    {
        // 根据不同情况，Cancel具有不同的含义
        Building building = gameinfo.players.GetBuilding(curbtn.grid);
        if(building.researching == 0)
        {
            // 当前没有研发科技
            if (building.producing.Count > 0)
            {
                // 当前在招募士兵
                DeProduce();
            }
            else
            {
                // 当前也没有招募士兵
                DeBuild();
            }
        }
        else
        {
            // 当前在研发科技
            DeResearch();
        }
    }
    private int curavatar = 0;
    public void ShowAvatar()
    {
        int right = background.curgrid.right;
        if (right != -1)
        {
            int buildingid = gameinfo.players.GetBuilding(curbtn.grid).id;
            if (curavatar != buildingid)
            {
                if (buildingid != 0)
                {
                    avatar.sprite = gameinfo.buildingmap[buildingid].sprite[right];
                }
                else if (buildingid == 0)
                {
                    avatar.sprite = null;
                }
                curavatar = buildingid;
            }
        }
        else if(right == -1)
        {
            // 未选中任何地点时的avatar，只会在游戏初始时用一次
        }
    }
    public void UpdateProgressBar()
    {
        int right = background.curgrid.right;
        if (right != -1)
        {
            Player player = gameinfo.players.GetPlayer(right);
            Building building = gameinfo.players.GetBuilding(curbtn.grid);
            if (building.id != 0)
            {
                BuildingInfo buildingInfo = gameinfo.buildingmap[building.id];
                int hp = building.unit.hp;
                int maxhp = buildingInfo.unitinfo.maxhp;
                hpbar.maxvalue = maxhp;
                hpbar.value = hp;
                hpbartext.text = string.Format("剩余耐久 ({0} / {1})", hp, maxhp);
                // 建造中、建造完成、研发中、生产中  buildbar展示不同信息
                int status = building.status;
                int maxstatus = building.maxstatus;
                if (right == gameinfo.client)
                {
                    // 自己人
                    if (status < maxstatus)
                    {
                        // 建造中
                        buildbar.gameObject.SetActive(true);
                        buildbar.maxvalue = maxstatus;
                        buildbar.value = status;
                        buildbartext.text = string.Format("{0} ({1} / {2})", buildingInfo.name, status, maxstatus);
                    }
                    else
                    {
                        // 建造完成
                        int reid = building.researching;
                        if (reid != 0)
                        {
                            // 有正在研发的科技
                            buildbar.gameObject.SetActive(true);
                            ResearchInfo researchinfo = gameinfo.researchmap[reid];
                            Research research = player.researches[reid];
                            status = (int)research.status;
                            maxstatus = research.maxstatus;
                            buildbar.maxvalue = maxstatus;
                            buildbar.value = status;
                            buildbartext.text = string.Format("{0} ({1} / {2})", researchinfo.name, status, maxstatus);
                        }
                        else
                        {
                            // 没有正在研发的科技
                            if (building.producing.Count > 0)
                            {
                                // 有正在招募的士兵
                                buildbar.gameObject.SetActive(true);
                                Produce produce = building.producing[0];
                                SoldierInfo soldierinfo = gameinfo.soldiermap[produce.id];
                                status = produce.status;
                                maxstatus = produce.maxstatus;
                                buildbar.maxvalue = produce.maxstatus;
                                buildbar.value = produce.status;
                                buildbartext.text = string.Format("{0} ({1} / {2})", soldierinfo.name, status, maxstatus);
                            }
                            else
                            {
                                // 没有正在招募的士兵
                                buildbartext.text = "";
                                buildbar.gameObject.SetActive(false);
                            }

                        }
                    }
                }
                else
                {
                    // 敌人不显示建造条
                    buildbar.gameObject.SetActive(false);
                    buildbartext.text = "";
                }
            }
        }
    }
    public void Init()
    {
        avatar = GameObject.Find("Background/BuildingAvatar").GetComponent<Image>();

        btnconfirm = GameObject.Find("InfoBar/Background/OperationBar/BtnConfirm");
        btnconfirm.GetComponent<Button>().onClick.AddListener(Confirm);

        btncancel = GameObject.Find("InfoBar/Background/OperationBar/BtnCancel");
        btncancel.GetComponent<Button>().onClick.AddListener(Cancel);

        operationbar = GameObject.Find("InfoBar/Background/OperationBar");

        opbarname = GameObject.Find("InfoBar/Background/OperationBar/Name");
        opbardesc = GameObject.Find("InfoBar/Background/OperationBar/Desc");
        opbarneed = GameObject.Find("InfoBar/Background/OperationBar/Need");

        opbarpre[0] = GameObject.Find("InfoBar/Background/OperationBar/Need1");
        opbarpre[1] = GameObject.Find("InfoBar/Background/OperationBar/Need2");

        progressbar = GameObject.Find("InfoBar/Background/ProgressBar");
        resourcebar = GameObject.Find("InfoBar/Background/ResourceBar");

        if(gameinfo.client == 1)
        {
            resourcebar.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
            resourcebar.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
            resourcebar.GetComponent<RectTransform>().pivot = new Vector2(0, 0);
        }
        else if(gameinfo.client == 0)
        {
            resourcebar.GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);
            resourcebar.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
            resourcebar.GetComponent<RectTransform>().pivot = new Vector2(1, 0);
        }
        resourcebar.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

        operationbar.SetActive(false);
        progressbar.SetActive(false);
        resourcebar.SetActive(false);
        initbtnbuilding();
        initbtnresearch();
        initbtnsoldier();

    }
    private void Log(string msg)
    {
        canvas.ShowMsg(msg);
    }
}
