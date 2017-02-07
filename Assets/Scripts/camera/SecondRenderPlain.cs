using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondRenderPlain : MonoBehaviour {

	FirstRenderCamera frc;
	Material mat;


	// Use this for initialization
	void Start () {
		frc = GameObject.FindGameObjectWithTag("PlayerCamera").GetComponent<FirstRenderCamera>();
		mat = new Material(Shader.Find("Mobile/Unlit (Supports Lightmap)"));

	}
	
	// Update is called once per frame
	void Update () {
		mat.mainTexture = frc.temp;
		GetComponent<MeshRenderer>().material = mat;

		float aspect = (float)frc.temp.width/(float)frc.temp.height;
		float yScale = ((float)frc.playerCamera.pixelHeight/(float)frc.rTexHeight)/(float)frc.scale;

		Vector3 lc = new Vector3(-aspect, 1f, -1);
		transform.localScale = lc;
	}
}
