using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class APIWrapper : MonoBehaviour {

	// The API base url
	const string API_URL = "https://www.vegvesen.no/nvdb/api/v2/";

	// We want numbers and periods, so this regex searches for the exact opposite to use as delimiters
	string regexPattern = @"[^0-9.]+";

	/* 
	The deviation between the latitude and longitude of your current location
	Used when querying the database to limit how many items we get. A bigger number
	means that you extend the area around you. Because of how coordinates work,
	to get a square, longitude needs to have a bigger delta.
	*/
	private float deltaLat = 0.002f;
	private float deltaLong = 0.003f;

	public Text debugText;

	private Dictionary<string, string> CreateHeaders() {
		Dictionary<string, string> headers = new Dictionary<string, string>();

		// Want it in JSON format
		headers.Add("Accept", "application/vnd.vegvesen.nvdb-v2+json");

		// To identify ourselves
		headers.Add("X-Client", "Client");
		headers.Add("X-Kontaktperson", "Contact");

		return headers;
	}

	// Use this to create and return a fetch request
	private WWW CreateFetchRequest(int id, double latitude, double longitude) {
		// The query to fetch road signs
        string url = API_URL + "vegobjekter/" + id + "?inkluder=geometri,egenskaper,relasjoner&srid=4326&kartutsnitt=" +
			(longitude - deltaLong) + "," +
			(latitude - deltaLat) + "," +
			(longitude + deltaLong) + "," +
			(latitude + deltaLat);
		Debug.Log(url);

		// Make a WWW (similar to fetch)       
		return new WWW(url, null, CreateHeaders());
	}

    private WWW CreateSingleFetchRequest(int id) {
        // The query to fetch road signs
        string url = API_URL + "vegobjekter/96/" + id + "?srid=4326&inkluder=alle";
        Debug.Log(url);

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

	public void FetchObjects(int id, GPSManager.GPSLocation location, Action<List<Objekt>> callback) {
		WWW www = CreateFetchRequest(id, location.latitude, location.longitude);

		// Start a coroutine that tries to get the data from the API
		StartCoroutine(WaitForRequest(www, objects => {
			callback(objects);
		}));
	}

    public void FetchObject(int id, Action<Objekt> callback) {
        WWW www = CreateSingleFetchRequest(id);

        StartCoroutine(WaitForSingleRequest(www, obj => {
            callback(obj);
        }));
    }

    IEnumerator WaitForSingleRequest(WWW www, Action<Objekt> callback) {
        yield return www;

        if(!string.IsNullOrEmpty(www.error)) {
            Debug.Log("WWW Error: " + www.error);
        }
        else {
            Objekt data = JsonUtility.FromJson<Objekt>(www.text);
            callback(data);
        }
    }

	// The coroutine that gets the data from the API
	// Parameters:
	//      WWW www -> The request URL with the correct headers
	IEnumerator WaitForRequest(WWW www, Action<List<Objekt>> callback) {
		// Request data from the API and come back when it's done
		yield return www;

		// If it has an error, print out the error
		if (!string.IsNullOrEmpty(www.error)) {
			Debug.Log("WWW Error: " + www.error);
		} else {
			List<Objekt> roadObjectList = new List<Objekt>();

			// Else handle the data
			// For debugging purposes
			//Debug.Log(roadObjectList.Count);
			Debug.Log("WWW Ok!: " + www.text);

			// Make a new RootObject and parse the json data from the request
			RootObject data = JsonUtility.FromJson<RootObject>(www.text);

			// Go through each Objekter in the data.objekter (the road objects)
			foreach (Objekt obj in data.objekter) {
                // For debugging purposes
                //Debug.Log(oLocation.latitude + " - " + oLocation.longitude + " - " + oLocation.altitude);
                //Debug.Log(oLocation.ToString());

                // Add the location to our roadObjectList
                roadObjectList.Add(ParseObject(obj));
			}
			callback(roadObjectList);

			// For debuggin purposes
			//			Debug.Log(roadObjectList.Count);
			//debugText.text = roadObjectList.Count + " objects";
		}
	}

    private Objekt ParseObject(Objekt objekt) {
        // For debugging purposes
        //Debug.Log(obj.geometri.wkt);

        string wkt = objekt.geometri.wkt;
        wkt = wkt.Substring(wkt.IndexOf("(") + 1).Trim(')');

        //[63.429624610409434, 10.393547899740911, 10.9]
        string[] wktArray = wkt.Split(',');

        List<GPSManager.GPSLocation> coordinates = new List<GPSManager.GPSLocation>();
        foreach(string s in wktArray) {
            string[] sArray = s.Trim().Split(' ');
            double latitude = double.Parse(sArray[0]);
            double longitude = double.Parse(sArray[1]);
            if(sArray.Length == 2) {
                coordinates.Add(new GPSManager.GPSLocation(latitude, longitude));
            }
            else {
                double altitude = double.Parse(sArray[2]);
                coordinates.Add(new GPSManager.GPSLocation(latitude, longitude, altitude));
            }
        }
        objekt.parsedLocation = coordinates;
        return objekt;
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
    public List<Egenskaper> egenskaper;
    public Relasjoner relasjoner;

    public List<GPSManager.GPSLocation> parsedLocation;
    public List<Objekt> plates;
}

[Serializable]
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
public class Enhet {
    public int id;
    public string navn;
    public string kortnavn;
}

[Serializable]
public class Relasjoner {
    public List<Barn> barn;
    public List<Foreldre> foreldre;
}

[Serializable]
public class Relasjon {
    public RelasjonType type;
    public List<int> vegobjekter;
}

[Serializable]
public class RelasjonType {
    public int id;
    public string navn;
}

[Serializable]
public class Barn: Relasjon {}

[Serializable]
public class Foreldre: Relasjon {}

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
public class RootSingleObject {
    public Objekt objekt;
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