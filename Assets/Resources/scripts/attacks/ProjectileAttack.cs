using UnityEngine;
using System.Collections;

public class ProjectileAttack : Attack {

	public GameObject projectilePrefab;

	public override void execute(GameObject target) {
		//base.execute();

		GameObject projectile = (GameObject)Instantiate(projectilePrefab, transform.position, Quaternion.identity);
		projectile.transform.SetParent(transform);
		projectile.GetComponent<Projectile>().attack = this;
		Vector3 direction = (target.transform.position-transform.position);
		projectile.GetComponent<MoveInDirection>().direction = direction/direction.magnitude;

		cooldown = cooldownTime;

	}
}
