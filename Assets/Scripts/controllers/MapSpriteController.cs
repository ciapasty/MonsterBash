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
	public Material material;
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
		floorTextureGO = createSpritesTextureForType("floorTexture", TileType.floor, TileType.wallBottom, TileType.wallMiddle, TileType.wallTop, 0f);
		updateWallTiles();
		wallBotTextureGO = createSpritesTextureForType("wallBottomTexture", TileType.wallBottom, TileType.wallBottom, TileType.wallBottom, TileType.wallBottom, 0.01f);
		wallMidTextureGO = createSpritesTextureForType("wallMiddleTexture", TileType.wallMiddle, TileType.wallMiddle, TileType.wallMiddle, TileType.wallMiddle, 0.01f);
		wallTopTextureGO = createSpritesTextureForType("wallTopTexture", TileType.wallTop, TileType.wallBottom, TileType.wallMiddle, TileType.floor, 0.05f);
		placeBonfireAndExit();
	}

	void placeBonfireAndExit() {
		bonfireGO = (GameObject)Instantiate(Resources.Load("prefabs/bonfire"), transform.position, Quaternion.identity);
		bonfireGO.transform.position = new Vector3(gc.map.bonfire.x+0.5f, gc.map.bonfire.y+0.5f, 0);

		exitGO = (GameObject)Instantiate(Resources.Load("prefabs/exit"), transform.position, Quaternion.identity);
		exitGO.transform.position = new Vector3(gc.map.exit.x+0.5f, gc.map.exit.y+0.5f, 0);
	}

	GameObject createSpritesTextureForType(string name, TileType type, TileType neighbour1, TileType neighbour2, TileType neighbour3, float height) {
		GameObject go = new GameObject(name);
		go.transform.SetParent(this.transform);
		go.transform.position = new Vector3(gc.map.width/2+0.5f, gc.map.height/2+0.5f, -height);
		go.transform.localRotation = new Quaternion(0f, 180f, 0f, 0f);

		MeshFilter mf = go.AddComponent<MeshFilter>();
		MeshRenderer mr = go.AddComponent<MeshRenderer>();

		Material mat = new Material(material);//new Material(Shader.Find("Unlit/Transparent Cutout"));
		Texture2D tex = new Texture2D(gc.map.width*pixelsPerUnit, gc.map.height*pixelsPerUnit);
		tex.filterMode = FilterMode.Point;

		tex = addSpritesForType(tex, type, neighbour1, neighbour2, neighbour3);

		tex.Apply();
		mat.mainTexture = tex;

		mf.mesh = CreateMesh(gc.map.width, gc.map.height);
		mr.material = mat;

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

	Mesh CreateMesh(float width, float height) {
		Mesh m = new Mesh();
		m.name = "ScriptedMesh";
		m.vertices = new Vector3[] {
			new Vector3(-width/2, -height/2, -0.01f),
			new Vector3(width/2, -height/2, -0.01f),
			new Vector3(width/2, height/2, -0.01f),
			new Vector3(-width/2, height/2, -0.01f)
		};
		m.uv = new Vector2[] {
			new Vector2(1, 0),
			new Vector2(0, 0),
			new Vector2(0, 1),
			new Vector2(1, 1)
		};
		m.triangles = new int[] { 0, 1, 2, 0, 2, 3};
		m.RecalculateNormals();

		return m;
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

	void createFloorTileGOs() {
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
				tile.registerBeenDiscoveredCallback(revealTile);
			}
		}
	}

	void createWallTileGOs() {
		for (int x = 0; x < gc.map.width; x++) {
			for (int y = 0; y < gc.map.height; y++) {
				Tile tile = gc.map.getTileAt(x, y);
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
					if (!go_fogMap.ContainsKey(go)) {
						go_fogMap.Add(go, fog_go);
					}
				}
				tile.registerBeenDiscoveredCallback(revealTile);
			}
		}
	}

	void removeFloorGOUnderWall(Tile tile) {
		if (go_tileMap.ContainsKey(tile)) {
			Destroy(go_tileMap[tile]);
			go_tileMap.Remove(tile);
		}
	}

	void updateFloorTileSprites() {
		Dictionary<string, Sprite> roomSprites = new Dictionary<string, Sprite>();
		foreach (var sprite in Resources.LoadAll<Sprite>("sprites/rooms/")) {
			roomSprites.Add(sprite.name, sprite);
		}

		foreach (var tile in go_tileMap.Keys) {
			if (tile.type == TileType.floor) {
				SpriteRenderer go_sr = go_tileMap[tile].GetComponent<SpriteRenderer>();

				TileType neighbourType1 = TileType.wallBottom;
				TileType neighbourType2 = TileType.wallMiddle;
				TileType neighbourType3 = TileType.wallTop;

				int tileIndex = getCrossTileIndex(// Above, left, below, right
					                neighbourType1, neighbourType2, neighbourType3,
					                gc.map.getTileAt(tile.x, tile.y+1),
					                gc.map.getTileAt(tile.x-1, tile.y),
					                gc.map.getTileAt(tile.x, tile.y-1), 
					                gc.map.getTileAt(tile.x+1, tile.y));

				if (tileIndex == 0) {
					// Diagonal tiles check
					tileIndex = 16+getCrossTileIndex(// AboveLeft, BelowLeft, BelowRight, AboveRight
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
	}

	void updateWallTileSprites() {
		Dictionary<string, Sprite> roomSprites = new Dictionary<string, Sprite>();
		foreach (var sprite in Resources.LoadAll<Sprite>("sprites/rooms/")) {
			roomSprites.Add(sprite.name, sprite);
		}

		foreach (var tile in go_tileMap.Keys) {
			if (tile.type == TileType.wallBottom || tile.type == TileType.wallMiddle || tile.type == TileType.wallTop) {

				SpriteRenderer go_sr = go_tileMap[tile].GetComponent<SpriteRenderer>();
				int tileIndex;

				if (tile.type == TileType.wallBottom || tile.type == TileType.wallMiddle) {
					tileIndex = getSideTileIndex( // left, right
						tile.type, TileType.wallTop, 
						gc.map.getTileAt(tile.x+1, tile.y),
						gc.map.getTileAt(tile.x-1, tile.y));
				} else {
					TileType neighbourType1 = TileType.floor;
					TileType neighbourType2 = TileType.wallMiddle;
					TileType neighbourType3 = TileType.wallBottom;

					tileIndex = getCrossTileIndex( // Above, left, below, right
						neighbourType1, neighbourType2, neighbourType3,
						gc.map.getTileAt(tile.x, tile.y+1),
						gc.map.getTileAt(tile.x-1, tile.y),
						gc.map.getTileAt(tile.x, tile.y-1), 
						gc.map.getTileAt(tile.x+1, tile.y));
					if (tileIndex == 0)
						// Diagonal tiles check
						tileIndex = 16+getCrossTileIndex( // AboveLeft, BelowLeft, BelowRight, AboveRight
							neighbourType1, neighbourType2, neighbourType3,
							gc.map.getTileAt(tile.x-1, tile.y+1),
							gc.map.getTileAt(tile.x-1, tile.y-1),
							gc.map.getTileAt(tile.x+1, tile.y-1),
							gc.map.getTileAt(tile.x+1, tile.y+1));
				}

				// Adjust bounding box for top walls
				if (tile.type == TileType.wallTop) {
					if (tileIndex%2 == 1) {
						BoxCollider2D box = go_tileMap[tile].GetComponent<BoxCollider2D>();
						Vector2 boxOffset = box.offset;
						Vector2 boxSize = box.size;
						boxOffset.y = -0.375f;
						boxSize.y = 0.25f;
						box.size = boxSize;
						box.offset = boxOffset;
					}
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
	}


	// ======================
	// TODO: GENERAL OVERHAUL
	// ======================

	int getSideTileIndex(TileType type1, TileType type2, Tile left, Tile right) {
		var sum = 0;
		if (left != null && (left.type == type1 || left.type == type2))  sum += 1;
		if (right != null && (right.type == type1 || right.type == type2)) sum += 2;
		return sum;
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
