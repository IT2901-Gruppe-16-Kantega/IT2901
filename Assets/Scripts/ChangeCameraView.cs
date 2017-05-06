using System.Collections;
using UnityEngine;

public class ChangeCameraView : MonoBehaviour {
	public static bool IsCarMode;
	private Vector3 _startPosition;
	private Vector3 _targetPosition;
	private const float MovementSpeed = 100;
	private const float BackOffset = -45;
	private const float UpOffset = 20;
	public static float DragSpeedX = 1f;
	public static float DragSpeedY = 0.7f;
	private const float TouchLimiterX = 0.1f;
	private const float TouchLimiterY = 0.05f;
	public Renderer UserRenderer;
	private bool _isRotating;

	private GyroscopeCamera _gyroscopeCamera;
	private Camera _mainCamera;
	[SerializeField] private ObjectSelect objectSelectScript;

	private void Start() {
		IsCarMode = false;
		_mainCamera = Camera.main;
		_gyroscopeCamera = GetComponent<GyroscopeCamera>();
		_startPosition = gameObject.transform.position;
		_targetPosition = _startPosition;
	}

	private void LateUpdate() {
		UserRenderer.gameObject.transform.forward = new Vector3(transform.forward.x, 0, transform.forward.z);
		if (!IsCarMode || ObjectSelect.IsDragging)
			return;
		if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.touchSupported))
			_isRotating = true;
		if (Input.GetMouseButtonUp(0) || (Input.touchCount == 0 && Input.touchSupported))
			_isRotating = false;
		if (!_isRotating)
			return;
		Vector3 previousPosition = transform.position;
		float yVal = Input.GetAxis("Mouse Y");
		float xVal = Input.GetAxis("Mouse X");
		if (Input.touchCount > 0) {
			yVal = Input.touches[0].deltaPosition.y * TouchLimiterY * _mainCamera.fieldOfView / 60f;
			xVal = Input.touches[0].deltaPosition.x * TouchLimiterX * _mainCamera.fieldOfView / 60f;
		}

		transform.RotateAround(UserRenderer.transform.position, transform.right, yVal * DragSpeedY);
		if (transform.localEulerAngles.x < 0 || transform.localEulerAngles.x > 60)
			transform.position = previousPosition;

		transform.RotateAround(UserRenderer.transform.position, Vector3.up, -xVal * DragSpeedX);
		transform.position = new Vector3(transform.position.x, 27, transform.position.z);
		transform.localEulerAngles = new Vector3(
			ClampAngle(transform.localEulerAngles.x, 0, 60),
			transform.localEulerAngles.y,
			0
		);
	}

	private static float ClampAngle(float angle, float min, float max) {
		if (angle < 90 || angle > 270) {
			if (angle > 180)
				angle -= 360;
			if (max > 180)
				max -= 360;
			if (min > 180)
				min -= 360;
		}
		angle = Mathf.Clamp(angle, min, max);
		if (angle < 0)
			angle += 360;
		return angle;
	}

	/// <summary>
	/// Changes the view from first person to third person (car mode)
	/// </summary>
	public void ChangeView() {
		if (ObjectSelect.ZoomedOnObject) {
			ObjectSelect.ZoomedOnObject = false;
			objectSelectScript.ZoomChange();
		}
		if (IsCarMode) {
			// Move main camera back into the start position
			_targetPosition = _startPosition;
			_gyroscopeCamera.IsCarMode = false;
			UserRenderer.enabled = false;
			foreach (Renderer child in UserRenderer.GetComponentsInChildren<Renderer>()) {
				child.enabled = false;
			}

			IsCarMode = false;
			StartCoroutine(LookAtUser());
		} else {
			// Move main camera back and up into third person view
			_targetPosition = _startPosition + UserRenderer.gameObject.transform.forward * BackOffset + UserRenderer.gameObject.transform.up * UpOffset;
			_gyroscopeCamera.IsCarMode = true;
			UserRenderer.enabled = true;
			foreach (Renderer child in UserRenderer.GetComponentsInChildren<Renderer>()) {
				child.enabled = true;
			}
			IsCarMode = true;
			StartCoroutine(LookAtUser());
		}
	}

	/// <summary>
	/// Moves the camera smoothly to first person view or third person view
	/// </summary>
	private IEnumerator LookAtUser() {
		ManualCalibration.DisableCalibration = true;
		while ((transform.position - _targetPosition).magnitude > 0.1f) {
			transform.position = Vector3.MoveTowards(transform.position, _targetPosition, MovementSpeed * Time.deltaTime);
			transform.forward = Vector3.Lerp(transform.forward, UserRenderer.gameObject.transform.forward, MovementSpeed * Time.deltaTime);
			yield return new WaitForEndOfFrame();
		}
		transform.forward = UserRenderer.gameObject.transform.forward;
		transform.position = _targetPosition; // To correct any floating errors.
		ManualCalibration.DisableCalibration = false;
	}
}