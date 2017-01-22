using UnityEngine;
using System.Collections.Generic;

public class StaminaBarControl : MonoBehaviour {

	public GameObject player;
	public GameObject staminaPanel;

	private List<GameObject> panels;
	int panelCount = 34;

	void Awake() {
		panels = new List<GameObject>();

		for (int i = 0; i < 34; i++) {
			addStaminaPanel(i);
		}
	}

	void updateStamina(float stamina) {
		float maxStamina = player.GetComponent<PlayerController>().maxStamina;
		float staminaPercent = stamina/maxStamina;
		int panelsVisible = (int)(staminaPercent*panelCount);

		for (int i = 0; i < panelCount; i++) {
			if (i < panelsVisible) {
				panels[i].GetComponent<UnityEngine.UI.Image>().enabled = true;
			} else {
				panels[i].GetComponent<UnityEngine.UI.Image>().enabled = false;
			}
		}
	}
	
	void addStaminaPanel(int i) {
		GameObject panel_go = (GameObject)Instantiate(staminaPanel, transform);
		panel_go.name = "stamina_panel_"+i;
		panel_go.transform.SetParent(transform);
		panel_go.transform.localScale = new Vector3(1f, 1f, 1f);
		panel_go.transform.position = new Vector3(0f, 0f, 10f);
		panels.Add(panel_go);
	}
}
