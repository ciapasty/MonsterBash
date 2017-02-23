using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSpriteController : MonoBehaviour {

	GameController gc;
	MiniMapControl miniMapControl;

	GameObject bonfireGO;
	GameObject exitGO;
	 
	public int pixelsPerUnit;
	public Material spriteDiffuseMaterial;
	public Sprite emptyTile;
	public Sprite transparentTile;

	List<GameObject> mapTextureGOs;
	List<GameObject> wallBoxCollidersGOs;
	Dictionary<string, Sprite> floorWallSprites;

	void Start() {
		gc = GameController.Instance;
		miniMapControl = FindObjectOfType<MiniMapControl>();

		floorWallSprites = new Dictionary<string, Sprite>();
		foreach (var sprite in Resources.LoadAll<Sprite>("sprites/rooms/")) {
			floorWallSprites.Add(sprite.name, sprite);
		}
	}

	public void setupSprites() {
		mapTextureGOs = new List<GameObject>();

		createSpritesTextureForType("floorTexture", TileType.floor, TileType.wallBottom, TileType.wallMiddle, TileType.wallTop, "Background", -2);

		wallBoxCollidersGOs = new List<GameObject>();
		createVerticalWallBoxColliders();
		createHorizontalWallBoxColliders();

		updateWallTiles();
		createSpritesTextureForType("wallBottomTexture", TileType.wallBottom, TileType.wallBottom, TileType.wallBottom, TileType.wallBottom, "Background", -1);
		createSpritesTextureForType("wallMiddleTexture", TileType.wallMiddle, TileType.wallMiddle, TileType.wallMiddle, TileType.wallMiddle, "Background", 0);
		createSpritesTextureForType("wallTopTexture", TileType.wallTop, TileType.wallBottom, TileType.wallMiddle, TileType.floor, "Wall Top", 0);
		placeBonfireAndExit();
	}

	void placeBonfireAndExit() {
		bonfireGO = (GameObject)Instantiate(Resources.Load("prefabs/bonfire"), transform.position, Quaternion.identity);
		bonfireGO.transform.position = new Vector3(gc.map.bonfire.x+0.5f, gc.map.bonfire.y+0.5f, 0);

		exitGO = (GameObject)Instantiate(Resources.Load("prefabs/exit"), transform.position, Quaternion.identity);
		exitGO.transform.position = new Vector3(gc.map.exit.x+0.5f, gc.map.exit.y+0.5f, 0);
	}

	void createSpritesTextureForType(string name, TileType type, TileType neighbour1, TileType neighbour2, TileType neighbour3, string layerName, int depth) {
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

		mapTextureGOs.Add(go);
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
					cols = emptyTile.texture.GetPixels((int)emptyTile.rect.x, (int)emptyTile.rect.y, 16, 16);
					tex.SetPixels(x*pixelsPerUnit, y*pixelsPerUnit, 16, 16, cols);
				} else {
					cols = transparentTile.texture.GetPixels((int)transparentTile.rect.x, (int)transparentTile.rect.y, 16, 16);
					tex.SetPixels(x*pixelsPerUnit, y*pixelsPerUnit, 16, 16, cols);
				}
			}
		}
		return tex;
	}

	Sprite getSpriteForTileWithNeighbourTypes(Tile tile, TileType t1, TileType t2, TileType t3) {
		int tileIndex = 0;

		if (tile.type == TileType.wallBottom || tile.type == TileType.wallMiddle) {
			tileIndex = getSideTileIndex(
				t1, t1,
				gc.map.getTileAt(tile.x-1, tile.y),
				gc.map.getTileAt(tile.x+1, tile.y)
			);

			tileIndex += 64+(4*((int)tile.type-3));
		} else {
			tileIndex = getCrossTileIndex(// Above, left, below, right
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

			tileIndex += ((int)tile.type-1)*32;
		}

		string spriteName = "";
		if (tile.room == null || tile.room.tp.type == RoomType.exit) {
			spriteName = "room-"+RoomType.generic.ToString()+"_"+tileIndex;
		} else {
			spriteName = "room-"+tile.room.tp.type.ToString()+"_"+tileIndex;
		}

		if (!floorWallSprites.ContainsKey(spriteName)) {
			Debug.LogError("No sprite with name: "+spriteName);
			return null;
		} else {
			return floorWallSprites[spriteName];
		}
	}

	int getSideTileIndex(TileType type1, TileType type2, Tile left, Tile right) {
		var sum = 0;
		if (left != null && (left.type == type1 || left.type == type2))  sum += 1;
		if (right != null && (right.type == type1 || right.type == type2)) sum += 2;
		return sum;
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

	void createVerticalWallBoxColliders() {
		// Vertical BoxColliders
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
	}

	void createHorizontalWallBoxColliders() {
		// Horizontal BoxColliders
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
						if (length == 1 && (nextTile.type == TileType.floor &&
						    				gc.map.getTileAt(x-1, y).type == TileType.floor &&
											gc.map.getTileAt(x, y+1).type == TileType.floor &&
						    				gc.map.getTileAt(x, y-1).type == TileType.floor)) {
							createBoxCollider(
								new Vector2(startTile.x+0.5f, startTile.y+0.5f),
								new Vector2(1f, height),
								new Vector2(0f, 0.5f)
							);
						} else if (length > 1) {
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
		go.transform.SetParent(this.transform);

		BoxCollider2D box = go.AddComponent<BoxCollider2D>();
		box.size = size;
		box.offset = offset;

		wallBoxCollidersGOs.Add(go);
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
			}
		}
	}

	void updateTileAreas(Tile sourceTile, Tile tile) {
		if (tile != null) {
			tile.setRoom(sourceTile.room);
			tile.setCorridor(sourceTile.corridor);
		}
	}

	public void removeSprites() {
		Destroy(bonfireGO);
		Destroy(exitGO);

		foreach (var go in mapTextureGOs) {
			Destroy(go);
		}
		mapTextureGOs = null;

		foreach (var go in wallBoxCollidersGOs) {
			Destroy(go);
		}
		wallBoxCollidersGOs = null;
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
		//go_fogMap[go_tileMap[tile]].GetComponent<SpriteRenderer>().enabled = false;
		//miniMapControl.updateTile(tile);
	}
}
