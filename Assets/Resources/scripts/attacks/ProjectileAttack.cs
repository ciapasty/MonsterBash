using UnityEngine;
using System.Collections;

public class ProjectileAttack : Attack {

	public float projectileSpeed;
	public GameObject projectilePrefab;

	public override void execute() {
		foreach (var tag in go_tags) {
			GameObject projectile = (GameObject)Instantiate(projectilePrefab, transform.position, Quaternion.identity);
			projectile.transform.SetParent(transform);
			projectile.GetComponent<Projectile>().attack = this;

			MoveInDirection move = projectile.GetComponent<MoveInDirection>();
			move.force = force;
			move.speed = projectileSpeed;

			// !! TODO: Pass direction differently!
			Vector3 direction = (GameObject.FindGameObjectWithTag(tag).transform.position-transform.position);
			move.direction = direction/direction.magnitude;

			// range/0.9*speed*force
			projectile.GetComponent<TimedDestroy>().time = range/(projectileSpeed*force);

			cooldown = cooldownTime;
		}
	}
}
