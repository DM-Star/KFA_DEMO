using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    // 继承自MapGrid
    private SpriteRenderer render;
    public SpriteRenderer border;
    private Position position;
    private BackGround background;
    public bool foodpoint;
    public bool ironpoint;
    private int foodleft = 0;
    private int ironleft = 0;

    void Awake()
    {
        render = GetComponent<SpriteRenderer>();
        border.enabled = false;
        unit = null;
    }
    void OnMouseDown()
    {
        background.gridclicked.Enqueue(position);
    }
    public void select(bool isselected)
    {
        if (isselected)
        {
            // render.color = Color.gray;
            border.enabled = true;
        }
        else
        {
            border.enabled = false;
        }
    }

    public void Init(Position gridpos, Vector3 transpos, BackGround bg, bool food = false, bool iron = false)
    {
        position = gridpos;
        transform.position = transpos;
        background = bg;

        producing = new List<Produce>();
        id = 0;
        status = 0;
        maxstatus = 0;
        frame = 0;
        unit = null;

        foodpoint = food;
        ironpoint = iron;
        if (foodpoint) foodleft = 3000;
        if (ironpoint) ironleft = 3000;
    }

    public void CheckFoodIronLeft(out int food, out int iron)
    {
        food = foodleft;
        iron = ironleft;
    }

    public int id;
    public Player player;   // 建筑归属于哪个玩家
    public int row;
    public int col;
    private int collect_food;
    private int collect_iron;
    public BuildingInfo buinfo;
    public Unit unit;
    public HpChangeTip hpchangeins;

    // 当前建造状态，0无建筑，小于maxstatue建造中，=maxstatus完成建造
    public int status;
    public int maxstatus;
    // 当前正在研发的科技，0表示没有
    public int researching;

    // 士兵招募队列
    public List<Produce> producing;
    public int AddProduce(SoldierInfo info, int food, int iron)
    {
        producing.Add(new Produce(info, food, iron));
        return producing.Count;
    }
    public int CancelProduce()
    {
        if (producing.Count > 0)
        {
            Produce produce = producing[producing.Count - 1];
            player.food += produce.food;
            player.iron += produce.iron;
            producing.RemoveAt(producing.Count - 1);
        }
        return producing.Count;
    }
    private int frame = 0;
    public void Move(GameInfo gameinfo)
    {
        if (id > 0 && unit.hp > 0)
        {
            BuildingInfo buinfo = gameinfo.buildingmap[id];
            if (status < maxstatus)
            {
                // 建造中
                status++;
                float complete = (float)status / maxstatus;
                render.color = new Color(1, 1, 1, 0.25f + complete / 4);
                int newmaxhp = System.Convert.ToInt32(buinfo.unitinfo.maxhp * complete);
                int hpdelta = newmaxhp - unit.maxhp;
                unit.ResetCD(100000000);    // 建造期间不能移动、发射
                if (hpdelta > 0)
                {
                    unit.maxhp += hpdelta;
                    unit.hp += hpdelta;
                }
                if (status == maxstatus)
                {
                    player.FinishBuild(id);
                    Position pos = new Position(player.isclient, row, col);
                    gameinfo.infobar.statuses.Enqueue(new StatusMsg(2, pos));
                    render.color = new Color(1, 1, 1, 1f);
                    if (player.isclient == gameinfo.client)
                    {
                        gameinfo.canvas.ShowMsg(string.Format("{0} 建造完成。", buinfo.name));
                    }
                    else
                    {
                        Debug.Log(string.Format("【对手】建造完成:{0}  [{1}, {2}]", buinfo.name, row, col));
                    }
                    unit.ResetCD(0);  // 建造完成可以移动、发射
                }
            }
            else
            {
                // 完成建造
                // 处理研发进度
                if (researching != 0)
                {
                    Research research = player.researches[researching];
                    research.Move(gameinfo);
                }
                // 处理招募进度
                if (producing.Count > 0)
                {
                    Produce produce = producing[0];
                    if (produce.Move())
                    {
                        // 招募完成了
                        int soid = produce.id;
                        player.FinishProduce(this, soid);
                        producing.RemoveAt(0);
                        if (producing.Count == 0)
                        {
                            // 列表清空了
                            Position pos = new Position(player.isclient, row, col);
                            gameinfo.infobar.statuses.Enqueue(new StatusMsg(9, pos));
                        }
                        if (player.isclient == gameinfo.client)
                        {
                            gameinfo.canvas.ShowMsg(string.Format("{0} 招募完成。", gameinfo.soldiermap[produce.id].name));
                        }
                    }
                }
                if (frame % 50 == 0)
                {
                    // 必须建造完成才能进行生产
                    // 处理产粮和产铁
                    if (foodleft < collect_food) collect_food = foodleft;
                    if (ironleft < collect_iron) collect_iron = ironleft;
                    player.food += collect_food;
                    player.iron += collect_iron;
                    foodleft -= collect_food;
                    ironleft -= collect_iron;
                }
            }
            frame++;
        }
    }
    public void Build(int r, int c, BuildingInfo info, bool finish = false)
    {
        // finish = true 表示直接完成
        buinfo = info;
        id = buinfo.id;
        row = r;
        col = c;
        frame = 0;
        collect_food = buinfo.collect_food;
        collect_iron = buinfo.collect_iron;

        unit = new Unit(background.gameinfo, this, r, player.isclient);

        maxstatus = buinfo.time;
        status = 0;
        researching = 0;
        producing.Clear();
        player.units.Add(transform, unit);
        render.color = new Color(1, 1, 1, 0.25f);
        render.sprite = buinfo.sprite[player.isclient];

        if (finish)
        {
            status = maxstatus;
            unit.hp = unit.maxhp = buinfo.unitinfo.maxhp;
            render.color = new Color(1, 1, 1, 1f);
        }
    }

    public void DamageTip(Damage damage)
    {
        HpChangeTip hpchange = Instantiate(hpchangeins, transform.parent);
        hpchange.Init(damage, transform.localPosition + new Vector3(-5, 0, 0));
    }
    public void HealTip(Heal heal)
    {
        HpChangeTip hpchange = Instantiate(hpchangeins, transform.parent);
        hpchange.Init(heal, transform.localPosition + new Vector3(-5, 0, 0));
    }
    public void Destroy()
    {
        // 建筑被破坏时，不会返回正在研发的科技材料，但是会返回招募队列中未在招募的兵种材料
        id = 0;
        status = 0;
        maxstatus = 0;
        researching = 0;
        collect_food = 0;
        collect_iron = 0;
        while (CancelProduce() >= 1) ;
        producing.Clear();
        frame = 0;
        render.color = new Color(1, 1, 1, 1f);
        render.sprite = background.buildingins.GetComponent<SpriteRenderer>().sprite;
        player.units.Remove(transform);
        unit = null;
    }
}