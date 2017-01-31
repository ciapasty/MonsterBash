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
	MiniMapControl miniMapControl;

	Dictionary<Tile, GameObject> go_tileMap;
	Dictionary<GameObject, GameObject> go_fogMap;

	void OnEnable() {
		areSpritesSetUp = false;
	}

	void Start() {
		gc = GameController.Instance;
		miniMapControl = FindObjectOfType<MiniMapControl>();
		go_tileMap = new Dictionary<Tile, GameObject>();
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
					go_tileMap.Add(tile, go);
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
					go_tileMap.Add(tile, go);
					// Fog of war GameObject
					GameObject fog_go = (GameObject)Instantiate(fogPrefab, transform.position, Quaternion.identity);
					fog_go.transform.SetParent(go.transform);
					fog_go.transform.position = go.transform.position;
					go_fogMap.Add(go, fog_go);
				}
				tile.registerBeenDiscoveredCallback(revealTile);
			}
		}
	}

	void updateTileSprites() {
		foreach (var tile in go_tileMap.Keys) {
			SpriteRenderer go_sr = go_tileMap[tile].GetComponent<SpriteRenderer>();

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


	public IEnumerator revealTilesForAreaWithID(int ID, Tile startTile) {
		Area tileArea;
		if (startTile.room != null && startTile.room.ID == ID) {
			tileArea = startTile.room;
		} else if (startTile.corridor != null && startTile.corridor.ID == ID) {
			tileArea = startTile.corridor;
		} else {
			Debug.LogError("Tile: ("+startTile.x+", "+startTile.y+") does not have Room nor Corridor!");
			tileArea = null;
			yield return null;
		}

		revealArea(tileArea, startTile);
	}

	void revealArea(Area area, Tile tile) {
		if (tile == null)
			return;

		if (tile.isDiscovered || tile.type == TileType.empty)
			return;

		Area tileArea;
		if (area is Room && tile.room != null) {
			tileArea = tile.room;
		} else if (area is Corridor && tile.corridor != null) {
			tileArea = tile.corridor;
		} else {
			return;
		}

		if (tileArea.ID != area.ID)
			return;

		tile.isDiscovered = true;

		revealArea(area, gc.map.getTileAt(tile.x+1, tile.y));
		revealArea(area, gc.map.getTileAt(tile.x-1, tile.y));
		revealArea(area, gc.map.getTileAt(tile.x, tile.y+1));
		revealArea(area, gc.map.getTileAt(tile.x, tile.y-1));
		revealArea(area, gc.map.getTileAt(tile.x+1, tile.y+1));
		revealArea(area, gc.map.getTileAt(tile.x+1, tile.y-1));
		revealArea(area, gc.map.getTileAt(tile.x-1, tile.y+1));
		revealArea(area, gc.map.getTileAt(tile.x-1, tile.y+1));
	}

	public void revealTile(Tile tile) {
		go_fogMap[go_tileMap[tile]].GetComponent<SpriteRenderer>().enabled = false;
		miniMapControl.updateTile(tile);
	}
}
