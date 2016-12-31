using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour {

	//private Animator animator;
	private Rigidbody2D rigidbod;
	private GameObject player;

	public float moveForce = 10f;
	public float speed = 2;

	public float attackRadius = 0.3f;
	public float attackForce = 10;
	public float attackInterval = 2f;
	private float attackTimer = 0f;

	void Start () {
		//animator = GetComponent<Animator>();
		rigidbod = GetComponent<Rigidbody2D>();

		player = GameObject.FindGameObjectWithTag("Player");
	}

	void Update() {
		Vector3 attkCircleCenter = GetComponent<Renderer>().bounds.center;
		attkCircleCenter.y -= 0.1f;

		Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attkCircleCenter, attackRadius);
		foreach (var collider in hitColliders) {
			if (collider.gameObject.tag == "Player") {
				//Debug.Log(collider.gameObject.name);
				if (attackTimer <= 0) {
					collider.gameObject.GetComponent<PlayerHealth>().SendMessage("takeDamage", this);
					attackTimer = attackInterval;
				}
			}
		}

		if (attackTimer > 0) {
			attackTimer -= Time.deltaTime;
		}
	}
	
	void FixedUpdate () {
		// Dumb movement
		Vector3 direction = (player.transform.position - transform.position);
		rigidbod.AddForce(direction/direction.magnitude*speed*moveForce);


	}
}
