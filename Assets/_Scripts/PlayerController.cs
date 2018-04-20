using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public float dashCooldown;
	private float dashLength;
	public float speed = 20f;
	public Sprite[] runningForwardSheet;
	public Sprite[] runningBackwardSheet;
	public Sprite[] idle;
	public int direction;
	private int running = 0;
	private int attack = 0;
	private int idleC = 0;
	private SpriteRenderer spriteR;


	Animator anim;

	private bool isMoving;
	private bool isDashing;

	private float timer;
	// Use this for initialization
	void Start () {
		dashLength = 0.05f;
		dashCooldown = .6f;
		anim = GetComponent<Animator> ();
		isMoving = false;
		spriteR = gameObject.GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	//spriteRenderer
	void Update () {
		if (Input.GetKey(KeyCode.Semicolon)) {
			isDashing = true;
		}
		if (Input.GetKey (KeyCode.D)) {
			isMoving = true;
			direction = 1;
		} else if (Input.GetKey (KeyCode.A)) {
			isMoving = true;
			direction = -1;
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
					transform.Translate (direction * Time.deltaTime * speed * 5f, 0, 0);
				} 
				if (timer >= dashLength + dashCooldown) {
					isDashing = false;
					timer = 0;
				}
			} else {
				Running (running);
				transform.Translate (direction * Time.deltaTime * speed, 0, 0);
				//SetAnims ("isMovingRight", true);
			}
		} else {
			//SetAnims ("isMovingRight", false);
			Idle(idleC);
		}
	}


	void Running(int counter){
		if (counter == 15) {
			running = 0;
		}
		if (direction == 1 && counter % 5 == 0) {
			//runningForwardSheet [counter];
			spriteR.sprite = runningForwardSheet [counter/5];
		} else if (direction == -1&& counter % 5 == 0) {
			//	runningBackwardSheet [counter];
			spriteR.sprite = runningBackwardSheet [counter/5];
		}
		running++;

	}
	void Attack(int counter){
		//if(attack bool triggered by typing in J)
	}
	void Idle(int counter){
		if (counter >= 36) {
			idleC = 0;
		}
		if (counter % 9 == 0) {
			//runningForwardSheet [counter];
			spriteR.sprite = idle [counter / 9];
		} 
		idleC++;
	}
	void SetAnims(string paramName, bool boolean)
	{
		if (boolean) {
			anim.SetBool (paramName, boolean);
		//	boolean = false; //fail-safe, in the event boolean value is not reset by getKeyUp (usually occurs when key is released mid-frame).
		} else {
			anim.SetBool (paramName, boolean);

		}

	}
}
