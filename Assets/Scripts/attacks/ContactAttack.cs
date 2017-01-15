using UnityEngine;
using System.Collections;

public class ContactAttack : Attack {

	void OnCollisionStay2D(Collision2D coll) {
		foreach (var targetTag in go_tags) {
			if (coll.gameObject.tag == targetTag) {
				if (cooldown <= 0) {
					animator.SetTrigger("attackTrigger");
					coll.gameObject.SendMessage("onHit", this);

					cooldown = cooldownTime;
				}
			}
		}
	}
}
