using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// introduce up slash from down slash position
/// introduce parry done by transitionint from up position to down position
/// cannot attack while dashing
/// </summary>
public class PlayerMovement : MonoBehaviour {

    public CharacterController2D controller;

    public float runSpeed = 40f;

    float horizontalMove = 1f;

    public Animator animator;

    public Boss_Run boss;
    public Rigidbody2D rb;
    public float speed;
    public Transform strikePoint;
    public Transform stabPoint;
    public float strikeRange = 0.5f;
    public float switchRange = 0.25f;
    public LayerMask enemyLayer;
    public float face = 1;
    public float inContact = 0;
    public float trackCon = 0;
    public float switchCounter = 1;
    float timeLeft = 0;
    bool upStanceAct = false;
    float timeLeftBoss = 1;
    public GameObject victoryPanel;
    private void Start()
    {
        playerParryCol.isParrying = true;
    }

    // Update is called once per frame
    void Update() {
        //if statement for hitpoint if it got slashed or stabbed
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
        if(animator)
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
        /*
         if(player.slash == true && Boss.Parry == true)
         {
               Boss takes no damage and counters Player            
         }
         else if(player.Parray == true and Boss.slash == true)
         {
            player takes no damage and counters boss 
        }
        else if(Boss.isStabbing == true && player.slash == false)
        {
            Game over for player
        }
        else if(player.isStabbing == true && Boss.slash == false)
        {
            Game over for player
        }
        resetColFlags();
         */
        resetColFlags();
        if (Input.GetKey((KeyCode.Semicolon)))
        {
            Debug.Log("semicolon called");
            if (Input.GetKeyDown(KeyCode.K) /*&& Input.GetKeyDown(KeyCode.LeftShift)*/)
            {
                DownStance();
            }
            if (Input.GetKeyDown(KeyCode.I) /*&& Input.GetKeyDown(KeyCode.LeftShift)*/)
            {
                Debug.Log("I called");
                UpStance();
                
            }
        }
        else {
            if (Input.GetKeyDown(KeyCode.K))
            {
                Slash();
               
                //if isColliding: move the player a litte bit away form the boss
                //if isStriking: end the game it has won

            }
        }

        if (animator.GetBool("isUp") == false && boss.isUp == false)
        {
            Flip();
        }
        else if (animator.GetBool("isUp") == true && boss.isUp == true)
        {
            Flip();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift)) {
            Dash();
        }
        if (upStanceAct == true)
        {
            timeLeft -= Time.deltaTime;
           /* if (timeLeft <= 0)
            {
                animator.SetBool("isParry", false);
               // Debug.Log("Time up. Parry should be false");
                // timeLeft = 2.5f;
                upStanceAct = false;
            }*/
        }
    }
    //not being called. Parried even if away from boss
    void UpStance() {
        animator.SetBool("isParry", true);
       // Debug.Log("IsParry: true");
        animator.SetBool("isUp", true);
        upStanceAct = true;
        Debug.Log("Upstance called");
       // Debug.Log("Setting up timer for parry to be false");
    }


    void DownStance() {
        animator.SetBool("isUp", false);
    }
    void victory() {
        victoryPanel.SetActive(true);
        FindObjectOfType<Boss_Run>().die();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = 0;
        GetComponent<CharacterController2D>().enabled = false;
        Destroy(this);
    }

    void Slash() {
        
        Collider2D[] hitEnemys = Physics2D.OverlapCircleAll(strikePoint.position, strikeRange, enemyLayer);
        playerStrikeCol.isStriking = true;
        foreach (Collider2D enemy in hitEnemys) {
            Debug.Log("We Hit " + enemy.name);
            victory();
        }
        float check = GetDistance(boss.transform.position);

        //if (playerStrikeCol.isParried == true)
      //  {
                if (check <= 1f && playerStrikeCol.isParried == true)
                {
            Debug.Log("Should be parried/stunned by boss.");
            animator.SetBool("isParried", true);
                //turn off being parried
                //Debug.Log("Distance: " + check);
                //Debug.Log("Boss was able to parry");
                
           // Set timer

                //Debug.Log("Boss Setting up timer for parry to be false");
            timeLeftBoss -= Time.deltaTime;
            //Debug.Log("Boss parry time left: " + timeLeftBoss);
            if (timeLeft <= 0)
            {
                animator.SetBool("isParried", false);
                playerStrikeCol.isParried = false;
                Debug.Log("Boss Time up. Parry should be false");
            }
        }
            else {
                if (playerStrikeCol.isParried == true)
                {
                    Debug.Log("Player.isParried is set to true, but the distance is off.\nDistance: "+ check);
                }
                animator.SetTrigger("Attack");
            }
        //}
        //else
        //{
        //    Debug.Log("Characters are too far from each other");
        //}
    }

    public float GetDistance(Vector3 player)
    {//Returns distance between player and enemy
        return Vector3.Distance(player, transform.position);
    }

    void Stab()
    {
        Collider2D[] hitEnemys = Physics2D.OverlapCircleAll(stabPoint.position, strikeRange, enemyLayer);
        playerStabCol.isStabbing = true;
        foreach (Collider2D enemy in hitEnemys){
            Debug.Log("We Hit " + enemy.name);
            victory();
        }
    }

    

    void Dash() {
       controller.Dash = true;
    }

    void Flip() {
        //Debug.Log(boss.rb.position.x);
        //Debug.Log(rb.position.x);
        if (boss.rb.position.x - rb.position.x >= 0)
        {
            face = 1;
        }
        else {
            face = -1;
        }
    }

    void FixedUpdate()
    {
        //Moves the character
        controller.Move(horizontalMove * Time.fixedDeltaTime, false, false, face);
        controller.checkDash(horizontalMove * Time.fixedDeltaTime);
    }

    public static void resetColFlags()
    {
        bossParryCol.isParrying = false;
        bossStabCol.isStabbing = false;
        bossStrikeCol.isStriking = false;

        playerParryCol.isParrying = false;

        //Debug.Log("IsParry: false");
        playerStabCol.isStabbing = false;
        playerStrikeCol.isStriking = false;
    }
}
