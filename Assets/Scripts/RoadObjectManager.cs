using System;
using UnityEngine;

public class RoadObjectManager : MonoBehaviour {
	[HideInInspector]
	public double Bearing;

	public Material[] Colors = new Material[4];
	public Renderer DefaultRenderer;

	[HideInInspector]
	public double DeltaBearing; // Angle moved from OriginPoint

	[HideInInspector]
	public double DeltaDistance; // Distance moved from OriginPoint

	[HideInInspector]
	public double Distance;

	public TextMesh DistanceText;

	[Range(0.01f, 1.00f)]
	public float DistanceThreshold = 100;

	public bool HasBeenMoved;
	public bool IsDefault;

	[HideInInspector]
	public Objekter Objekt;

	[HideInInspector]
	public Vector3 OriginPoint;

	public Renderer PoleRenderer;

	public GpsManager.GpsLocation RoadObjectLocation;

	public GameObject SignPost;

	public TextMesh SignText;
	public bool SomethingIsWrong;

	/// <summary>
	///     Updates the Distance and Bearing of this object
	/// </summary>
	public void UpdateLocation() {
		Distance = new Vector3(transform.position.x, 0, transform.position.z).magnitude;
		Bearing = Math.Asin(transform.position.x / Distance) + Math.PI / 2;
		DeltaDistance =
			(new Vector3(transform.position.x, 0, transform.position.z) - new Vector3(OriginPoint.x, 0, OriginPoint.z)).magnitude;
		DeltaBearing = Math.Atan2(transform.position.z - OriginPoint.z, transform.position.x - OriginPoint.x) * 180 / Math.PI
						-
						90;
		if (DeltaBearing < 0)
			DeltaBearing += 360;
		if (!HasBeenMoved)
			DeltaBearing = 0;
		Objekt.metadata.distance = DeltaDistance;
		Objekt.metadata.bearing = DeltaBearing;
		DistanceText.text = Distance.ToString("F2") + " m";
	}

	/// <summary>
	///     If this object is selected
	/// </summary>
	public void Selected() {
		if (IsDefault)
			DefaultRenderer.material = Colors[3];
		else
			PoleRenderer.material = Colors[3];
	}

	/// <summary>
	///     If this object is unselected
	/// </summary>
	public void UnSelected() {
		if (IsDefault) {
			DefaultRenderer.material = Objekt.geometri.egengeometri ? Colors[1] : (HasBeenMoved ? Colors[2] : Colors[0]);
			if (SomethingIsWrong)
				DefaultRenderer.material = Colors[2];
		} else {
			PoleRenderer.material = Objekt.geometri.egengeometri ? Colors[1] : (HasBeenMoved ? Colors[2] : Colors[0]);
			if (SomethingIsWrong)
				PoleRenderer.material = Colors[2];
		}
	}

	/// <summary>
	///     Resets the current object's
	/// </summary>
	public void ResetPosition() {
		if (IsDefault)
			transform.position = OriginPoint;
		else
			SignPost.transform.position = OriginPoint;
		HasBeenMoved = false;
		DeltaDistance =
			(new Vector3(transform.position.x, 0, transform.position.z) - new Vector3(OriginPoint.x, 0, OriginPoint.z)).magnitude;
		DeltaBearing = 0;
		Objekt.metadata.distance = DeltaDistance;
		Objekt.metadata.bearing = DeltaBearing;
	}
}