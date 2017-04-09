using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenerateObjects : MonoBehaviour {

	// Max seconds before fetching using default coords
	private const int MaxTimeWait = 10;
	private int _timeWaited;

	// Our location. Default location is somewhere in the middle of Trondheim
	//public static GPSManager.GPSLocation myLocation = new GPSManager.GPSLocation(63.430626, 10.392145);
	//public static GPSManager.GPSLocation myLocation = new GPSManager.GPSLocation(63.417687, 10.404782);

	// The list containing the locations of each road object
	private List<Objekter> _roadObjectList = new List<Objekter>();

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
	private GenerateRoads _roadGenerator;

	[HideInInspector] 
	public static bool IsCreatingSigns = true;

	private bool _useLocalData; // true if RN data is NOT used (fetch objects from this app)

	private void Start() {
		_apiWrapper = GetComponent<ApiWrapper>();
		_roadGenerator = GetComponent<GenerateRoads>();
		_useLocalData = PlayerPrefs.GetInt("UseLocalData", 1) == 1;

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

	public Text DebugText; // TODO remove when done. Is only for debugging

	private void Update() {
		DebugText.text = 1 / Time.deltaTime + "";
	}

	/// <summary>
	/// Gets the objects either from the database or locally and instantiates the objects
	/// </summary>
	private void FetchObjects() {
		// Second parameter is callback, initializing the object list and making the objects when the function is done.
		string localData = LocalStorage.GetData("data.json");
		if (_useLocalData) {
			_apiWrapper.FetchObjects(96, GpsManager.MyLocation, objects => {
				Debug.Log("Returned " + objects.Count + " objects"); // TODO remove when done. Is only for debugging
				_roadObjectList = objects;
				UiScripts.ObjectsToInstantiate = objects.Count;
				StartCoroutine(MakeObjects(_roadObjectList));
			});
		} else {
			if (string.IsNullOrEmpty(localData)) {
				// TODO Do something if data loaded is not there. Query the user maybe?
			}
			// Parse the local data
			NvdbObjekt data = JsonUtility.FromJson<NvdbObjekt>(localData);
			UiScripts.ObjectsToInstantiate = data.objekter.Count;
			// Go through each Objekter in the data.objekter (the road objects)
			foreach (Objekter obj in data.objekter) {
				// Add the location to our roadObjectList
				Objekter objekt = _apiWrapper.ParseObject(obj);
				if (objekt == null) // Skip null objects
					continue;
				_roadObjectList.Add(objekt);
			}
			// Make the objects
			StartCoroutine(MakeObjects(_roadObjectList));
		}
	}

	// Uses the locations in roadObjectList and instantiates objects
	private IEnumerator MakeObjects(IList<Objekter> objects) {
		IsCreatingSigns = true;
		for (int i = 0; i < objects.Count; i++) {
			Objekter objekt = objects[i];
// Instantiate a new GameObject on that location relative to us
			GameObject newGameObject = Instantiate(GetGameObject(objekt), Vector3.zero, Quaternion.identity) as GameObject;
			if (newGameObject == null)
				continue; // In case anything weird happens.
			List<Vector3> coordinates = new List<Vector3>();
			RoadObjectManager rom = newGameObject.GetComponent<RoadObjectManager>();
			foreach (GpsManager.GpsLocation location in objekt.parsedLocation) {
				Vector3 position = HelperFunctions.GetPositionFromCoords(location);

				// Set the parent of the new GameObject to be us (so we dont have a huge list in root)
				newGameObject.transform.parent = SignsParent.transform;

				rom.RoadObjectLocation = location;
				rom.UpdateLocation();
				rom.Objekt = objekt;
				Egenskaper prop = objekt.egenskaper.Find(egenskap => egenskap.id == 5530); // 5530 is sign number
				if (prop == null) {
					// TODO HANDLING OBJECTS MISSING STUFF HAS BEEN MOVED TO REACT NATIVE. Keep this in case it is needed.
					//SharedData.Data.Add(objekt);
					objekt.metaData.notat = "Mangler egenskap 5530";
				}
				string[] parts = prop == null ? new[] {"MANGLER", "EGENSKAP", "5530"} : prop.verdi.Split(' ', '-');
				rom.SignText.text = "";

				// Try to fit the text inside the sign
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
					rom.OriginPoint = position;
				}
				else {
					coordinates.Add(position);
				}
			}
			if (objekt.geometri.egengeometri) rom.PoleRenderer.material = rom.Colors[1]; // Changed egengeo signs to green.

			UiScripts.ObjectsInstantiated++;
			if(i % 10 == 0) yield return new WaitForEndOfFrame(); // 10 objects per frame

			if (objekt.parsedLocation.Count <= 1)
				continue; // To reduce nesting
			LineRenderer lineRenderer = newGameObject.AddComponent<LineRenderer>();
			lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
			lineRenderer.SetColors(Color.white, Color.white);
			lineRenderer.SetWidth(2, 2);
			lineRenderer.SetVertexCount(coordinates.Count);

			lineRenderer.SetPositions(coordinates.ToArray());
		}
		IsCreatingSigns = false;
	}

	private GameObject GetGameObject(Objekter objekt) {
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
