using UnityEngine;

public class RenderLimiter : MonoBehaviour {
	public float SphereRadius = 250f;
	[SerializeField] private GameObject _signs;
	[SerializeField] private GameObject _roads;

	private void Update() {
		foreach (Transform sign in _signs.transform) {
			// Reactivate object if not activated
			if (!sign.gameObject.activeInHierarchy)
				sign.gameObject.SetActive(true);
			sign.gameObject.SetActive(Physics.Raycast(transform.position, sign.position - transform.position, SphereRadius)); // Shoot a ray to the sign and activate if ray is long enough
		}
		foreach (Transform road in _roads.transform) {
			// Reactivate object if not activated
			if (!road.gameObject.activeInHierarchy)
				road.gameObject.SetActive(true);
			road.gameObject.SetActive(Physics.Raycast(transform.position, road.position - transform.position, SphereRadius * 2)); // Shoot a ray to the road and activate if ray is long enough
		}
	}
}
