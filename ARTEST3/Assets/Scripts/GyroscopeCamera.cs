using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GyroscopeCamera : MonoBehaviour {
	private Gyroscope gyro;
	private bool gyroIsSupported;


	public Text gyroText;
	public Text gyroText2;

	private const float lowPassFactor = 0.3f; // A float between 0.01f to 0.99f. Less means more dampening
	private bool lowPassInit = true;

	private readonly Quaternion baseIdentity = Quaternion.Euler(90, 0, 0);
	private readonly Quaternion landscapeRight = Quaternion.Euler(0, 0, 90);
	private readonly Quaternion landscapeLeft = Quaternion.Euler(0, 0, -90);
	private readonly Quaternion upsideDown = Quaternion.Euler(0, 0, 180);

	private Quaternion cameraBase = Quaternion.identity;
	private Quaternion calibration = Quaternion.identity;
	private Quaternion baseOrientation = Quaternion.Euler(90, 0, 0);
	private Quaternion baseOrientationRotationFix = Quaternion.identity;

	private Quaternion referanceRotation = Quaternion.identity;

	void Start() {
		gyroIsSupported = SystemInfo.supportsGyroscope;

		if (gyroIsSupported) {
			gyro = Input.gyro;
			gyro.enabled = true;
			ResetBaseOrientation();
			UpdateCalibration(true);
			UpdateCameraBaseRotation(true);
			RecalculateReferenceRotation();
		} else {
			Debug.Log("Gyroscope is not supported.");
		}
	}

	void Update() {
		if (!gyroIsSupported) {
			return;
		}
		transform.rotation = Quaternion.Slerp(transform.rotation,
cameraBase * (ConvertRotation(referanceRotation * Input.gyro.attitude)), lowPassFactor);
		gyroText.text = "";
		gyroText.text += "\nattitude" + gyro.attitude;
		gyroText.text += "\nattitude" + gyro.attitude.eulerAngles;
		gyroText.text += "\nrotation" + transform.rotation.eulerAngles;
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
		referanceRotation = Quaternion.Inverse(baseOrientation) * Quaternion.Inverse(calibration);
	}
}