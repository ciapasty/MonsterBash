using UnityEngine;
using System.Collections;

public class TimedDestroy : MonoBehaviour {

	public float timer = 15f;
	
	// Update is called once per frame
	void Update () {
		if (timer < 0) {
			Destroy(gameObject);
		}

		timer -= Time.deltaTime;
	}
}
