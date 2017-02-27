using UnityEngine;

public class RoadObjectManager : MonoBehaviour {

	public GPSManager.GPSLocation roadObjectLocation;

	public float dampening = 0.05f;
	private float distance;
	private float bearing;

	// Use this for initialization
	void Start() {
		// Subscribe to delegate
		GPSManager.onRoadObjectSpawn += this.updateLocation;
	}

	void Destroy() {
		// Unsubscribe to delegate
		GPSManager.onRoadObjectSpawn -= this.updateLocation;
	}

	public void updateLocation() {
		distance = GenerateObjects.Haversine(GenerateObjects.myLocation, roadObjectLocation);
		if (distance > 100) {
			gameObject.SetActive(false);
		} else {
			bearing = GenerateObjects.CalculateBearing(GenerateObjects.myLocation, roadObjectLocation);
			transform.position = Vector3.Lerp(transform.position, new Vector3(-Mathf.Cos(bearing) * distance, 0, Mathf.Sin(bearing) * distance), dampening);
		}
	}

}
