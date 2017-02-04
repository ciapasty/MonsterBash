using Delaunay.Geo;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Map {

	public int width  { get; protected set; }
	public int height { get; protected set; }

	Tile[,] tileMap;

	public List<Room> rooms { get; protected set; }
	public List<Corridor> corridors { get; protected set; }

	public Tile bonfire { get; protected set; }

	public Map(int width, int height) {
		this.width = width;
		this.height = height;

		tileMap = new Tile[width, height];
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				tileMap[x,y] = new Tile(x, y);
			}
		}

		rooms = new List<Room>();
		corridors = new List<Corridor>();
	}

	public Tile getTileAt(int x, int y) {
		if (x >= width || x < 0 || y >= height || y < 0) {
			//Debug.Log("Tried to fetch tile: ("+x+", "+y+") in range: "+width+", "+height);
			return null;
		}
		return tileMap[x,y];
	}

	public void addRoom(Room room) {
		rooms.Add(room);
	}

	public void addCorridor(Corridor corridor) {
		corridors.Add(corridor);
	}

	public Room getRoomWithID(int ID) {
		Room room;
		for (int i = 0; i < rooms.Count; i++) {
			room = rooms[i];
			if (room.ID == ID)
				return room;
		}
		return null;
	}

	public Corridor getCorridorWithID(int ID) {
		Corridor corridor;
		for (int i = 0; i < corridors.Count; i++) {
			corridor = corridors[i];
			if (corridor.ID == ID)
				return corridor;
		}
		return null;
	}

	public void setSpawnTileTo(Tile tile) {
		bonfire = tile;
	}
}