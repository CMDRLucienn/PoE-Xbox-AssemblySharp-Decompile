using System;
using UnityEngine;

public class UIColorPairSelector : MonoBehaviour
{
	public ColorListManager.ColorPickerType PickerTypeTop;

	public ColorListManager.ColorPickerType PickerTypeBottom;

	public CharacterStats.Subrace Subrace;

	public UIColorSelectorLine SelectorLine;

	public GameObject Collider;

	public UIWidget TopSprite;

	public UIWidget BottomSprite;

	public bool PickEnabled = true;

	private NPCAppearance m_Appearance;

	private bool m_ShowingTop;

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(Collider);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClick));
		if (SelectorLine != null)
		{
			UIColorSelectorLine selectorLine = SelectorLine;
			selectorLine.OnColorChanged = (UIColorSelectorLine.ColorChanged)Delegate.Combine(selectorLine.OnColorChanged, new UIColorSelectorLine.ColorChanged(OnColorChanged));
		}
	}

	private void OnColorChanged(Color color)
	{
		if (!(SelectorLine.CurrentTarget != base.gameObject))
		{
			if (m_ShowingTop)
			{
				TopSprite.color = color;
				SetColorFor(m_Appearance, PickerTypeTop, color);
			}
			else
			{
				BottomSprite.color = color;
				SetColorFor(m_Appearance, PickerTypeBottom, color);
			}
		}
	}

	private void OnClick(GameObject go)
	{
		if (PickEnabled)
		{
			SelectorLine.transform.localPosition = new Vector3(base.transform.localPosition.x - TopSprite.transform.localScale.x / 2f, base.transform.localPosition.y + TopSprite.transform.localScale.y / 2f, SelectorLine.transform.localPosition.z);
			Vector3 point = InGameUILayout.NGUICamera.ScreenToWorldPoint(GameInput.MousePosition);
			Vector3 vector = base.transform.worldToLocalMatrix.MultiplyPoint3x4(point);
			if (vector.x - vector.y < 0f)
			{
				m_ShowingTop = true;
				SelectorLine.Show(ColorListManager.GetColorList(PickerTypeTop, Subrace), TopSprite.color);
			}
			else
			{
				m_ShowingTop = false;
				SelectorLine.Show(ColorListManager.GetColorList(PickerTypeBottom, Subrace), BottomSprite.color);
			}
			SelectorLine.CurrentTarget = base.gameObject;
		}
	}

	public void Set(CharacterStats character)
	{
		Subrace = character.CharacterSubrace;
		m_Appearance = character.GetComponent<NPCAppearance>();
		if (m_Appearance == null)
		{
			Debug.LogError("Inventory tried to load a character that had no NPCAppearance component.");
		}
		TopSprite.color = GetColorFor(m_Appearance, PickerTypeTop);
		BottomSprite.color = GetColorFor(m_Appearance, PickerTypeBottom);
	}

	public static Color GetRandomColor(ColorListManager.ColorPickerType ptype, CharacterStats.Subrace subrace)
	{
		ColorList colorList = ColorListManager.GetColorList(ptype, subrace);
		if (colorList == null)
		{
			return Color.black;
		}
		return colorList.GetRandomColor();
	}

	public static Color GetColorFor(NPCAppearance appearance, ColorListManager.ColorPickerType type)
	{
		return type switch
		{
			ColorListManager.ColorPickerType.Hair => appearance.hairColor, 
			ColorListManager.ColorPickerType.Major => appearance.primaryColor, 
			ColorListManager.ColorPickerType.Minor => appearance.secondaryColor, 
			ColorListManager.ColorPickerType.Skin => appearance.skinColor, 
			_ => Color.white, 
		};
	}

	public static void SetColorFor(NPCAppearance appearance, ColorListManager.ColorPickerType type, Color color)
	{
		switch (type)
		{
		case ColorListManager.ColorPickerType.Hair:
			appearance.hairColor = color;
			break;
		case ColorListManager.ColorPickerType.Major:
			appearance.primaryColor = color;
			break;
		case ColorListManager.ColorPickerType.Minor:
			appearance.secondaryColor = color;
			break;
		case ColorListManager.ColorPickerType.Skin:
			appearance.skinColor = color;
			break;
		}
		appearance.ApplyTints();
	}
}
