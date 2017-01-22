﻿using UnityEngine;
using System.Collections.Generic;

public enum RoomType { bonfire, generic, exit }

public class Room {

	public int? id { get; protected set; }
	public int width { get; protected set; }
	public int height { get; protected set; }
	public RoomType? type { get; protected set; }
	public List<Door> doors { get; protected set; }

	public Vector2 center;

	public Tile roomBase { get; protected set; }

	public List<Room> connectedRooms;

	public Room(int id, int width, int height, Vector2 center) {
		connectedRooms = new List<Room>();
		doors = new List<Door>();

		this.id = id;
		this.width = width;
		this.height = height;
		this.center = center;

		this.type = null;
	}

	public void setBaseTileTo(Tile tile) {
		roomBase = tile;
	}

	public void setRoomType(RoomType? type) {
		this.type = type;
	}

	public void addDoor(Door door) {
		doors.Add(door);
	}

	public void doorsLocked(bool locked) {
		foreach (var door in doors) {
			door.isLocked = locked;
		}
	}
}
