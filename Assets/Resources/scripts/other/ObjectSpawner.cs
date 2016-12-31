using UnityEngine;
using System.Collections;

public class ObjectSpawner : MonoBehaviour {

	public GameObject prefab;
	public float spawnRate = 2f;
	public float delay = 4f;
	public bool spawnInsideView = false;
	public float minimumDistanceFromPlayer = 2f;

	void Update () {
		delay -= Time.deltaTime;

		if (delay <= 0) {
			GameObject go = (GameObject)Instantiate(prefab, getSpawnPosition(), Quaternion.identity);
			go.transform.SetParent(gameObject.transform);
			delay = (10*Random.value)/spawnRate;
		}
	}

	Vector3 getSpawnPosition() {
		Vector3 spawnPos = new Vector3();
		if (spawnInsideView) {
			spawnPos = randomCoordsInsideCameraView();
		} else {
			spawnPos = randomCoordsOutsideCameraView();
		}
		if (Vector3.Distance(spawnPos, GameObject.FindGameObjectWithTag("Player").transform.position) < minimumDistanceFromPlayer) {
			return getSpawnPosition();
		} else {
			return spawnPos;
		}
	}

	Vector3 randomCoordsOutsideCameraView() {
		float xCoord = Random.Range(-40, 140)/100f;
		float yCoord = 0;
		if (xCoord < -0.1f || xCoord > 1.1f) {
			yCoord = Random.value;
		} else {
			yCoord = (Random.Range(-40, 40)/100f);
			if (yCoord > 0) {
				yCoord += 1.1f;
			} else {
				yCoord -= 0.1f;
			}
		}
		return Camera.main.ViewportToWorldPoint(new Vector3(xCoord, yCoord, 10.0f));
	}

	Vector3 randomCoordsInsideCameraView() {
		return Camera.main.ViewportToWorldPoint(new Vector3(Random.value, Random.value, 10.0f));
	}
}
