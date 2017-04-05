using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class UiScripts : MonoBehaviour {

	public Image InfoBgImage;
	public Text InfoText;
	public GameObject Signs;
	// TODO currently only for saving reports. Maybe add for loading and saving files?
	public Image StatusImage;
	public Text StatusText;
	public GameObject LoadingPanel;

	private bool _isInfoShown;
	private readonly Vector2 _infoBgMaxSize = new Vector2(400, 500);
	private readonly Vector2 _infoTextMaxSize = new Vector2(380, 480);
	private const float AnimationDampening = 0.4f;

	private void Start() {
		StartCoroutine(LoadingScreen());
	}

	private IEnumerator LoadingScreen() {
		#if DEVELOPMENT_BUILD
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		#endif
		while (GenerateObjects.IsCreatingSigns || RoadGenerator.IsCreatingRoads) {
			yield return null;
		}
		#if DEVELOPMENT_BUILD
		stopwatch.Stop();
		Debug.Log("DONE LOADING " + stopwatch.Elapsed);
		#endif
		LoadingPanel.SetActive(false);
	}

	public void OpenReactNative() {
		Application.OpenURL("nvdbRn:");
	}

	public void ShowInfo() {
		StopCoroutine(AnimateShowInfo());
		StartCoroutine(AnimateShowInfo());
	}

	public void GenerateReport() {
		List<Objekter> movedSignsList = (from Transform sign in Signs.transform select sign.GetComponent<RoadObjectManager>() into rom where rom.HasBeenMoved select rom.Objekt).ToList();
		SharedData.Data.AddRange(movedSignsList);
		StatusText.text = LocalStorage.CreateReport("report.json", movedSignsList) ? "Report Saved Successfully" : "Report Failed To Save";
		StartCoroutine(AnimateStatus());
		StartCoroutine(LoadReportScene());
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

	private static IEnumerator LoadReportScene() {
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("report");
		while (!asyncOperation.isDone) yield return null;
	}
}
