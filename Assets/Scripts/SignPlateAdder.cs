using UnityEngine;

public class SignPlateAdder : MonoBehaviour {
	public string SignPostId;
	public int SignPlateCount;
	public GameObject BlueCircle;
	public GameObject RedCircle;
	public GameObject RedTriangle;

	public void AddPlate(Objekter objekt) {
		GameObject signPlate = Instantiate(GetGameObject(objekt), Vector3.zero, Quaternion.identity, transform) as GameObject;
		if (signPlate == null)
			return; // in case anything weird happens
		RoadObjectManager rom = signPlate.GetComponent<RoadObjectManager>();

		rom.RoadObjectLocation = objekt.parsedLocation[0];
		rom.UpdateLocation();
		rom.Objekt = objekt;
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
		rom.OriginPoint = position;
		signPlate.transform.Translate(0, 2 + SignPlateCount, 0);
		SignPlateCount++;
		if (SignPlateCount > 1) {
			// Unnecessary to have more than one distance text
			rom.DistanceText.gameObject.SetActive(false);
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
