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
        buildtime = 0;      // δ����
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
            case 1: // ����
                for (int i = 0; i < info.effects.Count; i++)
                {
                    // 1�ӹ�����2�����٣�3���������޺�����
                    switch (info.effects[i])
                    {
                        case 1: // ������ʱ��
                            break;
                        case 2: // �Ӹ�������
                            iron += info.values[i];
                            break;
                        case 3: // �Ӹ��ֿ���
                        case 4: 
                        case 5: 
                        case 6: 
                        case 7:
                            defend[info.effects[i] - 2] += info.values[i];
                            break;
                        
                    }
                }
                break;
            case 2: // ����
            case 5: // ���ֹ�������
            case 6: // ��������
                for(int i = 0; i < info.effects.Count; i++){
                    // 1�ӹ�����2�����٣�3���������޺�����
                    switch (info.effects[i])
                    {
                        case 1: // �ӹ���
                            attack += info.values[i];
                            break;
                        case 2: // ������
                            speed += info.values[i];
                            break;
                        case 3: // ����������
                            maxhp += info.values[i];
                            break;
                        case 4: // ����ʳ����
                            food += info.values[i];
                            break;
                        case 5: // �ӹ���
                            attack_speed += info.values[i];
                            break;
                        case 6: // �Ӹ��ֿ���
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                            defend[info.effects[i] - 5] += info.values[i];
                            break;
                        case 11: // �Ӽ���
                            foreach (string skill in info.skills)
                            {
                                skills.Add(skill);
                            }
                            break;
                    }
                }
                break;
            case 3: // �Ƽ�
                break;
            case 4: // Ӣ��
                break;
        }
    }
}

public class Damage
{
    public int commondamage;
    private int realdamage;
    public int attack_type; // �������ͣ�1�ⲫ 2���� 3����
    public int attack_mode; // ����ģʽ��4��ս 5Զ��
    public Damage(int dmg, int att_type, int att_mode)
    {
        commondamage = dmg;
        realdamage = 0;
        attack_type = att_type;
        attack_mode = att_mode;
    }
    // ������ʵ�˺�
    public void AddRealDamage(int damage)
    {
        realdamage += damage;
    }
    // �����˺��ṹ����˺�ֵ���˺�+��ʵ�˺���
    public int GetDamage()
    {
        return commondamage + realdamage;
    }

    // ���빥������buff��������ص�����
    public void CalDamage(Buff buff)
    {
        commondamage += buff.attack;
    }

    // �������˷���buff��������صļ���
    public void CalDamaged(Buff buff)
    {
        commondamage -= buff.defend[attack_type];     // �ⲫ ���� ����
        // ��ս Զ��
        commondamage -= buff.defend[attack_mode];
        if (commondamage < 1) commondamage = 1; // damage���ܵ���1
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