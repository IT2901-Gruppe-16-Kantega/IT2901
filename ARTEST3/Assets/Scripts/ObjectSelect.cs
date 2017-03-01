using UnityEngine;

public class ObjectSelect : MonoBehaviour {

	//The GameObject that is pressed
	private GameObject target;

	// The layers to target
	LayerMask layers;

	private bool isMouseDrag;
	private Vector3 screenPosition;
	private Vector3 offset;


	// Use this for initialization
	void Start() {
		// The layer to target
		layers = LayerMask.GetMask("Signs");
	}

	// Update is called once per frame
	void Update() {
		if (Input.GetMouseButtonDown(0)) {
			RaycastHit hitInfo;
			target = ReturnClickedObject(out hitInfo);

			if (target != null) {
				isMouseDrag = true;
				Debug.Log("Old target position :" + target.transform.position);
				//Convert the targets world position to screen position.
				screenPosition = Camera.main.WorldToScreenPoint(target.transform.position);
				// Calculate the offset between the targets position and the mouse pointer / finger position. We use the x and y position of the mouse, but the z position of the target
				offset = target.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPosition.z));
			}
		}
		if (Input.GetMouseButtonUp(0)) {
			isMouseDrag = false;
			if (target != null) {
				RoadObjectManager rom = target.GetComponent<RoadObjectManager>();
				Debug.Log("New target position :" + target.transform.position);
				// Use the Reverse Haversine formula to update the latitude and longitude
				// The distance to the target is the magnitude of the targets position vector since we are at 0,0,0 we dont need to subtract it to get the direction
				double distance = target.transform.position.magnitude;
				//distance = System.Math.Round(distance, 10);
				// The bearing is the arcsin of the targets normalized x value plus PI / 2 (because 
				double bearing = Mathf.Asin(target.transform.position.x / (float)distance) + Mathf.PI / 2;

				rom.deltaDistance = distance - rom.distance;
				rom.deltaBearing = bearing - rom.bearing;
				rom.hasBeenMoved = rom.deltaDistance != 0 || rom.deltaBearing != 0;

				//Debug.Log(rom.roadObjectLocation);
				//Debug.Log("DISTANCE: " + distance + " | " + rom.distance + " | " + distance / rom.distance);
				//Debug.Log("BEARING:  " + bearing + " | " + rom.bearing + " | " + bearing / rom.bearing);
				//GPSManager.GPSLocation newLocation = GenerateObjects.ReverseHaversine(GenerateObjects.myLocation, distance, bearing, rom.roadObjectLocation);
				//GPSManager.GPSLocation oldLocation  = rom.roadObjectLocation;
				//Debug.Log(oldLocation);
				//Debug.Log(newLocation);
				//Debug.Log(newLocation.latitude / oldLocation.latitude + " | " + newLocation.longitude / oldLocation.longitude);
				//rom.roadObjectLocation = newLocation;
			}
		}
		if (isMouseDrag && target != null) {
			// Track the mouse pointer / finger position in the x and y axis, using the depth of the target
			Vector3 currentScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPosition.z);
			// Convert screen position to world position with offset changes.
			Vector3 currentPosition = Camera.main.ScreenToWorldPoint(currentScreenSpace) + offset;
			// Set the targets position to the mouse pointer / finger position, but switching the y axis with the z axis.
			target.transform.position = new Vector3(currentPosition.x, target.transform.position.y, currentPosition.z + currentPosition.y);
		}
	}

	GameObject ReturnClickedObject(out RaycastHit hit) {
		GameObject newTarget = null;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		if (Physics.Raycast(ray, out hit, float.PositiveInfinity, layers)) {
			Debug.DrawLine(ray.origin, hit.point);
			newTarget = hit.collider.gameObject;
			newTarget.GetComponent<RoadObjectManager>().Selected();
		}
		if (newTarget != target && target != null) {
			target.GetComponent<RoadObjectManager>().UnSelected();
		}
		Debug.Log(newTarget);
		return newTarget;
	}
}
