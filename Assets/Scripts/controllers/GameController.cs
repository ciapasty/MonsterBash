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

	CameraFollowPlayer cameraFollowPlayer;

	// Player tracking
	Tile prevTile;
	Tile currTile;
	int prevRoomID;
	int currRoomID;

	// TEMP - for door lock testing
	float _timer = 0f;
	float timer {
		get { return _timer; }
		set {
			_timer = value;
			if (timer <= 0) {
				if (currRoomID != -1) {
					map.getRoomWithID(currRoomID).doorsLocked(false);
					Debug.Log("Unlocking doors in room "+currRoomID);
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
		cameraFollowPlayer = Camera.main.GetComponent<CameraFollowPlayer>();
	}

	void Start() {
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
				Time.timeScale = 1f;
				spawnPlayer();
			}
		}
		if (player != null)
			trackPlayer();

		if (currRoomID >= 0) {
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
		currRoomID = prevRoomID = currTile.roomID.Value;
		logRoomInfo();
	}

	void trackPlayer() {
		currTile = map.getTileAt((int)(player.transform.position.x), (int)(player.transform.position.y));
		currRoomID = currTile.roomID.Value;
		if (currTile != prevTile) {
			if (currRoomID != prevRoomID && currTile.tClass != TileClass.door) {
				// TEMP - forr door lock testing
				cameraFollowPlayer.room = map.getRoomWithID(currRoomID);
				cameraFollowPlayer.snapToRoom = true;
				logRoomInfo();
				if (currRoomID != -1) {
					map.getRoomWithID(currRoomID).doorsLocked(true);
					Debug.Log("Locking doors in room: "+currRoomID);
					timer = 3f;
				}
				prevRoomID = currRoomID;
			}
			prevTile = currTile;
		}
	}

	void logRoomInfo() {
		if (currRoomID == -1) {
			Debug.Log("Corridor");
		} else {
			Debug.Log("Room: id "+currRoomID+", room type "+map.getRoomWithID(currRoomID).type);
		}
	}
}
