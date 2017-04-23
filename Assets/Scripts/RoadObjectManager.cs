using System;
using UnityEngine;

public class RoadObjectManager : MonoBehaviour {

	public GpsManager.GpsLocation RoadObjectLocation;
	[Range(0.01f, 1.00f)]
	public float DistanceThreshold = 100;

	[HideInInspector]
	public Objekter Objekt;

	public TextMesh SignText;

	public Material[] Colors = new Material[4];
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
	public bool SomethingIsWrong;

	public GameObject SignPost;
	public Renderer PoleRenderer;

	public void UpdateLocation() {
		Distance = new Vector3(transform.position.x, 0, transform.position.z).magnitude;
		Bearing = Math.Asin(transform.position.x / Distance) + Math.PI / 2;
		DeltaDistance = (new Vector3(transform.position.x, 0, transform.position.z) - new Vector3(OriginPoint.x, 0, OriginPoint.z)).magnitude;
		DeltaBearing = Math.Atan2(transform.position.z - OriginPoint.z, transform.position.x - OriginPoint.x) * 180 / Math.PI - 90;
		if (DeltaBearing < 0) DeltaBearing += 360;
		if (!HasBeenMoved) DeltaBearing = 0;
		Objekt.metadata.distance = DeltaDistance;
		Objekt.metadata.bearing = DeltaBearing;
		DistanceText.text = Distance.ToString("F2") + " m";
	}

	public void Selected() {
		PoleRenderer.material = Colors[3];
    }

	public void UnSelected() {
		PoleRenderer.material = (Objekt.geometri.egengeometri) ? Colors[1] : (HasBeenMoved ? Colors[2] : Colors[0]);
        if (SomethingIsWrong) PoleRenderer.material = Colors[2];

	}

	public void ResetPosition() {
		SignPost.transform.position = OriginPoint;
		HasBeenMoved = false;
		DeltaDistance = (new Vector3(transform.position.x, 0, transform.position.z) - new Vector3(OriginPoint.x, 0, OriginPoint.z)).magnitude;
		DeltaBearing = 0;
		Objekt.metadata.distance = DeltaDistance;
		Objekt.metadata.bearing = DeltaBearing;
	}
}
