using UnityEngine;
using System.Collections;

public class ObjectSelect : MonoBehaviour {

	//The GameObject that is pressed
	private GameObject target;

	// The layers to target
	LayerMask layers;

	private bool isMouseDrag;
	private Vector3 screenPosition;
	private Vector3 offset;

	// Use this for initialization
	void Start() {
		// The layer to target
		layers = LayerMask.GetMask("Signs");
	}

	// Update is called once per frame
	void Update() {
		if (Input.GetMouseButtonDown(0)) {
			RaycastHit hitInfo;
			target = ReturnClickedObject(out hitInfo);

			if (target != null) {
				isMouseDrag = true;
				Debug.Log("Old target position :" + target.transform.position);
				//Convert world position to screen position.
				screenPosition = Camera.main.WorldToScreenPoint(target.transform.position);
				offset = target.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPosition.z));
			}
		}
		if (Input.GetMouseButtonUp(0)) {
			isMouseDrag = false;
			if (target != null) {
				Debug.Log("New target position :" + target.transform.position);
			}
		}
		if (isMouseDrag && target != null) {
			//track mouse position.
			Vector3 currentScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPosition.z);
			//convert screen position to world position with offset changes.
			Vector3 currentPosition = Camera.main.ScreenToWorldPoint(currentScreenSpace) + offset;
			//It will update target gameobject's current postion.
			target.transform.position = currentPosition;
		}
	}

	GameObject ReturnClickedObject(out RaycastHit hit) {
		GameObject newTarget = null;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		if (Physics.Raycast(ray, out hit, float.PositiveInfinity, layers)) {
			Debug.DrawLine(ray.origin, hit.point);
			newTarget = hit.collider.gameObject;
			newTarget.GetComponent<RoadObjectManager>().Selected();
		}
		if(newTarget != target && target != null) {
			target.GetComponent<RoadObjectManager>().UnSelected();
		}
		Debug.Log(newTarget);
		return newTarget;
	}
}
