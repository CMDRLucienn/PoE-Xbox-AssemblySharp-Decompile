using UnityEngine;

public abstract class UIScreenRectangleItem : MonoBehaviour, IClusterable
{
	public static bool DebugScreenRectangle;

	public Vector2 BasePosition;

	public Vector2 CorrectingOffset;

	public Vector2 ScreenPosition => CorrectingOffset + BasePosition;

	public Vector2 GetPosition()
	{
		return BasePosition;
	}

	public abstract Rect GetScreenBounds();
}
