using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Image Button")]
public class UIImageButton : MonoBehaviour
{
	public UISprite target;

	public string normalSprite;

	public string hoverSprite;

	public string pressedSprite;

	public string disabledSprite;

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
				UpdateImage();
			}
		}
	}

	private void Awake()
	{
		if (target == null)
		{
			target = GetComponentInChildren<UISprite>();
		}
	}

	private void OnEnable()
	{
		UpdateImage();
	}

	private void UpdateImage()
	{
		if (target != null)
		{
			if (isEnabled)
			{
				target.spriteName = (UICamera.IsHighlighted(base.gameObject) ? hoverSprite : normalSprite);
			}
			else
			{
				target.spriteName = disabledSprite;
			}
			target.MakePixelPerfect();
		}
	}

	private void OnHover(bool isOver)
	{
		if (isEnabled && target != null)
		{
			target.spriteName = (isOver ? hoverSprite : normalSprite);
			target.MakePixelPerfect();
		}
	}

	private void OnPress(bool pressed)
	{
		if (pressed)
		{
			target.spriteName = pressedSprite;
			target.MakePixelPerfect();
		}
		else
		{
			UpdateImage();
		}
	}
}
