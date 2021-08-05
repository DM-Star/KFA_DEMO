using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FoodIronbar : MonoBehaviour
{
    public Text foodtext, irontext;
    private int food, iron;
    public void SetNum(int f, int i)
    {
        food = f;
        iron = i;
    }
    public void SetFood(int f)
    {
        food = f;
    }

    public void SetIron(int i)
    {
        iron = i;
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
    // Start is called before the first frame update
    void Awake()
    {
        food = 0;
        iron = 0;
    }

    // Update is called once per frame
    void Update()
    {
        foodtext.text = food.ToString();
        irontext.text = iron.ToString();
    }
}
