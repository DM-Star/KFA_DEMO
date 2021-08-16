using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
public class BtnBuilding : MonoBehaviour
{
    // Start is called before the first frame update
    public Image avatar;
    public Sprite[] sprite;
    private Text progress;

    // 标识按钮在按钮栏的位置，从0计数
    private int btnid;

    // 标识建筑id/科技id/兵种id
    public int id;
    private InfoBar infobar;
    // 标识此按钮的类型，1建筑，2科技，3兵种
    private int type;
    private bool[] load;    // 建筑用，正像镜像都加载完之后再SetActive(false)
    private void Awake()
    {
        sprite = new Sprite[2];
        load = new bool[2] { false, false };
        progress = GetComponentInChildren<Text>();
        progress.text = "";
        avatar.enabled = false;
        progress.enabled = false;
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    private int relevel, remaxlevel;
    public void UpdateReProgressText()
    {
        relevel++;
        progress.text = string.Format("{0}/{1}", relevel, remaxlevel);
    }
    private int sonum;
    public void UpdateSoProgreeText(int op, int v = 0)
    {
        // op:0 set    1 add
        if (op == 0) sonum = v;
        else if (op != 0) sonum += v;

        if (sonum == 0) progress.text = "";
        else if(sonum != 0) progress.text = string.Format("{0}  ", sonum);
    }

    IEnumerator GetTextAndSprite(string path)
    {
        infobar.gameinfo.coroutinenum++;
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(path))
        {
            yield return uwr.SendWebRequest();
            
            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(uwr);
                avatar.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                if (type == 1)
                {
                    // 建筑
                    sprite[0] = avatar.sprite;
                    infobar.gameinfo.buildingmap[id].sprite[0] =
                    Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 20);
                    load[0] = true;
                    if (load[1])
                    {
                        avatar.sprite = sprite[infobar.gameinfo.client];
                        gameObject.SetActive(false);
                        // 因为SetActive(false)会终止协程，所以必须等镜像也加载完毕
                    }
                }
                else if (type == 2)
                {
                    // 科技
                    gameObject.SetActive(false);
                }
                else if (type == 3)
                {
                    infobar.gameinfo.soldiermap[id].sprite =
                        Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 20);
                    gameObject.SetActive(false);
                    // 兵种
                }
                avatar.enabled = true;
                progress.enabled = true;
            }
            uwr.Dispose();
            infobar.gameinfo.coroutinenum--;
        }
    }

    IEnumerator GetMirrorBuilding(string path)
    {
        infobar.gameinfo.coroutinenum++;
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(path))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(uwr);
                sprite[1] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                // 建筑
                infobar.gameinfo.buildingmap[id].sprite[1] =
                Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 20);
                load[1] = true;
                if (load[0])
                {
                    avatar.sprite = sprite[infobar.gameinfo.client];
                    gameObject.SetActive(false);
                }
            }
            uwr.Dispose();
            infobar.gameinfo.coroutinenum--;
        }
    }

    // 作为建筑按钮
    public void InitB(int btni, int buid, InfoBar bar, string path)
    {
        btnid = btni;
        id = buid;
        transform.position = new Vector3(344 + 75 * (btnid % 6), 125 - (btnid / 6) * 75, 0);
        infobar = bar;
        type = 1;
        GetComponent<Button>().onClick.AddListener(() => { infobar.clicked.Enqueue(new ClickMsg(2, id)); });
        StartCoroutine(GetTextAndSprite(Path.Combine(Application.streamingAssetsPath, "building", path)));
        StartCoroutine(GetMirrorBuilding(Path.Combine(Application.streamingAssetsPath, "building", "mirror_" + path)));
    }


    public void InitR(int btni, int reid, InfoBar bar,string path)
    {
        btnid = btni;
        id = reid;
        transform.position = new Vector3(200 + 75 * (btnid % 5), 125 - (btnid / 5) * 75, 0);
        infobar = bar;
        type = 2;
        remaxlevel = infobar.gameinfo.researchmap[id].max;
        relevel = 0;
        progress.text = string.Format("{0}/{1}", relevel, remaxlevel);
        GetComponent<Button>().onClick.AddListener(() => {
            infobar.clicked.Enqueue(new ClickMsg(3, id)); 
        });
        path = Path.Combine(Application.streamingAssetsPath, "research", path);
        StartCoroutine(GetTextAndSprite(path));
    }

    public void InitS(int btni, int soid, InfoBar bar, string path)
    {
        btnid = btni;
        id = soid;
        transform.position = new Vector3(200 + 75 * (btnid % 5), 125 - (btnid / 5) * 75, 0);
        infobar = bar;
        type = 3;
        sonum = 0;
        GetComponent<Button>().onClick.AddListener(() =>
        {
            infobar.clicked.Enqueue(new ClickMsg(5, id));
        });
        path = Path.Combine(Application.streamingAssetsPath, "soldier", path);
        StartCoroutine(GetTextAndSprite(path));
    }
}
