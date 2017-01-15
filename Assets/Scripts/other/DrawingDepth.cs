using UnityEngine;
using System.Collections;

public class DrawingDepth : MonoBehaviour {

	private SpriteRenderer render;

	void Start () {
		render = GetComponent<SpriteRenderer>();
	}

	void LateUpdate () {
		render.sortingOrder = (int)Camera.main.WorldToScreenPoint (render.bounds.min).y * -1;
	}
}
