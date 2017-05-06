using UnityEngine;

public class ManualCalibration : MonoBehaviour {
	private const float RotationLimit = 90f;
	private const float MinFov = 1f;
	private const float MaxFov = 90f;

	public static float RotationSensitivity = 0.8f; // To dampen the rotation. Higher means more rotation
	public static float RotationThreshold = 4f;
	public static float ZoomSensitivity = 0.3f; // To dampen the pinching. Higher means more zooming
	public static float ZoomThreshold = 50;
	public static bool DisableCalibration;

	private float _cameraRotationOffset;

	[SerializeField]
	private GyroscopeCamera _gyroCam;

	private bool _isRotating;
	private bool _isZooming;
	private float _totalRotation;
	private float _totalZoom;

	public float PerspectiveZoomSpeed = 0.5f; // The rate of change of the field of view in perspective mode.
	public GameObject User;

	private void Start() {
		_gyroCam = GetComponent<GyroscopeCamera>();
		ZoomSensitivity = PlayerPrefs.GetFloat("ZoomSens", ZoomSensitivity);
		ZoomThreshold = PlayerPrefs.GetFloat("ZoomThreshold", ZoomThreshold);
		RotationSensitivity = PlayerPrefs.GetFloat("RotationSens", RotationSensitivity);
		RotationThreshold = PlayerPrefs.GetFloat("RotationThreshold", RotationThreshold);
	}

	private void Update() {
		if (DisableCalibration)
			return;

		// If we dont have two fingers on the screen, reset total variables and set everything to false.
		if (Input.touchCount != 2) {
			_totalRotation = 0;
			_totalZoom = 0;
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
			_totalZoom = 0;
			return;
		}

		// Calculate angle and pinch
		DetectTouchMovement.Calculate();

		_totalZoom += Mathf.Abs(DetectTouchMovement.PinchDistanceDelta);
		// If _totalZoom crosses the threshold and we arent rotating, zoom
		if (_totalZoom >= ZoomThreshold && !_isRotating) {
			//Zoom
			_isZooming = true;
			float pinchAmount = DetectTouchMovement.PinchDistanceDelta * ZoomSensitivity;
			Camera.main.fieldOfView -= pinchAmount;
			Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, MinFov, MaxFov);
		}

		_totalRotation += Mathf.Abs(DetectTouchMovement.TurnAngleDelta);
		if (_totalRotation >= RotationThreshold && !_isZooming) {
			// Rotate
			_isRotating = true;
			float rotationAmount = DetectTouchMovement.TurnAngleDelta * RotationSensitivity;
			if (_gyroCam.IsCarMode) {
				// TODO temporarily disabled
				//transform.RotateAround(User.transform.position, Vector3.up, rotationAmount);
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