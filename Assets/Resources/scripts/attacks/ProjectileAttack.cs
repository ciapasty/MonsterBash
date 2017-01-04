using UnityEngine;
using System.Collections;

public class ProjectileAttack : Attack {

	public GameObject projectilePrefab;

	public override void execute() {
		foreach (var tag in go_tags) {
			GameObject projectile = (GameObject)Instantiate(projectilePrefab, transform.position, Quaternion.identity);
			projectile.transform.SetParent(transform);
			projectile.GetComponent<Projectile>().attack = this;

			// TODO: Pass direction differently!
			Vector3 direction = (GameObject.FindGameObjectWithTag(tag).transform.position-transform.position);
			projectile.GetComponent<MoveInDirection>().direction = direction/direction.magnitude;

			cooldown = cooldownTime;
		}
	}
}
