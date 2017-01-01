using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour {

	private Animator animator;
	private PlayerController playerController;

	public int totalMaxUltimateHitPoints = 12;
	public int maxHitpoints = 3;
	private int _hitpoints;
	public int hitpoints { 
		get {
			return _hitpoints;
		}
		protected set {
			_hitpoints = value;
			GameObject.FindGameObjectWithTag("UI_HealthBar").GetComponent<HealthBarControl>().SendMessage("updateHealth");
		}
	}
	public float repeatDamagePeriod = 0.5f;
	private float lastHitTime;

	private float restartTimer = 6f;

	void Start() {
		animator = GetComponent<Animator>();
		playerController = GetComponent<PlayerController>();

		hitpoints = maxHitpoints;
	}

	void Update() {
		if (hitpoints <= 0) {
			restartTimer -= Time.deltaTime;
		}

		if (restartTimer < 4) {
			GameObject.FindGameObjectWithTag("UI_YouDied").GetComponent<UnityEngine.UI.Text>().enabled = true;
		}

		if (restartTimer < 0) {
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
	}
	
	void OnCollisionEnter2D (Collision2D col) {
		switch(col.gameObject.tag) {
		case "Pickup_heart":
			if (hitpoints < maxHitpoints) {
				hitpoints += 1;
				Destroy(col.gameObject);
				AudioSource.PlayClipAtPoint((AudioClip)Resources.Load("sounds/heart_pickup"), transform.position, 0.6f);
			} else if (hitpoints == maxHitpoints && maxHitpoints < totalMaxUltimateHitPoints) {
				maxHitpoints += 1;
				hitpoints = hitpoints;
				Destroy(col.gameObject);
				AudioSource.PlayClipAtPoint((AudioClip)Resources.Load("sounds/heart_pickup"), transform.position, 0.6f);
			}
			break;
		default:
			break;
		}

		/*if (col.gameObject.tag == "Enemy") {
			if (Time.time > lastHitTime + repeatDamagePeriod) {
				if (hitPoints > 0) {
					takeDamage(col.transform);
					lastHitTime = Time.time;
				}
			}
		}*/
	}

	public void restoreHitpointsBy(int amount) {
		if (hitpoints > 0) {
			if (hitpoints+amount <= maxHitpoints) {
				hitpoints += amount;
			} else {
				hitpoints = maxHitpoints;
			}
		}
	}
	
	void takeDamage(EnemyController enemy) {
		if (Time.time > lastHitTime+repeatDamagePeriod) {
			if (!playerController.isRolling) {
				if (playerController.isBlocking) {
					if ((playerController.isFacingRight && (enemy.gameObject.transform.position-transform.position).x > 0) ||
					    (!playerController.isFacingRight && (enemy.gameObject.transform.position-transform.position).x < 0)) {
						playerController.stamina -= playerController.blockStaminaCost;
						GetComponent<SoundController>().playBlockSound();
					} else {
						onHit();
					}
				} else {
					onHit();
				}

				Vector3 hitVector = transform.position-enemy.transform.position;
				GetComponent<Rigidbody2D>().AddForce(hitVector*enemy.attackForce*100);
			}
		}
	}

	void onHit() {
		animator.SetTrigger("damageTrigger");
		hitpoints -= 1;
		if (hitpoints <= 0) {
			onDeath();
		} else {
			lastHitTime = Time.time;
			GetComponent<SoundController>().playHurtSound();
		}
	}

	void onDeath() {
		GetComponent<SoundController>().playDeathSound();
		animator.SetTrigger("deathTrigger");
		playerController.enabled = false;
		GetComponent<BoxCollider2D>().enabled = false;
		GetComponent<SpriteRenderer>().sortingLayerName = "Foliage";
	}
}
