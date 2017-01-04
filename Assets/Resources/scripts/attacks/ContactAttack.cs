using UnityEngine;
using System.Collections;

public class ContactAttack : Attack {

	void OnCollisionStay2D(Collision2D coll) {
		if (coll.gameObject.tag == "Player") {
			if (cooldownTimer <= 0) {
				animator.SetTrigger("attackTrigger");
				coll.gameObject.GetComponent<PlayerHealth>().SendMessage("onHit", this);

				cooldownTimer = cooldown;
			}
		}
	}
}
