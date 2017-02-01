using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds information about enemy
/// </summary>
public class Enemy {
	public Tile spawnTile;
	public GameObject prefab;

	public Enemy(GameObject prefab, Tile spawnTile) {
		this.prefab = prefab;
		this.spawnTile = spawnTile;
	}
}
