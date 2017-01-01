using UnityEngine;
using System.Collections;

public class MoveTowardsPlayer : MonoBehaviour {

	public float moveForce = 10f;
	public float speed = 2;

	private Rigidbody2D rigidbod;
	private GameObject player;

	void Start () {
		rigidbod = GetComponent<Rigidbody2D>();

		player = GameObject.FindGameObjectWithTag("Player");
	}

	void Update () {}

	void FixedUpdate () {
		// Dumb movement
		Vector3 direction = (player.transform.position - transform.position);
		rigidbod.AddForce(direction/direction.magnitude*speed*moveForce);
	}
}
