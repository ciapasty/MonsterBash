using UnityEngine;
using System.Collections;

public class DrawingDepthDynamic : MonoBehaviour {

	private SpriteRenderer render;

	void Start () {
		render = GetComponent<SpriteRenderer>();
	}

	void LateUpdate () {
		render.sortingOrder = -Mathf.CeilToInt(transform.position.y*16f);
	}
}