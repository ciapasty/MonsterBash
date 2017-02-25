using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSmashed : MonoBehaviour {

	public GameObject scrapPrefab;
	public int averageParts = 4;
	public float maxVelocity = 8f;

	public RoomController rc;
	Sprite[] textures;

	void Start () {
		textures = Resources.LoadAll<Sprite>("sprites/"+scrapPrefab.name);
		if (textures.Length == 0)
			Debug.LogError("No sprites with name: "+scrapPrefab.name);
	}
	
	void OnCollisionEnter2D(Collision2D col) {
		onHit(null);
	}

	void onHit(Attack attk) {
		int parts = Random.Range(averageParts-1, averageParts+2);
		for (int i = 0; i < parts; i++) {
			GameObject scrap = (GameObject)Instantiate(scrapPrefab, transform.position, Quaternion.Euler(new Vector3(0f,0f,Random.Range(0f, 90f))));
			SpriteRenderer scrap_sr = scrap.GetComponent<SpriteRenderer>();
			scrap_sr.sprite = textures[Random.Range(0, textures.Length)];
			scrap_sr.sortingOrder = Random.Range(1, 1000);

			Vector2 velocity = new Vector2(Random.Range(-maxVelocity, maxVelocity), Random.Range(-maxVelocity, maxVelocity));
			scrap.GetComponent<Rigidbody2D>().velocity = velocity;
			scrap.transform.SetParent(rc.transform);

			rc.garbageGOs.Add(scrap);
		}
		Destroy(gameObject);
	}
}
