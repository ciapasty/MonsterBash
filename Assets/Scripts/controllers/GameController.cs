using Delaunay.Geo;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {

	public static GameController Instance { get; protected set; }

	public Map map;

	public GameObject playerPrefab;
	GameObject player;

	MapGenerator mapGenerator;
	MapSpriteController mapSpriteController;
	MiniMapControl miniMapControl;

	CameraFollowPlayer cameraFollowPlayer;

	// Player tracking
	Tile prevTile;
	public Tile currTile { get; protected set; }
	Area prevArea;
	Area currArea;

	// TEMP - for door lock testing
	float _timer = 0f;
	float timer {
		get { return _timer; }
		set {
			_timer = value;
			if (timer <= 0) {
				if (currArea is Room) {
					map.getRoomWithID(currArea.ID).lockDoors(false);
					Debug.Log("Unlocking doors in room "+currArea.ID);
					//cameraFollowPlayer.snapToRoom = false;
				}
			}
		}
	}

	void OnEnable() {
		if(Instance != null) {
			Debug.LogError("There should never be two game controllers.");
		}
		Instance = this;
	}

	void Awake() {
		mapGenerator = GetComponentInChildren<MapGenerator>();
		mapSpriteController = GetComponentInChildren<MapSpriteController>();
		cameraFollowPlayer = GameObject.FindGameObjectWithTag("PlayerCamera").GetComponent<CameraFollowPlayer>();
	}

	void Start() {
		miniMapControl = FindObjectOfType<MiniMapControl>();
		/// 1. Generate Map
		mapGenerator.startMapCreation();

		/// 2. Fill in room -> randomize
		/// 3. Spawn enemies
		/// 4. Spawn player
		/// Move into UI scripts:
		/// 5. Reload UI
		/// 6. Hide loading screen
	}

	void Update() {
		if (!mapSpriteController.areSpritesSetUp) {
			if (mapGenerator.isFinished) {
				map = mapGenerator.map;
				mapSpriteController.setupSprites();
				FindObjectOfType<MiniMapControl>().setupMiniMap();
				Time.timeScale = 1f;
				spawnPlayer();
			}
		}
		if (player != null)
			trackPlayer();

		if (currArea != null && currArea.ID >= 0) {
			if (timer > 0) {
				timer -= Time.deltaTime;
			}
		}

		if (Input.GetKeyDown(KeyCode.Q)) {
			cameraFollowPlayer.snapToRoom = false;
		}
	}

	public void spawnPlayer() {
		GameObject p_go = (GameObject)Instantiate(playerPrefab, transform.position, Quaternion.identity);
		p_go.transform.position = new Vector3(map.bonfire.x-1, map.bonfire.y+1, 0);
		player = p_go;
		currTile = prevTile = map.getTileAt((int)(player.transform.position.x), (int)(player.transform.position.y));
		currArea = prevArea = currTile.room;
		logRoomInfo();
	}

	void trackPlayer() {
		currTile = map.getTileAt((int)(player.transform.position.x), (int)(player.transform.position.y));
		if (currTile != prevTile) {
			//logTileInfo();
			if (currTile.room != null && currTile.corridor != null) {
				// Door
				Debug.Log("Crossing door. "+"Room: "+currTile.room.ID+"; Corridor: "+currTile.corridor.ID);
				if (!currTile.corridor.isDiscovered) {
					StartCoroutine(mapSpriteController.revealTilesForAreaWithID(currTile.corridor.ID, currTile));
				}
				
			} 
			if (currTile.room != null && currTile.corridor == null) {
				// Entered Room
				currArea = currTile.room;
				if (!currArea.isDiscovered) {
					currArea.isDiscovered = true;
					StartCoroutine(mapSpriteController.revealTilesForAreaWithID(currArea.ID, currTile));
				}
				if (currArea.ID != prevArea.ID && currTile.tClass != TileClass.door) {
					// TEMP - for door lock testing
					cameraFollowPlayer.room = map.getRoomWithID(currArea.ID);
					cameraFollowPlayer.snapToRoom = true;
					logRoomInfo();
					map.getRoomWithID(currArea.ID).lockDoors(true);
					Debug.Log("Locking doors in room: "+currArea.ID);
					timer = 3f;
					prevArea = currArea;
				}
			} 
			if (currTile.room == null && currTile.corridor != null) {
				// Entered Corridor
				currArea = currTile.corridor;
				if (!currArea.isDiscovered) {
					currArea.isDiscovered = true;
					StartCoroutine(mapSpriteController.revealTilesForAreaWithID(currArea.ID, currTile));
				}
				if (currArea.ID != prevArea.ID) {
					logRoomInfo();
					prevArea = currArea;
				}
			} 
			if (currTile.room == null && currTile.corridor == null) {
				Debug.LogError("Player outside room or corridor??");
			}
			miniMapControl.updateTile(currTile);
			miniMapControl.updateTile(prevTile);
			prevTile = currTile;
		}
	}

	void logRoomInfo() {
		if (currArea is Corridor) {
			Debug.Log("Corridor: id "+currArea.ID+"; Discovered: "+currArea.isDiscovered);
		} else {
			Debug.Log("Room: id "+currArea.ID+", room type "+map.getRoomWithID(currArea.ID).type+"; Discovered: "+currArea.isDiscovered);
		}
	}

	void logTileInfo() {
		Debug.Log("Current tile: ("+currTile.x+", "+currTile.y+"); Corridor: "+currTile.corridor+"; Room: "+currTile.room);
	}
}
