using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DoorOrientation { NS, WE }

public class Door {

	public Tile tile { get; protected set; }
	public DoorOrientation orientation { get; protected set; }

	private bool _isOpen = false;
	public bool isOpen {
		get {
			return _isOpen;
		}
		set {
			_isOpen = value;
			if (!wereOpen && value) {
				wereOpen = true;
			}
			if (cbOnChanged != null)
				cbOnChanged(this);
		}
	}
	public bool wereOpen = false;
	public bool _isLocked = false;
	public bool isLocked { 
		get {
			return _isLocked;
		}
		set {
			_isLocked = value;
			if (value) {
				isOpen = false;
			} else {
				isOpen = wereOpen;
			}
			if (cbOnChanged != null)
				cbOnChanged(this);
		}
	}

	// Callback
	public Action<Door> cbOnChanged;

	public Door(Tile tile, DoorOrientation orientation) {
		this.tile = tile;
		this.orientation = orientation;
	}

	public void registerOnChangedCallback(Action<Door> callback) {
		cbOnChanged += callback;
	}

	public void unregisterOnChangedCallback(Action<Door> callback) {
		cbOnChanged -= callback;
	}
}
