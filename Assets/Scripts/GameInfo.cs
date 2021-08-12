using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System.Collections;
using System;

public class Info
{
    public int id;
    public string name;
    public string desc;
    public int food;
    public int iron;
    public List<int> pre_building;
    public List<int> pre_research;
    public int time;
    public int building;
    public string path;
    public Info(string[] info)
    {
        pre_building = new List<int>();
        pre_research = new List<int>();
        id = Convert.ToInt32(info[0]);
        name = info[1];
        desc = info[3];
        food = Convert.ToInt32(info[5]);
        iron = Convert.ToInt32(info[6]);
        building = Convert.ToInt32(info[7]);
        string[] prebuildings = info[8].Split('|');
        foreach (string prebuilding in prebuildings)
        {
            pre_building.Add(Convert.ToInt32(prebuilding));
        }

        string[] preresearchs = info[9].Split('|');
        foreach (string presearch in preresearchs)
        {
            pre_research.Add(Convert.ToInt32(presearch));
        }
        time = Convert.ToInt32(info[10]);
        path = info[11];
    }
}
public class BuildingInfo:Info
{
    public int type;    // 1核心 2资源 3军团 4功能 5研发
    public int collect_food;
    public int collect_iron;
    public UnitInfo unitinfo;

    public Sprite[] sprite;

    public BuildingInfo(string[] info):base(info)
    {
        type = Convert.ToInt32(info[2]);

        unitinfo = new UnitInfo(Unit.Type.BUILDING, info);

        collect_food = Convert.ToInt32(info[34]);
        collect_iron = Convert.ToInt32(info[35]);

        sprite = new Sprite[2];
        sprite[0] = sprite[1] = null;
    }
}
public class ResearchInfo:Info
{
    public int max;
    public class BuffInfo
    {
        public int type;
        public List<int> targets;
        public List<int> effects;
        public List<int> values;
        public List<string> skills;
        public BuffInfo()
        {
            targets = new List<int>();
            effects = new List<int>();
            values = new List<int>();
            skills = new List<string>();
        }
        public void SetTargets(string str)
        {
            foreach(string target in str.Split('|'))
            {
                targets.Add(Convert.ToInt32(target));
            }
        }
        public void SetEffects(string str)
        {
            foreach (string effect in str.Split('|'))
            {
                effects.Add(Convert.ToInt32(effect));
            }
        }
        public void SetValues(string str)
        {
            foreach (string value in str.Split('|'))
            {
                values.Add(Convert.ToInt32(value));
            }
        }
        public void SetSkills(string str)
        {
            if (str.Length > 0)
            {
                foreach (string skill in str.Split('|'))
                {
                    skills.Add(skill);
                }
            }
        }
    }
    public int buffnum;
    public List<BuffInfo> buffinfos;

    public ResearchInfo(string[] info):base(info)
    {
        buffinfos = new List<BuffInfo>();

        max = Convert.ToInt32(info[12]);

        
        buffnum = Convert.ToInt32(info[13]);
        for(int i = 0; i < buffnum; i++)
        {
            BuffInfo buff = new BuffInfo();
            buff.type = Convert.ToInt32(info[14+i]);
            buff.SetTargets(info[15+i]);
            buff.SetEffects(info[16+i]);
            buff.SetValues(info[17+i]);
            buff.SetSkills(info[18+i]);
            buffinfos.Add(buff);
        }
    }
}
// 用于记录每一种Unit的发射物信息
public class Projectile
{
    public int id;
    public float speed;
    public float distance;
    public int lockobj;    // 是否锁定目标，1锁定，0不锁
    public int value;   // 锁定：最大目标数   不锁：线数
    public int destroy; // 锁定时：如果有目标死亡是否销毁发射物  1销毁，0继续飞行
    public int crossnum;    // 穿透数，飞行过程中最多击中多少敌人
    public int attack;  // 攻击力
    public int attack_type; // 攻击类型：1肉搏 2兵刃 3气功
    public int attack_mood; // 攻击方式：4近战 5远程
    public Projectile(string[] info)
    {
        id = Convert.ToInt32(info[14]);
        if (id != 0)
        {
            attack_type = Convert.ToInt32(info[15]);
            attack_mood = Convert.ToInt32(info[16]);
            speed = (float)Convert.ToDouble(info[17]);
            distance = (float)Convert.ToDouble(info[18]);
            lockobj = Convert.ToInt32(info[19]);
            value = Convert.ToInt32(info[20]);
            destroy = Convert.ToInt32(info[21]);
            crossnum = Convert.ToInt32(info[22]);
            attack = Convert.ToInt32(info[23]);
        }
    }
}
public class UnitInfo
{
    public Unit.Type type;
    public int id;
    public int maxhp;
    public int hp;
    public float speed;
    public int hitdefend;
    public int cutdefend;
    public int wavedefend;
    public int meleedefend;
    public int rangeddefend;
    public int attackspeed;
    public List<string> skills;
    public struct AttackRange
    {
        public float back;
        public float forward;
        public int left;
        public AttackRange(string info)
        {
            string[] str = info.Split('|');
            back = (float)Convert.ToDouble(str[0]);
            forward = (float)Convert.ToDouble(str[1]);
            left = Convert.ToInt32(str[2]);
        }
    };
    public AttackRange attackrange;
    public Projectile projectile;
    public UnitInfo(Unit.Type unittype, string[] info)
    {
        type = unittype;

        id = Convert.ToInt32(info[0]);

        maxhp = Convert.ToInt32(info[12]);
        hp = Convert.ToInt32(info[13]);

        projectile = new Projectile(info);
        if (projectile.id != 0)
        {
            attackspeed = Convert.ToInt32(info[24]);
            attackrange = new AttackRange(info[25]);
        }

        hitdefend = Convert.ToInt32(info[27]);
        cutdefend = Convert.ToInt32(info[28]);
        wavedefend = Convert.ToInt32(info[29]);
        meleedefend = Convert.ToInt32(info[30]);
        rangeddefend = Convert.ToInt32(info[31]);

        speed = (float)Convert.ToDouble(info[32]);

        skills = new List<string>();
        if (info[33].Length > 0)
        {
            string[] skillsstr = info[33].Split('|');
            foreach (string skill in skillsstr)
            {
                skills.Add(skill);
            }
        }
    }
}
public class SoldierInfo:Info
{
    public int job;
    public UnitInfo unitinfo;
    public Sprite sprite;
    public SoldierInfo(string[] info):base(info)
    {
        job = Convert.ToInt32(info[2]);
        unitinfo = new UnitInfo(Unit.Type.SOLDIER, info);
        sprite = null;
    }
}
public class ProjectionInfo {
    public int id;
    public float height;
    public float width;
    public Sprite sprite;
    public ProjectionInfo(string[] info)
    {
        id = Convert.ToInt32(info[0]);
        height = 0;
        width = 0;
        sprite = null;
    }
}
public class GameInfo : MonoBehaviour
{
    // 游戏模式：0图鉴 1标准 2扩展 
    public int gamemode;
    public int row, playercolumn, centercolumn;
    public Dictionary<int, BuildingInfo> buildingmap;
    public Dictionary<int, ResearchInfo> researchmap;
    public Dictionary<int, SoldierInfo> soldiermap;
    public Dictionary<int, ProjectionInfo> projectionmap;
    public InfoBar infobar;
    public BackGround background;
    public Players players;
    public Cvs canvas;
    public MainCamera maincamera;
    public int client;
    public bool start;
    public SkillManager skills;
    // 核心建筑id列表
    public List<int> corelist;
    // Start is called before the first frame update
    void Awake()
    {
        buildingmap = new Dictionary<int, BuildingInfo>();
        researchmap = new Dictionary<int, ResearchInfo>();
        soldiermap = new Dictionary<int, SoldierInfo>();
        projectionmap = new Dictionary<int, ProjectionInfo>();
        corelist = new List<int>();
        skills = new SkillManager();
        start = false;
        coroutinenum = 100;
    }

    // Update is called once per frame
    void Update()
    {
        if(coroutinenum == 0)
        {
            start = true;
        }
        if (start)
        {
            canvas.Adapt();
        }
    }

    string ReadFile(string path)
    {
        path = Path.Combine(Application.streamingAssetsPath, path);
        UnityWebRequest uwr = UnityWebRequest.Get(path);
        uwr.SendWebRequest();
        while (!uwr.downloadHandler.isDone) {; }
        string str = uwr.downloadHandler.text;
        uwr.Dispose();
        return str;
    }
    void ReadBuildingInfo()
    {
        string[] text = ReadFile("building.csv").Split('\n');
        int i = 0;
        foreach(string line in text)
        {
            if (i != 2)
            {
                i++;
                continue;
            }
            if (line.Length > 0)
            {
                string[] info = line.Split('\t');
                string[] modes = info[4].Split('|');
                foreach (string mode in modes)
                {
                    if (System.Convert.ToInt32(mode) == gamemode)
                    {
                        int id = System.Convert.ToInt32(info[0]);
                        BuildingInfo buinfo = new BuildingInfo(info);
                        buildingmap.Add(id, buinfo);
                        if(buinfo.type == 1)
                        {
                            // 核心建筑加入列表
                            corelist.Add(id);
                        }
                        break;
                    }
                }
            }
        }
    }
    void ReadResearchInfo()
    {
        string[] text = ReadFile("research.csv").Split('\n');
        int i = 0;
        foreach(string line in text)
        {
            if (i != 2)
            {
                i++;
                continue;
            }
            if (line.Length > 0)
            {
                string[] info = line.Split('\t');
                string[] modes = info[4].Split('|');
                foreach (string mode in modes)
                {
                    if (System.Convert.ToInt32(mode) == gamemode)
                    {
                        int id = System.Convert.ToInt32(info[0]);
                        researchmap.Add(id, new ResearchInfo(info));
                        break;
                    }
                }
            }
        }
    }
    void ReadSoldierInfo()
    {
        string[] text = ReadFile("soldier.csv").Split('\n');
        int i = 0;
        foreach(string line in text)
        {
            if(i != 2)
            {
                i++;
                continue;
            }
            if(line.Length > 0)
            {
                string[] info = line.Split('\t');
                string[] modes = info[4].Split('|');
                foreach(string mode in modes)
                {
                    if(System.Convert.ToInt32(mode) == gamemode)
                    {
                        int id = System.Convert.ToInt32(info[0]);
                        soldiermap.Add(id, new SoldierInfo(info));
                        break;
                    }
                }
            }
        }
    }
    void ReadProjectionInfo()
    {
        string[] text = ReadFile("projection.csv").Split('\n');
        int i = 0;
        foreach (string line in text)
        {
            if (i != 2)
            {
                i++;
                continue;
            }
            if (line.Length > 0)
            {
                string[] info = line.Split('\t');
                int id = System.Convert.ToInt32(info[0]);
                projectionmap.Add(id, new ProjectionInfo(info));
                string path = info[1];
                path = Path.Combine(Application.streamingAssetsPath, "projection", path);
                StartCoroutine(LoadProjectionSprite(path, projectionmap[id]));
            }
        }
    }
    IEnumerator LoadProjectionSprite(string path, ProjectionInfo info)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(path))
        {
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(uwr);
                int pixelperunit = 20;
                info.height = (float)tex.height / pixelperunit;
                info.width = (float)tex.width / pixelperunit;
                info.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), pixelperunit);
            }
            uwr.Dispose();
        }
    }
    public int coroutinenum;    // 必须等所有协成结束后才能完成初始化
    public void Init(int ifclient, int mode = 1)
    {
        coroutinenum = 0;
        // 应根据实际情况赋值
        gamemode = mode;
        client = ifclient;
        ReadBuildingInfo();
        ReadResearchInfo();
        ReadSoldierInfo();
        ReadProjectionInfo();

        InitBackGround();

        InitPlayers();  // 必须先初始化BackGround

        InitCvs();
        InitInfoBar();  // 必须先初始化Cvs

        InitSkills();
        InitMainCamera();
    }
    public void GameStart()
    {
        // 游戏开始，第一次Move之前
        // 建造初始建筑
        players.GameStart(this);
    }
    private void InitSkills()
    {
        skills.Init();
    }
    private void InitInfoBar()
    {
        infobar.Init();
    }
    private void InitBackGround()
    {
        // 将来出新模式以后改为可变
        background.Init(row, playercolumn, centercolumn);
    }
    private void InitPlayers()
    {
        players.Init(this);
    }
    private void InitCvs()
    {
        canvas.Init();
    }
    private void InitMainCamera()
    {
        maincamera.Init(37, -12, 12, -6, client);
    }
}
