using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
Sketch of the multi-level array:

level 1: Direction
level 2: Stance
level 3: Move State
level 4: Sprites

We can use Enums for Direction, Stance, and Move State

Need to add a Facing layer later
*/

public class PlayerController : MonoBehaviour {

    //Implement values into 
    //Set up number system with stance
	// cooldown values
    public float dashCooldown = 0.6f;
	public float dashCooldown2 = 0.3f;
	public float dashLength = 0.05f;
	public float attackCooldown = 0.6f;
	public float stanceSwitchDelay = 0.3f;
	public float baseSpeed = 20f;
	public float attackSpeedModifier = 0.85f;
	// state vars
	public int direction;
	private int facing = 0;
	private int running = 0;
	public int stance = 0;
	private int tempStance;
	public bool isMoving;
	public bool isDashing;
	public bool isAttacking;
	private bool isSwitching;
	// hit boxes
    public Collider2D hitbox;
    public Collider2D attackbox;
	// counters
	private int idleC = 0;
	private int attackC = 0;
	private float dashtimer;
	private float attacktimer;
	private float stanceTimer;
	// speed
	private float attackMoveSpeed;
	private float speed;
	// components
	private SpriteRenderer spriteR;
	// Sprites
    [Serializable]
    public struct NamedImage
    {
        public string name;
        public Sprite image;
    }
    public NamedImage[] pictures;
	// Enums
	private enum Direction {Forward, Backward};
	private enum Stance {Left, High, Right, Low};
	private enum Action {Idle, Run, Attack, Dash};
	// Sprite hash map: Direction, Stance, Action, Frame Index
    private Sprite[,,,] runHash = new Sprite[2, 4, 4, 3];
	// Enemy
	public AIController enemy;

	// Loads the sprites from pictures into runHash
    private void loadHash()
    {
		/*
		Fill in Forwrd Dir, Back Dir,
				High stance, Right Stance, Left Stance
				Run
		 */

		for (int x = 0; x < 3; x++) // 3 stances
		{
			for(int y = 0; y < 3; y++) { // Idle
				runHash[Direction.Forward.GetHashCode(), x, Action.Idle.GetHashCode(), y] = pictures[x*14+y].image;
				runHash[Direction.Backward.GetHashCode(), x, Action.Idle.GetHashCode(), y] = pictures[x*14+y].image;
			}
			for(int y = 0; y < 6; y++) { // run forwards, backwards
				runHash[y/3, x, Action.Run.GetHashCode(), y%3] = pictures[x*14+y+3].image;
			}
			for(int y = 0; y < 3; y++) { // Attack
				runHash[Direction.Forward.GetHashCode(), x, Action.Attack.GetHashCode(), y] = pictures[x*14+y+9].image;
				runHash[Direction.Backward.GetHashCode(), x, Action.Attack.GetHashCode(), y] = pictures[x*14+y+9].image;
			}
			// Dashing
			runHash[Direction.Forward.GetHashCode(), x, Action.Dash.GetHashCode(), 0] = pictures[x*14+12].image;
			runHash[Direction.Backward.GetHashCode(), x, Action.Dash.GetHashCode(), 0] = pictures[x*14+13].image;
		}

    }

    // Use this for initialization
	// Setup
    void Start () {
		isMoving = false;
		spriteR = gameObject.GetComponent<SpriteRenderer>();
		speed = baseSpeed;
		attackMoveSpeed = baseSpeed * attackSpeedModifier;
		loadHash();
	}
	
	// Update is called once per frame
	// Controller
	// Where the magic starts
	void Update () {
		// check facing direction
		if(enemy.gameObject.transform.position.x - transform.position.x < 0) {
			spriteR.flipX = true;
			facing = 1;
		}
		else {
			spriteR.flipX = false;
			facing = 0;
		}
		// Dash
		if (Input.GetKey(KeyCode.Semicolon)) {
			isDashing = true;
		}
		// Attack and Stance
		if (Input.GetKey(KeyCode.J)) { // Left
			if (!Input.GetKey(KeyCode.LeftShift)){ // Attack
				isAttacking = true;
				speed = attackMoveSpeed;
			}
			if (stance != Stance.Left.GetHashCode()){ // Stance switch
				isSwitching = true;
				tempStance = Stance.Left.GetHashCode() + facing * 2;
			}
		}
		/*  if (Input.GetKey(KeyCode.K)) {
		 	stance = Stance.Low.GetHashCode();
		 } */
		if (Input.GetKey(KeyCode.I)) { // Up
			if (!Input.GetKey(KeyCode.LeftShift)){ // Attack
				isAttacking = true;
				speed = attackMoveSpeed;
			}
			if (stance != Stance.High.GetHashCode()){ // Stance switch
				isSwitching = true;
				tempStance = Stance.High.GetHashCode();
			}
		}
		if (Input.GetKey(KeyCode.L)) { // Right
			if (!Input.GetKey(KeyCode.LeftShift)){ // Attack
				isAttacking = true;
				speed = attackMoveSpeed;
			}
			if (stance != Stance.Right.GetHashCode()){ // Stance switch
				isSwitching = true;
				tempStance = Stance.Right.GetHashCode() - facing * 2;
			}
		}
		// Movement
		if (Input.GetKey (KeyCode.D)) { // Right
			isMoving = true;
			direction = Direction.Forward.GetHashCode();
		} else if (Input.GetKey (KeyCode.A)) { // Left
			isMoving = true;
			direction = Direction.Backward.GetHashCode();
		}
		else { // Idle
			isMoving = false;
			isDashing = false;
		}
	}

	// runs at a fixed rate.
	// Implementer
	void FixedUpdate()
	{
		if (isSwitching) { 							// If currently switching
			stanceTimer += Time.deltaTime;			//  increment
			if (stanceTimer >= stanceSwitchDelay) { //  If exceeded stance cooldown
				stanceTimer = 0;					//   Reset timer
				isSwitching = false;				//   Set swithing to false
			}
		}
		else if (stance != tempStance) {			// If not currently switching, but not switched
			stance = tempStance;					//  Switch
		}
		else if (attacktimer > 0) {					// Attack cooldown
			attacktimer += Time.deltaTime;
			if (attacktimer >= attackCooldown) {
				attacktimer = 0;
			}
		}
		//	 If  Attacking,     can attack, and     not currently switching stance
		else if (isAttacking && attacktimer == 0 && stance == tempStance) {
			spriteR.sprite = runHash[direction ^ facing, stance, Action.Attack.GetHashCode(), attackC/5];
			attackC++;
			if (attackC == 15) {
				attackC = 0;
				isAttacking = false;
				attacktimer = 0.001f;
				speed = baseSpeed;
			}
		}
		if (isMoving) { // ... duh
			if (isDashing) { // ... ... double duh
				dashtimer += Time.deltaTime;

				// If within dash time threshold
				if (dashtimer <= dashLength) { 
					// dash
					spriteR.sprite = runHash[direction ^ facing, stance, Action.Dash.GetHashCode(), 0];
					transform.Translate ((direction * (-2) + 1) * Time.deltaTime * speed * 5f, 0, 0);
				// Dash cooldown
				} else if (dashtimer >= dashLength + dashCooldown2) {
					// run
					Running ();
					transform.Translate ((direction * (-2) + 1) * Time.deltaTime * speed, 0, 0);
				// dashLength < now < dashCooldown2
				// Inital burst done, momentary freeze frame
				} else {
					spriteR.sprite = runHash[direction ^ facing, stance, Action.Dash.GetHashCode(), 0];
					transform.Translate ((direction * (-2) + 1) * Time.deltaTime * speed, 0, 0);
				}
				// Dash cooldown done
				if (dashtimer >= dashLength + dashCooldown) {
					isDashing = false;
					dashtimer = 0;
				}
			// not dashing
			} else {
				Running ();
				transform.Translate ((direction * (-2) + 1) * Time.deltaTime * speed, 0, 0);
				//SetAnims ("isMovingRight", true);
			}
		// Not moving
		} else {
			//SetAnims ("isMovingRight", false);
			if(dashtimer > 0) {
				dashtimer += Time.deltaTime;
				if (dashtimer >= dashLength + dashCooldown) {
					dashtimer = 0;
				}
			}
			Idle();
		}
	}

	void Running(){
		if (running == 15) {
			running = 0;
		}
		if (!isAttacking) {
			spriteR.sprite = runHash[direction ^ facing, stance, Action.Run.GetHashCode(), running / 5];
		}
		// if (direction == 1 && running % 5 == 0) {
        //     spriteR.sprite = (Sprite)runHash[stance]["Forwards"];

		// } else if (direction == -1&& running % 5 == 0) {
		// 	//	runningBackwardSheet [counter];
		// 	if (stance == 3) {
		// 		spriteR.sprite = runningBackwardRightStance [running / 5];
		// 	}
		// }
		running++;

	}
	void Attack(int counter){
		//if(attack bool triggered by typing in J)
	}
	void Idle(){
		if (idleC >= 27) {
			idleC = 0;
		}
		if (idleC % 9 == 0) {
			spriteR.sprite = runHash[direction ^ facing, stance, Action.Idle.GetHashCode(), idleC / 9];
		} 
		idleC++;
	}
}
