using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	private Animator animator;
	private Rigidbody2D rigidbod;
	private PlayerHealth playerHealth;

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

	// Attack parameters
	public int attackDamage = 1;
	public float attackRadius = 0.5f;
	public float attackForce = 10;
	private bool isAttacking = false;
	private float attackTimer = 0;
	private float attackDuration = 0.16f;

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
			if (!isAttacking) {
				if (Input.GetKeyDown(KeyCode.X)) {
					if (stamina-attackStaminaCost > 0) {
						isAttacking = true;
						animator.SetTrigger("attackTrigger");
						stamina -= attackStaminaCost;
						doAttack();
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
			} else {
				attackTimer += Time.deltaTime;
				if (attackTimer > attackDuration) {
					attackTimer = 0;
					isAttacking = false;
				}
				doAttack();
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
			if (!isAttacking) {
				if (!isBlocking) {
					doMovement();
				}
			}
		} else {
			rigidbod.AddForce(rollDirection);
		}

		maxSpeedTrim();
	}

	void OnCollisionEnter2D (Collision2D col) {
		switch(col.gameObject.tag) {
		case "Pickup_heart":
			if (playerHealth.hitpoints < playerHealth.maxHitpoints) {
				playerHealth.changeHitpointsBy(1);
				Destroy(col.gameObject);
				AudioSource.PlayClipAtPoint((AudioClip)Resources.Load("sounds/heart_pickup"), transform.position, 0.6f);
			} else if (playerHealth.hitpoints == playerHealth.maxHitpoints && playerHealth.maxHitpoints < playerHealth.hitpointsLimit) {
				playerHealth.increaseMaxHitpointsBy(1);
				Destroy(col.gameObject);
				AudioSource.PlayClipAtPoint((AudioClip)Resources.Load("sounds/heart_pickup"), transform.position, 0.6f);
			}
			break;
		case "Soul":
			souls += 10;
			col.gameObject.GetComponent<Animator>().SetTrigger("deathTrigger");
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

	void doAttack() {
		Collider2D[] hitColliders = Physics2D.OverlapCircleAll(GetComponent<Renderer>().bounds.center, attackRadius);
		foreach (var collider in hitColliders) {
			if (collider.gameObject.tag == "Enemy") {
				if ((isFacingRight && (collider.gameObject.transform.position-transform.position).x > 0) || 
					(!isFacingRight && (collider.gameObject.transform.position-transform.position).x < 0)) {
					collider.gameObject.GetComponent<EnemyHealth>().SendMessage("takeDamage", this);
				} 
			}
		}
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

	void takeDamage(EnemyController enemy) {
		if (Time.time > lastHitTime+repeatDamagePeriod) {
			if (!isRolling) {
				if (isBlocking) {
					if ((isFacingRight && (enemy.gameObject.transform.position-transform.position).x > 0) ||
						(!isFacingRight && (enemy.gameObject.transform.position-transform.position).x < 0)) {
						stamina -= blockStaminaCost;
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
		playerHealth.changeHitpointsBy(-1);
		animator.SetTrigger("damageTrigger");
		if (playerHealth.hitpoints <= 0) {
			onDeath();
		} else {
			lastHitTime = Time.time;
			GetComponent<SoundController>().playHurtSound();
		}
	}

	void onDeath() {
		GetComponent<SoundController>().playDeathSound();
		animator.SetTrigger("deathTrigger");

		GetComponent<BoxCollider2D>().enabled = false;
		GetComponent<SpriteRenderer>().sortingLayerName = "Foliage";
		GetComponent<SpriteRenderer>().sortingOrder = Random.Range(0, 255);
		this.enabled = false;
	}

	void onRespawn() {
		GetComponent<PlayerController>().enabled = true;
		GetComponent<BoxCollider2D>().enabled = true;
		GetComponent<SpriteRenderer>().sortingLayerName = "Player";
		GetComponent<SpriteRenderer>().sortingOrder = 0;

		playerHealth.changeHitpointsBy(playerHealth.maxHitpoints/2);
		animator.Play("idle");
	}
}