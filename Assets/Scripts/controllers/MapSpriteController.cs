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
	GameObject bonfireGO;
	GameObject exitGO;

	void Start() {
		gc = GameController.Instance;
		//miniMapControl = FindObjectOfType<MiniMapControl>();
	}

	public void setupSprites() {
		createTileGOs();
		updateTileSprites();
		placeBonfireAndExit();
	}

	public void removeSprites() {
		foreach (var tile in go_tileMap.Keys) {
			Destroy(go_fogMap[go_tileMap[tile]]);
			Destroy(go_tileMap[tile]);
		}
		Destroy(bonfireGO);
		Destroy(exitGO);

		go_fogMap = null;
		go_tileMap = null;
	}

	void createTileGOs() {
		go_tileMap = new Dictionary<Tile, GameObject>();
		go_fogMap = new Dictionary<GameObject, GameObject>();
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
				if (tile.type == TileType.wallBottom || tile.type == TileType.wallMiddle || tile.type == TileType.wallTop) {
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
			if (tile.type == TileType.wallBottom || tile.type == TileType.wallMiddle) {
				tileIndex = getSideTileIndex( // left, right
					tile.type, TileType.wallTop, 
					gc.map.getTileAt(tile.x+1, tile.y),
					gc.map.getTileAt(tile.x-1, tile.y));
			} else {
				TileType neighbourType1;
				TileType neighbourType2;
				TileType neighbourType3;

				switch(tile.type) {
				case TileType.floor:
					neighbourType1 = TileType.wallBottom;
					neighbourType2 = TileType.wallMiddle;
					neighbourType3 = TileType.wallTop;

					break;
				case TileType.wallTop:
					neighbourType1 = TileType.floor;
					neighbourType2 = TileType.wallMiddle;
					neighbourType3 = TileType.wallBottom;
					break;
				default:
					neighbourType1 = TileType.empty;
					neighbourType2 = TileType.empty;
					neighbourType3 = TileType.empty;
					break;
				}

				tileIndex = getCrossTileIndex( // Above, left, below, right
					neighbourType1, neighbourType2, neighbourType3,
					gc.map.getTileAt(tile.x, tile.y+1),
					gc.map.getTileAt(tile.x-1, tile.y),
					gc.map.getTileAt(tile.x, tile.y-1), 
					gc.map.getTileAt(tile.x+1, tile.y));

				// Adjust bounding box for top walls
				//				if (tile.type == TileType.wallTop) {
				//					if (tileIndex%2 == 1) {
				//						BoxCollider2D box = go_tileMap[tile].GetComponent<BoxCollider2D>();
				//						Vector2 boxOffset = box.offset;
				//						Vector2 boxSize = box.size;
				//						boxOffset.y = -0.125f;
				//						boxSize.y = 0.75f;
				//						box.size = boxSize;
				//						box.offset = boxOffset;
				//					}
				//				}
				if (tileIndex == 0)
					// Diagonal tiles check
					tileIndex = 16+getCrossTileIndex( // AboveLeft, BelowLeft, BelowRight, AboveRight
						neighbourType1, neighbourType2, neighbourType3,
						gc.map.getTileAt(tile.x-1, tile.y+1),
						gc.map.getTileAt(tile.x-1, tile.y-1),
						gc.map.getTileAt(tile.x+1, tile.y-1),
						gc.map.getTileAt(tile.x+1, tile.y+1));
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

	int getCrossTileIndex(TileType type1, TileType type2, TileType type3, Tile above, Tile left, Tile below, Tile right) {
		var sum = 0;
		if (above != null && (above.type == type1 || above.type == type2 || above.type == type3))
			sum += 1;
		if (left != null && (left.type == type1 || left.type == type2 || left.type == type3))
			sum += 2;
		if (below != null && (below.type == type1 || below.type == type2 || below.type == type3))
			sum += 4;
		if (right != null && (right.type == type1 || right.type == type2 || right.type == type3))
			sum += 8;
		return sum;
	}

	int getSideTileIndex(TileType type1, TileType type2, Tile left, Tile right) {
		var sum = 0;
		if (left != null && (left.type == type1 || left.type == type2))  sum += 1;
		if (right != null && (right.type == type1 || right.type == type2)) sum += 2;
		return sum;
	}

	void placeBonfireAndExit() {
		bonfireGO = (GameObject)Instantiate(Resources.Load("prefabs/bonfire"), transform.position, Quaternion.identity);
		bonfireGO.transform.position = new Vector3(gc.map.bonfire.x+0.5f, gc.map.bonfire.y+0.5f, 0);

		exitGO = (GameObject)Instantiate(Resources.Load("prefabs/exit"), transform.position, Quaternion.identity);
		exitGO.transform.position = new Vector3(gc.map.exit.x+0.5f, gc.map.exit.y+0.5f, 0);
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
