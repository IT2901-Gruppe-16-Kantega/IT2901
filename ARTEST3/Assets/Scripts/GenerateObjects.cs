using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateObjects : MonoBehaviour {

	// Max seconds before fetching using default coords
	const int MAX_TIME_WAIT = 10;
	int time_waited = 0;

	// Our location. Default location is somewhere in the middle of Trondheim
	//public static GPSManager.GPSLocation myLocation = new GPSManager.GPSLocation(63.430626, 10.392145);
	//public static GPSManager.GPSLocation myLocation = new GPSManager.GPSLocation(63.417687, 10.404782);

	// The list containing the locations of each road object
	List<Objekt> roadObjectList = new List<Objekt>();
	List<Objekt> road = new List<Objekt>();

	// The object to instantiate (create) when placing the road objects
	public GameObject blueSign;
	public GameObject redSign;
	public GameObject redTriangle;

	public GameObject SignsParent;

	// SerializeField makes the private field visible to the editor
	// Currently, no other GameObject needs the GPSManager, so this is fine
	// May want to make GPSManager static if all objects need access
	[SerializeField]
	private APIWrapper apiWrapper;

	[SerializeField]
	private RoadGenerator roadGenerator;

	// Use this for initialization
	void Start() {
		/*
		Get the GPSManager script atteched to this same object. Gives you an error if this gameobject does not have the GPSManager script attached
		so make sure that it has it attached
		*/
		apiWrapper = GetComponent<APIWrapper>();
		roadGenerator = GetComponent<RoadGenerator>();

		// Update position
		//        myLocation = GPSManager.myLocation;

		StartCoroutine(FetchAfterLocationUpdated());
	}

	// Coroutine: FetchObjects
	// Checks if GPS have gotten the user's location, or until timeout
	// Then fetches the objects from NVDB using the user's location or the mock location
	IEnumerator FetchAfterLocationUpdated() {
		// Wait until position has been updated, or until timeout before fetching
		while (time_waited < MAX_TIME_WAIT && !GPSManager.initialPositionUpdated) {
			yield return new WaitForSeconds(1);
			time_waited++;
		}
		FetchObjects();
		roadGenerator.FetchRoad();
	}

	private void FetchObjects() {
		// Second parameter is callback, initializing the object list and making the objects when the function is done.
		apiWrapper.FetchObjects(96, GPSManager.myLocation, objects => {
			Debug.Log("Returned " + objects.Count + " objects");
			this.roadObjectList = objects;
			this.MakeObjects(this.roadObjectList);

			/*foreach(Objekt o in objects) {
                foreach(Barn barn in o.relasjoner.barn) {
                    if(barn.type.id == 96) {
                        foreach(int id in barn.vegobjekter) {
                            apiWrapper.FetchObject(id, obj => {
                                o.plates.Add(obj);
                            });
                        }
                    }
                }
            }*/
		});
	}

	// Uses the locations in roadObjectList and instantiates objects
	void MakeObjects(List<Objekt> objects) {
		// For each location in the list

		foreach (Objekt objekt in objects) {

			// Instantiate a new GameObject on that location relative to us
			GameObject newGameObject = Instantiate(GetGameObject(objekt), Vector3.zero, Quaternion.identity) as GameObject;

			//Instantiate(blueSign, Vector3.zero, Quaternion.identity) as GameObject;
			GetGameObject(objekt);
			//newGameObject.AddComponent<RoadObjectManager>();

			List<Vector3> coordinates = new List<Vector3>();

			for (var i = 0; i < objekt.parsedLocation.Count; i++) {

				Vector3 position = HelperFunctions.GetPositionFromCoords(objekt.parsedLocation[i]);

				// Set the parent of the new GameObject to be us (so we dont have a huge list in root)
				newGameObject.transform.parent = SignsParent.transform;
				newGameObject.GetComponent<RoadObjectManager>().roadObjectLocation = objekt.parsedLocation[i];
				newGameObject.GetComponent<RoadObjectManager>().updateLocation();
				newGameObject.GetComponent<RoadObjectManager>().objekt = objekt;

				if (objekt.parsedLocation.Count == 1) {
					newGameObject.transform.position = position;
				} else {
					coordinates.Add(position);
				}
			}

			if (objekt.parsedLocation.Count > 1) {
				LineRenderer lineRenderer = newGameObject.AddComponent<LineRenderer>();
				lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
				lineRenderer.SetColors(Color.white, Color.white);
				lineRenderer.SetWidth(2, 2);
				lineRenderer.SetVertexCount(coordinates.Count);

				lineRenderer.SetPositions(coordinates.ToArray());
			}
		}
	}

	private GameObject GetGameObject(Objekt objekt) {
		var egenskap = objekt.egenskaper.Find(e => e.id == 5530);

		if (egenskap != null) {
			int signNumber;
			int.TryParse(egenskap.verdi.Substring(0, 1), out signNumber);

			if (signNumber == 1) {
				return redSign;
			} else if (signNumber == 2) {
				return redTriangle;
			}
		}
		return blueSign;
	}
}
