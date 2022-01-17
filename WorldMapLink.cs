using System;

[Serializable]
public class WorldMapLink
{
	public UIWorldMapIcon Place1;

	public UIWorldMapIcon Place2;

	public StartPoint.PointLocation OverrideStart1;

	public StartPoint.PointLocation OverrideStart2;

	public float TravelTimeHours = -1f;
}
