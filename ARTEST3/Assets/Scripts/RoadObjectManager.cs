using UnityEngine;

public class RoadObjectManager : MonoBehaviour {

	public GPSManager.GPSLocation roadObjectLocation;
	[Range(0.01f, 1.00f)]
	public float distanceThreshold = 100;

    [HideInInspector]
    public Objekt objekt;

	public Material[] colors = new Material[3];
	public Renderer poleRenderer;
	public TextMesh distanceText;

	[HideInInspector]
	public double distance;
	[HideInInspector]
	public double bearing;

	[HideInInspector]
	public double deltaDistance;
	[HideInInspector]
	public double deltaBearing;
	public bool hasBeenMoved;

	void Update() {
		// I'm Mr. Meeseeks, look at me!
		transform.LookAt(new Vector3(Camera.main.transform.position.x,
									0,
									Camera.main.transform.position.z));
		updateLocation();
	}

	public void updateLocation() {
		distance = new Vector3(transform.position.x, 0, transform.position.z).magnitude;
		bearing = System.Math.Asin(transform.position.x / distance) + System.Math.PI / 2;
		distanceText.text = distance.ToString("F2") + " m";

		//if (Mathf.Abs(distance) > distanceThreshold) {
		//	if (gameObject.activeSelf) gameObject.SetActive(false);
		//} else {
		//	if (!gameObject.activeSelf) gameObject.SetActive(true);
		//transform.position = 
		//	Vector3.Lerp(transform.position, 
		//				new Vector3(
		//					-Mathf.Cos((float) bearing) * (float) distance, 
		//					0, 
		//					Mathf.Sin((float) bearing) * (float) distance), 
		//					dampening);
		////}
	}

	public void Selected() {
		poleRenderer.material = colors[1];
	}

	public void UnSelected() {
		if(hasBeenMoved) {
			poleRenderer.material = colors[2];
		} else {
			poleRenderer.material = colors[0];
		}
	}
}
