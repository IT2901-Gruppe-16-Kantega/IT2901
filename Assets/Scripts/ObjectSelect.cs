using System;
using UnityEngine;
using UnityEngine.UI;

public class ObjectSelect : MonoBehaviour {

	//The GameObject that is pressed
	private GameObject _target;
	private GameObject _previousTarget;

	// The layers to target
	private LayerMask _layers;

	private bool _isMouseDrag;
	private Vector3 _screenPosition;
	private Vector3 _startingPoint;

	public Text ObjectText;


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
				//Convert the targets world position to screen position.
				_screenPosition = Camera.main.WorldToScreenPoint(_target.transform.position);
			}
		}
		if (Input.GetMouseButtonUp(0)) {
			_isMouseDrag = false;
		}

		if (!_isMouseDrag || _target == null)
			return; // To reduce nesting
		if (_target != null) {
			RoadObjectManager rom = _target.GetComponent<RoadObjectManager>();
			// The distance to the target is the magnitude of the targets position vector since we are at 0,0,0 we dont need to subtract it to get the direction
			double distance = _target.transform.position.magnitude;
			// The bearing is the arcsin of the targets normalized x value plus PI / 2 (because 
			double bearing = Mathf.Asin(_target.transform.position.x / (float) distance) + Mathf.PI / 2;

			rom.DeltaDistance = distance - rom.Distance;
			rom.DeltaBearing = bearing - rom.Bearing;
			rom.HasBeenMoved = Math.Abs(rom.DeltaDistance) > 0 || Math.Abs(rom.DeltaBearing) > 0;
			ObjectText.text =
				"id: " + rom.Objekt.id + "\n" +
				"egengeo: " + rom.Objekt.geometri.egengeometri + "\n" +
				"Skiltnummer: " + rom.Objekt.egenskaper.Find(egenskap => egenskap.id == 5530).verdi + "\n" +
				"manuelt flyttet: " + rom.HasBeenMoved + "\n" +
				"avstand flyttet: " + String.Format("{0:F2}m", rom.Distance);
		}


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
		_previousTarget = _target;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out hit, float.PositiveInfinity, _layers)) {
			newTarget = hit.collider.gameObject;
			RoadObjectManager rom = newTarget.GetComponent<RoadObjectManager>();
			rom.Selected();
		}
		if (newTarget != _target && _target != null) {
			_target.GetComponent<RoadObjectManager>().UnSelected();
		}
		if (newTarget == null) ObjectText.text = "";
		return newTarget;
	}

	private void OnGUI() {
		float buttonWidth = Screen.width / 3f;
		float buttonHeight = Screen.height / 10f;
		if (GUI.Button(new Rect(buttonWidth/20f, Screen.height - 4/3f*buttonHeight, buttonWidth, buttonHeight), "Reset posisjon") && _previousTarget != null) { // _previousTarget because we click and that updates the target. This will most likely cause a bug, so be check this if something is wrong.
			_previousTarget.GetComponent<RoadObjectManager>().ResetPosition();
			_target = _previousTarget;
			_target.GetComponent<RoadObjectManager>().Selected();
			ObjectText.text =
				"id: " + _previousTarget.GetComponent<RoadObjectManager>().Objekt.id + "\n" +
				"egengeo: " + _previousTarget.GetComponent<RoadObjectManager>().Objekt.geometri.egengeometri + "\n" +
				"Skiltnummer: " + _previousTarget.GetComponent<RoadObjectManager>().Objekt.egenskaper.Find(egenskap => egenskap.id == 5530).verdi + "\n" +
				"manuelt flyttet: " + _previousTarget.GetComponent<RoadObjectManager>().HasBeenMoved + "\n" +
				"avstand flyttet: " + String.Format("{0:F2}m", _previousTarget.GetComponent<RoadObjectManager>().Distance);
		}
	}
}
