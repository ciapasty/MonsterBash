using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelExit : MonoBehaviour {

	GameController gc;
	// Use this for initialization
	void Start () {
		gc = GameController.Instance;
	}

	void OnCollisionEnter2D(Collision2D col) {
		if (col.gameObject.tag == "Player") {
			ContactPoint2D[] contacts = col.contacts;
			if (contacts[0].point.y == contacts[1].point.y) {
				if (contacts[0].point.y < transform.position.y) {
					gc.SendMessage("playerEnteredExit");
				}
			}
		}
	}
}
