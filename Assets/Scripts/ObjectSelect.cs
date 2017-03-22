using System;
using UnityEngine;

public class ObjectSelect : MonoBehaviour {

	//The GameObject that is pressed
	private GameObject _target;

	// The layers to target
	private LayerMask _layers;

	private bool _isMouseDrag;
	private Vector3 _screenPosition;
	private Vector3 _offset;
	private Vector3 _startingPoint;


	// Use this for initialization
	private void Start() {
		// The layer to target
		_layers = LayerMask.GetMask("Signs");
	}

	// Update is called once per frame
	private void Update() {
		if (Input.GetMouseButtonDown(0)) {
			RaycastHit hitInfo;
			_target = ReturnClickedObject(out hitInfo);
			if (_target != null) {
				_isMouseDrag = true;
				_startingPoint = Input.mousePosition;
				Debug.Log("Old target position :" + _target.transform.position);
				//Convert the targets world position to screen position.
				_screenPosition = Camera.main.WorldToScreenPoint(_target.transform.position);
				// Calculate the offset between the targets position and the mouse pointer / finger position. We use the x and y position of the mouse, but the z position of the target
				_offset = _target.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _screenPosition.z));
				Debug.Log(_offset.ToString());
			}
		}
		if (Input.GetMouseButtonUp(0)) {
			_isMouseDrag = false;
			if (_target != null) {
				RoadObjectManager rom = _target.GetComponent<RoadObjectManager>();
				Debug.Log("New target position :" + _target.transform.position);
				// Use the Reverse Haversine formula to update the latitude and longitude
				// The distance to the target is the magnitude of the targets position vector since we are at 0,0,0 we dont need to subtract it to get the direction
				double distance = _target.transform.position.magnitude;
				//distance = System.Math.Round(distance, 10);
				// The bearing is the arcsin of the targets normalized x value plus PI / 2 (because 
				double bearing = Mathf.Asin(_target.transform.position.x / (float) distance) + Mathf.PI / 2;

				rom.DeltaDistance = distance - rom.Distance;
				rom.DeltaBearing = bearing - rom.Bearing;
				rom.HasBeenMoved = Math.Abs(rom.DeltaDistance) > 0 || Math.Abs(rom.DeltaBearing) > 0;
			}
		}

		if (!_isMouseDrag || _target == null)
			return; // To reduce nesting

		// Track the mouse pointer / finger position in the x and y axis, using the depth of the target
		Vector3 currentScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, _screenPosition.z);

		// The distance from where you clicked to where it is now in pixels. Z axis can be ignored
		Vector3 screenPointOffset = currentScreenSpace - _startingPoint;
		float xDir, yDir;
		// Check the device orientation, and change Translate values.
		// Yes, it uses a magic number to divide. Sorry.
		// ReSharper disable once SwitchStatementMissingSomeCases
		switch (Input.deviceOrientation) {
			case DeviceOrientation.LandscapeLeft:
				xDir = screenPointOffset.y / 50f;
				yDir = -screenPointOffset.x / 75f;
				break;
			case DeviceOrientation.LandscapeRight:
				xDir = -screenPointOffset.y / 50f;
				yDir = screenPointOffset.x / 75f;
				break;
			case DeviceOrientation.PortraitUpsideDown:
				xDir = screenPointOffset.x / 75f;
				yDir = screenPointOffset.y / 50f;
				break;
			default:
				xDir = -screenPointOffset.x / 75f;
				yDir = -screenPointOffset.y / 50f;
				break;
		}


		_target.transform.Translate(xDir, 0, yDir);
		_startingPoint = Input.mousePosition;
	}

	private GameObject ReturnClickedObject(out RaycastHit hit) {
		GameObject newTarget = null;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		if (Physics.Raycast(ray, out hit, float.PositiveInfinity, _layers)) {
			Debug.DrawLine(ray.origin, hit.point);
			newTarget = hit.collider.gameObject;
			newTarget.GetComponent<RoadObjectManager>().Selected();
		}
		if (newTarget != _target && _target != null) {
			_target.GetComponent<RoadObjectManager>().UnSelected();
		}
		Debug.Log(newTarget);
		return newTarget;
	}
}
