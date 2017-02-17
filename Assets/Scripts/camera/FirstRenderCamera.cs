using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstRenderCamera : MonoBehaviour {

	public RenderTexture temp = null;
	public int scale { get; protected set; }

	float ortographicSize;
	public int rTexHeight { get; protected set; }
	public int rTexWidth { get; protected set; }

	public int pixelsPerUnit = 16;
	public int roundingFactor = 2;

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
		playerCamera.orthographicSize = playerCamera.pixelHeight/(float)pixelsPerUnit/2f/(float)scale;
		rTexHeight = getRenderTextureHeight();
		rTexWidth = getRenderTextureWidth();
	}

	int getScale() {
		int scale = 1;
		float x = 20f;
		while(x > 12f) {
			x = playerCamera.pixelHeight/(float)pixelsPerUnit/(float)scale;
			scale++;
		}
		return scale-1;
	}

	int getRenderTextureHeight() {
		float height = playerCamera.pixelHeight/scale;
		return (roundingFactor*Mathf.CeilToInt(height/(float)roundingFactor));
	}

	int getRenderTextureWidth() {
		float width = rTexHeight*playerCamera.aspect;
		return (roundingFactor*Mathf.CeilToInt(width/(float)roundingFactor));
	}
}
