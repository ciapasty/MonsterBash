using UnityEngine;
using System.Collections;

public class PlayerSitDown : MonoBehaviour {

	private Animator animator;
	private PlayerController playerController;

	public bool isSitting;
	private float standupTimer;

	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator>();
		playerController = GetComponent<PlayerController>();
	}
	
	// Update is called once per frame
	void Update () {
		if (!playerController.enabled){
			if (Input.anyKeyDown && standupTimer < 0) {
				standupTimer = 0f;
				animator.SetTrigger("sitTrigger");
			}
			if (standupTimer >= 0) {
				standupTimer += Time.deltaTime;
			} 
			if (standupTimer > 0.4f) {
				playerController.enabled = true;
				isSitting = false;
			}
		}
	}

	void sitDown() {
		animator.SetTrigger("sitTrigger");
		playerController.enabled = false;
		standupTimer = -1f;
		isSitting = true;
	}
}
