using Delaunay.Geo;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Map {

	public int width  { get; protected set; }
	public int height { get; protected set; }

//	float minX = 0f;
//	float minY = 0f;

	Tile[,] tileMap;
	public List<Room> rooms;
	//List<LineSegment> corridors;

	//Vector2 playerPosition;

	public Map(int width, int height, List<Room> rooms) {
		this.width = width;
		this.height = height;
		this.rooms = rooms;
		//this.corridors = corridors;
		
		tileMap = new Tile[width, height];
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				tileMap[x,y] = new Tile(x, y);
			}
		}

		if (rooms != null) {
			layoutRooms();
			//setStartingPosition();
		}
//		if (corridors != null) {
//			layoutCorridors();
//		}
	}

	public Tile getTileAt(int x, int y) {
//		if (x >= width || y >= height) {
//			Debug.LogError("Tried to fetch tile: ("+x+", "+y+") in range: "+width+", "+height);
//		}
//		Debug.Log("Tried to fetch tile: ("+x+", "+y+") in range: "+width+", "+height);
		return tileMap[x,y];
	}

	/*public Vector2 getPlayerPosition() {
		return playerPosition;
	}

	// TEMPORARY
	void setStartingPosition() {
		int rand = Random.Range(0, rooms.Count);
		playerPosition = new Vector2(rooms[rand].center.x, rooms[rand].center.y);
	}*/


	void layoutRooms() {
		foreach (var room in rooms) {
			int roomBaseX = Mathf.CeilToInt(room.center.x)-(room.width/2);
			int roomBaseY = Mathf.CeilToInt(room.center.y)-(room.height/2);
			room.setBaseTileTo(tileMap[roomBaseX,roomBaseY]);
			for (int x = 0; x < room.width; x++) {
				for (int y = 0; y < room.height; y++) {
//					if (roomBaseX+x >= width || roomBaseX+x < 0 || roomBaseY+y >= height || roomBaseY+y < 0){
//						Debug.Log(width+" "+height);
//						Debug.Log((roomBaseX+x)+" "+(roomBaseY+y));
//					}
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

//	void layoutCorridors() {
//		foreach (var corridor in corridors) {
//			Vector2 corrStart = (Vector2)corridor.p0;
//			Vector2 corrEnd = (Vector2)corridor.p1;
//
//			corrStart.x += (int)(Mathf.Abs(minX));
//			corrStart.y += (int)(Mathf.Abs(minY));
//			corrEnd.x += (int)(Mathf.Abs(minX));
//			corrEnd.y += (int)(Mathf.Abs(minY));
//
//			if (corrStart.x == corrEnd.x) {
//				if (corrStart.y > corrEnd.y) {
//					for (int y = (int)corrEnd.y; y < corrStart.y; y++) {
//						setCorridor((int)corrStart.x, y, false);
//					}
//				} else {
//					for (int y = (int)corrStart.y; y < corrEnd.y; y++) {
//						setCorridor((int)corrStart.x, y, false);
//					}
//				}
//			} else if (corrStart.y == corrEnd.y) {
//				if (corrStart.x > corrEnd.x) {
//					for (int x = (int)corrEnd.x; x < corrStart.x; x++) {
//						setCorridor(x, (int)corrStart.y, true);
//					}
//				} else {
//					for (int x = (int)corrStart.x; x < corrEnd.x; x++) {
//						setCorridor(x, (int)corrStart.y, true);
//					}
//				}
//			}
//		}
//	}
//
//	void setCorridor(int x, int y, bool dirX) {
//		if (!dirX) {
//			if (tileMap[x-1,y].type == TileType.wall || tileMap[x-1,y].type == TileType.empty)
//				tileMap[x-1,y].type = TileType.wall;
//			if (tileMap[x+1,y].type == TileType.wall || tileMap[x+1,y].type == TileType.empty)
//				tileMap[x+1,y].type = TileType.wall;
//
//			tileMap[x,y].type = TileType.floor;
//			if (tileMap[x,y+1].type == TileType.wall || tileMap[x,y+1].type == TileType.empty)
//				tileMap[x,y+1].type = TileType.wall;
//			if (tileMap[x-1,y+1].type == TileType.wall || tileMap[x-1,y+1].type == TileType.empty)
//				tileMap[x-1,y+1].type = TileType.wall;
//			if (tileMap[x+1,y+1].type == TileType.wall || tileMap[x+1,y+1].type == TileType.empty)
//				tileMap[x+1,y+1].type = TileType.wall;
//		} else {
//			if (tileMap[x,y-1].type == TileType.wall || tileMap[x,y-1].type == TileType.empty)
//				tileMap[x,y-1].type = TileType.wall;
//			if (tileMap[x,y+1].type == TileType.wall || tileMap[x,y+1].type == TileType.empty)
//				tileMap[x,y+1].type = TileType.wall;
//
//			tileMap[x,y].type = TileType.floor;
//			if (tileMap[x+1,y].type == TileType.wall || tileMap[x+1,y].type == TileType.empty)
//				tileMap[x+1,y].type = TileType.wall;
//			if (tileMap[x+1,y-1].type == TileType.wall || tileMap[x+1,y-1].type == TileType.empty)
//				tileMap[x+1,y-1].type = TileType.wall;
//			if (tileMap[x+1,y+1].type == TileType.wall || tileMap[x+1,y+1].type == TileType.empty)
//				tileMap[x+1,y+1].type = TileType.wall;
//		}
//	}
}