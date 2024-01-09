using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerIsHit : MonoBehaviour
{
    public static bool isHit = false;
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
            isHit = true;
        }
        //else
        //{
        //    Debug.Log("Collider is not enemy - it's "
        //        + collision.collider.name);
        //}
    }
}
