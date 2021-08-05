using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BtnBuildingType : MonoBehaviour
{
    public Text text;
    // 按钮id，仅决定按钮在界面的排列
    private int btnid;
    private int type;
    private InfoBar infobar;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Init(int i, InfoBar bar)
    {
        type = i + 1;
        transform.position = new Vector3(253 - 91 * (type % 2), 137.5f - (type - 1) / 2 * 50, 0);
        infobar = bar;
        GetComponent<Button>().onClick.AddListener(() => { infobar.clicked.Enqueue(new ClickMsg(1, type)); });
    }
}
