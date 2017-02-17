using UnityEngine;
using System.Collections;

public class CameraFollowPlayer : MonoBehaviour {

	public GameObject player;

	bool snap = false;
	Room room;

	public float xMargin = 1f;		// Distance in the x axis the player can move before the camera follows.
	public float yMargin = 0.6f;		// Distance in the y axis the player can move before the camera follows.
	public float xSmooth = 1.5f;		// How smoothly the camera catches up with it's target movement in the x axis.
	public float ySmooth = 2f;		// How smoothly the camera catches up with it's target movement in the y axis.

	Vector2 halfCameraView;

	void Start() {
		Vector2 cameraCenter = GetComponent<Camera>().ViewportToWorldPoint(new Vector2(0.5f, 0.5f));
		Vector2 cameraBase = GetComponent<Camera>().ViewportToWorldPoint(new Vector2(0f, 0f));
		halfCameraView = cameraCenter-cameraBase;

	}

	public void snapToRoom(Room room, bool snap) {
		this.room = room;
		this.snap = snap;
	}

	bool CheckXMargin() {
		// Returns true if the distance between the camera and the player in the x axis is greater than the x margin.
		return Mathf.Abs(transform.position.x - player.transform.position.x) > xMargin;
	}

	bool CheckYMargin() {
		// Returns true if the distance between the camera and the player in the y axis is greater than the y margin.
		return Mathf.Abs(transform.position.y - player.transform.position.y) > yMargin;
	}

	void FixedUpdate () {
		if (player != null)
			TrackPlayer();		
	}

	void TrackPlayer () {
		// By default the target x and y coordinates of the camera are it's current x and y coordinates.
		float targetX = transform.position.x;
		float targetY = transform.position.y;

		float minX, maxX;
		float minY, maxY;

		// If the player has moved beyond the x margin...
		if(CheckXMargin())
			// ... the target x coordinate should be a Lerp between the camera's current x position and the player's current x position.
			targetX = Mathf.Lerp(transform.position.x, player.transform.position.x, xSmooth * Time.deltaTime);

		// If the player has moved beyond the y margin...
		if(CheckYMargin())
			// ... the target y coordinate should be a Lerp between the camera's current y position and the player's current y position.
			targetY = Mathf.Lerp(transform.position.y, player.transform.position.y, ySmooth * Time.deltaTime);

		if (snap && room != null) {
			// The target x and y coordinates should not be larger than the maximum or smaller than the minimum.
			if (room.tp.width <= halfCameraView.x*2) {
				maxX = minX = room.roomBase.x+room.tp.width/2;
			} else {
				minX = room.roomBase.x+halfCameraView.x;
				maxX = room.roomBase.x+room.tp.width-halfCameraView.x;
			}
			if (transform.position.x < room.roomBase.x+halfCameraView.x || transform.position.x > room.roomBase.x+room.tp.width-halfCameraView.x) {
				targetX = Mathf.Lerp(transform.position.x, Mathf.Clamp(targetX, minX, maxX), 2.5f*xSmooth * Time.deltaTime);
			} else {
				targetX = Mathf.Clamp(targetX, minX, maxX);
			}
				
			if (room.tp.height <= halfCameraView.y*2) {
				maxY = minY = room.roomBase.y+room.tp.width/2;
			} else {
				minY = room.roomBase.y+halfCameraView.y;
				maxY = room.roomBase.y+room.tp.height-halfCameraView.y;
			}
			if (transform.position.y < room.roomBase.y+halfCameraView.y || transform.position.y > room.roomBase.y+room.tp.height-halfCameraView.y) {
				targetY = Mathf.Lerp(transform.position.y, Mathf.Clamp(targetY, minY, maxY), 2.5f*ySmooth * Time.deltaTime);
			} else {
				targetY = Mathf.Clamp(targetY, minY, maxY);
			}

		}
		// Set the camera's position to the target position with the same z component.
		transform.position = new Vector3(targetX, targetY, transform.position.z);
	}
}
