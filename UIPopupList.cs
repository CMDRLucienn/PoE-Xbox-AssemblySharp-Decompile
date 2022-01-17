using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Popup List")]
public class UIPopupList : MonoBehaviour
{
	public enum Position
	{
		Auto,
		Above,
		Below
	}

	public delegate void OnSelectionChange(string item);

	public static UIPopupList current;

	private const float animSpeed = 0.15f;

	public UIAtlas atlas;

	public UIFont font;

	public UILabel textLabel;

	public string backgroundSprite;

	public string highlightSprite;

	public Position position;

	public List<string> items = new List<string>();

	public Vector2 padding = new Vector3(4f, 4f);

	public float textScale = 1f;

	public Color textColor = Color.white;

	public Color backgroundColor = Color.white;

	public Color highlightColor = new Color(0.596078455f, 1f, 0.2f, 1f);

	public bool isAnimated = true;

	public bool isLocalized;

	public GameObject eventReceiver;

	public string functionName = "OnSelectionChange";

	public bool sameOptionTrigger;

	public OnSelectionChange onSelectionChange;

	[HideInInspector]
	[SerializeField]
	private string mSelectedItem;

	private UIPanel mPanel;

	private GameObject mChild;

	private UISprite mBackground;

	private UISprite mHighlight;

	private UILabel mHighlightedLabel;

	private List<UILabel> mLabelList = new List<UILabel>();

	private float mBgBorder;

	public bool isOpen => mChild != null;

	public string selection
	{
		get
		{
			return mSelectedItem;
		}
		set
		{
			bool flag = false;
			if (mSelectedItem != value)
			{
				mSelectedItem = value;
				if (mSelectedItem == null)
				{
					return;
				}
				if (textLabel != null)
				{
					textLabel.text = (isLocalized ? Localization.Localize(value) : value);
				}
				flag = true;
			}
			if (mSelectedItem != null && (flag || sameOptionTrigger))
			{
				current = this;
				if (onSelectionChange != null)
				{
					onSelectionChange(mSelectedItem);
				}
				if (eventReceiver != null && !string.IsNullOrEmpty(functionName) && Application.isPlaying)
				{
					eventReceiver.SendMessage(functionName, mSelectedItem, SendMessageOptions.DontRequireReceiver);
				}
				current = null;
			}
		}
	}

	private bool handleEvents
	{
		get
		{
			UIButtonKeys component = GetComponent<UIButtonKeys>();
			if (!(component == null))
			{
				return !component.enabled;
			}
			return true;
		}
		set
		{
			UIButtonKeys component = GetComponent<UIButtonKeys>();
			if (component != null)
			{
				component.enabled = !value;
			}
		}
	}

	private void Start()
	{
		if (string.IsNullOrEmpty(mSelectedItem))
		{
			if (items.Count > 0)
			{
				selection = items[0];
			}
		}
		else
		{
			string text = mSelectedItem;
			mSelectedItem = null;
			selection = text;
		}
	}

	private void OnDestroy()
	{
		if (current == this)
		{
			current = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnLocalize(Localization loc)
	{
		if (isLocalized && textLabel != null)
		{
			textLabel.text = loc.Get(mSelectedItem);
		}
	}

	private void Highlight(UILabel lbl, bool instant)
	{
		if (!(mHighlight != null))
		{
			return;
		}
		TweenPosition component = lbl.GetComponent<TweenPosition>();
		if (component != null && component.enabled)
		{
			return;
		}
		mHighlightedLabel = lbl;
		UIAtlas.Sprite atlasSprite = mHighlight.GetAtlasSprite();
		if (atlasSprite != null)
		{
			float pixelSize = atlas.pixelSize;
			float num = (atlasSprite.inner.xMin - atlasSprite.outer.xMin) * pixelSize;
			float y = (atlasSprite.inner.yMin - atlasSprite.outer.yMin) * pixelSize;
			Vector3 vector = lbl.cachedTransform.localPosition + new Vector3(0f - num, y, 1f);
			if (instant || !isAnimated)
			{
				mHighlight.cachedTransform.localPosition = vector;
			}
			else
			{
				TweenPosition.Begin(mHighlight.gameObject, 0.1f, vector).method = UITweener.Method.EaseOut;
			}
		}
	}

	private void OnItemHover(GameObject go, bool isOver)
	{
		if (isOver)
		{
			UILabel component = go.GetComponent<UILabel>();
			Highlight(component, instant: false);
		}
	}

	private void Select(UILabel lbl, bool instant)
	{
		Highlight(lbl, instant);
		UIEventListener component = lbl.gameObject.GetComponent<UIEventListener>();
		selection = component.parameter as string;
		UIButtonSound[] components = GetComponents<UIButtonSound>();
		int i = 0;
		for (int num = components.Length; i < num; i++)
		{
			UIButtonSound uIButtonSound = components[i];
			if (uIButtonSound.trigger == UIButtonSound.Trigger.OnClick)
			{
				NGUITools.PlaySound(uIButtonSound.audioClip, uIButtonSound.volume, 1f);
			}
		}
	}

	private void OnItemPress(GameObject go, bool isPressed)
	{
		if (isPressed)
		{
			Select(go.GetComponent<UILabel>(), instant: true);
		}
	}

	private void OnKey(KeyCode key)
	{
		if (!base.enabled || !NGUITools.GetActive(base.gameObject) || !handleEvents)
		{
			return;
		}
		int num = mLabelList.IndexOf(mHighlightedLabel);
		switch (key)
		{
		case KeyCode.UpArrow:
			if (num > 0)
			{
				Select(mLabelList[--num], instant: false);
			}
			break;
		case KeyCode.DownArrow:
			if (num + 1 < mLabelList.Count)
			{
				Select(mLabelList[++num], instant: false);
			}
			break;
		case KeyCode.Escape:
			OnSelect(isSelected: false);
			break;
		}
	}

	private void OnSelect(bool isSelected)
	{
		if (isSelected || !(mChild != null))
		{
			return;
		}
		mLabelList.Clear();
		handleEvents = false;
		if (isAnimated)
		{
			UIWidget[] componentsInChildren = mChild.GetComponentsInChildren<UIWidget>();
			int i = 0;
			for (int num = componentsInChildren.Length; i < num; i++)
			{
				UIWidget obj = componentsInChildren[i];
				Color color = obj.color;
				color.a = 0f;
				TweenColor.Begin(obj.gameObject, 0.15f, color).method = UITweener.Method.EaseOut;
			}
			Collider[] componentsInChildren2 = mChild.GetComponentsInChildren<Collider>();
			int j = 0;
			for (int num2 = componentsInChildren2.Length; j < num2; j++)
			{
				componentsInChildren2[j].enabled = false;
			}
			Object.Destroy(mChild, 0.15f);
		}
		else
		{
			Object.Destroy(mChild);
		}
		mBackground = null;
		mHighlight = null;
		mChild = null;
	}

	private void AnimateColor(UIWidget widget)
	{
		Color color = widget.color;
		widget.color = new Color(color.r, color.g, color.b, 0f);
		TweenColor.Begin(widget.gameObject, 0.15f, color).method = UITweener.Method.EaseOut;
	}

	private void AnimatePosition(UIWidget widget, bool placeAbove, float bottom)
	{
		Vector3 localPosition = widget.cachedTransform.localPosition;
		Vector3 localPosition2 = (placeAbove ? new Vector3(localPosition.x, bottom, localPosition.z) : new Vector3(localPosition.x, 0f, localPosition.z));
		widget.cachedTransform.localPosition = localPosition2;
		TweenPosition.Begin(widget.gameObject, 0.15f, localPosition).method = UITweener.Method.EaseOut;
	}

	private void AnimateScale(UIWidget widget, bool placeAbove, float bottom)
	{
		GameObject go = widget.gameObject;
		Transform cachedTransform = widget.cachedTransform;
		float num = (float)font.size * textScale + mBgBorder * 2f;
		Vector3 localScale = cachedTransform.localScale;
		cachedTransform.localScale = new Vector3(localScale.x, num, localScale.z);
		TweenScale.Begin(go, 0.15f, localScale).method = UITweener.Method.EaseOut;
		if (placeAbove)
		{
			Vector3 localPosition = cachedTransform.localPosition;
			cachedTransform.localPosition = new Vector3(localPosition.x, localPosition.y - localScale.y + num, localPosition.z);
			TweenPosition.Begin(go, 0.15f, localPosition).method = UITweener.Method.EaseOut;
		}
	}

	private void Animate(UIWidget widget, bool placeAbove, float bottom)
	{
		AnimateColor(widget);
		AnimatePosition(widget, placeAbove, bottom);
	}

	private void OnClick()
	{
		if (base.enabled && NGUITools.GetActive(base.gameObject) && mChild == null && atlas != null && font != null && items.Count > 0)
		{
			mLabelList.Clear();
			handleEvents = true;
			if (mPanel == null)
			{
				mPanel = UIPanel.Find(base.transform, createIfMissing: true);
			}
			Transform transform = base.transform;
			Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(transform.parent, transform);
			mChild = new GameObject("Drop-down List");
			mChild.layer = base.gameObject.layer;
			Transform transform2 = mChild.transform;
			transform2.parent = transform.parent;
			transform2.localPosition = bounds.min;
			transform2.localRotation = Quaternion.identity;
			transform2.localScale = Vector3.one;
			mBackground = NGUITools.AddSprite(mChild, atlas, backgroundSprite);
			mBackground.pivot = UIWidget.Pivot.TopLeft;
			mBackground.depth = NGUITools.CalculateNextDepth(mPanel.gameObject);
			mBackground.color = backgroundColor;
			Vector4 border = mBackground.border;
			mBgBorder = border.y;
			mBackground.cachedTransform.localPosition = new Vector3(0f, border.y, 0f);
			mHighlight = NGUITools.AddSprite(mChild, atlas, highlightSprite);
			mHighlight.pivot = UIWidget.Pivot.TopLeft;
			mHighlight.color = highlightColor;
			UIAtlas.Sprite atlasSprite = mHighlight.GetAtlasSprite();
			if (atlasSprite == null)
			{
				return;
			}
			float num = atlasSprite.inner.yMin - atlasSprite.outer.yMin;
			float num2 = (float)font.size * font.pixelSize * textScale;
			float a = 0f;
			float num3 = 0f - padding.y;
			List<UILabel> list = new List<UILabel>();
			int i = 0;
			for (int count = items.Count; i < count; i++)
			{
				string text = items[i];
				UILabel uILabel = NGUITools.AddWidget<UILabel>(mChild);
				uILabel.pivot = UIWidget.Pivot.TopLeft;
				uILabel.font = font;
				uILabel.text = ((isLocalized && Localization.instance != null) ? Localization.instance.Get(text) : text);
				uILabel.color = textColor;
				uILabel.cachedTransform.localPosition = new Vector3(border.x + padding.x, num3, -1f);
				uILabel.MakePixelPerfect();
				if (textScale != 1f)
				{
					Vector3 localScale = uILabel.cachedTransform.localScale;
					uILabel.cachedTransform.localScale = localScale * textScale;
				}
				list.Add(uILabel);
				num3 -= num2;
				num3 -= padding.y;
				a = Mathf.Max(a, uILabel.relativeSize.x * num2);
				UIEventListener uIEventListener = UIEventListener.Get(uILabel.gameObject);
				uIEventListener.onHover = OnItemHover;
				uIEventListener.onPress = OnItemPress;
				uIEventListener.parameter = text;
				if (mSelectedItem == text)
				{
					Highlight(uILabel, instant: true);
				}
				mLabelList.Add(uILabel);
			}
			a = Mathf.Max(a, bounds.size.x - (border.x + padding.x) * 2f);
			Vector3 center = new Vector3(a * 0.5f / num2, -0.5f, 0f);
			Vector3 size = new Vector3(a / num2, (num2 + padding.y) / num2, 1f);
			int j = 0;
			for (int count2 = list.Count; j < count2; j++)
			{
				BoxCollider boxCollider = NGUITools.AddWidgetCollider(list[j].gameObject);
				center.z = boxCollider.center.z;
				boxCollider.center = center;
				boxCollider.size = size;
			}
			a += (border.x + padding.x) * 2f;
			num3 -= border.y;
			mBackground.cachedTransform.localScale = new Vector3(a, 0f - num3 + border.y, 1f);
			float num4 = 2f * atlas.pixelSize;
			mHighlight.cachedTransform.localScale = new Vector3(a - (border.x + padding.x) * 2f + (atlasSprite.inner.xMin - atlasSprite.outer.xMin) * num4, num2 + num * num4, 1f);
			bool flag = position == Position.Above;
			if (position == Position.Auto)
			{
				UICamera uICamera = UICamera.FindCameraForLayer(base.gameObject.layer);
				if (uICamera != null)
				{
					flag = uICamera.cachedCamera.WorldToViewportPoint(transform.position).y < 0.5f;
				}
			}
			if (isAnimated)
			{
				float bottom = num3 + num2;
				Animate(mHighlight, flag, bottom);
				int k = 0;
				for (int count3 = list.Count; k < count3; k++)
				{
					Animate(list[k], flag, bottom);
				}
				AnimateColor(mBackground);
				AnimateScale(mBackground, flag, bottom);
			}
			if (flag)
			{
				transform2.localPosition = new Vector3(bounds.min.x, bounds.max.y - num3 - border.y, bounds.min.z);
			}
		}
		else
		{
			OnSelect(isSelected: false);
		}
	}
}
