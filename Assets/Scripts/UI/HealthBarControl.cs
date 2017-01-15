using UnityEngine;
using System.Collections.Generic;

public class HealthBarControl : MonoBehaviour {

	public GameObject player;
	public GameObject heartPanel;

	public Sprite fullHeart;
	public Sprite emptyHeart;
	public Sprite lockedHeart;

	private PlayerHealth playerHealth;
	private List<GameObject> panels;

	void Awake() {
		playerHealth = player.GetComponent<PlayerHealth>();
		panels = new List<GameObject>();

		for (int i = 0; i < playerHealth.maxHitpoints; i++) {
			addHeartPanel(i);
		}
	}

	void updateHealth() {
		if (playerHealth.maxHitpoints > panels.Count) {
			int addPanels = playerHealth.maxHitpoints-panels.Count;
			for (int i = 0; i < addPanels; i++) {
				addHeartPanel(panels.Count+1);
			}
		}

		for (int i = 0; i < panels.Count; i++) {
			if (i < playerHealth.hitpoints) {
				panels[i].GetComponent<UnityEngine.UI.Image>().sprite = fullHeart;
			} else {
				if (playerHealth.lockedHitpoints && i > (playerHealth.maxHitpoints-1)/2) {
					panels[i].GetComponent<UnityEngine.UI.Image>().sprite = lockedHeart;
				} else {
					panels[i].GetComponent<UnityEngine.UI.Image>().sprite = emptyHeart;
				}
			}
		}

		GetComponent<AutomaticHorizontalSize>().AdjustSize();
	}

	void addHeartPanel(int i) {
		GameObject panel_go = (GameObject)Instantiate(heartPanel, transform);
		panel_go.name = "heart_panel_"+i;
		panel_go.transform.SetParent(transform);
		panel_go.transform.localScale = new Vector3(1f, 1f, 1f);
		panels.Add(panel_go);
	}
}
