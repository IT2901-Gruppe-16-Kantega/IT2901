using UnityEngine;

/*
Translates the device's gyroscope attitude into camera rotations
*/
public class GyroscopeCamera : MonoBehaviour {
	// Gyroscope variables
	private Gyroscope gyro;
	private bool gyroIsSupported;

	// For filtering gyro data
	private const float lowPassFactor = 0.8f; // A float between 0.01f to 0.99f. Less means more dampening

	// Different rotations based on the phone's display mode
	private readonly Quaternion baseIdentity = Quaternion.Euler(90, 0, 0);

	// Variables for fixing the gyroscope
	private Quaternion cameraBase = Quaternion.identity;
	private Quaternion calibration = Quaternion.identity;
	private Quaternion baseOrientation = Quaternion.Euler(90, 0, 0);
	private Quaternion baseOrientationRotationFix = Quaternion.identity;

	private Quaternion referenceRotation = Quaternion.identity;

	void Start() {
		// Check if gyroscope is supported on this device
		gyroIsSupported = SystemInfo.supportsGyroscope;

		if (gyroIsSupported) {
			// Get the gyroscope from input
			gyro = Input.gyro;
			// Enable it
			gyro.enabled = true;
			// Calibrate stuff
			ResetBaseOrientation();
			UpdateCalibration(true);
			UpdateCameraBaseRotation(true);
			RecalculateReferenceRotation();
		} else {
			Debug.Log("Gyroscope is not supported.");
		}
	}

	void Update() {
		// Can't do anything if we don't have a gyro.
		if (!gyroIsSupported) {
			return;
		}
		// Slerp is spherical linear interpolation, which means that our movement is smoothed instead of jittering
		transform.rotation = Quaternion.Slerp(transform.rotation,
cameraBase * (ConvertRotation(referenceRotation * Input.gyro.attitude)), lowPassFactor);
	}

	// Update the gyroscope calibration
	private void UpdateCalibration(bool onlyHorizontal) {
		if (onlyHorizontal) {
			var fw = (Input.gyro.attitude) * (-Vector3.forward);
			fw.z = 0;
			if (fw == Vector3.zero) {
				calibration = Quaternion.identity;
			} else {
				calibration = (Quaternion.FromToRotation(baseOrientationRotationFix * Vector3.up, fw));
			}
		} else {
			calibration = Input.gyro.attitude;
		}
	}


	private void UpdateCameraBaseRotation(bool onlyHorizontal) {
		if (onlyHorizontal) {
			var fw = transform.forward;
			fw.y = 0;
			if (fw == Vector3.zero) {
				cameraBase = Quaternion.identity;
			} else {
				cameraBase = Quaternion.FromToRotation(Vector3.forward, fw);
			}
		} else {
			cameraBase = transform.rotation;
		}
	}

	private static Quaternion ConvertRotation(Quaternion q) {
		return new Quaternion(q.x, q.y, -q.z, -q.w);
	}

	private void ResetBaseOrientation() {
		baseOrientation = baseOrientationRotationFix * baseIdentity;
	}

	private void RecalculateReferenceRotation() {
		referenceRotation = Quaternion.Inverse(baseOrientation) * Quaternion.Inverse(calibration);
	}
}