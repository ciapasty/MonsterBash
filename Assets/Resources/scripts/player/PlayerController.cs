using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	private Animator animator;
	private Rigidbody2D rigidbod;
	private PlayerHealth playerHealth;

	private MeleeAttack meleeAttack;
	private ProjectileAttack rangedAttack;

	// Block parameters
	private bool _isBlocking = false;
	public bool isBlocking {
		get {
			return _isBlocking;
		}
		set {
			_isBlocking = value;
			animator.SetBool("isBlocking", value);
		}
	}
	public bool isFacingRight {
		get {
			return !GetComponent<SpriteRenderer>().flipX;
		}
		set {
			GetComponent<SpriteRenderer>().flipX = !value;
		}
	}
	// Stamina parameters
	private float _stamina;
	public float stamina {
		get {
			return _stamina;
		}
		set {
			_stamina = value;
			GameObject.FindGameObjectWithTag("UI_StaminaBar").GetComponent<StaminaBarControl>().SendMessage("updateStamina", value);
		}
	}
	public float maxStamina = 100f;
	public float staminaRegenRate = 10f;
	public float staminaRegenRateBlocking = 5f;
	public float rollingStaminaCost = 10f;
	public float attackStaminaCost = 10f;
	public float blockStaminaCost = 5f;

	// Movement forces, speed
	public float walkingSpeed = 3f;
	public float rollingSpeed = 6f;

	// Dodge roll parameters
	public bool isRolling { get; protected set; }
	private float rollTimer = 0;
	private float rollDuration = 0.5f;
	private Vector2 rollDirection;

	public float repeatDamagePeriod = 0.5f;
	private float lastHitTime;

	// SOULS
	private int _souls;
	public int souls {
		get {
			return _souls;
		}
		set {
			_souls = value;
			GameObject.FindGameObjectWithTag("UI_SoulsCount").GetComponent<UnityEngine.UI.Text>().text = value.ToString();
		}
	}

	void Start () {
		animator = GetComponent<Animator>();
		rigidbod = GetComponent<Rigidbody2D>();
		playerHealth = GetComponent<PlayerHealth>();

		meleeAttack = GetComponent<MeleeAttack>();
		rangedAttack = GetComponent<ProjectileAttack>();

		GameObject.FindGameObjectWithTag("UI_StaminaBar").GetComponent<StaminaBarControl>().player = gameObject;
		stamina = maxStamina;
	}
	
	void Update () {
		// Stamina regeneration
		if (stamina < maxStamina) {
			if (!isRolling) {
				if (!isBlocking) {
					stamina += staminaRegenRate*Time.deltaTime;
				} else {
					stamina += staminaRegenRateBlocking*Time.deltaTime;
				}
			}
		}

		if (!isRolling) {
			// Attack
			if (!meleeAttack.isAttacking && !rangedAttack.isAttacking) {
				// Melee attack
				if (Input.GetButtonDown("Attack1")) {
					if (meleeAttack.cooldown <= 0) {
						if (stamina-attackStaminaCost > 0) {
							//animator.SetTrigger("attackTrigger");
							stamina -= attackStaminaCost;
							meleeAttack.execute();
						}
					}
				}
				if (Input.GetButtonDown("Attack2")) {
					if (rangedAttack.cooldown <= 0) {
						if (stamina-attackStaminaCost > 0) {
							animator.SetTrigger("attackTrigger");
							stamina -= attackStaminaCost;

							if (rigidbod.velocity.magnitude > 0) {
								rangedAttack.direction = rigidbod.velocity/rigidbod.velocity.magnitude;
							} else {
								rangedAttack.direction = isFacingRight ? Vector3.right : Vector3.left;
							}
							rangedAttack.execute();
						}
					}
				}
				// Blocking -> cannot move
				if (Input.GetButton("Block1")) {
					if (stamina > 1) {
						isBlocking = true;
						animator.SetBool("isWalking", false);
					} else {
						isBlocking = false;
					}
				} else {
					isBlocking = false;
				}
			}
		} else {
			rollTimer += Time.deltaTime;
			if (rollTimer > rollDuration) {
				rollTimer = 0;
				isRolling = false;
			}
		}
	}

	// Movement is done by physics, FixedUpdate is recommended
	void FixedUpdate () {
		if (!isRolling) {
			if (!meleeAttack.isAttacking && !rangedAttack.isAttacking) {
				if (!isBlocking) {
					doMovement();
				}
			}
		} else {
			rigidbod.velocity = rollDirection.normalized*rollingSpeed;
		}
		clampMovement();
	}

	// Pickups
	void OnCollisionEnter2D (Collision2D coll) {
		switch(coll.gameObject.tag) {
		case "Pickup_heart":
			if (playerHealth.lockedHitpoints) {
				if (playerHealth.hitpoints < (playerHealth.maxHitpoints+1)/2) {
					playerHealth.changeHitpointsBy(1);
				} else {
					break;
				}
			} else {
				if (playerHealth.hitpoints < playerHealth.maxHitpoints) {
					playerHealth.changeHitpointsBy(1);
				} else if (playerHealth.hitpoints == playerHealth.maxHitpoints && playerHealth.maxHitpoints < playerHealth.hitpointsLimit && !playerHealth.lockedHitpoints) {
					playerHealth.increaseMaxHitpointsBy(1);
				} else {
					break;
				}
			}
			AudioSource.PlayClipAtPoint((AudioClip)Resources.Load("sounds/heart_pickup"), transform.position, 0.6f);
			Destroy(coll.gameObject);
			break;
		default:
			break;
		}
	}


	// Souls and non-colliding projectiles
	void OnTriggerEnter2D(Collider2D coll) {
		switch(coll.gameObject.tag) {
		case "Player_Soul":
			playerHealth.lockedHitpoints = false;
			playerHealth.changeHitpointsBy(0);
			goto case "Soul";
		case "Soul":
			souls += coll.gameObject.GetComponent<Soul>().souls;
			coll.gameObject.GetComponent<Animator>().SetTrigger("deathTrigger");
			break;
		case "Projectile":
			Attack attk = coll.gameObject.GetComponent<Projectile>().attack;
			if (attk.gameObject != gameObject) {
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
			if (!isRolling) {
				Vector3 hitVector = transform.position-attack.transform.position;
				if (isBlocking) {
					if ((isFacingRight && (attack.gameObject.transform.position-transform.position).x > 0) ||
						(!isFacingRight && (attack.gameObject.transform.position-transform.position).x < 0)) {
						stamina -= blockStaminaCost;
						rigidbod.AddForce(hitVector*attack.force*50);
						GetComponent<SoundController>().playBlockSound();
						return;
					}
				}
				rigidbod.AddForce(hitVector*attack.force*100);
				takeDamage(attack.damage);
			}
			lastHitTime = Time.time;
		}
	}

	void onDeath() {
		GetComponent<SoundController>().playDeathSound();
		animator.SetTrigger("deathTrigger");

		// Check if there is any other player_soul -> spawn player soul
		GameObject pSoul = GameObject.FindGameObjectWithTag("Player_Soul");
		if (pSoul != null) {
			pSoul.SendMessage("killMe");
		}

		GameObject soul = (GameObject)Instantiate(Resources.Load("prefabs/player_soul"), transform.position, Quaternion.identity);
		soul.GetComponent<Soul>().souls = souls;
		souls = 0;

		GetComponent<BoxCollider2D>().enabled = false;
		enabled = false;
	}

	void onRespawn() {
		GetComponent<PlayerController>().enabled = true;
		GetComponent<BoxCollider2D>().enabled = true;

		playerHealth.changeHitpointsBy((playerHealth.maxHitpoints+1)/2);
		animator.Play("idle");
	}

	void doMovement() {
		float horizontalInput = Input.GetAxisRaw("Horizontal");
		float verticalInput = Input.GetAxisRaw("Vertical");

		animator.SetBool("isWalking", (horizontalInput != 0 || verticalInput != 0) );

		if (horizontalInput != 0)
			isFacingRight = (horizontalInput > 0);

		Vector2 moveDirection = new Vector2(horizontalInput, verticalInput);

		rigidbod.velocity = moveDirection.normalized*walkingSpeed;

		// Dodge roll
		if (Input.GetButtonDown("Dodge")) {
			if ((stamina-rollingStaminaCost) > 0) {
				animator.SetTrigger("rollTrigger");
				if (!isRolling) {
					isRolling = true;
					rollDirection = new Vector2(horizontalInput, verticalInput);
					stamina -= rollingStaminaCost;
				}
			}
		}
	}

	void clampMovement() {
		Vector2 maxXY = Camera.main.ViewportToWorldPoint(new Vector2(1, 1));
		maxXY.x = maxXY.x-GetComponent<SpriteRenderer>().bounds.extents.x;
		maxXY.y = maxXY.y-GetComponent<SpriteRenderer>().bounds.extents.y*2;
		Vector2 minXY = Camera.main.ViewportToWorldPoint(new Vector2(0, 0));
		minXY.x = minXY.x+GetComponent<SpriteRenderer>().bounds.extents.x;
		Vector3 pos = transform.position;

		if ((pos.x >= maxXY.x && rigidbod.velocity.x > 0) || (pos.x <= minXY.x && rigidbod.velocity.x < 0)) {
			rigidbod.velocity = new Vector2(0, rigidbod.velocity.y);
		}
		if ((pos.y >= maxXY.y && rigidbod.velocity.y > 0) || (pos.y <= minXY.y && rigidbod.velocity.y < 0)) {
			rigidbod.velocity = new Vector2(rigidbod.velocity.x, 0);
		}
	}

	void takeDamage(int damage) {
		playerHealth.changeHitpointsBy(-damage);
		animator.SetTrigger("damageTrigger");
		GetComponentInChildren<ParticleSystem>().Play();
		if (playerHealth.isDead) {
			onDeath();
		} else {
			GetComponent<SoundController>().playHurtSound();
		}
	}
}