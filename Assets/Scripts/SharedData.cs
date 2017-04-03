using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SharedData : MonoBehaviour {

	public static List<Objekter> Data = new List<Objekter>();

	private void Awake() {
		DontDestroyOnLoad(this);
	}

	//private void Start() {
	//	SceneManager.LoadScene("main");
	//}
}
