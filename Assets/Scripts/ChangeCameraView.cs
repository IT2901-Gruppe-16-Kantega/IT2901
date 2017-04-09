using System.Collections;
using UnityEngine;

public class ChangeCameraView : MonoBehaviour {
	private bool _isCarMode;
	private Vector3 _startPosition;
	private Vector3 _targetPosition;
	private const float MovementSpeed = 100;
	private const float BackOffset = -45;
	private const float UpOffset = 20;
	public static float DragSpeedX = 1f;
	public static float DragSpeedY = 0.7f;
	private const float TouchLimiterX = 0.1f;
	private const float TouchLimiterY = 0.05f;
	[SerializeField] private Renderer _userRenderer;
	private bool _isRotating;

	private GyroscopeCamera _gyroscopeCamera;
	private Camera _mainCamera;

	private void Start() {
		_mainCamera = Camera.main;
		_gyroscopeCamera = GetComponent<GyroscopeCamera>();
		_startPosition = gameObject.transform.position;
		_targetPosition = _startPosition;
	}

	private void LateUpdate() {
		if (!_isCarMode)
			return;
		if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.touchSupported))
			_isRotating = true;
		if (Input.GetMouseButtonUp(0) || (Input.touchCount == 0 && Input.touchSupported))
			_isRotating = false;
		if (!_isRotating) return;
		Vector3 previousPosition = transform.position;
		float yVal = Input.GetAxis("Mouse Y");
		float xVal = Input.GetAxis("Mouse X");
		if (Input.touchCount > 0) {
			yVal = Input.touches[0].deltaPosition.y * TouchLimiterY * _mainCamera.fieldOfView / 60f;
			xVal = Input.touches[0].deltaPosition.x * TouchLimiterX * _mainCamera.fieldOfView / 60f;
		}
		
		transform.RotateAround(_userRenderer.transform.position, transform.right, yVal * DragSpeedY);
		if (transform.localEulerAngles.x < 0 || transform.localEulerAngles.x > 60)
			transform.position = previousPosition;

		transform.RotateAround(_userRenderer.transform.position, Vector3.up, -xVal * DragSpeedX);
		transform.position = new Vector3(transform.position.x, 27, transform.position.z);
		transform.localEulerAngles = new Vector3(
			ClampAngle(transform.localEulerAngles.x, 0, 60),
			transform.localEulerAngles.y,
			0
		);
	}

	private static float ClampAngle(float angle, float min, float max) {
		if (angle < 90 || angle > 270) {
			if (angle > 180) angle -= 360;
			if (max > 180) max -= 360;
			if (min > 180) min -= 360;
		}
		angle = Mathf.Clamp(angle, min, max);
		if (angle < 0) angle += 360;
		return angle;
	}

	/// <summary>
	/// Changes the view from first person to third person (car mode)
	/// </summary>
	public void ChangeView() {
		if (_isCarMode) {
			// Move main camera back into the start position
			_targetPosition = _startPosition;
			_gyroscopeCamera.IsCarMode = false;
			_userRenderer.enabled = false;
			_isCarMode = false;
			StartCoroutine(LookAtUser());
		} else {
			// Move main camera back and up into third person view
			_targetPosition = _startPosition + _userRenderer.gameObject.transform.forward * BackOffset + _userRenderer.gameObject.transform.up * UpOffset;
			_gyroscopeCamera.IsCarMode = true;
			_userRenderer.enabled = true;
			_isCarMode = true;
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
			yield return new WaitForEndOfFrame();
		}
		transform.position = _targetPosition; // To correct any floating errors.
		ManualCalibration.DisableCalibration = false;
	}
}