using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light))]
public class BonfireLightFlicker : MonoBehaviour {

	private Light lightSource;

	public float rangeDiff = 0.05f;
	public float intensityDiff = 0.03f;

	public float range = 6;
	public float intensity = 4;

	public float flickRate = 0.05f;
	public float flickTime = 0.1f;
	private float timer;

	private Color color;

	// Use this for initialization
	void Start () {
		lightSource = GetComponent<Light>();

		color = Color.white;

		timer = flickRate;
	}
	
	// Update is called once per frame
	void Update () {
		if (timer < 0) {

			color.g = 0.9f+Random.Range(-0.1f, 0.1f);
			color.b = 0.4f;

			lightSource.color = color;
			lightSource.range = range+Random.Range(-rangeDiff, rangeDiff);
			lightSource.intensity = intensity+Random.Range(-intensityDiff, intensityDiff);

			timer = flickTime+Random.Range(-flickRate, flickRate);
		}
		timer -= Time.deltaTime;
	}
}
