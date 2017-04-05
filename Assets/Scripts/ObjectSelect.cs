using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Handles selection of objects
/// </summary>
public class ObjectSelect : MonoBehaviour {

	public Text ObjectText; // The text in the info box
	private GameObject _target; //The GameObject that is pressed
	private LayerMask _layers; // The layers to target
	private EventSystem _eventSystem;

	private bool _isDragging;
	private Vector3 _screenPosition;
	private Vector3 _startingPoint;
	private bool _isOverInfoBox;

	private void Start() {
		_layers = LayerMask.GetMask("Signs");
		_eventSystem = EventSystem.current;
	}

	private void Update() {
		if (Input.GetMouseButtonDown(0)) {
			if (_eventSystem.currentSelectedGameObject == null && !_isOverInfoBox) {
				_target = ReturnClickedObject();
				if (_target != null) {
					_isDragging = true;
					_startingPoint = Input.mousePosition;
					//Convert the targets world position to screen position.
					_screenPosition = Camera.main.WorldToScreenPoint(_target.transform.position);
				}
			}
		}
		if (Input.GetMouseButtonUp(0)) {
			_isDragging = false;
		}

		if (!_isDragging || _target == null)
			return; // To reduce nesting
		if (_target != null) {
			RoadObjectManager rom = _target.GetComponent<RoadObjectManager>();
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
		GetXandYOffsets(screenPointOffset, out xDir, out yDir);
		_target.transform.Translate(xDir, 0, yDir);
		_startingPoint = Input.mousePosition;
	}

	/// <summary>
	/// Checks the device orientation and changes the x and y Translate values
	/// </summary>
	/// <param name="screenPointOffset">The offset between the input location last frame and this frame</param>
	/// <param name="xDir">The output parameter for the X value</param>
	/// <param name="yDir">The output parameter for the Y value</param>
	private void GetXandYOffsets(Vector3 screenPointOffset, out float xDir, out float yDir) {
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
	}

	/// <summary>
	/// Casts a ray to the point on screen to get the GameObject
	/// </summary>
	/// <returns>Returns the Gameobject hit by the ray. Can return null if nothing was hit.</returns>
	private GameObject ReturnClickedObject() {
		GameObject newTarget = null;

		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		// If we hit something, set newTarget to that
		if (Physics.Raycast(ray, out hit, float.PositiveInfinity, _layers)) {
			newTarget = hit.collider.gameObject;
			newTarget.GetComponent<RoadObjectManager>().Selected();
		}
		// If new Target is different, change deselect previous target
		if (newTarget != _target && _target != null) {
			_target.GetComponent<RoadObjectManager>().UnSelected();
		}
		
		if (newTarget == null)
			ObjectText.text = "Ingenting valgt";
		return newTarget;
	}

	/// <summary>
	/// Resets the signs position
	/// </summary>
	public void ResetSignPosition() {
		if (_target != null)
			_target.GetComponent<RoadObjectManager>().ResetPosition();
	}

	/// <summary>
	/// If the mouse is over the info box
	/// </summary>
	public void EnterGameobject() {
		_isOverInfoBox = true;
	}

	/// <summary>
	/// If the mouse leaves the info box
	/// </summary>
	public void ExitGameobject() {
		_isOverInfoBox = false;
	}
}
