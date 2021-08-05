using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projection : MonoBehaviour
{
    public GameObject image;
    private int client;
    private float height;
    private float width;
    private BoxCollider2D box;
    private float speedbase = 0.5f;
    public Vector3 dir;

    public float speed;
    public float distance;
    public int lockobj;    // 是否锁定目标，1锁定，0不锁
    public int value;   // 锁定：最大目标数   不锁：线数
    public int destroy; // 锁定时：如果有目标死亡是否销毁发射物  1销毁，0继续飞行
    public int crossnum;    // 穿透数，飞行过程中最多击中多少敌人
    private int attack;
    private int attack_type;    // 攻击类型：1肉搏 2兵刃 3气功
    private int attack_mood;    // 攻击方式：1近战 2远程
    private Unit self;
    private Unit obj;
    private Dictionary<Transform, Unit> enemyunits;
    private Dictionary<Transform, bool> hasattack;
    private GameInfo gameinfo;
    void Awake()
    {
        box = GetComponent<BoxCollider2D>();
        hasattack = new Dictionary<Transform, bool>();
    }
    public void Init(ProjectionInfo info, Projectile projectile, Vector3 pos, int client, Unit source, Unit enemy, Players players)
    {
        gameinfo = players.gameinfo;
        // 初始位置也要赋值
        transform.localPosition = new Vector3(pos.x, pos.y, 0);
        image.GetComponent<SpriteRenderer>().sprite = info.sprite;
        height = info.height;
        width = info.width;
        image.transform.localPosition = new Vector3(width / 2, 0, -2);
        box.size = new Vector2(width, height);
        box.offset = new Vector2(width / 2, 0);
        box.enabled = true;
        speed = projectile.speed * speedbase;
        distance = projectile.distance;
        lockobj = projectile.lockobj;
        value = projectile.value;
        destroy = projectile.destroy;
        crossnum = projectile.crossnum;
        attack = projectile.attack;
        attack_type = projectile.attack_type;
        attack_mood = projectile.attack_mood;
        self = source;

        if(client == 0)
        {
            enemyunits = players.GetPlayer(1).units;
        }
        else if(client == 1)
        {
            enemyunits = players.GetPlayer(0).units;
        }

        if (lockobj == 1)
        {
            // 锁定目标
            obj = enemy;
            dir = (obj.transform.position - transform.position).normalized;
        }
        else if (lockobj == 0)
        {
            // 不锁定目标
            if (enemy.transform.position.x > pos.x)
            {
                dir = new Vector3(1, 0, 0);
            }
            else if (enemy.transform.position.x < pos.x)
            {
                dir = new Vector3(-1, 0, 0);
            }
            else
            {
                if (client == 0) dir = new Vector3(1, 0, 0);
                else if (client == 1) dir = new Vector3(-1, 0, 0);
            }
        }

        SetRotate();
    }
    private void SetRotate()
    {
        float angle = 0;
        angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    private void Damage(Unit unit)
    {
        Damage damage = new Damage(attack, attack_type, attack_mood);
        // 计算攻方buff
        damage.CalDamage(self.basebuff);
        damage.CalDamage(self.buff);
        // 插入时机：造成伤害
        gameinfo.skills.TriggerSkills(self, EVENT.BEFORE_DAMAGE, damage);

        // 计算受伤方buff
        // 未考虑身上的buff，仅考虑全局buff
        damage.CalDamaged(unit.basebuff);
        damage.CalDamaged(unit.buff);

        // 插入时机：受到伤害前
        gameinfo.skills.TriggerSkills(unit, EVENT.BEFORE_DAMAGED, damage);

        // 最后防止伤害溢出
        if (damage.damage > unit.hp) damage.damage = unit.hp;

        HpChange hpchange = new HpChange(unit.hp, unit.hp - damage.damage);
        unit.hp -= damage.damage;

        if (unit.type == Unit.Type.SOLDIER)
        {
            unit.soldier.SetHpBar();
            unit.soldier.DamageTip(damage);
        }

        // 插入时机：受到伤害后
        gameinfo.skills.TriggerSkills(unit, EVENT.AFTER_DAMAGED, damage);
        // 插入时机：生命值改变后
        gameinfo.skills.TriggerSkills(unit, EVENT.HPCHANGE, hpchange);
        // 插入时机：造成伤害后

        hasattack.Add(unit.transform, true);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Transform trans = collision.transform;
        if (crossnum > 0)
        {
            // 穿透数必须为正
            if (!hasattack.ContainsKey(trans))
            {
                // 同一个敌人不能重复碰撞

                if (lockobj == 1)
                {
                    // 锁定目标时，需要判断是否击中目标
                    if (trans == obj.transform)
                    {
                        // 造成伤害
                        Damage(obj);
                        crossnum--;
                    }
                }

                else if (lockobj == 0)
                {
                    // 非锁定目标时，需要判断是否是敌人
                    if (enemyunits.ContainsKey(trans))
                    {
                        // 是敌人
                        // 造成伤害
                        Damage(enemyunits[trans]);
                        crossnum--;
                    }
                }
            }
        }
    }
    public void Move()
    {
        SetRotate();
        // 命中后记得将hit置为true
        if (lockobj == 1)
        {
            // 锁定目标实时计算方向
            if (obj.hp > 0 && !hasattack.ContainsKey(obj.transform))
            {
                dir = (obj.transform.position - transform.position).normalized;
            }
            else
            {
                // 如果目标死亡，根据destroy属性决定是否销毁飞行物
                if (destroy == 1)
                {
                    crossnum = 0;
                }
                else if(destroy == 0)
                {
                    // 如果不需要销毁，则按原方向继续飞行，并且将lockobj置为0
                    lockobj = 0;
                }
            }
        }

        transform.position = transform.position + dir * speed;
        distance = distance - System.Math.Abs(dir.x * speed);
        // 最终的销毁会在players.Move()中进行，判断distance和crossnum是否都大于0
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
