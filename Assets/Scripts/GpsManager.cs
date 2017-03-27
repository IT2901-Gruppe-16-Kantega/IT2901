﻿using System.Collections;
using UnityEngine;

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
	private bool _gpsSet;
	private const float GpsAccuracy = 5f; // Accuracy in meters
	private const float GpsUpdateInterval = 5f; // How many meters before it updates

	private void Start() {
		// Set the service variable to the phones location manager (Input.location)
		_service = Input.location;
		// If the gps service is not enabled by the user
		if (!_service.isEnabledByUser) {
			Debug.Log("Location Services not enabled by user");
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

	private void Update() {
		if (!_gpsSet || !_service.isEnabledByUser)
			return;
		_oldLocation = MyLocation;
		MyLocation.Latitude = _service.lastData.latitude;
		MyLocation.Longitude = _service.lastData.longitude;
		MyLocation.Altitude = _service.lastData.altitude;
		if (!InitialPositionUpdated) {
			_oldLocation = MyLocation;
			InitialPositionUpdated = true;
		}
		double distance = HelperFunctions.Haversine(_oldLocation, MyLocation);
		double bearing = HelperFunctions.CalculateBearing(_oldLocation, MyLocation);
		transform.position =
			Vector3.Lerp(transform.position,
				new Vector3(
					(float) (-System.Math.Cos(bearing) * distance),
					0,
					(float) (System.Math.Sin(bearing) * distance)
				), Dampening);
	}

	/// <summary>
	/// Starts location service. Retries if it fails. Fails up to MaxTries amount of times
	/// </summary>
	private IEnumerator StartLocation() {
		// A loop to wait for the service starts. Waits a maximum of MaxWait seconds
		while (_service.status == LocationServiceStatus.Initializing && _waitTime < MaxWait) {
			yield return new WaitForSeconds(1);
			_waitTime++;
		}

		// If we timed out
		if (_waitTime >= MaxWait) {
			yield return new WaitForSeconds(1);
		}
		
		if (_service.status == LocationServiceStatus.Failed) {
			yield return new WaitForSeconds(1);
			if (_tries >= MaxTries)
				yield break;
			_tries++;
			StartCoroutine(StartLocation());
		} else {
			_gpsSet = true;
		}
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
}
