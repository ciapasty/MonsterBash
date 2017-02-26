using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour {

	private PlayerController playerController;

	public int startingHitpoints = 3;
	public int hitpointsLimit = 12;
	public bool lockedHitpoints = false;
	private int _maxHitpoints;
	public int maxHitpoints {
		get {
			return _maxHitpoints;
		}
		set {
			_maxHitpoints = value;
			GameObject.FindGameObjectWithTag("UI_HealthBar").GetComponent<HealthBarControl>().SendMessage("updateHealth");
		}
	}
	private int _hitpoints;
	public int hitpoints { 
		get {
			return _hitpoints;
		}
		protected set {
			if (value <= 0) {
				_hitpoints = 0;
				isDead = true;
				lockedHitpoints = true;
			} else {
				_hitpoints = value;
				isDead = false;
			}
			GameObject.FindGameObjectWithTag("UI_HealthBar").GetComponent<HealthBarControl>().SendMessage("updateHealth");
		}
	}

	public bool isDead { get; protected set; }

	void Start() {
		maxHitpoints = startingHitpoints;
		hitpoints = maxHitpoints;
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.E))
			GetComponent<PlayerController>().SendMessage("takeDamage", 1);
	}

	public void changeHitpointsBy(int amount) {
		if (lockedHitpoints) {
			if (hitpoints+amount <= (maxHitpoints+1)/2) {
				hitpoints += amount;
			} else {
				hitpoints = (maxHitpoints+1)/2;
			}
		} else {
			if (hitpoints+amount <= maxHitpoints) {
				hitpoints += amount;
			} else {
				hitpoints = maxHitpoints;
			}
		}
	}

	public void increaseMaxHitpointsBy(int amount) {
		if (maxHitpoints+amount <= hitpointsLimit) {
			maxHitpoints += amount;
		} else {
			maxHitpoints = hitpointsLimit;
		}
	}
}
