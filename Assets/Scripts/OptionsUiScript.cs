using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

/// <summary>
/// Handles the options inputs
/// </summary>
public class OptionsUiScript : MonoBehaviour {
	
	[SerializeField] private RectTransform _mainScreen;
	[SerializeField] private RectTransform _optionsScreen;

	// Main screen UI elements
	[SerializeField] private Button _rnDataButton;
	[SerializeField] private Button _localDataButton;
	[SerializeField] private Button _optionsButton;

	// Options screen UI elements
	[SerializeField] private Slider _zoomSensitivitySlider;
	[SerializeField] private InputField _zoomSensitivityInputField;
	[SerializeField] private Slider _zoomThresholdSlider;
	[SerializeField] private InputField _zoomThresholdInputField;
	[SerializeField] private Slider _rotateSensitivitySlider;
	[SerializeField] private InputField _rotateSensitivityInputField;
	[SerializeField] private Slider _rotateThresholdSlider;
	[SerializeField] private InputField _rotateThresholdInputField;	
	[SerializeField] private Button _backButton;
	[SerializeField] private Button _resetButton;

	private const float DefaultZoomSens = 0.3f;
	private const float DefaultZoomThreshold = 50f;
	private const float DefaultRotateSens = 0.8f;
	private const float DefaultRotateThreshold = 4f;
	private const float AnimationDampening = 0.4f;
	private bool _isOptionShown;
	private bool _disableInput;

	private void Start() {
		_zoomSensitivitySlider.onValueChanged.AddListener(delegate { ZoomSensitivityChange(true); });
		_zoomSensitivityInputField.onValueChanged.AddListener(delegate { ZoomSensitivityChange(false); });

		_zoomThresholdSlider.onValueChanged.AddListener(delegate { ZoomThresholdChange(true); });
		_zoomThresholdInputField.onValueChanged.AddListener(delegate { ZoomThresholdChange(false); });

		_rotateSensitivitySlider.onValueChanged.AddListener(delegate { RotateSensitivityChange(true); });
		_rotateSensitivityInputField.onValueChanged.AddListener(delegate { RotateSensitivityChange(false); });

		_rotateThresholdSlider.onValueChanged.AddListener(delegate { RotateThresholdChange(true); });
		_rotateThresholdInputField.onValueChanged.AddListener(delegate { RotateThresholdChange(false); });

		_rnDataButton.onClick.AddListener(delegate { GoToMain(false); });
		_localDataButton.onClick.AddListener(delegate { GoToMain(true); });
		_optionsButton.onClick.AddListener(GoOptionsOrBack);
		_backButton.onClick.AddListener(GoOptionsOrBack);
		_resetButton.onClick.AddListener(ResetOptions);

		// Load previous values
		_zoomSensitivitySlider.value = PlayerPrefs.GetFloat("ZoomSens", DefaultZoomSens);
		_zoomThresholdSlider.value = PlayerPrefs.GetFloat("ZoomThreshold", DefaultZoomThreshold);
		_rotateSensitivitySlider.value = PlayerPrefs.GetFloat("RotateSens", DefaultRotateSens);
		_rotateThresholdSlider.value = PlayerPrefs.GetFloat("RotateThreshold", DefaultRotateThreshold);
	}

	/// <summary>
	/// Go to the main scene.
	/// </summary>
	/// <param name="localData">If local data is to be used or data from the React Native app</param>
	public void GoToMain(bool localData) {
		PlayerPrefs.SetInt("UseLocalData", localData ? 1 : 0);
		SceneManager.LoadScene("Main Scene");
	}

	/// <summary>
	/// Handles the changing of the zoom sensitivity slider or input field, and saves the value.
	/// </summary>
	/// <param name="sliderChanged">If the slider was changed.</param>
	public void ZoomSensitivityChange(bool sliderChanged) {
		if (_disableInput)
			return;
		if (sliderChanged) {
			_zoomSensitivityInputField.text = _zoomSensitivitySlider.value.ToString();
		} else {
			_zoomSensitivitySlider.value = float.Parse(_zoomSensitivityInputField.text);
		}
		PlayerPrefs.SetFloat("ZoomSens", _zoomSensitivitySlider.value);
		PlayerPrefs.Save();
	}

	/// <summary>
	/// Handles the changing of the zoom threshold slider or input field, and saves the value.
	/// </summary>
	/// <param name="sliderChanged">If the slider was changed.</param>
	public void ZoomThresholdChange(bool sliderChanged) {
		if (_disableInput)
			return;
		if (sliderChanged) {
			_zoomThresholdInputField.text = _zoomThresholdSlider.value.ToString();
		} else {
			_zoomThresholdSlider.value = float.Parse(_zoomThresholdInputField.text);
		}
		PlayerPrefs.SetFloat("ZoomThreshold", _zoomThresholdSlider.value);
		PlayerPrefs.Save();
	}

	/// <summary>
	/// Handles the changing of the rotate sensitivity slider or input field, and saves the value.
	/// </summary>
	/// <param name="sliderChanged">If the slider was changed.</param>
	public void RotateSensitivityChange(bool sliderChanged) {
		if (_disableInput)
			return;
		if (sliderChanged) {
			_rotateSensitivityInputField.text = _rotateSensitivitySlider.value.ToString();
		} else {
			_rotateSensitivitySlider.value = float.Parse(_rotateSensitivityInputField.text);
		}
		PlayerPrefs.SetFloat("RotateSens", _rotateSensitivitySlider.value);
		PlayerPrefs.Save();
	}

	/// <summary>
	/// Handles the changing of the rotate threshold slider or input field, and saves the value.
	/// </summary>
	/// <param name="sliderChanged">If the slider was changed.</param>
	public void RotateThresholdChange(bool sliderChanged) {
		if (_disableInput) 
			return;
		if (sliderChanged) {
			_rotateThresholdInputField.text = _rotateThresholdSlider.value.ToString();
		} else {
			_rotateThresholdSlider.value = float.Parse(_rotateThresholdInputField.text);
		}
		PlayerPrefs.SetFloat("RotateThreshold", _rotateThresholdSlider.value);
		PlayerPrefs.Save();
	}

	/// <summary>
	/// Shows or hides the Options screen.
	/// </summary>
	public void GoOptionsOrBack() {
		if (_disableInput)
			return;
		StartCoroutine(AnimateOptions());
	}

	/// <summary>
	/// Resets the various values to the default ones.
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
	/// Animates showing or hiding of the options screen.
	/// </summary>
	private IEnumerator AnimateOptions() {
		_disableInput = true;
		if (_isOptionShown) {
			// Hide Options Screen
			_isOptionShown = false;
			while (_optionsScreen.anchoredPosition.x <= 719) {
				_mainScreen.localPosition = Vector3.Lerp(_mainScreen.localPosition, new Vector3(0, 0, 0), AnimationDampening);
				_optionsScreen.anchoredPosition = Vector2.Lerp(_optionsScreen.anchoredPosition, new Vector2(720, 0), AnimationDampening);
				yield return new WaitForEndOfFrame();
			}
			_mainScreen.localPosition = Vector3.zero;
			_optionsScreen.anchoredPosition = new Vector2(720, 0);
		} else {
			// Show Options Screen
			_isOptionShown = true;
			while (_optionsScreen.anchoredPosition.x >= 1f) {
				_mainScreen.localPosition = Vector3.Lerp(_mainScreen.localPosition, new Vector3(0, 0, 100), AnimationDampening);
				_optionsScreen.anchoredPosition = Vector2.Lerp(_optionsScreen.anchoredPosition, Vector2.zero, AnimationDampening);
				yield return new WaitForEndOfFrame();
			}
			_mainScreen.localPosition = new Vector3(0, 0, 100);
			_optionsScreen.anchoredPosition = Vector2.zero;
		}
		_disableInput = false;
	}
}
