using UnityEngine;
using System.Collections.Generic;

public enum RoomType { bonfire, exit, generic, library }

public class Room : Area {

	public Vector2 center;

	public Tile roomBase { get; protected set; }
	public List<Door> doors { get; protected set; }
	public List<Room> connectedRooms;
	public List<Blueprint> enemies { get; protected set; }
	public List<Blueprint> objects { get; protected set; }

	public RoomTemplate tp { get; protected set; }

	public Room(int ID, RoomTemplate template, Vector2 center) : base(ID) {
		this.tp = template;
		this.center = center;

		doors = new List<Door>();
		connectedRooms = new List<Room>();
		enemies = new List<Blueprint>();
		objects = new List<Blueprint>();
	}

	~Room() {
		doors = null;
		connectedRooms = null;
		enemies = null;
	}

	public void setBaseTileTo(Tile tile) {
		roomBase = tile;
	}

//	public void setRoomType(RoomType type) {
//		this.tp.type = type;
//	}

	public void addDoor(Door door) {
		doors.Add(door);
	}

	public void addEnemy(Blueprint enemy) {
		enemies.Add(enemy);
	}

	public void addObject(Blueprint obj) {
		objects.Add(obj);
	}
}
