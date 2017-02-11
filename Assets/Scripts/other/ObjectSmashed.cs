using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSmashed : MonoBehaviour {

	public GameObject scrapPrefab;

	Sprite[] textures;

	void Start () {
		textures = Resources.LoadAll<Sprite>("sprites/"+scrapPrefab.name);
		if (textures.Length == 0)
			Debug.LogError("No sprites with name: "+scrapPrefab.name);
	}
	
	void OnCollisionEnter2D(Collision2D col) {
		int parts = Random.Range(3, 6);
		for (int i = 0; i < parts; i++) {
			GameObject scrap = (GameObject)Instantiate(scrapPrefab, transform.position, Quaternion.Euler(new Vector3(0f,0f,Random.Range(0f, 90f))));
			SpriteRenderer scrap_sr = scrap.GetComponent<SpriteRenderer>();
			scrap_sr.sprite = textures[Random.Range(0, textures.Length)];
			scrap_sr.sortingOrder = Random.Range(1, 4000);

			Vector2 velocity = new Vector2(Random.Range(-8f, 8f), Random.Range(-8f, 8f));
			scrap.GetComponent<Rigidbody2D>().velocity = velocity;
		}
		Destroy(gameObject);
	}
}
