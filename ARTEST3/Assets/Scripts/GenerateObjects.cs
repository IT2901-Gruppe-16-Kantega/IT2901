using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateObjects : MonoBehaviour {

	// The earth's mean radius in meters
	//const double EARTH_MEAN_RADIUS = 6371001;
	const double EARTH_MEAN_RADIUS = 6361181;

	// Max seconds before fetching using default coords
	const int MAX_TIME_WAIT = 1;
	int time_waited = 0;

	// Our location. Default location is somewhere in the middle of Trondheim
	//public static GPSManager.GPSLocation myLocation = new GPSManager.GPSLocation(63.430626, 10.392145);
    public static GPSManager.GPSLocation myLocation = new GPSManager.GPSLocation(63.417687, 10.404782);

	// The list containing the locations of each road object
	List<Objekt> roadObjectList = new List<Objekt>();
    List<Objekt> road = new List<Objekt>();
	
	// The object to instantiate (create) when placing the road objects
	public GameObject blueSign;
	public GameObject redSign;
	public GameObject redTriangle;

	// SerializeField makes the private field visible to the editor
	// Currently, no other GameObject needs the GPSManager, so this is fine
	// May want to make GPSManager static if all objects need access
	[SerializeField]
	private APIWrapper apiWrapper;

	// Use this for initialization
	void Start() {
		/*
		Get the GPSManager script atteched to this same object. Gives you an error if this gameobject does not have the GPSManager script attached
		so make sure that it has it attached
		*/
		apiWrapper = GetComponent<APIWrapper>();

        // Update position
        myLocation = GPSManager.myLocation;
		
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
        FetchRoad();
	}

    private void FetchRoad() {
        apiWrapper.FetchObjects(532, myLocation, objects => {
            Debug.Log("Returned " + objects.Count + " objects");
            this.road = objects;
            this.MakeObjects(this.road);
        });
    }
        
	private void FetchObjects() {
        // Second parameter is callback, initializing the object list and making the objects when the function is done.
		apiWrapper.FetchObjects(96, myLocation, objects => {
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

	// The haversine formula calculates the distance between two gps locations by air (ignoring altitude).
	// Parameters:
	//		GPSLocation startLocation	-> The location where we are
	//		GPSLocation endLocation		-> The location where the object is
	// Returns the distance between the startLocation and endLocation in meters (1 Unit = 1 meter for simplicity)
	public static double Haversine(GPSManager.GPSLocation startLocation, GPSManager.GPSLocation endLocation) {
		double dLat = (endLocation.latitude - startLocation.latitude) * System.Math.PI / 180;
		double dLon = (endLocation.longitude - startLocation.longitude) * System.Math.PI / 180;
		startLocation.latitude *= System.Math.PI / 180;
		endLocation.latitude *= System.Math.PI / 180;
		// a = Sin(dLat/2)^2 + Sin(dLon/2)^2 * Cos(sLat) * Cos(eLat)
		double a = System.Math.Pow(System.Math.Sin(dLat / 2), 2)
				+ System.Math.Pow(System.Math.Sin(dLon / 2), 2)
				* System.Math.Cos(startLocation.latitude)
				* System.Math.Cos(endLocation.latitude);
		double c = 2 * System.Math.Asin(System.Math.Sqrt(a));
		double d = EARTH_MEAN_RADIUS * 2 * c;
		return d;
	}




	// PUTTER DENNE I HOLD. NØYAKTIGHETEN VI TRENGER ER IKKE GOD NOK. SKAL FINNE PÅ EN BEDRE MÅTE.
	// The reverse haversine formula calculates the latitude and longitude from the distance and bearing relative to another latitude and longitude
	// Parameters:
	//		GPSLocation startLocation	-> The location where we are
	//		float distance				-> the distance from the startLocation in meters
	//		float bearing				-> the angle from startLocation in radians
	//		out GPSLocation newLocation -> the output parameter with the calculated latitude and longitude
	public static GPSManager.GPSLocation ReverseHaversine(GPSManager.GPSLocation startLocation, double distance, double bearing, GPSManager.GPSLocation oldLocation) {
		GPSManager.GPSLocation newLocation;
		double eLat, eLon, sLat, sLon;
		//Debug.Log("DISTANCE: " + distance);
		//Debug.Log("BEARING: " + bearing);
		sLat = startLocation.latitude * System.Math.PI / 180;
		sLon = startLocation.longitude * System.Math.PI / 180;
		eLat = (System.Math.Asin(
				System.Math.Sin(sLat)
				* System.Math.Cos(distance / EARTH_MEAN_RADIUS)
				+ System.Math.Cos(sLat)
				* System.Math.Sin(distance / EARTH_MEAN_RADIUS)
				* System.Math.Cos(bearing)
				)) * 180 / System.Math.PI;
		eLon = (sLon +
				System.Math.Atan2(
					System.Math.Sin(bearing)
					* System.Math.Sin(distance / EARTH_MEAN_RADIUS)
					* System.Math.Cos(sLat),
					System.Math.Cos(distance / EARTH_MEAN_RADIUS)
					- System.Math.Sin(sLat)
					* System.Math.Sin(eLat)
				)) * 180 / System.Math.PI;
		//Debug.Log("LOOK AT MEEEEEEEEE");
		//Debug.Log(sLat * 180 / System.Math.PI + " | " + sLon * 180 / System.Math.PI);
		//Debug.Log(eLat + " | " + eLon);
		//Debug.Log("SOPTOPASD ASD");
		newLocation = new GPSManager.GPSLocation(eLat, eLon, oldLocation.altitude);
		return newLocation;
	}

	// The formula that calculates the bearing when travelling from startLocation to endLocation
	// Parameters:
	//		GPSLocation startLocation -> The location where we are
	//		GPSLocation endLocation		-> The location where the object is
	// Returns the bearing from startLocation to endLocation in radians
	public static double CalculateBearing(GPSManager.GPSLocation startLocation, GPSManager.GPSLocation endLocation) {
		double x = System.Math.Cos(startLocation.latitude * System.Math.PI / 180)
				* System.Math.Sin(endLocation.latitude * System.Math.PI / 180)
				- System.Math.Sin(startLocation.latitude * System.Math.PI / 180)
				* System.Math.Cos(endLocation.latitude * System.Math.PI / 180)
				* System.Math.Cos((endLocation.longitude - startLocation.longitude) * System.Math.PI / 180);
		double y = System.Math.Sin((endLocation.longitude - startLocation.longitude) * System.Math.PI / 180)
				* System.Math.Cos(endLocation.latitude * System.Math.PI / 180);
		return System.Math.Atan2(y, x) + System.Math.PI / 2;
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

            for(var i = 0; i < objekt.parsedLocation.Count; i++) {
                
                // Create a new position where x, y, and z is 0
                Vector3 position = Vector3.zero;

                // Calculate the distance and bearing between us and the location
                double distance = Haversine(myLocation, objekt.parsedLocation[i]);
                double bearing = CalculateBearing(myLocation, objekt.parsedLocation[i]);

                //Debug.Log("Distance: " + distance + "\nBearing: " + bearing);
                // calculate the x and z offset between us and the location and update the x and z position
                position.x = (float)(-System.Math.Cos(bearing) * distance);
                position.z = (float)(System.Math.Sin(bearing) * distance);

                // Set the parent of the new GameObject to be us (so we dont have a huge list in root)
                newGameObject.transform.parent = gameObject.transform;
                newGameObject.GetComponent<RoadObjectManager>().roadObjectLocation = objekt.parsedLocation[i];
                newGameObject.GetComponent<RoadObjectManager>().updateLocation();
                newGameObject.GetComponent<RoadObjectManager>().objekt = objekt;

                if(objekt.parsedLocation.Count == 1) {
                    newGameObject.transform.position = position;
                }
                else {
                    coordinates.Add(position);
                }
            }

            if (objekt.parsedLocation.Count > 1) {
                LineRenderer lineRenderer = newGameObject.AddComponent<LineRenderer>();
                lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
                lineRenderer.SetColors(Color.white, Color.white);
                lineRenderer.SetWidth(10, 10);
                lineRenderer.SetVertexCount(coordinates.Count);

                lineRenderer.SetPositions(coordinates.ToArray());
            }
		}
	}

    private GameObject GetGameObject(Objekt objekt) {
        var egenskap = objekt.egenskaper.Find(e => e.id == 5530);

        if(egenskap != null) {
            int signNumber;
            int.TryParse(egenskap.verdi.Substring(0, 1), out signNumber);

            if(signNumber == 1) {
                return redSign;
            }
            else if(signNumber == 2) {
                return redTriangle;
            }
        }
        return blueSign;
    }
}