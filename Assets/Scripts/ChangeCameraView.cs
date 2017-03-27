using System.Collections;
using UnityEngine;

public class ChangeCameraView : MonoBehaviour {
	private bool _isCarMode;
	private Vector3 _startPosition;
	private Vector3 _targetPosition;
	private const float MovementSpeed = 100;
	private const float RotationSpeed = 2.5f;
	private const float BackOffset = -45;
	private const float UpOffset = 20;
	public Renderer Renderer;

	private GyroscopeCamera _gyroscopeCamera;

	private void Start() {
		_gyroscopeCamera = GetComponent<GyroscopeCamera>();
		_startPosition = gameObject.transform.position;
		_targetPosition = _startPosition;
	}

	private void Update() {
		if (!_isCarMode) return;
		Quaternion rotationAmount = Quaternion.LookRotation((Renderer.transform.position - transform.position) + transform.up * UpOffset);
		transform.rotation = Quaternion.Slerp(transform.rotation, rotationAmount, Time.deltaTime * RotationSpeed);
	}

	public void ChangeView() {
		if (_isCarMode) {
			_targetPosition = _startPosition;
			_gyroscopeCamera.IsCarMode = false;
			Renderer.enabled = false;
			_isCarMode = false;
			StopCoroutine(LookAtUser());
			StartCoroutine(LookAtUser());
		}
		else {
			_targetPosition = _startPosition + Renderer.gameObject.transform.forward * BackOffset + Renderer.gameObject.transform.up * UpOffset;
			_gyroscopeCamera.IsCarMode = true;
			Renderer.enabled = true;
			_isCarMode = true;
			StopCoroutine(LookAtUser());
			StartCoroutine(LookAtUser());
		}
	}

	private IEnumerator LookAtUser() {
		while ((transform.position - _targetPosition).magnitude > 0.1f) {
			transform.position = Vector3.MoveTowards(transform.position, _targetPosition, MovementSpeed * Time.deltaTime);
			yield return new WaitForEndOfFrame();
		}
	}

	private void OnGUI() {
		float buttonWidth = Screen.width / 3f;
		float buttonHeight = Screen.height / 10f;
		GUI.skin.button.fontSize = (int) (buttonHeight / 4);

		if (GUI.Button(new Rect(Screen.width / 2f - buttonWidth/4f, Screen.height - 4/3f * buttonHeight , buttonWidth, buttonHeight), "Toggle Camera"))
			ChangeView();
	}
}