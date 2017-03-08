using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class GenerateObjects : MonoBehaviour {

	// Max seconds before fetching using default coords
	const int MAX_TIME_WAIT = 10;
	int time_waited = 0;

	// The list containing the locations of each road object
	List<Objekt> roadObjectList = new List<Objekt>();
	
	// The object to instantiate (create) when placing the road objects
    public GameObject signPost;
	public GameObject circleBlue;
	public GameObject circleRed;
	public GameObject triangleRed;

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
			GameObject signplateObject = Instantiate(GetGameObject(objekt), Vector3.zero, Quaternion.identity) as GameObject;

			//Instantiate(blueSign, Vector3.zero, Quaternion.identity) as GameObject;
			GetGameObject(objekt);
			//newGameObject.AddComponent<RoadObjectManager>();

			List<Vector3> coordinates = new List<Vector3>();

			for (var i = 0; i < objekt.parsedLocation.Count; i++) {

				Vector3 position = HelperFunctions.GetPositionFromCoords(objekt.parsedLocation[i]);

				// Set the parent of the new GameObject to be us (so we dont have a huge list in root)

                // Gets the signplate relations, and finds the parent with id 95 (signpost)
                Foreldre signpost = objekt.relasjoner.foreldre.Find(f => f.type.id == 95);
                GameObject signpostObject;

                // If sign plate has a parent signpost
                if(signpost != null) {

                    // Find the id of the first (and only signpost)
                    string skiltpunktID = signpost.vegobjekter[0].ToString();

                    Transform skiltpunktObjectTransform = SignsParent.transform.Find(skiltpunktID);

                    // Uses child count to place signs under each other
                    int childCount = 0;
                    if(skiltpunktObjectTransform == null) {
                        signpostObject = Instantiate(signPost, position, Quaternion.identity) as GameObject;
                        signpostObject.name = skiltpunktID;
                        signpostObject.transform.parent = SignsParent.transform;
                    }
                    else {
                        signpostObject = skiltpunktObjectTransform.gameObject;
                        childCount++;
                    }

                    signplateObject.transform.parent = signpostObject.transform;

                    RoadObjectManager rom = signplateObject.GetComponent<RoadObjectManager>();
                    rom.roadObjectLocation = objekt.parsedLocation[i];
                    rom.updateLocation();
                    rom.objekt = objekt;

                    rom.objectText.text = CreateRoadObjectText(objekt.egenskaper.Find(e => e.id == 5530).verdi);
                    rom.signpostRenderer = signplateObject.GetComponent<Renderer>();


                    if(objekt.parsedLocation.Count == 1) {
                        signplateObject.transform.position = position;
                        signplateObject.transform.Translate(0, 2 - (float)-childCount, 0);
                    }
                    else {
                        coordinates.Add(position);
                    }
                }
                else {
                    // DO SOMETHING FOR SIGNS WITHOUT PARENTS, THOSE POOR THINGS EXISTS :'(
                    Destroy(signplateObject);
                }
			}

			/*if (objekt.parsedLocation.Count > 1) {
                LineRenderer lineRenderer = signplateObject.AddComponent<LineRenderer>();
				lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
				lineRenderer.SetColors(Color.white, Color.white);
				lineRenderer.SetWidth(2, 2);
				lineRenderer.SetVertexCount(coordinates.Count);

				lineRenderer.SetPositions(coordinates.ToArray());
			}*/
		}
	}

    private string CreateRoadObjectText(string input) {
        int lineLength = 10;

        StringBuilder sb = new StringBuilder();

        int currentLineLength = 0;
        for (int i = 0; i < input.Length; i++) {
            currentLineLength++;

            if(currentLineLength >= lineLength && (input[i] == ' ' || input[i] == '-')) {
                sb.Append('\n');
                currentLineLength = 0;
            }

            sb.Append(input[i]);
        }
        return sb.ToString();
    }

	private GameObject GetGameObject(Objekt objekt) {
		var egenskap = objekt.egenskaper.Find(e => e.id == 5530);

		if (egenskap != null) {
			int signNumber;
			int.TryParse(egenskap.verdi.Substring(0, 1), out signNumber);

			if (signNumber == 1) {
                return circleRed;
			} else if (signNumber == 2) {
                return triangleRed;
			}
		}
        return circleBlue;
	}
}