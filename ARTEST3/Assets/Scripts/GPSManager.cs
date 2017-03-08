using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/*
Handles the GPS on the phone
*/
public class GPSManager : MonoBehaviour {
	// How many seconds to wait max
	const int MAX_WAIT = 10;
	private int waitTime = 0;
	// How many tries before giving up starting location
	private int maxTries = 3;
	private int tries = 0;
	[Range(0.01f, 1)]
	public float dampening = 0.8f;

	// Our default latitude, longitude, and altitude
	// Default is somewhere in the middle of Trondheim
	public static GPSLocation myLocation = new GPSLocation(63.430626, 10.392145, 10);
	private GPSLocation oldLocation = new GPSLocation();

	[HideInInspector]
	public static bool initialPositionUpdated = false;

	// The Service that handles the GPS on the phone
	private LocationService service;
	public float gpsAccuracy = 5f;
	public float gpsUpdateInterval = 5f;
	public Slider accuracySlider;
	public Slider intervalSlider;
	public Text accuracyText;
	public Text intervalText;

	public Text debugText;

	public void changeAccuracy() {
		gpsAccuracy = accuracySlider.value;
		accuracyText.text = accuracySlider.value.ToString();
	}

	public void changeInterval() {
		gpsUpdateInterval = intervalSlider.value;
		intervalText.text = intervalSlider.value.ToString();
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
			initialPositionUpdated = true;
		} else {
			// Start the service.
			// First parameter is how accurate we want it in meters
			// Second parameter is how far (in meters) we need to move before updating the location
			service.Start(gpsAccuracy, gpsUpdateInterval);
			// Start a coroutine to fetch our location. Because it might take a while, we run it as a coroutine. Running it as is will stall Start()
			StartCoroutine(StartLocation());
		}
	}

	IEnumerator StartLocation() {
		// A loop to wait for the service starts. Waits a maximum of maxWait seconds
		while (service.status == LocationServiceStatus.Initializing && waitTime < MAX_WAIT) {
			// Go out and wait one seconds before coming back in
			yield return new WaitForSeconds(1);
			waitTime++;
		}

		// If we timed out
		if (waitTime >= MAX_WAIT) {
			debugText.text = ("Timed out");
			yield return new WaitForSeconds(1);
		}

		// If the service failed, try again
		if (service.status == LocationServiceStatus.Failed) {
			debugText.text = ("Unable to determine device location");
			yield return new WaitForSeconds(1);
			if (tries < maxTries) {
				tries++;
				StartCoroutine(StartLocation());
			}

		} else {
			debugText.text = ("Eyyyyy");
			// Otherwise, update our location
			StartCoroutine(GetLocation());
		}
	}

	// The coroutine that gets our current GPS position
	IEnumerator GetLocation() {
		// Otherwise, update our location
		oldLocation = myLocation;
		myLocation = new GPSLocation(service.lastData.latitude, service.lastData.longitude, service.lastData.altitude);
		if (!initialPositionUpdated)
			oldLocation = myLocation;
		double distance = HelperFunctions.Haversine(oldLocation, myLocation);
		double bearing = HelperFunctions.CalculateBearing(oldLocation, myLocation);
		transform.position =
			Vector3.Lerp(transform.position,
				new Vector3(
					(float) (-System.Math.Cos(bearing) * distance),
					0,
					(float) (System.Math.Sin(bearing) * distance)
				), dampening);

		debugText.text = myLocation.latitude + ", " + myLocation.longitude;
		debugText.text += "\n" + oldLocation.latitude + ", " + oldLocation.longitude;
		initialPositionUpdated = true;
		// Wait a second to update. Can be removed if wanted, but if it requests updates too quickly, something bad might happen.
		// Comment to see if it is faster
		yield return new WaitForSeconds(0.5f);
		StartCoroutine(GetLocation());
	}

	// The struct which contains latitude, longitude and altitude
	[System.Serializable]
	public struct GPSLocation {
		public double latitude;
		public double longitude;
		public double altitude;

		// Constructor for only latitude and longitude
		public GPSLocation(double lat, double lon) {
			this.latitude = lat;
			this.longitude = lon;
			this.altitude = 0;
		}
		// Constructor for latitude, longitude, and altitude
		public GPSLocation(double lat, double lon, double alt) {
			this.latitude = lat;
			this.longitude = lon;
			this.altitude = alt;
		}

		public GPSLocation(GPSLocation other) {
			latitude = other.latitude;
			longitude = other.longitude;
			altitude = other.altitude;
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
	}
}
