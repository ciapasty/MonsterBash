using Delaunay.Geo;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Map {

	public int width  { get; protected set; }
	public int height { get; protected set; }

	Tile[,] tileMap;
	public List<Room> rooms { get; protected set; }

	public Tile bonfire { get; protected set; }

	public Map(int width, int height, List<Room> rooms) {
		this.width = width;
		this.height = height;
		this.rooms = rooms;

		tileMap = new Tile[width, height];
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				tileMap[x,y] = new Tile(x, y);
			}
		}

		if (rooms != null) {
			layoutRooms();
		}
	}

	public Tile getTileAt(int x, int y) {
		if (x >= width || y >= height) {
			Debug.LogError("Tried to fetch tile: ("+x+", "+y+") in range: "+width+", "+height);
			return null;
		}
		return tileMap[x,y];
	}

	public Room getRoomWithID(int id) {
		Room room;
		for (int i = 0; i < rooms.Count; i++) {
			room = rooms[i];
			if (room.id == id)
				return room;
		}
		return null;
	}

	public void setSpawnTileTo(Tile tile) {
		bonfire = tile;
	}

	void layoutRooms() {
		foreach (var room in rooms) {
			int roomBaseX = Mathf.CeilToInt(room.center.x)-(room.width/2);
			int roomBaseY = Mathf.CeilToInt(room.center.y)-(room.height/2);
			room.setBaseTileTo(tileMap[roomBaseX,roomBaseY]);
			for (int x = 0; x < room.width; x++) {
				for (int y = 0; y < room.height; y++) {
					if (roomBaseX+x >= width || roomBaseX+x < 0 || roomBaseY+y >= height || roomBaseY+y < 0){
						Debug.LogError("Tile ("+(roomBaseX+x)+", "+(roomBaseY+y)+") is out of map bounds ("+width+". "+height+")");
					}
					Tile tile = getTileAt(room.roomBase.x+x,room.roomBase.y+y);
					tile.setRoom(room.id);
					if (x == 0 || x == room.width-1 || y == 0 || y == room.height-1) {
						tile.type = TileType.wall;
					} else {
						tile.type = TileType.floor;
					}
				}
			}
		}
	}
}