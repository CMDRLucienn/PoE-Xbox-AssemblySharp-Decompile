using System;
using UnityEngine;

public class UICharacterCreationColorSelector : UICharacterCreationElement
{
	public ColorListManager.ColorPickerType PickerType;

	public UIColorSelectorLine SelectorLine;

	public UILabel ColorLabel;

	public UIWidget Collider;

	public UIWidget ColorTarget;

	protected override void Start()
	{
		base.Start();
		UIEventListener uIEventListener = UIEventListener.Get(Collider.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClicked));
		if (SelectorLine != null)
		{
			UIColorSelectorLine selectorLine = SelectorLine;
			selectorLine.OnColorChanged = (UIColorSelectorLine.ColorChanged)Delegate.Combine(selectorLine.OnColorChanged, new UIColorSelectorLine.ColorChanged(OnColorChanged));
		}
		RefreshColor();
	}

	private void Awake()
	{
		RefreshColor();
	}

	public void OnEnable()
	{
		RefreshColor();
	}

	private void RefreshColor()
	{
		bool enable = true;
		switch (PickerType)
		{
		case ColorListManager.ColorPickerType.Hair:
			if (ColorListManager.GetColorList(PickerType, base.Owner.Character.Subrace) == null)
			{
				enable = false;
				if ((bool)ColorLabel)
				{
					ColorLabel.color = new Color(ColorLabel.color.r, ColorLabel.color.g, ColorLabel.color.b, 0.3f);
				}
				ColorTarget.color = new Color(1f, 1f, 1f, 0.3f);
			}
			else
			{
				if ((bool)ColorLabel)
				{
					ColorLabel.color = new Color(ColorLabel.color.r, ColorLabel.color.g, ColorLabel.color.b, 1f);
				}
				ColorTarget.color = base.Owner.Character.HairColor;
			}
			break;
		case ColorListManager.ColorPickerType.Major:
			ColorTarget.color = base.Owner.Character.MajorColor;
			break;
		case ColorListManager.ColorPickerType.Minor:
			ColorTarget.color = base.Owner.Character.MinorColor;
			break;
		case ColorListManager.ColorPickerType.Skin:
			if (ColorListManager.GetColorList(PickerType, base.Owner.Character.Subrace) == null)
			{
				enable = false;
				if ((bool)ColorLabel)
				{
					ColorLabel.color = new Color(ColorLabel.color.r, ColorLabel.color.g, ColorLabel.color.b, 0.3f);
				}
				ColorTarget.color = new Color(1f, 1f, 1f, 0.3f);
			}
			else
			{
				if ((bool)ColorLabel)
				{
					ColorLabel.color = new Color(ColorLabel.color.r, ColorLabel.color.g, ColorLabel.color.b, 1f);
				}
				ColorTarget.color = base.Owner.Character.SkinColor;
			}
			break;
		}
		if ((bool)ColorLabel)
		{
			EnableSelectorElement(ColorLabel, enable);
		}
		if ((bool)ColorTarget)
		{
			EnableSelectorElement(ColorTarget, enable);
		}
		if ((bool)Collider)
		{
			EnableSelectorElement(Collider, enable);
			UIImageButtonRevised component = Collider.gameObject.GetComponent<UIImageButtonRevised>();
			if ((bool)component)
			{
				component.enabled = enable;
			}
		}
		EnableSelectorElement(base.gameObject.GetComponent<UISprite>(), enable);
	}

	private void EnableSelectorElement(UIWidget element, bool enable)
	{
		if ((bool)element)
		{
			element.alpha = (enable ? 1f : 0f);
		}
	}

	public bool IsSelectorEnabled()
	{
		return base.gameObject.GetComponent<UISprite>().alpha > 0f;
	}

	public override void SignalValueChanged(ValueType type)
	{
		if (type == ValueType.Color || type == ValueType.All)
		{
			RefreshColor();
		}
	}

	private void OnColorChanged(Color color)
	{
		if (!(SelectorLine.CurrentTarget != base.gameObject))
		{
			ColorTarget.color = color;
			switch (PickerType)
			{
			case ColorListManager.ColorPickerType.Hair:
				base.Owner.Character.HairColor = color;
				break;
			case ColorListManager.ColorPickerType.Major:
				base.Owner.Character.MajorColor = color;
				break;
			case ColorListManager.ColorPickerType.Minor:
				base.Owner.Character.MinorColor = color;
				break;
			case ColorListManager.ColorPickerType.Skin:
				base.Owner.Character.SkinColor = color;
				break;
			}
			base.Owner.SignalValueChanged(ValueType.Color);
		}
	}

	private void OnClicked(GameObject go)
	{
		SelectorLine.transform.localPosition = new Vector3(base.transform.localPosition.x - Collider.transform.localScale.x / 2f, base.transform.localPosition.y + Collider.transform.localScale.y / 2f, SelectorLine.transform.localPosition.z);
		SelectorLine.Show(ColorListManager.GetColorList(PickerType, base.Owner.Character.Subrace), ColorTarget.color);
		SelectorLine.CurrentTarget = base.gameObject;
	}
}
