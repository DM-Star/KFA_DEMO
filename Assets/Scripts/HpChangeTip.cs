using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpChangeTip : MonoBehaviour
{
    public GameObject word;
    private int frame = 25;
    private bool start = false;
    // Start is called before the first frame update
    void Awake()
    {
        word.GetComponent<MeshRenderer>().sortingOrder = 3;
    }

    public void Init(Damage damage, Vector3 position)
    {
        transform.localPosition = new Vector3(position.x + 5, position.y, 0);
        TextMesh damagetext = word.GetComponent<TextMesh>();
        damagetext.text = string.Format("-{0}", damage.damage);

        switch (damage.attack_type)
        {
            case 1:
                // �ⲫ
                damagetext.color = new Color(255, 0, 0, 180);
                break;
            case 2:
                // ����
                damagetext.color = new Color(0, 230, 255, 180);
                break;
            case 3:
                // ����
                damagetext.color = new Color(103, 0, 255, 180);
                break;
        }

        start = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (start)
        {
            if(frame > 0)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y + 0.4f, 0);
                frame--;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
