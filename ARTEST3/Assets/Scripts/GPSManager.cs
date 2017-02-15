using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
Handles the GPS on the phone
*/
public class GPSManager : MonoBehaviour {
	// SerializeField to make this visible only in the editor
	// The text on the UI 
	[SerializeField]
	private Text text;

	// How many seconds to wait max
	private int maxWait = 10;

	// The direction of true north in 360 degrees. 0/360 is true north
	private float trueNorth = 0f;
	// How accurate the trueNorth is. Possibly not functioning in unity 5.4? not really a big deal
	private float headingAccuracy = 0f;

	// Our defaukt latitude, longitude, and altitude
	// Default is somewhere in the middle of Trondheim
	public float myLatitude = 63.4238907f;
	public float myLongitude = 10.3990959f;
	public float myAltitude = 10f;

	// The Service that handles the GPS on the phone
	private LocationService service;

	void Start() {
		// Start a coroutine to fetch our location. Because it might take a while, we run it as a coroutine. Running it as is will stall Start()
		StartCoroutine(GetLocation());
	}

	void Update() {
		// Clear out the text
		text.text = "";
		// If the compass is enabeld update trueNorth and headingAccuracy
		if (Input.compass.enabled) {
			trueNorth = Input.compass.trueHeading;
			headingAccuracy = Input.compass.headingAccuracy;
			text.text += "True North: " + trueNorth + " +- " + headingAccuracy;
		}
		// If we have a service, update our position
		if (service != null) {
			text.text += service.status;
			if(service.status == LocationServiceStatus.Running) {
				myLatitude = service.lastData.latitude;
				myLongitude = service.lastData.longitude;
				myAltitude = service.lastData.altitude;
				text.text += "\nLatitude: " + myLatitude + 
							"\nLongitude: " + myLongitude + 
							"\nAltitude: " + myAltitude;
			}
		}
	}

	// The coroutine that gets our current GPS position
	IEnumerator GetLocation() {
		// Set the text
		text.text = "starting";
		// Set the service variable to the phones location manager (Input.location)
		service = Input.location;
		// If the gps service is not enabled by the user
		if (!service.isEnabledByUser) {
			// Write it to the text UI and stop this coroutine permanently
			text.text = ("Location Services not enabled by user");
			yield break;
		}
		// Start the service.
		// First parameter is how accurate we want it in meters
		// Second parameter is how far (in meters) we need to move before updating the location
		service.Start(5f, 5f);
		// A loop to wait for the service starts. Waits a maximum of maxWait seconds
		while (service.status == LocationServiceStatus.Initializing && maxWait > 0) {
			// Go out and wait one seconds before coming back in
			yield return new WaitForSeconds(1);
			maxWait--;
		}
		
		// If we timed out, stop this coroutine forever
		if (maxWait < 1) {
			text.text = ("Timed out");
			yield break;
		}
		// If the service failed, stop this coroutine forever
		if (service.status == LocationServiceStatus.Failed) {
			text.text = ("Unable to determine device location");
			yield break;
		} else {
		// Otherwise, update our location
			myLatitude = service.lastData.latitude;
			myLongitude = service.lastData.longitude;
			myAltitude = service.lastData.altitude;
			// Enable the device compass and get the trueHeading and headingAccuracy
			Input.compass.enabled = true;
			trueNorth = Input.compass.trueHeading;
			headingAccuracy = Input.compass.headingAccuracy;
		}

		// Start a new coroutine to update our position and compass
		//StartCoroutine(UpdatePositionAndHeading());
	}

	// The coroutine which updates our position and heading
	IEnumerator UpdatePositionAndHeading() {
		// Run this forever
		while(true) {
			// Clear out the text
			text.text = "";
			// If the compass is enabeld update trueNorth and headingAccuracy
			if (Input.compass.enabled) {
				trueNorth = Input.compass.trueHeading;
				headingAccuracy = Input.compass.headingAccuracy;
				text.text += "True North: " + trueNorth + " +- " + headingAccuracy;
			}
			// If we have a service, update our position
			if(service != null) {
				myLatitude = service.lastData.latitude;
				myLongitude = service.lastData.longitude;
				myAltitude = service.lastData.altitude;
				text.text += "\nLatitude: " + myLatitude + "\nLongitude: " + myLongitude + "\nAltitude: " + myAltitude;
			}
			// Go out and wait one second and come back in again
			yield return new WaitForSeconds(1);
		}
	}

}
