using UnityEngine;
using UnityEngine.UI;

/*
Renders the phone's camera onto the world
*/
public class CameraBackground : MonoBehaviour {
	// The camera itself
	private WebCamTexture _phoneCamera;
	// The object that displays the picture
	private RawImage _image;
	// To make the picture not squishy
	private AspectRatioFitter _arf;

	// Use this for initialization
	private void Start() {
		if (WebCamTexture.devices.Length == 0) {
			Debug.Log("No Camera Devices");
			return;
		}
		// Get the components that are attached to this GameObject
		_arf = GetComponent<AspectRatioFitter>();
		_image = GetComponent<RawImage>();

		// Make a new WebCamTexture that matches our screen width and height
		// Divide by 4 to reduce the quality to increase the framerate. Really important for Android phones. iOS devices are magically good at this somehow
		// Can maybe use 480x640 instead?
#if UNITY_ANDROID
		_phoneCamera = new WebCamTexture(Screen.width / 4, Screen.height / 4);
#else
		_phoneCamera = new WebCamTexture(Screen.width, Screen.height);
#endif
		// Set the texture of the RawImage to the WebCamTexture
		_image.texture = _phoneCamera;
		// Turn on the camera
		//phoneCamera.requestedFPS = 30;
		_phoneCamera.Play();
	}

	// Update is called once per frame
	private void Update() {
		if (WebCamTexture.devices.Length == 0) {
			return;
		}
		// If the camera rotation is wrong, fix it
		float cwNeeded = -_phoneCamera.videoRotationAngle;
		if (_phoneCamera.videoVerticallyMirrored)
			cwNeeded += 180f;
		_image.rectTransform.localEulerAngles = new Vector3(0f, 0f, cwNeeded);

		// Calculate the camera's aspect ratio
		float videoRatio = _phoneCamera.width / (float) _phoneCamera.height;
		// Set the aspect ratio of the displaying image to the camera's aspect ratio to fix distortion problems
		_arf.aspectRatio = videoRatio;

		// If the videofeed is vertically mirrored flip it, else let it be.
		_image.uvRect = _phoneCamera.videoVerticallyMirrored ? new Rect(1, 0, -1, 1) : new Rect(0, 0, 1, 1);
	}

	private void OnDestroy() {
		// Stop the camera when the object is destroyed (scene change). Without this, the app will crash because the camera keeps sending stuff to the app.
		_phoneCamera.Stop();
	}
}
