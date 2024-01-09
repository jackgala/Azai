using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerParryCol : MonoBehaviour
{
    public static bool isParrying = false;
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
        if (collision.collider.name == "strikePoint")
        {
            isParrying = true;
        }
        else
        {
            Debug.Log("Looking for boss parry\nCollider is not bossStrikeCol - it's: "
               + collision.collider.name);
        }
    }

}
