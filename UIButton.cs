using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Button")]
public class UIButton : UIButtonColor
{
	public Color disabledColor = Color.grey;

	public bool isEnabled
	{
		get
		{
			Collider component = GetComponent<Collider>();
			if ((bool)component)
			{
				return component.enabled;
			}
			return false;
		}
		set
		{
			Collider component = GetComponent<Collider>();
			if ((bool)component && component.enabled != value)
			{
				component.enabled = value;
				UpdateColor(value, immediate: false);
			}
		}
	}

	protected override void OnEnable()
	{
		if (isEnabled)
		{
			base.OnEnable();
		}
		else
		{
			UpdateColor(shouldBeEnabled: false, immediate: true);
		}
	}

	public override void OnHover(bool isOver)
	{
		if (isEnabled)
		{
			base.OnHover(isOver);
		}
	}

	public override void OnPress(bool isPressed)
	{
		if (isEnabled)
		{
			base.OnPress(isPressed);
		}
	}

	public void UpdateColor(bool shouldBeEnabled, bool immediate)
	{
		if ((bool)tweenTarget)
		{
			if (!mStarted)
			{
				mStarted = true;
				Init();
			}
			Color color = (shouldBeEnabled ? base.defaultColor : disabledColor);
			TweenColor tweenColor = TweenColor.Begin(tweenTarget, 0.15f, color);
			if (immediate)
			{
				tweenColor.color = color;
				tweenColor.enabled = false;
			}
		}
	}
}
