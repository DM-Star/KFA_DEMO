using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClickMsg
{
    // ��ť���ͣ�1�������Ͱ�ť 2������ť 3�Ƽ���ť 4���� 5���ְ�ť
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
    // ���ܽ������Ƽ�״̬������Ϣ
    // ��Ϣ���ͣ�1��ʼ���� 2������� 3ȡ������ 4��������
    // 5��ʼ�з� 6�з���� 7��ֹ�з�
    // 8��ʼ��ļ��֮ǰ��ļ����Ϊ�գ� 9��ļ����ȫ����ļ��� 10��ļ������ȫ���
    public int msg; 
    public Position pos;
    public int id;  // ��ĳЩ�����Ҫ�ã�������ʱ��Ҫ��¼������ı��
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
    // ��ť״̬
    public int buildingtype;// ��ҵ�ǰѡ���Ľ������Ͱ�ť��Ӧ�Ľ�������id��0��ʾδѡ��
    public int building;    // ��ҵ�ǰѡ���Ľ�����ť��Ӧ�Ľ���id��0��ʾδѡ��
    public int research;    // ��ҵ�ǰѡ���ĿƼ���ť��Ӧ�ĿƼ�id��0��ʾδѡ��
    public int soldier;     // ��ҵ�ǰѡ����ʿ����ť��Ӧ�ĿƼ�id��0��ʾδѡ��
    public Position grid;   // ��ҵ�ǰѡ�еĸ��ӣ�right = -1��ʾδѡ��
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
    // ���ڹ���ʵ��
    public BtnBuildingType btnbuildingtype;
    public BtnBuilding btnbuilding;
    public BtnBuilding btnresearch;
    public BtnBuilding btnsoldier;

    // ���°�ť�Ķ���
    public Queue<ClickMsg> clicked;
    // �������Ƽ�״̬����
    public Queue<StatusMsg> statuses;

    // 1���� 2��Դ 3���� 4���� 5�з�
    // δ�����ܻ��е����ֽ�����𣬺�ңԶ
    private List<BtnBuildingType> btnbuildingtypes;
    private SortedDictionary<int, List<BtnBuilding>> btnbuildings;
    private SortedDictionary<int, List<BtnBuilding>> btnresearchs;
    private SortedDictionary<int, List<BtnBuilding>> btnsoldiers;
    // �Ƽ�idӳ�䵽�Ƽ���ť�������ڰ�ť��չʾ���н���
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
        // ���������Ͱ�ť�����¼�
        BtnClicked();
        // ���������Ƽ�״̬����
        StatusChange();
        UpdateProgressBar();
        ShowResourceInfo();
        ShowAvatar();
    }
    // ��������֡�ź�
    public void RecvSignal(int type, Position pos, int id)
    {
        Player player = gameinfo.players.GetPlayer(pos.right);
        Building target = gameinfo.players.GetBuilding(pos);
        int row = pos.row;
        int col = pos.col;
        bool self = (pos.right == gameinfo.client);
        switch (type)
        {
            case 1:     // ����
                {
                    BuildingInfo tobuild = gameinfo.buildingmap[id];
                    player.StartBuild(row, col, tobuild);
                    statuses.Enqueue(new StatusMsg(1, pos, id));
                    if (self)
                    {
                        Log(string.Format("��ʼ���죺{0}��Ԥ�ƺ�ʱ��{1}�롣", tobuild.name, tobuild.time / 50));
                    }
                    break;
                }
            case 2:     // ȡ������
                {
                    if (target.status < target.maxstatus)
                    {
                        // ȡ������
                        statuses.Enqueue(new StatusMsg(3, pos, id));
                        BuildingInfo buildingInfo = gameinfo.buildingmap[id];
                        player.CancelBuild(row, col, buildingInfo);
                        if (self)
                        {
                            Log(string.Format("ȡ�����죺{0}��", buildingInfo.name));
                        }
                    }
                    else
                    {
                        // �ݻٽ���
                        statuses.Enqueue(new StatusMsg(4, pos, id));
                        BuildingInfo buildingInfo = gameinfo.buildingmap[id];
                        player.CancelBuild(row, col, buildingInfo);
                        if (self)
                        {
                            Log(string.Format("���������{0}��", buildingInfo.name));
                        }
                    }
                    break;
                }
            case 3:     // �з�
                {
                    ResearchInfo toresearch = gameinfo.researchmap[id];
                    player.StartResearch(row, col, toresearch);
                    statuses.Enqueue(new StatusMsg(5, pos, id));
                    if (self)
                    {
                        Log(string.Format("��ʼ�з���{0}��Ԥ�ƺ�ʱ��{1}�롣", toresearch.name,
                            (toresearch.time * (100 + player.researches[id].basebuff.time) / 100f / 50).ToString("0.0"))
                            );
                    }
                    break;
                }
            case 4:     // ȡ���з�
                {
                    statuses.Enqueue(new StatusMsg(7, pos, id));
                    player.CancelResearch(id);
                    if (self)
                    {
                        Log(string.Format("ȡ���з���{0}��", gameinfo.researchmap[id].name));
                    }
                    break;
                }
            case 5:     // ��ļ
                {
                    SoldierInfo toproduce = gameinfo.soldiermap[id];
                    statuses.Enqueue(new StatusMsg(11, pos, id));
                    int qcount = player.StartProduce(target, toproduce);
                    if (qcount == 1)
                    {
                        // �����ǰ����ֻ��1����ļʿ����������Ϣ8
                        // �����ı�����ʽ����
                        statuses.Enqueue(new StatusMsg(8, pos, id));
                        if (self)
                        {
                            Log(string.Format("��ʼ��ļ��{0}��Ԥ�ƺ�ʱ��{1}�롣", toproduce.name, toproduce.time / 50));
                        }
                    }
                    else
                    {
                        if (self)
                        {
                            Log(string.Format("{0} ������ļ���У�����ǰ������ {1} ����ļ����", toproduce.name, qcount - 1));
                        }
                    }
                    break;
                }
            case 6:     // ȡ����ļ
                {
                    string name = gameinfo.soldiermap[target.producing[target.producing.Count - 1].id].name;
                    statuses.Enqueue(new StatusMsg(12, pos, id));
                    int qcount = player.CancelProduce(target);
                    if (qcount == 0)
                    {
                        // �����ļ������գ�������Ϣ10
                        statuses.Enqueue(new StatusMsg(10, pos));
                        if (self)
                        {
                            Log(string.Format("ȡ����ļ��{0}��", name, qcount));
                        }
                    }
                    else
                    {
                        if (self)
                        {
                            Log(string.Format("ȡ����ļ��{0}�������л��� {1} ����ļ����", name, qcount));
                        }
                    }
                    break;
                }
        }
    }
    private void StatusChange()
    {
        // �������/�Ƽ����Ҳ��Ӱ�����������opbar��������Ҫ�ı�ǰ����ɫ
        while (statuses.Count > 0)
        {
            StatusMsg msg = statuses.Dequeue();
            int type = msg.msg;
            Position pos = msg.pos;
            Building building = gameinfo.players.GetBuilding(pos);
            int buid = curbtn.building;
            switch (type)
            {
                case 1:     //��ʼ����
                    {
                        if (curbtn.grid.Equals(pos))
                        {
                            // �����Ϣλ���ǵ��λ��
                            // Ϩ������ť�ͽ������Ͱ�ť
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
                                // Ϩ���찴ť������ȡ����ť
                                btnconfirm.SetActive(false);
                                btncancel.GetComponentInChildren<Text>().text = "ȡ��";
                                btncancel.SetActive(true);
                            }
                            else
                            {
                                operationbar.SetActive(true);
                                ShowEnemyBuilding(gameinfo.buildingmap[msg.id]);
                            }
                            // չʾ������
                            progressbar.SetActive(true);
                        }

                        break;
                    }
                case 2:     // �������
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
                            // �Լ��Ľ���
                            if (curbtn.grid.Equals(pos))
                            {
                                // �����Ϣλ���ǵ��λ��
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
                                            btnconfirm.GetComponentInChildren<Text>().text = "��ļ";
                                            btnconfirm.SetActive(true);
                                        }
                                    }
                                }
                                btncancel.GetComponentInChildren<Text>().text = "���";
                            }
                            else
                            {
                                // ��Ϣ�ǵ��λ��
                                if (buid != 0)
                                {
                                    // ��ǰλ�ô��ڽ���
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
                                    // ��ǰλ�ò����ڽ�����������ҵ���˽�����ť
                                    if (buid != 0)
                                    {
                                        ShowBuildingInfo(gameinfo.buildingmap[buid]);
                                    }
                                }
                            }
                        }
                        break;
                    }
                case 3:     // ȡ������
                    {
                        // չʾ������ť�ͽ������Ͱ�ť
                        if (curbtn.grid.Equals(pos))
                        {
                            if (pos.right == gameinfo.client)
                            {
                                // �Լ�ȡ�������Լ��Ľ���
                                foreach (BtnBuilding btn in btnbuildings[curbtn.buildingtype])
                                {
                                    btn.gameObject.SetActive(true);
                                }
                                for (int i = 0; i < 5; i++)
                                {
                                    btnbuildingtypes[i].gameObject.SetActive(true);
                                }
                                // Ϩ��Ƽ���ť
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
                                // Ϩ�������
                                progressbar.SetActive(false);
                                // �������찴ť��Ϩ��ȡ����ť
                                btnconfirm.GetComponentInChildren<Text>().text = "����";
                                btnconfirm.SetActive(true);
                                btncancel.SetActive(false);
                                ShowBuildingInfo(gameinfo.buildingmap[buid]);
                            }
                            else
                            {
                                //����ȡ�����Լ��Ľ���
                                // Ϩ�������
                                progressbar.SetActive(false);
                                // Ϩ�������
                                operationbar.SetActive(false);
                            }
                        }
                        break;
                    }
                case 4:     // ��������
                    {
                        if (curbtn.grid.Equals(pos))
                        {
                            if (pos.right == gameinfo.client)
                            {
                                // ���ٵ����Լ��Ľ���
                                foreach (BtnBuilding btn in btnbuildings[curbtn.buildingtype])
                                {
                                    btn.gameObject.SetActive(true);
                                }
                                for (int i = 0; i < 5; i++)
                                {
                                    btnbuildingtypes[i].gameObject.SetActive(true);
                                }
                                // Ϩ��Ƽ���ť
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
                                // Ϩ�������
                                progressbar.SetActive(false);
                                // �������찴ť��Ϩ��ȡ����ť
                                btnconfirm.GetComponentInChildren<Text>().text = "����";
                                btnconfirm.SetActive(true);
                                btncancel.SetActive(false);
                                ShowBuildingInfo(gameinfo.buildingmap[buid]);
                            }
                            else
                            {
                                // ���ٵ��ǵ��˵Ľ���
                                // Ϩ�������
                                progressbar.SetActive(false);
                                // Ϩ�������
                                operationbar.SetActive(false);
                            }
                        }
                        else
                        {
                            buid = gameinfo.players.GetBuilding(curbtn.grid).id;
                            // ����Ӱ�쵱ǰ���ӵ�ǰ��
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
                case 5:     // ��ʼ�з�
                    {
                        if (pos.right == gameinfo.client)
                        {
                            foreach (BtnBuilding btn in btnresearchs[building.id])
                            {
                                btn.gameObject.SetActive(false);
                            }
                            // չʾ������
                            progressbar.SetActive(true);
                            // Ϩ���찴ť������ȡ����ť
                            btnconfirm.SetActive(false);
                            btncancel.GetComponentInChildren<Text>().text = "ȡ��";
                            btncancel.SetActive(true);
                        }
                        break;
                    }
                case 6:     // �з����
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
                                // Ϩ���찴ť������ȡ����ť
                                // btnconfirm.GetComponentInChildren<Text>().text = "�з�";
                                // btnconfirm.SetActive(true); // ������������ˣ�Ҳ���ܵ������찴ť
                                ShowBuildingInfo(gameinfo.buildingmap[buid]);
                                btncancel.GetComponentInChildren<Text>().text = "���";
                                btncancel.SetActive(true);

                            }
                            else
                            {
                                // ��Ϣ�ǵ��λ��
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
                case 7:     // ��ֹ�з�
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
                                    btnconfirm.GetComponentInChildren<Text>().text = "�з�";
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

                                btncancel.GetComponentInChildren<Text>().text = "���";
                                btncancel.SetActive(true);
                            }
                        }
                        break;
                    }
                case 8:     // ��ʼ��ļ��֮ǰ��ļ����Ϊ�գ�
                    {
                        if (pos.right == gameinfo.client)
                        {
                            // չʾ������
                            progressbar.SetActive(true);
                            // ����ȡ����ť
                            btncancel.GetComponentInChildren<Text>().text = "ȡ��";
                            btncancel.SetActive(true);
                        }
                        break;
                    }
                case 9:     // ��ļ����ȫ����ļ���
                    {
                        if (curbtn.grid.Equals(pos))
                        {
                            if (pos.right == gameinfo.client)
                            {
                                btncancel.GetComponentInChildren<Text>().text = "���";
                                btncancel.SetActive(true);
                            }
                        }
                        break;
                    }
                case 10:    // ��ļ������ȫ���
                    {
                        if (curbtn.grid.Equals(pos))
                        {
                            if (pos.right == gameinfo.client)
                            {
                                btncancel.GetComponentInChildren<Text>().text = "���";
                                btncancel.SetActive(true);
                            }
                        }
                        break;
                    }
                case 11:    // ����һ����ļʿ��
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
                case 12:    // ����һ����ļʿ��
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
    // ���������ְ�������ʵʱ����curbtn�����ҽ����ʵ���SetAction(false)����
    private BtnStatus curbtn = new BtnStatus();
    // operatorbar����ĸ����ı����
    private GameObject opbarname, opbardesc, opbarneed;
    public FoodIronbar opbarfoodiron;
    public FoodIronbar rebarfoodiron;
    private GameObject[] opbarpre;
    private void ShowBuildingInfo(BuildingInfo info)
    {
        // ֻ��opbarname opbardesc opbarfoodiron opbarpre
        // ���찴ť�Ͳ����ť�������������
        Player player = gameinfo.players.GetPlayer(gameinfo.client);
        opbarname.GetComponent<Text>().text = info.name;
        opbardesc.GetComponent<Text>().text = info.desc;
        opbarname.SetActive(true);
        opbardesc.SetActive(true);
        int needfood = (int)(info.food * (1 + player.buildingbuff[info.id].food / 100f));
        int neediron = (int)(info.iron * (1 + player.buildingbuff[info.id].iron / 100f));
        opbarfoodiron.SetNum(needfood, neediron);
        opbarfoodiron.SetActive(true);
        // ��������Ҫ����/��Ҫ�Ƽ�
        // ��������������ʱ�����������Ƽ����ʱ����Ҫ��鵱ǰ��InfoBar��������Ҫ��ɫ
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
            opbarneed.GetComponent<Text>().text = "��Ҫ";
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
        // ֻ��opbarname opbardesc opbarfoodiron opbarpre
        // confirm��ť��cancel��ť�������������
        Player player = gameinfo.players.GetPlayer(gameinfo.client);
        opbarname.GetComponent<Text>().text = info.name;
        opbardesc.GetComponent<Text>().text = info.desc;
        opbarfoodiron.SetNum(info.food, info.iron);
        opbarname.SetActive(true);
        opbardesc.SetActive(true);
        opbarfoodiron.SetActive(true);
        // ��������Ҫ����/��Ҫ�Ƽ�
        // ��������������ʱ�����������Ƽ����ʱ����Ҫ��鵱ǰ��InfoBar��������Ҫ��ɫ
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
            opbarneed.GetComponent<Text>().text = "��Ҫ";
        }
        while (prenum < 2)
        {
            opbarpre[prenum].GetComponent<Text>().text = "";
            prenum++;
        }
    }
    private void ShowSoldierInfo(SoldierInfo info)
    {
        // ֻ��opbarname opbardesc opbarfoodiron opbarpre
        // confirm��ť��cancel��ť�������������
        Player player = gameinfo.players.GetPlayer(gameinfo.client);
        opbarname.GetComponent<Text>().text = info.name;
        opbardesc.GetComponent<Text>().text = info.desc;
        int needfood = (int)(info.food * (1 + player.soldierbuff[info.id].food / 100f));
        int neediron = (int)(info.iron * (1 + player.soldierbuff[info.id].iron / 100f));
        opbarfoodiron.SetNum(needfood, neediron);
        opbarname.SetActive(true);
        opbardesc.SetActive(true);
        opbarfoodiron.SetActive(true);
        // ��������Ҫ����/��Ҫ�Ƽ�
        // ��������������ʱ�����������Ƽ����ʱ����Ҫ��鵱ǰ��InfoBar��������Ҫ��ɫ
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
            opbarneed.GetComponent<Text>().text = "��Ҫ";
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
        // ���Խ��е��޸ģ�δ�ﵽ���������Ľ����ͿƼ�����ʾconfirm��ť
        while(clicked.Count > 0)
        {
            ClickMsg msg = clicked.Dequeue();
            int id = msg.id;
            int type = msg.type;
            int client = gameinfo.client;
            Player player;
            switch (type)
            {
                case 1: // �������Ͱ�ť
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
                case 2: // ������ť
                    {
                        curbtn.building = id;
                        btncancel.SetActive(false);
                        btnconfirm.GetComponentInChildren<Text>().text = "����";
                        btnconfirm.SetActive(true);
                        operationbar.SetActive(true);
                        ShowBuildingInfo(gameinfo.buildingmap[id]);
                        break;
                    }
                case 3: // �Ƽ���ť
                    {
                        curbtn.research = id;
                        btncancel.GetComponentInChildren<Text>().text = "���";
                        btncancel.SetActive(true);
                        btnconfirm.GetComponentInChildren<Text>().text = "�з�";
                        btnconfirm.SetActive(true);
                        operationbar.SetActive(true);
                        ShowResearchInfo(gameinfo.researchmap[id]);
                        break;
                    }
                case 4: // ����
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
                                // �ȰѾɵĿƼ���ťϨ��
                                Building oldbuilding = gameinfo.players.GetPlayer(curbtn.grid.right).buildings[curbtn.grid.row, curbtn.grid.col];
                                if (btnresearchs.ContainsKey(oldbuilding.id))
                                {
                                    foreach (BtnBuilding btn in btnresearchs[oldbuilding.id])
                                    {
                                        btn.gameObject.SetActive(false);
                                    }
                                }
                                // �Ѿɵı��ְ�ťϨ��
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
                                // �յ�
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
                                        btnconfirm.GetComponentInChildren<Text>().text = "����";
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
                                // ���ڽ���
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
                                    // ���Լ��Ľ���
                                    // �Ѿ��������
                                    operationbar.SetActive(true);
                                    if (building.status == building.maxstatus)
                                    {
                                        if (btnresearchs.ContainsKey(building.id))
                                        {
                                            // ���û�����з���չʾ������Ϣ 
                                            if (building.researching == 0)
                                            {
                                                foreach (BtnBuilding btn in btnresearchs[building.id])
                                                {
                                                    btn.gameObject.SetActive(true);
                                                }
                                                btncancel.GetComponentInChildren<Text>().text = "���";
                                                btnconfirm.SetActive(false);
                                                ShowBuildingInfo(gameinfo.buildingmap[building.id]);
                                            }
                                            else
                                            {
                                                // �����з�����չʾȡ����ť
                                                int reid = building.researching;
                                                ShowResearchInfo(gameinfo.researchmap[reid]);
                                                btncancel.GetComponentInChildren<Text>().text = "ȡ��";
                                                btnconfirm.SetActive(false);
                                            }
                                        }
                                        else if (btnsoldiers.ContainsKey(building.id))
                                        {
                                            // ��ǰ�Ǿ��Ž���
                                            foreach (BtnBuilding btn in btnsoldiers[building.id])
                                            {
                                                btn.UpdateSoProgreeText(0);
                                                btn.gameObject.SetActive(true);
                                            }
                                            // ����ʿ����ť��ļ����
                                            foreach (Produce produce in building.producing)
                                            {
                                                int soid = produce.id;
                                                btnsoldiermap[soid].UpdateSoProgreeText(1, 1);
                                            }
                                            if (building.producing.Count > 0)
                                            {
                                                // ������ļ����չʾȡ��
                                                btncancel.GetComponentInChildren<Text>().text = "ȡ��";
                                            }
                                            else
                                            {
                                                // δ����ļ����չʾ���
                                                btncancel.GetComponentInChildren<Text>().text = "���";
                                            }

                                            if (gameinfo.soldiermap.ContainsKey(curbtn.soldier))
                                            {
                                                SoldierInfo soinfo = gameinfo.soldiermap[curbtn.soldier];
                                                if (soinfo.building == building.id)
                                                {
                                                    ShowSoldierInfo(soinfo);
                                                    btnconfirm.GetComponentInChildren<Text>().text = "��ļ";
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
                                            btncancel.GetComponentInChildren<Text>().text = "���";
                                            ShowBuildingInfo(gameinfo.buildingmap[building.id]);
                                            btnconfirm.SetActive(false);
                                        }
                                    }
                                    else
                                    {
                                        // ���ڽ���
                                        btncancel.GetComponentInChildren<Text>().text = "ȡ��";
                                        ShowBuildingInfo(gameinfo.buildingmap[building.id]);
                                        btnconfirm.SetActive(false);
                                    }
                                    btncancel.SetActive(true);
                                }
                                else if (right != client)
                                {
                                    // �Ǳ��˵Ľ���
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
                case 5: // ���ְ�ť
                    {
                        curbtn.soldier = id;
                        btncancel.GetComponentInChildren<Text>().text = "���";
                        btncancel.SetActive(true);
                        btnconfirm.GetComponentInChildren<Text>().text = "��ļ";
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
        string[] typestring = {"����", "��Դ", "����", "����", "�з�" };
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
        // Ŀ��λ���ϵ�ǰ��״��
        Building target = gameinfo.players.GetBuilding(curbtn.grid);
        // ������������Ϣ
        BuildingInfo tobuild = gameinfo.buildingmap[curbtn.building];

        // �ж�ʧЧ�󣬿�������һ����Ϣ��

        // ���ж���ҵ���Ľ����ǲ����Լ��Ľ���
        if (right == client)
        {
            // ���������еĽ��������½���
            if (target.id == 0)
            {
                if (tobuild.collect_food <= 0 || target.foodpoint)
                {
                    if (tobuild.collect_iron <= 0 || target.ironpoint)
                    {
                        // ��Ҫ�㹻����Դ
                        int needfood = (int)(tobuild.food * (1 + player.buildingbuff[tobuild.id].food / 100f));
                        if (player.food >= needfood)
                        {
                            int neediron = (int)(tobuild.iron * (1 + player.buildingbuff[tobuild.id].iron / 100f));
                            if (player.iron >= neediron)
                            {
                                // ǰ���Ƽ���ǰ������������
                                int lack;
                                if (player.HasBuildings(tobuild.pre_building, out lack))
                                {
                                    if (player.HasResearches(tobuild.pre_research, out lack))
                                    {
                                        // ���ͽ����ź�
                                        socket.CreateSignal(1, curbtn.grid, tobuild.id);
                                    }
                                    else
                                    {
                                        Log(string.Format("�����з���{0}", gameinfo.researchmap[lack].name));
                                    }
                                }
                                else
                                {
                                    Log(string.Format("���Ƚ��죺{0}", gameinfo.buildingmap[lack].name));
                                }
                            }
                            else
                            {
                                Log(string.Format("��������"));
                            }
                        }
                        else
                        {
                            Log(string.Format("��ʳ����"));
                        }
                    }
                    else
                    {
                        Log(string.Format("{0} ���뽨�ڿ�ɽǰ", tobuild.name));
                    }
                }
                else
                {
                    Log(string.Format("{0} ���뽨��������", tobuild.name));
                }
            }
        }
    }
    private void Research()
    {
        int client = gameinfo.client;
        Player player = gameinfo.players.GetPlayer(client);
        // Ŀ��λ���ϵ�ǰ��״��
        Building target = gameinfo.players.GetBuilding(curbtn.grid);
        int id = curbtn.research;
        // ���з��Ƽ�����Ϣ
        ResearchInfo toresearch = gameinfo.researchmap[id];

        // �Ƽ���������ӦΪ��ǰ���ӽ���
        if (target.id == toresearch.building)
        {
            // ��ǰ����δ���з��Ƽ�
            if (target.researching == 0)
            {
                // ��ǰ�Ƽ�δ���о�
                if (player.researches[id].status == player.researches[id].maxstatus)
                {
                    // ��ǰ�Ƽ�δ���з���������
                    if (player.researches[id].level < toresearch.max || toresearch.max == -1)
                    {
                        // ��Ҫ�㹻����Դ
                        int needfood = toresearch.food;
                        if (player.food >= needfood)
                        {
                            int neediron = toresearch.iron;
                            if (player.iron >= neediron)
                            {
                                // ǰ���Ƽ���ǰ������������
                                int lack;
                                if (player.HasBuildings(toresearch.pre_building, out lack))
                                {
                                    if (player.HasResearches(toresearch.pre_research, out lack))
                                    {
                                        // �����з��ź�
                                        socket.CreateSignal(3, curbtn.grid, id);
                                    }
                                    else
                                    {
                                        Log(string.Format("�����з���{0}", gameinfo.researchmap[lack].name));
                                    }
                                }
                                else
                                {
                                    Log(string.Format("���Ƚ��죺{0}", gameinfo.buildingmap[lack].name));
                                }
                            }
                            else
                            {
                                Log(string.Format("��������"));
                            }
                        }
                        else
                        {
                            Log(string.Format("��ʳ����"));
                        }
                    }
                    else
                    {
                        Log(string.Format("{0} �Ѵﵽ�ȼ����ޡ�", toresearch.name));
                    }
                }
                else
                {
                    Log(string.Format("{0} �Ѿ����о��ˡ�", toresearch.name));
                }
            }
        }
    }
    private void Produce()
    {
        int client = gameinfo.client;
        Player player = gameinfo.players.GetPlayer(client);
        // Ŀ��λ���ϵ�ǰ��״��
        Building target = gameinfo.players.GetBuilding(curbtn.grid);
        int id = curbtn.soldier;
        // ����ļ���ֵ���Ϣ
        SoldierInfo toproduce = gameinfo.soldiermap[id];

        // ������������ӦΪ��ǰ���ӽ���
        if (target.id == toproduce.building)
        {
            // ��Ҫ�㹻����Դ
            int needfood = (int)(toproduce.food * (1 + player.soldierbuff[toproduce.id].food / 100f));
            if (player.food >= needfood)
            {
                int neediron = (int)(toproduce.iron * (1 + player.soldierbuff[toproduce.id].iron / 100f));
                if (player.iron >= neediron)
                {
                    // ǰ���Ƽ���ǰ������������
                    int lack;
                    if (player.HasBuildings(toproduce.pre_building, out lack))
                    {
                        if (player.HasResearches(toproduce.pre_research, out lack))
                        {
                            // ������ļ�ź�
                            socket.CreateSignal(5, curbtn.grid, id);
                        }
                        else
                        {
                            Log(string.Format("�����з���{0}", gameinfo.researchmap[lack].name));
                        }
                    }
                    else
                    {
                        Log(string.Format("���Ƚ��죺{0}", gameinfo.buildingmap[lack].name));
                    }
                }
                else
                {
                    Log(string.Format("��������"));
                }
            }
            else
            {
                Log(string.Format("��ʳ����"));
            }
        }
    }
    private void DeBuild()
    {
        int client = gameinfo.client;
        int right = curbtn.grid.right;
        // Ŀ��λ���ϵ�ǰ��״��
        Building target = gameinfo.players.GetBuilding(curbtn.grid);
        int buid = target.id;
        BuildingInfo todebuild = gameinfo.buildingmap[buid];
        // ���ж���ҵ���Ľ����ǲ����Լ��Ľ���
        if (right == client)
        {
            // ������ڽ���
            if (target.id != 0)
            {
                if (todebuild.type != 1 || gameinfo.players.GetCoreNum(client) > 1)
                {
                    // ���Ͳ���ź�
                    socket.CreateSignal(2, curbtn.grid, buid);
                }
                else
                {
                    Log(string.Format("�㲻�ܲ����Ψһ�ĺ��Ľ���"));
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
        // ���ж���ҵ���Ľ����ǲ����Լ��Ľ���
        if (right == client)
        {
            // ������ڿƼ�
            if (reid != 0)
            {
                // ����ȡ���ź�
                socket.CreateSignal(4, curbtn.grid, reid);
            }
        }
    }
    private void DeProduce()
    {
        int client = gameinfo.client;
        int right = curbtn.grid.right;
        Building building = gameinfo.players.GetBuilding(curbtn.grid);
        // ���ж���ҵ���Ľ����ǲ����Լ��Ľ���
        if (right == client)
        {
            // ���������ļ
            if (building.producing.Count > 0)
            {
                int soid = building.producing[building.producing.Count - 1].id;
                // ����ȡ���ź�
                socket.CreateSignal(6, curbtn.grid, soid);

                // �յ�ȡ���źź���ȡ��
                
            }
        }
    }
    private void Confirm()
    {
        // ���ݲ�ͬ�����Confirm���в�ͬ�ĺ���
        Building building = gameinfo.players.GetBuilding(curbtn.grid);
        int id = building.id;
        if (id == 0)
        {
            // �յؽ���
            Build();
        }
        else if(building.id != 0){
            // ���ǿյأ����ݽ������;���
            int type = gameinfo.buildingmap[id].type;
            switch (type)
            {
                case 5:
                    // �з��ͽ���
                    Research();
                    break;
                case 3:
                    // �����ͽ���
                    Produce();
                    break;
            }
        }
    }
    private void Cancel()
    {
        // ���ݲ�ͬ�����Cancel���в�ͬ�ĺ���
        Building building = gameinfo.players.GetBuilding(curbtn.grid);
        if(building.researching == 0)
        {
            // ��ǰû���з��Ƽ�
            if (building.producing.Count > 0)
            {
                // ��ǰ����ļʿ��
                DeProduce();
            }
            else
            {
                // ��ǰҲû����ļʿ��
                DeBuild();
            }
        }
        else
        {
            // ��ǰ���з��Ƽ�
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
            // δѡ���κεص�ʱ��avatar��ֻ������Ϸ��ʼʱ��һ��
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
                hpbartext.text = string.Format("ʣ���;� ({0} / {1})", hp, maxhp);
                // �����С�������ɡ��з��С�������  buildbarչʾ��ͬ��Ϣ
                int status = building.status;
                int maxstatus = building.maxstatus;
                if (right == gameinfo.client)
                {
                    // �Լ���
                    if (status < maxstatus)
                    {
                        // ������
                        buildbar.gameObject.SetActive(true);
                        buildbar.maxvalue = maxstatus;
                        buildbar.value = status;
                        buildbartext.text = string.Format("{0} ({1} / {2})", buildingInfo.name, status, maxstatus);
                    }
                    else
                    {
                        // �������
                        int reid = building.researching;
                        if (reid != 0)
                        {
                            // �������з��ĿƼ�
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
                            // û�������з��ĿƼ�
                            if (building.producing.Count > 0)
                            {
                                // ��������ļ��ʿ��
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
                                // û��������ļ��ʿ��
                                buildbartext.text = "";
                                buildbar.gameObject.SetActive(false);
                            }

                        }
                    }
                }
                else
                {
                    // ���˲���ʾ������
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
