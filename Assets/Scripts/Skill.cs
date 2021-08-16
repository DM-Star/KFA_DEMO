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
    private GameInfo gameinfo;
    public void Init(GameInfo info)
    {
        gameinfo = info;

        skilltable = new SortedDictionary<string, Skill>();
        skilleventtable = new SortedDictionary<EVENT, List<Skill>>();
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
                       damage.delta = (int)(damage.commondamage * 0.5);
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
           new List<EVENT> { EVENT.AFTER_FIND_ENEMIES },
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
               float lself = self.transform.position.x - 7;
               float rself = self.transform.position.x + 7;

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
                                   Projection proup = gameinfo.background.InsProjection();
                                   Projection prodown = gameinfo.background.InsProjection();
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

        // ά�ޣ�����ÿ����Ϊ���ڵ��ѷ������ظ�50������ֵ
        CreateSkill(
            "weixiu",
            new List<EVENT> { EVENT.AFTER_FIND_ENEMIES },
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
                        if ((distancex | distancey) == 1)
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
               return self.HasSkill("weixiu") 
               && self.type == Unit.Type.BUILDING
               && self.building.status >= self.building.maxstatus;
           });

        // ���ˣ�ÿ�ν����ܵ���ս�˺����������˺���Դ���5������˺�
        CreateSkill(
           "fanshang",
           new List<EVENT> { EVENT.AFTER_DAMAGED },
           
           (Unit self, EVENT evente, object data) =>
           {
               Damage damage = data as Damage;
               Damage fanshang = new Damage(5, 2, 4, self, damage.from, "fanshang");
               fanshang.Do();
               return;
           },
           
           (Unit self, object data) =>
           {
               Damage damage = data as Damage;
               // ���˲��ܷ����˵��˺�
               return self.HasSkill("fanshang") 
               && damage.attack_mode == 4
               && self.type == Unit.Type.BUILDING
               && self.building.status >= self.building.maxstatus
               && damage.skill != "fanshang";
           }
           );

        // �������������ʱ��3*3��Χ�����е��˷����ʸ�����10������˺�
        CreateSkill(
           "wanjian",
           new List<EVENT> { EVENT.DEATH },
           (Unit self, EVENT evente, object data) =>
           {
               List<List<Unit>> units = self.players.units;
               for (int row = self.row - 1; row <= self.row + 1; row++)
               {
                   if (row >= 0 && row < units.Count)
                   {
                       foreach (Unit obj in units[row])
                       {
                           if(self.client != obj.client)
                           {
                               if (Math.Abs(obj.transform.position.x - self.transform.position.x) <= 18)
                               {
                                   Projection projection = gameinfo.background.InsProjection();
                                   projection.Init(gameinfo.projectionmap[2],
                                       new Projectile("0|1|2|3|4|5|6|7|8|9|10|11|12|13|2|2|4|1|20|1|999|0|1|10".Split('|')),
                                       self.transform.localPosition,
                                       self.client,
                                       self,
                                       obj,
                                       obj.players);
                                   self.players.projections.Add(projection);
                               }
                           }
                       }
                   }
               }
               return;
           },
           (Unit self, object data) =>
           {
               return self.HasSkill("wanjian")
               && self.type == Unit.Type.BUILDING
               && self.building.status >= self.building.maxstatus;
           }
           );

        // ��ȫ�������ڼ����90%
        CreateSkill(
           "anquan",
           new List<EVENT> { EVENT.BEFORE_DAMAGED },

           (Unit self, EVENT evente, object data) =>
           {
               Damage damage = data as Damage;
               damage.delta -= (int)(damage.commondamage * 0.9);
               return;
           },

           (Unit self, object data) =>
           {
               Damage damage = data as Damage;
               return self.HasSkill("anquan")
               && self.type == Unit.Type.BUILDING
               && self.building.status < self.building.maxstatus;
           }
           );

        // �������������
        CreateSkill(
          "tiewei",
          new List<EVENT> { EVENT.BEFORE_ATTACK },

          (Unit self, EVENT evente, object data) =>
          {
              List<Unit> enemies = data as List<Unit>;
              int count = 0;
              // �Ƚ����п��Թ������ľ��г���ĵ��˼������
              foreach(Unit tiewei in self.FindUnitsBySkill("tiewei"))
              {
                  if (self.CanAttack(tiewei) && !enemies.Contains(tiewei))
                  {
                      enemies.Add(tiewei);
                      count++;
                  }
              }
              // Ȼ���޳������г���Ķ���ĵ���
              List<Unit> todelete = new List<Unit>();
              for(int i = enemies.Count - 1; i >= 0 && count > 0; i--)
              {
                  Unit enemy = enemies[i];
                  if (!enemy.HasSkill("tiewei"))
                  {
                      enemies.RemoveAt(i);
                      count--;
                  }
              }
              // ���޳����г���Ķ���ĵ���
              enemies.RemoveRange(enemies.Count - count, count);
              return;
          },

          (Unit self, object data) =>
          {
              return self.projectile.lockobj == 1 && self.FindUnitsBySkill("tiewei").Count > 0;
          }
          );

        // ���䣺���Ƚ�������ֵ��͵ĵ���
        CreateSkill(
          "shenshe",
          new List<EVENT> { EVENT.AFTER_FIND_ENEMIES },

          (Unit self, EVENT evente, object data) =>
          {
              List<Unit> enemies = data as List<Unit>;
              if (enemies.Count == 0) return;
              Unit target = enemies[0];
              int hp = 1000000;
              foreach(Unit enemy in enemies)
              {
                  if(enemy.type == Unit.Type.SOLDIER && enemy.hp < hp)
                  {
                      hp = enemy.hp;
                      target = enemy;
                  }
              }
              enemies.Clear();
              if (target != null) enemies.Add(target);
              return;
          },

          (Unit self, object data) =>
          {
              return self.HasSkill("shenshe");
          }
          );

        // �ƻ����Խ�������˺�������һ��pohuai���
        CreateSkill(
          "pohuai",
          new List<EVENT> { EVENT.AFTER_DAMAGE },

          (Unit self, EVENT evente, object data) =>
          {
              Damage damage = data as Damage;
              damage.to.SetTag("pohuai", gameinfo.GetFrame());
              return;
          },

          (Unit self, object data) =>
          {
              Damage damage = data as Damage;
              return self.HasSkill("pohuai") && damage.to.type == Unit.Type.BUILDING;
          }
          );

        // �ƻ�������������pohuai��ǵĵ�λ�ڱ�Ǻ��2��������������80%
        CreateSkill(
          "pohuai2",
          new List<EVENT> { EVENT.BEFORE_HEALED },

          (Unit self, EVENT evente, object data) =>
          {
              Heal heal = data as Heal;
              heal.value /= 5; 
              return;
          },

          (Unit self, object data) =>
          {
              int frame = self.TagNum("pohuai");
              return frame > 0 && gameinfo.GetFrame() - frame <= 100;
          }
          );

        // Ͷʯ���Խ������˫���˺�
        CreateSkill(
          "toushi",
          new List<EVENT> { EVENT.BEFORE_DAMAGE },

          (Unit self, EVENT evente, object data) =>
          {
              Damage damage = data as Damage;
              damage.delta += damage.commondamage;
              return;
          },

          (Unit self, object data) =>
          {
              Damage damage = data as Damage;
              return self.HasSkill("toushi") && damage.to.type == Unit.Type.BUILDING;
          }
          );

        // ����������������ֵ10%���˺�
        CreateSkill(
          "jindu",
          new List<EVENT> { EVENT.BEFORE_DAMAGE },

          (Unit self, EVENT evente, object data) =>
          {
              Damage damage = data as Damage;
              damage.AddRealDamage(damage.to.maxhp / 10);
              return;
          },

          (Unit self, object data) =>
          {
              Damage damage = data as Damage;
              return self.HasSkill("jindu") && damage.to.type == Unit.Type.SOLDIER;
          }
          );

        // ��أ��������κεط������
        CreateSkill(
          "juedi",
          new List<EVENT> { EVENT.FINISH_RESEARCH },

          (Unit self, EVENT evente, object data) =>
          {
              Building[,] buildings = self.players.GetPlayer(self.client).buildings;
              foreach(Building building in buildings)
              {
                  if (!building.ironpoint)
                  {
                      building.ironpoint = true;
                      building.ironleft = 1000;
                  }
              }
              return;
          },

          (Unit self, object data) =>
          {
              ResearchInfo reinfo = data as ResearchInfo;
              if (self.HasSkill("juedi"))
              {
                  foreach (ResearchInfo.BuffInfo buff in reinfo.buffinfos)
                  {
                      if (buff.skills.Contains("juedi")) return true;
                  }
              }
              return false;
          }
          );

        // ũ�ģ��������κεط�����ũ��
        CreateSkill(
          "nonggai",
          new List<EVENT> { EVENT.FINISH_RESEARCH },

          (Unit self, EVENT evente, object data) =>
          {
              Building[,] buildings = self.players.GetPlayer(self.client).buildings;
              foreach (Building building in buildings)
              {
                  if (!building.foodpoint)
                  {
                      building.foodpoint = true;
                      building.foodleft = 1000;
                  }
              }
              return;
          },

          (Unit self, object data) =>
          {
              ResearchInfo reinfo = data as ResearchInfo;
              if (self.HasSkill("nonggai"))
              {
                  foreach (ResearchInfo.BuffInfo buff in reinfo.buffinfos)
                  {
                      if (buff.skills.Contains("nonggai")) return true;
                  }
              }
              return false;
          }
          );
    }

    private SortedDictionary<string, Skill> skilltable;
    private SortedDictionary<EVENT, List<Skill>> skilleventtable;
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
    public void TriggerSkills(Unit self, EVENT evente, object data = null)
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
    // �ܵ��˺���self���ܵ��˺����ˣ�data��Damage�ṹ��
    AFTER_DAMAGED,
    // ����˺���self������˺����ˣ�data��Damage�ṹ��
    AFTER_DAMAGE,
    // ��ȡ�����б��self��׼�������������ˣ�data��List<Unit>����ʾ���е��˵��б�
    AFTER_FIND_ENEMIES,
    // ��ȡ�����б��׼���Ե��˷�������֮ǰ��self��׼�������������ˣ�data��List<Unit>����ʾ���е��˵��б�
    BEFORE_ATTACK,
    // �ܵ�����֮ǰ��self���ܵ����Ƶ��ˣ�data ��Heal�ṹ��
    BEFORE_HEALED,
    // ����ʱ
    DEATH,
    // �з���ɺ�self������з��Ľ�����data��RerearchInfo
    FINISH_RESEARCH
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

