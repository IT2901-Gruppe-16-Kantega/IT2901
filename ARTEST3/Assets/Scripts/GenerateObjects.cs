using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateObjects : MonoBehaviour {

	const string API_URL = "https://www.vegvesen.no/nvdb/api/v2/";
	const float EARTH_MEAN_RADIUS = 6372.8e3f;

	GPSLocation myLocation = new GPSLocation(63.411595, 10.438659);
	List<GPSLocation> roadObjectList = new List<GPSLocation>();

	char[] delimiterChars = { ' ', '(', ')' };

	public GameObject aGameObjectToGenerate;

	/* 
	The deviation between the latitude and longitude of your current location
	Used when querying the database to limit how many items we get. A bigger number
	means that you extend the area around you.
	*/ 
	private float deltaLatLong = 0.001f;

	[SerializeField]
	private GPSManager gpsManager;

	// Use this for initialization
	void Start() {
		/*
		Get the GPSManager script atteched to this same object. Gives you an error if this gameobject does not have the GPSManager script attached
		so make sure that it has it attached
		*/
		gpsManager = GetComponent<GPSManager>();
		myLocation = new GPSLocation(gpsManager.myLatitude, gpsManager.myLongitude, gpsManager.myAltitude);
		string url = API_URL + "vegobjekter/96?inkluder=geometri&srid=4326&kartutsnitt=" +
		(myLocation.longitude - deltaLatLong) + "," +
		(myLocation.latitude - deltaLatLong) + "," +
		(myLocation.longitude + deltaLatLong) + "," +
		(myLocation.latitude + deltaLatLong);
		Dictionary<string, string> headers = new Dictionary<string, string>();
		headers.Add("Accept", "application/vnd.vegvesen.nvdb-v2+json");
		WWW www = new WWW(url, null, headers);
		print(url);
		StartCoroutine(waitForRequest(www));
	}

	// Update is called once per frame
	void Update() {
		if(gpsManager != null) {
			myLocation = new GPSLocation(gpsManager.myLatitude, gpsManager.myLongitude, gpsManager.myAltitude);
		}
	}

	float Haversine(GPSLocation startLocation, GPSLocation endLocation) {
		double dLat = (endLocation.latitude - startLocation.latitude) * Mathf.Deg2Rad;
		double dLon = (endLocation.longitude - startLocation.longitude) * Mathf.Deg2Rad;
		startLocation.latitude *= Mathf.Deg2Rad;
		endLocation.latitude *= Mathf.Deg2Rad;

		float a = Mathf.Pow(Mathf.Sin((float) dLat / 2), 2) + Mathf.Pow(Mathf.Sin((float) dLon / 2), 2) * Mathf.Cos((float) startLocation.latitude) * Mathf.Cos((float) endLocation.latitude);
		float c = 2 * Mathf.Asin(Mathf.Sqrt(a));
		return EARTH_MEAN_RADIUS * 2 * c;
	}

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

	void MakeObjectsFromLatLon() {
		foreach (GPSLocation location in roadObjectList) {
			Vector3 position = Vector3.zero;
			float distance = Haversine(myLocation, location);
			float bearing = CalculateBearing(myLocation, location);
			position.x = -Mathf.Cos(bearing) * distance;
			position.z = Mathf.Sin(bearing) * distance;

			GameObject newGameObject = (GameObject) Instantiate(aGameObjectToGenerate, position, Quaternion.identity);
			newGameObject.transform.parent = gameObject.transform;
		}
	}

	IEnumerator waitForRequest(WWW www) {
		yield return www;
		if (!string.IsNullOrEmpty(www.error)) {
			print("WWW Error: " + www.error);
		} else {
			Debug.Log(roadObjectList.Count);
			print("WWW Ok!: " + www.text);
			RootObject data = JsonUtility.FromJson<RootObject>(www.text);
			foreach(Objekter o in data.objekter) {
				Debug.Log(o.geometri.wkt);
				string[] wkt = o.geometri.wkt.Split(delimiterChars);
				GPSLocation oLocation = new GPSLocation(double.Parse(wkt[2]), double.Parse(wkt[3]), double.Parse(wkt[4]));
				Debug.Log(oLocation.latitude + " - " + oLocation.longitude + " - " + oLocation.altitude);
				roadObjectList.Add(oLocation);
			}
			Debug.Log(roadObjectList.Count);
			MakeObjectsFromLatLon();
		}
	}

	public struct GPSLocation {
		public double latitude;
		public double longitude;
		public double altitude;
		public GPSLocation(double lat, double lon) {
			this.latitude = lat;
			this.longitude = lon;
			this.altitude = 0;
		}
		public GPSLocation(double lat, double lon, double alt) {
			this.latitude = lat;
			this.longitude = lon;
			this.altitude = alt;
		}
	}
}

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