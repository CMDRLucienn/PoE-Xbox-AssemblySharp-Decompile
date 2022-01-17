using System;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Slider")]
public class UISlider : IgnoreTimeScale
{
	public enum Direction
	{
		Horizontal,
		Vertical
	}

	public delegate void OnValueChange(float val);

	public static UISlider current;

	public Transform foreground;

	public Transform thumb;

	public Direction direction;

	public GameObject eventReceiver;

	public string functionName = "OnSliderChange";

	public OnValueChange onValueChange;

	public int numberOfSteps;

	[HideInInspector]
	[SerializeField]
	private float rawValue = 1f;

	private BoxCollider mCol;

	private Transform mTrans;

	private Transform mFGTrans;

	private UIWidget mFGWidget;

	private UISprite mFGFilled;

	private bool mInitDone;

	private Vector2 mSize = Vector2.zero;

	private Vector2 mCenter = Vector3.zero;

	public float sliderValue
	{
		get
		{
			float num = rawValue;
			if (numberOfSteps > 1)
			{
				num = Mathf.Round(num * (float)(numberOfSteps - 1)) / (float)(numberOfSteps - 1);
			}
			return num;
		}
		set
		{
			Set(value, force: false);
		}
	}

	public Vector2 fullSize
	{
		get
		{
			return mSize;
		}
		set
		{
			if (mSize != value)
			{
				mSize = value;
				ForceUpdate();
			}
		}
	}

	private void Init()
	{
		mInitDone = true;
		if (foreground != null)
		{
			mFGWidget = foreground.GetComponent<UIWidget>();
			mFGFilled = ((mFGWidget != null) ? (mFGWidget as UISprite) : null);
			mFGTrans = foreground.transform;
			if (mSize == Vector2.zero)
			{
				mSize = foreground.localScale;
			}
			if (mCenter == Vector2.zero)
			{
				mCenter = foreground.localPosition + foreground.localScale * 0.5f;
			}
		}
		else if (mCol != null)
		{
			if (mSize == Vector2.zero)
			{
				mSize = mCol.size;
			}
			if (mCenter == Vector2.zero)
			{
				mCenter = mCol.center;
			}
		}
		else
		{
			Debug.LogWarning("UISlider expected to find a foreground object or a box collider to work with", this);
		}
	}

	private void Awake()
	{
		mTrans = base.transform;
		mCol = GetComponent<Collider>() as BoxCollider;
	}

	private void Start()
	{
		Init();
		if (Application.isPlaying && thumb != null && thumb.GetComponent<Collider>() != null)
		{
			UIEventListener uIEventListener = UIEventListener.Get(thumb.gameObject);
			uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, new UIEventListener.BoolDelegate(OnPressThumb));
			uIEventListener.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener.onDrag, new UIEventListener.VectorDelegate(OnDragThumb));
		}
		Set(rawValue, force: true);
	}

	private void OnDestroy()
	{
		if (current == this)
		{
			current = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnPress(bool pressed)
	{
		if (base.enabled && pressed && UICamera.currentTouchID != -100)
		{
			UpdateDrag();
		}
	}

	private void OnDrag(Vector2 delta)
	{
		if (base.enabled)
		{
			UpdateDrag();
		}
	}

	private void OnPressThumb(GameObject go, bool pressed)
	{
		if (base.enabled && pressed)
		{
			UpdateDrag();
		}
	}

	private void OnDragThumb(GameObject go, Vector2 delta)
	{
		if (base.enabled)
		{
			UpdateDrag();
		}
	}

	private void OnKey(KeyCode key)
	{
		if (!base.enabled)
		{
			return;
		}
		float num = (((float)numberOfSteps > 1f) ? (1f / (float)(numberOfSteps - 1)) : 0.125f);
		if (direction == Direction.Horizontal)
		{
			switch (key)
			{
			case KeyCode.LeftArrow:
				Set(rawValue - num, force: false);
				break;
			case KeyCode.RightArrow:
				Set(rawValue + num, force: false);
				break;
			}
		}
		else
		{
			switch (key)
			{
			case KeyCode.DownArrow:
				Set(rawValue - num, force: false);
				break;
			case KeyCode.UpArrow:
				Set(rawValue + num, force: false);
				break;
			}
		}
	}

	private void UpdateDrag()
	{
		if (!(mCol == null) && !(UICamera.currentCamera == null) && UICamera.currentTouch != null)
		{
			UICamera.currentTouch.clickNotification = UICamera.ClickNotification.None;
			Ray ray = UICamera.currentCamera.ScreenPointToRay(UICamera.currentTouch.pos);
			if (new Plane(mTrans.rotation * Vector3.back, mTrans.position).Raycast(ray, out var enter))
			{
				Vector3 vector = mTrans.localPosition + (Vector3)(mCenter - mSize * 0.5f);
				Vector3 vector2 = mTrans.localPosition - vector;
				Vector3 vector3 = mTrans.InverseTransformPoint(ray.GetPoint(enter)) + vector2;
				Set((direction == Direction.Horizontal) ? (vector3.x / mSize.x) : (vector3.y / mSize.y), force: false);
			}
		}
	}

	private void Set(float input, bool force)
	{
		if (!mInitDone)
		{
			Init();
		}
		float num = Mathf.Clamp01(input);
		if (num < 0.001f)
		{
			num = 0f;
		}
		float num2 = sliderValue;
		rawValue = num;
		float num3 = sliderValue;
		if (!force && num2 == num3)
		{
			return;
		}
		Vector3 localScale = mSize;
		if (direction == Direction.Horizontal)
		{
			localScale.x *= num3;
		}
		else
		{
			localScale.y *= num3;
		}
		if (mFGFilled != null && mFGFilled.type == UISprite.Type.Filled)
		{
			mFGFilled.fillAmount = num3;
		}
		else if (foreground != null)
		{
			mFGTrans.localScale = localScale;
			if (mFGWidget != null)
			{
				if (num3 > 0.001f)
				{
					mFGWidget.enabled = true;
					mFGWidget.MarkAsChanged();
				}
				else
				{
					mFGWidget.enabled = false;
				}
			}
		}
		if (thumb != null)
		{
			Vector3 localPosition = thumb.localPosition;
			if (mFGFilled != null && mFGFilled.type == UISprite.Type.Filled)
			{
				if (mFGFilled.fillDirection == UISprite.FillDirection.Horizontal)
				{
					localPosition.x = (mFGFilled.invert ? (mSize.x - localScale.x) : localScale.x);
				}
				else if (mFGFilled.fillDirection == UISprite.FillDirection.Vertical)
				{
					localPosition.y = (mFGFilled.invert ? (mSize.y - localScale.y) : localScale.y);
				}
				else
				{
					Debug.LogWarning("Slider thumb is only supported with Horizontal or Vertical fill direction", this);
				}
			}
			else if (direction == Direction.Horizontal)
			{
				localPosition.x = localScale.x;
			}
			else
			{
				localPosition.y = localScale.y;
			}
			thumb.localPosition = localPosition;
		}
		current = this;
		if (eventReceiver != null && !string.IsNullOrEmpty(functionName) && Application.isPlaying)
		{
			eventReceiver.SendMessage(functionName, num3, SendMessageOptions.DontRequireReceiver);
		}
		if (onValueChange != null)
		{
			onValueChange(num3);
		}
		current = null;
	}

	public void ForceUpdate()
	{
		Set(rawValue, force: true);
	}
}