using UnityEngine;
using System.Collections;

public class MoveIdle : MonoBehaviour {

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
			rigidbod.velocity = direction.normalized*speed;
		}
	}

	// TODO: Change this, to be more universal. May ignore bonfire totally.
	Vector3 getNewDestination() {
		destination = new Vector3(transform.position.x+Random.Range(-3f, 3f), transform.position.y+Random.Range(-3f,3f), 0);
		return (Vector3.Distance(destination, bonfire.transform.position) < 2f) ? getNewDestination() : destination;
	}
}
