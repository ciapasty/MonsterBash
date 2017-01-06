using UnityEngine;
using System.Collections;

public class MeleeAttack : Attack {

	override public void Update () {
		base.Update();
		if (isAttacking) {
			doAttack();
			durationTimer += Time.deltaTime;
		}

		if (durationTimer >= duration) {
			isAttacking = false;
			durationTimer = 0;
		}
	}

	public override void execute() {
		//base.execute();
		isAttacking = true;
		cooldown = cooldownTime;
	}

	void doAttack() {
		Collider2D[] hitColliders = Physics2D.OverlapCircleAll(GetComponent<Renderer>().bounds.center, radius);
		foreach (var collider in hitColliders) {
			foreach (var tag in go_tags) {
				if (collider.gameObject.tag == tag) {
					if ((!gameObject.GetComponent<SpriteRenderer>().flipX && (collider.gameObject.transform.position-transform.position).x > 0) || 
						(gameObject.GetComponent<SpriteRenderer>().flipX && (collider.gameObject.transform.position-transform.position).x < 0)) {
						collider.gameObject.SendMessage("onHit", this);
					}
				}
			}
		}
	}
}
