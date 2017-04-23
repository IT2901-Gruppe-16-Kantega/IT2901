using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UiScripts : MonoBehaviour {

	public Image InfoBgImage;
	public Text InfoText;
	public GameObject Signs;
	// TODO currently only for saving reports. Maybe add for loading and saving files?
	public Image StatusImage;
	public Text StatusText;
	[SerializeField] private GameObject _loadingPanel;
	[SerializeField] private Text _loadObjectText;
	[SerializeField] private Text _loadRoadText;
	[SerializeField] private RectTransform _menuButton;
	[SerializeField] private RectTransform _menuTop;
	[SerializeField] private RectTransform _menuMiddle;
	[SerializeField] private RectTransform _menuBottom;
	[SerializeField] private RectTransform _menuBg;
	private readonly Vector3 _menuButtonStartPosition = new Vector3(-10, -10);
	private readonly Vector3 _menuButtonEndPosition = new Vector3(-310, -10);
	private readonly Vector3 _middleTargetPosition = new Vector3(-10, 0);
	private readonly Vector3 _menuBgStartPosition = new Vector3(720, -10);
	private readonly Vector3 _menuBgEndPosition = new Vector3(320, -10);
	private readonly Vector3 _topTargetRotation = new Vector3(0, 0, -225);
	private readonly Vector3 _bottomTargetRotation = new Vector3(0, 0, 225);
	private bool _isMenuShown;

	private bool _isInfoShown;
    private readonly Vector2 _infoBgMaxSize = new Vector2(320, 300);
	private readonly Vector2 _infoTextMaxSize = new Vector2(300, 280);

    private bool _isObjectMarked;
    // TODO object marked

    private const float AnimationDampening = 0.4f;

	public static int RoadsToInstantiate;
	public static int RoadsInstantiated;
	public static int ObjectsToInstantiate;
	public static int ObjectsInstantiated;

	private void Start() {
		_loadingPanel.SetActive(true);
		StartCoroutine(LoadingScreen());
	}

	private IEnumerator LoadingScreen() {
		// Reset this to handle changing of scenes.
		GenerateObjects.IsCreatingSigns = true;
		GenerateRoads.IsCreatingRoads = true;
		ObjectsInstantiated = 0;
		RoadsInstantiated = 0;

		while (GenerateObjects.IsCreatingSigns || GenerateRoads.IsCreatingRoads) {
			_loadObjectText.text = (GenerateObjects.IsCreatingSigns) ? string.Format("Loading Objects... {0} of {1}", ObjectsInstantiated, ObjectsToInstantiate) : "Done loading objects";
			_loadRoadText.text = (GenerateObjects.IsCreatingSigns) ? string.Format("Loading Roads... {0} of {1}", RoadsInstantiated, RoadsToInstantiate) : "Done loading roads";
			yield return null;
		}
		_loadingPanel.SetActive(false);
	}

	public void OpenReactNative() {
		Application.OpenURL("nvdbRn:");
	}

	public void ShowInfo() {
		StopCoroutine(AnimateShowInfo());
		StartCoroutine(AnimateShowInfo());
	}

    public void GenerateReport() {
		// Add all signplates that has been moved or is wrong
		List<Objekter> movedSignsList = (from Transform signPost in Signs.transform from Transform signPlate in signPost.transform select signPlate.GetComponent<RoadObjectManager>() into rom where rom.HasBeenMoved || rom.SomethingIsWrong select rom.Objekt).ToList();
	    /* // NON LINQ CODE
		 * List<Objekter> movedSignsList = new List<Objekter>();
		 * foreach (Transform signPost in Signs.transform) {
		 *   foreach (Transform signPlate in signPost.transform) {
		 *     RoadObjectManager rom = signPlate.GetComponent<RoadObjectManager>();
		 *     if(rom.HasBeenMoved || rom.SomethingIsWrong) movedSignsList.Add(rom.Objekt);
		 *   }
		 * }
		 */
		SharedData.Data.AddRange(movedSignsList);
		StatusText.text = LocalStorage.CreateReport("report.json", movedSignsList) ? "Report Saved Successfully" : "Report Failed To Save";
		StartCoroutine(AnimateStatus());
		SceneManager.LoadScene("Report Scene");
	}

	public void ShowMenu() {
		StopCoroutine(AnimateMenu());
		StartCoroutine(AnimateMenu());
	}

	/// <summary>
	/// Shrink or enlarge the sign info box
	/// </summary>
	private IEnumerator AnimateShowInfo() {
		if (_isInfoShown) {
			_isInfoShown = false;
			while (InfoBgImage.rectTransform.sizeDelta.x >= 1f && !_isInfoShown) { // shrink
				InfoBgImage.rectTransform.sizeDelta = Vector2.Lerp(InfoBgImage.rectTransform.sizeDelta, Vector2.zero,
					AnimationDampening);
				InfoText.rectTransform.sizeDelta = Vector2.Lerp(InfoText.rectTransform.sizeDelta, Vector2.zero,
					AnimationDampening);
				yield return new WaitForEndOfFrame();
			}
			InfoText.rectTransform.sizeDelta = Vector2.zero;
			InfoBgImage.rectTransform.sizeDelta = Vector2.zero;
		} else {
			_isInfoShown = true;
			while (InfoBgImage.rectTransform.sizeDelta.x <= _infoBgMaxSize.x - 1f && _isInfoShown) { // enlarge
				InfoBgImage.rectTransform.sizeDelta = Vector2.Lerp(InfoBgImage.rectTransform.sizeDelta, _infoBgMaxSize,
					AnimationDampening);
				InfoText.rectTransform.sizeDelta = Vector2.Lerp(InfoText.rectTransform.sizeDelta, _infoTextMaxSize,
					AnimationDampening);
				yield return new WaitForEndOfFrame();
			}
			InfoText.rectTransform.sizeDelta = _infoTextMaxSize;
			InfoBgImage.rectTransform.sizeDelta = _infoBgMaxSize;
		}
	}

    /// <summary>
    /// Fade in and out status
    /// </summary>
    private IEnumerator AnimateStatus() {
		Color statusImageTargetColor = StatusImage.color;
		Color statusTextTargetColor = StatusText.color;
		statusImageTargetColor.a = 1;
		statusTextTargetColor.a = 1;
		// Increase opacity
		while (StatusImage.color.a < 0.99f) {
			StatusImage.color = Color.Lerp(StatusImage.color, statusImageTargetColor, AnimationDampening);
			StatusText.color = Color.Lerp(StatusText.color, statusTextTargetColor, AnimationDampening);
			yield return new WaitForEndOfFrame();
		}
		StatusImage.color = statusImageTargetColor;
		StatusText.color = statusTextTargetColor;

		// Wait 5 seconds
		yield return new WaitForSeconds(2);

		statusImageTargetColor.a = 0;
		statusTextTargetColor.a = 0;
		// Decrease opacity
		while (StatusImage.color.a > 0.01f) {
			StatusImage.color = Color.Lerp(StatusImage.color, statusImageTargetColor, AnimationDampening);
			StatusText.color = Color.Lerp(StatusText.color, statusTextTargetColor, AnimationDampening);
			yield return new WaitForEndOfFrame();
		}
		StatusImage.color = statusImageTargetColor;
		StatusText.color = statusTextTargetColor;
	}

	private IEnumerator AnimateMenu() {
		if (_isMenuShown) {
			_isMenuShown = false;
			// Hide menu and animate button back to three bars
			while (_menuMiddle.anchoredPosition.x <= -1) {
				_menuBg.anchoredPosition = Vector3.Lerp(_menuBg.anchoredPosition, _menuBgStartPosition, AnimationDampening);
				_menuButton.anchoredPosition = Vector3.Lerp(_menuButton.anchoredPosition, _menuButtonStartPosition, AnimationDampening);
				_menuMiddle.anchoredPosition = Vector3.Lerp(_menuMiddle.anchoredPosition, Vector3.zero, AnimationDampening);
				_menuTop.localEulerAngles = Vector3.Lerp(_menuTop.localEulerAngles, Vector3.zero, AnimationDampening);
				_menuBottom.localEulerAngles = Vector3.Lerp(_menuBottom.localEulerAngles, Vector3.zero, AnimationDampening);
				yield return new WaitForEndOfFrame();
			}
			_menuBg.anchoredPosition = _menuBgStartPosition;
			_menuButton.anchoredPosition = _menuButtonStartPosition;
			_menuMiddle.anchoredPosition = Vector3.zero;
			_menuTop.localEulerAngles = Vector3.zero;
			_menuBottom.localEulerAngles = Vector3.zero;
		} else {
			_isMenuShown = true;
			// Show menu and animate button to arrow
			while (_menuMiddle.anchoredPosition.x >= _middleTargetPosition.x + 1) {
				_menuBg.anchoredPosition = Vector3.Lerp(_menuBg.anchoredPosition, _menuBgEndPosition, AnimationDampening);
				_menuButton.anchoredPosition = Vector3.Lerp(_menuButton.anchoredPosition, _menuButtonEndPosition, AnimationDampening);
				_menuMiddle.anchoredPosition = Vector3.Lerp(_menuMiddle.anchoredPosition, _middleTargetPosition, AnimationDampening);
				_menuTop.localEulerAngles = Vector3.Lerp(_menuTop.localEulerAngles, _topTargetRotation, AnimationDampening);
				_menuBottom.localEulerAngles = Vector3.Lerp(_menuBottom.localEulerAngles, _bottomTargetRotation, AnimationDampening);
				yield return new WaitForEndOfFrame();
			}
			_menuBg.anchoredPosition = _menuBgEndPosition;
			_menuButton.anchoredPosition = _menuButtonEndPosition;
			_menuMiddle.anchoredPosition = _middleTargetPosition;
			_menuTop.localEulerAngles = _topTargetRotation;
			_menuBottom.localEulerAngles = _bottomTargetRotation;
		}
	}
}
