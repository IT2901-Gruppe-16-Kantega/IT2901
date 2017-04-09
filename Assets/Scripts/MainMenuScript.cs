using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuScript : MonoBehaviour {


	[SerializeField] private RectTransform _mainScreen;
	[SerializeField] private RectTransform _optionsScreen;

	// Main screen UI elements
	[SerializeField] private Button _rnDataButton;
	[SerializeField] private Button _localDataButton;

	private const float AnimationDampening = 0.2f;
	private bool _isOptionShown;
	private bool _disableInput;
	private readonly Vector3 _mainScreenStartPosition = Vector3.zero;
	private readonly Vector3 _mainScreenEndPosition = new Vector3(0, 0, 100);
	private readonly Vector3 _optionScreenStartPosition = new Vector3(720, 0);
	private readonly Vector3 _optionScreenEndPosition = Vector3.zero;

	// Use this for initialization
	private void Start() {
		_rnDataButton.onClick.AddListener(delegate { GoToMain(false); });
		_localDataButton.onClick.AddListener(delegate { GoToMain(true); });
	}

	/// <summary>
	/// Go to the main scene.
	/// </summary>
	/// <param name="localData">If local data is to be used or data from the React Native app</param>
	public void GoToMain(bool localData) {
		PlayerPrefs.SetInt("UseLocalData", localData ? 1 : 0); // 1 is true, 0 is false. PlayerPrefs does not have a SetBool function.
		SceneManager.LoadScene("Main Scene");
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
	/// Animates showing or hiding of the options screen.
	/// </summary>
	private IEnumerator AnimateOptions() {
		_disableInput = true;
		if (_isOptionShown) {
			// Hide Options Screen
			_isOptionShown = false;
			while (_optionsScreen.anchoredPosition.x <= 719) {
				_mainScreen.localPosition = Vector3.Lerp(_mainScreen.localPosition, _mainScreenStartPosition, AnimationDampening);
				_optionsScreen.anchoredPosition = Vector2.Lerp(_optionsScreen.anchoredPosition, _optionScreenStartPosition, AnimationDampening);
				yield return new WaitForEndOfFrame();
			}
			_mainScreen.localPosition = _mainScreenStartPosition;
			_optionsScreen.anchoredPosition = _optionScreenStartPosition;
		} else {
			// Show Options Screen
			_isOptionShown = true;
			while (_optionsScreen.anchoredPosition.x >= 1f) {
				_mainScreen.localPosition = Vector3.Lerp(_mainScreen.localPosition, _mainScreenEndPosition, AnimationDampening);
				_optionsScreen.anchoredPosition = Vector2.Lerp(_optionsScreen.anchoredPosition, _optionScreenEndPosition, AnimationDampening);
				yield return new WaitForEndOfFrame();
			}
			_mainScreen.localPosition = _mainScreenEndPosition;
			_optionsScreen.anchoredPosition = _optionScreenEndPosition;
		}
		_disableInput = false;
	}
}
