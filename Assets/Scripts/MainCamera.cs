using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    public int left;
    public int right;
    public int height;
    // 是否将视野调整至最左边？
    public bool rightview;
    private bool active = false;
    public void Init(int size, int l, int r, int height, int client)
    {
        GetComponent<Camera>().orthographicSize = size;
        left = l;
        right = r;
        // 需要根据主玩家调整左右
        if (client == 1)
        {
            rightview = true;
            transform.position = new Vector3(right, height, -10);
        }
        else
        {
            rightview = false;
            transform.position = new Vector3(left, height, -10);
        }
        active = true;
    }

    public void GameOver()
    {
        active = false;
        transform.position = new Vector3(right, height, 0);
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Camera>().aspect = 1482 / 732;
        if (active)
        {
            FixView();
        }
    }

    private void FixView()
    {
        float currentx = transform.position.x;
        float currenty = transform.position.y;
        if (rightview)
        {
            if (currentx > right)
            {
                transform.position = new Vector3(right, currenty, -10);
                return;
            }
            else if (currentx == right)
            {
                return;
            }
            else
            {
                float delta = right - currentx;
                currentx = currentx + delta / 30.0f;
                transform.position = new Vector3(currentx, currenty, -10);
            }
        }
        else
        {
            if (currentx < left)
            {
                transform.position = new Vector3(left, currenty, -10);
                return;
            }
            else if (currentx == left)
            {
                return;
            }
            else
            {
                float delta = currentx - left;
                currentx = currentx - delta / 30.0f;
                transform.position = new Vector3(currentx, currenty, -10);
            }
        }
    }
}
