using UnityEngine;
using System.Collections;

public class MoveIdle : MonoBehaviour {

	public float moveForce = 10f;
	public float speed = 1;

	private Rigidbody2D rigidbod;
	private GameObject bonfire;

	private Vector3 destination;
	private float newDestinationTimer = -1f;


	void Start () {
		rigidbod = GetComponent<Rigidbody2D>();
		bonfire = GameObject.FindGameObjectWithTag("Bonfire");
	}

	void Update () {
		if (newDestinationTimer < 0) {
			getNewDestination();
			newDestinationTimer = 2f + Random.value;
		}
		newDestinationTimer -= Time.deltaTime;
	}

	void FixedUpdate () {
		if (Vector3.Distance(destination, transform.position) > 0.2) {
			Vector3 direction = (destination-transform.position);
			rigidbod.AddForce(direction/direction.magnitude*speed*moveForce);
		}
	}

	Vector3 getNewDestination() {
		destination = new Vector3(transform.position.x+Random.Range(-3f, 3f), transform.position.y+Random.Range(-3f,3f), 0);
		return (Vector3.Distance(destination, bonfire.transform.position) < 2f) ? getNewDestination() : destination;
	}
}
