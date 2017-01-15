using UnityEngine;
using System.Collections;

public class DrawBorders : MonoBehaviour {

	public Color color = Color.yellow;

	#if UNITY_EDITOR		
		void OnDrawGizmos() {
			// Collider body gizmo
			if (GetComponent<BoxCollider2D>()) {
				UnityEditor.Handles.color = color;
				UnityEditor.Handles.DrawWireCube(GetComponent<BoxCollider2D>().bounds.center, GetComponent<BoxCollider2D>().bounds.size);
			}
		}
	#endif
}
