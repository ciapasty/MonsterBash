using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds spawn tile and prefab information for object/enemy
/// </summary>
public class Blueprint {
	public Tile spawnTile;
	public GameObject prefab;

	public Blueprint(GameObject prefab, Tile spawnTile) {
		this.prefab = prefab;
		this.spawnTile = spawnTile;
	}
}
