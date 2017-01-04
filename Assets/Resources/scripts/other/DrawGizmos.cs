using UnityEngine;
using System.Collections;

public class DrawGizmos : MonoBehaviour {
	
	void OnDrawGizmos() {
		// Attack range gizmo
		UnityEditor.Handles.color = Color.red;
		if (GetComponent<PlayerController>()) {
			UnityEditor.Handles.DrawWireDisc(GetComponent<Renderer>().bounds.center, Vector3.back, GetComponent<PlayerController>().attackRadius);
		} else if (GetComponent<EnemyController>()) {
			//Vector3 attkCircleCenter = GetComponent<Renderer>().bounds.center;
			//attkCircleCenter.y -= 0.1f;
			UnityEditor.Handles.DrawWireDisc(GetComponent<Renderer>().bounds.center, Vector3.back, GetComponent<Attack>().radius);
		}

		// Collider bod gizmo
		UnityEditor.Handles.color = Color.yellow;
		UnityEditor.Handles.DrawWireCube(GetComponent<BoxCollider2D>().bounds.center, GetComponent<BoxCollider2D>().bounds.size);
	}
}
