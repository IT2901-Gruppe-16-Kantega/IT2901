using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
Renders the phone's camera onto the world
*/
public class Camerabackground : MonoBehaviour {
	// The camera itself
	private WebCamTexture phoneCamera;
	// The object that displays the picture
	private RawImage image;
	// To make the picture not squishy
	private AspectRatioFitter arf;

	// Use this for initialization
	void Start () {
		// Get the components that are attached to this GameObject
		arf = GetComponent<AspectRatioFitter>();
		image = GetComponent<RawImage>();

		// Make a new WebCamTexture that matches our screen width and height
		phoneCamera = new WebCamTexture(Screen.width, Screen.height);
		// Set the texture of the RawImage to the WebCamTexture
		image.texture = phoneCamera;
		// Turn on the camera
		phoneCamera.Play();
	}
	
	// Update is called once per frame
	void Update () {
		// If the camera rotation is wrong, fix it
		float cwNeeded = -phoneCamera.videoRotationAngle;
		if (phoneCamera.videoVerticallyMirrored)
			cwNeeded += 180f;
		image.rectTransform.localEulerAngles = new Vector3(0f, 0f, cwNeeded);

		// Calculate the camera's aspect ratio
		float videoRatio = (float) phoneCamera.width / (float) phoneCamera.height;
		// Set the aspect ratio of the displaying image to the camera's aspect ratio to fix distortion problems
		arf.aspectRatio = videoRatio;

		// If the videofeed is vertically mirrored
		if(phoneCamera.videoVerticallyMirrored) {
			// Flip the image
			image.uvRect = new Rect(1, 0, -1, 1);
		} else {
			// Otherwise draw it as it is
			image.uvRect = new Rect(0, 0, 1, 1);
		}
	}
}
