using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Translates the gyroscope in our phone to move the camera

I AM GOING TO COMPLETELY REWRITE THIS BECAUSE IT IS FAULTY, BUT AS A CONCEPT IT WORKS
*/
public class GyroCam : MonoBehaviour {
	// The gyroscope
	private Gyroscope gyro;
	// If the device has a gyroscope
	private bool gyroSupported;
	// A Quaternion to fix rotation problems
	private Quaternion rotFix;

	// SerializeField makes the private object accessable through the editor
	// The worldOject
	[SerializeField]
	private Transform worldObj;
	// Our y start value
	private float startY;

	// Use this for initialization
	void Start () {
		// Check if our device has a gyroscope
		gyroSupported = SystemInfo.supportsGyroscope;

		// Make a new GameObject camParent and make us the child of that GameObject
		// I don't know why I did this, the guy I watched did this, so I just mimiced him.
		// Read the all caps text on the top of this file
		GameObject camParent = new GameObject("camParent");
		camParent.transform.position= transform.position;
		transform.parent = camParent.transform;

		// If the device has a gyroscope
		if(gyroSupported) {
			// Set the gyro to the device's gyro
			gyro = Input.gyro;
			// Enable it
			gyro.enabled = true;

			// rotate the camParent to fix rotation errors
			camParent.transform.rotation = Quaternion.Euler(90f, 180f, 0f);
			// Make a new Quaternion to fix rotations
			rotFix = new Quaternion(0, 0, 1, 0);
		}
	}
	
	// Update is called once per frame
	void Update () {
		// If the gyroscope is supported and startY is 0, Reset the gyroscope rotation
		if (gyroSupported && startY == 0) {
			ResetGyroRotation();
		}
		// If the gyroscope is supported, rotate the unity camera to match the gyroscope rotation
		if(gyroSupported)
			transform.localRotation = gyro.attitude * rotFix;
	}

	// Method that resets the gyroscope rotation
	void ResetGyroRotation() {
		// Sett the startY to whatever the y eulerAngle is on the unity camera
		startY = transform.eulerAngles.y;
		// Rotate the world to match the rotation of the startY eulerAngle
		worldObj.rotation = Quaternion.Euler(0f, startY, 0f);
	}
}
