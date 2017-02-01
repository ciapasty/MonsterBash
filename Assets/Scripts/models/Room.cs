using UnityEngine;
using System.Collections.Generic;

public enum RoomType { bonfire, generic, exit }

public class Room : Area {

	public int width { get; protected set; }
	public int height { get; protected set; }
	public Vector2 center;

	public Tile roomBase { get; protected set; }
	public List<Door> doors { get; protected set; }
	public List<Room> connectedRooms;
	public List<Enemy> enemies { get; protected set; }

	public RoomType? type { get; protected set; }

	public Room(int ID, int width, int height, Vector2 center) : base(ID) {
		this.width = width;
		this.height = height;
		this.center = center;
		this.type = null;

		doors = new List<Door>();
		connectedRooms = new List<Room>();
		enemies = new List<Enemy>();
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

	public void addEnemy(Enemy enemy) {
		enemies.Add(enemy);
	}

	public void lockDoors(bool locked) {
		foreach (var door in doors) {
			door.isLocked = locked;
		}
	}
}
