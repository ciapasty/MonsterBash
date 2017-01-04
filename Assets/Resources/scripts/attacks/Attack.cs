using UnityEngine;
using System.Collections;

public class Attack : MonoBehaviour {

	public int damage = 1;

	public float radius = 0.3f;
	public float force = 10;

	public float range = 3;

	protected float duration = 0.1f;
	protected float durationTimer = 0f;

	public bool isAttacking { get; protected set; }

	protected Animator animator;

	void Start() {
		animator = GetComponent<Animator>();

		isAttacking = false;
	}

	virtual public void execute() {}

}