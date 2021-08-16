using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �洢�й�ս������Ϣ
/// </summary>
public class Unit:IComparable
{
    public enum Type{
        BUILDING, SOLDIER
    };

    public Type type;

    public Players players;
    private SkillManager skillmanager;
    public Transform transform;
    public int client;
    public int row;

    public int id;

    public int maxhp;
    public int hp;
    public bool alive;

    public Projectile projectile;   // idΪ0��ʾ���ܹ���
    public ProjectionInfo prinfo;

    private float speedbase = 0.0625f;
    public float speed;

    public int attackspeed;
    public UnitInfo.AttackRange attackrange;     // ������Χ

    public Buff buff;       // ս��buff
    private Dictionary<string, int> tagdict;

    public float viewr, viewx, viewy;
    ///////////////////////////////// ����������Ҫ���Ի���ʼ��
    public Soldier soldier = null;
    public Building building = null;
    public Buff basebuff;   // ȫ��buff
    private float width;    // ��ȣ�����10��ʿ��5
    public Vector3 dir; // ǰ��������Ҫ��һ��

    public Unit(GameInfo gameinfo, Component father, UnitInfo unitinfo, int r, int cl)
    {
        type = unitinfo.type;
        players = gameinfo.players;
        skillmanager = players.gameinfo.skills;
        transform = father.transform;
        client = cl;
        row = r;

        id = unitinfo.id;
        maxhp = unitinfo.maxhp;
        hp = unitinfo.hp;
        alive = true;

        projectile = unitinfo.projectile;
        if (projectile.id != 0)
        {
            prinfo = gameinfo.projectionmap[unitinfo.projectile.id];
        }

        speed = speedbase * unitinfo.speed;

        attackspeed = unitinfo.attackspeed;
        attackrange = unitinfo.attackrange;
        viewr = unitinfo.eyesight.r;
        viewx = unitinfo.eyesight.x;
        viewy = unitinfo.eyesight.y;

        buff = new Buff();
        tagdict = new Dictionary<string, int>();
    }

    public Unit(GameInfo gameinfo, Soldier father, int r, int cl):
        this(gameinfo, father, father.soinfo.unitinfo, r, cl)
    {
        soldier = father;
        basebuff = players.GetPlayer(client).soldierbuff[id];
        foreach(string skill in basebuff.skills)
        {
            players.AddUnitToSkill(skill, this);
        }
        maxhp += (basebuff.maxhp + buff.maxhp);
        hp = maxhp;
        width = 5;
        if (client == 0)
        {
            dir = new Vector3(1, 0);
        }
        else if (client == 1)
        {
            dir = new Vector3(-1, 0);
            father.GetComponent<SpriteRenderer>().flipX = true;
        }
    }
    public Unit(GameInfo gameinfo, Building father, int r, int cl):
        this(gameinfo, father, father.buinfo.unitinfo, r, cl)
    {
        building = father;
        basebuff = players.GetPlayer(client).buildingbuff[id];
        foreach (string skill in basebuff.skills)
        {
            players.AddUnitToSkill(skill, this);
        }
        maxhp = hp;
        width = 10;
        if (client == 0)
        {
            dir = new Vector3(1, 0);
        }
        else if (client == 1)
        {
            dir = new Vector3(-1, 0);
        }
    }
    private List<Unit> FindEnemy()
    {
        // ʿ������ײ�д�С��5
        List<Unit> enemies = new List<Unit>();
        // < 0 ��ʾ�����ж����ṥ��
        if (attackrange.left < 0) return enemies;
        List<List<Unit>> units = players.units;
        // ���ȱ���
        foreach (Unit obj in units[row])
        {
            if (CanAttack(obj)) enemies.Add(obj);
        }
        // �ٿ��Ǹ���
        for(int i = 1; i <= attackrange.left; i++)
        {
            int up = row - i;
            if(up >= 0)
            {
                foreach (Unit obj in units[up])
                {
                    if (CanAttack(obj)) enemies.Add(obj);
                }
            }
            int down = row + i;
            if(down < units.Count)
            {
                foreach (Unit obj in units[down])
                {
                    if (CanAttack(obj)) enemies.Add(obj);
                }
            }
        }
        // �ж�˳�����ȴ����ϼ������
        if (client == 0)
        {
            // �ϼ�����
            enemies.Sort();
        }
        else if (client == 1)
        {
            // �ϼ�����
            enemies.Sort((x, y) => -x.CompareTo(y));
        }

        // ��Ŀ��
        if (projectile.lockobj == 1)
        {
            if (enemies.Count > projectile.value)
            {
                enemies.RemoveRange(projectile.value, enemies.Count - projectile.value);
            }
        }

        return enemies;
    }
    public bool CanAttack(Unit obj)
    {
        if (client != obj.client)
        {
            // �Լ����ܴ��Լ�
            if (Math.Abs(row - obj.row) <= attackrange.left)
            {
                float lself, rself, lobj, robj;
                GetBorder(out lself, out rself);
                obj.GetBorder(out lobj, out robj);
                if (dir.x > 0)
                {
                    // ������
                    return (lobj - rself <= attackrange.forward) &&
                        (lself - robj <= attackrange.back);
                }
                else
                {
                    // ������
                    return (lself - robj <= attackrange.forward) &&
                        (lobj - rself <= attackrange.back);
                }
            }
        }
        return false;
    }
    public void GetBorder(out float left, out float right)
    {
        left = transform.localPosition.x - width / 2;
        right = transform.localPosition.x + width / 2;
    }
    private int calmdown = 0;
    public void Move()
    {
        // ���ڽ�����˵�������ڼ��calmdown�����0����Building.Move�н��п���
        if (calmdown > 0)
        {
            // ��ȴ
            calmdown--;
        }
        else
        {
            if (projectile.id > 0)
            {
                List<Unit> enemies = FindEnemy();
                // ����ʱ������ȡ����Ŀ���
                players.gameinfo.skills.TriggerSkills(this, EVENT.AFTER_FIND_ENEMIES, enemies);
                // ����ʱ������������֮ǰ
                players.gameinfo.skills.TriggerSkills(this, EVENT.BEFORE_ATTACK, enemies);
                if (enemies.Count > 0)
                {
                    if (projectile.lockobj == 1)
                    {
                        foreach (Unit enemy in enemies)
                        {
                            Projection projection = players.gameinfo.background.InsProjection();
                            projection.Init(prinfo, projectile, transform.localPosition, client, this, enemy, players);
                            players.projections.Add(projection);
                        }
                    }
                    else if (projectile.lockobj == 0)
                    {
                        for (int i = -projectile.value; i <= projectile.value; i++)
                        {
                            int currow = i + row;
                            if (currow >= 0 && currow < players.gameinfo.row)
                            {
                                Projection projection = players.gameinfo.background.InsProjection();
                                projection.Init(prinfo, projectile, transform.localPosition + new Vector3(0, -10 * i, 0), client, this, enemies[0], players);
                                players.projections.Add(projection);
                            }
                        }
                    }
                    ResetCD();
                }
            }
            if (calmdown == 0)   // �����жϲ���ʡ�ԣ���Ϊ��Щ���ܻ���AFTER_CHOOSE_OBJECTʱ�����calmdown����
            {
                transform.localPosition = transform.localPosition + dir * speed * (1 + (basebuff.speed + buff.speed) / 100f);
            }
        }

    }
    public HpChange TakeDamage(Damage damage)
    {
        // �������˷�buff
        // δ�������ϵ�buff��������ȫ��buff
        damage.CalDamaged(basebuff);
        damage.CalDamaged(buff);

        // ����ʱ�����ܵ��˺�ǰ
        skillmanager.TriggerSkills(this, EVENT.BEFORE_DAMAGED, damage);

        HpChange hpchange = new HpChange(hp, hp - damage.GetDamage());
        // ��ֹ�˺����
        if(hpchange.later < 0)
        {
            hpchange.later = 0;
        }
        hp = hpchange.later;

        // ����Ѫ�����˺�ָʾ
        if (type == Type.SOLDIER)
        {
            soldier.SetHpBar();
            soldier.DamageTip(damage);
        }
        else if (type == Type.BUILDING)
        {
            building.DamageTip(damage);
        }

        // ����ʱ�����ܵ��˺���
        if (damage.GetDamage() > 0)
        {
            skillmanager.TriggerSkills(this, EVENT.AFTER_DAMAGED, damage);
        }

        // ����ʱ��������ֵ�ı��
        if (hpchange.before != hpchange.later)
        {
            skillmanager.TriggerSkills(this, EVENT.HPCHANGE, hpchange);
        }

        return hpchange;
    }
    public HpChange RestoreHp(Heal heal)
    {
        // ����ʱ�����ܵ�����ǰ
        skillmanager.TriggerSkills(this, EVENT.BEFORE_HEALED, heal);

        HpChange hpchange = new HpChange(hp, hp + heal.value);
        if(hpchange.later > maxhp)
        {
            hpchange.later = maxhp;
        }
        hp = hpchange.later;
        heal.value = hpchange.later - hpchange.before;
        
        // ����ʱ��������ֵ�ı��
        if (hpchange.before != hpchange.later)
        {
            // ����Ѫ�����˺�ָʾ
            if (type == Type.SOLDIER)
            {
                soldier.SetHpBar();
                soldier.HealTip(heal);
            }
            else if (type == Type.BUILDING)
            {
                building.HealTip(heal);
            }
            skillmanager.TriggerSkills(this, EVENT.HPCHANGE, hpchange);
        }
        return hpchange;
    }
    public List<Unit> FindUnitsBySkill(string skill)
    {
        if (players.skillunit.ContainsKey(skill))
            return players.skillunit[skill];
        else return new List<Unit>();
    }
    // ����CD,����cd�������ٹ���ô��ʱ������ٴ��ƶ�/����������ʡ�Ա�ʾĬ��CD
    public void ResetCD(int cd = -1)
    {
        if (cd == -1) calmdown = (int)(attackspeed / (1 + (basebuff.attack_speed + buff.attack_speed) / 100f));
        else calmdown = cd;
    }
    public bool HasSkill(string skill)
    {
        return basebuff.skills.Contains(skill) || buff.skills.Contains(skill);
    }
    public int TagNum(string tag)
    {
        if (tagdict.ContainsKey(tag))
        {
            return tagdict[tag];
        }
        else return 0;
    }
    public void AddTag(string tag, int num = 1)
    {
        if (tagdict.ContainsKey(tag))
        {
            tagdict[tag] += num;
        }
        else
        {
            tagdict.Add(tag, num);
        }
    }
    public void SetTag(string tag, int num = 1)
    {
        if (tagdict.ContainsKey(tag))
        {
            tagdict[tag] = num;
        }
        else
        {
            tagdict.Add(tag, num);
        }
    }
    public void RemoveAllTags(string tag)
    {
        if (tagdict.ContainsKey(tag))
        {
            tagdict.Remove(tag);
        }
    }
    public int CompareTo(Unit other)
    {
        return Convert.ToInt32(transform.position.x - other.transform.position.x);
    }
    int IComparable.CompareTo(object obj)
    {
        if (!(obj is Unit))
        {
            throw new InvalidOperationException("�Ƚ�ʧ�ܣ�ֻ�ܺ�soldier���бȽ�");
        }
        return CompareTo((Unit)obj);
    }
}
