using UnityEngine;
using System.Collections;

public class DestroyMe : MonoBehaviour {

	public float time = -1f;
	public bool destroy = true;
	public bool onlyOutsideOfView = false;

	private bool isInView = false;
	
	// Update is called once per frame
	void Update () {
		Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
		isInView = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

		if (time < 0 && time > -1) {
			if (destroy) {
				destroyMe();
			} else {
				killMe();
			}

		}

		if (!onlyOutsideOfView) {
			time -= Time.deltaTime;
		} else {
			if (!isInView) {
				time -= Time.deltaTime;
			}
		}
	}

	void killMe() {
		GetComponent<Animator>().SetTrigger("deathTrigger");
	}

	void destroyMe() {
		Destroy(gameObject);
	}
}
