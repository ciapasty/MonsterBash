using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSpriteController : MonoBehaviour {

	public GameObject wallPrefab;
	public GameObject floorPrefab;
	public GameObject doorPrefab;
	public GameObject fogPrefab;

	GameController gc;
	MiniMapControl miniMapControl;

	Dictionary<Tile, GameObject> go_tileMap;
	Dictionary<GameObject, GameObject> go_fogMap;
	GameObject bonfireGO;
	GameObject exitGO;

	// Testing
	public Material spriteDiffuseMaterial;
	public int pixelsPerUnit;
	public Sprite emptyTile;
	public Sprite transparentTile;

	GameObject floorTextureGO;
	GameObject wallBotTextureGO;
	GameObject wallMidTextureGO;
	GameObject wallTopTextureGO;

	Dictionary<string, Sprite> roomSprites;

	void Start() {
		gc = GameController.Instance;
		miniMapControl = FindObjectOfType<MiniMapControl>();

		roomSprites = new Dictionary<string, Sprite>();
		foreach (var sprite in Resources.LoadAll<Sprite>("sprites/rooms/")) {
			roomSprites.Add(sprite.name, sprite);
		}
	}

	public void setupSprites() {
		floorTextureGO = createSpritesTextureForType("floorTexture", TileType.floor, TileType.wallBottom, TileType.wallMiddle, TileType.wallTop, "Background", -2);
		createWallBoxColliders();
		updateWallTiles();
		wallBotTextureGO = createSpritesTextureForType("wallBottomTexture", TileType.wallBottom, TileType.wallBottom, TileType.wallBottom, TileType.wallBottom, "Background", -1);
		wallMidTextureGO = createSpritesTextureForType("wallMiddleTexture", TileType.wallMiddle, TileType.wallMiddle, TileType.wallMiddle, TileType.wallMiddle, "Background", 0);
		wallTopTextureGO = createSpritesTextureForType("wallTopTexture", TileType.wallTop, TileType.wallBottom, TileType.wallMiddle, TileType.floor, "Wall Top", 0);
		placeBonfireAndExit();
	}

	void placeBonfireAndExit() {
		bonfireGO = (GameObject)Instantiate(Resources.Load("prefabs/bonfire"), transform.position, Quaternion.identity);
		bonfireGO.transform.position = new Vector3(gc.map.bonfire.x+0.5f, gc.map.bonfire.y+0.5f, 0);

		exitGO = (GameObject)Instantiate(Resources.Load("prefabs/exit"), transform.position, Quaternion.identity);
		exitGO.transform.position = new Vector3(gc.map.exit.x+0.5f, gc.map.exit.y+0.5f, 0);
	}

	GameObject createSpritesTextureForType(string name, TileType type, TileType neighbour1, TileType neighbour2, TileType neighbour3, string layerName, int depth) {
		GameObject go = new GameObject(name);
		go.transform.SetParent(this.transform);
		go.transform.position = new Vector3(gc.map.width/2, gc.map.height/2, 0);

		Texture2D tex = new Texture2D(gc.map.width*pixelsPerUnit, gc.map.height*pixelsPerUnit);
		tex.filterMode = FilterMode.Point;
		tex = addSpritesForType(tex, type, neighbour1, neighbour2, neighbour3);
		tex.Apply();

		Sprite sprite = Sprite.Create(tex, new Rect(0,0, tex.width, tex.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
		SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
		sr.material = spriteDiffuseMaterial;
		sr.sprite = sprite;
		sr.sortingLayerName = layerName;
		sr.sortingOrder = depth;

		return go;
	}

	Texture2D addSpritesForType(Texture2D tex, TileType type, TileType neighbour1, TileType neighbour2, TileType neighbour3) {
		for (int x = 0; x < gc.map.width; x++) {
			for (int y = 0; y < gc.map.height; y++) {
				Tile tile = gc.map.getTileAt(x, y);
				Sprite sprite;
				Color[] cols;
				if (tile.type == type) {
					sprite = getSpriteForTileWithNeighbourTypes(tile, neighbour1, neighbour2, neighbour3);
					if (sprite != null) {
						cols = sprite.texture.GetPixels((int)sprite.rect.x, (int)sprite.rect.y, (int)sprite.rect.width, (int)sprite.rect.height);
						tex.SetPixels(x*pixelsPerUnit, y*pixelsPerUnit, (int)sprite.rect.width, (int)sprite.rect.height, cols);
					}
				} else if (tile.type == TileType.empty) {
					cols = emptyTile.texture.GetPixels(0, 0, 16, 16);
					tex.SetPixels(x*pixelsPerUnit, y*pixelsPerUnit, 16, 16, cols);
				} else {
					cols = transparentTile.texture.GetPixels(0, 0, 16, 16);
					tex.SetPixels(x*pixelsPerUnit, y*pixelsPerUnit, 16, 16, cols);
				}
			}
		}
		return tex;
	}


	Sprite getSpriteForTileWithNeighbourTypes(Tile tile, TileType t1, TileType t2, TileType t3) {
		
		int tileIndex = getCrossTileIndex(// Above, left, below, right
			t1, t2, t3,
			gc.map.getTileAt(tile.x, tile.y+1),
			gc.map.getTileAt(tile.x-1, tile.y),
			gc.map.getTileAt(tile.x, tile.y-1), 
			gc.map.getTileAt(tile.x+1, tile.y));

		if (tileIndex == 0) {
			// Diagonal tiles check
			tileIndex = 16+getCrossTileIndex(// AboveLeft, BelowLeft, BelowRight, AboveRight
				t1, t2, t3,
				gc.map.getTileAt(tile.x-1, tile.y+1),
				gc.map.getTileAt(tile.x-1, tile.y-1),
				gc.map.getTileAt(tile.x+1, tile.y-1),
				gc.map.getTileAt(tile.x+1, tile.y+1));
		}

		string spriteName = RoomType.generic.ToString()+"_"+tile.type.ToString()+"_"+tileIndex;
		// Temporary, for testing
		if (!roomSprites.ContainsKey(spriteName)) {
			Debug.LogError("No sprite with name: "+spriteName);
			return null;
		} else {
			return roomSprites[spriteName];
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

	void createWallBoxColliders() {
		for (int x = 0; x < gc.map.width; x++) {
			Tile currTile;
			Tile nextTile;

			Tile startTile = null;
			int count = 0;

			for (int y = 0; y < gc.map.height; y++) {
				currTile = gc.map.getTileAt(x, y);
				nextTile = gc.map.getTileAt(x, y+1);
				if (currTile != null && currTile.type == TileType.wallBottom) {
					if (count == 0) {
						startTile = currTile;
						count = 1;
					} else {
						count++;
					}
					if (nextTile != null && nextTile.type != TileType.wallBottom) {
						int length = currTile.y-startTile.y+1;
						if (length > 2) {
							createBoxCollider(
								new Vector2(startTile.x+0.5f, ((startTile.y+currTile.y)/2f)+0.5f),
								new Vector2(1f, length),
								Vector2.zero
							);
						}
						count = 0;
					}
				}
			}
		}

		for (int y = 0; y < gc.map.height; y++) {
			Tile currTile;
			Tile nextTile;

			Tile startTile = null;
			int count = 0;
			float height = 1;

			for (int x = 0; x < gc.map.width; x++) {
				currTile = gc.map.getTileAt(x, y);
				nextTile = gc.map.getTileAt(x+1, y);
				if (currTile != null && currTile.type == TileType.wallBottom) {
					if (count == 0) {
						startTile = currTile;
						count = 1;
					} else {
						
						count++;
					}

					if (gc.map.getTileAt(x, y+1).type == TileType.floor)
						height = 2f;
					
					if (nextTile != null && nextTile.type != TileType.wallBottom) {
						int length = currTile.x-startTile.x+1;
						if (length > 1) {
							if (height == 2) {
								createBoxCollider(
									new Vector2((((startTile.x+currTile.x)/2f)+0.5f), startTile.y+0.5f),
									new Vector2(length, height),
									new Vector2(0f, 0.5f)
								);
							} else {
								createBoxCollider(
									new Vector2((((startTile.x+currTile.x)/2f)+0.5f), startTile.y+0.5f),
									new Vector2(length, 1f),
									Vector2.zero
								);
							}
						}
						count = 0;
					}
				}
			}
		}
	}

	void createBoxCollider(Vector2 position, Vector2 size, Vector2 offset) {
		GameObject go = new GameObject("BoxCollider");
		go.transform.position = position;

		BoxCollider2D box = go.AddComponent<BoxCollider2D>();
		box.size = size;
		box.offset = offset;
	}

	void updateWallTiles() {
		for (int y = gc.map.height-1; y >= 0; y--) {
			for (int x = 0; x < gc.map.width; x++) {
				Tile tile = gc.map.getTileAt(x, y);
				Tile tileUp = gc.map.getTileAt(x, y+1);
				Tile tileUpUp = gc.map.getTileAt(x, y+2);
				if (tile.type == TileType.wallBottom) {
					if (gc.map.getTileAt(x, y-1) == null || gc.map.getTileAt(x, y-1).type == TileType.empty) {
						tile.type = TileType.empty;
						tileUp.type = TileType.empty;
						tileUpUp.type = TileType.wallTop;
					} else {
						tileUp.type = TileType.wallMiddle;
						tileUpUp.type = TileType.wallTop;
					}
				}
				updateTileAreas(tile, tileUp);
				updateTileAreas(tile, tileUpUp);
			}
		}
	}

	void updateTileAreas(Tile sourceTile, Tile tile) {
		if (tile != null) {
			if (tile.room == null)
				tile.setRoom(sourceTile.room);
			if (tile.corridor == null)
				tile.setCorridor(sourceTile.corridor);
		}
	}

	// ====================
	// ======= Deprecated??
	// ====================

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

	// ======================
	// TODO: GENERAL OVERHAUL
	// ======================

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
