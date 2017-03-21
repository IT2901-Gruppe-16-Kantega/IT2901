using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/*
Handles the GPS on the phone
*/
public class GpsManager : MonoBehaviour {
	// How many seconds to wait max
	private const int MaxWait = 10;
	private int _waitTime;
	// How many tries before giving up starting location
	private const int MaxTries = 3;
	private int _tries;
	[Range(0.01f, 1)]
	public float Dampening = 0.8f;

	// Our default latitude, longitude, and altitude
	// Default is somewhere in the middle of Trondheim
	public static GpsLocation MyLocation = new GpsLocation(63.430626, 10.392145, 10);
	private GpsLocation _oldLocation;

	[HideInInspector]
	public static bool InitialPositionUpdated;

	// The Service that handles the GPS on the phone
	private LocationService _service;
	public float GpsAccuracy = 5f;
	public float GpsUpdateInterval = 5f;
	public Slider AccuracySlider;
	public Slider IntervalSlider;
	public Text AccuracyText;
	public Text IntervalText;

	public Text DebugText;

	public void ChangeAccuracy() {
		GpsAccuracy = AccuracySlider.value;
		AccuracyText.text = AccuracySlider.value.ToString();
	}

	public void ChangeInterval() {
		GpsUpdateInterval = IntervalSlider.value;
		IntervalText.text = IntervalSlider.value.ToString();
	}

	private void Start() {
		ChangeAccuracy();
		ChangeInterval();
		// Set the service variable to the phones location manager (Input.location)
		_service = Input.location;
		// If the gps service is not enabled by the user
		if (!_service.isEnabledByUser) {
			Debug.Log("Location Services not enabled by user");
			DebugText.text = ("Location Services not enabled by user");
			InitialPositionUpdated = true;
		} else {
			// Start the service.
			// First parameter is how accurate we want it in meters
			// Second parameter is how far (in meters) we need to move before updating the location
			_service.Start(GpsAccuracy, GpsUpdateInterval);
			// Start a coroutine to fetch our location. Because it might take a while, we run it as a coroutine. Running it as is will stall Start()
			StartCoroutine(StartLocation());
		}
	}

	private IEnumerator StartLocation() {
		// A loop to wait for the service starts. Waits a maximum of maxWait seconds
		while (_service.status == LocationServiceStatus.Initializing && _waitTime < MaxWait) {
			// Go out and wait one seconds before coming back in
			yield return new WaitForSeconds(1);
			_waitTime++;
		}

		// If we timed out
		if (_waitTime >= MaxWait) {
			DebugText.text = ("Timed out");
			yield return new WaitForSeconds(1);
		}

		// If the service failed, try again
		if (_service.status == LocationServiceStatus.Failed) {
			DebugText.text = ("Unable to determine device location");
			yield return new WaitForSeconds(1);
			if (_tries >= MaxTries)
				yield break;
			_tries++;
			StartCoroutine(StartLocation());
		} else {
			DebugText.text = ("Eyyyyy");
			// Otherwise, update our location
			StartCoroutine(GetLocation());
		}
	}

	// The coroutine that gets our current GPS position
	// ReSharper disable once FunctionRecursiveOnAllPaths
	private IEnumerator GetLocation() {
		// Otherwise, update our location
		_oldLocation = MyLocation;
		MyLocation = new GpsLocation(_service.lastData.latitude, _service.lastData.longitude, _service.lastData.altitude);
		if (!InitialPositionUpdated)
			_oldLocation = MyLocation;
		double distance = HelperFunctions.Haversine(_oldLocation, MyLocation);
		double bearing = HelperFunctions.CalculateBearing(_oldLocation, MyLocation);
		transform.position =
			Vector3.Lerp(transform.position,
				new Vector3(
					(float) (-System.Math.Cos(bearing) * distance),
					0,
					(float) (System.Math.Sin(bearing) * distance)
				), Dampening);

		DebugText.text = MyLocation.Latitude + ", " + MyLocation.Longitude;
		DebugText.text += "\n" + _oldLocation.Latitude + ", " + _oldLocation.Longitude;
		InitialPositionUpdated = true;
		// Wait a second to update. Can be removed if wanted, but if it requests updates too quickly, something bad might happen.
		// Comment to see if it is faster
		yield return new WaitForSeconds(0.5f);
		StartCoroutine(GetLocation());
	}

	// The struct which contains latitude, longitude and altitude
	[System.Serializable]
	public struct GpsLocation {
		public double Latitude;
		public double Longitude;
		public double Altitude;

		// Constructor for only latitude and longitude
		public GpsLocation(double lat, double lon) {
			Latitude = lat;
			Longitude = lon;
			Altitude = 0;
		}
		// Constructor for latitude, longitude, and altitude
		public GpsLocation(double lat, double lon, double alt) {
			Latitude = lat;
			Longitude = lon;
			Altitude = alt;
		}

		public GpsLocation(GpsLocation other) {
			Latitude = other.Latitude;
			Longitude = other.Longitude;
			Altitude = other.Altitude;
		}

		public override string ToString() {
			return Latitude + ", " + Longitude + ", " + Altitude;
		}
	}

	private void OnGUI() {
		if (!GUI.Button(new Rect(Screen.width * 0.9f - 10, Screen.height - 150, Screen.width / 10f, Screen.height / 20f),
				"Restart GPS"))
			return; // To reduce nesting.

		StopCoroutine(StartLocation());
		StopCoroutine(GetLocation());
		_service.Stop();
		_service.Start(GpsAccuracy, GpsUpdateInterval);
		StartCoroutine(StartLocation());
	}
}
