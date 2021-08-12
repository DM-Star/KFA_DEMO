using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    // �̳���MapGrid
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
    public Player player;   // �����������ĸ����
    public int row;
    public int col;
    private int collect_food;
    private int collect_iron;
    public BuildingInfo buinfo;
    public Unit unit;
    public HpChangeTip hpchangeins;

    // ��ǰ����״̬��0�޽�����С��maxstatue�����У�=maxstatus��ɽ���
    public int status;
    public int maxstatus;
    // ��ǰ�����з��ĿƼ���0��ʾû��
    public int researching;

    // ʿ����ļ����
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
                // ������
                status++;
                float complete = (float)status / maxstatus;
                render.color = new Color(1, 1, 1, 0.25f + complete / 4);
                int newmaxhp = System.Convert.ToInt32(buinfo.unitinfo.maxhp * complete);
                int hpdelta = newmaxhp - unit.maxhp;
                unit.ResetCD(100000000);    // �����ڼ䲻���ƶ�������
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
                        gameinfo.canvas.ShowMsg(string.Format("{0} ������ɡ�", buinfo.name));
                    }
                    else
                    {
                        Debug.Log(string.Format("�����֡��������:{0}  [{1}, {2}]", buinfo.name, row, col));
                    }
                    unit.ResetCD(0);  // ������ɿ����ƶ�������
                }
            }
            else
            {
                // ��ɽ���
                // �����з�����
                if (researching != 0)
                {
                    Research research = player.researches[researching];
                    research.Move(gameinfo);
                }
                // ������ļ����
                if (producing.Count > 0)
                {
                    Produce produce = producing[0];
                    if (produce.Move())
                    {
                        // ��ļ�����
                        int soid = produce.id;
                        player.FinishProduce(this, soid);
                        producing.RemoveAt(0);
                        if (producing.Count == 0)
                        {
                            // �б������
                            Position pos = new Position(player.isclient, row, col);
                            gameinfo.infobar.statuses.Enqueue(new StatusMsg(9, pos));
                        }
                        if (player.isclient == gameinfo.client)
                        {
                            gameinfo.canvas.ShowMsg(string.Format("{0} ��ļ��ɡ�", gameinfo.soldiermap[produce.id].name));
                        }
                    }
                }
                if (frame % 50 == 0)
                {
                    // ���뽨����ɲ��ܽ�������
                    // ��������Ͳ���
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
        // finish = true ��ʾֱ�����
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
        // �������ƻ�ʱ�����᷵�������з��ĿƼ����ϣ����ǻ᷵����ļ������δ����ļ�ı��ֲ���
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