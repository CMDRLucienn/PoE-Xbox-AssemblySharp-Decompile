using UnityEngine;

[AddComponentMenu("NGUI/Internal/Event Listener")]
public class UIEventListener : MonoBehaviour
{
	public delegate void VoidDelegate(GameObject go);

	public delegate void BoolDelegate(GameObject go, bool state);

	public delegate void FloatDelegate(GameObject go, float delta);

	public delegate void VectorDelegate(GameObject go, Vector2 delta);

	public delegate void StringDelegate(GameObject go, string text);

	public delegate void ObjectDelegate(GameObject go, GameObject draggedObject);

	public delegate void KeyCodeDelegate(GameObject go, KeyCode key);

	public object parameter;

	public VoidDelegate onSubmit;

	public VectorDelegate onDrag;

	public ObjectDelegate onDrop;

	public VoidDelegate onClick;

	public VoidDelegate onDoubleClick;

	public BoolDelegate onPress;

	public BoolDelegate onSelect;

	public VectorDelegate onRightDrag;

	public ObjectDelegate onRightDrop;

	public VoidDelegate onRightClick;

	public VoidDelegate onRightDoubleClick;

	public BoolDelegate onRightPress;

	public BoolDelegate onRightSelect;

	public VectorDelegate onOtherDrag;

	public ObjectDelegate onOtherDrop;

	public VoidDelegate onOtherClick;

	public VoidDelegate onOtherDoubleClick;

	public BoolDelegate onOtherPress;

	public BoolDelegate onOtherSelect;

	public BoolDelegate onHover;

	public FloatDelegate onScroll;

	public StringDelegate onInput;

	public KeyCodeDelegate onKey;

	public BoolDelegate onTooltip;

	private static UIEventListener s_LastListener;

	private void OnSubmit()
	{
		if (onSubmit != null)
		{
			onSubmit(base.gameObject);
		}
	}

	private void OnDrag(Vector2 delta)
	{
		if (onDrag != null)
		{
			onDrag(base.gameObject, delta);
		}
	}

	private void OnDrop(GameObject go)
	{
		if (onDrop != null)
		{
			onDrop(base.gameObject, go);
		}
	}

	private void OnClick()
	{
		if (onClick != null)
		{
			onClick(base.gameObject);
		}
	}

	private void OnDoubleClick()
	{
		if (onDoubleClick != null)
		{
			onDoubleClick(base.gameObject);
		}
	}

	private void OnPress(bool isPressed)
	{
		if (onPress != null)
		{
			onPress(base.gameObject, isPressed);
		}
	}

	private void OnSelect(bool selected)
	{
		if (onSelect != null)
		{
			onSelect(base.gameObject, selected);
		}
	}

	private void OnRightDrag(Vector2 delta)
	{
		if (onRightDrag != null)
		{
			onRightDrag(base.gameObject, delta);
		}
	}

	private void OnRightDrop(GameObject go)
	{
		if (onRightDrop != null)
		{
			onRightDrop(base.gameObject, go);
		}
	}

	private void OnRightClick()
	{
		if (onRightClick != null)
		{
			onRightClick(base.gameObject);
		}
	}

	private void OnRightDoubleClick()
	{
		if (onRightDoubleClick != null)
		{
			onRightDoubleClick(base.gameObject);
		}
	}

	private void OnRightPress(bool isPressed)
	{
		if (onRightPress != null)
		{
			onRightPress(base.gameObject, isPressed);
		}
	}

	private void OnRightSelect(bool selected)
	{
		if (onRightSelect != null)
		{
			onRightSelect(base.gameObject, selected);
		}
	}

	private void OnOtherDrag(Vector2 delta)
	{
		if (onOtherDrag != null)
		{
			onOtherDrag(base.gameObject, delta);
		}
	}

	private void OnOtherDrop(GameObject go)
	{
		if (onOtherDrop != null)
		{
			onOtherDrop(base.gameObject, go);
		}
	}

	private void OnOtherClick()
	{
		if (onOtherClick != null)
		{
			onOtherClick(base.gameObject);
		}
	}

	private void OnOtherDoubleClick()
	{
		if (onOtherDoubleClick != null)
		{
			onOtherDoubleClick(base.gameObject);
		}
	}

	private void OnOtherPress(bool isPressed)
	{
		if (onOtherPress != null)
		{
			onOtherPress(base.gameObject, isPressed);
		}
	}

	private void OnOtherSelect(bool selected)
	{
		if (onOtherSelect != null)
		{
			onOtherSelect(base.gameObject, selected);
		}
	}

	private void OnHover(bool isOver)
	{
		if (onHover != null)
		{
			onHover(base.gameObject, isOver);
		}
	}

	private void OnScroll(float delta)
	{
		if (onScroll != null)
		{
			onScroll(base.gameObject, delta);
		}
	}

	private void OnInput(string text)
	{
		if (onInput != null)
		{
			onInput(base.gameObject, text);
		}
	}

	private void OnKey(KeyCode key)
	{
		if (onKey != null)
		{
			onKey(base.gameObject, key);
		}
	}

	private void OnTooltip(bool show)
	{
		if (onTooltip != null)
		{
			onTooltip(base.gameObject, show);
		}
	}

	public static UIEventListener Get(GameObject go)
	{
		if ((bool)s_LastListener && s_LastListener.gameObject == go)
		{
			return s_LastListener;
		}
		s_LastListener = go.GetComponent<UIEventListener>();
		if (s_LastListener == null)
		{
			s_LastListener = go.AddComponent<UIEventListener>();
		}
		return s_LastListener;
	}

	public static UIEventListener Get(Component mb)
	{
		return Get(mb.gameObject);
	}
}
