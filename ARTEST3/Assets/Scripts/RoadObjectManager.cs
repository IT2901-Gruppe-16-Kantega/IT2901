using UnityEngine;
using System.Collections;

public class RoadObjectManager : MonoBehaviour {

	public GenerateObjects.GPSLocation roadObjectLocation;

	public float dampening = 0.05f;

	// Use this for initialization
	void Start () {
	// Subscribe to delegate
		GPSManager.onRoadObjectSpawn += this.updateLocation;
	}

	void Destroy() {
		// Unsubscribe to delegate
		GPSManager.onRoadObjectSpawn -= this.updateLocation;
	}

	public void updateLocation() {
		float distance = GenerateObjects.Haversine(GenerateObjects.myLocation, roadObjectLocation);
		float bearing = GenerateObjects.CalculateBearing(GenerateObjects.myLocation, roadObjectLocation);
		transform.position = Vector3.Lerp(transform.position, new Vector3(-Mathf.Cos(bearing) * distance, 0, Mathf.Sin(bearing) * distance), dampening);
	}
	
}
