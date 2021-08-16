using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 存储有关战斗的信息
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

    public Projectile projectile;   // id为0表示不能攻击
    public ProjectionInfo prinfo;

    private float speedbase = 0.0625f;
    public float speed;

    public int attackspeed;
    public UnitInfo.AttackRange attackrange;     // 攻击范围

    public Buff buff;       // 战斗buff
    private Dictionary<string, int> tagdict;

    public float viewr, viewx, viewy;
    ///////////////////////////////// 以下属性需要个性化初始化
    public Soldier soldier = null;
    public Building building = null;
    public Buff basebuff;   // 全局buff
    private float width;    // 宽度，建筑10，士兵5
    public Vector3 dir; // 前进方向，需要归一化

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
        // 士兵的碰撞盒大小是5
        List<Unit> enemies = new List<Unit>();
        // < 0 表示连本行都不会攻击
        if (attackrange.left < 0) return enemies;
        List<List<Unit>> units = players.units;
        // 优先本行
        foreach (Unit obj in units[row])
        {
            if (CanAttack(obj)) enemies.Add(obj);
        }
        // 再考虑隔行
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
        // 判定顺序，优先打离老家最近的
        if (client == 0)
        {
            // 老家在左
            enemies.Sort();
        }
        else if (client == 1)
        {
            // 老家在右
            enemies.Sort((x, y) => -x.CompareTo(y));
        }

        // 多目标
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
            // 自己不能打自己
            if (Math.Abs(row - obj.row) <= attackrange.left)
            {
                float lself, rself, lobj, robj;
                GetBorder(out lself, out rself);
                obj.GetBorder(out lobj, out robj);
                if (dir.x > 0)
                {
                    // 朝右走
                    return (lobj - rself <= attackrange.forward) &&
                        (lself - robj <= attackrange.back);
                }
                else
                {
                    // 朝左走
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
        // 对于建筑来说，建造期间的calmdown恒大于0，在Building.Move中进行控制
        if (calmdown > 0)
        {
            // 冷却
            calmdown--;
        }
        else
        {
            if (projectile.id > 0)
            {
                List<Unit> enemies = FindEnemy();
                // 插入时机：获取攻击目标后
                players.gameinfo.skills.TriggerSkills(this, EVENT.AFTER_FIND_ENEMIES, enemies);
                // 插入时机：发动攻击之前
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
            if (calmdown == 0)   // 这里判断不能省略，因为有些技能会在AFTER_CHOOSE_OBJECT时机里把calmdown重置
            {
                transform.localPosition = transform.localPosition + dir * speed * (1 + (basebuff.speed + buff.speed) / 100f);
            }
        }

    }
    public HpChange TakeDamage(Damage damage)
    {
        // 计算受伤方buff
        // 未考虑身上的buff，仅考虑全局buff
        damage.CalDamaged(basebuff);
        damage.CalDamaged(buff);

        // 插入时机：受到伤害前
        skillmanager.TriggerSkills(this, EVENT.BEFORE_DAMAGED, damage);

        HpChange hpchange = new HpChange(hp, hp - damage.GetDamage());
        // 防止伤害溢出
        if(hpchange.later < 0)
        {
            hpchange.later = 0;
        }
        hp = hpchange.later;

        // 更新血条和伤害指示
        if (type == Type.SOLDIER)
        {
            soldier.SetHpBar();
            soldier.DamageTip(damage);
        }
        else if (type == Type.BUILDING)
        {
            building.DamageTip(damage);
        }

        // 插入时机：受到伤害后
        if (damage.GetDamage() > 0)
        {
            skillmanager.TriggerSkills(this, EVENT.AFTER_DAMAGED, damage);
        }

        // 插入时机：生命值改变后
        if (hpchange.before != hpchange.later)
        {
            skillmanager.TriggerSkills(this, EVENT.HPCHANGE, hpchange);
        }

        return hpchange;
    }
    public HpChange RestoreHp(Heal heal)
    {
        // 插入时机：受到治疗前
        skillmanager.TriggerSkills(this, EVENT.BEFORE_HEALED, heal);

        HpChange hpchange = new HpChange(hp, hp + heal.value);
        if(hpchange.later > maxhp)
        {
            hpchange.later = maxhp;
        }
        hp = hpchange.later;
        heal.value = hpchange.later - hpchange.before;
        
        // 插入时机：生命值改变后
        if (hpchange.before != hpchange.later)
        {
            // 更新血条和伤害指示
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
    // 重置CD,传入cd，表明再过这么长时间才能再次移动/攻击。参数省略表示默认CD
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
            throw new InvalidOperationException("比较失败，只能和soldier进行比较");
        }
        return CompareTo((Unit)obj);
    }
}
