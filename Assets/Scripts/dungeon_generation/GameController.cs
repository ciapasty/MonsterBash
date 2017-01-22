using Delaunay.Geo;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {

	public static GameController instance { get; protected set; }

	public Map map;

	public GameObject playerPrefab;
	public GameObject wallPrefab;
	public GameObject floorPrefab;

	MapGenerator mapGenerator;

	private bool tilesSetupFinished = false;

	void OnEnable() {
		if(instance != null) {
			Debug.LogError("There should never be two game controllers.");
		}
		instance = this;
	}

	void Awake() {
		mapGenerator = GetComponentInChildren<MapGenerator>();

	}

	void Start() {
		/// 1. Generate Map
		mapGenerator.startMapCreation();

		/// 2. Fill in room -> randomize
		/// 3. Spawn enemies
		/// 4. Spawn player
		/// Move into UI scripts:
		/// 5. Reload UI
		/// 6. Hide splash screen
	}

	void Update() {
		if (!tilesSetupFinished) {
			if (mapGenerator.isFinished) {
				setupWorld();
			}
		}
	}

	public void setupWorld() {
		map = mapGenerator.map;
		generateTiles();

		Time.timeScale = 1f;

		spawnPlayer();

		tilesSetupFinished = true;
	}

	public void spawnPlayer() {
		GameObject player = (GameObject)Instantiate(playerPrefab, transform.position, Quaternion.identity);
		//player.transform.position = world.getPlayerPosition();
	}

	public void generateTiles() {
		for (int x = 0; x < map.width; x++) {
			for (int y = 0; y < map.height; y++) {
				if (map.getTileAt(x, y).type == TileType.floor) {
					GameObject tile = (GameObject)Instantiate(floorPrefab, transform.position, Quaternion.identity);
					tile.transform.SetParent(this.transform);
					tile.transform.position = new Vector2(x+0.5f, y+0.5f);
					if (map.getTileAt(x, y).tClass == TileClass.door) {
						tile.GetComponent<SpriteRenderer>().color = Color.red;
					}
				}
				if (map.getTileAt(x, y).type == TileType.wall) {
					GameObject tile = (GameObject)Instantiate(wallPrefab, transform.position, Quaternion.identity);
					tile.transform.SetParent(this.transform);
					tile.transform.position = new Vector2(x+0.5f, y+0.5f);
				}
			}
		}
	}
}
