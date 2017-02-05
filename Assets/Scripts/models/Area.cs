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
			}
		}
	}
		
	public Area(int ID) {
		this.ID += ID;
	}
}

