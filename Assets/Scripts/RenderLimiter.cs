using UnityEngine;

public class RenderLimiter : MonoBehaviour {
	public float SphereRadius = 250f;
	[SerializeField] private GameObject _signs;
	
	private void Update() {
		//foreach (Transform sign in _signs.transform) {
		for(int i = 0; i < _signs.transform.childCount; i++) {
			// Sign is active only if it is within the sphere radius and within the FOV of the camera
			_signs.transform.GetChild(i).gameObject.SetActive((_signs.transform.GetChild(i).position - transform.position).magnitude < SphereRadius && Vector3.Angle(Camera.main.transform.forward, new Vector3(Camera.main.transform.position.x - _signs.transform.GetChild(i).transform.position.x, 0, Camera.main.transform.position.z - _signs.transform.GetChild(i).transform.position.z)) > 180 - Camera.main.fieldOfView / 2);
		}
	}
}
