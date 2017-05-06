using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     Handles the options inputs
/// </summary>
public class OptionsUiScript : MonoBehaviour {
	private const float DefaultZoomSens = 0.3f;
	private const float DefaultZoomThreshold = 50f;
	private const float DefaultRotateSens = 0.8f;
	private const float DefaultRotateThreshold = 4f;
	private const float DefaultDraggingSensX = 1f;
	private const float DefaultDraggingSensY = 0.7f;

	private const float AnimationDampening = 0.2f;
	private bool _disableInput;
	private bool _isOptionShown;

	[SerializeField]
	private Toggle _lowEnergyToggle;

	[SerializeField]
	private RectTransform _optionsScreen;

	[SerializeField]
	private InputField _rotateSensitivityInputField;

	[SerializeField]
	private Slider _rotateSensitivitySlider;

	[SerializeField]
	private InputField _rotateThresholdInputField;

	[SerializeField]
	private Slider _rotateThresholdSlider;

	[SerializeField]
	private InputField _xDraggingSensitivityInputField;

	[SerializeField]
	private Slider _xDraggingSensitivitySlider;

	[SerializeField]
	private InputField _yDraggingSensitivityInputField;

	[SerializeField]
	private Slider _yDraggingSensitivitySlider;

	[SerializeField]
	private InputField _zoomSensitivityInputField;

	// Options screen UI elements
	[SerializeField]
	private Slider _zoomSensitivitySlider;

	[SerializeField]
	private InputField _zoomThresholdInputField;

	[SerializeField]
	private Slider _zoomThresholdSlider;

	private void Start() {
		// Load previous values
		_zoomSensitivitySlider.value = PlayerPrefs.GetFloat("ZoomSens", DefaultZoomSens);
		_zoomThresholdSlider.value = PlayerPrefs.GetFloat("ZoomThreshold", DefaultZoomThreshold);
		_rotateSensitivitySlider.value = PlayerPrefs.GetFloat("RotateSens", DefaultRotateSens);
		_rotateThresholdSlider.value = PlayerPrefs.GetFloat("RotateThreshold", DefaultRotateThreshold);
		_xDraggingSensitivitySlider.value = PlayerPrefs.GetFloat("DragSensX", DefaultDraggingSensX);
		_yDraggingSensitivitySlider.value = PlayerPrefs.GetFloat("DragSensY", DefaultDraggingSensY);
		_lowEnergyToggle.isOn = PlayerPrefs.GetInt("leMode", 0) == 1;
	}

	/// <summary>
	///     Handles the changing of the zoom sensitivity slider or input field, and saves the value.
	/// </summary>
	/// <param name="sliderChanged">If the slider was changed.</param>
	public void ZoomSensitivityChange(bool sliderChanged) {
		if (_disableInput)
			return;
		if (sliderChanged)
			_zoomSensitivityInputField.text = _zoomSensitivitySlider.value.ToString();
		else
			_zoomSensitivitySlider.value = float.Parse(_zoomSensitivityInputField.text);
		PlayerPrefs.SetFloat("ZoomSens", _zoomSensitivitySlider.value);
		ManualCalibration.ZoomSensitivity = _zoomSensitivitySlider.value;
		PlayerPrefs.Save();
	}

	/// <summary>
	///     Handles the changing of the zoom threshold slider or input field, and saves the value.
	/// </summary>
	/// <param name="sliderChanged">If the slider was changed.</param>
	public void ZoomThresholdChange(bool sliderChanged) {
		if (_disableInput)
			return;
		if (sliderChanged)
			_zoomThresholdInputField.text = _zoomThresholdSlider.value.ToString();
		else
			_zoomThresholdSlider.value = float.Parse(_zoomThresholdInputField.text);
		PlayerPrefs.SetFloat("ZoomThreshold", _zoomThresholdSlider.value);
		ManualCalibration.ZoomThreshold = _zoomThresholdSlider.value;
		PlayerPrefs.Save();
	}

	/// <summary>
	///     Handles the changing of the rotate sensitivity slider or input field, and saves the value.
	/// </summary>
	/// <param name="sliderChanged">If the slider was changed.</param>
	public void RotateSensitivityChange(bool sliderChanged) {
		if (_disableInput)
			return;
		if (sliderChanged)
			_rotateSensitivityInputField.text = _rotateSensitivitySlider.value.ToString();
		else
			_rotateSensitivitySlider.value = float.Parse(_rotateSensitivityInputField.text);
		PlayerPrefs.SetFloat("RotateSens", _rotateSensitivitySlider.value);
		ManualCalibration.RotationSensitivity = _rotateSensitivitySlider.value;
		PlayerPrefs.Save();
	}

	/// <summary>
	///     Handles the changing of the rotate threshold slider or input field, and saves the value.
	/// </summary>
	/// <param name="sliderChanged">If the slider was changed.</param>
	public void RotateThresholdChange(bool sliderChanged) {
		if (_disableInput)
			return;
		if (sliderChanged)
			_rotateThresholdInputField.text = _rotateThresholdSlider.value.ToString();
		else
			_rotateThresholdSlider.value = float.Parse(_rotateThresholdInputField.text);
		PlayerPrefs.SetFloat("RotateThreshold", _rotateThresholdSlider.value);
		ManualCalibration.RotationThreshold = _rotateThresholdSlider.value;
		PlayerPrefs.Save();
	}

	/// <summary>
	///     Handles the changing of the x dragging sensitivity slider or input field, and saves the value.
	/// </summary>
	/// <param name="sliderChanged">If the slider was changed.</param>
	public void XDragSensChange(bool sliderChanged) {
		if (_disableInput)
			return;
		if (sliderChanged)
			_xDraggingSensitivityInputField.text = _xDraggingSensitivitySlider.value.ToString();
		else
			_xDraggingSensitivitySlider.value = float.Parse(_xDraggingSensitivityInputField.text);
		PlayerPrefs.SetFloat("DragSensX", _xDraggingSensitivitySlider.value);
		ChangeCameraView.DragSpeedX = _xDraggingSensitivitySlider.value;
		PlayerPrefs.Save();
	}

	/// <summary>
	///     Handles the changing of the y dragging sensitivity slider or input field, and saves the value.
	/// </summary>
	/// <param name="sliderChanged">If the slider was changed.</param>
	public void YDragSensChange(bool sliderChanged) {
		if (_disableInput)
			return;
		if (sliderChanged)
			_yDraggingSensitivityInputField.text = _yDraggingSensitivitySlider.value.ToString();
		else
			_yDraggingSensitivitySlider.value = float.Parse(_yDraggingSensitivityInputField.text);
		PlayerPrefs.SetFloat("DragSensY", _yDraggingSensitivitySlider.value);
		ChangeCameraView.DragSpeedY = _yDraggingSensitivitySlider.value;
		PlayerPrefs.Save();
	}

	/// <summary>
	///     Handles the changing of the low energy mode toggle
	/// </summary>
	public void ToggleLowEnergyMode() {
		PlayerPrefs.SetInt("leMode", _lowEnergyToggle.isOn ? 1 : 0);
		PlayerPrefs.Save();
	}

	/// <summary>
	///     Shows or hides the Options screen.
	/// </summary>
	public void GoOptionsOrBack() {
		if (_disableInput)
			return;
		StartCoroutine(AnimateOptions());
	}

	/// <summary>
	///     Resets the various values to the default ones.
	/// </summary>
	public void ResetOptions() {
		// Reset all values
		PlayerPrefs.SetFloat("ZoomSens", DefaultZoomSens);
		PlayerPrefs.SetFloat("ZoomThreshold", DefaultZoomThreshold);
		PlayerPrefs.SetFloat("RotateSens", DefaultRotateSens);
		PlayerPrefs.SetFloat("RotateThreshold", DefaultRotateThreshold);
		// Reset all sliders
		_zoomSensitivitySlider.value = PlayerPrefs.GetFloat("ZoomSens", DefaultZoomSens);
		_zoomThresholdSlider.value = PlayerPrefs.GetFloat("ZoomThreshold", DefaultZoomThreshold);
		_rotateSensitivitySlider.value = PlayerPrefs.GetFloat("RotateSens", DefaultRotateSens);
		_rotateThresholdSlider.value = PlayerPrefs.GetFloat("RotateThreshold", DefaultRotateThreshold);
	}

	/// <summary>
	///     Animates showing or hiding of the options screen.
	/// </summary>
	private IEnumerator AnimateOptions() {
		_disableInput = true;
		if (_isOptionShown) {
			// Hide Options Screen
			_isOptionShown = false;
			while (_optionsScreen.anchoredPosition.x <= 719) {
				_optionsScreen.anchoredPosition = Vector2.Lerp(_optionsScreen.anchoredPosition, new Vector2(720, 0),
					AnimationDampening);
				yield return new WaitForEndOfFrame();
			}
			_optionsScreen.anchoredPosition = new Vector2(720, 0);
		} else {
			// Show Options Screen
			_isOptionShown = true;
			while (_optionsScreen.anchoredPosition.x >= 1f) {
				_optionsScreen.anchoredPosition = Vector2.Lerp(_optionsScreen.anchoredPosition, Vector2.zero, AnimationDampening);
				yield return new WaitForEndOfFrame();
			}
			_optionsScreen.anchoredPosition = Vector2.zero;
		}
		_disableInput = false;
	}
}