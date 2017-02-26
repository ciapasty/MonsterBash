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

	private float restartTimer = 6f;

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
		mapGenerator.startMapCreation();
	}

	void postMapCreationSetup() {
		roomControllersMap = new Dictionary<Room, RoomController>();
		map = mapGenerator.map;

		Time.timeScale = 1f;

		mapSpriteController.setupSprites();
		foreach (var room in map.rooms) {
			GameObject room_go = new GameObject("Room_"+room.ID);
			room_go.transform.parent = gameObject.transform;
			room_go.transform.position = new Vector2(room.roomBase.x, room.roomBase.y);
			RoomController rc = room_go.AddComponent<RoomController>();
			rc.room = room;
			roomControllersMap.Add(room, rc);
			rc.createGOs();
		}
		//miniMapControl.setupMiniMap();
		if (player == null) {
			spawnPlayer();
		} else {
			player.transform.position = new Vector3(map.bonfire.x-1, map.bonfire.y+1, 0);
			currTile = prevTile = map.getTileAt((int)(player.transform.position.x), (int)(player.transform.position.y));
			currArea = prevArea = currTile.room;
		}
		//miniMapControl.updateMiniMap();
		StartCoroutine(mapSpriteController.revealTilesForAreaWithID(map.bonfire.room.ID, map.bonfire));
	}

	void Update() {
		if (player != null) {
			if (map != null)
				trackPlayer(); 

			// Player has died
			if (player.GetComponent<PlayerHealth>().isDead) {
				restartTimer -= Time.deltaTime;
			}
			if (restartTimer < 4) {
				// Show "You Died" splash
				GameObject.FindGameObjectWithTag("UI_YouDied").GetComponent<UnityEngine.UI.Text>().enabled = true;
			}
			if (restartTimer < 0) {
				// Hide "You Died" splash
				GameObject.FindGameObjectWithTag("UI_YouDied").GetComponent<UnityEngine.UI.Text>().enabled = false;
				player.transform.position = new Vector3(map.bonfire.x-1, map.bonfire.y+1, 0);
				player.SendMessage("onRespawn");

				cameraFollowPlayer.snapToRoom(null, false);

				resetRooms();

				restartTimer = 6f;
			}
		}
		

		// TEMP, debug
		if (Input.GetKeyDown(KeyCode.Q)) {
			cameraFollowPlayer.snapToRoom(null, false);
		}
	}

	void resetRooms() {
		foreach (var room in map.rooms) {
			roomControllersMap[room].respawnEnemies();
			roomControllersMap[room].lockDoors(false);
		}
	}

	public void spawnPlayer() {
		GameObject p_go = (GameObject)Instantiate(playerPrefab, transform.position, Quaternion.identity);
		p_go.transform.position = new Vector3(map.bonfire.x-1, map.bonfire.y+1, 0);
		player = p_go;
		currTile = prevTile = map.getTileAt((int)(player.transform.position.x), (int)(player.transform.position.y));
		currArea = prevArea = currTile.room;
		//miniMapControl.updateTile(currTile);
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
			Debug.Log("Room: id "+currArea.ID+", room type "+map.getRoomWithID(currArea.ID).tp.type+"; Discovered: "+currArea.isDiscovered);
		}
	}

	void logTileInfo() {
		Debug.Log("Current tile: ("+currTile.x+", "+currTile.y+"); Corridor: "+currTile.corridor+"; Room: "+currTile.room);
	}

	void playerEnteredExit() {
		// 1. Show loading screen

		// 2. Save player state

		// 3. Map sprites clean-up
		foreach (var room in roomControllersMap.Keys) {
			roomControllersMap[room].removeGOs();
			Destroy(roomControllersMap[room].gameObject);
			//roomControllersMap.Remove(room);
		}
		roomControllersMap = null;

		mapSpriteController.removeSprites();

		// 4. Map cleanup
		map = null;

		// 5. Map generation with new level parameteres
		mapGenerator.startMapCreation();

		// 6. Restore player state

		// 7. Hide loading screen

	}
}
