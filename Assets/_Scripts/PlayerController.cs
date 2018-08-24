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
    //run hash = 2D array for stances and running: [stance][type of running] 
    public float dashCooldown = 0.6f;
	public float dashCooldown2 = 0.3f;
	public float dashLength = 0.05f;
	public float speed = 20f;
	// public Sprite[] runningForwardSheet;
	// public Sprite[] runningForwardRightStance;
	// public Sprite[] runningForwardUpStance;
	// public Sprite[] runningBackwardRightStance;
	// public Sprite[] runningBackwardUpStance;
	// public Sprite[] runningBackwardSheet;
	public Sprite[] idle;
	//public Sprite[] attackF;
	public Sprite[] stances;
	public Sprite[] dashing;
	public int direction;
	private int running = 0;
	//private int attack = 0;
	private int idleC = 0;
	private int stance = 0;
	private SpriteRenderer spriteR;
    [Serializable]
    public struct NamedImage
    {
        public string name;
        public Sprite image;
    }
    public NamedImage[] pictures;
    private Sprite[,,,] runHash = new Sprite[2, 4, 3, 3];
	private enum Direction {Forward, Backward};
	private enum Stance {Left, High, Right, Low};
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

		for (int x = 0; x < 18; x++)
		{
			int dir = (x%6)/3;
			int stance = x/6;
			int frame = x%3;
			
			runHash[dir, stance, 1, frame] = pictures[x].image;
		}

		for(int x = 0; x < 4; x++){
			runHash[0, x, 2, 0] = dashing[0];
			runHash[1, x, 2, 0] = dashing[1];
		}

    }
    Animator anim;

	private bool isMoving;
	private bool isDashing;

	private float timer;
    // Use this for initialization
    void Start () {
		anim = GetComponent<Animator> ();
		isMoving = false;
		spriteR = gameObject.GetComponent<SpriteRenderer>();
		loadHash();
	}
	
	// Update is called once per frame
	//spriteRenderer
	void Update () {
		if (Input.GetKey(KeyCode.Semicolon)) {
			isDashing = true;
		}
		if (Input.GetKey(KeyCode.J)) {
			stance = Stance.Left.GetHashCode();
		}
		if (Input.GetKey(KeyCode.K)) {
			stance = Stance.Low.GetHashCode();
		}
		if (Input.GetKey(KeyCode.I)) {
			stance = Stance.High.GetHashCode();
		}
		if (Input.GetKey(KeyCode.L)) {
			stance = Stance.Right.GetHashCode();
		}
		if (Input.GetKey (KeyCode.D)) {
			isMoving = true;
			direction = Direction.Forward.GetHashCode();
		} else if (Input.GetKey (KeyCode.A)) {
			isMoving = true;
			direction = Direction.Backward.GetHashCode();
		}  
		else {
			isMoving = false;
		}
	}

	//runs at a fixed rate.
	void FixedUpdate()
	{
		if (isMoving) {
			if (isDashing) {
				timer += Time.deltaTime;

				if (timer <= dashLength) { 	//while within the threshold, animation will run.
					spriteR.sprite = runHash[direction, stance, 2, 0];
					transform.Translate ((direction * (-2) + 1) * Time.deltaTime * speed * 5f, 0, 0);
				} else if (timer >= dashLength + dashCooldown2) {
					Running ();
					transform.Translate ((direction * (-2) + 1) * Time.deltaTime * speed, 0, 0);
					//SetAnims ("isMovingRight", true);
				} else {
					spriteR.sprite = runHash[direction, stance, 2, 0];
					transform.Translate ((direction * (-2) + 1) * Time.deltaTime * speed, 0, 0);
				}
				if (timer >= dashLength + dashCooldown) {
					isDashing = false;
					timer = 0;
				}
			} else {
				Running ();
				transform.Translate ((direction * (-2) + 1) * Time.deltaTime * speed, 0, 0);
				//SetAnims ("isMovingRight", true);
			}
		} else {
			//SetAnims ("isMovingRight", false);
			if(isDashing) {
				isDashing = false;
				timer = 0;
			}
			Idle();
		}
	}


	void Running(){
		if (running == 15) {
			running = 0;
		}
		spriteR.sprite = runHash[direction, stance, 1, running / 5];
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
	void StanceM(int counter){
		spriteR.sprite = stances [stance];
	}
	void Idle(){
		if (idleC >= 36) {
			idleC = 0;
		}
		if (idleC % 9 == 0) {
			//runningForwardSheet [counter];
			spriteR.sprite = idle [idleC / 9];
		} 
		idleC++;
	}
	void SetAnims(string paramName, bool boolean)
	{
		if (boolean) {
			anim.SetBool (paramName, boolean);
			boolean = false; //fail-safe, in the event boolean value is not reset by getKeyUp (usually occurs when key is released mid-frame).
		} else {
			anim.SetBool (paramName, boolean);

		}

	}
}
