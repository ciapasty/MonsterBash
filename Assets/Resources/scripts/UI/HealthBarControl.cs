using UnityEngine;
using System.Collections.Generic;

public class HealthBarControl : MonoBehaviour {

	public GameObject player;
	public GameObject heartPanel;

	public Sprite emptyHeart;
	public Sprite fullHeart;

	private List<GameObject> panels;

	void Awake() {
		panels = new List<GameObject>();

		int maxHitpoints = player.GetComponent<PlayerHealth>().maxHitpoints;
		for (int i = 0; i < maxHitpoints; i++) {
			addHeartPanel(i);
		}
	}

	void updateHealth() {
		int maxHitpoints = player.GetComponent<PlayerHealth>().maxHitpoints;
		int currentHitPoints = player.GetComponent<PlayerHealth>().hitpoints;

		//Debug.Log("Max: "+maxHitpoints+" Current: "+currentHitPoints);

		if (maxHitpoints > panels.Count) {
			for (int i = 0; i < maxHitpoints-panels.Count; i++) {
				addHeartPanel(panels.Count+1);
			}
		}

		for (int i = 0; i < panels.Count; i++) {
			if (i < currentHitPoints) {
				panels[i].GetComponent<UnityEngine.UI.Image>().sprite = fullHeart;
			} else {
				panels[i].GetComponent<UnityEngine.UI.Image>().sprite = emptyHeart;
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
