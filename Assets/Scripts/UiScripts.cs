﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UiScripts : MonoBehaviour {
	private const float AnimationDampening = 0.4f;

	public static int RoadsToInstantiate;
	public static int RoadsInstantiated;
	public static int ObjectsToInstantiate;
	public static int ObjectsInstantiated;
	private readonly Vector3 _bottomTargetRotation = new Vector3(0, 0, 225);
	private readonly Vector3 _infoBoxEndPosition = new Vector3(0, -100);
	private readonly Vector3 _infoBoxStartPosition = new Vector3(-300, -100);
	private readonly Vector3 _menuBgEndPosition = new Vector3(320, -10);
	private readonly Vector3 _menuBgStartPosition = new Vector3(720, -10);
	private readonly Vector3 _menuButtonEndPosition = new Vector3(-310, -10);
	private readonly Vector3 _menuButtonStartPosition = new Vector3(-10, -10);
	private readonly Vector3 _middleTargetPosition = new Vector3(-10, 0);
	private readonly Vector3 _topTargetRotation = new Vector3(0, 0, -225);

	[SerializeField]
	private RectTransform _infoBox;

	private bool _isInfoShown;
	private bool _isMenuShown;

	[SerializeField]
	private GameObject _loadingPanel;

	[SerializeField]
	private Text _loadObjectText;

	[SerializeField]
	private Text _loadRoadText;

	[SerializeField]
	private RectTransform _menuBg;

	[SerializeField]
	private RectTransform _menuBottom;

	[SerializeField]
	private RectTransform _menuButton;

	[SerializeField]
	private RectTransform _menuMiddle;

	[SerializeField]
	private RectTransform _menuTop;

	public GameObject Signs;
	// TODO currently only for saving reports. Maybe add for loading and saving files?
	public Image StatusImage;
	public Text StatusText;

	private void Start() {
		_loadingPanel.SetActive(true);
		StartCoroutine(LoadingScreen());
	}

	/// <summary>
	///     Opens the react native app VegAR Kart
	/// </summary>
	public void OpenReactNative() {
		Application.OpenURL("vegar.kart:");
	}

	/// <summary>
	///     Shows the object info on the top left
	/// </summary>
	public void ShowInfo() {
		StopCoroutine(AnimateShowInfo());
		StartCoroutine(AnimateShowInfo());
	}

	/// <summary>
	///     Gets all of the signs that was marked or moved and adds them to the SharedData.WrongObjects list
	/// </summary>
	public void GenerateReport() {
		// Add all signplates that has been moved or is wrong
		List<Objekter> movedSignsList = (from Transform signPost in Signs.transform
			from Transform signPlate in signPost.transform
			select signPlate.GetComponent<RoadObjectManager>()
			into rom
			where rom && (rom.HasBeenMoved || rom.SomethingIsWrong)
			select rom.Objekt).ToList();
		/* // NON LINQ CODE
		 List<Objekter> movedSignsList = new List<Objekter>();
		 foreach (Transform signPost in Signs.transform) {
		   foreach (Transform signPlate in signPost.transform) {
		     RoadObjectManager rom = signPlate.GetComponent<RoadObjectManager>();
		     if(rom && (rom.HasBeenMoved || rom.SomethingIsWrong)) movedSignsList.Add(rom.Objekt);
		   }
		 }
		 */
		SharedData.AllData.report = movedSignsList;

		SharedData.WrongObjects.AddRange(movedSignsList);
		if (LocalStorage.CreateReport("report.json", movedSignsList)) {
			StatusText.text = "Raport lagret";
			StartCoroutine(AnimateStatus());
			SceneManager.LoadScene("Report Scene");
		} else {
			StatusText.text = "Raport feilet";
			StartCoroutine(AnimateStatus());
		}
	}

	/// <summary>
	///     Method to show or hide menu. Stops the previous coroutine in case it was pressed twice
	/// </summary>
	public void ShowMenu() {
		StopCoroutine(AnimateMenu());
		StartCoroutine(AnimateMenu());
	}

	/// <summary>
	///     Shrink or enlarge the sign info box
	/// </summary>
	private IEnumerator AnimateShowInfo() {
		if (_isInfoShown) {
			_isInfoShown = false;
			while (_infoBox.anchoredPosition.x >= _infoBoxStartPosition.x + 1) {
				_infoBox.anchoredPosition = Vector3.Lerp(_infoBox.anchoredPosition, _infoBoxStartPosition, AnimationDampening);
				yield return new WaitForEndOfFrame();
			}
			_infoBox.anchoredPosition = _infoBoxStartPosition;
		} else {
			_isInfoShown = true;
			while (_infoBox.anchoredPosition.x <= _infoBoxEndPosition.x - 1) {
				_infoBox.anchoredPosition = Vector3.Lerp(_infoBox.anchoredPosition, _infoBoxEndPosition, AnimationDampening);
				yield return new WaitForEndOfFrame();
			}
			_infoBox.anchoredPosition = _infoBoxEndPosition;
		}
	}

	/// <summary>
	///     Fade in and out status
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

	/// <summary>
	///     Animate the three bars into an arrow and show the top right menu and vice versa
	/// </summary>
	private IEnumerator AnimateMenu() {
		if (_isMenuShown) {
			_isMenuShown = false;
			// Hide menu and animate button back to three bars
			while (_menuMiddle.anchoredPosition.x <= -1) {
				_menuBg.anchoredPosition = Vector3.Lerp(_menuBg.anchoredPosition, _menuBgStartPosition, AnimationDampening);
				_menuButton.anchoredPosition = Vector3.Lerp(_menuButton.anchoredPosition, _menuButtonStartPosition,
					AnimationDampening);
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

	/// <summary>
	///     The loading screen to show progress for the user
	/// </summary>
	/// <returns></returns>
	private IEnumerator LoadingScreen() {
		// Reset this to handle changing of scenes.
		GenerateObjects.IsCreatingSigns = true;
		GenerateRoads.IsCreatingRoads = true;
		ObjectsInstantiated = 0;
		RoadsInstantiated = 0;

		while (GenerateObjects.IsCreatingSigns || GenerateRoads.IsCreatingRoads) {
			_loadObjectText.text = GenerateObjects.IsCreatingSigns
				? string.Format("Laster inn vegobjekter: {0} av {1} ({2}%)", ObjectsInstantiated, ObjectsToInstantiate,
					ObjectsToInstantiate > 0 ? ObjectsInstantiated / ObjectsToInstantiate * 100 : 0)
				: "Lastet inn alle vegobjekter";
			_loadRoadText.text = GenerateObjects.IsCreatingSigns
				? string.Format("Laster inn veger: {0} av {1} ({2}%)", RoadsInstantiated, RoadsToInstantiate,
					RoadsToInstantiate > 0 ? RoadsInstantiated / RoadsToInstantiate * 100 : 0)
				: "Lastet inn alle veger";
			yield return null;
		}
		_loadingPanel.SetActive(false);
	}
}