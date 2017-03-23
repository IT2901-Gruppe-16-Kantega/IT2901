using UnityEngine;

public class RenderLimiter : MonoBehaviour {
	public float SphereRadius = 250f;
	[SerializeField] private GameObject _signs;
	[SerializeField] private GameObject _roads;

	private void Update() {
		foreach (Transform sign in _signs.transform) {
			// Reactivate object if not activated
			sign.gameObject.SetActive((sign.position - transform.position).magnitude < SphereRadius);
		}
		foreach (Transform road in _roads.transform) {
			// Reactivate object if not activated
			road.gameObject.SetActive((road.position - transform.position).magnitude < SphereRadius * 2);
		}
	}
}
