using System;
using UnityEngine;

public class RoadObjectManager : MonoBehaviour {

	public GpsManager.GpsLocation RoadObjectLocation;
	[Range(0.01f, 1.00f)]
	public float DistanceThreshold = 100;

	[HideInInspector]
	public Objekter Objekt;

	public TextMesh SignText;

	public Material[] Colors = new Material[3];
	public Renderer PoleRenderer;
	public TextMesh DistanceText; 

	[HideInInspector]
	public double Distance;
	[HideInInspector]
	public double Bearing;

	[HideInInspector]
	public Vector3 OriginPoint;
	[HideInInspector]
	public double DeltaDistance; // Distance moved from OriginPoint
	[HideInInspector]
	public double DeltaBearing; // Angle moved from OriginPoint
	public bool HasBeenMoved;


	private void Update() {
		// I'm Mr. Meeseeks, look at me!
		transform.LookAt(new Vector3(Camera.main.transform.position.x,
									0,
									Camera.main.transform.position.z));
		UpdateLocation();
	}

	public void UpdateLocation() {
		Distance = new Vector3(transform.position.x, 0, transform.position.z).magnitude;
		Bearing = Math.Asin(transform.position.x / Distance) + Math.PI / 2;
		DeltaDistance = (new Vector3(transform.position.x, 0, transform.position.z) - new Vector3(OriginPoint.x, 0, OriginPoint.z)).magnitude;
		DeltaBearing = Math.Atan2(transform.position.z - OriginPoint.z, transform.position.x - OriginPoint.x) * 180 / Math.PI - 90;
		if (DeltaBearing < 0) DeltaBearing += 360;
		if (!HasBeenMoved) DeltaBearing = 0;
		Objekt.distance = DeltaDistance;
		Objekt.bearing = DeltaBearing;
		DistanceText.text = Distance.ToString("F2") + " m";
	}

	public void Selected() {
		PoleRenderer.material = Colors[1];
	}

	public void UnSelected() {
		PoleRenderer.material = HasBeenMoved ? Colors[2] : Colors[0];
	}

	public void ResetPosition() {
		transform.position = OriginPoint;
		HasBeenMoved = false;
		DeltaDistance = (new Vector3(transform.position.x, 0, transform.position.z) - new Vector3(OriginPoint.x, 0, OriginPoint.z)).magnitude;
		DeltaBearing = 0;
		Objekt.distance = DeltaDistance;
		Objekt.bearing = DeltaBearing;
	}
}
