using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier : MonoBehaviour
{
    public int id;
    public int client;
    public Unit unit;
    public SoldierInfo soinfo;

    private SpriteRenderer image;
    private Player player;
    public GameObject hpbar;
    // 掉血文字实例
    public HpChangeTip hpchangeins;
    // public static Object Instantiate(Object original, Vector3 position, Quaternion rotation, Transform parent);
    public void Init(GameInfo info, int soid, int cl, int row)
    {
        // 麻烦，之后直接传一个gameinfo和一个soid
        soinfo = info.soldiermap[soid];
        client = cl;
        id = soinfo.id;
        unit = new Unit(info, this, row, cl) ;
        image = GetComponent<SpriteRenderer>();
        image.sprite = soinfo.sprite;
        player = info.players.GetPlayer(client);
        player.units.Add(transform, unit);
        // 测试用代码
        /*Projection projection = Instantiate(projectionins, transform.parent);
        projection.Init(prinfo);*/
    }
    
    public void SetHpBar()
    {
        float p = (float)unit.hp / unit.maxhp;
        hpbar.transform.localScale = new Vector3(p, 1, 1);
        hpbar.transform.localPosition = new Vector3(-4.54f * (1 - p) * 0.5f, 0, 0);
    }

    public void DamageTip(Damage damage)
    {
        HpChangeTip hpchange = Instantiate(hpchangeins, transform.parent);
        hpchange.Init(damage, transform.localPosition);
    }
    private void OnDestroy()
    {
        player.units.Remove(transform);
    }
}
