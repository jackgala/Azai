using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	Animator anim;
	private bool slashRight; //used to flag animation bool for SlashRight
	private bool isMovingR; //used to flag animation bool for moving right.
	private bool isMovingL;
	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();
		slashRight = false; //init.
		isMovingR = false;
		isMovingL = false;
	}
	
	// Update is called once per frame
	void Update () {

			
	}

	void FixedUpdate() 
	{
		if (Input.GetKey(KeyCode.J)) { //sets appropriate booleans and animations while the key is pressed (returns true the first frame key is pressed DOWN).
//			Debug.Log ("J was pressed!");	//DEBUGGING.
			slashRight = true;
			SetAnims ("SlashRight", slashRight); 
		}
		else if(slashRight)// says that when the key is not down, but slashRight is still true, the key has been released.
						  // Hopes to elliminate bug when GetKeyUp does not detect. 
		{
			slashRight = false;
			SetAnims ("SlashRight", slashRight);
		}
		if (Input.GetKeyUp(KeyCode.J)) //sets appropriate booleans and animations once key is released (returns true the first frame key is NOT pressed down).
		{
//			Debug.Log ("J was released!");	//DEBUGGING.
			slashRight = false;
			SetAnims ("SlashRight", slashRight); 
		}

		if (Input.GetKey (KeyCode.D) && !isMovingL) {
			isMovingR = true;
			SetAnims ("isMovingRight", isMovingR);
			transform.Translate (Time.deltaTime * 20f, 0, 0);
//			Debug.Log ("isMoving is: "+isMoving); //DEBUGGING.
		} 
		else if(isMovingR)
		{
			isMovingR = false;
			SetAnims ("isMovingRight", isMovingR);
		}
		if (Input.GetKeyUp (KeyCode.D)) {
			isMovingR = false;
			SetAnims ("isMovingRight", isMovingR);
		}

		if (Input.GetKey (KeyCode.A) && !isMovingR) {
			isMovingL = true;
			SetAnims ("isMovingLeft", isMovingL);
			transform.Translate (Time.deltaTime * -20f, 0, 0);
		} 
		else if (isMovingL)
		{
			isMovingL = false;
			SetAnims ("isMovingLeft", isMovingL);
		}
		if (Input.GetKeyUp (KeyCode.A)) {
			isMovingL = false;
			SetAnims ("isMovingLeft", isMovingL);
		}

	}

	public void SetStanceState(string state){
		anim.SetBool ("StanceUp", false);
		anim.SetBool ("StanceLeft", false);
		anim.SetBool ("StanceRight", false);
		anim.SetBool ("StanceCenter", false);
		anim.SetBool (state, true);
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
