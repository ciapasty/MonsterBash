using Delaunay;
using Delaunay.Geo;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour {

	public int roomCount = 30;
	public int mainRoomCount = 6;
	public int maxRoomWidth = 10;
	public int maxRoomHeight = 12;
	public int elipseWidth = 10;
	public int elipseHeight = 10;

	public GameObject room_go;

	public int alignmentIterations = 3;
	public int mainRoomsBuffer = 2;
	public int roomWallMargin = 1;
	public float connectionsAdded = 0.1f;

	public float mainRoomMeanValueMod = 0.8f;

	public bool drawTriangulation = false;
	public bool drawSpanningTree = false;
	public bool drawExtendedTree = false;
	public bool drawFinalTree = false;

	float meanRoomWidth = 0f;
	float meanRoomHeight = 0f;

	float minX = 0f;
	float minY = 0f;

	private Room[] rooms;
	private List<Room> mainRooms;
	private Dictionary<Room, GameObject> roomGoMap;

	private int iterationCount = 1;

	// Delunay
	private List<Vector2> m_points;
	private List<LineSegment> m_spanningTree;
	private List<LineSegment> m_delaunayTriangulation;
	private List<LineSegment> m_extendedTree;

	private List<LineSegment> corridors;

	// World
	public Map map { get; protected set; }

	public bool isFinished { get; protected set; }

	void Awake() {
		isFinished = false;
	}

	/// <summary>
	/// Starts the map generation routine.
	/// </summary>
	public void startMapCreation() {
		generateInitialRooms();
		createRoomGOs();

		Time.timeScale = 5f;
		StartCoroutine(CheckObjectsHaveStopped());
	}

	/// <summary>
	/// Generates all rooms (main and fill).
	/// </summary>
	void generateInitialRooms() {
		rooms = new Room[roomCount];
		mainRooms = new List<Room>();

		for (int i = 0; i < roomCount; i++) {
			int roomWidth = getRoomDimention(maxRoomWidth);
			int roomHeight = getRoomDimention(maxRoomHeight);

			rooms[i] = new Room(i, roomWidth, roomHeight, getRandomPointInElipse(elipseWidth, elipseHeight));

			meanRoomWidth += roomWidth;
			meanRoomHeight += roomHeight;
		}
		meanRoomWidth = meanRoomWidth/roomCount;
		meanRoomHeight = meanRoomHeight/roomCount;

		print(roomCount+" rooms created");

		foreach (var room in rooms) {
			if (room.width > meanRoomWidth*0.8f && room.height > meanRoomHeight*0.8f) {
				mainRooms.Add(room);
			}
		}

		if (mainRooms.Count < mainRoomCount) {
			generateInitialRooms();
		}
	}

	int getRoomDimention(int max) {
		return Mathf.CeilToInt(max*randomGauss());
	}

	/// <summary>
	/// Checks if the physics engine has stopped.
	/// </summary>
	IEnumerator CheckObjectsHaveStopped() {
		Debug.Log(iterationCount+" iteration of room alignment");

		bool allSleeping = false;
		while(!allSleeping)
		{
			allSleeping = true;

			foreach (var room in roomGoMap.Keys) {
				if(!roomGoMap[room].GetComponent<Rigidbody2D>().IsSleeping()) {
					allSleeping = false;
					yield return null;
					break;
				}
			}
		}

		Debug.Log("Rooms not overlaping");

		updateMainRoomsPosition();
		// Move everything to positive coords
		foreach (var room in mainRooms) {
			float bottom = room.center.y-room.height/2;
			float left = room.center.x-room.width/2;

			if (bottom < minY) { minY = bottom; }
			if (left < minX) { minX = left; }
		}

		foreach (var room in mainRooms) {
			room.center.x += -minX;
			room.center.y += -minY;

			Vector2 go_pos = roomGoMap[room].transform.position;
			go_pos.x += -minX;
			go_pos.y += -minY;
		}

		Camera.main.transform.Translate(new Vector3(-minX, -minY, 0));
		createSpanningTree();
	}

	/// <summary>
	/// Rounds main rooms positions to int values.
	/// Restarts CheckObjectsHaveStopped().
	/// </summary>
	void updateMainRoomsPosition() {
		foreach (var room in mainRooms) {
			GameObject go = roomGoMap[room];
			room.center = go.transform.position;
		}
	}

	/// <summary>
	/// Triangulates the rooms and creates spanning tree.
	/// </summary>
	void createSpanningTree() {
		List<uint> colors = new List<uint> ();
		m_points = new List<Vector2> ();

		for (int i = 0; i < mainRooms.Count; i++) {
			colors.Add(0);
			m_points.Add(mainRooms[i].center);
		}

		Delaunay.Voronoi v = new Delaunay.Voronoi(m_points, null, new Rect());

		m_delaunayTriangulation = v.DelaunayTriangulation();
		m_spanningTree = v.SpanningTree(KruskalType.MINIMUM);

		Debug.Log("Triangulation complete");

		createFinalSpanningTree();
	}

	/// <summary>
	/// Creates the final spanning tree and adds connected rooms to each other.
	/// </summary>
	void createFinalSpanningTree() {
		m_extendedTree = new List<LineSegment>(m_spanningTree);

		int i = 0;
		while (i < (m_delaunayTriangulation.Count*connectionsAdded)) {
			LineSegment seg = m_delaunayTriangulation[Random.Range(0, m_delaunayTriangulation.Count)];
			if (!m_spanningTree.Contains(seg)) {
				m_extendedTree.Add(seg);
				i++;
			}
		}

		foreach (var edge in m_extendedTree) {
			Vector2 left = (Vector2)edge.p0;
			Vector2 right = (Vector2)edge.p1;
			foreach (var room in mainRooms) {
				if (room.center == left) {
					foreach (var connRoom in mainRooms) {
						if (connRoom.center == right) {
							if (!room.connectedRooms.Contains(connRoom))
								room.connectedRooms.Add(connRoom);
						}
					}
				} else if (room.center == right) {
					foreach (var connRoom in mainRooms) {
						if (connRoom.center == left) {
							if (!room.connectedRooms.Contains(connRoom))
								room.connectedRooms.Add(connRoom);
						}
					}
				}
			}
		}

		Debug.Log("Final tree created");
		drawFinalTree = true;

		float maxX = 0f;
		float maxY = 0f;

		foreach (var room in mainRooms) {
			float top = (room.center.y+room.height/2)+mainRoomsBuffer*10;
			float right = (room.center.x+room.width/2)+mainRoomsBuffer*10;

			if (top > maxY) { maxY = top; }
			if (right > maxX) { maxX = right; }
		}

		// Layout rooms in newly created tile map.
		map = new Map(Mathf.CeilToInt(maxX)+1, Mathf.CeilToInt(maxY)+1, mainRooms);
		removeRoomGOs();

		generateCorridors();

		addCorridorWalls();

		isFinished = true;
	}

	// Corridor generation

	/// <summary>
	/// Generates corridors to connected rooms.
	/// </summary>
	void generateCorridors() {
		List<Room> done = new List<Room>();
		Tile tile1, tile2, tile3;

		foreach (var r1 in map.rooms) {
			foreach (var r2 in r1.connectedRooms) {
				if (!done.Contains(r2)) {
					int midY = ((r1.roomBase.y+(r1.height/2))+(r2.roomBase.y+(r2.height/2)))/2;
					int midX = ((r1.roomBase.x+(r1.width/2))+(r2.roomBase.x+(r2.width/2)))/2;

					if(isHorizontalCorridor(r1, r2, midY)) {
						if (midX < r1.roomBase.x+r1.width/2) {
							tile2 = map.getTileAt(r1.roomBase.x, midY);
							tile1 = map.getTileAt(r2.roomBase.x+r2.width-1, midY);
						} else {
							tile1 = map.getTileAt(r1.roomBase.x+r1.width-1, midY);
							tile2 = map.getTileAt(r2.roomBase.x, midY);
						}
						if (!crossesRoomHorizontally(tile1, tile2))
							layoutHorizontalCorridor(tile1, tile2);

					} else if (isVerticalCorridor(r1, r2, midX)) {
						if (midY < r1.roomBase.y+r1.height/2) {
							tile2 = map.getTileAt(midX, r1.roomBase.y);
							tile1 = map.getTileAt(midX, r2.roomBase.y+r2.height-1);
						} else {
							tile1 = map.getTileAt(midX, r1.roomBase.y+r1.height-1);
							tile2 = map.getTileAt(midX, r2.roomBase.y);
						}
						if (!crossesRoomVertically(tile1, tile2))
							layoutVerticallCorridor(tile1, tile2);
						
					} else {
						// TODO: Double segment corridors

						tile2 = map.getTileAt(r1.roomBase.x+r1.width/2, r2.roomBase.y+r2.height/2);
						if (r1.roomBase.y > r2.roomBase.y) {
							tile1 = map.getTileAt(r1.roomBase.x+r1.width/2, r1.roomBase.y);
						} else {
							tile1 = map.getTileAt(r1.roomBase.x+r1.width/2, r1.roomBase.y+r1.height-1);
						}
						if (r1.roomBase.x > r2.roomBase.x) {
							tile3 = map.getTileAt(r2.roomBase.x+r2.width-1, r2.roomBase.y+r2.height/2);
						} else {
							tile3 = map.getTileAt(r2.roomBase.x, r2.roomBase.y+r2.height/2);
						}

						if (crossesRoomVertically(tile1, tile2) || crossesRoomHorizontally(tile2, tile3)) {
							tile2 = map.getTileAt(r2.roomBase.x+r2.width/2, r1.roomBase.y+r1.height/2);
							if (r1.roomBase.y > r2.roomBase.y) {
								tile1 = map.getTileAt(r2.roomBase.x+r2.width/2, r2.roomBase.y+r2.height-1);
							} else {
								tile1 = map.getTileAt(r2.roomBase.x+r2.width/2, r2.roomBase.y);
							}
							if (r1.roomBase.x > r2.roomBase.x) {
								tile3 = map.getTileAt(r1.roomBase.x, r1.roomBase.y+r1.height/2);
							} else {
								tile3 = map.getTileAt(r1.roomBase.x+r1.width-1, r1.roomBase.y+r1.height/2);
							}
							if (!(crossesRoomVertically(tile1, tile2) || crossesRoomHorizontally(tile2, tile3))) {
								layoutHorizontalCorridor(tile3, tile2);
								layoutVerticallCorridor(tile2, tile1);
							}
						} else {
							layoutHorizontalCorridor(tile3, tile2);
							layoutVerticallCorridor(tile2, tile1);
						}
					}
				}
			}
			done.Add(r1);
		}
	}

	/// <summary>
	/// Checks if the corridor is horizontal.
	/// </summary>
	/// <returns><c>true</c>, if rooms are lined up horizontally, <c>false</c> otherwise.</returns>
	/// <param name="r1">Room 1.</param>
	/// <param name="r2">Room 2.</param>
	/// <param name="midY">Middle point Y.</param>
	bool isHorizontalCorridor(Room r1, Room r2, int midY) {
		return ((midY-r1.roomBase.y-roomWallMargin) >= 0 && (midY-r1.roomBase.y-roomWallMargin) < r1.height-roomWallMargin*2) 
			&& ((midY-r2.roomBase.y-roomWallMargin) >= 0 && (midY-r2.roomBase.y-roomWallMargin) < r2.height-roomWallMargin*2);
	}

	/// <summary>
	/// Checks if the corridor is vertical.
	/// </summary>
	/// <returns><c>true</c>, if connected rooms are lined up vertically, <c>false</c> otherwise.</returns>
	/// <param name="r1">Room 1.</param>
	/// <param name="r2">Room 2.</param>
	/// <param name="midY">Middle point X.</param>
	bool isVerticalCorridor(Room r1, Room r2, int midX) {
		return ((midX-r1.roomBase.x-roomWallMargin) >= 0 && (midX-r1.roomBase.x-roomWallMargin) < r1.width-roomWallMargin*2) 
			&& ((midX-r2.roomBase.x-roomWallMargin) >= 0 && (midX-r2.roomBase.x-roomWallMargin) < r2.width-roomWallMargin*2);
	}

	/// <summary>
	/// Checks if horizontal corridor crosses any rooms.
	/// </summary>
	/// <returns><c>true</c>, if room was crossed, <c>false</c> otherwise.</returns>
	/// <param name="t1">Corridor start tile.</param>
	/// <param name="t2">Corridor end tile.</param>
	bool crossesRoomHorizontally(Tile t1, Tile t2) {
		int min, max;
		min = (t1.x > t2.x) ? t2.x : t1.x;
		max = (t1.x > t2.x) ? t1.x : t2.x;

		for (int i = min+1; i < max; i++) {
			if (map.getTileAt(i, t1.y).type == TileType.floor || map.getTileAt(i, t1.y).type == TileType.wall ||
				map.getTileAt(i, t1.y+1).type == TileType.floor || map.getTileAt(i, t1.y+1).type == TileType.wall ||
				map.getTileAt(i, t1.y-1).type == TileType.floor || map.getTileAt(i, t1.y-1).type == TileType.wall )
				return true;
		}
		return false;
	}

	/// <summary>
	/// Checks if vertical corridor crosses any rooms.
	/// </summary>
	/// <returns><c>true</c>, if room was crossed, <c>false</c> otherwise.</returns>
	/// <param name="t1">Corridor start tile.</param>
	/// <param name="t2">Corridor end tile.</param>
	bool crossesRoomVertically(Tile t1, Tile t2) {
		int min, max;
		min = (t1.y > t2.y) ? t2.y : t1.y;
		max = (t1.y > t2.y) ? t1.y : t2.y;

		for (int i = min+1; i < max; i++) {
			if (map.getTileAt(t1.x, i).type == TileType.floor || map.getTileAt(t1.x, i).type == TileType.wall ||
				map.getTileAt(t1.x+1, i).type == TileType.floor || map.getTileAt(t1.x+1, i).type == TileType.wall ||
				map.getTileAt(t1.x-1, i).type == TileType.floor || map.getTileAt(t1.x-1, i).type == TileType.wall )
				return true;
		}
		return false;
	}

	void layoutHorizontalCorridor(Tile t1, Tile t2) {
		// TODO: change door status
		if (t1.roomID != null && t1.roomID != -1)
			t1.tClass = TileClass.door;

		if (t2.roomID != null && t2.roomID != -1)
			t2.tClass = TileClass.door;

		int min, max;
		min = (t1.x > t2.x) ? t2.x : t1.x;
		max = (t1.x > t2.x) ? t1.x : t2.x;

		for (int i = min; i <= max; i++) {
			Tile t = map.getTileAt(i, t1.y);
			t.type = TileType.floor;
			if (!t.roomID.HasValue)
				t.setRoom(-1);
		}
	}

	void layoutVerticallCorridor(Tile t1, Tile t2) {
		// TODO: change door status
		if (t1.roomID != null && t1.roomID != -1)
			t1.tClass = TileClass.door;

		if (t2.roomID != null && t2.roomID != -1)
			t2.tClass = TileClass.door;

		int min, max;
		min = (t1.y > t2.y) ? t2.y : t1.y;
		max = (t1.y > t2.y) ? t1.y : t2.y;

		for (int i = min; i <= max; i++) {
			Tile t = map.getTileAt(t1.x, i);
			t.type = TileType.floor;
			if (!t.roomID.HasValue)
				t.setRoom(-1);
		}
	}

	void addCorridorWalls() {
		for (int x = 0; x < map.width; x++) {
			for (int y = 0; y < map.height; y++) {
				Tile t = map.getTileAt(x, y);
				if (t.roomID == -1 && t.type == TileType.floor) {
					if (x+1 < map.width)
						setCorridorWall(map.getTileAt(x+1, y));
					if (x-1 > 0)
						setCorridorWall(map.getTileAt(x-1, y));
					if (y+1 < map.height)
						setCorridorWall(map.getTileAt(x, y+1));
					if (y-1 > 0)
						setCorridorWall(map.getTileAt(x, y-1));
					if (x+1 < map.width && y+1 < map.height)
						setCorridorWall(map.getTileAt(x+1, y+1));
					if (x+1 < map.width && y-1 > 0)
						setCorridorWall(map.getTileAt(x+1, y-1));
					if (x-1 > 0 && y+1 < map.height)
						setCorridorWall(map.getTileAt(x-1, y+1));
					if (x-1 > 0 && y-1 > 0)
						setCorridorWall(map.getTileAt(x-1, y-1));
				}
			}
		}
	}

	void setCorridorWall(Tile tile) {
		if (tile.type == TileType.empty && !tile.roomID.HasValue) {
			tile.setRoom(-1);
			tile.type = TileType.wall;
		}
	}

	/// <summary>
	/// Creates GameObjects with box colliders for all rooms.
	/// </summary>
	void createRoomGOs() {
		roomGoMap = new Dictionary<Room, GameObject>();

		foreach (var room in rooms) {
			GameObject go = (GameObject)Instantiate(room_go, transform.position, Quaternion.identity);
			go.transform.SetParent(this.transform);
			go.transform.position = room.center;
			go.GetComponent<BoxCollider2D>().size = new Vector2(room.width+mainRoomsBuffer, room.height+mainRoomsBuffer);
			roomGoMap.Add(room, go);

			if (mainRooms.Contains(room)) {
				go.GetComponent<DrawBorders>().color = Color.red;
			}
		}

		Debug.Log(mainRooms.Count+" main rooms");
	}

	/// <summary>
	/// Creates GameObjects with box colliders for main rooms only.
	/// </summary>
	void createMainRoomGOs () {
		roomGoMap = new Dictionary<Room, GameObject>();

		foreach (var room in mainRooms) {
			GameObject go = (GameObject)Instantiate(room_go, transform.position, Quaternion.identity);
			go.transform.SetParent(this.transform);
			go.transform.position = room.center;
			go.GetComponent<BoxCollider2D>().size = new Vector2(room.width+mainRoomsBuffer, room.height+mainRoomsBuffer);
			roomGoMap.Add(room, go);

			go.GetComponent<DrawBorders>().color = Color.red;
		}
	}

	/// <summary>
	/// Removes all GameObjects assigned to rooms.
	/// </summary>
	void removeRoomGOs() {
		foreach (var room in roomGoMap.Keys) {
			Destroy(roomGoMap[room]);
		}
		roomGoMap = null;
	}


	/// <summary>
	/// Random point in elipse.
	/// </summary>
	/// <returns>Random point in elipse.</returns>
	/// <param name="elipseWidth">Elipse width.</param>
	/// <param name="elipseHeight">Elipse height.</param>
	Vector2 getRandomPointInElipse(int elipseWidth, int elipseHeight) {
		float t = 2*Mathf.PI*Random.value;
		float u = Random.value+Random.value;
		float r;
		if (u > 1) {
			r = 2-u;
		} else {
			r = u;
		}
		return new Vector2(elipseWidth*r*Mathf.Cos(t)/2, elipseHeight*r*Mathf.Sin(t)/2);
	}

	/// <summary>
	/// Midpoint between specified p1 and p2.
	/// </summary>
	/// <param name="p1">P1.</param>
	/// <param name="p2">P2.</param>
	Vector2 midpoint(Vector2 p1, Vector2 p2) {
		float x = Mathf.Ceil(p1.x+p2.x)/2;
		float y = Mathf.Ceil(p1.y+p2.y)/2;

		if ((x % 1) == 0) {
			x += 0.5f;
		}

		if ((y % 1) == 0) {
			y += 0.5f;
		}

		return new Vector2(x, y);
	}

	public float randomGauss() {
		float rand = Mathf.Abs(nextGaussian());
		return ((rand < 1 && rand > 0.6) || (rand < 0.2 && rand > 0.1)) ? rand : randomGauss();
	}

	public float nextGaussian() {
		float u, v, S;

		do
		{
			u = 2.0f * Random.value - 1.0f;
			v = 2.0f * Random.value - 1.0f;
			S = u * u + v * v;
		}
		while (S >= 1.0f);

		float fac = Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);
		return u * fac;
	}

	#if UNITY_EDITOR
	void OnDrawGizmos ()
	{
		if (drawTriangulation) {
			Gizmos.color = Color.magenta;
			if (m_delaunayTriangulation != null) {
				for (int i = 0; i < m_delaunayTriangulation.Count; i++) {
					Vector2 left = (Vector2)m_delaunayTriangulation[i].p0;
					Vector2 right = (Vector2)m_delaunayTriangulation[i].p1;
					Gizmos.DrawLine(left, right);
				}
			}
		}

		if (drawSpanningTree) {
			Gizmos.color = Color.green;
			if (m_spanningTree != null) {
				for (int i = 0; i < m_spanningTree.Count; i++) {
					Vector2 left = (Vector2)m_spanningTree[i].p0;
					Vector2 right = (Vector2)m_spanningTree[i].p1;
					Gizmos.DrawLine(left, right);
				}
			}
		}

		if (drawExtendedTree) {
			Gizmos.color = Color.yellow;
			if (m_extendedTree != null) {
				for (int i = 0; i < m_extendedTree.Count; i++) {
					Vector2 left = (Vector2)m_extendedTree[i].p0;
					Vector2 right = (Vector2)m_extendedTree[i].p1;
					Gizmos.DrawLine(left, right);
				}
			}
		}

		if (drawFinalTree) {
			Gizmos.color = Color.blue;
//			foreach (var corr in corridors) {
//				Vector2 left = (Vector2)corr.p0;
//				Vector2 right = (Vector2)corr.p1;
//				Gizmos.DrawLine(left, right);
//			}
			Gizmos.color = Color.white;
			foreach (var room in mainRooms) {
				Gizmos.DrawWireCube(new Vector2((room.roomBase.x+room.width/2)+0.5f, (room.roomBase.y+room.height/2)+0.5f), new Vector2(1f, 1f));
			}
//			foreach (var corr in corridors) {
//				Vector2 left = (Vector2)corr.p0;
//				Vector2 right = (Vector2)corr.p1;
//				//Gizmos.DrawLine(left, right);
//				Vector2 size = new Vector2(3f, 3f);
//				if (left.x != right.x) {
//					size.x = Vector2.Distance(left, right);
//				} else if (left.y != right.y) {
//					size.y = Vector2.Distance(left, right);
//				}
//				Gizmos.DrawWireCube(midpoint(left, right), size);
//			}
		}
	}
	#endif
}
