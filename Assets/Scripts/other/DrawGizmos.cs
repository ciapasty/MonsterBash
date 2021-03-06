﻿using UnityEngine;
using System.Collections;

public class DrawGizmos : MonoBehaviour {

	#if UNITY_EDITOR
		Color orange = Color.white; // 255, 147, 0
		
		void OnDrawGizmos() {
			// Setup orange color
			orange.g = 0.58f;
			orange.b = 0f;

			// Attack radius gizmo
			UnityEditor.Handles.color = Color.red;
			Attack[] attks = GetComponents<Attack>();
			foreach (var attk in attks) {
				Vector3 radiusCircleCenter = GetComponent<Renderer>().bounds.center;
				radiusCircleCenter.x += attk.horizontalRadiusShift;
				radiusCircleCenter.y += attk.verticalRadiusShift;
				UnityEditor.Handles.DrawWireDisc(radiusCircleCenter, Vector3.back, attk.radius);
				// Attack range
				UnityEditor.Handles.color = orange;
				UnityEditor.Handles.DrawWireDisc(GetComponent<Renderer>().bounds.center, Vector3.back, attk.range);
			}

			// Collider body gizmo
			if (GetComponent<BoxCollider2D>()) {
				UnityEditor.Handles.color = Color.yellow;
				UnityEditor.Handles.DrawWireCube(GetComponent<BoxCollider2D>().bounds.center, GetComponent<BoxCollider2D>().bounds.size);
			} else if (GetComponent<CircleCollider2D>()) {
				UnityEditor.Handles.color = Color.yellow;
				UnityEditor.Handles.DrawWireDisc(GetComponent<CircleCollider2D>().bounds.center, Vector3.back, GetComponent<CircleCollider2D>().radius);
			}

		}
	#endif
}
