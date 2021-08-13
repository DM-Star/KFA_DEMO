using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct Position
{
    // ��ʼֵ {-1, -1, -1}
    public int right; // 0��1��
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
    public Building buildingins; // ����ʵ����
    public Building[,,] buildings;
    public GameInfo gameinfo;
    public InfoBar infobar;
    public Projection projectionins;    // ����ʵ����Ͷ����
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

    // ��ҵ�ǰѡ��ķ���
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
                // ��Ҫ�����µ�Gridλ�ø�InfoBar
                infobar.clicked.Enqueue(new ClickMsg(grid));
            }
        }
    }

    private int leftupx;
    private int leftupy;
    private int width;
    private int height;
    public void Init(int ro, int playerc, int centerc)
    {
        row = ro;
        playercolumn = playerc;
        centercolumn = centerc;
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

    public Vector3 Pos2Pix(Vector3 pos)
    {
        return new Vector3(leftupx + pos.z * 10 + pos.x * (centercolumn + playercolumn) * 10, leftupy - pos.y * 10, 0);
    }

    public Projection InsProjection()
    {
        return Instantiate(projectionins, transform);
    }
}
