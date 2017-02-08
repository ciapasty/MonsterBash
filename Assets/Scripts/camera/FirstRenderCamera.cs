using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstRenderCamera : MonoBehaviour {

	public RenderTexture temp = null;
	public int scale { get; protected set; }

	float ortographicSize;
	public int rTexHeight { get; protected set; }
	public int rTexWidth { get; protected set; }

	public Camera playerCamera { get; protected set; }

	void Start() {
		playerCamera = GetComponent<Camera>();
		setScreenRenderSizes();
		temp = RenderTexture.GetTemporary(rTexWidth, rTexHeight);
	}

	void OnPreRender() {
		temp = RenderTexture.GetTemporary(rTexWidth, rTexHeight);
		temp.filterMode = FilterMode.Point;
		playerCamera.targetTexture = temp;
	}

	void OnPostRender() {
		GetComponent<Camera>().targetTexture = null;
		Graphics.Blit(temp, (RenderTexture)null);
		RenderTexture.ReleaseTemporary(temp);
	}

	// ================

	void setScreenRenderSizes() {
		scale = getScale();
		playerCamera.orthographicSize = playerCamera.pixelHeight/24f/2f/(float)scale;
		rTexHeight = getRenderTextureHeight();
		rTexWidth = getRenderTextureWidth();
	}

	int getScale() {
		int scale = 1;
		float x = 20f;
		while(x > 9f) {
			x = playerCamera.pixelHeight/24f/(float)scale;
			scale++;
		}
		return scale-1;
	}

	int getRenderTextureHeight() {
		float height = playerCamera.pixelHeight/scale;
		return (2*Mathf.CeilToInt(height/2f));
	}

	int getRenderTextureWidth() {
		float width = rTexHeight*playerCamera.aspect;
		return (2*Mathf.CeilToInt(width/2f));
	}
}
