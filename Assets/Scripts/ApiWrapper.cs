﻿using System;
using System.Collections;
using System.Collections.Generic;
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

	/// <summary>
	///     Creates the header to send with the WWW request
	/// </summary>
	/// <returns>The created headers</returns>
	private static Dictionary<string, string> CreateHeaders() {
		Dictionary<string, string> headers = new Dictionary<string, string> {
			{
				"Accept", "application/vnd.vegvesen.nvdb-v2+json"
			}, {
				"X-Client", "Client"
			}, {
				"X-Kontaktperson", "Contact"
			}
		};
		return headers;
	}

	/// <summary>
	///     Creates a fetch request to the NVDB api
	/// </summary>
	/// <param name="id">The id of the road object</param>
	/// <param name="latitude">Your current latitude in srid 4326</param>
	/// <param name="longitude">Your current longitude in srid 4326</param>
	/// <returns>A WWW request to the NVDB api</returns>
	private static WWW CreateFetchRequest(int id, double latitude, double longitude) {
		// The query to fetch road signs
		string url = ApiUrl + "vegobjekter/" + id + "?inkluder=alle&pretty=true&srid=4326&kartutsnitt=" +
					(longitude - DeltaLong) + "," +
					(latitude - DeltaLat) + "," +
					(longitude + DeltaLong) + "," +
					(latitude + DeltaLat);
		Debug.Log(url);

		// Make a WWW (similar to fetch)       
		return new WWW(url, null, CreateHeaders());
	}

	/// <summary>
	///     Makes a request to the NVDB API and then executes the callback functions with the result
	/// </summary>
	/// <param name="id">The id of the road object</param>
	/// <param name="location">Your current GPS location in srid 4326</param>
	/// <param name="callback">A list of functions to run when the request is complete</param>
	public void FetchObjects(int id, GpsManager.GpsLocation location, Action<List<Objekter>> callback) {
		WWW www = CreateFetchRequest(id, location.Latitude, location.Longitude);

		// Start a coroutine that tries to get the data from the API
		StartCoroutine(WaitForRequest(www, id == 532, objects => {
			callback(objects);
			if (!string.IsNullOrEmpty(www.error))
				return;
			// If there is no error and the requested object isnt a road
			// Quick and dirty, only if the requested id is 532. Obviously should do this someplace else
			//if (id == 532)
			//	Debug.Log(LocalStorage.SaveData("roads.json", www.text) ? "FILE SAVED" : "The file failed to save.");
			//else
			//	Debug.Log(LocalStorage.SaveData("localRoadObjects.json", www.text) ? "FILE SAVED" : "The file failed to save.");
		}));
	}

	/// <summary>
	///     The coroutine that gets the data from the API
	/// </summary>
	/// <param name="www">The request URL with the correct headers</param>
	/// <param name="isRoads">If we request roads</param>
	/// <param name="callback">The functions to run when the request is done.</param>
	private IEnumerator WaitForRequest(WWW www, bool isRoads, Action<List<Objekter>> callback) {
		// Request data from the API and come back when it's done
		yield return www;

		// If it has an error, print out the error
		if (!string.IsNullOrEmpty(www.error)) {
			Debug.Log("WWW Error: \n" + www.error);
		} else {
			List<Objekter> roadObjectList = new List<Objekter>();

			// Else handle the data
			Debug.Log("WWW Ok!: \n" + www.text);

			// Make a new RootObject and parse the json data from the request
			NvdbObjekt data = JsonUtility.FromJson<NvdbObjekt>(www.text);
			if (!isRoads) {
				RoadSearchObject searchData = new RoadSearchObject {
					roadObjects = data.objekter
				};
				SharedData.AllData = searchData;
			}

			// Go through each Objekter in the data.objekter (the road objects)
			foreach (Objekter obj in data.objekter) {
				// Add the location to our roadObjectList
				Objekter objekt = ParseObject(obj);
				if (objekt == null)
					continue;
				roadObjectList.Add(objekt);
			}
			callback(roadObjectList);
		}
	}

	/// <summary>
	///     Parses the wkt from the object into a GpsManager.GpsLocation
	/// </summary>
	/// <param name="objekt">The object to parse</param>
	/// <returns>The object with a parsed wkt</returns>
	public Objekter ParseObject(Objekter objekt) {
		string wkt = objekt.geometri.wkt;
		if (string.IsNullOrEmpty(wkt))
			return null;
		// Make a substring of the contents between the parenthesis
		wkt = wkt.Substring(wkt.IndexOf("(", StringComparison.Ordinal) + 1)
			.Trim(')');
		if (wkt[0] == '(')
			wkt = wkt.Substring(wkt.IndexOf("(", StringComparison.Ordinal) + 1)
				.Trim(')');

		// Each triplets of coordinates have a comma in between
		string[] wktArray = wkt.Split(',');

		List<GpsManager.GpsLocation> coordinates = new List<GpsManager.GpsLocation>();
		foreach (string s in wktArray) {
			string[] sArray = s.Trim()
				.Split(' ');
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