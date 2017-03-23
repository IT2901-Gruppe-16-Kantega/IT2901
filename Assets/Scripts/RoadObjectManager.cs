using UnityEngine;

public class RoadObjectManager : MonoBehaviour {

	public GpsManager.GpsLocation RoadObjectLocation;
	[Range(0.01f, 1.00f)]
	public float DistanceThreshold = 100;

	[HideInInspector]
	public Objekt Objekt;

	public TextMesh SignText;

	public Material[] Colors = new Material[3];
	public Renderer PoleRenderer;
	public TextMesh DistanceText;

	[HideInInspector]
	public double Distance;
	[HideInInspector]
	public double Bearing;

	[HideInInspector]
	public double DeltaDistance;
	[HideInInspector]
	public double DeltaBearing;
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
		Bearing = System.Math.Asin(transform.position.x / Distance) + System.Math.PI / 2;
		DistanceText.text = Distance.ToString("F2") + " m";
	}

	public void Selected() {
		PoleRenderer.material = Colors[1];
	}

	public void UnSelected() {
		PoleRenderer.material = HasBeenMoved ? Colors[2] : Colors[0];
	}

	public void ResetPosition() {
		transform.position = HelperFunctions.GetPositionFromCoords(RoadObjectLocation);
		HasBeenMoved = false;
	}
}
