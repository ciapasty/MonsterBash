using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedColliderOff : MonoBehaviour {

	public float time = 1f;
	float timer = 0;
	// Use this for initialization
	void Start () {
		timer = time;
	}
	
	// Update is called once per frame
	void Update () {
		if (timer < 0) {
			GetComponent<Rigidbody2D>().simulated = false;
			GetComponent<BoxCollider2D>().enabled = false;
		} else {
			timer -= Time.deltaTime;
		}
	}
}
