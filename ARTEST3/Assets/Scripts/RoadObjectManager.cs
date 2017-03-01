using UnityEngine;

public class RoadObjectManager : MonoBehaviour {

	public GPSManager.GPSLocation roadObjectLocation;
	[Range(0.01f, 1.00f)]
	public float dampening = 0.05f;
	public float distanceThreshold = 100;

	public Material[] colors = new Material[2];
	public Renderer plateRenderer;
	public TextMesh distanceText;

	[HideInInspector]
	public double distance;
	[HideInInspector]
	public double bearing;

	// Use this for initialization
	void Start() {
		// Subscribe to delegate
		GPSManager.onRoadObjectSpawn += this.updateLocation;
	}

	void Update() {
		// I'm Mr. Meeseeks, look at me!
		transform.LookAt(new Vector3(Camera.main.transform.position.x,
									0,
									Camera.main.transform.position.z));
		//updateLocation();
	}

	void Destroy() {
		// Unsubscribe to delegate
		GPSManager.onRoadObjectSpawn -= this.updateLocation;
	}

	public void updateLocation() {
		distance = GenerateObjects.Haversine(GenerateObjects.myLocation, roadObjectLocation);
		//if (Mathf.Abs(distance) > distanceThreshold) {
		//	if (gameObject.activeSelf) gameObject.SetActive(false);
		//} else {
		//	if (!gameObject.activeSelf) gameObject.SetActive(true);
		distanceText.text = distance.ToString("F2") + " m";
		bearing = GenerateObjects.CalculateBearing(GenerateObjects.myLocation, roadObjectLocation);
		transform.position = Vector3.Lerp(transform.position, new Vector3(-Mathf.Cos((float) bearing) * (float) distance, 0, Mathf.Sin((float) bearing) * (float) distance), dampening);
		//}
	}

	public void Selected() {
		plateRenderer.material = colors[1];
	}

	public void UnSelected() {
		plateRenderer.material = colors[0];
	}
}
