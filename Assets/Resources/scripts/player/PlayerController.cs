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
	public float walkingForce = 100f;
	public float rollForce = 200f;
	public float maxWalkingSpeed = 3f;
	public float maxRollingSpeed = 6f;

	private float moveForce = 0;
	private float maxMoveSpeed = 0;

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

		moveForce = walkingForce;
		maxMoveSpeed = maxWalkingSpeed;
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
				if (Input.GetKeyDown(KeyCode.X)) {
					if (meleeAttack.cooldown <= 0) {
						if (stamina-attackStaminaCost > 0) {
							animator.SetTrigger("attackTrigger");
							stamina -= attackStaminaCost;
							meleeAttack.execute();
						}
					}
				}
				if (Input.GetKeyDown(KeyCode.Z)) {
					if (rangedAttack.cooldown <= 0) {
						if (stamina-attackStaminaCost > 0) {
							animator.SetTrigger("attackTrigger");
							stamina -= attackStaminaCost;
							rangedAttack.execute();
						}
					}
				}
				// Blocking -> cannot move
				if (Input.GetKey(KeyCode.LeftShift)) {
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

				moveForce = walkingForce;
				maxMoveSpeed = maxWalkingSpeed;
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
			rigidbod.AddForce(rollDirection);
		}

		maxSpeedTrim();
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
		}
	}

	void onDeath() {
		GetComponent<SoundController>().playDeathSound();
		animator.SetTrigger("deathTrigger");

		// Check if there is any other player_soul -> spawn player soul
		GameObject pSoul = GameObject.FindGameObjectWithTag("Player_Soul");
		if (pSoul != null) {
			pSoul.GetComponent<Animator>().SetTrigger("deathTrigger");
		}

		GameObject soul = (GameObject)Instantiate(Resources.Load("prefabs/player_soul"), transform.position, Quaternion.identity);
		soul.GetComponent<Soul>().souls = souls;
		souls = 0;

		GetComponent<BoxCollider2D>().enabled = false;
		GetComponent<SpriteRenderer>().sortingLayerName = "Foliage";
		GetComponent<SpriteRenderer>().sortingOrder = Random.Range(0, 255);
		enabled = false;
	}

	void onRespawn() {
		GetComponent<PlayerController>().enabled = true;
		GetComponent<BoxCollider2D>().enabled = true;
		GetComponent<SpriteRenderer>().sortingLayerName = "Player";
		GetComponent<SpriteRenderer>().sortingOrder = 0;

		playerHealth.changeHitpointsBy((playerHealth.maxHitpoints+1)/2);
		animator.Play("idle");
	}

	void doMovement() {
		float horizontalInput = Input.GetAxisRaw("Horizontal");
		float verticalInput = Input.GetAxisRaw("Vertical");

		if (horizontalInput != 0 || verticalInput != 0) {
			if (horizontalInput != 0)
				isFacingRight = (horizontalInput > 0);
			animator.SetBool("isWalking", true);

			if (horizontalInput*rigidbod.velocity.x < maxMoveSpeed) {
				rigidbod.AddForce(Vector2.right*horizontalInput*moveForce);
			}

			if (verticalInput*rigidbod.velocity.y < maxMoveSpeed) {
				rigidbod.AddForce(Vector2.up*verticalInput*moveForce);
			}

			// Dodge roll
			if (Input.GetKeyDown(KeyCode.Space)) {
				if ((stamina-rollingStaminaCost) > 0) {
					animator.SetTrigger("rollTrigger");
					if (!isRolling) {
						isRolling = true;
						moveForce = rollForce;
						maxMoveSpeed = maxRollingSpeed;
						rollDirection = new Vector2(horizontalInput*rollForce, verticalInput*rollForce);
						stamina -= rollingStaminaCost;
					}
				}
			}

		} else {
			animator.SetBool("isWalking", false);
		}
	}

	void maxSpeedTrim() {
		if (Mathf.Abs(rigidbod.velocity.x) > maxMoveSpeed) {
			rigidbod.velocity = new Vector2(Mathf.Sign(rigidbod.velocity.x)*maxMoveSpeed, rigidbod.velocity.y);
		}

		if (Mathf.Abs(rigidbod.velocity.y) > maxMoveSpeed) {
			rigidbod.velocity = new Vector2(rigidbod.velocity.x, Mathf.Sign(rigidbod.velocity.y)*maxMoveSpeed);
		}
	}

	void takeDamage(int damage) {
		playerHealth.changeHitpointsBy(-damage);
		animator.SetTrigger("damageTrigger");
		GetComponent<ParticleSystem>().Play();
		if (playerHealth.isDead) {
			onDeath();
		} else {
			lastHitTime = Time.time;
			GetComponent<SoundController>().playHurtSound();
		}
	}
}