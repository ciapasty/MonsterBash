using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapControl : MonoBehaviour {

	public GameObject tile_prefab;
	GameController gc;

	Dictionary<Tile, GameObject> tile_PanelMap;

	public Color wallColor;
	public Color floorColor;
	public Color doorColor;
	public Color playerColor;
	Color seeThrough;

	// Use this for initialization
	void Start () {
		gc = GameController.Instance;

		seeThrough = Color.white;
		seeThrough.a = 0.0f;
	}

	public void setupMiniMap() {
		Rect rect = GetComponent<RectTransform>().rect;
		GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, gc.map.width);
		GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, gc.map.height);

		tile_PanelMap = new Dictionary<Tile, GameObject>();

		for (int x = 0; x < gc.map.width; x++) {
			for (int y = 0; y < gc.map.height; y++) {
				addMapPanel(x, y);
			}
		}

		updateMiniMap();
	}

	public void updateMiniMap() {
		for (int x = 0; x < gc.map.width; x++) {
			for (int y = 0; y < gc.map.height; y++) {
				Tile tile = gc.map.getTileAt(x, y);
				GameObject panel = tile_PanelMap[tile];
				UnityEngine.UI.Image panelImage = panel.GetComponent<UnityEngine.UI.Image>();

				if (tile == gc.currTile) {
					panelImage.color = playerColor;
				} else {
					switch (gc.map.getTileAt(x, y).type) {
					case TileType.outerWall:
						panelImage.color = wallColor; 
						break;
					case TileType.floor:
						panelImage.color = floorColor;
						break;
					case TileType.empty:
						panelImage.color = seeThrough;
						break;
					}
				}

				if (tile.room != null && !tile.room.isDiscovered) {
					panelImage.color = seeThrough;
				}

				if (tile.corridor != null && !tile.corridor.isDiscovered) {
					panelImage.color = seeThrough;
				}
			}
		}
	}

	public void updateTile(Tile tile) {
		GameObject panel = tile_PanelMap[tile];
		UnityEngine.UI.Image panelImage = panel.GetComponent<UnityEngine.UI.Image>();

		if (!tile.isDiscovered) {
			panelImage.color = seeThrough;
			return;
		}

		if (tile == gc.currTile) {
			panelImage.color = playerColor;
		} else {
			switch (tile.type) {
			case TileType.outerWall:
				panelImage.color = wallColor; 
				return;
			case TileType.floor:
				panelImage.color = floorColor;
				return;
			case TileType.empty:
				panelImage.color = seeThrough;
				return;
			}
		}
	}

	void addMapPanel(int x, int y) {
		GameObject panel_go = (GameObject)Instantiate(tile_prefab, transform);
		panel_go.name = "map_panel_"+x+"_"+y;
		panel_go.transform.SetParent(transform);
		panel_go.transform.position = new Vector3(0, 0, 0);
		panel_go.transform.localScale = new Vector3(1f, 1f, 1f);
		tile_PanelMap.Add(gc.map.getTileAt(x, y), panel_go);
	}
}
