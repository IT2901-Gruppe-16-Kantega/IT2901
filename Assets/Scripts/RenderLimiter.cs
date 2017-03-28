using UnityEngine;

public class RenderLimiter : MonoBehaviour {
	public float SphereRadius = 250f;
	[SerializeField]
	private GameObject _signs;

	private void Update() {
		//foreach (Transform sign in _signs.transform) {
		for(int i = 0; i < _signs.transform.childCount; i++) {
			// Reactivate object if not activated
			_signs.transform.GetChild(i).gameObject.SetActive((_signs.transform.GetChild(i).position - transform.position).magnitude < SphereRadius);
		}
	}
}
