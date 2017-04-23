using System;
using UnityEngine;
using System.Collections;

public class DefaultRoadObjectManager : MonoBehaviour {
	[HideInInspector]
	public Objekter Objekt;

	public TextMesh SignText;
	public TextMesh DistanceText;

	[Range(0.01f, 1.00f)]
	public float DistanceThreshold = 100;

	[HideInInspector]
	public double Distance;
	[HideInInspector]
	public double Bearing;

	private void FixedUpdate() {
		if (Vector3.Angle(
			Camera.main.transform.forward, new Vector3(Camera.main.transform.position.x - transform.position.x, 0, Camera.main.transform.position.z - transform.position.z)) < 90f)
			return;
		transform.LookAt(new Vector3(Camera.main.transform.position.x,
									0,
									Camera.main.transform.position.z));
		UpdateLocation();
	}
	public void UpdateLocation() {
		Distance = new Vector3(transform.position.x, 0, transform.position.z).magnitude;
		DistanceText.text = Distance.ToString("F2") + " m";
	}
}
