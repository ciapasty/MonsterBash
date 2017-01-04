using UnityEngine;
using System.Collections;

public class Attack : MonoBehaviour {

	public int damage = 1;
	// The radius of damage dealt (red gizmo)
	public float radius = 0.3f;
	public float verticalRadiusShift = -0.1f;
	public float horizontalRadiusShift = 0f;
	// Physical force applied to player on hit
	public float force = 10;
	// Distance between enemy center to character center, at which enemy should attack (orange gizmo)
	public float range = 3;

	// Time between attacks
	public float cooldownTime = 2f;
	public float cooldown { get; protected set; }

	// Time of damage being dealt in radius
	protected float duration = 0.1f;
	protected float durationTimer = 0f;

	public bool isAttacking { get; protected set; }

	protected Animator animator;

	void Start() {
		animator = GetComponent<Animator>();
		isAttacking = false;
		cooldown = 0f;
	}

	void Update() {
		if (cooldown > 0) {
			cooldown -= Time.deltaTime;
		}
	}

	virtual public void execute(GameObject target) {}

}