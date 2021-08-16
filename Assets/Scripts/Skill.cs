using System;
using System.Collections.Generic;
/// <summary>
/// 只有三个接口，分别是：
/// Skill GetSkill(string) 获取对应技能名称的技能对象。主要用于给Unit添加特定名称的技能
/// void TriggerSkills(Unit, EVENT, object) 执行某个时机的所有技能
/// void Init() 在gameinfo中初始化用
/// </summary>
public class SkillManager
{
    /// <summary>
    /// 这个函数不会正式使用，放在这儿用于说明如何使用CreateSkill()构造技能
    /// </summary>
    private void CreateSkillTemplete()
    {
        CreateSkill(
           // 第一个参数：技能名，不能和已有技能名相同。
           "skill",
           // 触发时机列表，技能只有在相应的时机才会被触发
           new List<EVENT> { EVENT.BEFORE_DAMAGE },
           // 触发时动作，三个参数，其中self是拥有该技能的单位，evente是当前时机，data是必要数据。在不同的时机下，self身份不同，data类型不同
           // 能够进入这个函数就首先已经保证了触发时机在时机列表里。
           (Unit self, EVENT evente, object data) =>
           {
               Damage damage = data as Damage;  // 用这种方法将data转成所需类型
               return;
           },
           // 触发条件，两个参数，其中self是拥有该技能的单位，data是必要数据。在不同的时机下，self身份不同，data类型不同
           // 能够进入这个函数就首先已经保证了触发时机在时机列表里。但不保证self一定拥有这个技能
           // 这个参数可以省略，表示恒true
           (Unit self, object data) =>
           {
               // 一般而言，这里判断self具有该技能就行
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

        // 同归剑法：生命值低于50%时，移动速度和攻击速度提高100%，并且造成双倍伤害
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

        // 大北斗七式：受到近战伤害后，立即刷新攻击冷却时间
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

        // 飞花剑法：斜着攻击敌人
        CreateSkill(
           "feihua",
           new List<EVENT> { EVENT.AFTER_FIND_ENEMIES },
           (Unit self, EVENT evente, object data) =>
           {
               List<Unit> enemies = data as List<Unit>;
               // 重新获取敌人目标
               if (enemies.Count > 0) enemies.Clear();

               List<List<Unit>> units = self.players.units;
               int right = -1;
               if (self.dir.x > 0)
               {
                   // 朝右走
                   right = 1;
               }
               float lself = self.transform.position.x - 7;
               float rself = self.transform.position.x + 7;

               // 往上找一个敌人
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

        // 维修：建筑每秒钟为相邻的友方建筑回复50点生命值
        CreateSkill(
            "weixiu",
            new List<EVENT> { EVENT.AFTER_FIND_ENEMIES },
            (Unit self, EVENT evente, object data) =>
            {
                List<Unit> enemies = data as List<Unit>;
                // 重新获取敌人目标
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

        // 反伤：每次建筑受到近战伤害，立即对伤害来源造成5点兵刃伤害
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
               // 反伤不能反反伤的伤害
               return self.HasSkill("fanshang") 
               && damage.attack_mode == 4
               && self.type == Unit.Type.BUILDING
               && self.building.status >= self.building.maxstatus
               && damage.skill != "fanshang";
           }
           );

        // 万箭：建筑死亡时向3*3范围内所有敌人发射箭矢，造成10点兵刃伤害
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

        // 安全：建造期间减伤90%
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

        // 铁卫：嘲讽敌人
        CreateSkill(
          "tiewei",
          new List<EVENT> { EVENT.BEFORE_ATTACK },

          (Unit self, EVENT evente, object data) =>
          {
              List<Unit> enemies = data as List<Unit>;
              int count = 0;
              // 先将所有可以攻击到的具有嘲讽的敌人加入队列
              foreach(Unit tiewei in self.FindUnitsBySkill("tiewei"))
              {
                  if (self.CanAttack(tiewei) && !enemies.Contains(tiewei))
                  {
                      enemies.Add(tiewei);
                      count++;
                  }
              }
              // 然后剔除不具有嘲讽的多余的敌人
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
              // 再剔除具有嘲讽的多余的敌人
              enemies.RemoveRange(enemies.Count - count, count);
              return;
          },

          (Unit self, object data) =>
          {
              return self.projectile.lockobj == 1 && self.FindUnitsBySkill("tiewei").Count > 0;
          }
          );

        // 神射：优先进攻生命值最低的敌人
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

        // 破坏：对建筑造成伤害后，增加一个pohuai标记
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

        // 破坏衍生技：具有pohuai标记的单位在标记后的2秒内治疗量降低80%
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

        // 投石：对建筑造成双倍伤害
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

        // 浸毒：造成最大生命值10%的伤害
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

        // 掘地：允许在任何地方建造矿场
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

        // 农改：允许在任何地方建造农场
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
        // 存表
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
    // 造成伤害前，self是造成伤害的人，data是Damage结构体
    BEFORE_DAMAGE,
    // 受到伤害前，self是受到伤害的人，data是Damage结构体
    BEFORE_DAMAGED,
    // 生命值改变后，self是生命值改变的人，data是HpChange结构体
    HPCHANGE,
    // 受到伤害后，self是受到伤害的人，data是Damage结构体
    AFTER_DAMAGED,
    // 造成伤害后，self是造成伤害的人，data是Damage结构体
    AFTER_DAMAGE,
    // 获取敌人列表后，self是准备发动攻击的人，data是List<Unit>，表示所有敌人的列表
    AFTER_FIND_ENEMIES,
    // 获取敌人列表后，准备对敌人发动攻击之前，self是准备发动攻击的人，data是List<Unit>，表示所有敌人的列表
    BEFORE_ATTACK,
    // 受到治疗之前，self是受到治疗的人，data 是Heal结构体
    BEFORE_HEALED,
    // 死亡时
    DEATH,
    // 研发完成后，self是完成研发的建筑，data是RerearchInfo
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

