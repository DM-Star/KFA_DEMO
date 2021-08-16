using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Cvs : MonoBehaviour
{
    public Button leftarrow, rightarrow;
    public MainCamera maincamera;
    public GameInfo gameinfo;
    public FoodIronbar foodironbar;
    private Player player;
    // 用于实例化
    public Tip tip;
    private Queue<Tip> tips;
    private Queue<string> msgs;
    // Start is called before the first frame update
    void Awake()
    {
        tips = new Queue<Tip>();
        msgs = new Queue<string>();
    }

    // Update is called once per frame
    void Update()
    {
        foodironbar.SetNum(player.food, player.iron);
        while(msgs.Count > 0)
        {
            Tip newtip = Instantiate(tip, transform);
            newtip.Init(msgs.Dequeue());
            tips.Enqueue(newtip);
        }
        while(tips.Count > 0 && tips.Peek().start == 0)
        {
            Destroy(tips.Dequeue().gameObject);
        }
    }

    public void ShowMsg(string msg)
    {
        msgs.Enqueue(msg);
    }
    public void Adapt()
    {
        int height = Screen.height;
        float factor = (height - (500f / 732 * height)) / 232;
        GetComponent<CanvasScaler>().scaleFactor = factor;
    }
    public void Init()
    {
        gameObject.SetActive(true);
        leftarrow.onClick.AddListener(ViewLeft);
        rightarrow.onClick.AddListener(ViewRight);
        player = gameinfo.players.GetPlayer(gameinfo.client);
        Adapt();
    }
    private void ViewLeft()
    {
        maincamera.rightview = false;
    }

    private void ViewRight()
    {
        maincamera.rightview = true;
    }

}
