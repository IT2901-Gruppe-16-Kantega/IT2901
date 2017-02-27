using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class RoadObject : MonoBehaviour {

    private bool selected = false;


	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        if(Input.GetMouseButtonDown(0)) {
            selected = !selected;
        }
        if(selected) {
            GetComponentInChildren<Renderer>().material.shader = Shader.Find("Self-Illumin/Outlined Diffuse");
        }
        else {
            GetComponentInChildren<Renderer>().material.shader = Shader.Find("Diffuse");
        }
	}
}
