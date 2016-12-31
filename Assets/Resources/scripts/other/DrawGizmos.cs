using UnityEngine;
using System.Collections;

public class DrawGizmos : MonoBehaviour {

	void OnDrawGizmos() {
		// Attack range gizmo
		UnityEditor.Handles.color = Color.red;
		if (GetComponent<PlayerController>()) {
			UnityEditor.Handles.DrawWireDisc(GetComponent<Renderer>().bounds.center, Vector3.back, GetComponent<PlayerController>().attackRadius);
		} else if (GetComponent<EnemyController>()) {
			UnityEditor.Handles.DrawWireDisc(GetComponent<Renderer>().bounds.center, Vector3.back, GetComponent<EnemyController>().attackRadius);
		}

		// Collider bod gizmo
		UnityEditor.Handles.color = Color.yellow;
		UnityEditor.Handles.DrawWireCube(GetComponent<BoxCollider2D>().bounds.center, GetComponent<BoxCollider2D>().bounds.size);
	}
}
