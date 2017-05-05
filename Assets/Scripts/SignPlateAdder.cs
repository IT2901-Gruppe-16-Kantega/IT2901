using System.Collections.Generic;
using UnityEngine;

public class SignPlateAdder : MonoBehaviour {
	public string SignPostId;
	public int SignPlateCount;
	public GameObject BlueCircle;
	public GameObject RedCircle;
	public GameObject RedTriangle;
	public Renderer PoleRenderer;
	private readonly List<GameObject> _children = new List<GameObject>();

	private void FixedUpdate() {
		if (Vector3.Angle(
			Camera.main.transform.forward, new Vector3(Camera.main.transform.position.x - transform.position.x, 0, Camera.main.transform.position.z - transform.position.z)) < 90f)
			return;
		transform.LookAt(new Vector3(Camera.main.transform.position.x,
									0,
									Camera.main.transform.position.z));
		foreach (GameObject child in _children) {
			child.GetComponent<RoadObjectManager>().UpdateLocation();
		}
	}

	public void AddPlate(Objekter objekt) {
		GameObject signPlate = Instantiate(GetGameObject(objekt), Vector3.zero, Quaternion.identity, transform) as GameObject;
		if (signPlate == null)
			return; // in case anything weird happens
		signPlate.name = objekt.id.ToString();
		RoadObjectManager rom = signPlate.GetComponent<RoadObjectManager>();

		rom.RoadObjectLocation = objekt.parsedLocation[0];
		rom.UpdateLocation();
		rom.Objekt = objekt;
		rom.SignPost = gameObject; // let the RoadObjectManager know that this is its signpost.
		rom.PoleRenderer = PoleRenderer;
		rom.UnSelected();
		Egenskaper prop = objekt.egenskaper.Find(egenskap => egenskap.id == 5530); // 5530 is sign number
		if (prop == null) {
			// TODO HANDLING OBJECTS MISSING STUFF HAS BEEN MOVED TO REACT NATIVE. Keep this in case it is needed.
			//SharedData.Data.Add(objekt);
			objekt.metadata.notat = "Mangler egenskap 5530";
		}
		string[] parts = prop == null ? new[] { "MANGLER", "EGENSKAP", "5530" } : prop.verdi.Split(' ', '-');
		rom.SignText.text = "";

		// Try to fit the text inside the sign
		string text = "";
		foreach (string s in parts) {
			rom.SignText.text += s + " ";
			if (rom.SignText.GetComponent<Renderer>().bounds.extents.x > .5) {
				rom.SignText.text = text.TrimEnd() + "\n" + s + " ";
			}
			text = rom.SignText.text;
		}
		Vector3 position = HelperFunctions.GetPositionFromCoords(objekt.parsedLocation[0]);
		signPlate.transform.position = position;
		signPlate.transform.localRotation = Quaternion.identity;
		rom.OriginPoint = position;
		signPlate.transform.Translate(0, 2 + SignPlateCount, 0);
		SignPlateCount++;
		if (SignPlateCount > 1) {
			// Unnecessary to have more than one distance text
			rom.DistanceText.gameObject.SetActive(false);
		}
		_children.Add(signPlate);
	}

	public void MarkPlates(bool value) {
		foreach (GameObject signPlate in _children) {
			RoadObjectManager rom = signPlate.GetComponent<RoadObjectManager>();
			rom.SomethingIsWrong = value;
			rom.Objekt.metadata.notat = rom.SomethingIsWrong ? "Markert som feil av bruker (arvet fra skiltpunkt)" : "";
		}
	}

	private GameObject GetGameObject(Objekter objekt) {
		Egenskaper egenskap = objekt.egenskaper.Find(e => e.id == 5530);

		if (egenskap == null)
			return BlueCircle;
		int signNumber;
		int.TryParse(egenskap.verdi.Substring(0, 1), out signNumber);

		switch (signNumber) {
			case 1:
				return RedCircle;
			case 2:
				return RedTriangle;
			default:
				return BlueCircle;
		}
	}
}
