using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Handles selection of objects
/// </summary>
public class ObjectSelect : MonoBehaviour {

	public Text ObjectText; // The text in the info box
	private GameObject _targetPlate; //The GameObject that is pressed
	private GameObject _targetPole; // the plate's pole which we want to move
	private LayerMask _layers; // The layers to target
	private EventSystem _eventSystem;

	public static bool IsDragging;
	private Vector3 _screenPosition;
	private Vector3 _startingPoint;
	private bool _isOverInfoBox;

	private int _mouseClicks;
	private float _mouseTimer;
	private const float MouseTimerLimit = .6f;
	public static bool IsZoomed;
	private Vector3 _lastPosition;
	private Quaternion _lastRotation;
	private float _lastFov;

	[SerializeField] private Button _markTargetButton;
	[SerializeField] private Button _resetSignButton;

	private void Start() {
		_layers = LayerMask.GetMask("Signs");
		_eventSystem = EventSystem.current;
	}

	private void Update() {
		// Dobbel click detection
		int mouseClicks = CheckMouseDoubleClick();

		if (Input.GetMouseButtonDown(0)) {
			if (_eventSystem.currentSelectedGameObject == null && !_isOverInfoBox) {
				_targetPlate = ReturnClickedObject();
				if (_targetPlate != null) {
					_targetPole = _targetPlate.GetComponent<RoadObjectManager>().SignPost;
					IsDragging = true;
					_startingPoint = Input.mousePosition;
					//Convert the targets world position to screen position.
					_screenPosition = Camera.main.WorldToScreenPoint(_targetPlate.transform.position);
				}
			}
		}
		if (Input.GetMouseButtonUp(0)) {
			IsDragging = false;
		}

		_resetSignButton.interactable = _targetPlate;
		_markTargetButton.interactable = _targetPlate;

		if (!IsDragging || _targetPlate == null)
			return; // To reduce nesting

		// Deactivate gyro

		RoadObjectManager rom = _targetPlate.GetComponent<RoadObjectManager>();
		if (_targetPlate != null) {
			rom.HasBeenMoved = Math.Abs(rom.DeltaDistance) > 0;
			Egenskaper signNumber = rom.Objekt.egenskaper.Find(egenskap => egenskap.id == 5530);
			ObjectText.text =
				"ID: " + rom.Objekt.id + "\n" +
				"Egengeometri: " + rom.Objekt.geometri.egengeometri + "\n" +
				((signNumber == null) ? "" : "Skiltnummer: " + signNumber.verdi + "\n") +
				"Manuelt flyttet: " + rom.HasBeenMoved + "\n" +
				"Markert som feil: " + rom.SomethingIsWrong + "\n" +
				"Avstand flyttet: " + string.Format("{0:F2}m", rom.DeltaDistance) + "\n" +
				"Retning flyttet [N]: " + string.Format("{0:F2} grader", rom.DeltaBearing);

		}

		// Track the mouse pointer / finger position in the x and y axis, using the depth of the target
		Vector3 currentScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, _screenPosition.z);

		// The distance from where you clicked to where it is now in pixels. Z axis can be ignored
		Vector3 screenPointOffset = currentScreenSpace - _startingPoint;
		float xDir, yDir;
		GetXandYOffsets(screenPointOffset, out xDir, out yDir);

		_startingPoint = Input.mousePosition;
		if (!rom.Objekt.geometri.egengeometri) { // Only move if the object does not have egengeometri
			if (_targetPole != null) {
				_targetPole.transform.Translate(xDir, 0, yDir);
			} else {
				_targetPlate.transform.Translate(xDir, 0, yDir);
			}
		}
		_startingPoint = Input.mousePosition;

		if (mouseClicks != 2)
			return;
		if (IsZoomed) {
			Camera.main.transform.position = _lastPosition;
			Camera.main.transform.rotation = _lastRotation;
			Camera.main.fieldOfView = _lastFov;
		} else {
			_lastPosition = Camera.main.transform.position;
			_lastRotation = Camera.main.transform.rotation;
			_lastFov = Camera.main.fieldOfView;

			Camera.main.transform.LookAt(_targetPlate.transform.position);
			float distance = new Vector3(Camera.main.transform.position.x - _targetPlate.transform.position.x, 0, Camera.main.transform.position.z - _targetPlate.transform.position.z).magnitude;
			// Magic number
			Camera.main.fieldOfView = 2f * Mathf.Atan(5.2f / distance) * Mathf.Rad2Deg;
			Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, 1f, 90f);
		}
		IsZoomed = !IsZoomed;
	}

	/// <summary>
	/// Checks the device orientation and changes the x and y Translate values
	/// </summary>
	/// <param name="screenPointOffset">The offset between the input location last frame and this frame</param>
	/// <param name="xDir">The output parameter for the X value</param>
	/// <param name="yDir">The output parameter for the Y value</param>
	private static void GetXandYOffsets(Vector3 screenPointOffset, out float xDir, out float yDir) {
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
		if (newTarget != _targetPlate && _targetPlate != null) {
			_targetPlate.GetComponent<RoadObjectManager>().UnSelected();
		}
		if (newTarget == null)
			ObjectText.text = "Ingenting valgt";
		return newTarget;
	}

	/// <summary>
	/// Resets the signs position
	/// </summary>
	public void ResetSignPosition() {
		if (_targetPlate != null)
			_targetPlate.GetComponent<RoadObjectManager>().ResetPosition();
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

	/// <summary>
	/// UI OnClick function
	/// For Marking objects with wrong egengeometri
	/// </summary>
	public void MarkTarget() {
		if (_targetPlate == null)
			return;
		RoadObjectManager rom = _targetPlate.GetComponent<RoadObjectManager>();

		rom.SomethingIsWrong = !rom.SomethingIsWrong;
		if (_targetPlate != null) {
			_targetPole.GetComponent<SignPlateAdder>().MarkPlates(rom.SomethingIsWrong);
		}

		// Update objectText
		ObjectText.text =
				"ID: " + rom.Objekt.id + "\n" +
				"Egengeometri: " + rom.Objekt.geometri.egengeometri + "\n" +
				"Skiltnummer: " + rom.Objekt.egenskaper.Find(egenskap => egenskap.id == 5530).verdi + "\n" +
				"Manuelt flyttet: " + rom.HasBeenMoved + "\n" +
				"Markert som feil: " + rom.SomethingIsWrong + "\n" +
				"Avstand flyttet: " + string.Format("{0:F2}m", rom.DeltaDistance) + "\n" +
				"Retning flyttet [N]: " + string.Format("{0:F2} grader", rom.DeltaBearing);
	}

	/// <summary>
	/// Handles double click/tap zooming
	/// </summary>
	/// <returns>Returns the amount of clicks/taps up to 2</returns>
	private int CheckMouseDoubleClick() {
		if (Input.GetMouseButtonDown(0) && GUIUtility.hotControl == 0)
			_mouseClicks++;
		if (_mouseClicks < 1 || _mouseClicks >= 3)
			return 0;
		_mouseTimer += Time.fixedDeltaTime;

		if (_mouseClicks == 2) {
			_mouseTimer = 0;
			_mouseClicks = 0;
			return (_mouseTimer - MouseTimerLimit < 0) ? 2 : 1;
		}
		if (!(_mouseTimer > MouseTimerLimit))
			return 0;
		_mouseClicks = 0;
		_mouseTimer = 0;
		return 1;
	}
}
