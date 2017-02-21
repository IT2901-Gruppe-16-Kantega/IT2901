using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
Handles the GPS on the phone
*/
public class GPSManager : MonoBehaviour {
	// How many seconds to wait max
	private int maxWait = 10;

	// Our defaukt latitude, longitude, and altitude
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
			StartCoroutine(GetLocation());
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

	// The coroutine that gets our current GPS position
	IEnumerator GetLocation() {
		// A loop to wait for the service starts. Waits a maximum of maxWait seconds
		while (service.status == LocationServiceStatus.Initializing && maxWait > 0) {
			// Go out and wait one seconds before coming back in
			yield return new WaitForSeconds(1);
			maxWait--;
		}

		// If we timed out, stop this coroutine forever
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
			myLatitude = service.lastData.latitude;
			myLongitude = service.lastData.longitude;
			myAltitude = service.lastData.altitude;
			initialPositionUpdated = true;
			updatePositions();
		}
		// Wait a second to update. Can be removed if wanted, but if it requests updates too quickly, something bad might happen.
		yield return new WaitForSeconds(1);
		StartCoroutine(GetLocation());
	}
}
