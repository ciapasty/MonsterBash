using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapControl : MonoBehaviour {

	public GameObject tile_prefab;
	GameController gc;

	Dictionary<Tile, GameObject> tile_PanelMap;
	GameObject playerPanel;

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

	public void updateTile(Tile tile) {
		GameObject panel = tile_PanelMap[tile];
		UnityEngine.UI.Image panelImage = panel.GetComponent<UnityEngine.UI.Image>();

		panelImage.enabled = true;
	}

	public void movePlayerTile(Tile tile) {
		RectTransform panel_rect = playerPanel.GetComponent<RectTransform>();
		panel_rect.anchoredPosition = new Vector3(tile.x, tile.y, 0f);
	}

	public void setupMiniMap() {
		Rect rect = GetComponent<RectTransform>().rect;
		GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, gc.map.width);
		GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, gc.map.height);

		tile_PanelMap = new Dictionary<Tile, GameObject>();

		for (int x = 0; x < gc.map.width; x++) {
			for (int y = 0; y < gc.map.height; y++) {
				if (gc.map.getTileAt(x,y).type != TileType.empty)
					addMapPanel(gc.map.getTileAt(x,y));
			}
		}
		createPlayerPanel();

		updateMiniMap();
	}

	public void updateMiniMap() {
		foreach (var tile in tile_PanelMap.Keys) {
			GameObject panel = tile_PanelMap[tile];
			UnityEngine.UI.Image panelImage = panel.GetComponent<UnityEngine.UI.Image>();

			switch (tile.type) {
			case TileType.wallTop:
				panelImage.color = wallColor; 
				break;
			case TileType.floor:
				panelImage.color = floorColor;
				break;
			case TileType.empty:
				panelImage.color = seeThrough;
				break;
			}

			if (tile.room != null && !tile.room.isDiscovered) {
				panelImage.enabled = false;
			}

			if (tile.corridor != null && !tile.corridor.isDiscovered) {
				panelImage.enabled = false;
			}
		}
	}

	void addMapPanel(Tile tile) {
		GameObject panel_go = (GameObject)Instantiate(tile_prefab, transform);
		panel_go.name = "map_panel_"+tile.x+"_"+tile.y;
		panel_go.transform.SetParent(transform);
		RectTransform panel_rect = panel_go.GetComponent<RectTransform>();
		panel_rect.anchoredPosition = new Vector3(tile.x, tile.y, 0);
		panel_rect.sizeDelta = new Vector2(1f, 1f);
		panel_go.transform.localScale = new Vector3(1f, 1f, 1f);
		tile_PanelMap.Add(gc.map.getTileAt(tile.x, tile.y), panel_go);
	}

	void createPlayerPanel() {
		playerPanel = (GameObject)Instantiate(tile_prefab, transform);
		playerPanel.name = "player_panel";
		playerPanel.transform.SetParent(transform);
		RectTransform panel_rect = playerPanel.GetComponent<RectTransform>();
		panel_rect.anchoredPosition = Vector3.zero;
		panel_rect.sizeDelta = new Vector2(1f, 1f);
		playerPanel.transform.localScale = new Vector3(1f, 1f, 1f);

		UnityEngine.UI.Image panelImage = playerPanel.GetComponent<UnityEngine.UI.Image>();
		panelImage.color = playerColor;
	}
}
