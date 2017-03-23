using UnityEngine;

public class RenderLimiter : MonoBehaviour {
	public float SphereRadius = 250f;
	[SerializeField]
	private GameObject _signs;

	private void Update() {
		foreach (Transform sign in _signs.transform) {
			// Reactivate object if not activated
			sign.gameObject.SetActive((sign.position - transform.position).magnitude < SphereRadius);
		}
	}
}
