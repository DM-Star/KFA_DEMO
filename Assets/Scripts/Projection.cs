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
    public int lockobj;    // �Ƿ�����Ŀ�꣬1������0����
    public int value;   // ���������Ŀ����   ����������
    public int destroy; // ����ʱ�������Ŀ�������Ƿ����ٷ�����  1���٣�0��������
    public int crossnum;    // ��͸�������й����������ж��ٵ���
    private int attack;
    private int attack_type;    // �������ͣ�1�ⲫ 2���� 3����
    private int attack_mood;    // ������ʽ��1��ս 2Զ��
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
        // ��ʼλ��ҲҪ��ֵ
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
            // ����Ŀ��
            obj = enemy;
            dir = (obj.transform.position - transform.position).normalized;
        }
        else if (lockobj == 0)
        {
            // ������Ŀ��
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
        // ���㹥��buff
        damage.CalDamage(self.basebuff);
        damage.CalDamage(self.buff);
        // ����ʱ��������˺�
        gameinfo.skills.TriggerSkills(self, EVENT.BEFORE_DAMAGE, damage);

        // �������˷�buff
        // δ�������ϵ�buff��������ȫ��buff
        damage.CalDamaged(unit.basebuff);
        damage.CalDamaged(unit.buff);

        // ����ʱ�����ܵ��˺�ǰ
        gameinfo.skills.TriggerSkills(unit, EVENT.BEFORE_DAMAGED, damage);

        // ����ֹ�˺����
        if (damage.damage > unit.hp) damage.damage = unit.hp;

        HpChange hpchange = new HpChange(unit.hp, unit.hp - damage.damage);
        unit.hp -= damage.damage;

        if (unit.type == Unit.Type.SOLDIER)
        {
            unit.soldier.SetHpBar();
            unit.soldier.DamageTip(damage);
        }

        // ����ʱ�����ܵ��˺���
        gameinfo.skills.TriggerSkills(unit, EVENT.AFTER_DAMAGED, damage);
        // ����ʱ��������ֵ�ı��
        gameinfo.skills.TriggerSkills(unit, EVENT.HPCHANGE, hpchange);
        // ����ʱ��������˺���

        hasattack.Add(unit.transform, true);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Transform trans = collision.transform;
        if (crossnum > 0)
        {
            // ��͸������Ϊ��
            if (!hasattack.ContainsKey(trans))
            {
                // ͬһ�����˲����ظ���ײ

                if (lockobj == 1)
                {
                    // ����Ŀ��ʱ����Ҫ�ж��Ƿ����Ŀ��
                    if (trans == obj.transform)
                    {
                        // ����˺�
                        Damage(obj);
                        crossnum--;
                    }
                }

                else if (lockobj == 0)
                {
                    // ������Ŀ��ʱ����Ҫ�ж��Ƿ��ǵ���
                    if (enemyunits.ContainsKey(trans))
                    {
                        // �ǵ���
                        // ����˺�
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
        // ���к�ǵý�hit��Ϊtrue
        if (lockobj == 1)
        {
            // ����Ŀ��ʵʱ���㷽��
            if (obj.hp > 0 && !hasattack.ContainsKey(obj.transform))
            {
                dir = (obj.transform.position - transform.position).normalized;
            }
            else
            {
                // ���Ŀ������������destroy���Ծ����Ƿ����ٷ�����
                if (destroy == 1)
                {
                    crossnum = 0;
                }
                else if(destroy == 0)
                {
                    // �������Ҫ���٣���ԭ����������У����ҽ�lockobj��Ϊ0
                    lockobj = 0;
                }
            }
        }

        transform.position = transform.position + dir * speed;
        distance = distance - System.Math.Abs(dir.x * speed);
        // ���յ����ٻ���players.Move()�н��У��ж�distance��crossnum�Ƿ񶼴���0
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
