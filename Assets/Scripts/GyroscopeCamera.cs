using UnityEngine;

/*
Translates the device's gyroscope attitude into camera rotations
*/
public class GyroscopeCamera : MonoBehaviour {
	// Gyroscope variables
	private Gyroscope _gyro;
	private bool _gyroIsSupported;
	public bool Calibrating;
	public float Rotation;
	public bool IsCarMode;

	public GameObject User;

	// For filtering gyro data
	private const float LowPassFactor = 0.5f; // A float between 0.01f to 0.99f. Less means more dampening

	// Different rotations based on the phone's display mode
	private readonly Quaternion _baseIdentity = Quaternion.Euler(90, 0, 0);

	// Variables for fixing the gyroscope
	private Quaternion _cameraBase = Quaternion.identity;
	private Quaternion _calibration = Quaternion.identity;
	private Quaternion _baseOrientation = Quaternion.Euler(90, 0, 0);
	private readonly Quaternion _baseOrientationRotationFix = Quaternion.identity;

	private Quaternion _referenceRotation = Quaternion.identity;

	private void Start() {
		// Check if gyroscope is supported on this device
		_gyroIsSupported = SystemInfo.supportsGyroscope;

		if (_gyroIsSupported) {
			// Get the gyroscope from input
			_gyro = Input.gyro;
			// Enable it
			_gyro.enabled = true;
			// Calibrate stuff
			ResetBaseOrientation();
			UpdateCalibration(true);
			UpdateCameraBaseRotation(true);
			RecalculateReferenceRotation();
		} else {
			Debug.Log("Gyroscope is not supported.");
		}
	}

	private void OnDestroy() {
		if (_gyroIsSupported)
			_gyro.enabled = false;
	}

	private void Update() {
		// Can't do anything if we don't have a gyro.
		if (!_gyroIsSupported || Calibrating || IsCarMode || ObjectSelect.IsDragging || ObjectSelect.IsZoomed) {
			return;
		}
		Vector3 gyroTemp = _gyro.attitude.eulerAngles;
		gyroTemp.y += Rotation;

		// Slerp is spherical linear interpolation, which means that our movement is smoothed instead of jittering
		transform.rotation = Quaternion.Slerp(transform.rotation,
			_cameraBase * (ConvertRotation(_referenceRotation * _gyro.attitude)), LowPassFactor);
		transform.Rotate(0, Rotation, 0, Space.World);
		User.transform.rotation = Quaternion.AngleAxis(transform.rotation.eulerAngles.y, User.transform.up);
	}

	// Update the gyroscope calibration
	private void UpdateCalibration(bool onlyHorizontal) {
		if (onlyHorizontal) {
			Vector3 fw = (Input.gyro.attitude) * (-Vector3.forward);
			fw.z = 0;
			_calibration = fw == Vector3.zero ? Quaternion.identity : (Quaternion.FromToRotation(_baseOrientationRotationFix * Vector3.up, fw));
		} else {
			_calibration = Input.gyro.attitude;
		}
	}


	private void UpdateCameraBaseRotation(bool onlyHorizontal) {
		if (onlyHorizontal) {
			Vector3 fw = transform.forward;
			fw.y = 0;
			_cameraBase = fw == Vector3.zero ? Quaternion.identity : Quaternion.FromToRotation(Vector3.forward, fw);
		} else {
			_cameraBase = transform.rotation;
		}
	}

	// Flips the gyroscope attitude
	private static Quaternion ConvertRotation(Quaternion q) {
		return new Quaternion(q.x, q.y, -q.z, -q.w);
	}

	private void ResetBaseOrientation() {
		_baseOrientation = _baseOrientationRotationFix * _baseIdentity;
	}

	private void RecalculateReferenceRotation() {
		_referenceRotation = Quaternion.Inverse(_baseOrientation) * Quaternion.Inverse(_calibration);
	}
}