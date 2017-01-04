using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour {

	private Animator animator;
	private Rigidbody2D rigidbod;
	private EnemyHealth enemyHealth;

	private Attack attack;

	public GameObject target;

	public float attackInterval = 2f;
	private float attackIntervalTimer;

	public int soulsCarried = 10;

	void Start () {
		animator = GetComponent<Animator>();
		rigidbod = GetComponent<Rigidbody2D>();
		enemyHealth = GetComponent<EnemyHealth>();

		target = GameObject.FindGameObjectWithTag("Player");

		attack = GetComponent<Attack>();

		attackIntervalTimer = attackInterval;
	}

	void Update() {
		// Attack timer
		if (!attack.isAttacking) {
			if (attackIntervalTimer > attackInterval) {
				if (Vector3.Distance(transform.position, target.transform.position) < attack.range) {
					doAttack();
					attackIntervalTimer = 0;
				}
			}
		}

		attackIntervalTimer += Time.deltaTime;
	}

	void doAttack() {
		attack.execute();
	}

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
		GetComponent<BoxCollider2D>().enabled = false;
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
