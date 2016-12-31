using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	private Animator animator;
	private Rigidbody2D rigidbod;

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
	public float attackRadius = 0.65f;
	public float attackForce = 10;
	private bool isAttacking = false;
	private float attackTimer = 0;
	private float attackDuration = 0.2f;

	void Start () {
		animator = GetComponent<Animator>();
		rigidbod = GetComponent<Rigidbody2D>();

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
}