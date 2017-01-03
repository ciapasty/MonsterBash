using UnityEngine;
using System.Collections;

public class PlayerRespawn : MonoBehaviour {

	public GameObject playerBodyPrefab;

	private PlayerHealth playerHealth;
	private float restartTime = 6f;
	private float timer = 0;

	private Vector3 startPosition = new Vector3(-1f, 0.5f, 0);

	void Start() {
		playerHealth = GetComponent<PlayerHealth>();
		timer = restartTime;
	}

	void Update() {

		Debug.Log(timer);

		if (playerHealth.isDead) {
			timer -= Time.deltaTime;
		}

		if (timer < 4) {
			GameObject.FindGameObjectWithTag("UI_YouDied").GetComponent<UnityEngine.UI.Text>().enabled = true;
		}

		if (timer < 0) {
			// Create permanent body in game after death
			GameObject body_go = (GameObject)Instantiate(playerBodyPrefab, transform.position, Quaternion.identity);
			body_go.GetComponent<SpriteRenderer>().sortingOrder = Random.Range(0, 255);
			body_go.GetComponent<SpriteRenderer>().flipX = gameObject.GetComponent<SpriteRenderer>().flipX;
			// Move player to starting location
			transform.position = startPosition;
			Camera.main.transform.position = new Vector3(0,0,-2f);
			GameObject.FindGameObjectWithTag("UI_YouDied").GetComponent<UnityEngine.UI.Text>().enabled = false;
			timer = restartTime;

			playerHealth.SendMessage("onRespawn");
			GameObject.FindObjectOfType<GrassSpawner>().SendMessage("initialSpawn");
		}
	}
}
