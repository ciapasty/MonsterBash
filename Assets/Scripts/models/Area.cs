using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Area {

	public int ID { get; protected set; }
	protected bool _isDiscovered = false;
	public bool isDiscovered { 
		get {
			return _isDiscovered;
		}
		set {
			if (value && !_isDiscovered) {
				_isDiscovered = value;
				if (cbIsDiscovered != null)
					cbIsDiscovered(this);
			}
		}
	}

	Action<Area> cbIsDiscovered;

	public Area(int ID) {
		this.ID = ID;
	}

	public void registerBeenDiscoveredCallback(Action<Area> callback) {
		cbIsDiscovered += callback;
	}

	public void unregisterBeenDiscoveredCallback(Action<Area> callback) {
		cbIsDiscovered -= callback;
	}
}

