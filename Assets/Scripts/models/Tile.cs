using UnityEngine;
using System;
using System.Collections;

// To be extended
public enum TileType {empty, floor, wallTop, wallMiddle, wallBottom}
// ??
public enum TileClass {room, corridor, door}

public class Tile {

	public int x { get; protected set; }
	public int y { get; protected set; }

	public bool hasContent = false;
	protected bool _isDiscovered = false;
	public bool isDiscovered { 
		get {
			return _isDiscovered;
		}
		set {
			if (value && !_isDiscovered) {
				_isDiscovered = value;
				if (cbIsDiscovered != null)
					cbIsDiscovered(this);
			}
		}
	}

	Action<Tile> cbIsDiscovered;

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

	public void registerBeenDiscoveredCallback(Action<Tile> callback) {
		cbIsDiscovered += callback;
	}

	public void unregisterBeenDiscoveredCallback(Action<Tile> callback) {
		cbIsDiscovered -= callback;
	}

}
