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
	public float connectionsAdded = 0.1f;

	public float mainRoomMeanValueMod = 0.8f;
	public int roomWallMargin = 2;

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

		iterationCount++;
		if (iterationCount <= alignmentIterations) {
			updateMainRoomsPosition();
		} else {
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
	}

	/// <summary>
	/// Rounds main rooms positions to int values.
	/// Restarts CheckObjectsHaveStopped().
	/// </summary>
	void updateMainRoomsPosition() {
		foreach (var room in mainRooms) {
			GameObject go = roomGoMap[room];

			float xPos = Mathf.Ceil(go.transform.position.x);
			float yPos = Mathf.Ceil(go.transform.position.y);

			Vector2 pos = new Vector2(xPos, yPos);

			go.transform.position = pos;
			room.center = pos;
		}

		if (iterationCount <= alignmentIterations) {
			removeRoomGOs();
			createMainRoomGOs();
			StartCoroutine(CheckObjectsHaveStopped());
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

		// Layout rooms in newly created tile map.
		map = new Map(mainRooms);
		removeRoomGOs();

		generateCorridors();

		isFinished = true;
	}

	// Corridor generation

	void generateCorridors() {
		List<Room> done = new List<Room>();
		corridors = new List<LineSegment>();

		foreach (var room in mainRooms) {
			foreach (var connRoom in room.connectedRooms) {
				if (!done.Contains(connRoom)) {
					Vector2[] corridor = checkSuitableCorridor(room, connRoom);
				}
			}
			done.Add(room);
		}
	}
		
	Vector2[] checkSuitableCorridor(Room r1, Room r2) {
//		if() {
//			
//		}
		return null;
	}

//	void generateCorridors_OLD() {
//		List<Room> done = new List<Room>();
//		corridors = new List<LineSegment>();
//		Vector2 corridorStart;
//		Vector2 corridorEnd;
//
//		foreach (var room in mainRooms) {
//			foreach (var connRoom in room.connectedRooms) {
//				if (!done.Contains(connRoom)) {
//					Vector2 midPoint = midpoint(room.center, connRoom.center);
//					float sizeAdjust = 2.0f;
//					// Vertical corridors
//					if ((midPoint.x > room.center.x-room.size.x/2+roomWallMargin && midPoint.x < room.center.x+room.size.x/2-roomWallMargin) && 
//						(midPoint.x > connRoom.center.x-connRoom.size.x/2+roomWallMargin && midPoint.x < connRoom.center.x+connRoom.size.x/2-roomWallMargin)) {
//						corridorStart = new Vector2(midPoint.x, room.center.y);
//						corridorEnd = new Vector2(midPoint.x, connRoom.center.y);
//						if (room.center.y > connRoom.center.y) {
//							corridorStart.y += sizeAdjust-room.size.y/2;
//							corridorEnd.y += -sizeAdjust+connRoom.size.y/2;
//						} else {
//							corridorStart.y += -sizeAdjust+room.size.y/2;
//							corridorEnd.y += sizeAdjust-connRoom.size.y/2;
//						}
//
//						Collider2D[] hitColliders = Physics2D.OverlapAreaAll(corridorStart, corridorEnd);
//						if (hitColliders.Length < 3) {
//							corridors.Add(new LineSegment(corridorStart, corridorEnd));
//						}
//						// Horizontal corridors
//					} else if ((midPoint.y > room.center.y-room.size.y/2+roomWallMargin && midPoint.y < room.center.y+room.size.y/2-roomWallMargin) && 
//						(midPoint.y > connRoom.center.y-connRoom.size.y/2+roomWallMargin && midPoint.y < connRoom.center.y+connRoom.size.y/2-roomWallMargin)) {
//						corridorStart = new Vector2(room.center.x, midPoint.y);
//						corridorEnd = new Vector2(connRoom.center.x, midPoint.y);
//						if (room.center.x > connRoom.center.x) {
//							corridorStart.x += sizeAdjust-room.size.x/2;
//							corridorEnd.x += -sizeAdjust+connRoom.size.x/2;
//						} else {
//							corridorStart.x += -sizeAdjust+room.size.x/2;
//							corridorEnd.x += sizeAdjust-connRoom.size.x/2;
//						}
//							
//						Collider2D[] hitColliders = Physics2D.OverlapAreaAll(corridorStart, corridorEnd);
//						if (hitColliders.Length < 3) {
//							corridors.Add(new LineSegment(corridorStart, corridorEnd));
//						}
//						// Two segment corridors
//					} else {
//						Vector2 corridor1Start = new Vector2(room.center.x, room.center.y);
//						Vector2 corridor1End = new Vector2(connRoom.center.x, room.center.y);
//						if (room.center.x > connRoom.center.x) {
//							corridor1Start.x += sizeAdjust-room.size.x/2;
//						} else {
//							corridor1Start.x += -sizeAdjust+room.size.x/2;
//						}
//
//						Vector2 corridor2Start = new Vector2(connRoom.center.x, room.center.y);
//						Vector2 corridor2End = new Vector2(connRoom.center.x, connRoom.center.y);
//						if (room.center.y > connRoom.center.y) {
//							corridor2End.y += -sizeAdjust+connRoom.size.y/2;
//						} else {
//							corridor2End.y += sizeAdjust-connRoom.size.y/2;
//						}
//
//						Collider2D[] hitColliders = Physics2D.OverlapAreaAll(corridor1Start, corridor2End);
//						if (hitColliders.Length >= 3) {
//							corridor1Start = new Vector2(connRoom.center.x, connRoom.center.y);
//							corridor1End = new Vector2(room.center.x, connRoom.center.y);
//							if (connRoom.center.x > room.center.x) {
//								corridor1Start.x += sizeAdjust-connRoom.size.x/2;
//							} else {
//								corridor1Start.x += -sizeAdjust+connRoom.size.x/2;
//							}
//
//							corridor2Start = new Vector2(room.center.x, connRoom.center.y);
//							corridor2End = new Vector2(room.center.x, room.center.y);
//							if (connRoom.center.y > room.center.y) {
//								corridor2End.y += -sizeAdjust+room.size.y/2;
//							} else {
//								corridor2End.y += sizeAdjust-room.size.y/2;
//							}
//							Collider2D[] hitColliders2 = Physics2D.OverlapAreaAll(corridor1Start, corridor2End);
//							if (hitColliders2.Length < 3) {
//								corridors.Add(new LineSegment(corridor1Start, corridor1End));
//								corridors.Add(new LineSegment(corridor2Start, corridor2End));
//							}
//						} else {
//							corridors.Add(new LineSegment(corridor1Start, corridor1End));
//							corridors.Add(new LineSegment(corridor2Start, corridor2End));
//						}
//
//					}
//
//
//				}
//			}
//			done.Add(room);
//		}
//	}

	/// <summary>
	/// Creates GameObjects with box colliders for all rooms.
	/// </summary>
	void createRoomGOs() {
		roomGoMap = new Dictionary<Room, GameObject>();

		foreach (var room in rooms) {
			GameObject go = (GameObject)Instantiate(room_go, transform.position, Quaternion.identity);
			go.transform.SetParent(this.transform);
			go.transform.position = room.center;
			go.GetComponent<BoxCollider2D>().size = new Vector2(room.width, room.height);
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
			go.GetComponent<BoxCollider2D>().size = new Vector2(room.width, room.height);
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
				Gizmos.DrawWireCube(room.center, new Vector2(room.width, room.height));
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
