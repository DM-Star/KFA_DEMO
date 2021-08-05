using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bar : MonoBehaviour
{
    public Color color;
    public float maxvalue;
    public float value;
    //private GameObject bar;
    private RectTransform trans;
    public GameObject valuebar;
    // Start is called before the first frame update
    void Awake()
    {
        trans = valuebar.GetComponent<RectTransform>();
        valuebar.GetComponent<Image>().color = color;
    }

    // Update is called once per frame
    void Update()
    {
        if (maxvalue != 0)
        {
            trans.sizeDelta = new Vector2(226 * value / maxvalue, 26);
        }
    }
}
