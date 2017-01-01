using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour {

	private Animator animator;
	//private PlayerController playerController;

	public int hitpoints = 1;

	private float lastHitTime;

	void Start () {
		animator = GetComponent<Animator>();
		//playerController = GetComponent<PlayerController>();
	}

	/*void OnCollisionEnter2D (Collision2D col) {
		if (col.gameObject.tag == "Player") {
			if (Time.time > lastHitTime + repeatDamagePeriod) {
				if (hitPoints > 0) {
					takeDamage(col.transform);
					lastHitTime = Time.time;
				} else {
					Debug.Log("Enemy "+gameObject.name+" is DEAD!");

					onDeath();
				}
			}
		}
	}*/

	void takeDamage(PlayerController player) {
		Vector3 hitVector = transform.position - player.gameObject.transform.position;
		GetComponent<Rigidbody2D>().AddForce(hitVector * player.attackForce * 100);
		hitpoints -= 1;
		if (hitpoints <= 0) {
			onDeath();
		}
	}

	void onDeath() {
		animator.SetTrigger("deathTrigger");
		GetComponent<BoxCollider2D>().enabled = false;
		GetComponent<EnemyController>().enabled = false;
		GetComponent<SpriteRenderer>().sortingLayerName = "Foliage";

		Destroy (gameObject, 5);
	}
}
