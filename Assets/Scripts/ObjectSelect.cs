using System;
using UnityEngine;
using UnityEngine.UI;

public class ObjectSelect : MonoBehaviour {
	public Text ObjectText;
	
	//The GameObject that is pressed
	private GameObject _target;
	private GameObject _previousTarget;

	// The layers to target
	private LayerMask _layers;

	private bool _isMouseDrag;
	private Vector3 _screenPosition;
	private Vector3 _startingPoint;


	private void Start() {
		// The layer to target
		_layers = LayerMask.GetMask("Signs");
	}

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
        RoadObjectManager rom = _target.GetComponent<RoadObjectManager>();
        if (_target != null) {
			// TODO commented out distance, bearing, DeltaDistance and DeltaBearing because they are calculated in RoadObjectManager instead.
			// The distance to the target is the magnitude of the targets position vector since we are at 0,0,0 we dont need to subtract it to get the direction
			//double distance = _target.transform.position.magnitude;
			// The bearing is the arcsin of the targets normalized x value plus PI / 2 (because 
			//double bearing = Mathf.Asin(_target.transform.position.x / (float) distance) + Mathf.PI / 2;

			//rom.DeltaDistance = distance - rom.Distance;
			//rom.DeltaBearing = bearing - rom.Bearing;
			rom.HasBeenMoved = Math.Abs(rom.DeltaDistance) > 0;
			ObjectText.text =
				"id: " + rom.Objekt.id + "\n" +
				"egengeo: " + rom.Objekt.geometri.egengeometri + "\n" +
				"Skiltnummer: " + rom.Objekt.egenskaper.Find(egenskap => egenskap.id == 5530).verdi + "\n" +
				"manuelt flyttet: " + rom.HasBeenMoved + "\n" +
				"avstand flyttet: " + string.Format("{0:F2}m", rom.DeltaDistance) + "\n" +
				"retning flyttet [N]: " + string.Format("{0:F2} grader", rom.DeltaBearing);
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


        if (!rom.Objekt.geometri.egengeometri) // Dont move egengeo objects - Vegard
        {
            _target.transform.Translate(xDir, 0, yDir);
        }
		_startingPoint = Input.mousePosition;
	}

	private GameObject ReturnClickedObject(out RaycastHit hit) {
		GameObject newTarget = null;
		_previousTarget = _target;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		// TODO Check if UI was clicked instead. Look at EventSystems
		if (Physics.Raycast(ray, out hit, float.PositiveInfinity, _layers)) {
			newTarget = hit.collider.gameObject;
			newTarget.GetComponent<RoadObjectManager>().Selected();
		}
		if (newTarget != _target && _target != null) {
			_target.GetComponent<RoadObjectManager>().UnSelected();
		}
		if (newTarget == null)
			ObjectText.text = "Ingenting valgt";
		return newTarget;
	}

	public void ResetSignPosition() {
		// _previousTarget because we click and that updates the target. This will most likely cause a bug, so check this if something is wrong.
		// TODO click ui without calling ReturnClickedObject
		if (_previousTarget == null) return;
		RoadObjectManager prevRom = _previousTarget.GetComponent<RoadObjectManager>();
		prevRom.ResetPosition();
		_target = _previousTarget;
		_target.GetComponent<RoadObjectManager>().Selected();
		ObjectText.text =
			"id: " + prevRom.Objekt.id + "\n" +
			"egengeo: " + prevRom.Objekt.geometri.egengeometri + "\n" +
			"Skiltnummer: " + prevRom.Objekt.egenskaper.Find(egenskap => egenskap.id == 5530).verdi + "\n" +
			"manuelt flyttet: " + prevRom.HasBeenMoved + "\n" +
			"avstand flyttet: " + string.Format("{0:F2} m", prevRom.DeltaDistance) + "\n" +
			"retning flyttet [N]: " + string.Format("{0:F2} grader", prevRom.DeltaBearing);
	}
}
