using UnityEngine;
using System.Collections;

public class SoundController : MonoBehaviour {

	public AudioClip[] stepSounds;
	public float stepSoundVolume = 1.0f;
	public AudioClip[] dodgeRollSounds;
	public float dodgeRollSoundVolume = 1.0f;
	public AudioClip[] attackSounds;
	public float attackSoundVolume = 1.0f;
	public AudioClip[] blockSounds;
	public float blockSoundVolume = 1.0f;
	public AudioClip[] hurtSounds;
	public float hurtSoundVolume = 1.0f;
	public AudioClip[] deathSounds;
	public float deathSoundVolume = 1.0f;


	void Start() {}

	void Update() {}

	private void playSound(AudioClip[] clipArray, float volume) {
		AudioSource.PlayClipAtPoint(clipArray[Random.Range(0, clipArray.Length)], transform.position, volume);
	}

	public void playStepSound() {
		playSound(stepSounds, stepSoundVolume);
	}

	public void playDodgeRollSound() {
		playSound(dodgeRollSounds, dodgeRollSoundVolume);
	}

	public void playAttackSound() {
		playSound(attackSounds, attackSoundVolume);
	}

	public void playBlockSound() {
		playSound(blockSounds, blockSoundVolume);
	}

	public void playHurtSound() {
		playSound(hurtSounds, hurtSoundVolume);
	}

	public void playDeathSound() {
		playSound(deathSounds, deathSoundVolume);
	}


}
