using UnityEngine;
using System.Collections;

public class MoveTowardsTarget : MonoBehaviour {
	public float speed = 2;

	public float keepMaxDistance = 0f;
	public float keepMinDistance = 0f;

	private Rigidbody2D rigidbod;
	private GameObject target;

	private Vector3 waypoint;
	private float newWaypointTimer = 0f;

	void Start () {
		rigidbod = GetComponent<Rigidbody2D>();

		target = GameObject.FindGameObjectWithTag("Player");
		waypoint = target.transform.position;
	}

	void FixedUpdate () {
		float distance = Vector3.Distance(target.transform.position, transform.position);
		if (distance > keepMaxDistance ) {
			waypoint = target.transform.position;
		} else if ((distance < keepMaxDistance) && (distance > keepMinDistance)) {
			if (newWaypointTimer <= 0) {
				waypoint = getWaypointBetweenMinMax();
				newWaypointTimer = 1f + Random.value;
			}
			newWaypointTimer -= Time.deltaTime;
		} 

		Vector3 direction = (waypoint - transform.position);
		if (distance < keepMinDistance) {
			direction = -(target.transform.position - transform.position);
		}

		GetComponent<Rigidbody2D>().velocity = direction.normalized*speed;

		clampMovement();
	}

	void clampMovement() {
		Vector2 maxXY = GameObject.FindGameObjectWithTag("PlayerCamera").GetComponent<Camera>().ViewportToWorldPoint(new Vector2(1, 1));
		maxXY.x = maxXY.x-GetComponent<SpriteRenderer>().bounds.extents.x;
		maxXY.y = maxXY.y-GetComponent<SpriteRenderer>().bounds.extents.y*2;
		Vector2 minXY = GameObject.FindGameObjectWithTag("PlayerCamera").GetComponent<Camera>().ViewportToWorldPoint(new Vector2(0, 0));
		minXY.x = minXY.x+GetComponent<SpriteRenderer>().bounds.extents.x;
		Vector3 pos = transform.position;

		if ((pos.x >= maxXY.x && rigidbod.velocity.x > 0) || (pos.x <= minXY.x && rigidbod.velocity.x < 0)) {
			rigidbod.velocity = new Vector2(0, rigidbod.velocity.y);
		}
		if ((pos.y >= maxXY.y && rigidbod.velocity.y > 0) || (pos.y <= minXY.y && rigidbod.velocity.y < 0)) {
			rigidbod.velocity = new Vector2(rigidbod.velocity.x, 0);
		}
	}

	Vector3 getWaypointBetweenMinMax() {
		Vector3 wp = new Vector3(transform.position.x+Random.Range(-1f, 1f), transform.position.y+Random.Range(-1f,1f), 0);
		float distance = Vector3.Distance(wp, target.transform.position);
		return ((distance < keepMaxDistance) && (distance > keepMinDistance)) ? wp : getWaypointBetweenMinMax();
	}
}
