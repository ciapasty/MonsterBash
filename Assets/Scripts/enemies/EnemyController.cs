﻿using UnityEngine;
using System;
using System.Collections;

public class EnemyController : MonoBehaviour {

	public Enemy enemy;

	private Animator animator;
	private Rigidbody2D rigidbod;
	private EnemyHealth enemyHealth;

	private Attack[] attacks;

	public GameObject target;

	public float repeatDamagePeriod = 0.5f;
	private float lastHitTime;

	public int soulsCarried = 10;

	Action<Enemy> cbOnDeath;

	void Start () {
		animator = GetComponent<Animator>();
		rigidbod = GetComponent<Rigidbody2D>();
		enemyHealth = GetComponent<EnemyHealth>();

		target = GameObject.FindGameObjectWithTag("Player");

		attacks = GetComponents<Attack>();
	}

	void Update() {
		if (attacks.Length > 0) {
			foreach (var attk in attacks) {
				if (attk.cooldown <= 0) {
					if (Vector3.Distance(target.transform.position, transform.position) < attk.range) {
						if (attk as ProjectileAttack) {
							Vector3 direction = (target.transform.position-transform.position);
							(attk as ProjectileAttack).direction = direction/direction.magnitude;
						}
						attk.execute();
					}
				}
			}
		}

		if (Mathf.Abs(rigidbod.velocity.x) > 0.1f || Mathf.Abs(rigidbod.velocity.y) > 0.1f) {
			GetComponent<SpriteRenderer>().flipX = !(rigidbod.velocity.x > 0);
			animator.SetBool("isWalking", true);
		} else {
			animator.SetBool("isWalking", false);
		}
	}

	public void switchAttackStateTo(bool state) {
		GetComponent<MoveTowardsTarget>().enabled = state;
		GetComponent<MoveIdle>().enabled = !state;
	}

	// Souls and non-colliding projectiles
	void OnTriggerEnter2D(Collider2D coll) {
		switch(coll.gameObject.tag) {
		case "Projectile":
			Attack attk = coll.gameObject.GetComponent<Projectile>().attack;
			if (attk.gameObject != gameObject && attk.gameObject.tag == "Player") {
				onHit(attk);
				coll.gameObject.GetComponent<Animator>().SetTrigger("deathTrigger");
			}
			break;
		default:
			break;
		}
	}

	void onHit(Attack attack) {
		if (Time.time > lastHitTime+repeatDamagePeriod) {
			// No advanced hit detection now :(
			takeDamage(attack.damage);
			Vector3 hitVector = transform.position-attack.gameObject.transform.position;
			rigidbod.AddForce(hitVector*attack.force*100);

			lastHitTime = Time.time;

			if (GetComponent<MoveIdle>().enabled) {
				GetComponent<MoveIdle>().enabled = false;
				GetComponent<MoveTowardsTarget>().enabled = true;
			}
		}
	}

	void takeDamage(int damage) {
		animator.SetTrigger("damageTrigger");
		if (GetComponentInChildren<ParticleSystem>() != null) {
			GetComponentInChildren<ParticleSystem>().Play();
		}
		enemyHealth.changeHitpointsBy(-1);
		if (enemyHealth.isDead) {
			onDeath();
		}
	}

	void onDeath() {
		animator.SetTrigger("deathTrigger");
		GetComponent<Collider2D>().enabled = false;
		GetComponent<MoveIdle>().enabled = false;
		GetComponent<MoveTowardsTarget>().enabled = false;

		// TODO: Spawn dead body GO with timed death -> prefab

		GameObject soul = (GameObject)Instantiate(Resources.Load("prefabs/soul"), transform.position, Quaternion.identity);
		soul.GetComponent<Soul>().souls = soulsCarried;

		// Callback
		if (cbOnDeath != null)
			cbOnDeath(this.enemy);
		// Remove references
		cbOnDeath = null;
		enemy = null;

		Destroy (gameObject, 5);
		this.enabled = false;
	}

	public void registerOnChangedCallback(Action<Enemy> callback) {
		cbOnDeath += callback;
	}

	public void unregisterOnChangedCallback(Action<Enemy> callback) {
		cbOnDeath -= callback;
	}
}
