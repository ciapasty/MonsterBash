using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTemplate {

	public int width { get; protected set; }
	public int height { get; protected set; }
	public RoomType type { get; protected set; }
	public Tile[,] tileMap { get; protected set; }

	public RoomTemplate(int width, int height, string type, Tile[,] tileMap) {
		this.width = width;
		this.height = height;
		this.type = (RoomType)Enum.Parse(this.type.GetType(), type);
		this.tileMap = tileMap;
	}
}
