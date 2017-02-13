using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateObjects : MonoBehaviour {

	// The API base url
	const string API_URL = "https://www.vegvesen.no/nvdb/api/v2/";
	// The earth's mean radius in meters
	const float EARTH_MEAN_RADIUS = 6372.8e3f;

	// Our location. Default location is somewhere in the middle of Trondheim
	GPSLocation myLocation = new GPSLocation(63.411595, 10.438659);
	// The list containing the locations of each road object
	List<GPSLocation> roadObjectList = new List<GPSLocation>();

	// The characters we dont want when parsing the geometry.wkt
	char[] delimiterChars = { ' ', '(', ')' };

	// The object to instantiate (create) when placing the road objects
	public GameObject aGameObjectToGenerate;

	/* 
	The deviation between the latitude and longitude of your current location
	Used when querying the database to limit how many items we get. A bigger number
	means that you extend the area around you.
	*/ 
	private float deltaLatLong = 0.001f;

	// SerializeField makes the private field visible to the editor
	// Currently, no other GameObject needs the GPSManager, so this is fine
	// May want to make GPSManager static if all objects need access
	[SerializeField]
	private GPSManager gpsManager;

	// Use this for initialization
	void Start() {
		/*
		Get the GPSManager script atteched to this same object. Gives you an error if this gameobject does not have the GPSManager script attached
		so make sure that it has it attached
		*/
		gpsManager = GetComponent<GPSManager>();
		// Update our location using whatever data the GPSManager got
		myLocation = new GPSLocation(gpsManager.myLatitude, gpsManager.myLongitude, gpsManager.myAltitude);
		// The query to fetch road signs
		string url = API_URL + "vegobjekter/96?inkluder=geometri&srid=4326&kartutsnitt=" +
			(myLocation.longitude - deltaLatLong) + "," +
			(myLocation.latitude - deltaLatLong) + "," +
			(myLocation.longitude + deltaLatLong) + "," +
			(myLocation.latitude + deltaLatLong);
		// A dictionary that contains the relevant headers we need to send to the API
		Dictionary<string, string> headers = new Dictionary<string, string>();
		// Want it in JSON format
		headers.Add("Accept", "application/vnd.vegvesen.nvdb-v2+json");
		// Make a WWW (similar to fetch)
		WWW www = new WWW(url, null, headers);
		print(url);
		// Start a coroutine that tries to get the data from the API
		// We dont want this as a method on it's own, because it will stall the Start() method
		StartCoroutine(WaitForRequest(www));
	}

	// Update is called once per frame
	void Update() {
		// If we have a gpsManager
		if(gpsManager != null) {
			// Update our location
			myLocation = new GPSLocation(gpsManager.myLatitude, gpsManager.myLongitude, gpsManager.myAltitude);
		}
	}

	// The haversine formula calculates the distance between two gps locations by air (ignoring altitude).
	// Paramters:
	//		GPSLocation startLocation -> The location where we are
	//		GPSLocation endLocation -> The location where the object is
	// Returns the distance between the startLocation and endLocation in meters (1 Unit = 1 meter for simplicity)
	float Haversine(GPSLocation startLocation, GPSLocation endLocation) {
		double dLat = (endLocation.latitude - startLocation.latitude) * Mathf.Deg2Rad;
		double dLon = (endLocation.longitude - startLocation.longitude) * Mathf.Deg2Rad;
		startLocation.latitude *= Mathf.Deg2Rad;
		endLocation.latitude *= Mathf.Deg2Rad;

		float a = Mathf.Pow(Mathf.Sin((float) dLat / 2), 2) + Mathf.Pow(Mathf.Sin((float) dLon / 2), 2) * Mathf.Cos((float) startLocation.latitude) * Mathf.Cos((float) endLocation.latitude);
		float c = 2 * Mathf.Asin(Mathf.Sqrt(a));
		return EARTH_MEAN_RADIUS * 2 * c;
	}

	// The formula that calculates the bearing when travelling from startLocation to endLocation
	// Parameters:
	//		GPSLocation startLocation -> The location where we are
	//		GPSLocation endLocation -> The location where the object is
	// Returns the bearing from startLocation to endLocation in radians
	float CalculateBearing(GPSLocation startLocation, GPSLocation endLocation) {
		float x = Mathf.Cos((float) startLocation.latitude * Mathf.Deg2Rad)
				* Mathf.Sin((float) endLocation.latitude * Mathf.Deg2Rad)
				- Mathf.Sin((float) startLocation.latitude * Mathf.Deg2Rad) 
				* Mathf.Cos((float) endLocation.latitude * Mathf.Deg2Rad) 
				* Mathf.Cos((float) (endLocation.longitude - startLocation.longitude) * Mathf.Deg2Rad);
		float y = Mathf.Sin((float) (endLocation.longitude - startLocation.longitude) * Mathf.Deg2Rad) 
				* Mathf.Cos((float) endLocation.latitude * Mathf.Deg2Rad);
		return Mathf.Atan2(y, x) + Mathf.PI / 2;
	}

	// Uses the locations in roadObjectList and instantiates objects
	void MakeObjectsFromLatLon() {
		// For each location in the list
		foreach (GPSLocation location in roadObjectList) {
			// Create a new position where x, y, and z is 0
			Vector3 position = Vector3.zero;
			// Calculate the distance and bearing between us and the location
			float distance = Haversine(myLocation, location);
			float bearing = CalculateBearing(myLocation, location);
			// calculate the x and z offset between us and the location and update the x and z position
			position.x = -Mathf.Cos(bearing) * distance;
			position.z = Mathf.Sin(bearing) * distance;

			// Instantiate a new GameObject on that location relative to us
			GameObject newGameObject = (GameObject) Instantiate(aGameObjectToGenerate, position, Quaternion.identity);
			// Set the parent of the new GameObject to be us (so we dont have a huge list in root)
			newGameObject.transform.parent = gameObject.transform;
		}
	}

	// The coroutine that gets the data from the API
	// Parameters:
	//		WWW www -> The request URL with the correct headers
	IEnumerator WaitForRequest(WWW www) {
		// Request data from the API and come back when it's done
		yield return www;
		// If it has an error, print out the error
		if (!string.IsNullOrEmpty(www.error)) {
			Debug.Log("WWW Error: " + www.error);
		} else {
		// Else handle the data
			// For debugging purposes
			Debug.Log(roadObjectList.Count);
			Debug.Log("WWW Ok!: " + www.text);

			// Make a new RootObject and parse the json data from the request
			RootObject data = JsonUtility.FromJson<RootObject>(www.text);
			// Go through each Objekter in the data.objekter (the road objects)
			foreach(Objekter o in data.objekter) {
				// For debugging purposes
				Debug.Log(o.geometri.wkt);

				// Split the object's wkt using the delimiterChars defined above
				string[] wkt = o.geometri.wkt.Split(delimiterChars);
				// Make a new GPSLocation using the values from the splitted text
				// TODO Currently only supports POINT because POINT has 3 points of data
				GPSLocation oLocation = new GPSLocation(double.Parse(wkt[2]), double.Parse(wkt[3]), double.Parse(wkt[4]));

				// For debugging purposes
				Debug.Log(oLocation.latitude + " - " + oLocation.longitude + " - " + oLocation.altitude);

				// Add the location to our roadObjectList
				roadObjectList.Add(oLocation);
			}
			// For debuggin purposes
			Debug.Log(roadObjectList.Count);

			// Run the method that instantiates the GameObjects from the locations
			MakeObjectsFromLatLon();
		}
	}

	// The struct which contains latitude, longitude and altitude
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
	}
}

// The objects that exist in the JSON data returned from the API
// This only works for road signs (I think)
// TODO find a way to parse all types of data from the API instead of just this.
// As it is right now, it works.
[Serializable]
public class Geometri {
	public string wkt;
	public int srid;
	public bool egengeometri;
}

[Serializable]
public class Objekter {
	public int id;
	public string href;
	public Geometri geometri;
}

[Serializable]
public class Neste {
	public string start;
	public string href;
}

[Serializable]
public class Metadata {
	public int antall;
	public int returnert;
	public Neste neste;
}

[Serializable]
public class RootObject {
	public List<Objekter> objekter;
	public Metadata metadata;
}