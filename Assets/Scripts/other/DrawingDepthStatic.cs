using UnityEngine;
using System.Collections;

public class DrawingDepthStatic : MonoBehaviour {

	private SpriteRenderer render;

	void Start () {
		render = GetComponent<SpriteRenderer>();
		render.sortingOrder = -Mathf.FloorToInt(transform.position.y*16f);
	}
}