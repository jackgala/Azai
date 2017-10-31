using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	Animator anim;
	private bool slashRight; //used to flag animation bool for SlashRight
	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();
		slashRight = false; //init.
	}
	
	// Update is called once per frame
	void Update () {

			
	}

	void FixedUpdate() 
	{
		if (Input.GetKeyDown(KeyCode.J)) //sets appropriate booleans and animations while the key is pressed (returns true the first frame key is pressed DOWN).
		{
//			Debug.Log ("J was pressed!");	//DEBUGGING.
			slashRight = true;
			SetAnims ("SlashRight", slashRight); 
		}
		if (Input.GetKeyUp(KeyCode.J)) //sets appropriate booleans and animations once key is released (returns true the first frame key is NOT pressed down).
		{
//			Debug.Log ("J was released!");	//DEBUGGING.
			slashRight = false;
			SetAnims ("SlashRight", slashRight); 
		}
		if (Input.GetAxis ("Horizontal") != 0)
			anim.SetBool ("isMoving", true);
		else if (Input.GetAxis ("Horizontal") == 0){
			anim.SetBool ("isMoving", false);
		}
	}

	public void SetStanceState(string state){
		anim.SetBool ("StanceLeft", false);
		anim.SetBool ("StanceRight", false);
		anim.SetBool ("StanceUp", false);
		anim.SetBool (state, true);
	}

	void SetAnims(string animation, bool boolean)
	{
		if (boolean) {
			anim.SetBool (animation, boolean);
			boolean = false; //fail-safe, in the event boolean value is not reset by getKeyUp (usually occurs when key is released mid-frame).
		} else {
			anim.SetBool (animation, boolean);

		}
	}
}
