using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerStrikeCol : MonoBehaviour
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
    //every collider looks for 
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.name == "parryPoint")
        {
            isParried = true;
            //Debug.Log("SetTrigger Parry Commented out");
            Debug.Log("IsParried is "+isParried);
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
            //Debug.Log("Looking for player strike\nCollider is not strikePoint, hitPoint, or parryPoint - it's "
            //    + collision.collider.name);
            isParried = false;
            Debug.Log("IsParried is " + isParried);
            isStriking = false;
            isColliding = false; 
        }
    }
}
