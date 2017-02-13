using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GPSManager : MonoBehaviour {

	float trueNorth = 0f;
	float headingAccuracy = 0f;

	[SerializeField]
	private Text text;
	private int maxWait = 10;

	public float myLatitude = 63.4238907f;
	public float myLongitude = 10.3990959f;
	public float myAltitude = 10f;

	LocationService service;

	void Start() {
		StartCoroutine(getLocation());
	}
	IEnumerator getLocation() {
		text.text = "starting";
		service = Input.location;
		if (!service.isEnabledByUser) {
			text.text = ("Location Services not enabled by user");
			yield break;
		}
		service.Start(5f, 5f);
		while (service.status == LocationServiceStatus.Initializing && maxWait > 0) {
			yield return new WaitForSeconds(1);
			maxWait--;
		}
		//service.Stop();

		if (maxWait < 1) {
			text.text = ("Timed out");
			yield break;
		}
		if (service.status == LocationServiceStatus.Failed) {
			text.text = ("Unable to determine device location");
			yield break;
		} else {
			myLatitude = service.lastData.latitude;
			myLongitude = service.lastData.longitude;
			myAltitude = service.lastData.altitude;
			Input.compass.enabled = true;
			trueNorth = Input.compass.trueHeading;
			headingAccuracy = Input.compass.headingAccuracy;
		}
		StartCoroutine(UpdateHeader());
	}

	IEnumerator UpdateHeader() {
	while(true) {
			if (Input.compass.enabled) {
				trueNorth = Input.compass.trueHeading;
				headingAccuracy = Input.compass.headingAccuracy;
				text.text = "True North: " + trueNorth + " +- " + headingAccuracy;
			}
			if(service != null) {
				myLatitude = service.lastData.latitude;
				myLongitude = service.lastData.longitude;
				myAltitude = service.lastData.altitude;
			}
			yield return new WaitForSeconds(1);
		}
	}

}
