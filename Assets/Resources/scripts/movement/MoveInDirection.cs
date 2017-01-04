using UnityEngine;
using System.Collections;

public class MoveInDirection : MonoBehaviour {

	public float moveForce = 10f;
	public float speed = 2;
	public Vector3 direction;

	void FixedUpdate () {
		GetComponent<Rigidbody2D>().AddForce(direction/direction.magnitude*speed*moveForce);
	}
}
