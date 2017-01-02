using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour {

	private Animator animator;
	private Rigidbody2D rigidbod;
	private EnemyHealth enemyHealth;
	private GameObject player;

	public float attackRadius = 0.3f;
	public float attackForce = 10;
	public float attackInterval = 2f;
	private float attackIntervalTimer = 0f;
	private bool isAttacking = false;
	private float attackDuration = 0.2f;
	private float attackDurationTimer = 0f;

	public int soulsCarried = 10;

	void Start () {
		animator = GetComponent<Animator>();
		rigidbod = GetComponent<Rigidbody2D>();
		enemyHealth = GetComponent<EnemyHealth>();
	}

	void Update() {
		// Attack timer
		if (!isAttacking) {
			if (attackIntervalTimer <= 0) {
				doAttack();
			}
		} else {
			attackDurationTimer += Time.deltaTime;
			if (attackDurationTimer > attackDuration) {
				attackDurationTimer = 0;
				isAttacking = false;
			}
			doAttack();
		}

		if (attackIntervalTimer > 0) {
			attackIntervalTimer -= Time.deltaTime;
		}
	}

	void doAttack() {
		Vector3 attkCircleCenter = GetComponent<Renderer>().bounds.center;
		attkCircleCenter.y -= 0.1f;
		Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attkCircleCenter, attackRadius);
		foreach (var collider in hitColliders) {
			if (collider.gameObject.tag == "Player") {
				//Debug.Log(collider.gameObject.name);
				animator.SetTrigger("attackTrigger");
				collider.gameObject.GetComponent<PlayerHealth>().SendMessage("takeDamage", this);
				attackIntervalTimer = attackInterval;
			}
		}
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

	void takeDamage(PlayerController player) {
		Vector3 hitVector = transform.position - player.gameObject.transform.position;
		rigidbod.AddForce(hitVector * player.attackForce * 100);
		enemyHealth.changeHitpointsBy(-1);
		if (enemyHealth.hitpoints <= 0) {
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
