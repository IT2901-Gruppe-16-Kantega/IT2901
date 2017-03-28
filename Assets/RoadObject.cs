using UnityEngine;

public class RoadObject : MonoBehaviour {

	// ReSharper disable once RedundantDefaultMemberInitializer
	private bool _selected = false;


	// Use this for initialization
	private void Start() {
	}

	// Update is called once per frame
	private void Update() {
		if (Input.GetMouseButtonDown(0)) {
			_selected = !_selected;
		}
		GetComponentInChildren<Renderer>().material.shader = Shader.Find(_selected ? "Self-Illumin/Outlined Diffuse" : "Diffuse");
	}
}
