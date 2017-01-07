using UnityEngine;
using System.Collections;

public class CameraFollowHorizontally : MonoBehaviour {

	public GameObject player;

	public float xMargin = 1f;					// Distance in the x axis the player can move before the camera follows.
	public float xSmooth = 2.5f;				// How smoothly the camera catches up with it's target movement in the x axis.
	//public Vector2 maxXAndY;					// The maximum x and y coordinates the camera can have.
	public Vector2 minXAndY = Vector2.zero;		// The minimum x and y coordinates the camera can have.
	public bool rightMovementOnly = false;

	bool CheckXMargin()
	{
		// Returns true if the distance between the camera and the player in the x axis is greater than the x margin.
		return Mathf.Abs(transform.position.x - player.transform.position.x) > xMargin;
	}

	void FixedUpdate ()
	{
		TrackPlayer();
	}


	void TrackPlayer ()
	{
		// By default the target x and y coordinates of the camera are it's current x and y coordinates.
		float targetX = transform.position.x;
		float targetY = transform.position.y;

		// If the player has moved beyond the x margin...
		if(CheckXMargin())
			// ... the target x coordinate should be a Lerp between the camera's current x position and the player's current x position.
			targetX = Mathf.Lerp(transform.position.x, player.transform.position.x, xSmooth * Time.deltaTime);

		// The target x and y coordinates should not be larger than the maximum or smaller than the minimum.
		targetX = Mathf.Clamp(targetX, minXAndY.x, Mathf.Infinity);

		if (rightMovementOnly) {
			if (targetX < transform.position.x) {
				targetX = transform.position.x;
			}
		}

		// Set the camera's position to the target position with the same z component.
		transform.position = new Vector3(targetX, targetY, transform.position.z);
	}
}
