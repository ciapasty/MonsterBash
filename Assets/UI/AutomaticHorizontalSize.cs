using UnityEngine;
using System.Collections;

public class AutomaticHorizontalSize : MonoBehaviour {

	public float childWidth = 10f;

	// Use this for initialization
	void Start () {
		AdjustSize();
	}
	
	public void AdjustSize() {
		Vector2 size = this.GetComponent<RectTransform>().sizeDelta;
		size.x = this.transform.childCount * childWidth;
		this.GetComponent<RectTransform>().sizeDelta = size;
	}
}
