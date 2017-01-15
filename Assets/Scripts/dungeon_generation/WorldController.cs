using Delaunay.Geo;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldController : MonoBehaviour {

	public World world;

	public GameObject playerPrefab;
	public GameObject wallPrefab;
	public GameObject floorPrefab;

	private MapGenerator generator;

	void Awake() {
		generator = GetComponentInChildren<MapGenerator>();
	}

	void Start() {
		/// 1. Generate Map
		generator.generateRooms();

		/// 2. Fill in room -> randomize
		/// 3. Spawn enemies
		/// 4. Spawn player
		/// Move into UI scripts:
		/// 5. Reload UI
		/// 6. Hide splash screen
	}

	void Update() {
		
	}

	public void setupWorld(List<Room> mainRooms, List<LineSegment> corridors) {
		world = new World(mainRooms, corridors);
		generateTiles();

		Time.timeScale = 1f;

		spawnPlayer();
	}

	public void spawnPlayer() {
		GameObject player = (GameObject)Instantiate(playerPrefab, transform.position, Quaternion.identity);
		player.transform.position = world.getPlayerPosition();
	}

	public void generateTiles() {
		for (int x = 0; x < world.width; x++) {
			for (int y = 0; y < world.height; y++) {
				if (world.getTileAt(x, y).type == TileType.floor) {
					GameObject tile = (GameObject)Instantiate(floorPrefab, transform.position, Quaternion.identity);
					tile.transform.SetParent(this.transform);
					tile.transform.position = new Vector2(x+0.5f, y+0.5f);
				}
				if (world.getTileAt(x, y).type == TileType.wall) {
					GameObject tile = (GameObject)Instantiate(wallPrefab, transform.position, Quaternion.identity);
					tile.transform.SetParent(this.transform);
					tile.transform.position = new Vector2(x+0.5f, y+0.5f);
				}
			}
		}
	}
}
