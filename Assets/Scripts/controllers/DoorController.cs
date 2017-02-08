using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

public class DoorController : MonoBehaviour {

	Dictionary<string, Sprite> doorSprites;
	SpriteRenderer sr;

	public Door door;

	void OnEnable(){
		sr = GetComponent<SpriteRenderer>();
		doorSprites = new Dictionary<string, Sprite>();
		foreach (var sprite in Resources.LoadAll<Sprite>("sprites/door")) {
			doorSprites.Add(sprite.name, sprite);
		}
	}

	void OnCollisionEnter2D(Collision2D col) {
		if (col.gameObject.tag == "Player") {
			if (!door.isLocked) {
				if (!door.isOpen) {
					door.isOpen = true;
				}
			} else {
				Debug.Log("Door locked!");
			}
		}
	}

	/// <summary>
	/// Callback. Updates door sprite and BoxCollider.
	/// </summary>
	/// <param name="door">Door.</param>
	public void onStateChanged(Door door) {
		if (door.isOpen) {
			sr.sprite =doorSprites["door_"+door.orientation.ToString()+"_open"];
			GetComponent<BoxCollider2D>().enabled = false;
		} else {
			GetComponent<BoxCollider2D>().enabled = true;
			if (door.isLocked) {
				sr.sprite = doorSprites["door_"+door.orientation.ToString()+"_locked"];
			} else {
				sr.sprite = doorSprites["door_"+door.orientation.ToString()+"_closed"];
			}
		}
	}
}
