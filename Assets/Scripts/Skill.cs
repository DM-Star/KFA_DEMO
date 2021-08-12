using System;
using System.Collections.Generic;
/// <summary>
/// ֻ�������ӿڣ��ֱ��ǣ�
/// Skill GetSkill(string) ��ȡ��Ӧ�������Ƶļ��ܶ�����Ҫ���ڸ�Unit����ض����Ƶļ���
/// void TriggerSkills(Unit, EVENT, object) ִ��ĳ��ʱ�������м���
/// void Init() ��gameinfo�г�ʼ����
/// </summary>
public class SkillManager
{
    /// <summary>
    /// �������������ʽʹ�ã������������˵�����ʹ��CreateSkill()���켼��
    /// </summary>
    private void CreateSkillTemplete()
    {
        CreateSkill(
           // ��һ�������������������ܺ����м�������ͬ��
           "skill",
           // ����ʱ���б�����ֻ������Ӧ��ʱ���Żᱻ����
           new List<EVENT> { EVENT.BEFORE_DAMAGE },
           // ����ʱ��������������������self��ӵ�иü��ܵĵ�λ��evente�ǵ�ǰʱ����data�Ǳ�Ҫ���ݡ��ڲ�ͬ��ʱ���£�self��ݲ�ͬ��data���Ͳ�ͬ
           // �ܹ�������������������Ѿ���֤�˴���ʱ����ʱ���б��
           (Unit self, EVENT evente, object data) =>
           {
               Damage damage = data as Damage;  // �����ַ�����dataת����������
               return;
           },
           // ������������������������self��ӵ�иü��ܵĵ�λ��data�Ǳ�Ҫ���ݡ��ڲ�ͬ��ʱ���£�self��ݲ�ͬ��data���Ͳ�ͬ
           // �ܹ�������������������Ѿ���֤�˴���ʱ����ʱ���б��������֤selfһ��ӵ���������
           // �����������ʡ�ԣ���ʾ��true
           (Unit self, object data) =>
           {
               // һ����ԣ������ж�self���иü��ܾ���
               return self.HasSkill("skill");
           }
           );
    }
    public void Init()
    {
        skilltable = new Dictionary<string, Skill>();
        skilleventtable = new Dictionary<EVENT, List<Skill>>();
        foreach (EVENT evente in Enum.GetValues(typeof(EVENT)))
        {
            skilleventtable.Add(evente, new List<Skill>());
        }

        // ͬ�齣��������ֵ����50%ʱ���ƶ��ٶȺ͹����ٶ����100%���������˫���˺�
        CreateSkill(
           "tonggui",
           new List<EVENT> { EVENT.HPCHANGE, EVENT.BEFORE_DAMAGE },
           (Unit self, EVENT evente, object data) =>
           {
               if (evente == EVENT.BEFORE_DAMAGE)
               {
                   Damage damage = data as Damage;
                   if (self.TagNum("tonggui") > 0)
                   {
                       damage.commondamage = (int)(damage.commondamage * 1.5);
                   }
               }
               else if (evente == EVENT.HPCHANGE)
               {
                   HpChange change = data as HpChange;
                   if(5 * change.later < self.maxhp)
                   {
                       if(self.TagNum("tonggui") == 0)
                       {
                           self.AddTag("tonggui", 1);
                           self.buff.attack_speed += 50;
                           self.buff.speed += 50;
                       }
                   }
                   else
                   {
                       if (self.TagNum("tonggui") > 0)
                       {
                           self.RemoveAllTags("tonggui");
                           self.buff.attack_speed -= 100;
                           self.buff.speed -= 100;
                       }
                   }
               }
               return;
           },
           (Unit self, object data) =>
           {
               return self.HasSkill("tonggui");
           }
           );

        // �󱱶���ʽ���ܵ���ս�˺�������ˢ�¹�����ȴʱ��
        CreateSkill(
           "beidou",
           new List<EVENT> { EVENT.AFTER_DAMAGED },
           (Unit self, EVENT evente, object data) =>
           {
               Damage damage = data as Damage;
               if(damage.attack_mode == 4)
               {
                   self.ResetCD(0);
               }
               
               return;
           },
           (Unit self, object data) =>
           {
               return self.HasSkill("beidou");
           }
           );

        // �ɻ�������б�Ź�������
        CreateSkill(
           "feihua",
           new List<EVENT> { EVENT.AFTER_CHOOSE_OBJECT },
           (Unit self, EVENT evente, object data) =>
           {
               List<Unit> enemies = data as List<Unit>;
               // ���»�ȡ����Ŀ��
               if (enemies.Count > 0) enemies.Clear();

               List<List<Unit>> units = self.players.units;
               int right = -1;
               if (self.dir.x > 0)
               {
                   // ������
                   right = 1;
               }
               float lself = self.transform.position.x + (-7) * right;
               float rself = self.transform.position.x + 7 * right;

               // ������һ������
               for (int i = 0; i <= 10; i++)
               {
                   for(int u = -1; u <= 1; u += 2)
                   {
                       int row = self.row + u * i;
                       if(row >= 0 && row < units.Count)
                       {
                           for (int j = 0; j < units[row].Count; j++)
                           {
                               Unit obj = units[row][j];
                               if (obj.client != self.client && obj.transform.position.x > lself && obj.transform.position.x < rself
                               && (obj.transform.position.x - self.transform.position.x) * right > 0
                               )
                               {
                                   Projection proup = self.players.gameinfo.background.InsProjection();
                                   Projection prodown = self.players.gameinfo.background.InsProjection();
                                   proup.Init(self.prinfo, 
                                       self.projectile, 
                                       self.transform.localPosition, 
                                       self.client, 
                                       self,
                                       obj, 
                                       obj.players);
                                   prodown.Init(self.prinfo,
                                       self.projectile,
                                       self.transform.localPosition,
                                       self.client,
                                       self,
                                       obj,
                                       self.players);
                                   proup.dir = new UnityEngine.Vector3(right, 1, 0).normalized;
                                   prodown.dir = new UnityEngine.Vector3(right, -1, 0).normalized;
                                   self.players.projections.Add(proup);
                                   self.players.projections.Add(prodown);
                                   self.ResetCD();
                                   return;
                               }
                           }
                       }
                   }
                   lself += 10 * right;
                   rself += 10 * right;
               }
               return;
           },
           (Unit self, object data) =>
           {
               return self.HasSkill("feihua");
           }
           );

        // ά�ޣ�ÿ����Ϊ���ڵ��ѷ������ظ�50������ֵ
        CreateSkill(
            "weixiu",
            new List<EVENT> { EVENT.AFTER_CHOOSE_OBJECT },
            (Unit self, EVENT evente, object data) =>
            {
                List<Unit> enemies = data as List<Unit>;
                // ���»�ȡ����Ŀ��
                if (enemies.Count > 0) enemies.Clear();
                Building[,] buildings = self.players.GetPlayer(self.client).buildings;
                Building weixiuzhan = self.building;
                foreach(Building friend in buildings)
                {
                    if (friend.id != 0)
                    {
                        int distancex = Math.Abs(friend.row - weixiuzhan.row);
                        int distancey = Math.Abs(friend.col - weixiuzhan.col);
                        if (distancex == 1 || distancey == 1)
                        {
                            Heal heal = new Heal(self, friend.unit, 50);
                            HpChange hpchange = friend.unit.RestoreHp(heal);
                            if (hpchange.before != hpchange.later)
                            {
                                self.ResetCD();
                            }
                        }
                    }
                }
                return;
            },
           (Unit self, object data) =>
           {
               return self.HasSkill("weixiu") && self.type == Unit.Type.BUILDING;
           });
    }

    private Dictionary<string, Skill> skilltable;
    private Dictionary<EVENT, List<Skill>> skilleventtable;
    private void CreateSkill(string name, List<EVENT> events, ONEXECUTE Exec, CANEXECUTE CanExec)
    {
        Skill skill = new Skill(Exec, CanExec);
        // ���
        skilltable.Add(name, skill);
        foreach (EVENT evente in events)
        {
            skilleventtable[evente].Add(skill);
        }
    }
    public Skill GetSkill(string name) { return skilltable[name]; }
    public void TriggerSkills(Unit self, EVENT evente, object data)
    {
        foreach (Skill skill in skilleventtable[evente])
        {
            if (skill.CanExec(self, data))
            {
                skill.Exec(self, evente, data);
            }
        }
    }
}

public delegate void ONEXECUTE(Unit self, EVENT evente, object data);
public delegate bool CANEXECUTE(Unit self, object data);
public enum EVENT
{
    // ����˺�ǰ��self������˺����ˣ�data��Damage�ṹ��
    BEFORE_DAMAGE,
    // �ܵ��˺�ǰ��self���ܵ��˺����ˣ�data��Damage�ṹ��
    BEFORE_DAMAGED,
    // ����ֵ�ı��self������ֵ�ı���ˣ�data��HpChange�ṹ��
    HPCHANGE,
    // �ܵ��˺���self���ܵ��˺����ˣ�data��Damafe�ṹ��
    AFTER_DAMAGED,
    // ѡȡ����Ŀ���self��׼�������������ˣ�data��List<Unit>����ʾ���е��˵��б�
    AFTER_CHOOSE_OBJECT
}
public class Skill
{
    public Skill(ONEXECUTE skillexec, CANEXECUTE skillcanexec = null)
    {
        canexec = skillcanexec;
        exec = skillexec;
    }
    public bool CanExec(Unit self, object data)
    {
        if (canexec == null) return true;
        else return canexec(self, data);
    }
    public void Exec(Unit self, EVENT evente, object data)
    {
        exec(self, evente, data);
    }

    private CANEXECUTE canexec;
    private ONEXECUTE exec;
}

