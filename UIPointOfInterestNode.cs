using System;
using UnityEngine;

public class UIPointOfInterestNode : MonoBehaviour
{
	public UITexture Icon;

	public UITexture Arrow;

	public UILabel HoverText;

	public PointOfInterest PointOfInterestReference;

	public UIPointOfInterestVisualData Visuals;

	private const float ARROW_OFFSET = 0.15f;

	private const float ARROW_DEFAULT_ANGLE = 90f;

	private static bool sOffsetIconsBasedOnArrowDirection;

	private static bool sShowArrows;

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void SetUpIcons(PointOfInterest poi, Vector3 position)
	{
		UIAnchor component = Arrow.GetComponent<UIAnchor>();
		UIAnchor component2 = HoverText.GetComponent<UIAnchor>();
		Shader shader = Shader.Find("Unlit/Transparent Colored");
		UIPointOfInterestVisualData visuals = Visuals;
		if (!poi || !visuals)
		{
			UIDebug.Instance.LogOnScreenWarning("No POI visuals attached to the POI on object: " + poi.gameObject.name, UIDebug.Department.Design, 10f);
		}
		if (Arrow != null)
		{
			Material material = new Material(shader);
			Arrow.material = material;
			Arrow.color = Color.white;
			if (visuals != null)
			{
				Arrow.mainTexture = visuals.Arrow;
				Arrow.MakePixelPerfect();
				Arrow.transform.localScale = new Vector3(visuals.ArrowSize.x, visuals.ArrowSize.y, 1f);
			}
		}
		Material material2 = new Material(shader);
		Icon.material = material2;
		if ((bool)poi.Visuals)
		{
			Icon.mainTexture = ((poi.GivesDiscoveryXp && (bool)visuals.XpIcon) ? visuals.XpIcon : visuals.Icon);
			Icon.MakePixelPerfect();
			Icon.transform.localScale = new Vector3(visuals.IconSize.x, visuals.IconSize.y, 1f);
		}
		component.side = UIAnchor.Side.Center;
		component.relativeOffset = new Vector2(0f, 0f);
		if ((bool)Arrow && !sShowArrows)
		{
			Arrow.gameObject.SetActive(value: false);
		}
		if (component != null)
		{
			switch (poi.ArrowDirection)
			{
			case PointOfInterest.PointDirection.SOUTH_WEST:
				if (sOffsetIconsBasedOnArrowDirection)
				{
					component.side = UIAnchor.Side.BottomLeft;
					component.relativeOffset = new Vector2(0.15f, 0.15f);
					Icon.transform.localPosition = new Vector3(Icon.transform.localPosition.x + Icon.transform.localScale.x / 2f, Icon.transform.localPosition.y + Icon.transform.localScale.y / 2f, Icon.transform.localPosition.z);
				}
				Arrow.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 135f));
				HoverText.pivot = UIWidget.Pivot.Left;
				component2.side = UIAnchor.Side.Right;
				break;
			case PointOfInterest.PointDirection.SOUTH_EAST:
				if (sOffsetIconsBasedOnArrowDirection)
				{
					component.side = UIAnchor.Side.BottomRight;
					component.relativeOffset = new Vector2(-0.15f, 0.15f);
					Icon.transform.localPosition = new Vector3(Icon.transform.localPosition.x - Icon.transform.localScale.x / 2f, Icon.transform.localPosition.y + Icon.transform.localScale.y / 2f, Icon.transform.localPosition.z);
				}
				Arrow.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 225f));
				HoverText.pivot = UIWidget.Pivot.Right;
				component2.side = UIAnchor.Side.Left;
				break;
			case PointOfInterest.PointDirection.NORTH_EAST:
				if (sOffsetIconsBasedOnArrowDirection)
				{
					component.side = UIAnchor.Side.TopRight;
					component.relativeOffset = new Vector2(-0.15f, -0.15f);
					Icon.transform.localPosition = new Vector3(Icon.transform.localPosition.x - Icon.transform.localScale.x / 2f, Icon.transform.localPosition.y - Icon.transform.localScale.y / 2f, Icon.transform.localPosition.z);
				}
				Arrow.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, -45f));
				HoverText.pivot = UIWidget.Pivot.Right;
				component2.side = UIAnchor.Side.Left;
				break;
			case PointOfInterest.PointDirection.NORTH_WEST:
				if (sOffsetIconsBasedOnArrowDirection)
				{
					component.side = UIAnchor.Side.TopLeft;
					component.relativeOffset = new Vector2(0.15f, -0.15f);
					Icon.transform.localPosition = new Vector3(Icon.transform.localPosition.x + Icon.transform.localScale.x / 2f, Icon.transform.localPosition.y - Icon.transform.localScale.y / 2f, Icon.transform.localPosition.z);
				}
				Arrow.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 45f));
				HoverText.pivot = UIWidget.Pivot.Left;
				component2.side = UIAnchor.Side.Right;
				break;
			case PointOfInterest.PointDirection.NORTH:
				if (sOffsetIconsBasedOnArrowDirection)
				{
					component.side = UIAnchor.Side.Top;
					component.relativeOffset = new Vector2(0f, 0f);
					Icon.transform.localPosition = new Vector3(Icon.transform.localPosition.x, Icon.transform.localPosition.y - Icon.transform.localScale.y / 2f, Icon.transform.localPosition.z);
				}
				Arrow.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
				HoverText.pivot = UIWidget.Pivot.Top;
				component2.side = UIAnchor.Side.Bottom;
				break;
			case PointOfInterest.PointDirection.WEST:
				if (sOffsetIconsBasedOnArrowDirection)
				{
					component.side = UIAnchor.Side.Left;
					component.relativeOffset = new Vector2(0f, 0f);
					Icon.transform.localPosition = new Vector3(Icon.transform.localPosition.x + Icon.transform.localScale.x / 2f, Icon.transform.localPosition.y, Icon.transform.localPosition.z);
				}
				Arrow.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));
				HoverText.pivot = UIWidget.Pivot.Left;
				component2.side = UIAnchor.Side.Right;
				break;
			case PointOfInterest.PointDirection.SOUTH:
				if (sOffsetIconsBasedOnArrowDirection)
				{
					component.side = UIAnchor.Side.Bottom;
					component.relativeOffset = new Vector2(0f, 0f);
					Icon.transform.localPosition = new Vector3(Icon.transform.localPosition.x, Icon.transform.localPosition.y + Icon.transform.localScale.y / 2f, Icon.transform.localPosition.z);
				}
				Arrow.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 180f));
				HoverText.pivot = UIWidget.Pivot.Bottom;
				component2.side = UIAnchor.Side.Top;
				break;
			case PointOfInterest.PointDirection.EAST:
				if (sOffsetIconsBasedOnArrowDirection)
				{
					component.side = UIAnchor.Side.Right;
					component.relativeOffset = new Vector2(0f, 0f);
					Icon.transform.localPosition = new Vector3(Icon.transform.localPosition.x - Icon.transform.localScale.x / 2f, Icon.transform.localPosition.y, Icon.transform.localPosition.z);
				}
				Arrow.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 270f));
				HoverText.pivot = UIWidget.Pivot.Right;
				component2.side = UIAnchor.Side.Left;
				break;
			case PointOfInterest.PointDirection.CENTER:
				if (position.x < 0f)
				{
					HoverText.pivot = UIWidget.Pivot.Left;
					component2.side = UIAnchor.Side.Right;
				}
				else
				{
					HoverText.pivot = UIWidget.Pivot.Right;
					component2.side = UIAnchor.Side.Left;
				}
				if ((bool)Arrow)
				{
					Arrow.gameObject.SetActive(value: false);
				}
				break;
			}
		}
		if ((bool)visuals && (visuals.Type == UIPointOfInterestVisualData.PointOfInterestType.Companion || poi.Visuals.Type == UIPointOfInterestVisualData.PointOfInterestType.Player))
		{
			HoverText.text = CharacterStats.Name(poi.gameObject);
		}
		else
		{
			HoverText.text = poi.DBText.GetText();
		}
		if ((bool)visuals && visuals.UseFriendlyColorForIcon)
		{
			Icon.color = InGameHUD.GetFriendlyColor();
		}
		if ((bool)visuals && visuals.AlwaysShowText)
		{
			HoverText.gameObject.SetActive(value: true);
		}
		HoverText.effectStyle = UILabel.Effect.Shadow;
		base.gameObject.transform.localPosition = position;
	}

	private void OnHover(GameObject obj, bool isOver)
	{
		OnHover(isOver);
	}

	private void OnHover(bool isOver)
	{
		if (!Visuals.AlwaysShowText)
		{
			HoverText.gameObject.SetActive(isOver);
		}
	}

	private void OnClick(GameObject obj)
	{
		OnClick();
	}

	private void OnClick()
	{
		(UnityEngine.Object.FindObjectOfType(typeof(UIAreaMap)) as UIAreaMap).FocusCameraOnPointer();
	}

	private void OnDoubleClick()
	{
		UIAreaMapManager.Instance.HideWindow();
	}

	private void OnDoubleClick(GameObject obj)
	{
		OnDoubleClick();
	}

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(Icon.gameObject);
		uIEventListener.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onHover, new UIEventListener.BoolDelegate(OnHover));
		UIEventListener uIEventListener2 = UIEventListener.Get(Icon.gameObject);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnClick));
		UIEventListener uIEventListener3 = UIEventListener.Get(Icon.gameObject);
		uIEventListener3.onDoubleClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onDoubleClick, new UIEventListener.VoidDelegate(OnDoubleClick));
	}
}
