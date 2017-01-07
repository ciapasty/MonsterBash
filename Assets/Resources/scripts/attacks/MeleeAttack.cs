using UnityEngine;
using System.Collections;

public class MeleeAttack : Attack {

	public bool preAttack = false;
	public float preAttackTime = 0.5f;
	private float preAttackTimer = 0;

	override public void Update () {
		base.Update();
		if (isAttacking) {
			if (preAttack) {
				if (preAttackTimer <= 0) {
					doAttack();
				}
				preAttackTimer -= Time.deltaTime;
			} else {
				doAttack();
			}
		}
	}

	public override void execute() {
		//base.execute();
		isAttacking = true;
		cooldown = cooldownTime;
		if (preAttack) {
			preAttackTimer = preAttackTime;
			animator.SetTrigger("preAttackTrigger");
		}
	}

	void doAttack() {
		animator.SetTrigger("attackTrigger");
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
		isAttacking = false;
	}
}
