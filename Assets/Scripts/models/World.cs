using Delaunay.Geo;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class World {

	public int width  { get; protected set; }
	public int height { get; protected set; }

	float minX = 0f;
	float minY = 0f;

	Tile[,] tileMap;
	List<Room> rooms;
	List<LineSegment> corridors;

	Vector2 playerPosition;

	public World(List<Room> rooms, List<LineSegment> corridors) {
		this.rooms = rooms;
		this.corridors = corridors;

		getWorldDimentions();
		tileMap = new Tile[width, height];
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				tileMap[x,y] = new Tile();
			}
		}

		if (rooms != null) {
			layoutRooms();
			setStartingPosition();
		}
		if (corridors != null) {
			layoutCorridors();
		}


	}

	public Tile getTileAt(int x, int y) {
		return tileMap[x,y];
	}

	public Vector2 getPlayerPosition() {
		return playerPosition;
	}

	// TEMPORARY
	void setStartingPosition() {
		int rand = Random.Range(0, rooms.Count);
		playerPosition = new Vector2(rooms[rand].position.x, rooms[rand].position.y);
	}


	void layoutRooms() {
		foreach (var room in rooms) {
			int roomBaseX = (int)(room.position.x-(room.size.x/2));
			int roomBaseY = (int)(room.position.y-(room.size.y/2));
			for (int x = 0; x < room.size.x; x++) {
				for (int y = 0; y < room.size.y; y++) {
					if (roomBaseX+x >= width || roomBaseX+x < 0 || roomBaseY+y >= height || roomBaseY+y < 0){
						Debug.Log((roomBaseX+x)+" "+(roomBaseY+y));
					}
					if (x == 0 || x == room.size.x-1 || y == 0 || y == room.size.y-1) {
						tileMap[roomBaseX+x, roomBaseY+y].type = TileType.wall;
					} else {
						tileMap[roomBaseX+x, roomBaseY+y].type = TileType.floor;
					}
				}
			}
		}
	}

	void layoutCorridors() {
		foreach (var corridor in corridors) {
			Vector2 corrStart = (Vector2)corridor.p0;
			Vector2 corrEnd = (Vector2)corridor.p1;

			corrStart.x += (int)(Mathf.Abs(minX));
			corrStart.y += (int)(Mathf.Abs(minY));
			corrEnd.x += (int)(Mathf.Abs(minX));
			corrEnd.y += (int)(Mathf.Abs(minY));

			if (corrStart.x == corrEnd.x) {
				if (corrStart.y > corrEnd.y) {
					for (int y = (int)corrEnd.y; y < corrStart.y; y++) {
						setCorridor((int)corrStart.x, y, false);
					}
				} else {
					for (int y = (int)corrStart.y; y < corrEnd.y; y++) {
						setCorridor((int)corrStart.x, y, false);
					}
				}
			} else if (corrStart.y == corrEnd.y) {
				if (corrStart.x > corrEnd.x) {
					for (int x = (int)corrEnd.x; x < corrStart.x; x++) {
						setCorridor(x, (int)corrStart.y, true);
					}
				} else {
					for (int x = (int)corrStart.x; x < corrEnd.x; x++) {
						setCorridor(x, (int)corrStart.y, true);
					}
				}
			}
		}
	}

	void setCorridor(int x, int y, bool dirX) {
		if (!dirX) {
			if (tileMap[x-1,y].type == TileType.wall || tileMap[x-1,y].type == TileType.empty)
				tileMap[x-1,y].type = TileType.wall;
			if (tileMap[x+1,y].type == TileType.wall || tileMap[x+1,y].type == TileType.empty)
				tileMap[x+1,y].type = TileType.wall;

			tileMap[x,y].type = TileType.floor;
			if (tileMap[x,y+1].type == TileType.wall || tileMap[x,y+1].type == TileType.empty)
				tileMap[x,y+1].type = TileType.wall;
			if (tileMap[x-1,y+1].type == TileType.wall || tileMap[x-1,y+1].type == TileType.empty)
				tileMap[x-1,y+1].type = TileType.wall;
			if (tileMap[x+1,y+1].type == TileType.wall || tileMap[x+1,y+1].type == TileType.empty)
				tileMap[x+1,y+1].type = TileType.wall;
		} else {
			if (tileMap[x,y-1].type == TileType.wall || tileMap[x,y-1].type == TileType.empty)
				tileMap[x,y-1].type = TileType.wall;
			if (tileMap[x,y+1].type == TileType.wall || tileMap[x,y+1].type == TileType.empty)
				tileMap[x,y+1].type = TileType.wall;

			tileMap[x,y].type = TileType.floor;
			if (tileMap[x+1,y].type == TileType.wall || tileMap[x+1,y].type == TileType.empty)
				tileMap[x+1,y].type = TileType.wall;
			if (tileMap[x+1,y-1].type == TileType.wall || tileMap[x+1,y-1].type == TileType.empty)
				tileMap[x+1,y-1].type = TileType.wall;
			if (tileMap[x+1,y+1].type == TileType.wall || tileMap[x+1,y+1].type == TileType.empty)
				tileMap[x+1,y+1].type = TileType.wall;
		}
	}

	void getWorldDimentions() {		
		float maxX = 0f;
		float maxY = 0f;

		foreach (var room in rooms) {
			float top = room.position.y+room.size.y/2;
			float right = room.position.x+room.size.x/2;

			if (top > maxY) { maxY = top; }
			if (right > maxX) { maxX = right; }
		}

		//Debug.Log("minX: "+minX+" maxX: "+maxX+" minY: "+minY+" maxY: "+maxY);

		width = Mathf.CeilToInt(maxX);
		height = Mathf.CeilToInt(maxY);

		Debug.Log("Width: "+width+ " Height: "+height);
	}
}