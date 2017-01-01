using UnityEngine;
using System.Collections;

public class SoundController : MonoBehaviour {

	public AudioClip[] stepSounds;

	void Start() {}

	void Update() {}

	void playStepSound() {
		AudioSource.PlayClipAtPoint(stepSounds[Random.Range(0, stepSounds.Length)], transform.position, 1.0f);
	}
}
