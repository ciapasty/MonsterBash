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

	void Awake() {
		cameraFollowPlayer = GameObject.FindGameObjectWithTag("PlayerCamera").GetComponent<CameraFollowPlayer>();
	}

	public void playerEntered() {
		if (enemyGoMap.Count > 0) {
			cameraFollowPlayer.snapToRoom(room, true);
			lockDoors(true);
		}
	}

	public void createInRoomGOs() {
		enemyGoMap = new Dictionary<Enemy, GameObject>();
		foreach (var door in room.doors) {
			GameObject door_go = (GameObject)Instantiate(Resources.Load<GameObject>("prefabs/door"), transform.position, Quaternion.identity);
			door_go.transform.SetParent(this.transform);
			door_go.transform.position = new Vector2(door.tile.x+0.5f, door.tile.y+0.5f);
			door_go.GetComponent<DoorController>().door = door;
			door.registerOnChangedCallback(door_go.GetComponent<DoorController>().onStateChanged);
			door.cbOnChanged(door);
		}
		foreach (var enemy in room.enemies) {
			GameObject enemy_go = (GameObject)Instantiate(enemy.prefab, transform.position, Quaternion.identity);
			enemy_go.transform.SetParent(this.transform);
			enemy_go.transform.position = new Vector2(enemy.spawnTile.x+0.5f, enemy.spawnTile.y+0.5f);
			enemy_go.GetComponent<EnemyController>().enemy = enemy;
			enemy_go.GetComponent<EnemyController>().registerOnChangedCallback(enemyDiedCallback);
			enemyGoMap.Add(enemy, enemy_go);
		}
	}

	void lockDoors(bool locked) {
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
