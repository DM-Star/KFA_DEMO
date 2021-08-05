using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Tip : MonoBehaviour
{
    public Text text;
    public int start = 0;
    void Awake()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(start > 0)
        {
            start--;
            transform.position = new Vector3(transform.position.x, transform.position.y + 2, transform.position.z);
        }
    }

    public void Init(string msg)
    {
        text.text = msg;
        start = 25;
    }
}
