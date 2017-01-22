using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door {

	public Tile tile { get; protected set; }

	public bool isOpen { get; protected set; }
	public bool isLocked { get; protected set; }

	public Door(Tile tile) {
		this.tile = tile;

		isLocked = false;
		isOpen = false;
	}

	public void lockDoor() {
		isLocked = true;
	}
}
