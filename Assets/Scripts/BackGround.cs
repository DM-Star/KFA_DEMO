using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct Position
{
    // 初始值 {-1, -1, -1}
    public int right; // 0左，1右
    public int row;
    public int col;
    public Position(int a, int b, int c)
    {
        right = a;
        row = b;
        col = c;
    }
    public void set(Position pos)
    {
        right = pos.right;
        row = pos.row;
        col = pos.col;
    }
    public Position (Position pos)
    {
        right = pos.right;
        row = pos.row;
        col = pos.col;
    }
    public void show()
    {
        Debug.Log(string.Format("client = {0}, row = {1}, col = {2}", right, row, col));
    }
}
public class BackGround : MonoBehaviour
{
    private int row, playercolumn, centercolumn;
    public GameObject greyblock;//, yellowblock;
    public Building buildingins; // 用于实例化
    public Building[,,] buildings;
    public GameInfo gameinfo;
    public InfoBar infobar;
    public Projection projectionins;    // 用于实例化投射物
    public Mist mistins;        // 用于实例化战争迷雾
    private Mist[,] mists;
    private float mistsize = 1f;
    // Start is called before the first frame update
    void Awake()
    {
        gridclicked = new Queue<Position>();
        curgrid = new Position(-1, -1, -1);
    }

    // Update is called once per frame
    void Update()
    {
        GridClicked();
    }

    // 玩家当前选择的方格
    public Position curgrid;
    public Queue<Position> gridclicked;
    private void GridClicked()
    {
        while (gridclicked.Count > 1) gridclicked.Dequeue();
        if(gridclicked.Count == 1)
        {
            Position grid = gridclicked.Dequeue();
            if(!grid.Equals(curgrid))
            {
                if (curgrid.right != -1)
                {
                    buildings[curgrid.right, curgrid.row, curgrid.col].select(false);
                }
                buildings[grid.right, grid.row, grid.col].select(true);
                curgrid = grid;
                // 需要传送新的Grid位置给InfoBar
                infobar.clicked.Enqueue(new ClickMsg(grid));
            }
        }
    }

    private int leftupx;
    private int leftupy;
    private int width;
    private int height;
    private int hmist, wmist;
    private float xmist, ymist;
    public void Init(GameInfo info)
    {
        row = info.row;
        playercolumn = info.playercolumn;
        centercolumn = info.centercolumn;
        width = (2 * playercolumn + centercolumn) * 10;
        height = row * 10;
        leftupx = -width / 2 + 5;
        leftupy = height / 2 - 5;

        buildings = new Building[2, row, playercolumn];
        for (int r = 0; r < 2; r++)
        {
            for (int i = 0; i < row; i++)
            {
                bool food = (i == 0 || i == row - 1);
                bool iron = (i == 0);
                for (int j = 0; j < playercolumn; j++)
                {
                    buildings[r, i, j] = Instantiate(buildingins, transform);
                    buildings[r, i, j].Init(new Position(r, i, j),
                        new Vector3(leftupx + j * 10 + r * (centercolumn + playercolumn) * 10, leftupy - i * 10, -1), this, food, iron);
                    
                }
            }
        }

        mistins.transform.localScale = new Vector3(mistsize, mistsize, 1);
        hmist = (int)(10 / mistsize * row);
        wmist = (int)(10 / mistsize * (playercolumn + centercolumn));
        mists = new Mist[hmist, wmist];
        xmist = mistsize / 2 - centercolumn * 5;
        ymist = -mistsize / 2 + row * 5;
        if(info.client == 1)
        {
            xmist -= playercolumn * 10;
        }
        for(int i = 0; i < hmist; i++)
        {
            for(int j = 0; j < wmist; j++)
            {
                mists[i, j] = Instantiate(mistins, new Vector3(xmist + j * mistsize, ymist - i * mistsize, -2), new Quaternion(), transform);
            }
        }

        /*for (int i = 0; i < row; i++)
        {
            for (int j = playercolumn; j < playercolumn + centercolumn; j++)
            {
                GameObject img = Instantiate(greyblock, transform);
                img.name = string.Format("blocks{0}_{1}", i, j);
                img.transform.position = new Vector3(leftupx + 10 * j, leftupy - 10 * i, -1);
            }
        }*/
    }

    private void MistPix2Pos(Vector3 pos, out int r, out int c)
    {
        r = (int)((ymist - pos.y) / mistsize);
        c = (int)((pos.x - xmist) / mistsize);
    }

    private int MistY2Posy(float y)
    {
        return (int)((ymist - y) / mistsize);
    }
    private float MistPosy2Y(int row)
    {
        return ymist - mistsize / 2 - row * mistsize;
    }

    public void UpdateMist(Dictionary<Transform, Unit> unitmap)
    {
        SortedDictionary<int, int>[] map = new SortedDictionary<int, int>[hmist];
        for (int i = 0; i < hmist; i++) map[i] = new SortedDictionary<int, int>();
        int l, r, row;
        foreach (KeyValuePair<Transform, Unit> pair in unitmap)
        {
            if (pair.Value.type == Unit.Type.BUILDING
                && pair.Value.building.status < pair.Value.building.maxstatus) continue;
            Vector3 circle = pair.Key.position;
            float radius = pair.Value.eyesight;
            int down = MistY2Posy(circle.y - radius);
            int up = MistY2Posy(circle.y + radius);
            if (up < 0) up = 0;
            while (up <= down)
            {
                float yup = MistPosy2Y(up);
                float dis = yup - circle.y;
                if (dis != 0) dis -= mistsize / 2;
                float halfchord = Mathf.Sqrt(radius * radius - dis * dis);
                float left = circle.x - halfchord;
                float right = circle.x + halfchord;
                MistPix2Pos(new Vector3(left, yup), out row, out l);
                MistPix2Pos(new Vector3(right, yup), out row, out r);
                if (row >= map.Length) break;
                else
                {
                    if (r >= wmist) r = wmist - 1;
                    if (map[row].ContainsKey(l))
                    {
                        if (map[row][l] < r) map[row][l] = r;
                    }
                    else
                    {
                        map[row].Add(l, r);
                    }
                }
                up++;
            }
        }

        row = 0;
        foreach(SortedDictionary<int, int> intervals in map)
        {
            int cur = 0;
            foreach (var interval in intervals)
            {
                if (interval.Value < cur) continue;
                while (cur < interval.Key)
                {
                    mists[row, cur].gameObject.SetActive(true);
                    cur++;
                }
                while (cur <= interval.Value)
                {
                    mists[row, cur].gameObject.SetActive(false);
                    cur++;
                }
            }
            while (cur < wmist)
            {
                mists[row, cur].gameObject.SetActive(true);
                cur++;
            }
            row++;
        }
    }

    public Vector3 Pos2Pix(Vector3 pos)
    {
        return new Vector3(leftupx + pos.z * 10 + pos.x * (centercolumn + playercolumn) * 10, leftupy - pos.y * 10, 0);
    }

    public Projection InsProjection()
    {
        return Instantiate(projectionins, transform);
    }
}
