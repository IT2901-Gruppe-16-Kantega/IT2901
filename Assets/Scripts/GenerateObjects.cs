using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateObjects : MonoBehaviour {

	// Max seconds before fetching using default coords
	private const int MaxTimeWait = 10;
	private int _timeWaited;

	// Our location. Default location is somewhere in the middle of Trondheim
	//public static GPSManager.GPSLocation myLocation = new GPSManager.GPSLocation(63.430626, 10.392145);
	//public static GPSManager.GPSLocation myLocation = new GPSManager.GPSLocation(63.417687, 10.404782);

	// The list containing the locations of each road object
	private List<Objekt> _roadObjectList = new List<Objekt>();

	// The object to instantiate (create) when placing the road objects
	public GameObject BlueSign;
	public GameObject RedSign;
	public GameObject RedTriangle;

	public GameObject SignsParent;

	// SerializeField makes the private field visible to the editor
	// Currently, no other GameObject needs the GPSManager, so this is fine
	// May want to make GPSManager static if all objects need access
	[SerializeField]
	private ApiWrapper _apiWrapper;

	[SerializeField]
	private RoadGenerator _roadGenerator;

	// Use this for initialization
	private void Start() {
		/*
		Get the GPSManager script atteched to this same object. Gives you an error if this gameobject does not have the GPSManager script attached
		so make sure that it has it attached
		*/
		_apiWrapper = GetComponent<ApiWrapper>();
		_roadGenerator = GetComponent<RoadGenerator>();

		// Update position
		//        myLocation = GPSManager.myLocation;

		StartCoroutine(FetchAfterLocationUpdated());
	}

	// Coroutine: FetchObjects
	// Checks if GPS have gotten the user's location, or until timeout
	// Then fetches the objects from NVDB using the user's location or the mock location
	private IEnumerator FetchAfterLocationUpdated() {
		// Wait until position has been updated, or until timeout before fetching
		while (_timeWaited < MaxTimeWait && !GpsManager.InitialPositionUpdated) {
			yield return new WaitForSeconds(1);
			_timeWaited++;
		}
		FetchObjects();
		_roadGenerator.FetchRoad();
	}

	private void FetchObjects() {
		// Second parameter is callback, initializing the object list and making the objects when the function is done.
        string localData = "{\"objekter\": " + LocalStorage.GetData("data.json") + "}";
		if (string.IsNullOrEmpty(localData)) {
			_apiWrapper.FetchObjects(96, GpsManager.MyLocation, objects => {
				Debug.Log("Returned " + objects.Count + " objects");
				_roadObjectList = objects;
				MakeObjects(_roadObjectList);

			});
		} else {
			// Parse the local data
			RootObject data = JsonUtility.FromJson<RootObject>(localData);

			// Go through each Objekter in the data.objekter (the road objects)
			foreach (Objekt obj in data.objekter) {
				// Add the location to our roadObjectList
				_roadObjectList.Add(_apiWrapper.ParseObject(obj));
			}
            Debug.Log(data.objekter.Count + " objekter");
			// Make the objects
			MakeObjects(_roadObjectList);
		}
	}

	// Uses the locations in roadObjectList and instantiates objects
	private void MakeObjects(List<Objekt> objects) {
		// For each location in the list

		foreach (Objekt objekt in objects) {

			// Instantiate a new GameObject on that location relative to us
			GameObject newGameObject = Instantiate(GetGameObject(objekt), Vector3.zero, Quaternion.identity) as GameObject;
			if (newGameObject == null)
				continue; // In case anything weird happens.

			GetGameObject(objekt);

			List<Vector3> coordinates = new List<Vector3>();

			foreach (GpsManager.GpsLocation location in objekt.parsedLocation) {
				Vector3 position = HelperFunctions.GetPositionFromCoords(location);

				// Set the parent of the new GameObject to be us (so we dont have a huge list in root)
				newGameObject.transform.parent = SignsParent.transform;
				RoadObjectManager rom = newGameObject.GetComponent<RoadObjectManager>();
				rom.RoadObjectLocation = location;
				rom.UpdateLocation();
				rom.Objekt = objekt;
				string[] parts = objekt.egenskaper.Find(egenskap => egenskap.id == 5530).verdi.Split(' ', '-');
				rom.SignText.text = "";
				string text = "";
				foreach (string s in parts) {
					rom.SignText.text += s + " ";
					if (rom.SignText.GetComponent<Renderer>().bounds.extents.x > .5) {
						rom.SignText.text = text.TrimEnd() + "\n" + s + " ";
					}
					text = rom.SignText.text;
				}

				if (objekt.parsedLocation.Count == 1) {
					newGameObject.transform.position = position;
				} else {
					coordinates.Add(position);
				}
			}

			if (objekt.parsedLocation.Count <= 1)
				continue; // To reduce nesting

			LineRenderer lineRenderer = newGameObject.AddComponent<LineRenderer>();
			lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
			lineRenderer.SetColors(Color.white, Color.white);
			lineRenderer.SetWidth(2, 2);
			lineRenderer.SetVertexCount(coordinates.Count);

			lineRenderer.SetPositions(coordinates.ToArray());
		}
	}

	private GameObject GetGameObject(Objekt objekt) {
		Egenskaper egenskap = objekt.egenskaper.Find(e => e.id == 5530);

		if (egenskap == null)
			return BlueSign;
		int signNumber;
		int.TryParse(egenskap.verdi.Substring(0, 1), out signNumber);

		switch (signNumber) {
			case 1:
				return RedSign;
			case 2:
				return RedTriangle;
			default:
				return BlueSign;
		}
	}
}
