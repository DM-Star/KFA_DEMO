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
            // 1�ӹ�����2�����٣�3���������޺�����
            switch (info.effects[i])
            {
                case 5: // ����ʳ����
                    food += info.values[i];
                    break;
                case 6: // �Ӹ�������
                    iron += info.values[i];
                    break;
                case 10: // �ӽ���/�з�/ѵ��ʱ��
                    time += info.values[i];
                    break;
                case 12: // ����������
                    maxhp += info.values[i];
                    break;
                case 23: // �ӹ���
                    attack += info.values[i];
                    break;
                case 24: // �ӹ���
                    attack_speed += info.values[i];
                    break;
                case 27: // �Ӹ��ֿ���
                case 28:
                case 29:
                case 30:
                case 31:
                    defend[info.effects[i] - 26] += info.values[i];
                    break;
                case 32: // ������
                    speed += info.values[i];
                    break;
                case 33: // �Ӽ���
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
    public int attack_type; // �������ͣ�1�ⲫ 2���� 3����
    public int attack_mode; // ����ģʽ��4��ս 5Զ��
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
            // ���㹥��buff
            CalDamage(from.basebuff);
            CalDamage(from.buff);

            // ����ʱ��������˺�ǰ
            gameinfo.skills.TriggerSkills(from, EVENT.BEFORE_DAMAGE, this);

            commondamage = Mathf.Max(1, commondamage + delta);
            delta = 0;

            // Ŀ���ܵ��˺�
            to.TakeDamage(this);

            // ������ʱ��������˺���
            gameinfo.skills.TriggerSkills(from, EVENT.AFTER_DAMAGE, this);
        }
    }

    // ������ʵ�˺�
    public void AddRealDamage(int damage)
    {
        realdamage += damage;
    }
    // �����˺��ṹ����˺�ֵ���˺�+��ʵ�˺���
    public int GetDamage()
    {
        int damage = Mathf.Max(1, commondamage + delta) + realdamage;
        return damage;
    }

    // ���빥����������buff��������ص�����
    public void CalDamage(Buff buff)
    {
        commondamage += buff.attack;
    }

    // �������˷���buff��������صļ���
    public void CalDamaged(Buff buff)
    {
        delta -= buff.defend[attack_type];     // �ⲫ ���� ����
        // ��ս Զ��
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