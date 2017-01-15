using UnityEngine;
using System.Collections;

public class ProjectileAttack : Attack {

	public float projectileSpeed;
	public Vector3 direction;
	public GameObject projectilePrefab;

	public override void execute() {
		animator.SetTrigger("attackTrigger");
		GameObject projectile = (GameObject)Instantiate(projectilePrefab, GetComponent<Renderer>().bounds.center, Quaternion.identity);
		projectile.transform.SetParent(transform);
		projectile.GetComponent<Projectile>().attack = this;

		MoveInDirection move = projectile.GetComponent<MoveInDirection>();
		move.speed = projectileSpeed;
		move.direction = direction;

		// v = s/t -> velocity = range/time -> time = range/velocity
		projectile.GetComponent<DestroyMe>().time = 1.2f*range/(direction.normalized.magnitude*projectileSpeed);

		cooldown = cooldownTime;
	}
}
