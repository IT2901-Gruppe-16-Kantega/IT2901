using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public class ApiWrapper : MonoBehaviour {

	// The API base url
	private const string ApiUrl = "https://www.vegvesen.no/nvdb/api/v2/";

	/* 
	The deviation between the latitude and longitude of your current location
	Used when querying the database to limit how many items we get. A bigger number
	means that you extend the area around you. Because of how coordinates work,
	to get a square, longitude needs to have a bigger delta.
	*/
	private const float DeltaLat = 0.0015f;
	private const float DeltaLong = 0.0025f;

	private static Dictionary<string, string> CreateHeaders() {
		Dictionary<string, string> headers = new Dictionary<string, string>
		{
			{"Accept", "application/vnd.vegvesen.nvdb-v2+json"},
			{"X-Client", "Client"},
			{"X-Kontaktperson", "Contact"}
		};
		return headers;
	}

	// Use this to create and return a fetch request
	private WWW CreateFetchRequest(int id, double latitude, double longitude) {
		// The query to fetch road signs
		string url = ApiUrl + "vegobjekter/" + id + "?inkluder=geometri,egenskaper,relasjoner&pretty=true&srid=4326&kartutsnitt=" +
			(longitude - DeltaLong) + "," +
			(latitude - DeltaLat) + "," +
			(longitude + DeltaLong) + "," +
			(latitude + DeltaLat);
		Debug.Log(url);

		// Make a WWW (similar to fetch)       
		return new WWW(url, null, CreateHeaders());
	}

	public void FetchObjects(int id, GpsManager.GpsLocation location, Action<List<Objekt>> callback) {
		WWW www = CreateFetchRequest(id, location.Latitude, location.Longitude);

		// Start a coroutine that tries to get the data from the API
		StartCoroutine(WaitForRequest(www, objects => {
			callback(objects);
			if (!string.IsNullOrEmpty(www.error))
				return;
			// If there is no error and the requested object isnt a road
			// Quick and dirty, only if the requested id is 532. Obviously should do this someplace else
			if (id == 532) {
				// Try to save the data
				Debug.Log(LocalStorage.SaveData("roads.json", www.text) ? "FILE SAVED" : "The file failed to save.");
			} else {
				// Try to save the data
				Debug.Log(LocalStorage.SaveData("data.json", www.text) ? "FILE SAVED" : "The file failed to save.");
			}
		}));
	}

	// The coroutine that gets the data from the API
	// Parameters:
	//      WWW www -> The request URL with the correct headers
	private IEnumerator WaitForRequest(WWW www, Action<List<Objekt>> callback) {
		// Request data from the API and come back when it's done
		yield return www;

		// If it has an error, print out the error
		if (!string.IsNullOrEmpty(www.error)) {
			Debug.Log("WWW Error: " + www.error);
		} else {
			List<Objekt> roadObjectList = new List<Objekt>();

			// Else handle the data
			Debug.Log("WWW Ok!: " + www.text);

			// Make a new RootObject and parse the json data from the request
			RootObject data = JsonUtility.FromJson<RootObject>(www.text);

			// Go through each Objekter in the data.objekter (the road objects)
			foreach (Objekt obj in data.objekter) {
				// Add the location to our roadObjectList
				roadObjectList.Add(ParseObject(obj));
			}
			callback(roadObjectList);
		}
	}

	public Objekt ParseObject(Objekt objekt) {
		// For debugging purposes
		//Debug.Log(obj.geometri.wkt);

		string wkt = objekt.geometri.wkt;
		wkt = wkt.Substring(wkt.IndexOf("(", StringComparison.Ordinal) + 1).Trim(')');

		//[63.429624610409434, 10.393547899740911, 10.9]
		string[] wktArray = wkt.Split(',');

		List<GpsManager.GpsLocation> coordinates = new List<GpsManager.GpsLocation>();
		foreach (string s in wktArray) {
			string[] sArray = s.Trim().Split(' ');
			double latitude;
			double.TryParse(sArray[0], out latitude);

			double longitude;
			double.TryParse(sArray[1], out longitude);

			if (sArray.Length == 2) {
				coordinates.Add(new GpsManager.GpsLocation(latitude, longitude));
			} else {
				double altitude = double.Parse(sArray[2]);
				coordinates.Add(new GpsManager.GpsLocation(latitude, longitude, altitude));
			}
		}
		objekt.parsedLocation = coordinates;
		return objekt;
	}
}

// The objects that exist in the JSON data returned from the API
// This only works for road signs (I think)
// TODO find a way to parse all types of data from the API instead of just this.
// As it is right now, it works.
// [SuppressMessage("ReSharper", "InconsistentNaming")] is to supress Visual Studio plugin (ReSharper)  messages due to strange naming conventions
[Serializable]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class Geometri {
	public string wkt;
	public int srid;
	public bool egengeometri;
}

[Serializable]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class Objekt {
	public int id;
	public string href;
	public Geometri geometri;
	public List<Egenskaper> egenskaper;
	public Relasjoner relasjoner;
	public List<GpsManager.GpsLocation> parsedLocation;
	public List<Objekt> plates;
}

[Serializable]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class Egenskaper {
	public int id;
	public string navn;
	public int datatype;
	public string datatype_tekst;
	public string verdi;
	public int enum_id;
	public Enhet enhet;
}

[Serializable]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class Enhet {
	public int id;
	public string navn;
	public string kortnavn;
}

[Serializable]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class Relasjoner {
	public List<Foreldre> foreldre;
}

[Serializable]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class Relasjon {
	public RelasjonType type;
	public List<int> vegobjekter;
}

[Serializable]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class RelasjonType {
	public int id;
	public string navn;
}

[Serializable]
public class Foreldre : Relasjon {
}

[Serializable]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class Neste {
	public string start;
	public string href;
}

[Serializable]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class Metadata {
	public int antall;
	public int returnert;
	public Neste neste;
}

[Serializable]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class RootObject {
	public List<Objekt> objekter;
	//public Metadata metadata;
}

[Serializable]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class RootSingleObject {
	public Objekt objekt;
}

[Serializable]
[SuppressMessage("ReSharper", "InconsistentNaming")]
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
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class RootObjectType {
	public List<ObjectType> vegobjekttyper;
}