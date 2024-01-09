using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_Run : MonoBehaviour
{

    public float speed = 2.5f;
    public float attackRange = 0.01f;

    public Animator animator;
    
    public enum states {DEF, ESC, AGROSL, AGROSTB};
    int rand;
    int time = 1;
    states curr =  states.AGROSTB;
    public Transform strikePoint;
    public Transform stabPoint;
    public Transform switchPoint;
    public float strikeRange = 0.5f;
    public float switchRange = 0.25f;
    public LayerMask playerLayer;
    public Rigidbody2D rb;
    public Boss boss;
    public bool isUp;
    public bossStrikeCol bossStrike;
    public bossParryCol bossParryCol;
    public GameObject player;
    public float timeLeft = 2f;
    bool setTimer = false;
    public float GetDistance(Vector3 player)
    {//Returns distance between player and enemy
        return Vector3.Distance(player, transform.position);
    }


    public states choseState(states curr, int time) {
        rb = animator.GetComponent<Rigidbody2D>();
        boss = animator.GetComponent<Boss>();
        System.Random rnd = new System.Random();
        rand = rnd.Next(0, 4); 
        while (rand == 0 && GetDistance(player.transform.position)> attackRange)
        {
            //Debug.Log("In Boss, characters are too far from each other\nDistance: "+
              //  GetDistance(player.transform.position));
            playerStrikeCol.isParried = false;
            bossStrikeCol.isParried = false;
            rand = rnd.Next(1, 4);
            //Select another number if CPU wants to parry but is too far from player
        }
      
        states myStates = curr;
        if (time % 100 == 0) {
            switch (rand)
            {
                case 0:
                    //Check distance before setting state to DEF.
                    //If too far away, choose another number
                   // Debug.Log("Boss was able to parry1 "+rand);
                    if (Vector2.Distance(boss.player.position, rb.position) < attackRange)
                    {
                       // Debug.Log("Valid value for distance: " + Vector2.Distance(boss.player.position, rb.position));
                        myStates = states.DEF;
                    }
                    else
                    {
                        rand = rnd.Next(1, 4);
                        break;
                    }
                    //Debug.Log("DEF");
                    break;
                case 1:
                    myStates = states.ESC;
                    //Debug.Log("ESC");
                    break;
                case 2:
                    myStates = states.AGROSL;
                    //Debug.Log("AGROSL");
                    break;
                case 3:
                    myStates = states.AGROSTB;
                    //Debug.Log("AGROSTB");
                    break;
            }
        }
        
        return myStates;

    }

    public void die() {
        animator.SetTrigger("die");
        Destroy(this);
    }

    public states escChoseState(states curr, int time)
    {
        rb = animator.GetComponent<Rigidbody2D>();
        boss = animator.GetComponent<Boss>();
        System.Random rnd = new System.Random();
        rand = rnd.Next(0, 3);
        states myStates = curr;
        if (time % 15 == 0)
        {
            switch (rand)
            {
                case 0:
                    myStates = states.DEF;//Is this also for the parry?
                   // Debug.Log("Boss was able to parry2 " + rand);
                    //Debug.Log("DEF");
                    break;
                case 1:
                    myStates = states.AGROSL;
                   // Debug.Log("AGROSL");
                    break;
                case 2:
                   myStates = states.AGROSTB;
                  // Debug.Log("AGROSTB");
                    break;
            }
        }

        return myStates;

    }


    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    void Update()
    {
        boss.LookAtPlayer();
        //each state leads to a movement behavior and a triggering of an animator trigger that triggers and animation
        //change this to if statements instead of switch
        curr = choseState(curr, time);
        Debug.Log("current State "  + curr);
        if (curr == states.ESC)
        {
            Vector2 target = new Vector2(boss.player.position.x, rb.position.y);
            Vector2 newPos = Vector2.MoveTowards(rb.position, target, -speed * Time.fixedDeltaTime);
            if (Mathf.Abs(boss.player.position.x - rb.position.x) >= 2f)
            {
                //switch value to any of the other three
                curr = escChoseState(curr, time);
            }
            rb.MovePosition(newPos);
        }
        else if (curr == states.AGROSL)
        {
            Vector2 target = new Vector2(boss.player.position.x, rb.position.y);
            Vector2 newPos = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
            if (Vector2.Distance(boss.player.position, rb.position) > attackRange)
            {
                newPos = new Vector2(newPos.x - .01f, newPos.y);
                Debug.Log("Should move from " + rb.position + " to " + newPos);
                rb.MovePosition(newPos);
                animator.SetTrigger("up");
                isUp = false;
            }

            else
            {
                //Attack
                //distance is less than or equal to attack range at wierd distances
                //Debug.Log(Vector2.Distance(boss.player.position, rb.position));
                //Debug.Log(attackRange);
                animator.SetTrigger("slash");
                isUp = true;
                Collider2D[] hitEnemys = Physics2D.OverlapCircleAll(strikePoint.position, strikeRange, playerLayer);
            }
        }
        else if (curr == states.AGROSTB)
        {
            Vector2 target = new Vector2(boss.player.position.x, rb.position.y);
            Vector2 newPos = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);
            Debug.Log("Should move");

            //set animation for moving
            animator.SetTrigger("down");
            isUp = false;
        }
        else if (curr == states.DEF) {
            animator.SetTrigger("down");
            animator.SetTrigger("parry");
            Debug.Log("Should set parry to true.");
            System.Random rnd = new System.Random();
            int rand = rnd.Next(1, 3);
            if (rand == 1)
            {
                Vector2 target = new Vector2(boss.player.position.x, rb.position.y);
                Vector2 newPos = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
                rb.MovePosition(newPos);
            }
            else if (rand == 2)
            {
                Vector2 target = new Vector2(boss.player.position.x, rb.position.y);
                Vector2 newPos = Vector2.MoveTowards(rb.position, target, -speed * Time.fixedDeltaTime);
                rb.MovePosition(newPos);
            }
            //boss parry variable should be boolean not trigger
            if (setTimer == false)
            { 
                //animator.SetTrigger("parry");
                //Debug.Log("SetTrigger Parry Commented out");
                playerStrikeCol.isParried = true;//Needed in order for parry animation to play
                isUp = true;
                Debug.Log("Should set timer");
                setTimer = true;
            }
        }
        time++;
        //if statement for time
        if (setTimer == true)//If if-statement is set on 177 for setTimer to be true
        {//Then code needs to see the timer be false in order to go back to parry
            //Boss won't be able to parry until timer is up and put back to false.
            Debug.Log(Time.deltaTime);
            timeLeft -= 1f;
            Debug.Log(timeLeft);
            //Debug.Log("Timer is running.");
            if (timeLeft <= 0)
            {
                Debug.Log("Time for parry is up");
                animator.ResetTrigger("parry");
                playerStrikeCol.isParried = false;
                timeLeft = 2f;
                setTimer = false;
            }
        }
    }
    

    private void FixedUpdate()
    {
        
    }
    

}
