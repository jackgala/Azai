using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMovement : MonoBehaviour
{

    public CharacterController2D controller;

    public float runSpeed = 40f;

    float horizontalMove = 1f;

    public Animator animator;

    public Transform strikePoint;
    public Transform stabPoint;
    public Transform switchPoint;
    public float strikeRange = 0.5f;
    public float switchRange = 0.25f;
    public LayerMask enemyLayer;
    public float face = 1;
    public float inContact = 0;
    public float trackCon = 0;
    public float switchCounter = 1;

    //state based AI
    //distance variable


    //attack boolean
    //parry boolean



    // Update is called once per frame
    void Update()
    {

        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
        //inputs -> booleans that will change the animator
        //Switching to match opponet will be done in CharacterController
        //booleans:
        /*isUp
         * isMoving
         * Attack
         * isMovingBack
         * isParried
         * isFaceRight
         */
        /*if (Input.GetKeyDown(KeyCode.Semicolon))
        {
            shift = true;
        }
        else if (Input.GetKeyUp((KeyCode.Semicolon))) {
            shift = false;
        }*/
        if (Input.GetKey((KeyCode.Semicolon)))
        {
            if (Input.GetKeyDown(KeyCode.K) /*&& Input.GetKeyDown(KeyCode.LeftShift)*/)
            {
                DownStance();
            }
            if (Input.GetKeyDown(KeyCode.I) /*&& Input.GetKeyDown(KeyCode.LeftShift)*/)
            {
                UpStance();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                Slash();
            }
        }

        if (animator.GetBool("isUp") == false)
        {
            Flip();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Dash();
        }

    }

    void UpStance()
    {
        animator.SetTrigger("isParry");
        animator.SetBool("isUp", true);
    }


    void DownStance()
    {
        animator.SetBool("isUp", false);
    }

    void Slash()
    {
        animator.SetTrigger("Attack");
        Collider2D[] hitEnemys = Physics2D.OverlapCircleAll(strikePoint.position, strikeRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemys)
        {
            Debug.Log("We Hit " + enemy.name);
        }
    }

    void Stab()
    {
        Collider2D[] hitEnemys = Physics2D.OverlapCircleAll(stabPoint.position, strikeRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemys)
        {
            Debug.Log("We Hit " + enemy.name);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (strikePoint == null || stabPoint == null || switchPoint == null)
            return;

        //Gizmos.DrawWireSphere(strikePoint.position, strikeRange);
        //Gizmos.DrawWireSphere(stabPoint.position, strikeRange);
        Gizmos.DrawWireSphere(switchPoint.position, switchRange);
    }

    void Dash()
    {
        controller.Dash = true;
    }

    void Flip()
    {
        Collider2D[] hitEnemys = Physics2D.OverlapCircleAll(switchPoint.position, switchRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemys)
        {
            inContact += 1;
            trackCon = inContact;
        }

        if (inContact >= 1)
        {
            if (inContact == 1)
            {

                face *= -1;
                switchCounter += 1;
            }

            if (trackCon < inContact)
            {
                inContact = 0;
            }

            trackCon -= 1;
        }
    }

    void FixedUpdate()
    {
        //Moves the character
        controller.Move(horizontalMove * Time.fixedDeltaTime, false, false, face);
        controller.checkDash(horizontalMove * Time.fixedDeltaTime);
    }
}

