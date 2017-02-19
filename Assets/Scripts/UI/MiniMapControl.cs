using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapControl : MonoBehaviour {

	GameController gc;

	public Color wallColor;
	public Color wallMiddleColor;
	public Color floorColor;
	public Color doorColor;
	public Color playerColor;
	Color seeThrough;

	Texture2D mapTexture;
	Material mapMateral;
	CanvasRenderer render;

	// Use this for initialization
	void Start () {
		gc = GameController.Instance;
		render = GetComponent<CanvasRenderer>();

		seeThrough = Color.white;
		seeThrough.a = 0.0f;
	}

	public void updateTile(Tile tile) {
		if (tile.isDiscovered) {
			if (tile == gc.currTile) {
				mapTexture.SetPixel(tile.x, tile.y, playerColor);
				return;
			}

			switch (tile.type) {
			case TileType.floor:
				mapTexture.SetPixel(tile.x, tile.y, floorColor);
				break;
			case TileType.wallBottom:
			case TileType.wallMiddle:
				mapTexture.SetPixel(tile.x, tile.y, wallMiddleColor);
				break;
			case TileType.wallTop:
				mapTexture.SetPixel(tile.x, tile.y, wallColor);
				break;
			case TileType.empty:
			default:
				mapTexture.SetPixel(tile.x, tile.y, seeThrough);
				break;
			}
		} else {
			mapTexture.SetPixel(tile.x, tile.y, seeThrough);
		}
		mapTexture.Apply();
	}

	public void setupMiniMap() {
		RectTransform rectT = GetComponent<RectTransform>();
		rectT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, gc.map.width);
		rectT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, gc.map.height);

		mapTexture = new Texture2D(gc.map.width, gc.map.height);
		mapTexture.filterMode = FilterMode.Point;

		for (int x = 0; x < gc.map.width; x++) {
			for (int y = 0; y < gc.map.height; y++) {
				updateTile(gc.map.getTileAt(x, y));
			}
		}

		mapTexture.Apply();
		render.SetTexture(mapTexture);
	}
}
