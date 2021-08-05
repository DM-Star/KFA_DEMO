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
    public int buildtime;
    public List<string> skills;
    public Buff()
    {
        defend = new int[6];
        for (int i = 0; i < 6; i++) defend[i] = 0;
        attack_speed = 0;
        attack = 0;
        speed = 0;
        maxhp = 0;
        food = 0;
        iron = 0;
        buildtime = 0;      // 未启用
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
        switch (info.type)
        {
            case 1: // 建筑
                for (int i = 0; i < info.effects.Count; i++)
                {
                    // 1加攻击，2加移速，3加生命上限和生命
                    switch (info.effects[i])
                    {
                        case 1: // 加生产时间
                            break;
                        case 2: // 加钢铁消耗
                            iron += info.values[i];
                            break;
                        case 3: // 加各种抗性
                        case 4: 
                        case 5: 
                        case 6: 
                        case 7:
                            defend[info.effects[i] - 2] += info.values[i];
                            break;
                        
                    }
                }
                break;
            case 2: // 兵种
            case 5: // 兵种攻击类型
            case 6: // 兵种门派
                for(int i = 0; i < info.effects.Count; i++){
                    // 1加攻击，2加移速，3加生命上限和生命
                    switch (info.effects[i])
                    {
                        case 1: // 加攻击
                            attack += info.values[i];
                            break;
                        case 2: // 加移速
                            speed += info.values[i];
                            break;
                        case 3: // 加生命上限
                            maxhp += info.values[i];
                            break;
                        case 4: // 加粮食消耗
                            food += info.values[i];
                            break;
                        case 5: // 加攻速
                            attack_speed += info.values[i];
                            break;
                        case 6: // 加各种抗性
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                            defend[info.effects[i] - 5] += info.values[i];
                            break;
                        case 11: // 加技能
                            foreach (string skill in info.skills)
                            {
                                skills.Add(skill);
                            }
                            break;
                    }
                }
                break;
            case 3: // 科技
                break;
            case 4: // 英雄
                break;
        }
    }
}

public class Damage
{
    public int commondamage;
    private int realdamage;
    public int attack_type; // 攻击类型：1肉搏 2兵刃 3气功
    public int attack_mode; // 攻击模式：4近战 5远程
    public Damage(int dmg, int att_type, int att_mode)
    {
        commondamage = dmg;
        realdamage = 0;
        attack_type = att_type;
        attack_mode = att_mode;
    }
    // 增加真实伤害
    public void AddRealDamage(int damage)
    {
        realdamage += damage;
    }
    // 返回伤害结构体的伤害值（伤害+真实伤害）
    public int GetDamage()
    {
        return commondamage + realdamage;
    }

    // 传入攻击方的buff，计算相关的增伤
    public void CalDamage(Buff buff)
    {
        commondamage += buff.attack;
    }

    // 传入受伤方的buff，计算相关的减伤
    public void CalDamaged(Buff buff)
    {
        commondamage -= buff.defend[attack_type];     // 肉搏 兵刃 气功
        // 近战 远程
        commondamage -= buff.defend[attack_mode];
        if (commondamage < 1) commondamage = 1; // damage不能低于1
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