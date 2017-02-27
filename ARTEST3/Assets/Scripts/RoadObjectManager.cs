using UnityEngine;

public class RoadObjectManager : MonoBehaviour {

	public GPSManager.GPSLocation roadObjectLocation;
	[Range(0.01f, 1.00f)]
	public float dampening = 0.05f;
	public float distanceThreshold = 100;

	public Material[] colors = new Material[2];
	public Renderer plateRenderer;
	public TextMesh distanceText;


	private float distance;
	private float bearing;

	// Use this for initialization
	void Start() {
		// Subscribe to delegate
		GPSManager.onRoadObjectSpawn += this.updateLocation;
	}

	void Update() {
	// I'm Mr. Meeseeks, look at me!
		transform.LookAt(new Vector3(transform.position.x - Camera.main.transform.position.x, transform.position.y, transform.position.z - Camera.main.transform.position.z));
	}

	void Destroy() {
		// Unsubscribe to delegate
		GPSManager.onRoadObjectSpawn -= this.updateLocation;
	}

	public void updateLocation() {
		distance = GenerateObjects.Haversine(GenerateObjects.myLocation, roadObjectLocation);
		//if (Mathf.Abs(distance) > distanceThreshold) {
		//	//if (gameObject.activeSelf) gameObject.SetActive(false);
		//} else {
			//if (!gameObject.activeSelf) gameObject.SetActive(true);
			distanceText.text = distance + " m";
			bearing = GenerateObjects.CalculateBearing(GenerateObjects.myLocation, roadObjectLocation);
			transform.position = Vector3.Lerp(transform.position, new Vector3(-Mathf.Cos(bearing) * distance, 0, Mathf.Sin(bearing) * distance), dampening);
		//}
	}

	public void Selected() {
		plateRenderer.material = colors[1];
	}

	public void UnSelected() {
		plateRenderer.material = colors[0];
	}
}
