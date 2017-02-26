using UnityEngine;
using System.Collections;

public class MoveTowardsTarget : MonoBehaviour {
	public float speed = 2f;

	public float minDistance = 0.2f;
	public float maxDistance = 2.0f;
	public GameObject target;

	public float neighboursRadius = 1.0f;
	public float minEnemyDistance = 0.2f;

	Rigidbody2D r;
	SpriteRenderer sr;

	void Start() {
		r = GetComponent<Rigidbody2D>();
		sr = GetComponent<SpriteRenderer>();

		target = GameObject.FindGameObjectWithTag("Player");
		//waypoint = target.transform.position;
	}

	void Update () {
		Vector2 direction2Target = target.transform.position-transform.position;
		float distance2Target = Vector3.Distance(target.transform.position, transform.position);
		Vector2 velocity = Vector2.zero;

		if (distance2Target > maxDistance) {
			velocity = direction2Target;
		}
		if (distance2Target < maxDistance && distance2Target > minDistance+minDistance*0.5f) {
			Vector2 dir = new Vector2(direction2Target.x*0.3f, direction2Target.y);
			velocity = dir;
		}
		if (distance2Target < minDistance+minDistance*0.5f && distance2Target > minDistance) {
			// do nothing
		}
		if (distance2Target < minDistance) {
			velocity = -direction2Target;
		}

		Vector2 v = Vector2.zero;
		Collider2D[] hitColliders = Physics2D.OverlapCircleAll(GetComponent<Renderer>().bounds.center, neighboursRadius);
		foreach (var collider in hitColliders) {
			if (collider.gameObject.tag == "Enemy") {
				if (collider.gameObject != gameObject) {
					int count = 0;
					float d = Vector2.Distance(collider.transform.position, transform.position);
					if (d > 0f && d < minEnemyDistance) {
						v = (collider.transform.position-transform.position).normalized/d;
						count++;
					}
					if (count > 0) {
						v /= count;
					}
				}
			}
		}

		r.velocity = (velocity.normalized-v.normalized).normalized*speed;

		sr.flipX = (direction2Target.x < 0);

		//Debug.Log(r.velocity);
	}
}
