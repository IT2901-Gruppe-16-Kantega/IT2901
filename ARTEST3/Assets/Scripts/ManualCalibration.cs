using UnityEngine;

public class ManualCalibration : MonoBehaviour {
	public float perspectiveZoomSpeed = 0.5f;        // The rate of change of the field of view in perspective mode.
	public float orthoZoomSpeed = 0.5f;        // The rate of change of the orthographic size in orthographic mode.
	[Range(0.01f, 1)]
	public float rotationDampening = 0.8f;
	private float totalRotation = 0.0f;
	private float rotationThreshold = 20;

	[SerializeField] private GyroscopeCamera gyroCam;

	void Start() {
		gyroCam = GetComponent<GyroscopeCamera>();
	}


	void Update()
	{
		/*
		Debug.Log (Input.touchCount);
		// If there are two touches on the device...
		if (Input.touchCount == 2) {
			gyroCam.calibrating = true;

			// Store both touches.
			Touch touchZero = Input.GetTouch (0);
			Touch touchOne = Input.GetTouch (1);

			// Find the position in the previous frame of each touch.
			Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
			Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
			Vector2 previousPositionDirection = touchZeroPrevPos - touchOnePrevPos;
			Vector2 currentPositionDirection = touchZero.position - touchOne.position;

			//Vector2 previousPositionDirection = (touchZero.position.magnitude > touchOne.position.magnitude) ? (touchZeroPrevPos - touchOnePrevPos) : (touchOnePrevPos - touchZeroPrevPos);
			//Vector2 currentPositionDirection = (touchZero.position.magnitude > touchOne.position.magnitude) ? (touchZero.position - touchOne.position) : (touchOne.position - touchZero.position);

			Vector2 previousPosition = touchZeroPrevPos - touchOnePrevPos;
			Vector2 currentPosition = touchZero.position - touchOne.position;
			float previousDirection = Vector2.Angle (touchZeroPrevPos, touchOnePrevPos);
			float currentDirection = Vector2.Angle (touchZero.position, touchOne.position);
			Debug.Log ("previousDirection: " + previousDirection);
			Debug.Log ("currentDirection: " + currentDirection);

			float rotation = Vector2.Angle(previousPositionDirection,currentPositionDirection);
			// some code to find whether the angle is positive or negative?

			if (previousPositionDirection.y > currentPositionDirection.y) rotation*=(-1);

			//gyroCam.rotation = rotation;
			//transform.RotateAround (transform.position, transform.up, rotation);
			transform.Rotate(0,rotation,0, Space.World);
			//sDebug.Log (rotation);

		} else {
			gyroCam.calibrating = false;
		}
		*/
		if (!gyroCam.calibrating) {
			totalRotation = 0;
			return;
		}

		if (Input.touchCount != 2)
			totalRotation = 0;

		float pinchAmount = 0;
		Quaternion desiredRotation = transform.rotation;
		float rotationAmount = 0.0f;

		DetectTouchMovement.Calculate ();
		/*
		if (Mathf.Abs (DetectTouchMovement.pinchDistanceDelta) > 0) { //Zoom
			pinchAmount = DetectTouchMovement.pinchDistanceDelta;
			Camera.main.fieldOfView -= pinchAmount;
		}
		*/
		totalRotation += Mathf.Abs (DetectTouchMovement.turnAngleDelta);
		Debug.Log (totalRotation);
		if (totalRotation >= rotationThreshold) { // Rotate
			//Vector3 rotationDeg = Vector3.zero;
			//rotationDeg.y = -DetectTouchMovement.turnAngleDelta;
			//desiredRotation *= Quaternion.Euler (rotationDeg);
			rotationAmount = DetectTouchMovement.turnAngleDelta * rotationDampening;
			transform.Rotate( 0, rotationAmount, 0, Space.World);
		}

	}
	// A button on screen that plays or pauses the camera
	void OnGUI() {
		if (GUI.Button(new Rect(10, Screen.height / 2 + 100, Screen.width / 10, Screen.height / 10), "Manual Calibration")) {
			gyroCam.calibrating = !gyroCam.calibrating;
		}
	}
}
