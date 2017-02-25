using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class APIWrapper : MonoBehaviour {

	// The API base url
	const string API_URL = "https://www.vegvesen.no/nvdb/api/v2/";

	// The characters we dont want when parsing the geometry.wkt
	char[] delimiterChars = { ' ', '(', ')' };
	string regexPattern = @"[^0-9.]+";

	/* 
	The deviation between the latitude and longitude of your current location
	Used when querying the database to limit how many items we get. A bigger number
	means that you extend the area around you. Because of how coordinates work,
	to get a square, longitude needs to have a bigger delta.
    */
	private float deltaLat = 0.002f;
	private float deltaLong = 0.003f;

	private Dictionary<string, string> CreateHeaders() {
		Dictionary<string, string> headers = new Dictionary<string, string>();

		// Want it in JSON format
		headers.Add("Accept", "application/vnd.vegvesen.nvdb-v2+json");

		// To identify ourselves
		headers.Add("X-Client", "");
		headers.Add("X-Kontaktperson", "");

		return headers;
	}

	// Use this to create and return a fetch request
	private WWW CreateFetchRequest(double latitude, double longitude) {
		// The query to fetch road signs
		string url = API_URL + "vegobjekter/96?inkluder=geometri,egenskaper&srid=4326&kartutsnitt=" +
			(longitude - deltaLong) + "," +
			(latitude - deltaLat) + "," +
			(longitude + deltaLong) + "," +
			(latitude + deltaLat);

		Debug.Log(url);

		// A dictionary that contains the relevant headers we need to send to the API

		// Make a WWW (similar to fetch)       
		return new WWW(url, null, CreateHeaders());
	}

	public void FetchObjectTypes() {
		string url = API_URL + "vegobjekttyper";
		WWW www = new WWW(url, null, CreateHeaders());

		StartCoroutine(WaitForObjectTypeRequest(www, objectTypes => {
			Debug.Log(objectTypes.Count);
		}));

	}

	public void FetchObjects(GPSManager.GPSLocation location, Action<List<GPSManager.GPSLocation>> callback) {
		WWW www = CreateFetchRequest(location.latitude, location.longitude);

		// Start a coroutine that tries to get the data from the API
		StartCoroutine(WaitForRequest(www, objects => {
			callback(objects);
		}));
	}

	// The coroutine that gets the data from the API
	// Parameters:
	//      WWW www -> The request URL with the correct headers
	IEnumerator WaitForRequest(WWW www, Action<List<GPSManager.GPSLocation>> callback) {
		// Request data from the API and come back when it's done
		yield return www;

		// If it has an error, print out the error
		if (!string.IsNullOrEmpty(www.error)) {
			Debug.Log("WWW Error: " + www.error);
		} else {
			List<GPSManager.GPSLocation> roadObjectList = new List<GPSManager.GPSLocation>();

			// Else handle the data
			// For debugging purposes
			//Debug.Log(roadObjectList.Count);
			Debug.Log("WWW Ok!: " + www.text);

			// Make a new RootObject and parse the json data from the request
			RootObject data = JsonUtility.FromJson<RootObject>(www.text);

			// Go through each Objekter in the data.objekter (the road objects)
			foreach (Objekt obj in data.objekter) {
				// For debugging purposes
				//Debug.Log(obj.geometri.wkt);

				// Split the object's wkt using the delimiterChars defined above
				string[] wkt = Regex.Split(obj.geometri.wkt, regexPattern);
				List<double> points = new List<double>();
				foreach (string s in wkt) {
					if(!String.IsNullOrEmpty(s)) {
						points.Add(double.Parse(s));
					}
				}

				// Make a new GPSLocation using the values from the splitted text
				// TODO Currently only supports POINT because POINT has 3 points of data
				GPSManager.GPSLocation oLocation;
				if(points.Count == 3) {
					oLocation = new GPSManager.GPSLocation(points[0], points[1], points[2]);
				} else if (points.Count == 2) {
					oLocation = new GPSManager.GPSLocation(points[0], points[1]);
				} else {
					oLocation = new GPSManager.GPSLocation(points[0], points[1], points[2]);
					// MULTILINE???????
					// ?
				}
				oLocation.obj = obj;

				// For debugging purposes
				//Debug.Log(oLocation.latitude + " - " + oLocation.longitude + " - " + oLocation.altitude);
				Debug.Log(oLocation.ToString());
				// Add the location to our roadObjectList
				roadObjectList.Add(oLocation);
			}
			callback(roadObjectList);

			// For debuggin purposes
			//Debug.Log(roadObjectList.Count);
		}
	}

	// Much the same as WaitForRequest
	IEnumerator WaitForObjectTypeRequest(WWW www, Action<List<ObjectType>> callback) {
		yield return www;

		if (!string.IsNullOrEmpty(www.error)) {
			Debug.Log("WWW Error: " + www.error);
		} else {
			// Need to wrap the returned text into a dictionary key. That's just how JsonUtility works
			string wrapper = string.Format("{{ \"{0}\": {1}}}", www.text, "vegobjekttyper");

			var data = JsonUtility.FromJson<RootObjectType>(wrapper);
			callback(data.vegobjekttyper);
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
public class Objekt {
	public int id;
	public string href;
	public Geometri geometri;
	public List<Dictionary<string, string>> egenskaper;
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
	public List<Objekt> objekter;
	public Metadata metadata;
}

[Serializable]
public class ObjectType {
	public int id;
	public string navn;
	public string beskrivelse;
	public string stedfesting;
	public string objektliste_dato;
	public string sosinavn;
	public string sosinvdbnavn;
	public int sorteringsnummer;
}

[Serializable]
public class RootObjectType {
	public List<ObjectType> vegobjekttyper;
}