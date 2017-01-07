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

		clampMovement();
	}

	void clampMovement() {
		Vector2 maxXY = Camera.main.ViewportToWorldPoint(new Vector2(1, 1));
		maxXY.x = maxXY.x-GetComponent<SpriteRenderer>().bounds.extents.x;
		maxXY.y = maxXY.y-GetComponent<SpriteRenderer>().bounds.extents.y*2;
		Vector2 minXY = Camera.main.ViewportToWorldPoint(new Vector2(0, 0));
		minXY.x = minXY.x+GetComponent<SpriteRenderer>().bounds.extents.x;
		Vector3 pos = transform.position;

		if ((pos.x >= maxXY.x && rigidbod.velocity.x > 0) || (pos.x <= minXY.x && rigidbod.velocity.x < 0)) {
			rigidbod.velocity = new Vector2(0, rigidbod.velocity.y);
		}
		if ((pos.y >= maxXY.y && rigidbod.velocity.y > 0) || (pos.y <= minXY.y && rigidbod.velocity.y < 0)) {
			rigidbod.velocity = new Vector2(rigidbod.velocity.x, 0);
		}
	}

	// TODO: Change this, to be more universal. May ignore bonfire totally.
	Vector3 getNewDestination() {
		destination = new Vector3(transform.position.x+Random.Range(-3f, 3f), transform.position.y+Random.Range(-3f,3f), 0);
		return (Vector3.Distance(destination, bonfire.transform.position) < 2f) ? getNewDestination() : destination;
	}
}
