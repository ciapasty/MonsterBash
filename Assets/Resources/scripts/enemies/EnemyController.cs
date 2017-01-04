using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour {

	private Animator animator;
	private Rigidbody2D rigidbod;
	private EnemyHealth enemyHealth;

	private Attack[] attacks;

	public GameObject target;

	public int soulsCarried = 10;

	void Start () {
		animator = GetComponent<Animator>();
		rigidbod = GetComponent<Rigidbody2D>();
		enemyHealth = GetComponent<EnemyHealth>();

		target = GameObject.FindGameObjectWithTag("Player");

		attacks = GetComponents<Attack>();
	}

	void Update() {
		foreach (var attk in attacks) {
			if (attk.cooldown <= 0) {
				if (Vector3.Distance(target.transform.position, transform.position) < attk.range ) {
					attk.execute();
				}
			}
		}
	}

	void doAttack() {}

	public void switchAttackStateTo(bool state) {
		GetComponent<MoveTowardsPlayer>().enabled = state;
		GetComponent<MoveIdle>().enabled = !state;
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

	void onHit(PlayerController player) {
		// No advanced hit detection now :(
		takeDamage(player.attackDamage);
		Vector3 hitVector = transform.position - player.gameObject.transform.position;
		rigidbod.AddForce(hitVector * player.attackForce * 100);
	}

	void takeDamage(int damage) {
		enemyHealth.changeHitpointsBy(-1);
		if (enemyHealth.isDead) {
			onDeath();
		}
	}

	void onDeath() {
		animator.SetTrigger("deathTrigger");
		GetComponent<Collider2D>().enabled = false;
		GetComponent<MoveIdle>().enabled = false;
		GetComponent<MoveTowardsPlayer>().enabled = false;

		// TODO: Spawn dead body GO with timed death -> prefab
		GetComponent<SpriteRenderer>().sortingLayerName = "Foliage";
		GetComponent<SpriteRenderer>().sortingOrder = Random.Range(0, 255);

		GameObject soul = (GameObject)Instantiate(Resources.Load("prefabs/soul"), transform.position, Quaternion.identity);
		soul.GetComponent<Soul>().souls = soulsCarried;

		Destroy (gameObject, 5);
		this.enabled = false;
	}
}
