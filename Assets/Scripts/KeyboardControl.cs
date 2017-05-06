using UnityEngine;

/// <summary>
///     Rotates the camera using directional input (Arrow keys, WASD, Joypad, etc)
///     Uses only the horizontal axis to avoid weird rotation
/// </summary>
public class KeyboardControl : MonoBehaviour {
	private const float Speed = 100;

	// Update is called once per frame
	private void Update() {
		Vector3 inputVector = new Vector3(0, Input.GetAxis("Horizontal"));
		transform.Rotate(inputVector * Speed * Time.deltaTime);
	}
}