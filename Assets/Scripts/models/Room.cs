using UnityEngine;
using System.Collections.Generic;

public class Room {

	public int? id { get; protected set; }
	public int width { get; protected set; }
	public int height { get; protected set; }

	public Vector2 center;

	public Tile roomBase { get; protected set; }

	public List<Room> connectedRooms;

	public Room(int id, int width, int height, Vector2 center) {
		connectedRooms = new List<Room>();

		this.id = id;
		this.width = width;
		this.height = height;
		this.center = center;
	}

	public void setBaseTileTo(Tile tile) {
		roomBase = tile;
	}
}
