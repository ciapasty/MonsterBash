using Delaunay;
using Delaunay.Geo;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour {

	public string baseTemplatesFile = "base_room_templates.xml";
	public string roomTemplatesFile = "room_templates.xml";
	private RoomXMLParser xmlParser;
	private List<RoomTemplate> baseRoomTemplates;
	private List<RoomTemplate> roomTemplates;

	// Rooms paramteres
	public int roomCount = 30;
	public int elipseWidth = 10;
	public int elipseHeight = 10;

	public GameObject room_go;

	public int alignmentIterations = 3;
	public int mainRoomsBuffer = 2;
	public float connectionsAdded = 0.1f;

	// Debug drawing
	public bool drawTriangulation = false;
	public bool drawSpanningTree = false;
	public bool drawExtendedTree = false;

	float minX = 0f;
	float minY = 0f;

	private Room bonfireRoom;
	private List<Room> rooms;
	private Dictionary<Room, GameObject> roomGoMap;

	private int iterationCount = 1;

	// Delunay
	private List<Vector2> m_points;
	private List<LineSegment> m_spanningTree;
	private List<LineSegment> m_delaunayTriangulation;
	private List<LineSegment> m_extendedTree;

	// Enemies
	public GameObject[] enemyPrefabs;
	// Objects
	public GameObject[] objectsPrefabs;

	// World
	public Map map { get; protected set; }

	public bool isFinished { get; protected set; }

	void Awake() {
		isFinished = false;
		xmlParser = new RoomXMLParser();
	}

	/// <summary>
	/// Starts the map generation routine.
	/// </summary>
	public void startMapCreation() {

		baseRoomTemplates = xmlParser.getRoomTemplatesDictFrom(baseTemplatesFile);
		roomTemplates = xmlParser.getRoomTemplatesDictFrom(roomTemplatesFile);

		generateRooms();
		createRoomGOs();

		Time.timeScale = 5f;
		StartCoroutine(CheckObjectsHaveStopped());
	}

	/// <summary>
	/// Generates all rooms (main and fill).
	/// </summary>
	void generateRooms() {
		rooms = new List<Room>();

		// Bonfire
		bonfireRoom = new Room(0, baseRoomTemplates[0], getRandomPointInElipse(elipseWidth, elipseHeight));
		rooms.Add(bonfireRoom);

		for (int i = 0; i < roomCount; i++) {
			rooms.Add(new Room(i+1, roomTemplates[Random.Range(0,roomTemplates.Count)], getRandomPointInElipse(elipseWidth, elipseHeight)));
		}

		print(roomCount+2+" rooms created");
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
		foreach (var room in rooms) {
			float bottom = room.center.y-room.tp.height/2;
			float left = room.center.x-room.tp.width/2;

			if (bottom < minY) { minY = bottom; }
			if (left < minX) { minX = left; }
		}

		foreach (var room in rooms) {
			room.center.x += -minX+mainRoomsBuffer;
			room.center.y += -minY+mainRoomsBuffer;

			Vector2 go_pos = roomGoMap[room].transform.position;
			go_pos.x += -minX;
			go_pos.y += -minY;
		}

		createSpanningTree();
	}

	/// <summary>
	/// Rounds main rooms positions to int values.
	/// Restarts CheckObjectsHaveStopped().
	/// </summary>
	void updateMainRoomsPosition() {
		foreach (var room in rooms) {
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

		for (int i = 0; i < rooms.Count; i++) {
			colors.Add(0);
			m_points.Add(rooms[i].center);
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
			foreach (var room in rooms) {
				if (room.center == left) {
					foreach (var connRoom in rooms) {
						if (connRoom.center == right) {
							if (!room.connectedRooms.Contains(connRoom))
								room.connectedRooms.Add(connRoom);
						}
					}
				} else if (room.center == right) {
					foreach (var connRoom in rooms) {
						if (connRoom.center == left) {
							if (!room.connectedRooms.Contains(connRoom))
								room.connectedRooms.Add(connRoom);
						}
					}
				}
			}
		}

		Debug.Log("Final tree created");

		float maxX = 0f;
		float maxY = 0f;

		List<Room> singleEntranceRooms = new List<Room>();

		foreach (var room in rooms) {
			float top = (room.center.y+room.tp.height/2)+mainRoomsBuffer*10;
			float right = (room.center.x+room.tp.width/2)+mainRoomsBuffer*10;

			if (room.connectedRooms.Count == 1) {
				singleEntranceRooms.Add(room);
			}

			if (top > maxY) { maxY = top; }
			if (right > maxX) { maxX = right; }
		}

		removeRoomGOs();
		// Regenerate map if there is no single entrance room exit
		Room exitRoom;
		if (singleEntranceRooms.Count < 1) {
			Debug.Log("Not enough single entrance rooms! Regenerating!");
			startMapCreation();
			return;
		}

		exitRoom = singleEntranceRooms[0];
		foreach (var room in singleEntranceRooms) {
			if (Vector2.Distance(exitRoom.center, bonfireRoom.center) < Vector2.Distance(room.center, bonfireRoom.center))
				exitRoom = room;
		}
		int index = rooms.IndexOf(exitRoom);
		rooms[index] = new Room(exitRoom.ID, baseRoomTemplates[1], exitRoom.center);
		rooms[index].connectedRooms = exitRoom.connectedRooms;
		int index2 = rooms[index].connectedRooms[0].connectedRooms.IndexOf(exitRoom);
		exitRoom.connectedRooms[0].connectedRooms[index2] = rooms[index];
		exitRoom = rooms[index];

		// Layout rooms in newly created tile map.
		map = new Map(Mathf.CeilToInt(maxX)+1, Mathf.CeilToInt(maxY)+1);

		// Final sequence

		layOutRooms();
		generateCorridors();
		addCorridorWalls();
		map.setSpawnTileTo(map.getTileAt(bonfireRoom.roomBase.x+bonfireRoom.tp.width/2, bonfireRoom.roomBase.y+bonfireRoom.tp.height/2));
		map.setExitTileTo(map.getTileAt(exitRoom.roomBase.x+exitRoom.tp.width/2, exitRoom.roomBase.y+exitRoom.tp.height/2));

		placeObjects();
		placeEnemies();

		// map is complete

		GameController.Instance.SendMessage("postMapCreationSetup");
	}

	/// <summary>
	/// Lays out main rooms on tile map
	/// </summary>
	void layOutRooms() {
		foreach (var room in rooms) {
			map.addRoom(room);
			int roomBaseX = Mathf.CeilToInt(room.center.x)-(room.tp.width/2);
			int roomBaseY = Mathf.CeilToInt(room.center.y)-(room.tp.height/2);
			room.setBaseTileTo(map.getTileAt(roomBaseX,roomBaseY));
			for (int x = 0; x < room.tp.width; x++) {
				for (int y = 0; y < room.tp.height; y++) {
					if (roomBaseX+x >= map.width || roomBaseX+x < 0 || roomBaseY+y >= map.height || roomBaseY+y < 0){
						Debug.LogError("Tile ("+(roomBaseX+x)+", "+(roomBaseY+y)+") is out of map bounds ("+map.width+". "+map.height+")");
					}
					Tile tile = map.getTileAt(room.roomBase.x+x,room.roomBase.y+y);
					tile.setRoom(room);

					tile.type = room.tp.tileTypeMap[x,y];
				}
			}
		}
	}

	// Corridor generation

	/// <summary>
	/// Generates corridors to connected rooms.
	/// </summary>
	void generateCorridors() {
		List<Room> done = new List<Room>();
		Tile tile1, tile2, tile3;
		Corridor corridor;
		int corridorID = roomCount;

		foreach (var r1 in map.rooms) {
			foreach (var r2 in r1.connectedRooms) {
				if (!done.Contains(r2)) {
					corridor = new Corridor(corridorID);

					int midY = ((r1.roomBase.y+(r1.tp.height/2))+(r2.roomBase.y+(r2.tp.height/2)))/2;
					int midX = ((r1.roomBase.x+(r1.tp.width/2))+(r2.roomBase.x+(r2.tp.width/2)))/2;

					if(isHorizontalCorridor(r1, r2, midY)) {
						if (midX < r1.roomBase.x+r1.tp.width/2) {
							// room 1 West wall, room 2 East wall
							tile1 = getDoorAnchorTileClosestToXY(r1.tp.doorAnchorsWest, new Vector2(midX, midY-2), r1);
							tile2 = getNextRoomsOppositeDoorAnchorTile(r2, r2.tp.doorAnchorsEast, tile1, true);
							if (tile2 == null)
								tile2 = getDoorAnchorTileClosestToXY(r2.tp.doorAnchorsEast, new Vector2(midX, midY-2), r2);
						} else {
							// room 1 East wall, room 2 West wall
							tile1 = getDoorAnchorTileClosestToXY(r1.tp.doorAnchorsEast, new Vector2(midX, midY-2), r1);
							tile2 = getNextRoomsOppositeDoorAnchorTile(r2, r2.tp.doorAnchorsWest, tile1, true);
							if (tile2 == null)
								tile2 = getDoorAnchorTileClosestToXY(r2.tp.doorAnchorsWest, new Vector2(midX, midY-2), r2);
						}
						if (!crossesRoomHorizontally(tile1, tile2)) {
							if (tile1.y == tile2.y) {
								layOutHorizontalCorridor(tile1, tile2, corridor);
							} else {
								// 3-segment corridor
								int vertMid = (tile1.x+tile2.x)/2;
								layOutHorizontalCorridor(tile1, map.getTileAt(vertMid, tile1.y), corridor);
								layOutHorizontalCorridor(tile2, map.getTileAt(vertMid, tile2.y), corridor);
								// vertical segment
								if (tile1.y > tile2.y) {
									layOutVerticalCorridor(map.getTileAt(vertMid-1, tile2.y+1), map.getTileAt(vertMid-1, tile1.y+3), corridor);
								} else {
									layOutVerticalCorridor(map.getTileAt(vertMid-1, tile1.y+1), map.getTileAt(vertMid-1, tile2.y+3), corridor);
								}
							}
						}
					} else if (isVerticalCorridor(r1, r2, midX)) {
						if (midY < r1.roomBase.y+r1.tp.height/2) {
							tile1 = getDoorAnchorTileClosestToXY(r1.tp.doorAnchorsSouth, new Vector2(midX-1, midY), r1);
							tile2 = getNextRoomsOppositeDoorAnchorTile(r2, r2.tp.doorAnchorsNorth, tile1, false);
							if (tile2 == null)
								tile2 = getDoorAnchorTileClosestToXY(r2.tp.doorAnchorsNorth, new Vector2(midX-1, midY), r2);
						} else {
							tile1 = getDoorAnchorTileClosestToXY(r1.tp.doorAnchorsNorth, new Vector2(midX-1, midY), r1);
							tile2 = getNextRoomsOppositeDoorAnchorTile(r2, r2.tp.doorAnchorsSouth, tile1, false);
							if (tile2 == null)
								tile2 = getDoorAnchorTileClosestToXY(r2.tp.doorAnchorsSouth, new Vector2(midX-1, midY), r2);
						}
						if (!crossesRoomVertically(tile1, tile2)) {
							if (tile1.x == tile2.x) {
								layOutVerticalCorridor(tile1, tile2, corridor);
							} else {
								// 3-segment corridor
								int vertMid = (tile1.y+tile2.y)/2;
								layOutVerticalCorridor(tile1, map.getTileAt(tile1.x, vertMid), corridor);
								layOutVerticalCorridor(tile2, map.getTileAt(tile2.x, vertMid), corridor);
								// horizontal segment
								if (tile1.x > tile2.x) {
									layOutHorizontalCorridor(map.getTileAt(tile2.x+1, vertMid-1), map.getTileAt(tile1.x+2, vertMid-1), corridor);
								} else {
									layOutHorizontalCorridor(map.getTileAt(tile1.x+1, vertMid-1), map.getTileAt(tile2.x+2, vertMid-1), corridor);
								}
							}
						}
					} else {
						// Double segment corridors

						if (r1.roomBase.x < r2.roomBase.x) {
							tile1 = getDoorAnchorTileClosestToXY(r1.tp.doorAnchorsEast, new Vector2(r2.roomBase.x, r2.roomBase.y), r1);
						} else {
							tile1 = getDoorAnchorTileClosestToXY(r1.tp.doorAnchorsWest, new Vector2(r2.roomBase.x, r2.roomBase.y), r1);
						}
						if (r1.roomBase.y < r2.roomBase.y) {
							tile2 = getDoorAnchorTileClosestToXY(r2.tp.doorAnchorsSouth, new Vector2(r1.roomBase.x, r1.roomBase.y), r2);
						} else {
							tile2 = getDoorAnchorTileClosestToXY(r2.tp.doorAnchorsNorth, new Vector2(r1.roomBase.x, r1.roomBase.y), r2);
						}
						tile3 = map.getTileAt(tile2.x, tile1.y);

						if (crossesRoomHorizontally(tile1, tile3) || crossesRoomVertically(tile2, tile3)) {
							
							if (r1.roomBase.x < r2.roomBase.x) {
								tile2 = getDoorAnchorTileClosestToXY(r2.tp.doorAnchorsWest, new Vector2(r1.roomBase.x, r1.roomBase.y), r2);
							} else {
								tile2 = getDoorAnchorTileClosestToXY(r2.tp.doorAnchorsEast, new Vector2(r1.roomBase.x, r1.roomBase.y), r2);
							}
							if (r1.roomBase.y < r2.roomBase.y) {
								tile1 = getDoorAnchorTileClosestToXY(r1.tp.doorAnchorsNorth, new Vector2(r2.roomBase.x, r2.roomBase.y), r1);
							} else {
								tile1 = getDoorAnchorTileClosestToXY(r1.tp.doorAnchorsSouth, new Vector2(r2.roomBase.x, r2.roomBase.y), r1);
							}
							tile3 = map.getTileAt(tile1.x, tile2.y);

							if (!crossesRoomHorizontally(tile2, tile3) && !crossesRoomVertically(tile1, tile3)) {
								if (tile1.x < tile2.x) {
									layOutHorizontalCorridor(tile2, map.getTileAt(tile3.x+1, tile3.y), corridor);
								} else {
									layOutHorizontalCorridor(tile2, map.getTileAt(tile3.x+1, tile3.y), corridor);
								}
								if (tile1.y < tile2.y) {
									layOutVerticalCorridor(tile1, map.getTileAt(tile3.x, tile3.y+3), corridor);
								} else {
									layOutVerticalCorridor(tile1, map.getTileAt(tile3.x, tile3.y+1), corridor);
								}
							}
						} else {
							if (tile1.x < tile2.x) {
								layOutHorizontalCorridor(tile1, map.getTileAt(tile3.x+1, tile3.y), corridor);
							} else {
								layOutHorizontalCorridor(tile1, map.getTileAt(tile3.x+1, tile3.y), corridor);
							}
							if (tile1.y < tile2.y) {
								layOutVerticalCorridor(tile2, map.getTileAt(tile3.x, tile3.y+1), corridor);
							} else {
								layOutVerticalCorridor(tile2, map.getTileAt(tile3.x, tile3.y+3), corridor);
							}
						}
					}
					corridorID += 1;
				}
			}
			done.Add(r1);
		}
	}

	/// <summary>
	/// Returns door anchor tile closest to point x,y
	/// </summary>
	/// <returns>Door anchor tile closest to x,y.</returns>
	/// <param name="anchors">Anchors.</param>
	/// <param name="X">Point x coord.</param>
	/// <param name="Y">Point y coord.</param>
	/// <param name="room">Anchors room.</param>
	Tile getDoorAnchorTileClosestToXY(List<Tuple<int, int>> anchors, Vector2 point, Room room) {
		float distance = Vector2.Distance(point, new Vector2(Mathf.Infinity, Mathf.Infinity));
		Tuple<int, int> anchor = null;
		foreach (var ac in anchors) {
			Vector2 anchorPoint = new Vector2(room.roomBase.x+ac.first, room.roomBase.y+ac.second);
			float acDistance = Vector2.Distance(point, anchorPoint);
			if (acDistance < distance) {
				distance = acDistance;
				anchor = ac;
			}
		}
		if (anchor == null)
			Debug.LogError("No anchor closer than point (inf,inf)!");
		
		return map.getTileAt(room.roomBase.x+anchor.first, room.roomBase.y+anchor.second);
	}

	/// <summary>
	/// Returns next rooms door anchor tile if it's opossite the door anchor tile. Returns null otherwise.
	/// </summary>
	/// <returns>The next rooms opposite door anchor tile.</returns>
	/// <param name="nextRoom">Next room.</param>
	/// <param name="nextRoomAnchors">Next room anchors.</param>
	/// <param name="doorAnchor">Door anchor.</param>
	/// <param name="horizontal">If set to <c>true</c> horizontal.</param>
	Tile getNextRoomsOppositeDoorAnchorTile(Room nextRoom, List<Tuple<int, int>> nextRoomAnchors, Tile doorAnchor, bool horizontal) {
		foreach (var ac in nextRoomAnchors) {
			if (horizontal) {
				if (nextRoom.roomBase.y+ac.second == doorAnchor.y) {
					return map.getTileAt(nextRoom.roomBase.x+ac.first, nextRoom.roomBase.y+ac.second);
				}
			} else {
				if (nextRoom.roomBase.x+ac.first == doorAnchor.x) {
					return map.getTileAt(nextRoom.roomBase.x+ac.first, nextRoom.roomBase.y+ac.second);
				}
			}
		}
		return null;
	}

	/// <summary>
	/// Checks if the corridor is horizontal.
	/// </summary>
	/// <returns><c>true</c>, if rooms are lined up horizontally, <c>false</c> otherwise.</returns>
	/// <param name="r1">Room 1.</param>
	/// <param name="r2">Room 2.</param>
	/// <param name="midY">Middle point Y.</param>
	bool isHorizontalCorridor(Room r1, Room r2, int midY) {
		return ((midY-r1.roomBase.y) >= 0 && (midY-r1.roomBase.y) < r1.tp.height) 
			&& ((midY-r2.roomBase.y) >= 0 && (midY-r2.roomBase.y) < r2.tp.height);
	}

	/// <summary>
	/// Checks if the corridor is vertical.
	/// </summary>
	/// <returns><c>true</c>, if connected rooms are lined up vertically, <c>false</c> otherwise.</returns>
	/// <param name="r1">Room 1.</param>
	/// <param name="r2">Room 2.</param>
	/// <param name="midY">Middle point X.</param>
	bool isVerticalCorridor(Room r1, Room r2, int midX) {
		return ((midX-r1.roomBase.x) >= 0 && (midX-r1.roomBase.x) < r1.tp.width) 
			&& ((midX-r2.roomBase.x) >= 0 && (midX-r2.roomBase.x) < r2.tp.width);
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
			if (map.getTileAt(i, t1.y).type == TileType.floor || map.getTileAt(i, t1.y).type == TileType.wallBottom ||
				map.getTileAt(i, t1.y+1).type == TileType.floor || map.getTileAt(i, t1.y+1).type == TileType.wallBottom ||
				map.getTileAt(i, t1.y+2).type == TileType.floor || map.getTileAt(i, t1.y+2).type == TileType.wallBottom ||
				map.getTileAt(i, t1.y+3).type == TileType.floor || map.getTileAt(i, t1.y+3).type == TileType.wallBottom ||
				map.getTileAt(i, t1.y+4).type == TileType.floor || map.getTileAt(i, t1.y+4).type == TileType.wallBottom )
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
			if (map.getTileAt(t1.x, i).type == TileType.floor || map.getTileAt(t1.x, i).type == TileType.wallBottom ||
				map.getTileAt(t1.x+1, i).type == TileType.floor || map.getTileAt(t1.x+1, i).type == TileType.wallBottom ||
				map.getTileAt(t1.x+2, i).type == TileType.floor || map.getTileAt(t1.x+2, i).type == TileType.wallBottom ||
				map.getTileAt(t1.x+3, i).type == TileType.floor || map.getTileAt(t1.x+3, i).type == TileType.wallBottom )
				return true;
		}
		return false;
	}

	/// <summary>
	/// Layouts horizontal corridor on tile map.
	/// </summary>
	/// <param name="t1">Start tile.</param>
	/// <param name="t2">End tile.</param>
	/// <param name="corridor">Corridor</param>> 
	void layOutHorizontalCorridor(Tile t1, Tile t2, Corridor corridor) {
		if (t1.room != null) {
			// TODO: change door status
			Room r = map.getRoomWithID(t1.room.ID);
			r.addDoor(new Door(map.getTileAt(t1.x, t1.y+3), DoorOrientation.NS));
			map.getTileAt(t1.x, t1.y+1).setCorridor(corridor);
			map.getTileAt(t1.x, t1.y+2).setCorridor(corridor);
			map.getTileAt(t1.x, t1.y+3).setCorridor(corridor);
			map.getTileAt(t1.x, t1.y+4).setCorridor(corridor);
		}

		if (t2.room != null) {
			// TODO: change door status
			Room r = map.getRoomWithID(t2.room.ID);
			r.addDoor(new Door(map.getTileAt(t2.x, t2.y+3), DoorOrientation.NS));
			map.getTileAt(t2.x, t2.y+1).setCorridor(corridor);
			map.getTileAt(t2.x, t2.y+2).setCorridor(corridor);
			map.getTileAt(t2.x, t2.y+3).setCorridor(corridor);
			map.getTileAt(t2.x, t2.y+4).setCorridor(corridor);
		}

		map.addCorridor(corridor);

		int min, max;
		min = (t1.x > t2.x) ? t2.x : t1.x;
		max = (t1.x > t2.x) ? t1.x : t2.x;

		Tile t;
		for (int i = min; i <= max; i++) {
			t = map.getTileAt(i, t1.y+1);
			t.type = TileType.floor;
			if (t.corridor == null)
				t.setCorridor(corridor);
			t = map.getTileAt(i, t1.y+2);
			t.type = TileType.floor;
			if (t.corridor == null)
				t.setCorridor(corridor);
			t = map.getTileAt(i, t1.y+3);
			t.type = TileType.floor;
			if (t.corridor == null)
				t.setCorridor(corridor);
		}
	}

	/// <summary>
	/// Lays out verticall corridor on tile map.
	/// </summary>
	/// <param name="t1">Start tile.</param>
	/// <param name="t2">End tile.</param>
	/// <param name="corridor">Corridor</param>
	void layOutVerticalCorridor(Tile t1, Tile t2, Corridor corridor) {
		if (t1.room != null) {
			// TODO: change door status
			Room r = map.getRoomWithID(t1.room.ID);
			r.addDoor(new Door(map.getTileAt(t1.x+1, t1.y), DoorOrientation.WE));
			map.getTileAt(t1.x+1, t1.y).setCorridor(corridor);
			map.getTileAt(t1.x+2, t1.y).setCorridor(corridor);
			map.getTileAt(t1.x+3, t1.y).setCorridor(corridor);
		}

		if (t2.room != null) {
			// TODO: change door status
			Room r = map.getRoomWithID(t2.room.ID);
			r.addDoor(new Door(map.getTileAt(t2.x+1, t2.y), DoorOrientation.WE));
			map.getTileAt(t2.x+1, t2.y).setCorridor(corridor);
			map.getTileAt(t2.x+2, t2.y).setCorridor(corridor);
			map.getTileAt(t2.x+3, t2.y).setCorridor(corridor);
		}

		map.addCorridor(corridor);

		int min, max;
		min = (t1.y > t2.y) ? t2.y : t1.y;
		max = (t1.y > t2.y) ? t1.y : t2.y;

		Tile t;
		for (int i = min; i <= max; i++) {
			t = map.getTileAt(t1.x+1, i);
			t.type = TileType.floor;
			if (t.corridor == null)
				t.setCorridor(corridor);
			t = map.getTileAt(t1.x+2, i);
			t.type = TileType.floor;
			if (t.corridor == null)
				t.setCorridor(corridor);
		}
	}

	/// <summary>
	/// Adds walls to corridors. Loops through whole tile map
	/// </summary>
	void addCorridorWalls() {
		for (int x = 0; x < map.width; x++) {
			for (int y = 0; y < map.height; y++) {
				Tile t = map.getTileAt(x, y);
				if (t.corridor != null && t.type == TileType.floor) {
					setCorridorWall(map.getTileAt(x+1, y), t.corridor);
					setCorridorWall(map.getTileAt(x-1, y), t.corridor);
					setCorridorWall(map.getTileAt(x, y+1), t.corridor);
					setCorridorWall(map.getTileAt(x, y-1), t.corridor);
					setCorridorWall(map.getTileAt(x+1, y+1), t.corridor);
					setCorridorWall(map.getTileAt(x+1, y-1), t.corridor);
					setCorridorWall(map.getTileAt(x-1, y+1), t.corridor);
					setCorridorWall(map.getTileAt(x-1, y-1), t.corridor);
				}
			}
		}
	}

	/// <summary>
	/// Sets tile to corridor wall if empty.
	/// </summary>
	/// <param name="tile">Tile.</param>
	void setCorridorWall(Tile tile, Corridor corridor) {
		if (tile.type == TileType.empty) {
			tile.setCorridor(corridor);
			tile.type = TileType.wallBottom;
		}
	}

	/// <summary>
	/// Places random number of destructible objects on random floor tiles
	/// </summary>
	void placeObjects() {
		// TODO: Place objects only adjacent to North, East, West walls
		if (objectsPrefabs.Length > 0) {
			foreach (var room in map.rooms) {
				for (int x = 0; x < room.roomBase.x+room.tp.width; x++) {
					for (int y = 0; y < room.roomBase.y+room.tp.height; y++) {
						Tile tile = map.getTileAt(x, y);
						if (!tile.hasContent && tile.type == TileType.floor && isAdjacentToWalls(tile)) {
							if (Random.value < 0.5f) {
								room.addObject(new Blueprint(objectsPrefabs[Random.Range(0, objectsPrefabs.Length)], tile));
								tile.hasContent = true;
							}
						}
					}
				}
			}
		}	
	}

	bool isAdjacentToWalls(Tile tile) {
		if ((map.getTileAt(tile.x, tile.y-1) != null && map.getTileAt(tile.x, tile.y-1).type == TileType.wallBottom) ||
			(map.getTileAt(tile.x, tile.y-2) != null && map.getTileAt(tile.x, tile.y-2).type == TileType.wallBottom) ||
			(map.getTileAt(tile.x, tile.y-3) != null && map.getTileAt(tile.x, tile.y-3).type == TileType.wallBottom)) {
			return false;
		}
		
		if ((map.getTileAt(tile.x+1, tile.y) != null && map.getTileAt(tile.x+1, tile.y).type == TileType.wallBottom) ||
			(map.getTileAt(tile.x-1, tile.y) != null && map.getTileAt(tile.x-1, tile.y).type == TileType.wallBottom) ||
			(map.getTileAt(tile.x, tile.y+1) != null && map.getTileAt(tile.x, tile.y+1).type == TileType.wallBottom)) {
			return true;
		}

		return false;
	}

	/// <summary>
	/// Places random number of enemies on random floor tiles
	/// </summary>
	void placeEnemies() {
		if (enemyPrefabs.Length > 0) {
			foreach (var room in map.rooms) {
				if (room.tp.type != RoomType.bonfire && room.tp.type != RoomType.exit) {
					int enemiesCount = Random.Range(3, 5);
					while (enemiesCount > 0) {
						int randX = Random.Range(room.roomBase.x+1, room.roomBase.x+room.tp.width-1);
						int randY = Random.Range(room.roomBase.y+1, room.roomBase.y+room.tp.height-1);
						Tile tile = map.getTileAt(randX, randY);
						if (tile.hasContent || tile.type != TileType.floor)
							continue;

						room.addEnemy(new Blueprint(enemyPrefabs[Random.Range(0, enemyPrefabs.Length)], tile));
						tile.hasContent = true;
						enemiesCount--;
					}
				}
			}
		}
	}

	/// <summary>
	/// Creates GameObjects with box colliders for all rooms.
	/// </summary>
	void createRoomGOs () {
		roomGoMap = new Dictionary<Room, GameObject>();

		foreach (var room in rooms) {
			GameObject go = (GameObject)Instantiate(room_go, transform.position, Quaternion.identity);
			go.transform.SetParent(this.transform);
			go.transform.position = room.center;
			go.GetComponent<BoxCollider2D>().size = new Vector2(room.tp.width+mainRoomsBuffer, room.tp.height+mainRoomsBuffer);
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
	}
	#endif
}
