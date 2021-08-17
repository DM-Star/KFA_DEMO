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
    private float mistsize = 0.1f;
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
        hmist = row + 1;
        wmist = (int)(10 / mistsize * (playercolumn + centercolumn + 1));
        mists = new Mist[wmist, hmist];
        xmist =  -centercolumn * 5;
        ymist = -row * 5;
        if(info.client == 1)
        {
            xmist -= (playercolumn + 1) * 10;
        }
        for(int i = 0; i < wmist; i++)
        {
            for(int j = 0; j < hmist; j++)
            {
                Mist mist = Instantiate(mistins, new Vector3(xmist + i * mistsize, ymist + j * 30 - 30, -2), new Quaternion(), transform);
                mist.gameObject.transform.localScale = new Vector3(1.01f, 31, 1);
                mists[i, j] = mist;
            }
        }

    }

    public void GameOver()
    {
        gridclicked = new Queue<Position>();
        curgrid = new Position(-1, -1, -1);
        foreach (Building building in buildings) Destroy(building.gameObject);
        foreach (Mist mist in mists) Destroy(mist.gameObject);
    }
    private float MistPosx2X(int x)
    {
        return xmist + x * mistsize + mistsize / 2;
    }
    private int MistX2Posx(float x)
    {
        return (int)((x - xmist) / mistsize);
    }

    public void UpdateMist(Dictionary<Transform, Unit> unitmap)
    {
        SortedDictionary<float, float>[] map = new SortedDictionary<float, float>[wmist];
        for (int i = 0; i < wmist; i++) map[i] = new SortedDictionary<float, float>();
        foreach (KeyValuePair<Transform, Unit> pair in unitmap)
        {
            if (pair.Value.type == Unit.Type.BUILDING
                && pair.Value.building.status < pair.Value.building.maxstatus) continue;
            Vector3 circle = pair.Key.position;
            float radius = pair.Value.viewr;
            float viewx = Mathf.Min(radius, pair.Value.viewx);
            float viewy = Mathf.Min(radius, pair.Value.viewy);
            int left = MistX2Posx(circle.x - viewx);
            int right = MistX2Posx(circle.x + viewx);
            if (left < 0) left = 0;
            while (left <= right && left < wmist)
            {
                float dis = circle.x - MistPosx2X(left);
                float halfchord = Mathf.Min(viewy, Mathf.Sqrt(radius * radius - dis * dis));
                float up = circle.y + halfchord;
                float down = circle.y - halfchord;
                if (up - down > 0.01f)
                {
                    if (map[left].ContainsKey(down))
                    {
                        if (map[left][down] < up) map[left][down] = up;
                    }
                    else
                    {
                        map[left].Add(down, up);
                    }
                }
                left++;
            }
        }

        int col = 0;
        foreach(SortedDictionary<float, float> intervals in map)
        {
            float down = ymist - 5, up = ymist + row * 10 + 10;  // 当前迷雾条的起始点和终点
            int index = 0;  // 当前迷雾条的编号
            Mist mist = null;
            foreach (var interval in intervals)
            {
                if (interval.Value <= down) continue;
                if(interval.Key > down)
                {
                    mist = mists[col, index];
                    mist.gameObject.SetActive(true);
                    mist.transform.localScale = new Vector3(mist.transform.localScale.x, (interval.Key - down), 1);
                    mist.transform.localPosition = new Vector3(mist.transform.localPosition.x, down, -2);
                    index++;
                }
                down = interval.Value;
            }
            if(down < up)
            {
                mist = mists[col, index];
                mist.gameObject.SetActive(true);
                mist.transform.localScale = new Vector3(mist.transform.localScale.x, (up - down), 1);
                mist.transform.localPosition = new Vector3(mist.transform.localPosition.x, down, -2);
                index++;
            }
            while(index < hmist)
            {
                mists[col, index].gameObject.SetActive(false);
                index++;
            }
            col++;
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
