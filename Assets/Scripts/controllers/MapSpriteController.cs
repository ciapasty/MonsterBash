using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSpriteController : MonoBehaviour {

	public GameObject wallPrefab;
	public GameObject floorPrefab;
	public GameObject doorPrefab;
	public GameObject fogPrefab;

	public Color bonfireRoomColor;
	public Color exitRoomColor;
	public Color corridorColor;

	GameController gc;
	//MiniMapControl miniMapControl;

	Dictionary<Tile, GameObject> go_tileMap;
	Dictionary<GameObject, GameObject> go_fogMap;

	void Start() {
		gc = GameController.Instance;
		//miniMapControl = FindObjectOfType<MiniMapControl>();
		go_tileMap = new Dictionary<Tile, GameObject>();
		go_fogMap = new Dictionary<GameObject, GameObject>();
	}

	public void setupSprites() {
		createTileGOs();
		updateTileSprites();
		placeBonfireAndExit();
	}

	void createTileGOs() {
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
				if (tile.type == TileType.outerWall) {
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
		Dictionary<string, Sprite> roomSprites = new Dictionary<string, Sprite>();
		foreach (var sprite in Resources.LoadAll<Sprite>("sprites/rooms/")) {
			roomSprites.Add(sprite.name, sprite);
		}

		foreach (var tile in go_tileMap.Keys) {
			SpriteRenderer go_sr = go_tileMap[tile].GetComponent<SpriteRenderer>();

			if (tile.room != null) {
				switch (gc.map.getRoomWithID(tile.room.ID).type) {
				case RoomType.exit:
					go_sr.color = exitRoomColor;
					break;
				case RoomType.bonfire:
					go_sr.color = bonfireRoomColor;
					break;
				case RoomType.generic:
					break;
				}
			} else {
				go_sr.color = corridorColor;
			}

			int tileIndex;
			if (tile.type == TileType.outerWall) {
				tileIndex = getCrossTileIndex( // Above, left, below, right
					TileType.floor, 
					gc.map.getTileAt(tile.x, tile.y+1),
					gc.map.getTileAt(tile.x-1, tile.y),
					gc.map.getTileAt(tile.x, tile.y-1), 
					gc.map.getTileAt(tile.x+1, tile.y));

				// Adjust bounding box for bottom outer walls
				if (tileIndex%2 == 1) {
					BoxCollider2D box = go_tileMap[tile].GetComponent<BoxCollider2D>();
					Vector2 boxOffset = box.offset;
					Vector2 boxSize = box.size;
					boxOffset.y = -0.125f;
					boxSize.y = 0.75f;
					box.size = boxSize;
					box.offset = boxOffset;
				}
				if (tileIndex == 0)
					// Diagonal tiles check
					tileIndex = 16+getCrossTileIndex( // AboveLeft, BelowLeft, BelowRight, AboveRight
						TileType.floor,
						gc.map.getTileAt(tile.x-1, tile.y+1),
						gc.map.getTileAt(tile.x-1, tile.y-1),
						gc.map.getTileAt(tile.x+1, tile.y-1),
						gc.map.getTileAt(tile.x+1, tile.y+1));
			} else {
				tileIndex = getCrossTileIndex( // Above, left, below, right
					tile.type, 
					gc.map.getTileAt(tile.x, tile.y+1),
					gc.map.getTileAt(tile.x-1, tile.y),
					gc.map.getTileAt(tile.x, tile.y-1), 
					gc.map.getTileAt(tile.x+1, tile.y));
			}

			string spriteName = RoomType.generic.ToString()+"_"+tile.type.ToString()+"_"+tileIndex;
			// Temporary, for testing
			if (!roomSprites.ContainsKey(spriteName)) {
				Debug.LogError("No sprite with name: "+spriteName);
			} else {
				go_sr.sprite = roomSprites[spriteName];
			}
		}
	}

	int getCrossTileIndex(TileType type, Tile above, Tile left, Tile below, Tile right) {
		var sum = 0;
		if (above != null && above.type == type) sum += 1;
		if (left != null && left.type == type)  sum += 2;
		if (below != null && below.type == type) sum += 4;
		if (right != null && right.type == type) sum += 8;
		return sum;
	}

	void placeBonfireAndExit() {
		GameObject bonfire_go = (GameObject)Instantiate(Resources.Load("prefabs/bonfire"), transform.position, Quaternion.identity);
		bonfire_go.transform.position = new Vector3(gc.map.bonfire.x+0.5f, gc.map.bonfire.y+0.5f, 0);

		GameObject exit_go = (GameObject)Instantiate(Resources.Load("prefabs/exit"), transform.position, Quaternion.identity);
		exit_go.transform.position = new Vector3(gc.map.exit.x+0.5f, gc.map.exit.y+0.5f, 0);
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
		//miniMapControl.updateTile(tile);
	}
}
