using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSpriteController : MonoBehaviour {

	public bool areSpritesSetUp { get; protected set;}

	public GameObject wallPrefab;
	public GameObject floorPrefab;
	public GameObject doorPrefab;
	public GameObject fogPrefab;

	GameController gc;

	Dictionary<GameObject, Tile> go_tileMap;
	Dictionary<GameObject, GameObject> go_fogMap;

	void OnEnable() {
		areSpritesSetUp = false;
	}

	void Start() {
		gc = GameController.Instance;
		go_tileMap = new Dictionary<GameObject, Tile>();
		go_fogMap = new Dictionary<GameObject, GameObject>();
	}

	public void setupSprites() {
		createGOs();
		updateTileSprites();

		createDoorGOs();

		placeBonfire();
		areSpritesSetUp = true;
	}

	void createGOs() {
		for (int x = 0; x < gc.map.width; x++) {
			for (int y = 0; y < gc.map.height; y++) {
				Tile tile = gc.map.getTileAt(x, y);
				if (tile.type == TileType.floor) {
					// Floor tile GameObject
					GameObject go = (GameObject)Instantiate(floorPrefab, transform.position, Quaternion.identity);
					go.transform.SetParent(this.transform);
					go.transform.position = new Vector2(x+0.5f, y+0.5f);
					go_tileMap.Add(go, tile);
					// Fog of war GameObject
					GameObject fog_go = (GameObject)Instantiate(fogPrefab, transform.position, Quaternion.identity);
					fog_go.transform.SetParent(go.transform);
					fog_go.transform.position = go.transform.position;
					go_fogMap.Add(go, fog_go);
				}
				if (tile.type == TileType.wall) {
					// Wall tile GameObject
					GameObject go = (GameObject)Instantiate(wallPrefab, transform.position, Quaternion.identity);
					go.transform.SetParent(this.transform);
					go.transform.position = new Vector2(x+0.5f, y+0.5f);
					go_tileMap.Add(go, tile);
					// Fog of war GameObject
					GameObject fog_go = (GameObject)Instantiate(fogPrefab, transform.position, Quaternion.identity);
					fog_go.transform.SetParent(go.transform);
					fog_go.transform.position = go.transform.position;
					go_fogMap.Add(go, fog_go);
				}
			}
		}
	}

	void updateTileSprites() {
		foreach (var go in go_tileMap.Keys) {
			SpriteRenderer go_sr = go.GetComponent<SpriteRenderer>();

			Tile tile = go_tileMap[go];
			if (tile.room != null) {
				switch (gc.map.getRoomWithID(tile.room.ID).type) {
				case RoomType.exit:
					go_sr.color = Color.blue;
					break;
				case RoomType.bonfire:
					go_sr.color = Color.green;
					break;
				case RoomType.generic:
					break;
				}
				if (tile.tClass == TileClass.door) {
					go_sr.color = Color.red;
				}
			} else {
				go_sr.color = Color.yellow;
			}
		}
	}

	void createDoorGOs() {
		foreach (var room in gc.map.rooms) {
			foreach (var door in room.doors) {
				GameObject door_go = (GameObject)Instantiate(doorPrefab, transform.position, Quaternion.identity);
				door_go.transform.SetParent(this.transform);
				door_go.transform.position = new Vector2(door.tile.x+0.5f, door.tile.y+0.5f);
				door_go.GetComponent<DoorController>().door = door;
				door.registerOnChangedCallback(door_go.GetComponent<DoorController>().onStateChanged);
				door.cbOnChanged(door);
			}
		}

	}

	void placeBonfire() {
		GameObject go = (GameObject)Instantiate(Resources.Load("prefabs/bonfire"), transform.position, Quaternion.identity);
		go.transform.position = new Vector3(gc.map.bonfire.x+0.5f, gc.map.bonfire.y+0.5f, 0);
	}


	public void revealTilesForAreaWithID(int ID) {
		foreach (var go in go_tileMap.Keys) {
			if ((go_tileMap[go].room != null && go_tileMap[go].room.ID == ID) || 
				(go_tileMap[go].corridor != null && go_tileMap[go].corridor.ID == ID)) {
				go_fogMap[go].GetComponent<SpriteRenderer>().enabled = false;
			}
		}
	}
}
