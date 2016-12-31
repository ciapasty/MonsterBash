using UnityEngine;
using System.Collections;

public class TimedDestroy : MonoBehaviour {

	public float timer = 15f;
	public bool onlyOutsideOfView = false;

	private bool isInView = false;
	
	// Update is called once per frame
	void Update () {
		Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
		isInView = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

		if (timer < 0) {
			Destroy(gameObject);
		}

		if (!onlyOutsideOfView) {
			timer -= Time.deltaTime;
		} else {
			if (!isInView) {
				timer -= Time.deltaTime;
			}
		}
	}
}
