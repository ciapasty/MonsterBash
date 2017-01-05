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
	}

	Vector3 getWaypointBetweenMinMax() {
		Vector3 wp = new Vector3(transform.position.x+Random.Range(-1f, 1f), transform.position.y+Random.Range(-1f,1f), 0);
		float distance = Vector3.Distance(wp, target.transform.position);
		return ((distance < keepMaxDistance) && (distance > keepMinDistance)) ? wp : getWaypointBetweenMinMax();
	}
}
