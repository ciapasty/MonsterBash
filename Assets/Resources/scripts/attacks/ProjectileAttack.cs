using UnityEngine;
using System.Collections;

public class ProjectileAttack : Attack {

	public float projectileSpeed;
	public Vector3 direction;
	public GameObject projectilePrefab;

	public override void execute() {
		foreach (var tag in go_tags) {
			GameObject projectile = (GameObject)Instantiate(projectilePrefab, GetComponent<Renderer>().bounds.center, Quaternion.identity);
			projectile.transform.SetParent(transform);
			projectile.GetComponent<Projectile>().attack = this;

			MoveInDirection move = projectile.GetComponent<MoveInDirection>();
			move.force = force;
			move.speed = projectileSpeed;

			move.direction = direction;

			// range/0.9*speed*force
			projectile.GetComponent<TimedDestroy>().time = range/(projectileSpeed*force);

			cooldown = cooldownTime;
		}
	}
}
