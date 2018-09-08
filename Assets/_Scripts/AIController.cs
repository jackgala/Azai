using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
	AI reasoning:
		Variables:
			Action state: {Attack, Dash, Run, Idle}
			Distance: {InRange, OutOfRange, Coward}
		Actions:
			{Dash, Attack, Switch, Run, Idle???}
	
		Descision tree:
			If (InRange)
				if (attack)
					dash away
				if (idle)
					attack
				if (dash towards)
					dash away
				if (dash away)
					dash towards
				if (run towards)
					attack
				if (run away)
					run towards
			If (OutOfRange)
				if (attack)
					idle
				if (idle)
					run towards
				if (dash towards)
					idle
				if (dash away)
					dash towards
				if (run towards)
					idle
				if (run away)
					dash towards
			If (Coward)
				dash towards

 */

public class AIController : MonoBehaviour {

	public float noise;
	public float refreshRate = 1f;

	private int[,] stateSpace = new int[3,6];
	private enum Distance {InRange, OutOfRange, Coward};
	private enum Action {Attack, Idle, DashForward, DashBackward, RunForward, RunBackward};
	private enum Direction {Backward, Forward};
	public float dashCooldown = 0.6f;
	public float dashCooldown2 = 0.3f;
	public float dashLength = 0.05f;
	public float attackCooldown = 0.6f;
	public float stanceSwitchDelay = 0.3f;
	public float baseSpeed = 20f;
	public float attackSpeedModifier = 0.85f;
	// public Sprite[] runningForwardSheet;
	// public Sprite[] runningForwardRightStance;
	// public Sprite[] runningForwardUpStance;
	// public Sprite[] runningBackwardRightStance;
	// public Sprite[] runningBackwardUpStance;
	// public Sprite[] runningBackwardSheet;
	//public Sprite[] idle;
	//public Sprite[] attackF;
	private int direction;
	private int running = 0;
    public Collider2D hitbox;
    public Collider2D attackbox;
	public PlayerController enemy;
	//private int attack = 0;
	private int idleC = 0;
	private int attackC = 0;
	private int stance = 0;
	private int enemyStance;
	private int enemyActionState;
	private int enemyRange;
	private float attackMoveSpeed;
	private float speed;
	private SpriteRenderer spriteR;
    [Serializable]
    public struct NamedImage
    {
        public string name;
        public Sprite image;
    }
    public NamedImage[] pictures;
	private enum Stance {Left, High, Right, Low};
    private Sprite[,,] runHash = new Sprite[4, 6, 3];
    private void loadHash()
    {
		/*
		Fill in Forwrd Dir, Back Dir,
				High stance, Right Stance, Left Stance
				Run
		 */
		/*
        for (int x = 0; x < 6; x++)
        {
            loadingStance += x;
            runHash[] = pictures[loadingStance].image;
        }
        for (int x = 0; x < 6; x++)
        {
            loadingStance += x;
            runHash[1].Add(pictures[loadingStance].name, pictures[loadingStance].image);
        }
        for (int x = 0; x < 6; x++)
        {
            loadingStance += x;
            runHash[2].Add(pictures[loadingStance].name, pictures[loadingStance].image);
        }*/

		for (int x = 0; x < 3; x++)
		{
			for(int y = 0; y < 6; y++) {
				runHash[x, Action.RunForward.GetHashCode(), y%3] = pictures[x*11+y].image;
			}
			for(int y = 0; y < 3; y++) {
				runHash[x, Action.Attack.GetHashCode(), y] = pictures[x*11+y+6].image;
				runHash[x, Action.Attack.GetHashCode(), y] = pictures[x*11+y+6].image;
			}
			runHash[x, Action.DashForward.GetHashCode(), 0] = pictures[x*11+9].image;
			runHash[x, Action.DashBackward.GetHashCode(), 0] = pictures[x*11+10].image;
		}

    }
    Animator anim;

	private bool isMoving = false;
	private bool isDashing = false;
	private bool isAttacking = false;
	private bool isSwitching = false;
	private int tempStance;

	private float timer;
	private float dashtimer;
	private float attacktimer;
	private float stanceTimer;
    // Use this for initialization
    void Start () {
		anim = GetComponent<Animator> ();
		isMoving = false;
		spriteR = gameObject.GetComponent<SpriteRenderer>();
		enemyStance = enemy.stance;
		enemyActionState = GetEnemyActionState();
		enemyRange = JudgeRange();
		speed = baseSpeed;
		attackMoveSpeed = baseSpeed * attackSpeedModifier;
		loadHash();
		// In Range
		stateSpace[Distance.InRange.GetHashCode(), Action.Attack.GetHashCode()] =
					Action.DashBackward.GetHashCode();
		stateSpace[Distance.InRange.GetHashCode(), Action.Idle.GetHashCode()] =
					Action.Attack.GetHashCode();
		stateSpace[Distance.InRange.GetHashCode(), Action.DashForward.GetHashCode()] =
					Action.DashBackward.GetHashCode();
		stateSpace[Distance.InRange.GetHashCode(), Action.DashBackward.GetHashCode()] =
					Action.DashForward.GetHashCode();
		stateSpace[Distance.InRange.GetHashCode(), Action.RunForward.GetHashCode()] =
					Action.Attack.GetHashCode();
		stateSpace[Distance.InRange.GetHashCode(), Action.RunBackward.GetHashCode()] =
					Action.RunForward.GetHashCode();
		// Out of Range
		stateSpace[Distance.OutOfRange.GetHashCode(), Action.Attack.GetHashCode()] =
					Action.Idle.GetHashCode();
		stateSpace[Distance.OutOfRange.GetHashCode(), Action.Idle.GetHashCode()] =
					Action.RunForward.GetHashCode();
		stateSpace[Distance.OutOfRange.GetHashCode(), Action.DashForward.GetHashCode()] =
					Action.Idle.GetHashCode();
		stateSpace[Distance.OutOfRange.GetHashCode(), Action.DashBackward.GetHashCode()] =
					Action.DashForward.GetHashCode();
		stateSpace[Distance.OutOfRange.GetHashCode(), Action.RunForward.GetHashCode()] =
					Action.Idle.GetHashCode();
		stateSpace[Distance.OutOfRange.GetHashCode(), Action.RunBackward.GetHashCode()] =
					Action.DashForward.GetHashCode();
		// Coward
		stateSpace[Distance.Coward.GetHashCode(), Action.Attack.GetHashCode()] =
					Action.DashForward.GetHashCode();
		stateSpace[Distance.Coward.GetHashCode(), Action.Idle.GetHashCode()] =
					Action.DashForward.GetHashCode();
		stateSpace[Distance.Coward.GetHashCode(), Action.DashForward.GetHashCode()] =
					Action.DashForward.GetHashCode();
		stateSpace[Distance.Coward.GetHashCode(), Action.DashBackward.GetHashCode()] =
					Action.DashForward.GetHashCode();
		stateSpace[Distance.Coward.GetHashCode(), Action.RunForward.GetHashCode()] =
					Action.DashForward.GetHashCode();
		stateSpace[Distance.Coward.GetHashCode(), Action.RunBackward.GetHashCode()] =
					Action.DashForward.GetHashCode();
	}

    // Update is called once per frame
    //spriteRenderer
    void Update () {
		timer += Time.deltaTime;

		if(timer >= refreshRate) {
			timer = 0;

			// Judge Range
			enemyRange = JudgeRange();
			
			// get enemy state
			enemyStance = enemy.stance;
			enemyActionState = GetEnemyActionState();
			
			// get next state (w/ noise)
			int nextAction = stateSpace[enemyRange, enemyActionState];
			// Noise???

			Debug.Log(enemyRange);
			Debug.Log(enemyActionState);
			Debug.Log(nextAction);

			// set movement vars
			if(nextAction == Action.Attack.GetHashCode())
				isAttacking = true;
			if(nextAction == Action.DashBackward.GetHashCode()){
				direction = Direction.Backward.GetHashCode();
				isMoving = true;
				isDashing = true;
			}
			if(nextAction == Action.DashForward.GetHashCode()){
				direction = Direction.Forward.GetHashCode();
				isMoving = true;
				isDashing = true;
			}
			if(nextAction == Action.Idle.GetHashCode()){
				isMoving = false;
				isDashing = false;
			}
			if(nextAction == Action.RunBackward.GetHashCode()){
				direction = Direction.Backward.GetHashCode();
				isMoving = true;
				isDashing = false;
			}
			if(nextAction == Action.RunForward.GetHashCode()){
				direction = Direction.Forward.GetHashCode();
				isMoving = true;
				isDashing = false;
			}

			// change stance with a random chance (if no change in action??)
		}
	}

	//runs at a fixed rate.
	void FixedUpdate()
	{
		if (isSwitching) {
			stanceTimer += Time.deltaTime;
			if (stanceTimer >= stanceSwitchDelay) {
				stanceTimer = 0;
				isSwitching = false;
			}
		}
		else if (stance != tempStance) {
			stance = tempStance;
		}
		else if (attacktimer > 0) {
			attacktimer += Time.deltaTime;
			if (attacktimer >= attackCooldown) {
				attacktimer = 0;
			}
		}
		else if (isAttacking && attacktimer == 0 && stance == tempStance) {
			spriteR.sprite = runHash[stance, Action.Attack.GetHashCode(), attackC/5];
			attackC++;
			if (attackC == 15) {
				attackC = 0;
				isAttacking = false;
				attacktimer = 0.001f;
				speed = baseSpeed;
			}
		}
		if (isMoving) {
			if (isDashing) {
				dashtimer += Time.deltaTime;

				if (dashtimer <= dashLength) { 	//while within the threshold, animation will run.
					spriteR.sprite = runHash[stance, Action.DashForward.GetHashCode(), 0];
					transform.Translate ((direction * (-2) + 1) * Time.deltaTime * speed * 5f, 0, 0);
				} else if (dashtimer >= dashLength + dashCooldown2) {
					Running ();
					transform.Translate ((direction * (-2) + 1) * Time.deltaTime * speed, 0, 0);
					//SetAnims ("isMovingRight", true);
				} else {
					spriteR.sprite = runHash[stance, Action.DashForward.GetHashCode(), 0];
					transform.Translate ((direction * (-2) + 1) * Time.deltaTime * speed, 0, 0);
				}
				if (dashtimer >= dashLength + dashCooldown) {
					isDashing = false;
					dashtimer = 0;
				}
			} else {
				Running ();
				transform.Translate ((direction * (-2) + 1) * Time.deltaTime * speed, 0, 0);
				//SetAnims ("isMovingRight", true);
			}
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

    private int GetEnemyActionState()
    {
        if(enemy.isDashing)
			return enemy.direction == Direction.Forward.GetHashCode() ? Action.DashForward.GetHashCode() : Action.DashBackward.GetHashCode();
		if(enemy.isMoving)
			return enemy.direction == Direction.Forward.GetHashCode() ? Action.RunForward.GetHashCode() : Action.RunBackward.GetHashCode();
		if(enemy.isAttacking)
			return Action.Attack.GetHashCode();
		return Action.Idle.GetHashCode();
    }

    private int JudgeRange()
    {
		float distance = Math.Abs(enemy.gameObject.transform.position.x - transform.position.x);
        if(distance < 6)
			return Distance.InRange.GetHashCode();
		if(distance < 12)
			return Distance.OutOfRange.GetHashCode();
		return Distance.Coward.GetHashCode();
    }

	void Running(){
		if (running == 15) {
			running = 0;
		}
		if (!isAttacking) {
			spriteR.sprite = runHash[stance, Action.RunForward.GetHashCode(), running / 5];
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
			//runningForwardSheet [counter];
			spriteR.sprite = runHash [stance, Action.Idle.GetHashCode(), idleC / 9];
		} 
		idleC++;
	}
}
