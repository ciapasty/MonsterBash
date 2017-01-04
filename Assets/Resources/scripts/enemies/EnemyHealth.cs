using UnityEngine;
using System.Collections;

// TODO: Change EnemyHealth to Health and make PlayerHealth a subclass

public class EnemyHealth : MonoBehaviour {

	public int maxHitpoints = 1;
	private int _hitpoints;
	public int hitpoints { 
		get {
			return _hitpoints;
		}
		protected set {
			if (value <= 0) {
				_hitpoints = 0;
				isDead = true;
			} else {
				_hitpoints = value;
				isDead = false;
			}
		}
	}

	public bool isDead { get; protected set; }

	void Start () {
		hitpoints = maxHitpoints;
	}

	public void changeHitpointsBy(int amount) {
		if (hitpoints+amount <= maxHitpoints) {
			hitpoints += amount;
		} else {
			hitpoints = maxHitpoints;
		}
	}
}