using UnityEngine;
using System.Collections;

public class ObjectSpawner : MonoBehaviour {

	public GameObject[] prefabs;
	public float spawnRate = 2f;
	public float delay = 4f;
	public bool spawnInsideView = false;
	public bool spawnOnlyOnRight = false;
	public float minimumDistanceFromPlayer = 2f;

	private Camera playerCamera;
	private Vector3 currCameraPos;
	private Vector3 prevCameraPos;

	void Start() {
		playerCamera = GameObject.FindGameObjectWithTag("PlayerCamera").GetComponent<Camera>();
		currCameraPos = GameObject.FindGameObjectWithTag("PlayerCamera").transform.position;
		prevCameraPos = currCameraPos;
	}

	void Update () {
		if (delay > 0) {
			delay -= Time.deltaTime;
		} else {
			currCameraPos = playerCamera.transform.position;

			if (Vector3.Distance(currCameraPos, prevCameraPos) > 0) {
				GameObject go = (GameObject)Instantiate(prefabs[Random.Range(0, prefabs.Length)], getSpawnPosition(), Quaternion.identity);
				go.transform.SetParent(gameObject.transform);
				delay = (10*Random.value)/spawnRate;
			}

			prevCameraPos = currCameraPos;
		}
	}

	Vector3 getSpawnPosition() {
		Vector3 spawnPos = new Vector3();
		if (spawnInsideView) {
			spawnPos = randomCoordsInsideCameraView();
		} else {
			if (spawnOnlyOnRight) {
				spawnPos = randomCoordsOutsideRightSideOfCameraView();
			} else {
				spawnPos = randomCoordsOutsideCameraView();
			}
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
		return playerCamera.ViewportToWorldPoint(new Vector3(xCoord, yCoord, Mathf.Abs(playerCamera.transform.position.z)));
	}

	Vector3 randomCoordsOutsideRightSideOfCameraView() {
		return playerCamera.ViewportToWorldPoint(new Vector3(1.2f, Random.value, Mathf.Abs(playerCamera.transform.position.z)));
	}

	Vector3 randomCoordsInsideCameraView() {
		return playerCamera.ViewportToWorldPoint(new Vector3(Random.Range(0.2f, 0.8f), Random.Range(0.2f, 0.8f), Mathf.Abs(playerCamera.transform.position.z)));
	}
}
