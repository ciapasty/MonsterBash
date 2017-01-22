using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSpriteController : MonoBehaviour {

	public bool areSpritesSetup { get; protected set;}

	public GameObject wallPrefab;
	public GameObject floorPrefab;

	GameController gc;

	Dictionary<GameObject, Tile> go_tileMap;

	void OnEnable() {
		areSpritesSetup = false;
	}

	void Start() {
		gc = GameController.Instance;
		go_tileMap = new Dictionary<GameObject, Tile>();
	}

	public void setupSprites() {
		createBasicGOs();
		updateSprites();
		placeBonfire();
		areSpritesSetup = true;
	}

	void createBasicGOs() {
		for (int x = 0; x < gc.map.width; x++) {
			for (int y = 0; y < gc.map.height; y++) {
				Tile tile = gc.map.getTileAt(x, y);
				if (tile.type == TileType.floor) {
					GameObject go = (GameObject)Instantiate(floorPrefab, transform.position, Quaternion.identity);
					go.transform.SetParent(this.transform);
					go.transform.position = new Vector2(x+0.5f, y+0.5f);
					go_tileMap.Add(go, tile);
				}
				if (tile.type == TileType.wall) {
					GameObject go = (GameObject)Instantiate(wallPrefab, transform.position, Quaternion.identity);
					go.transform.SetParent(this.transform);
					go.transform.position = new Vector2(x+0.5f, y+0.5f);
					go_tileMap.Add(go, tile);
				}
			}
		}
	}

	void updateSprites() {
		foreach (var go in go_tileMap.Keys) {
			SpriteRenderer go_sr = go.GetComponent<SpriteRenderer>();

			Tile tile = go_tileMap[go];
			if (tile.roomID != -1) {
				switch (gc.map.getRoomWithID(tile.roomID.Value).type) {
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

	void placeBonfire() {
		GameObject go = (GameObject)Instantiate(Resources.Load("prefabs/bonfire"), transform.position, Quaternion.identity);
		go.transform.position = new Vector3(gc.map.bonfire.x+0.5f, gc.map.bonfire.y+0.5f, 0);
	}
}
