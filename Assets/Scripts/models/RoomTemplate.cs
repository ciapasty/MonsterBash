using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTemplate {

	public int width { get; protected set; }
	public int height { get; protected set; }
	public RoomType type { get; protected set; }
	public TileType[,] tileTypeMap { get; protected set; }

	public List<Tuple<int, int>> doorAnchorsNorth { get; protected set; }
	public List<Tuple<int, int>> doorAnchorsEast { get; protected set; }
	public List<Tuple<int, int>> doorAnchorsSouth { get; protected set; }
	public List<Tuple<int, int>> doorAnchorsWest { get; protected set; }

	public RoomTemplate(int width, int height, string type, TileType[,] tileTypeMap,
						List<Tuple<int, int>> doorAnchorsNorth, List<Tuple<int, int>> doorAnchorsEast,
						List<Tuple<int, int>> doorAnchorsSouth, List<Tuple<int, int>> doorAnchorsWest) 
	{
		this.width = width;
		this.height = height;
		this.type = (RoomType)Enum.Parse(this.type.GetType(), type);
		this.tileTypeMap = tileTypeMap;

		this.doorAnchorsNorth = doorAnchorsNorth;
		this.doorAnchorsEast = doorAnchorsEast;
		this.doorAnchorsSouth = doorAnchorsSouth;
		this.doorAnchorsWest = doorAnchorsWest;
	}
}
