using UnityEngine;
using System.Collections;

public enum TileType {empty, floor, wall}
public enum TileClass {room, corridor}

public class Tile {

	public TileType type;
	public TileClass tClass;

	public Tile() {}

}
