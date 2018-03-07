using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public float dashCooldown;
	private float dashLength;
	public float speed = 20f;
	public int direction;

	Animator anim;

	private bool isMoving;
	private bool isDashing;

	private float timer;
	// Use this for initialization
	void Start () {
		dashLength = 0.1f;
		dashCooldown = .2f;
		anim = GetComponent<Animator> ();
		isMoving = false;
	}
	
	// Update is called once per frame
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
		if (isMoving) 
		{
			if (isDashing) 
			{
				timer += Time.deltaTime;

				if (timer <= dashLength) { 	//while within the threshold, animation will run.
					transform.Translate (direction * Time.deltaTime * speed * 5f, 0, 0);
				} 
				if (timer >= dashLength + dashCooldown)
				{
					isDashing = false;
					timer = 0;
				}
			} 
			else 
			{
				transform.Translate (direction * Time.deltaTime * speed, 0, 0);
			}
		}
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
