// From author: Caue Rego (cawas)
// wiki.unity3d.com/index.php/DetectTouchMovement
// Modified names to fit naming conventions.

using UnityEngine;

public class DetectTouchMovement : MonoBehaviour {
	private const float PinchTurnRatio = Mathf.PI / 2;
	private const float MinTurnAngle = 0;

	private const float PinchRatio = 1;
	private const float MinPinchDistance = 0;

	/// <summary>
	///     The delta of the angle between two touch points
	/// </summary>
	public static float TurnAngleDelta;

	/// <summary>
	///     The angle between two touch points
	/// </summary>
	public static float TurnAngle;

	/// <summary>
	///     The delta of the distance between two touch points that were distancing from each other
	/// </summary>
	public static float PinchDistanceDelta;

	/// <summary>
	///     The distance between two touch points that were distancing from each other
	/// </summary>
	public static float PinchDistance;

	/// <summary>
	///     Calculates Pinch and Turn - This should be used inside LateUpdate
	/// </summary>
	public static void Calculate() {
		PinchDistance = PinchDistanceDelta = 0;
		TurnAngle = TurnAngleDelta = 0;

		// if two fingers are touching the screen at the same time ...
		if (Input.touchCount != 2)
			return;

		Touch touch1 = Input.touches[0];
		Touch touch2 = Input.touches[1];

		// ... if at least one of them moved ...
		if (touch1.phase != TouchPhase.Moved && touch2.phase != TouchPhase.Moved)
			return;
		// ... check the delta distance between them ...
		PinchDistance = Vector2.Distance(touch1.position, touch2.position);
		float prevDistance = Vector2.Distance(touch1.position - touch1.deltaPosition,
			touch2.position - touch2.deltaPosition);
		PinchDistanceDelta = PinchDistance - prevDistance;

		// ... if it's greater than a minimum threshold, it's a pinch!
		if (Mathf.Abs(PinchDistanceDelta) > MinPinchDistance)
			PinchDistanceDelta *= PinchRatio;
		else
			PinchDistance = PinchDistanceDelta = 0;

		// ... or check the delta angle between them ...
		TurnAngle = Angle(touch1.position, touch2.position);
		float prevTurn = Angle(touch1.position - touch1.deltaPosition,
			touch2.position - touch2.deltaPosition);
		TurnAngleDelta = Mathf.DeltaAngle(prevTurn, TurnAngle);

		// ... if it's greater than a minimum threshold, it's a turn!
		if (Mathf.Abs(TurnAngleDelta) > MinTurnAngle)
			TurnAngleDelta *= PinchTurnRatio;
		else
			TurnAngle = TurnAngleDelta = 0;
	}

	/// <summary>
	///     Calculates the angle between two Vector2's
	/// </summary>
	/// <param name="pos1">The first Vector2</param>
	/// <param name="pos2">The second Vector2</param>
	/// <returns>The angle between pos1 and pos2</returns>
	private static float Angle(Vector2 pos1, Vector2 pos2) {
		Vector2 from = pos2 - pos1;
		Vector2 to = new Vector2(1, 0);

		float result = Vector2.Angle(from, to);
		Vector3 cross = Vector3.Cross(from, to);

		if (cross.z > 0)
			result = 360f - result;

		return result;
	}
}