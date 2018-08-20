using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    //Implement values into 
    //Set up number system with stance
    //run hash = 2D array for stances and running: [stance][type of running] 
    public float dashCooldown;
	private float dashLength;
	public float speed = 20f;
	public Sprite[] runningForwardSheet;
	public Sprite[] runningForwardRightStance;
	public Sprite[] runningForwardUpStance;
	public Sprite[] runningBackwardRightStance;
	public Sprite[] runningBackwardUpStance;
	public Sprite[] runningBackwardSheet;
	public Sprite[] idle;
	public Sprite[] attackF;
	public Sprite[] stances;
	public int direction;
	private int running = 0;
	private int attack = 0;
	private int idleC = 0;
	private int stance = 0;
	private SpriteRenderer spriteR;
    [Serializable]
    public struct NamedImage
    {
        public string name;
        public Sprite image;

        public NamedImage(String n, Sprite i) {
            name = n;
            image = i;
        }
    }
    public NamedImage[] pictures;
    private NamedImage[][] runArray = new NamedImage[4][];
    int loadingStance = 0;
    private void loadHash()
    {
      
        for (int x = 0; x < 6; x++)
        {
            loadingStance += x;
            runArray[0][x] = new NamedImage(pictures[loadingStance].name, pictures[loadingStance].image);
        }
        for (int x = 0; x < 6; x++)
        {
            loadingStance += x;
            runArray[1][x] = new NamedImage(pictures[loadingStance].name, pictures[loadingStance].image);
        }
        for (int x = 0; x < 6; x++)
        {
            loadingStance += x;
            runArray[2][x] = new NamedImage(pictures[loadingStance].name, pictures[loadingStance].image);
        }


    }
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
		if (Input.GetKey(KeyCode.J)) {
			stance = 0;
		}
		if (Input.GetKey(KeyCode.K)) {
			stance = 1;
		}
		if (Input.GetKey(KeyCode.I)) {
			stance = 2;
		}
		if (Input.GetKey(KeyCode.L)) {
			stance = 3;
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
				SetAnims ("isMovingRight", true);
			}
		} else {
			SetAnims ("isMovingRight", false);
			Idle(idleC);
		}
	}


	void Running(int counter){
		if (counter == 15) {
			running = 0;
		}
		if (direction == 1 && counter % 5 == 0) {
            if (counter == 15) {
                counter = 0;
            }
            spriteR.sprite = (Sprite)runArray[stance][counter/5].image.;

		} else if (direction == -1&& counter % 5 == 0) {
			//	runningBackwardSheet [counter];
			if (stance == 3) {
                if (counter == 31)
                {
                    counter = 1;
                }
                spriteR.sprite = (Sprite)runningBackwardRightStance [(counter / 5) + 3];
			}
		}
		running++;

	}
	void Attack(int counter){
		//if(attack bool triggered by typing in J)
	}
	void StanceM(int counter){
		spriteR.sprite = stances [stance];
	}
	void Idle(int counter){
		if (counter >= 36) {
			idleC = 0;
		}
		if (counter % 9 == 0) {
			//runningForwardSheet [counter];
			//spriteR.sprite = idle [counter / 9];
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
