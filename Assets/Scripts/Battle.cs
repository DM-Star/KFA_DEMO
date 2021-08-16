using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buff
{
    public int[] defend;
    public int attack_speed;
    public int attack;
    public int speed;
    public int maxhp;
    public int food;
    public int iron;
    public int time;
    public List<string> skills;
    public Buff()
    {
        defend = new int[6] { 0, 0, 0, 0, 0, 0 };
        attack_speed = 0;
        attack = 0;
        speed = 0;
        maxhp = 0;
        food = 0;
        iron = 0;
        time = 0;
        skills = new List<string>();
    }
    public Buff(UnitInfo info):this()
    {
        defend[1] = info.hitdefend;
        defend[2] = info.cutdefend;
        defend[3] = info.wavedefend;
        defend[4] = info.meleedefend;
        defend[5] = info.rangeddefend;
        skills = new List<string>(info.skills);

    }
    public void AddBuff(ResearchInfo.BuffInfo info)
    {
        for (int i = 0; i < info.effects.Count; i++)
        {
            // 1加攻击，2加移速，3加生命上限和生命
            switch (info.effects[i])
            {
                case 5: // 加粮食消耗
                    food += info.values[i];
                    break;
                case 6: // 加钢铁消耗
                    iron += info.values[i];
                    break;
                case 10: // 加建造/研发/训练时间
                    time += info.values[i];
                    break;
                case 12: // 加生命上限
                    maxhp += info.values[i];
                    break;
                case 23: // 加攻击
                    attack += info.values[i];
                    break;
                case 24: // 加攻速
                    attack_speed += info.values[i];
                    break;
                case 27: // 加各种抗性
                case 28:
                case 29:
                case 30:
                case 31:
                    defend[info.effects[i] - 26] += info.values[i];
                    break;
                case 32: // 加移速
                    speed += info.values[i];
                    break;
                case 33: // 加技能
                    foreach (string skill in info.skills)
                    {
                        skills.Add(skill);
                    }
                    break;
            }
        }
    }
}

public class Damage
{
    private GameInfo gameinfo;
    public int commondamage;
    private int realdamage;
    public int attack_type; // 攻击类型：1肉搏 2兵刃 3气功
    public int attack_mode; // 攻击模式：4近战 5远程
    public int delta = 0;
    public Unit from, to;
    public string skill;
    public Damage(int dmg, int att_type, int att_mode, Unit source, Unit target, string skillname = "")
    {
        commondamage = dmg;
        realdamage = 0;
        attack_type = att_type;
        attack_mode = att_mode;
        delta = 0;
        from = source;
        to = target;
        skill = skillname;
        gameinfo = from.players.gameinfo;
    }

    public void Do()
    {
        if (to.alive)
        {
            // 计算攻方buff
            CalDamage(from.basebuff);
            CalDamage(from.buff);

            // 插入时机：造成伤害前
            gameinfo.skills.TriggerSkills(from, EVENT.BEFORE_DAMAGE, this);

            commondamage = Mathf.Max(1, commondamage + delta);
            delta = 0;

            // 目标受到伤害
            to.TakeDamage(this);

            // 待插入时机：造成伤害后
            gameinfo.skills.TriggerSkills(from, EVENT.AFTER_DAMAGE, this);
        }
    }

    // 增加真实伤害
    public void AddRealDamage(int damage)
    {
        realdamage += damage;
    }
    // 返回伤害结构体的伤害值（伤害+真实伤害）
    public int GetDamage()
    {
        int damage = Mathf.Max(1, commondamage + delta) + realdamage;
        return damage;
    }

    // 传入攻击方的属性buff，计算相关的增伤
    public void CalDamage(Buff buff)
    {
        commondamage += buff.attack;
    }

    // 传入受伤方的buff，计算相关的减伤
    public void CalDamaged(Buff buff)
    {
        delta -= buff.defend[attack_type];     // 肉搏 兵刃 气功
        // 近战 远程
        delta -= buff.defend[attack_mode];
    }
}
public class Heal
{
    public Unit source;
    public Unit target;
    public int value;
    public Heal(Unit from, Unit to, int hp)
    {
        source = from;
        target = to;
        value = hp;
    }
}
public class HpChange
{
    public int before;
    public int later;
    public HpChange(int befor, int late)
    {
        before = befor;
        later = late;
    }
}