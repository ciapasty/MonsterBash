using Delaunay.Geo;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {

	public static GameController Instance { get; protected set; }

	public Map map;
	Dictionary<Room, RoomController> roomControllersMap;

	public GameObject playerPrefab;
	GameObject player;
	CameraFollowPlayer cameraFollowPlayer;

	MapGenerator mapGenerator;
	MapSpriteController mapSpriteController;
	MiniMapControl miniMapControl;

	// Player tracking
	public Tile currTile { get; protected set; }
	Tile prevTile;
	Area prevArea;
	Area currArea;

	void OnEnable() {
		if(Instance != null) {
			Debug.LogError("There should never be two game controllers.");
		}
		Instance = this;
	}

	void Awake() {
		roomControllersMap = new Dictionary<Room, RoomController>();
		mapGenerator = GetComponentInChildren<MapGenerator>();
		mapSpriteController = GetComponentInChildren<MapSpriteController>();
		cameraFollowPlayer = GameObject.FindGameObjectWithTag("PlayerCamera").GetComponent<CameraFollowPlayer>();
	}

	void Start() {
		//miniMapControl = FindObjectOfType<MiniMapControl>();

		/// 1. Generate Map
		mapGenerator.startMapCreation();

		/// 2. Fill in room -> randomize
		/// 3. Spawn enemies
		/// 4. Spawn player
		/// Move into UI scripts:
		/// 5. Reload UI
		/// 6. Hide loading screen
	}

	void postMapCreationSetup() {
		map = mapGenerator.map;
		mapSpriteController.setupSprites();
		foreach (var room in map.rooms) {
			GameObject room_go = new GameObject("Room_"+room.ID);
			room_go.transform.parent = gameObject.transform;
			room_go.transform.position = new Vector2(room.roomBase.x, room.roomBase.y);
			RoomController rc = room_go.AddComponent<RoomController>();
			rc.room = room;
			roomControllersMap.Add(room, rc);
			rc.createInRoomGOs();
		}
		//miniMapControl.setupMiniMap();
		Time.timeScale = 1f;
		spawnPlayer();
		StartCoroutine(mapSpriteController.revealTilesForAreaWithID(map.bonfire.room.ID, map.bonfire));
	}

	void Update() {
		if (player != null)
			trackPlayer();

		// TEMP, debug
		if (Input.GetKeyDown(KeyCode.Q)) {
			cameraFollowPlayer.snapToRoom(null, false);
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
					logRoomInfo();
					roomControllersMap[currArea as Room].playerEntered();
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
			//miniMapControl.updateTile(currTile);
			//miniMapControl.updateTile(prevTile);
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
