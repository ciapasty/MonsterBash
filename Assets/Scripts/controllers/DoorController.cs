using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

public class DoorController : MonoBehaviour {

	Sprite[] doorSprites;
	SpriteRenderer sr;

	public Door door;

	void OnEnable(){
		sr = GetComponent<SpriteRenderer>();
		doorSprites = Resources.LoadAll<Sprite>("sprites/door");
	}

	void OnCollisionEnter2D(Collision2D col) {
		if (!door.isLocked) {
			if (!door.isOpen) {
				door.isOpen = true;
			}
		} else {
			Debug.Log("Door locked!");
		}
	}

	/// <summary>
	/// Callback. Updates door sprite and BoxCollider.
	/// </summary>
	/// <param name="door">Door.</param>
	public void onStateChanged(Door door) {
		if (door.isOpen) {
			sr.sprite = getDoorSprite("door_"+door.orientation.ToString()+"_open");
			GetComponent<BoxCollider2D>().enabled = false;
		} else {
			GetComponent<BoxCollider2D>().enabled = true;
			if (door.isLocked) {
				sr.sprite = getDoorSprite("door_"+door.orientation.ToString()+"_locked");
			} else {
				sr.sprite = getDoorSprite("door_"+door.orientation.ToString()+"_closed");
			}
		}
	}

	Sprite getDoorSprite(string name) {
		for (int i = 0; i < doorSprites.Length; i++) {
			if (doorSprites[i].name == name)
				return doorSprites[i];
		}
		return null;
	}
}
