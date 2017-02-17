using UnityEngine;
using System.Collections;

public class DrawingDepthStatic : MonoBehaviour {

	private SpriteRenderer render;

	void Start () {
		render = GetComponent<SpriteRenderer>();
		render.sortingOrder = (int)Camera.main.WorldToScreenPoint (render.bounds.min).y * -1;
	}
}