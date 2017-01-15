using UnityEngine;
using System.Collections.Generic;

public class Room {

	public int id;
	public Vector2 size;
	public Vector2 position;

	public List<Room> connectedRooms;

	public Room(int id, int width, int height, Vector2 position) {
		connectedRooms = new List<Room>();

		this.size = new Vector2(width, height);
		this.position = position;
	}
}
