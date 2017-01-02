using UnityEngine;
using System.Collections;

public class BonfireSafeZone : MonoBehaviour {

	private GameObject[] spawners;
	private GameObject[] enemies;
	private GameObject player;
	private bool atSafeZone = true;

	public float restoreHitpointsInterval = 5f;
	private float restoreHitpointsTimer;

	// Use this for initialization
	void Start () {
		player = GameObject.FindGameObjectWithTag("Player");
		spawners = GameObject.FindGameObjectsWithTag("Spawner");

		restoreHitpointsTimer = restoreHitpointsInterval;
	}
	
	// Update is called once per frame
	void Update () {
		if (Vector3.Distance(player.transform.position, transform.position) <= 2) {
			restoreHitpointsTimer -= Time.deltaTime;
			if (!atSafeZone) {
				setEnemiesAndSpawnersAttackTo(false);
				atSafeZone = true;
			}
			if (restoreHitpointsTimer < 0) {
				player.GetComponent<PlayerHealth>().changeHitpointsBy(1);
				restoreHitpointsTimer = restoreHitpointsInterval;
			}
		} else {
			if (atSafeZone) {
				restoreHitpointsTimer = restoreHitpointsInterval;
				setEnemiesAndSpawnersAttackTo(true);
				atSafeZone = false;
			}
		}
	}

	void setEnemiesAndSpawnersAttackTo(bool state) {
		foreach (var spawner in spawners) {
			spawner.gameObject.GetComponent<ObjectSpawner>().enabled = state;
		}
		foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy")) {
			if (enemy.GetComponent<EnemyHealth>().hitpoints > 0) {
				enemy.GetComponent<EnemyController>().SendMessage("switchAttackStateTo", state);
			}
		}
	}
}
