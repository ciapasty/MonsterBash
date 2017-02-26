using UnityEngine;
using System.Collections;

public class MoveInDirection : MonoBehaviour {

	public float speed = 2;
	public Vector3 direction;

	void FixedUpdate () {
		GetComponent<Rigidbody2D>().velocity = direction.normalized*speed;
	}
}
