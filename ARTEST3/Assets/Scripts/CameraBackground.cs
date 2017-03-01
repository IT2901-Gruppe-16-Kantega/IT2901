using UnityEngine;
using UnityEngine.UI;

/*
Renders the phone's camera onto the world
*/
public class CameraBackground : MonoBehaviour {
	// The camera itself
	private WebCamTexture phoneCamera;
	// The object that displays the picture
	private RawImage image;
	// To make the picture not squishy
	private AspectRatioFitter arf;

	// Use this for initialization
	void Start() {
		if (WebCamTexture.devices.Length != 0) {
			// Get the components that are attached to this GameObject
			arf = GetComponent<AspectRatioFitter>();
			image = GetComponent<RawImage>();

			// Make a new WebCamTexture that matches our screen width and height
			// Divide by 4 to reduce the quality to increase the framerate. Really important for Android phones. iOS devices are magically good at this somehow
			// Can maybe use 480x640 instead?
			#if UNITY_ANDROID
				phoneCamera = new WebCamTexture(Screen.width / 4, Screen.height / 4);
			#else
				phoneCamera = new WebCamTexture(Screen.width, Screen.height);
			#endif
			// Set the texture of the RawImage to the WebCamTexture
			image.texture = phoneCamera;
			// Turn on the camera
			//phoneCamera.requestedFPS = 30;
			phoneCamera.Play();
		}
	}

	// Update is called once per frame
	void Update() {
		if (WebCamTexture.devices.Length == 0) {
			return;
		}
		// If the camera rotation is wrong, fix it
		float cwNeeded = -phoneCamera.videoRotationAngle;
		if (phoneCamera.videoVerticallyMirrored)
			cwNeeded += 180f;
		image.rectTransform.localEulerAngles = new Vector3(0f, 0f, cwNeeded);

		// Calculate the camera's aspect ratio
		float videoRatio = (float)phoneCamera.width / (float)phoneCamera.height;
		// Set the aspect ratio of the displaying image to the camera's aspect ratio to fix distortion problems
		arf.aspectRatio = videoRatio;

		// If the videofeed is vertically mirrored
		if (phoneCamera.videoVerticallyMirrored) {
			// Flip the image
			image.uvRect = new Rect(1, 0, -1, 1);
		} else {
			// Otherwise draw it as it is
			image.uvRect = new Rect(0, 0, 1, 1);
		}
	}

	// A button on screen that plays or pauses the camera
	void OnGUI() {
		if (GUI.Button(new Rect(10, Screen.height / 2 - 100, Screen.width / 10, Screen.height / 10), "Toggle Camera")) {
			if (phoneCamera.isPlaying) {
				phoneCamera.Pause();
			} else {
				phoneCamera.Play();
			}
		}
	}
}
