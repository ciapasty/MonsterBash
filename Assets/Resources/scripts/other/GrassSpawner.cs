using UnityEngine;
using System.Collections;

public class GrassSpawner : MonoBehaviour {

	public Sprite[] grassSprites;
	public int initialCount = 15;
	public int spawnDensity = 25;
	public float spawnRate = 2;

	private Vector3 currCameraPos;
	private Vector3 prevCameraPos;
	private float timer;

	void Start() {
		currCameraPos = Camera.main.transform.position;
		prevCameraPos = currCameraPos;
		timer = 1/spawnRate;

		for (int i = 0; i < initialCount; i++) {
			GameObject go = createGrassGO();
			go.transform.position = getSpawnPosition(true);
		}
	}

	void Update() {
		if (timer <= 0) {
			currCameraPos = Camera.main.transform.position;
			if (Vector3.Distance(currCameraPos, prevCameraPos) > 0) {
				for (int i = 0; i < spawnDensity; i++) {
					GameObject go = createGrassGO();
					go.transform.position = getSpawnPosition(false);
				}
			}
			timer = 1/spawnRate;
		}
		prevCameraPos = currCameraPos;
		timer -= Time.deltaTime;
	}

	GameObject createGrassGO() {
		GameObject go = new GameObject();
		go.name = "grass";
		go.transform.SetParent(transform);

		SpriteRenderer go_sr = go.AddComponent<SpriteRenderer>();
		go_sr.sprite = grassSprites[Random.Range(0, 4)];
		go_sr.material = (Material)Resources.Load("materials/Lighting_Sprite");
		go_sr.sortingLayerName = "Foliage";

		TimedDestroy go_td = go.AddComponent<TimedDestroy>();
		go_td.timer = 1f;
		go_td.onlyOutsideOfView = true;

		return go;
	}

	Vector3 getSpawnPosition(bool insideScreen) {
		Vector3 spawnPos = new Vector3();
		if (insideScreen) {
			spawnPos = randomCoordsInsideCameraView();
		} else {
			spawnPos = randomCoordsOutsideCameraView();
		}
		if (Vector3.Distance(spawnPos, GameObject.FindGameObjectWithTag("Player").transform.position) < 0) {
			return getSpawnPosition(insideScreen);
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
		return Camera.main.ViewportToWorldPoint(new Vector3(xCoord, yCoord, Mathf.Abs(Camera.main.transform.position.z)));
	}

	Vector3 randomCoordsInsideCameraView() {
		return Camera.main.ViewportToWorldPoint(new Vector3(Random.value, Random.value, Mathf.Abs(Camera.main.transform.position.z)));
	}
}
