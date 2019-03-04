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

	// enums
	private enum Distance {InRange, OutOfRange, Coward};
	private enum Action {Attack, Idle, DashForward, DashBackward, RunForward, RunBackward};
	private enum Stance {Left, High, Right, Low};
	private enum Direction {Backward, Forward};
	private enum AnimState {Start, Playing, End}
	// State vars
	private AnimState animState = AnimState.Start;
	private Action state = Action.Idle;
	private Action nextAction;
	private int direction;
	private int running = 0;
	private int stance = 0;
	// State space
	private Action[,] stateSpace = new Action[3,6];
	// cooldowns
	public float dashCooldown = 0.6f;
	public float dashCooldown2 = 0.3f;
	public float dashLength = 0.05f;
	public float attackCooldown = 0.6f;
	public float stanceSwitchDelay = 0.3f;
	// speeds
	public float baseSpeed = 20f;
	public float attackSpeedModifier = 0.85f;
	// components
    public Collider2D hitbox;
    public Collider2D attackbox;
	public PlayerController enemy;
	// counters
	private int idleC = 0;
	private int attackC = 0;
	// enemy tracker
	private int enemyStance;
	private int enemyActionState;
	private int enemyRange;
	// speeds
	private float attackMoveSpeed;
	private float speed;
	// Sprites
	private SpriteRenderer spriteR;
    [Serializable]
    public struct NamedImage
    {
        public string name;
        public Sprite image;
    }
    public NamedImage[] pictures;
    private Sprite[,,] runHash = new Sprite[4, 6, 3];

	// Load from pictures in to runhash
    private void loadHash()
    {
		/*
		Fill in Forwrd Dir, Back Dir,
				High stance, Right Stance, Left Stance
				Run
		 */
		
		for (int x = 0; x < 3; x++)
		{
			for(int y = 0; y < 3; y++) {
				runHash[x, Action.Attack.GetHashCode(), y] = pictures[x*11+y].image;
				runHash[x, Action.Idle.GetHashCode(), y] = pictures[x*11+y+3].image;
				runHash[x, Action.RunForward.GetHashCode(), y] = pictures[x*11+y+8].image;
				runHash[x, Action.RunBackward.GetHashCode(), y] = pictures[x*11+y+11].image;
			}
			runHash[x, Action.DashForward.GetHashCode(), 0] = pictures[x*11+6].image;
			runHash[x, Action.DashBackward.GetHashCode(), 0] = pictures[x*11+7].image;
		}

    }
    Animator anim;

	private bool isMoving = false;
	// private bool isDashing = false;
	// private bool isAttacking = false;
	// private bool isSwitching = false;
	// private int tempStance;

	public float timer;
    public int grandmaTimer;
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
					Action.DashBackward;
		stateSpace[Distance.InRange.GetHashCode(), Action.Idle.GetHashCode()] =
					Action.Attack;
		stateSpace[Distance.InRange.GetHashCode(), Action.DashForward.GetHashCode()] =
					Action.DashBackward;
		stateSpace[Distance.InRange.GetHashCode(), Action.DashBackward.GetHashCode()] =
					Action.DashForward;
		stateSpace[Distance.InRange.GetHashCode(), Action.RunForward.GetHashCode()] =
					Action.Attack;
		stateSpace[Distance.InRange.GetHashCode(), Action.RunBackward.GetHashCode()] =
					Action.RunForward;
		// Out of Range
		stateSpace[Distance.OutOfRange.GetHashCode(), Action.Attack.GetHashCode()] =
					Action.Idle;
		stateSpace[Distance.OutOfRange.GetHashCode(), Action.Idle.GetHashCode()] =
					Action.RunForward;
		stateSpace[Distance.OutOfRange.GetHashCode(), Action.DashForward.GetHashCode()] =
					Action.Idle;
		stateSpace[Distance.OutOfRange.GetHashCode(), Action.DashBackward.GetHashCode()] =
					Action.DashForward;
		stateSpace[Distance.OutOfRange.GetHashCode(), Action.RunForward.GetHashCode()] =
					Action.Idle;
		stateSpace[Distance.OutOfRange.GetHashCode(), Action.RunBackward.GetHashCode()] =
					Action.DashForward;
		// Coward
		stateSpace[Distance.Coward.GetHashCode(), Action.Attack.GetHashCode()] =
					Action.DashForward;
		stateSpace[Distance.Coward.GetHashCode(), Action.Idle.GetHashCode()] =
					Action.DashForward;
		stateSpace[Distance.Coward.GetHashCode(), Action.DashForward.GetHashCode()] =
					Action.DashForward;
		stateSpace[Distance.Coward.GetHashCode(), Action.DashBackward.GetHashCode()] =
					Action.DashForward;
		stateSpace[Distance.Coward.GetHashCode(), Action.RunForward.GetHashCode()] =
					Action.DashForward;
		stateSpace[Distance.Coward.GetHashCode(), Action.RunBackward.GetHashCode()] =
					Action.DashForward;
	}

    // Update is called once per frame
    //spriteRenderer
    void Update () {
		timer += Time.deltaTime;

		if(timer >= refreshRate) {
			timer = 0;

			// Judge Range
			var tempRange = JudgeRange();
			
			// get enemy state
			var tempEnemyStance = enemy.stance;
			var tempActionState = GetEnemyActionState();

			// get direction
			if(enemy.gameObject.transform.position.x - transform.position.x < 0) {
				direction = 0;
				spriteR.flipX = true;
			}
			else {
				direction = 1;
				spriteR.flipX = false;
			}
			
			// If nothing changed
			if(tempRange == enemyRange && tempEnemyStance == enemyStance && tempActionState == enemyActionState) {
				return;
			}
			// else change
			else {
				enemyRange = tempRange;
				enemyStance = tempEnemyStance;
				enemyActionState = tempActionState;
			}

			// get next state (w/ noise)
			nextAction = stateSpace[enemyRange, enemyActionState];
			// Noise???
			// Signal for current animation to end
			if(state != nextAction){
				animState = AnimState.End;
			}

			Debug.Log("Range: " + enemyRange);
			Debug.Log("EnemyActionState: " + enemyActionState);
			Debug.Log("Next Action: " + nextAction);
			Debug.Log("Direction: " + direction);

			// change stance with a random chance (if no change in action??)
		}
	}

	//runs at a fixed rate.
	void FixedUpdate()
	{
		// if (isSwitching) {
		// 	stanceTimer += Time.deltaTime;
		// 	if (stanceTimer >= stanceSwitchDelay) {
		// 		stanceTimer = 0;
		// 		isSwitching = false;
		// 	}
		// }
		// else if (stance != tempStance) {
		// 	stance = tempStance;
		// }
		// else if (attacktimer > 0) { // cooldown
		// 	attacktimer += Time.deltaTime;
		// 	if (attacktimer >= attackCooldown) {
		// 		attacktimer = 0;
		// 	}
		// }
		// else if (isAttacking && attacktimer == 0 && stance == tempStance) { // attack
		// 	spriteR.sprite = runHash[stance, Action.Attack.GetHashCode(), attackC/5];
		// 	attackC++;
		// 	if (attackC == 15) {
		// 		attackC = 0;
		// 		isAttacking = false;
		// 		attacktimer = 0.001f;
		// 		speed = baseSpeed;
		// 	}
		// }
		// if (isMoving) {
		// 	if (isDashing) {
		// 		dashtimer += Time.deltaTime;

		// 		if (dashtimer <= dashLength) { 	//while within the threshold, animation will run.
		// 			spriteR.sprite = runHash[stance, Action.DashForward.GetHashCode(), 0];
		// 			transform.Translate ((direction * (-2) + 1) * Time.deltaTime * speed * 5f, 0, 0);
		// 		} else if (dashtimer >= dashLength + dashCooldown2) {
		// 			Running ();
		// 			transform.Translate ((direction * (-2) + 1) * Time.deltaTime * speed, 0, 0);
		// 			//SetAnims ("isMovingRight", true);
		// 		} else {
		// 			spriteR.sprite = runHash[stance, Action.DashForward.GetHashCode(), 0];
		// 			transform.Translate ((direction * (-2) + 1) * Time.deltaTime * speed, 0, 0);
		// 		}
		// 		if (dashtimer >= dashLength + dashCooldown) {
		// 			isDashing = false;
		// 			dashtimer = 0;
		// 		}
		// 	} else {
		// 		Running ();
		// 		transform.Translate ((direction * (-2) + 1) * Time.deltaTime * speed, 0, 0);
		// 		//SetAnims ("isMovingRight", true);
		// 	}
		// } else {
		// 	//SetAnims ("isMovingRight", false);
		// 	if(dashtimer > 0) {
		// 		dashtimer += Time.deltaTime;
		// 		if (dashtimer >= dashLength + dashCooldown) {
		// 			dashtimer = 0;
		// 		}
		// 	}
		// 	Idle();
		// }

		if(state == Action.Attack) { // Attack
			if(animState == AnimState.Start) { // Start Anim
				// enable attackbox
				animState = AnimState.Playing;
				attacktimer = 0;
				attackC = 0;
				spriteR.sprite = runHash[stance, Action.Attack.GetHashCode(), 0];
			}
			if(animState == AnimState.Playing) { // Anim playing
				if (attacktimer > 0) { // cooldown
					attacktimer += Time.deltaTime;
					if (attacktimer >= attackCooldown) {
						attacktimer = 0;
					}
				}
				else if (attacktimer == 0) { // attack
					spriteR.sprite = runHash[stance, Action.Attack.GetHashCode(), attackC/5];
					attackC++;
					if (attackC == 15) {
						attackC = 0;
						attacktimer = 0.001f;
					}
				}
			}
			if(animState == AnimState.End) { // Anim ending
				// disable attackbox
				state = nextAction;
				animState = AnimState.Start;
			}
		}
		if(state == Action.Idle) {
			if(animState == AnimState.Start) {
				animState = AnimState.Playing;
				timer = 0;
			}
			if(animState == AnimState.Playing) {
				spriteR.sprite = runHash[stance, Action.Idle.GetHashCode(), (int)timer/5];
				timer += 1;
				timer %= 15;
			}
			if(animState == AnimState.End) {
				state = nextAction;
				animState = AnimState.Start;
			}
		}
		if(state == Action.DashForward) {
			if(animState == AnimState.Start) {
				animState = AnimState.Playing;
				dashtimer = 0;
			}
			if(animState == AnimState.Playing) {
				dashtimer += Time.deltaTime;

				if (dashtimer <= dashLength) { 	//while within the threshold, animation will run.
					spriteR.sprite = runHash[stance, Action.DashForward.GetHashCode(), 0];
					transform.Translate ((direction * (2) - 1) * Time.deltaTime * speed * 5f, 0, 0);
				} else if (dashtimer >= dashLength + dashCooldown2) {
					// Running ();
					spriteR.sprite = runHash[stance, Action.RunForward.GetHashCode(), (int)timer/5];
					transform.Translate ((direction * (2) - 1) * Time.deltaTime * speed, 0, 0);
					grandmaTimer += 1;
					grandmaTimer %= 15;
				} else {
					spriteR.sprite = runHash[stance, Action.DashForward.GetHashCode(), 0];
					transform.Translate ((direction * (2) - 1) * Time.deltaTime * speed, 0, 0);
				}
				if (dashtimer >= dashLength + dashCooldown) {
					dashtimer = 0;
				}
			}
			if(animState == AnimState.End) {
                if (dashtimer <= dashLength)
                {
                    spriteR.sprite = runHash[stance, Action.DashForward.GetHashCode(), 0];
                    transform.Translate((direction * (2) - 1) * Time.deltaTime * speed * 5f, 0, 0);
                    dashtimer += Time.deltaTime;
                }
                else {
                    state = nextAction;
                    animState = AnimState.Start;
                }
			}
		}
		if(state == Action.DashBackward) {
			if(animState == AnimState.Start) {
				animState = AnimState.Playing;
				dashtimer = 0;
			}
			if(animState == AnimState.Playing) {
				dashtimer += Time.deltaTime;

				if (dashtimer <= dashLength) { 	//while within the threshold, animation will run.
					spriteR.sprite = runHash[stance, Action.DashBackward.GetHashCode(), 0];
					transform.Translate ((direction * (-2) + 1) * Time.deltaTime * speed * 5f, 0, 0);
				} else if (dashtimer >= dashLength + dashCooldown2) {
					// Running ();
					spriteR.sprite = runHash[stance, Action.RunBackward.GetHashCode(), (int)timer/5];
					transform.Translate ((direction * (-2) + 1) * Time.deltaTime * speed, 0, 0);
					timer += 1;
					timer %= 15;
				} else {
					spriteR.sprite = runHash[stance, Action.DashBackward.GetHashCode(), 0];
					transform.Translate ((direction * (-2) + 1) * Time.deltaTime * speed, 0, 0);
				}
				if (dashtimer >= dashLength + dashCooldown) {
					dashtimer = 0;
				}
			}
			if(animState == AnimState.End) {
				state = nextAction;
				animState = AnimState.Start;
			}
		}
		if(state == Action.RunForward) {
			if(animState == AnimState.Start) {
				animState = AnimState.Playing;
                grandmaTimer = 0;
			}
			if(animState == AnimState.Playing) {
				spriteR.sprite = runHash[stance, Action.RunForward.GetHashCode(), (int)grandmaTimer/ 5];
				transform.Translate ((direction * (2) - 1) * Time.deltaTime * speed, 0, 0);
                grandmaTimer += 1;
                grandmaTimer %= 15;
			}
			if(animState == AnimState.End) {
                    state = nextAction;
                    animState = AnimState.Start;
			}
		}
		if(state == Action.RunBackward) {
			if(animState == AnimState.Start) {
				animState = AnimState.Playing;
                grandmaTimer = 0;
			}
			if(animState == AnimState.Playing) {
				spriteR.sprite = runHash[stance, Action.RunForward.GetHashCode(), (int)grandmaTimer/ 5];
				transform.Translate ((direction * (- 2) + 1) * Time.deltaTime * speed, 0, 0);
                grandmaTimer += 1;
                grandmaTimer %= 15;
			}
			if(animState == AnimState.End) {
                state = nextAction;
				animState = AnimState.Start;
			}
		}

	}

	// back = 0
	// forward = 1
	// stances: left, up, right, down
	//          0,    1,  2,     3
	// if backward:
	// 0 = 2, 2 = 0, 1 = 1, 3 = 3
	// : stance - 2) * -1) + 4) % 4
	// = (4 - (stance - 2)) % 4
	// if forward:
	// original
	// = ?
	// what if ignore down?
	// if backward:
	// stance - 2) * -1
	// = 2 - stance
	// if forward:
	// original
	// = ?

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
		// spriteR.sprite = runHash[stance, Action.RunForward.GetHashCode(), running / 5];
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
			spriteR.sprite = runHash [stance, Action.Idle.GetHashCode(), idleC / 9];
		} 
		idleC++;
	}
}
