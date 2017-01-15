using UnityEngine;
using System.Collections;

public class WorldController : MonoBehaviour {

	public World world;

	public GameObject wallPrefab;
	public GameObject floorPrefab;
	
	// Update is called once per frame
	void Update () {
		
	}

	public void generateTiles() {
		for (int x = 0; x < world.width; x++) {
			for (int y = 0; y < world.height; y++) {
				if (world.getTileAt(x, y).type == TileType.floor) {
					GameObject tile = (GameObject)Instantiate(floorPrefab, transform.position, Quaternion.identity);
					tile.transform.SetParent(this.transform);
					tile.transform.position = new Vector2(x+0.5f, y+0.5f);
				}
				if (world.getTileAt(x, y).type == TileType.wall) {
					GameObject tile = (GameObject)Instantiate(wallPrefab, transform.position, Quaternion.identity);
					tile.transform.SetParent(this.transform);
					tile.transform.position = new Vector2(x+0.5f, y+0.5f);
				}
			}
		}
	}
}
