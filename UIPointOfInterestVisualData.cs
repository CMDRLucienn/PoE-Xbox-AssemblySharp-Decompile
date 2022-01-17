using UnityEngine;

public class UIPointOfInterestVisualData : ScriptableObject
{
	public enum PointOfInterestType
	{
		TransitionNormal,
		TransitionMap,
		TransitionDoor,
		Monument,
		UserNote,
		Generic,
		Companion,
		Player,
		None
	}

	public PointOfInterestType Type;

	public Texture2D Icon;

	public Texture2D VisitedIcon;

	public Texture2D XpIcon;

	public Texture2D Arrow;

	public Vector2 IconSize;

	public Vector2 ArrowSize;

	public bool AlwaysShowText;

	public bool UseFriendlyColorForIcon;
}
