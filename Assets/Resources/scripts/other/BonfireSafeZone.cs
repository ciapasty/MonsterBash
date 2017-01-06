using UnityEngine;
using System.Collections;

public class BonfireSafeZone : MonoBehaviour {

	private GameObject[] spawners;
	private GameObject[] enemies;
	private GameObject player;
	private bool atSafeZone = true;

	public float sitDownTime = 5f;
	private float timer;

	// Use this for initialization
	void Start () {
		player = GameObject.FindGameObjectWithTag("Player");
		spawners = GameObject.FindGameObjectsWithTag("Spawner");

		timer = sitDownTime;
	}
	
	// Update is called once per frame
	void Update () {
		if (Vector3.Distance(player.transform.position, transform.position) <= 2) {
			timer -= Time.deltaTime;
			if (!atSafeZone) {
				setEnemiesAndSpawnersAttackTo(false);
				atSafeZone = true;
			}
			if (timer < 0) {
				if (!player.GetComponent<PlayerHealth>().isDead) {
					if(!player.GetComponent<PlayerSitDown>().isSitting) {
						player.SendMessage("sitDown");
					}
				} else {
					timer = sitDownTime;
				}
			}
			if (Input.anyKeyDown) {
				timer = sitDownTime;
			}

		} else {
			if (atSafeZone) {
				timer = sitDownTime;
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
