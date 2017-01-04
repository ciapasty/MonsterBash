using UnityEngine;
using System.Collections;

public class MoveTowardsPlayer : MonoBehaviour {

	public float moveForce = 10f;
	public float speed = 2;

	private Rigidbody2D rigidbod;
	private GameObject player;

	void Start () {
		player = GameObject.FindGameObjectWithTag("Player");
	}

	void FixedUpdate () {
		// Dumb movement
		Vector3 direction = (player.transform.position - transform.position);
		GetComponent<Rigidbody2D>().AddForce(direction/direction.magnitude*speed*moveForce);
	}
}
