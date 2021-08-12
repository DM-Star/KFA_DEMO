using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Sot : MonoBehaviour
{
    public Button btnhost, btncon, btnoff;
    public Text ipv4text;
    public Canvas canvas;
    private Socket opposot, seversot;
    private bool online = false;
    private bool start = false;
    private int client;
    public GameInfo gameinfo;
    public InfoBar infobar;
    private Dictionary<int, string> sigmap;
    private int frame = 0;
    private int flock = -1; // ��ʾCreateSignal�����޸�flock֡
    public float framerate;   // 20����һ������֡
    private float starttime;
    private float time;
    private AI ai;
    private char Int2Char(int i)
    {
        return (char)('A' + i);
    }
    private int Char2Int(char c)
    {
        int i = c - 'A';
        return i;
    }
    public void CreateSignal(int type, Position pos, int id = 0)
    {
        // typeָ���ź�����
        // 1���죬2ȡ�����죬3�з���4ȡ���з���5��ļ��6ȡ����ļ
        // ����idֻ��1�ֽڣ������������̫�࣬�Ʊ�Ҫ�ĳ�2�ֽڣ�����Parse��صĺ��������޸�
        string res = string.Format("{0}{1}{2}{3}{4}",
            Int2Char(type),
            Int2Char(pos.right),
            Int2Char(pos.row),
            Int2Char(pos.col),
            Int2Char(id));
        flock = frame;
        sigmap[frame] += res;
        flock = -1;
    }
    private void ParseSignal(string msg, out int type, out Position pos, out int id)
    {
        type = Char2Int(msg[0]);
        pos = new Position(Char2Int(msg[1]), Char2Int(msg[2]), Char2Int(msg[3]));
        id = Char2Int(msg[4]);
    }
    void Awake()
    {
        sigmap = new Dictionary<int, string>();
        sigmap.Add(0, "");
        btnhost.onClick.AddListener(host);
        btncon.onClick.AddListener(connect);
        btnoff.onClick.AddListener(offline);
        framerate = 0.02f;
    }

    // Update is called once per frame
    private bool firststart = false;
    void Update()
    {
        if (gameinfo.start && !firststart)
        {
            start = true;
            gameinfo.GameStart();
            starttime = Time.realtimeSinceStartup;
            firststart = true;
        }
        time = Time.realtimeSinceStartup;
        if (start)
        {
            if (time > starttime + framerate * frame)
            {
                int loser = gameinfo.players.Move();
                if(loser != -1)
                {
                    if(loser == client)
                    {
                        Debug.Log("������");
                    }
                    else
                    {
                        Debug.Log("��Ӯ��");
                    }
                    start = false;
                }

                // �½���һ֡�Ŀռ�
                sigmap.Add(frame + 1, "");
                frame++;

                // �鿴��
                while (flock == frame - 1) ;
                string oppostr = "000";
                try
                {
                    // ����Ϣ
                    Send(frame - 1);
                    // ����Ϣ
                    oppostr = Receive();

                }
                catch (Exception ex)
                {
                    Console.WriteLine("���շ���˷��͵���Ϣ����:" + ex.ToString());
                }

                if (client == 0)
                {
                    Parse(sigmap[frame - 1]);
                    Parse(oppostr);
                }
                else if (client == 1)
                {
                    Parse(oppostr);
                    Parse(sigmap[frame - 1]);
                }
                // �ͷ�sigmap�ռ�
                sigmap.Remove(frame - 1);
            }
        }
    }
    

    private void host()
    {
        try
        {
            seversot = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress address = IPAddress.Parse("0.0.0.0");
            IPEndPoint endPoint = new IPEndPoint(address, 9527);
            seversot.Bind(endPoint);
            seversot.Listen(5);
            opposot = seversot.Accept();

            frame = 0;
            client = 0;
            gameinfo.Init(client);
            canvas.gameObject.SetActive(false);
            starttime = Time.realtimeSinceStartup;
            online = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("���շ���˷��͵���Ϣ����:" + ex.ToString());
        }
    }
    private void connect()
    {
        string host = ipv4text.text;
        if(host == "")
        {
            host = "127.0.0.1";
        }
        try
        {
            int port = 9527;
            ///�����ս��EndPoint
            IPAddress ip = IPAddress.Parse(host);
            IPEndPoint ipe = new IPEndPoint(ip, port);//��ip�Ͷ˿�ת��ΪIPEndpointʵ��

            ///����socket�����ӵ�������
            opposot = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//����Socket
            Debug.Log("Conneting��");
            opposot.Connect(ipe);//���ӵ�������


            frame = 0;
            client = 1;
            gameinfo.Init(client);
            canvas.gameObject.SetActive(false);
            starttime = Time.realtimeSinceStartup;
            online = true;

        }
        catch (ArgumentNullException e)
        {
            Debug.Log(string.Format("argumentNullException: {0}", e));
        }
        catch (SocketException e)
        {
            Debug.Log(string.Format("SocketException:{0}", e));
        }
    }
    private void offline()
    {
        frame = 0;
        client = 0;
        gameinfo.Init(client);
        canvas.gameObject.SetActive(false);
        starttime = Time.realtimeSinceStartup;
        ai = new AI();
        ai.Init(gameinfo);
        online = false;
    }

    private void Parse(string str)
    {
        for(int i = 3; i < str.Length; i = i + 5)
        {
            string msg = str.Substring(i, 5);
            int type;
            Position pos;
            int id;
            ParseSignal(msg, out type, out pos, out id);
            infobar.RecvSignal(type, pos, id);
        }
    }
    private void Send(int fid)
    {
        try
        {
            // ����3λ��Ϣͷ
            int len = sigmap[fid].Length;
            string slen = len.ToString();
            while(slen.Length < 3)
            {
                slen = "0" + slen;
            }

            sigmap[fid] = slen + sigmap[fid];
            if (online)
            {
                // Debug.Log(string.Format("���ڴ��͵�{0}֡��{1}", fid, sigmap[fid]));
                byte[] b = Encoding.ASCII.GetBytes(sigmap[fid]);
                opposot.Send(b, b.Length, 0);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("���շ���˷��͵���Ϣ����:" + ex.ToString());
        }
    }
    private int calmdown = 0;
    private string Receive()
    {
        string recvStr = "";
        if (online)
        {
            // ����Ϣ��������Ϣͷ
            int msglen = 3;
            byte[] buffer = new byte[msglen];
            recvStr = "";
            int need = msglen;
            while (need > 0)
            {
                int bytes = opposot.Receive(buffer, need, 0);//�ӷ������˽��ܷ�����Ϣ
                recvStr = recvStr + Encoding.ASCII.GetString(buffer, 0, bytes);
                need = need - bytes;
            }
            // ������Ϣ����
            msglen = Convert.ToInt32(recvStr);
            buffer = new byte[msglen];
            need = msglen;
            while (need > 0)
            {
                int bytes = opposot.Receive(buffer, need, 0);//�ӷ������˽��ܷ�����Ϣ
                recvStr = recvStr + Encoding.ASCII.GetString(buffer, 0, bytes);
                need = need - bytes;
            }
        }
        else
        {
            if(calmdown > 0)
            {
                calmdown--;
                recvStr = "000";
            }
            else
            {
                recvStr = ai.Operator();
                if(recvStr != "000")
                {
                    calmdown = 20;
                }
            }
        }
        return recvStr;
    }
}
