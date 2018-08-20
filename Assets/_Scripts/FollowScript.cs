using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowScript : MonoBehaviour {

	public Transform target;

	private Vector3 offset;

	// Use this for initialization
	void Start () {
		offset = transform.position - target.position;
	}
	
	// Update is called once per frame
	void Update () {

		Vector3 dest = target.position + offset;
		transform.position = Vector3.Lerp(transform.position, target.position, 5.0f * Time.deltaTime);
	}
}
