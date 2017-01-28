using UnityEngine;
using System;
using System.Collections;

// To be extended
public enum TileType {empty, floor, wall}
// ??
public enum TileClass {room, corridor, door}

public class Tile {

	public int x { get; protected set; }
	public int y { get; protected set; }

	public TileType type;
	public TileClass tClass;
	public Room room { get; protected set; }
	public Corridor corridor { get; protected set; }

	public Tile(int x, int y) {
		this.x = x;
		this.y = y;
	}

	public void setRoom(Room room) {
		this.room = room;
	}

	public void setCorridor(Corridor corridor) {
		this.corridor = corridor;
	}

}
