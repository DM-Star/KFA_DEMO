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
    public void Init()
    {
        skilltable = new Dictionary<string, Skill>();
        skilleventtable = new Dictionary<EVENT, List<Skill>>();
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
           new List<EVENT> { EVENT.AFTER_CHOOSE_OBJECT },
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
               float lself = self.transform.position.x + (-7) * right;
               float rself = self.transform.position.x + 7 * right;

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

        // 维修：每秒钟为相邻的友方建筑回复50点生命值
        CreateSkill(
            "weixiu",
            new List<EVENT> { EVENT.AFTER_CHOOSE_OBJECT },
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
        // 存表
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
    // 造成伤害前，self是造成伤害的人，data是Damage结构体
    BEFORE_DAMAGE,
    // 受到伤害前，self是受到伤害的人，data是Damage结构体
    BEFORE_DAMAGED,
    // 生命值改变后，self是生命值改变的人，data是HpChange结构体
    HPCHANGE,
    // 受到伤害后，self是受到伤害的人，data是Damafe结构体
    AFTER_DAMAGED,
    // 选取攻击目标后，self是准备发动攻击的人，data是List<Unit>，表示所有敌人的列表
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

