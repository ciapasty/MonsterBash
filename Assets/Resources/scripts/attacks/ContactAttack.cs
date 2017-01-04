using UnityEngine;
using System.Collections;

public class ContactAttack : Attack {

	void Update() {
		// Attack timer
		if (isAttacking) {
			if (durationTimer > duration) {
				durationTimer = 0;
				isAttacking = false;
			} else {
				execute();
			}
			durationTimer += Time.deltaTime;
		}
	}

	override public void execute() {
		isAttacking = true;
		Collider2D[] hitColliders = Physics2D.OverlapCircleAll(GetComponent<Renderer>().bounds.center, radius);
		foreach (var collider in hitColliders) {
			if (collider.gameObject.tag == "Player") {
				animator.SetTrigger("attackTrigger");
				collider.gameObject.GetComponent<PlayerHealth>().SendMessage("onHit", this);
			}
		}
	}
}
