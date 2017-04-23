using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
	private readonly Hashtable _signPosts = new Hashtable();

	// The object to instantiate (create) when placing the road objects
	public GameObject SignPost;
	public GameObject DefaultObject;
	public GameObject LineObject;

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
			int objectType = objekt.metadata.type.id;
			if (objectType == 96) {
				// If it's a signplate
				Foreldre foreldre = objekt.relasjoner.foreldre.Find(f => f.type.id == 95);
				// Check if its parent is in the hashmap already and add a plate to it
				if (foreldre != null && _signPosts.ContainsKey(foreldre.vegobjekter[0])) {
					GameObject signPost = _signPosts[foreldre.vegobjekter[0]] as GameObject;
					if (signPost)
						signPost.GetComponent<SignPlateAdder>().AddPlate(objekt);
				} else if (foreldre == null && _signPosts.ContainsKey(objekt.id)) {
					// Check if it doesnt have a sign point parent (95) and instantiate it and add it to the report
					GameObject signPost = _signPosts[objekt.id] as GameObject;
					if (signPost)
						signPost.GetComponent<SignPlateAdder>().AddPlate(objekt);
					objekt.metadata.notat = "Mangler forelder: skiltpunkt (95)";
					SharedData.Data.Add(objekt);
				} else {
					// add the parent to the hashmap and add a plate to it
					GameObject newSignPost =
						Instantiate(SignPost, HelperFunctions.GetPositionFromCoords(objekt.parsedLocation[0]), Quaternion.identity, SignsParent.transform) as GameObject;
					// Add signpost to hashmap using the signpoint id, otherwise use signplate id
					_signPosts.Add(foreldre != null ? foreldre.vegobjekter[0] : objekt.id, newSignPost);
					if (newSignPost) {
						if (foreldre != null) newSignPost.name = foreldre.vegobjekter[0].ToString();
						newSignPost.GetComponent<SignPlateAdder>().AddPlate(objekt);
					}
				}
			} else {
				// For everything else
				if (objekt.parsedLocation.Count == 1) {
					// if it has a single location
					GameObject newDefaultGameObject = Instantiate(DefaultObject, HelperFunctions.GetPositionFromCoords(objekt.parsedLocation[0]), Quaternion.identity, SignsParent.transform) as GameObject;
					if (newDefaultGameObject != null) {
						RoadObjectManager rom = newDefaultGameObject.GetComponent<RoadObjectManager>();
						rom.RoadObjectLocation = objekt.parsedLocation[0];
						rom.Objekt = objekt;
						rom.UpdateLocation();
						rom.SignText.text = objekt.metadata.type.navn;
					}
				} else {
					// if it has more (or zero)
					Vector3[] coordinates = new Vector3[objekt.parsedLocation.Count];
					for (int index = 0; index < objekt.parsedLocation.Count; index++) {
						GpsManager.GpsLocation location = objekt.parsedLocation[index];
						coordinates[index] = HelperFunctions.GetPositionFromCoords(location);
					}

					if (coordinates.Length == 0)
						continue;
					GameObject lineObject = Instantiate(LineObject, Vector3.zero, Quaternion.identity, SignsParent.transform) as GameObject;
					if (lineObject != null) {
						LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
						lineRenderer.SetVertexCount(coordinates.Length);
						lineRenderer.SetPositions(coordinates.ToArray());
					}
				}
			}
			if (i % 10 == 0)
				yield return new WaitForEndOfFrame();
		}
		IsCreatingSigns = false;
	}
}
