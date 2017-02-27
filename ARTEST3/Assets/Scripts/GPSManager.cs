using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/*
Handles the GPS on the phone
*/
public class GPSManager : MonoBehaviour {
	// How many seconds to wait max
	private int maxWait = 10;

	// Our default latitude, longitude, and altitude
	// Default is somewhere in the middle of Trondheim
	public float myLatitude = 63.4238907f;
	public float myLongitude = 10.3990959f;
	public float myAltitude = 10f;
	[HideInInspector]
	public bool initialPositionUpdated = false;

	// The Service that handles the GPS on the phone
	private LocationService service;
	public float gpsAccuracy = 5f;
	public float gpsUpdateInterval = 5f;
	public Slider accuracySlider;
	public Slider intervalSlider;
	public Text accuracyText;
	public Text intervalText;


	public delegate void RoadObjectEventHandler();
	public static event RoadObjectEventHandler onRoadObjectSpawn;

	public Text debugText;

	public void changeAccuracy() {
		gpsAccuracy = accuracySlider.value;
		accuracyText.text = accuracySlider.value.ToString();
	}

	public void changeInterval() {
		gpsUpdateInterval = intervalSlider.value;
		intervalText.text = intervalSlider.value.ToString();
	}

	public static void updatePositions() {
		if (onRoadObjectSpawn != null) {
			Debug.Log("Updating Positions of Signs...");
			onRoadObjectSpawn();
		}
	}

	void Start() {
		changeAccuracy();
		changeInterval();
		// Set the service variable to the phones location manager (Input.location)
		service = Input.location;
		// If the gps service is not enabled by the user
		if (!service.isEnabledByUser) {
			Debug.Log("Location Services not enabled by user");
			debugText.text = ("Location Services not enabled by user");
		} else {
			// Start the service.
			// First parameter is how accurate we want it in meters
			// Second parameter is how far (in meters) we need to move before updating the location
			service.Start(gpsAccuracy, gpsUpdateInterval);
			// Start a coroutine to fetch our location. Because it might take a while, we run it as a coroutine. Running it as is will stall Start()
			StartCoroutine(StartLocation());
		}
	}

	void Update() {
		// If we have a service, update our position
		//if (service.status == LocationServiceStatus.Running) {
		//	myLatitude = service.lastData.latitude;
		//	myLongitude = service.lastData.longitude;
		//	myAltitude = service.lastData.altitude;
		//}
	}

	IEnumerator StartLocation() {
		// A loop to wait for the service starts. Waits a maximum of maxWait seconds
		while (service.status == LocationServiceStatus.Initializing && maxWait > 0) {
			// Go out and wait one seconds before coming back in
			yield return new WaitForSeconds(1);
			maxWait--;
		}

		// If we timed out
		if (maxWait < 1) {
			debugText.text = ("Timed out");
			yield return new WaitForSeconds(1);
		}

		// If the service failed, stop this coroutine forever
		if (service.status == LocationServiceStatus.Failed) {
			debugText.text = ("Unable to determine device location");
			yield return new WaitForSeconds(1);
		} else {
			debugText.text = ("Eyyyyy");
			// Otherwise, update our location
			StartCoroutine(GetLocation());
		}
	}

	// The coroutine that gets our current GPS position
	IEnumerator GetLocation() {
		// Otherwise, update our location
		myLatitude = service.lastData.latitude;
		myLongitude = service.lastData.longitude;
		myAltitude = service.lastData.altitude;
		debugText.text = myLatitude + ", " + myLongitude;
		initialPositionUpdated = true;
		updatePositions();
		// Wait a second to update. Can be removed if wanted, but if it requests updates too quickly, something bad might happen.
		// Comment to see if it is faster
		yield return new WaitForSeconds(0.5f);
		StartCoroutine(GetLocation());
	}

	// The struct which contains latitude, longitude and altitude
	public struct GPSLocation {
		public double latitude;
		public double longitude;
		public double altitude;
		public Objekt obj;

		// Constructor for only latitude and longitude
		public GPSLocation(double lat, double lon) {
			this.latitude = lat;
			this.longitude = lon;
			this.altitude = 0;
			this.obj = null;
		}
		// Constructor for latitude, longitude, and altitude
		public GPSLocation(double lat, double lon, double alt) {
			this.latitude = lat;
			this.longitude = lon;
			this.altitude = alt;
			this.obj = null;
		}

		public override string ToString() {
			return latitude + ", " + longitude + ", " + altitude;
		}
	}

	void OnGUI() {
		if (GUI.Button(new Rect(Screen.width * 0.9f - 10, Screen.height - 150, Screen.width / 10, Screen.height / 20), "Restart GPS")) {
			StopCoroutine(StartLocation());
			StopCoroutine(GetLocation());
			service.Stop();
			service.Start(gpsAccuracy, gpsUpdateInterval);
			StartCoroutine(StartLocation());
		}

		//if (GUI.Button(new Rect(Screen.width * 0.9f - 10, Screen.height - 350, Screen.width / 10, Screen.height / 20), "Manual update positions")) {
		//	updatePositions();
		//}
	}
}
