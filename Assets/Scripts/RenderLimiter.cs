using UnityEngine;

public class RenderLimiter : MonoBehaviour {
	public float SphereRadius = 250f;
	public GameObject Signs;
	[Range(0.01f, 2f)]
	public float CarLimit = 0.2f;

	private void Update() {
		if(GenerateObjects.LineObjects)
			return;
		for(int i = 0; i < Signs.transform.childCount; i++) {
			// Sign is active only if it is within the sphere radius and within the FOV of the camera
			Signs.transform.GetChild(i).gameObject.SetActive(
				(Signs.transform.GetChild(i).position - transform.position).magnitude < SphereRadius && 
				Vector3.Angle(Camera.main.transform.forward, new Vector3(
					Camera.main.transform.position.x - Signs.transform.GetChild(i).transform.position.x, 
					0, 
					Camera.main.transform.position.z - Signs.transform.GetChild(i).transform.position.z)
				) > 180 - Camera.main.fieldOfView / (ChangeCameraView.IsCarMode ? CarLimit : 2));
		}
	}
}
