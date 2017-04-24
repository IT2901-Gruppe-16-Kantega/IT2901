using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ReportDataHandler : MonoBehaviour {

	[SerializeField]
	private GameObject _contentGameObject;

	[SerializeField]
	private GameObject _objekt;
	
	private void Start() {
		StartCoroutine(ShowReport());
	}

	/// <summary>
	/// Go back to main scene
	/// </summary>
	public void GoBack() {
		StartCoroutine(LoadMainScene());
	}

	/// <summary>
	/// Open React Native scene
	/// </summary>
	public void OpenReactNative() {
	// TODO for ios. send report data
		Application.OpenURL("vegar://rapport?id="+SharedData.AllData.key);
	}

	/// <summary>
	/// Loads main scene
	/// </summary>
	private static IEnumerator LoadMainScene() {
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("Main Scene");
		while (!asyncOperation.isDone)
			yield return null;
	}
	

	/// <summary>
	/// Creates a list of the objects in the report with their ID and metadata
	/// Adds one each frame, and because of the very empty scene, this is really quick.
	/// </summary>
	private IEnumerator ShowReport() {
		// Create a list of road objects with their info
		Debug.Log("Creating report");
		float height = 50;
		RectTransform contentRectTransform = _contentGameObject.GetComponent<RectTransform>();
		contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, 0);
		foreach (Objekter objekt in SharedData.Data) {
			// Create a parent gameobject for the road objects whose parent is the _contentGameObject
			GameObject parent = Instantiate(_objekt, _contentGameObject.transform) as GameObject;
			if (parent == null) continue;
			parent.name = objekt.id.ToString();

			// Write the name (ID) of the object
			Text objektNameText = parent.transform.GetChild(0).GetComponent<Text>();
			if (objektNameText == null)
				continue;
			objektNameText.text = string.Format("ID: {0}", objekt.id);
			objektNameText.transform.SetParent(parent.transform);

			// Write how far and the bearing of the distance moved
			Text objektInfoText = objektNameText.transform.GetChild(0).GetComponent<Text>();
			if (objektInfoText == null)
				continue;
			objektInfoText.text = string.Format("Distance from origin: \t\t\t{0:F2} meters\nbearing from origin: \t\t\t{1:F2} degrees\nNotes: \t\t\t\t\t{2}", objekt.metadata.distance, objekt.metadata.bearing, objekt.metadata.notat);
			objektInfoText.transform.parent.SetParent(parent.transform);
			parent.transform.localScale = Vector3.one;
			parent.transform.localPosition = new Vector3(350, -height, 10);
			height += 100;
			contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, contentRectTransform.sizeDelta.y + 100);
			Debug.Log(objekt.id.ToString());
			yield return null;
		}
		Debug.Log("Report Created");
	}
}
