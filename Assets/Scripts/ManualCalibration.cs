using UnityEngine;

public class ManualCalibration : MonoBehaviour {
	public GameObject User;

	public float PerspectiveZoomSpeed = 0.5f;        // The rate of change of the field of view in perspective mode.

	private const float RotationDampening = 0.8f; // To dampen the rotation
	private float _totalRotation;
	private const float RotationThreshold = 20;
	private bool _isRotating;
	private const float RotationLimit = 90f;

	private float _cameraRotationOffset;
	private const float PinchDampening = 0.1f; // To dampen the pinching
	private float _totalPinch;
	private const float PinchThreshold = 150;
	private bool _isZooming;
	private const float MinFov = 1f;
	private const float MaxFov = 90f;

	[SerializeField] private GyroscopeCamera _gyroCam;

	private void Start() {
		_gyroCam = GetComponent<GyroscopeCamera>();
	}


	private void Update() {
		// If we dont have two fingers on the screen, reset total variables and set everything to false.
		if (Input.touchCount != 2) {
			_totalRotation = 0;
			_totalPinch = 0;
			_gyroCam.Calibrating = false;
			_isRotating = false;
			_isZooming = false;
		} else {
			// If we have two fingers on the screen, calibrate
			_gyroCam.Calibrating = true;
		}

		// Reset total values and return
		if (!_gyroCam.Calibrating) {
			_totalRotation = 0;
			_totalPinch = 0;
			return;
		}

		// Calculate angle and pinch
		DetectTouchMovement.Calculate();

		_totalPinch += Mathf.Abs(DetectTouchMovement.PinchDistanceDelta);
		// If _totalPinch crosses the threshold and we arent rotating, zoom
		if (_totalPinch >= PinchThreshold && !_isRotating) { //Zoom
			Debug.Log("Zooming");
			_isZooming = true;
			float pinchAmount = DetectTouchMovement.PinchDistanceDelta * PinchDampening;
			Camera.main.fieldOfView -= pinchAmount;
			Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, MinFov, MaxFov);
		}

		_totalRotation += Mathf.Abs(DetectTouchMovement.TurnAngleDelta);
		if (_totalRotation >= RotationThreshold && !_isZooming) {
			Debug.Log("Rotating");
			// Rotate
			_isRotating = true;
			float rotationAmount = DetectTouchMovement.TurnAngleDelta * RotationDampening;
			if (_gyroCam.IsCarMode) {
				transform.RotateAround(User.transform.position, Vector3.up, rotationAmount);
			} else {
				transform.Rotate(0, rotationAmount, 0, Space.World);
				_cameraRotationOffset += rotationAmount;
			}
		}
		// Force rotation to be between -90 and 90
		_cameraRotationOffset = Mathf.Clamp(_cameraRotationOffset, -RotationLimit, RotationLimit);
		// Store rotation to _gyroCam
		_gyroCam.Rotation = _cameraRotationOffset;
	}
}
