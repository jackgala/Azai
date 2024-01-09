using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerStabCol : MonoBehaviour
{
    public static bool isStabbing = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.name == "enemy")
        {
            isStabbing = true;
        }
        else {
            isStabbing = false; 
        }
        //else
        //{
        //    Debug.Log("Collider is not enemy - it's "
        //        + collision.collider.name);
        //}
    }
}
