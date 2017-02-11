using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// 
/// - Controlls doors, lock/unlock
/// - Spawns/wakes enemies
/// - Tracks enemies status
///

/// <summary>
/// Class controlling room behavior
/// </summary>
public class RoomController : MonoBehaviour {

	public Room room;

	CameraFollowPlayer cameraFollowPlayer;

	Dictionary<Enemy, GameObject> enemyGoMap;
	Dictionary<Door, GameObject> doorGoMap;

	void Awake() {
		cameraFollowPlayer = GameObject.FindGameObjectWithTag("PlayerCamera").GetComponent<CameraFollowPlayer>();
	}

	public void playerEntered() {
		if (enemyGoMap.Count > 0) {
			cameraFollowPlayer.snapToRoom(room, true);
			lockDoors(true);
		}
	}

	public void createGOs() {
		createDoors();
		createObjects();
		spawnEnemies();
	}

	public void removeGOs() {
		removeDoors();
		removeEnemies();
		room = null;
	}

	void createDoors() {
		doorGoMap = new Dictionary<Door, GameObject>();
		foreach (var door in room.doors) {
			GameObject door_go = (GameObject)Instantiate(Resources.Load<GameObject>("prefabs/door"), transform.position, Quaternion.identity);
			door_go.transform.SetParent(this.transform);
			door_go.transform.position = new Vector2(door.tile.x+0.5f, door.tile.y+0.5f);
			door_go.GetComponent<DoorController>().door = door;
			doorGoMap.Add(door, door_go);
			door.registerOnChangedCallback(door_go.GetComponent<DoorController>().onStateChanged);
			door.cbOnChanged(door);
		}
	}

	void createObjects() {
		//enemyGoMap = new Dictionary<Enemy, GameObject>();
		foreach (var obj in room.objects) {
			GameObject obj_go = (GameObject)Instantiate(obj.prefab, transform.position, Quaternion.Euler(new Vector3(0f, 0f, Random.Range(0f, 90f))));
			obj_go.transform.SetParent(this.transform);
			obj_go.transform.position = new Vector2(obj.spawnTile.x+0.5f, obj.spawnTile.y+0.5f);
//			obj_go.GetComponent<EnemyController>().enemy = obj;
//			obj_go.GetComponent<EnemyController>().registerOnChangedCallback(enemyDiedCallback);
//			enemyGoMap.Add(obj, obj_go);
		}
	}

	void removeDoors() {
		foreach (var door in room.doors) {
			door.cbOnChanged = null;
			doorGoMap[door].GetComponent<DoorController>().door = null;
			Destroy(doorGoMap[door]);
			doorGoMap.Remove(door);
		}
		doorGoMap = null;
	}

	void spawnEnemies() {
		enemyGoMap = new Dictionary<Enemy, GameObject>();
		foreach (var enemy in room.enemies) {
			GameObject enemy_go = (GameObject)Instantiate(enemy.prefab, transform.position, Quaternion.identity);
			enemy_go.transform.SetParent(this.transform);
			enemy_go.transform.position = new Vector2(enemy.spawnTile.x+0.5f, enemy.spawnTile.y+0.5f);
			enemy_go.GetComponent<EnemyController>().enemy = enemy;
			enemy_go.GetComponent<EnemyController>().registerOnChangedCallback(enemyDiedCallback);
			enemyGoMap.Add(enemy, enemy_go);
		}
	}

	public void respawnEnemies() {
		if (enemyGoMap.Count > 0) {
			foreach (var enemy in enemyGoMap.Keys) {
				Destroy(enemyGoMap[enemy]);
			}
		}
		spawnEnemies();
	}

	void removeEnemies() {
		if (enemyGoMap.Count > 0) {
			foreach (var enemy in room.enemies) {
				enemyGoMap[enemy].GetComponent<EnemyController>().unregisterOnChangedCallback(enemyDiedCallback);
				enemyGoMap[enemy].GetComponent<EnemyController>().enemy = null;
				Destroy(enemyGoMap[enemy]);
				enemyGoMap.Remove(enemy);
			}
		}
		enemyGoMap = null;
	}

	public void lockDoors(bool locked) {
		Debug.Log("Locking doors in room: "+room.ID);
		foreach (var door in room.doors) {
			door.isLocked = locked;
		}
	}

	void enemyDiedCallback(Enemy enemy) {
		enemyGoMap.Remove(enemy);
		Debug.Log("Enemies left: "+enemyGoMap.Count);
		if (enemyGoMap.Count == 0) {
			cameraFollowPlayer.snapToRoom(null, false);
			lockDoors(false);
		}
	}
}
