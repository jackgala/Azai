using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bossStrikeCol : MonoBehaviour
{
    public static bool isStriking = false;
    public static bool isParried = false;
    public static bool isColliding = false;
    //public GameObject placeHolder;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //this.transform.position = placeHolder.transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("Boss strike");
        if (collision.collider.name == "parryPoint")
        {
            isParried = true;
        }
        else if (collision.collider.name == "hitPoint")
        {
            isStriking = true;
        }
        else if (collision.collider.name == "strikePoint")
        {
            isColliding = true;
        }
        else
        {
            //Debug.Log("Looking for boss strike\nCollider is not strikePoint, hitPoint, or parryPoint - it's "
            //    + collision.collider.name);
        }
    }
}
