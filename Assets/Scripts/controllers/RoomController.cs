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

	Dictionary<Blueprint, GameObject> enemyGoMap;
	Dictionary<Blueprint, GameObject> objectGoMap;
	Dictionary<Door, GameObject> doorGoMap;

	public List<GameObject> garbageGOs;

	void Awake() {
		cameraFollowPlayer = GameObject.FindGameObjectWithTag("PlayerCamera").GetComponent<CameraFollowPlayer>();
	}

	public void playerEntered() {
		if (enemyGoMap.Count > 0) {
			cameraFollowPlayer.snapToRoom(room, true);
			lockDoors(true);
			foreach (var enemy in enemyGoMap.Keys) {
				enemyGoMap[enemy].GetComponent<EnemyController>().activateEnemy();
			}
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
			door_go.transform.position = new Vector2(door.tile.x, door.tile.y);
			door_go.GetComponent<DoorController>().door = door;
			door_go.GetComponent<DoorController>().setupBoxCollider();
			doorGoMap.Add(door, door_go);
			door.registerOnChangedCallback(door_go.GetComponent<DoorController>().onStateChanged);
			door.cbOnChanged(door);
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

	void createObjects() {
		objectGoMap = new Dictionary<Blueprint, GameObject>();
		garbageGOs = new List<GameObject>();
		foreach (var obj in room.objects) {
			GameObject obj_go = (GameObject)Instantiate(obj.prefab, transform.position, Quaternion.identity);
			obj_go.transform.SetParent(this.transform);
			obj_go.transform.position = new Vector2(obj.spawnTile.x+0.5f, obj.spawnTile.y+0.5f);
			obj_go.GetComponent<ObjectSmashed>().rc = this;
			objectGoMap.Add(obj, obj_go);
		}
	}

	void removeObjects() {
		foreach (var obj in room.objects) {
			Destroy(objectGoMap[obj]);
			objectGoMap.Remove(obj);
		}
		objectGoMap = null;
		foreach (var trash in garbageGOs) {
			Destroy(trash);
		}
		garbageGOs = null;
	}

	void spawnEnemies() {
		enemyGoMap = new Dictionary<Blueprint, GameObject>();
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
				if (enemyGoMap.ContainsKey(enemy)) {
					enemyGoMap[enemy].GetComponent<EnemyController>().unregisterOnChangedCallback(enemyDiedCallback);
					enemyGoMap[enemy].GetComponent<EnemyController>().enemy = null;
					Destroy(enemyGoMap[enemy]);
					enemyGoMap.Remove(enemy);
				}
			}
		}
		enemyGoMap = null;
	}

	public void lockDoors(bool locked) {
		if (room.doors.Count > 0) {
			Debug.Log("Locking doors in room: "+room.ID);
			foreach (var door in room.doors) {
				door.isLocked = locked;
			}
		}
	}

	void enemyDiedCallback(Blueprint enemy) {
		enemyGoMap.Remove(enemy);
		Debug.Log("Enemies left: "+enemyGoMap.Count);
		if (enemyGoMap.Count == 0) {
			cameraFollowPlayer.snapToRoom(null, false);
			lockDoors(false);
		}
	}
}
