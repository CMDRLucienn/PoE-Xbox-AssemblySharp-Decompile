using System;
using UnityEngine;

public class UIPuckScrollbar : MonoBehaviour
{
	public delegate void OnChange(float real, float norm);

	public UISprite PuckSprite;

	public UISprite TrackSprite;

	public UISprite UpArrowSprite;

	public UISprite DownArrowSprite;

	public GameObject ScrollTarget;

	private IUIPuckScrollable m_ScrollTargetComponent;

	public GameObject SizeTarget;

	public bool IsVertical = true;

	private float mPuckStartPos;

	private float mMaxRealRange;

	public int ArrowIncrement = 1;

	public int MousewheelIncrement = 1;

	public OnChange onChange;

	public bool UninvertedContent;

	private float mPuckOffset;

	private float mPixelToReal;

	private float mRealToPixel;

	private float mMaxPixelRange;

	private float PuckInitialMin;

	private float PuckInitialMax;

	public bool HideIfNoScrollAvailable;

	public UIPanel ResizePanelTargetIfHidden;

	public UILabel[] ResizeLabelsOnScrollbarHide;

	public float ResizePanelSize;

	private bool m_LastOffsetWasScrollBarVisible = true;

	private float mTrackExtraSpace;

	private bool hoverUpArrow;

	private bool hoverDownArrow;

	private bool hoverMouseDown;

	private float hoverTime;

	private float lastHoverFire;

	public float NormalizedOffset
	{
		get
		{
			if (mMaxRealRange == 0f)
			{
				return 0f;
			}
			return mPuckOffset / mMaxRealRange;
		}
	}

	public float RealOffset => mPuckOffset;

	private void SetScrollTarget(GameObject target)
	{
		ScrollTarget = target;
		if (ScrollTarget != null)
		{
			m_ScrollTargetComponent = (IUIPuckScrollable)ScrollTarget.GetComponent(typeof(IUIPuckScrollable));
			if (m_ScrollTargetComponent != null)
			{
				m_ScrollTargetComponent.RestrictWithinBoundsEnabled = false;
			}
			else
			{
				Debug.LogError("UIPuckScrollbar ScrollTarget must have a component that implements IUIPuckScrollable.");
			}
		}
	}

	private float GetScrollAxis(Vector3 v3)
	{
		if (!IsVertical)
		{
			return v3.x;
		}
		return v3.y;
	}

	private void Awake()
	{
		if (IsVertical)
		{
			PuckInitialMin = DownArrowSprite.transform.localPosition.y + PuckSprite.transform.localScale.y / 2f;
			PuckInitialMax = UpArrowSprite.transform.localPosition.y - PuckSprite.transform.localScale.y / 2f;
			PuckInitialMin -= DownArrowSprite.transform.localScale.y * (float)(UIWidgetUtils.PivotDirY(DownArrowSprite.pivot) - 1) * 0.5f;
			PuckInitialMax -= UpArrowSprite.transform.localScale.y * (float)(UIWidgetUtils.PivotDirY(UpArrowSprite.pivot) + 1) * 0.5f;
		}
		else
		{
			PuckInitialMin = DownArrowSprite.transform.localPosition.x + PuckSprite.transform.localScale.x / 2f;
			PuckInitialMax = UpArrowSprite.transform.localPosition.x;
			PuckInitialMin -= DownArrowSprite.transform.localScale.x * (float)(UIWidgetUtils.PivotDirX(DownArrowSprite.pivot) - 1) * 0.5f;
			PuckInitialMax -= UpArrowSprite.transform.localScale.x * (float)(UIWidgetUtils.PivotDirX(UpArrowSprite.pivot) + 1) * 0.5f;
		}
	}

	private void Start()
	{
		SetScrollTarget(ScrollTarget);
		if (PuckSprite != null)
		{
			UIEventListener uIEventListener = UIEventListener.Get(PuckSprite.gameObject);
			uIEventListener.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener.onDrag, new UIEventListener.VectorDelegate(OnDragPuck));
			uIEventListener.onScroll = (UIEventListener.FloatDelegate)Delegate.Combine(uIEventListener.onScroll, new UIEventListener.FloatDelegate(OnScroll));
		}
		else
		{
			Debug.LogError("UIPuckScrollbar PuckSprite can't be null.");
		}
		if (UpArrowSprite != null)
		{
			UIEventListener uIEventListener2 = UIEventListener.Get(UpArrowSprite.gameObject);
			uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnUpArrow));
			uIEventListener2.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onHover, new UIEventListener.BoolDelegate(OnHoverUpArrow));
			uIEventListener2.onScroll = (UIEventListener.FloatDelegate)Delegate.Combine(uIEventListener2.onScroll, new UIEventListener.FloatDelegate(OnScroll));
		}
		else
		{
			Debug.LogError("UIPuckScrollbar UpArrowSprite can't be null.");
		}
		if (DownArrowSprite != null)
		{
			UIEventListener uIEventListener3 = UIEventListener.Get(DownArrowSprite.gameObject);
			uIEventListener3.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onClick, new UIEventListener.VoidDelegate(OnDownArrow));
			uIEventListener3.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener3.onHover, new UIEventListener.BoolDelegate(OnHoverDownArrow));
			uIEventListener3.onScroll = (UIEventListener.FloatDelegate)Delegate.Combine(uIEventListener3.onScroll, new UIEventListener.FloatDelegate(OnScroll));
		}
		else
		{
			Debug.LogError("UIPuckScrollbar DownArrowSprite can't be null.");
		}
		if (TrackSprite != null)
		{
			UIEventListener uIEventListener4 = UIEventListener.Get(TrackSprite.gameObject);
			uIEventListener4.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener4.onClick, new UIEventListener.VoidDelegate(OnClickTrack));
			uIEventListener4.onScroll = (UIEventListener.FloatDelegate)Delegate.Combine(uIEventListener4.onScroll, new UIEventListener.FloatDelegate(OnScroll));
		}
		if ((bool)ScrollTarget)
		{
			if (m_ScrollTargetComponent is UIDraggablePanel)
			{
				UIDraggablePanel obj = (UIDraggablePanel)m_ScrollTargetComponent;
				obj.onScrolled = (UIEventListener.FloatDelegate)Delegate.Combine(obj.onScrolled, new UIEventListener.FloatDelegate(OnScroll));
			}
			else
			{
				UIDragPanelContents[] componentsInChildren = ScrollTarget.GetComponentsInChildren<UIDragPanelContents>(includeInactive: true);
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					UIEventListener uIEventListener5 = UIEventListener.Get(componentsInChildren[i].gameObject);
					uIEventListener5.onScroll = (UIEventListener.FloatDelegate)Delegate.Combine(uIEventListener5.onScroll, new UIEventListener.FloatDelegate(OnScroll));
				}
			}
		}
		if (SizeTarget != null)
		{
			if (PuckInitialMax != PuckInitialMin)
			{
				mTrackExtraSpace = GetScrollAxis(SizeTarget.transform.localScale) - (PuckInitialMax - PuckInitialMin);
			}
			else
			{
				mTrackExtraSpace = GetScrollAxis(SizeTarget.transform.localScale) - GetScrollAxis(TrackSprite.transform.localScale);
			}
		}
		if (PuckInitialMax != PuckInitialMin)
		{
			mMaxPixelRange = PuckInitialMax - PuckInitialMin;
			if (UninvertedContent)
			{
				mPuckStartPos = PuckInitialMax;
			}
			else
			{
				mPuckStartPos = PuckInitialMin;
			}
		}
		else
		{
			mMaxPixelRange = GetScrollAxis(TrackSprite.transform.localScale);
			mPuckStartPos = GetScrollAxis(PuckSprite.transform.localPosition);
		}
		CalcConversions();
		if (HideIfNoScrollAvailable)
		{
			bool flag = mRealToPixel != 0f;
			PuckSprite.gameObject.SetActive(flag);
			TrackSprite.gameObject.SetActive(flag);
			UpArrowSprite.gameObject.SetActive(flag);
			DownArrowSprite.gameObject.SetActive(flag);
			ResizeScrollPanelForScrollbarHidden(flag);
		}
	}

	public void ResetForNewContent()
	{
		if (SizeTarget != null)
		{
			Resize(GetScrollAxis(SizeTarget.transform.localScale));
		}
		if (m_ScrollTargetComponent != null)
		{
			SetRealRange(m_ScrollTargetComponent.GetRealMax());
		}
		CalcConversions();
		if (HideIfNoScrollAvailable)
		{
			bool flag = mRealToPixel != 0f;
			PuckSprite.gameObject.SetActive(flag);
			TrackSprite.gameObject.SetActive(flag);
			UpArrowSprite.gameObject.SetActive(flag);
			DownArrowSprite.gameObject.SetActive(flag);
			ResizeScrollPanelForScrollbarHidden(flag);
		}
		if (m_ScrollTargetComponent != null)
		{
			PostChange(UninvertedContent ? mMaxRealRange : (0f - mMaxRealRange));
		}
	}

	public void Update()
	{
		if ((hoverDownArrow || hoverUpArrow) && !hoverMouseDown && GameInput.GetMouseButton(0, setHandled: true))
		{
			hoverMouseDown = true;
			if (hoverUpArrow)
			{
				PostChange(ArrowIncrement);
			}
			else
			{
				PostChange(-ArrowIncrement);
			}
		}
		if (hoverMouseDown)
		{
			hoverTime += TimeController.sUnscaledDelta;
			if (hoverTime > GameInput.Instance.KeyRepeatDelay)
			{
				int num = Mathf.FloorToInt((hoverTime - lastHoverFire) / (1f / GameInput.Instance.KeyRepeatRate));
				lastHoverFire += (float)num / GameInput.Instance.KeyRepeatRate;
				if (hoverUpArrow)
				{
					PostChange(ArrowIncrement * num);
				}
				else
				{
					PostChange(-ArrowIncrement * num);
				}
			}
		}
		if (SizeTarget != null)
		{
			Resize(GetScrollAxis(SizeTarget.transform.localScale));
		}
		if (m_ScrollTargetComponent != null)
		{
			SetRealRange(m_ScrollTargetComponent.GetRealMax());
		}
		if (m_ScrollTargetComponent != null && m_ScrollTargetComponent.GetScroll() != mPuckOffset)
		{
			PostChange(0f);
		}
		if (HideIfNoScrollAvailable)
		{
			bool flag = mRealToPixel != 0f;
			if (flag != PuckSprite.gameObject.activeSelf)
			{
				PuckSprite.gameObject.SetActive(flag);
				TrackSprite.gameObject.SetActive(flag);
				UpArrowSprite.gameObject.SetActive(flag);
				DownArrowSprite.gameObject.SetActive(flag);
				ResizeScrollPanelForScrollbarHidden(flag);
			}
		}
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void CalcConversions()
	{
		mPixelToReal = mMaxRealRange / mMaxPixelRange;
		if (mMaxRealRange == 0f)
		{
			mRealToPixel = 0f;
		}
		else
		{
			mRealToPixel = mMaxPixelRange / mMaxRealRange;
		}
	}

	public void SetRealRange(float max)
	{
		float num = mMaxRealRange;
		mMaxRealRange = Mathf.Max(0f, max - m_ScrollTargetComponent.GetRealRange());
		if (mPuckOffset > mMaxRealRange)
		{
			mPuckOffset = mMaxRealRange;
		}
		if (num != mMaxRealRange)
		{
			CalcConversions();
			PostChange(0f);
		}
	}

	public void Resize(float newsize)
	{
		float num = mMaxPixelRange;
		mMaxPixelRange = newsize - mTrackExtraSpace;
		if (mMaxPixelRange != num)
		{
			float num2 = mMaxPixelRange - num;
			if (TrackSprite != null && IsVertical)
			{
				TrackSprite.transform.localPosition = new Vector3(TrackSprite.transform.localPosition.x, TrackSprite.transform.localPosition.y + num2 / 2f, TrackSprite.transform.localPosition.z);
				TrackSprite.transform.localScale = new Vector3(TrackSprite.transform.localScale.x, TrackSprite.transform.localScale.y + num2, TrackSprite.transform.localScale.z);
			}
			if (UpArrowSprite != null && IsVertical)
			{
				UpArrowSprite.transform.localPosition = new Vector3(UpArrowSprite.transform.localPosition.x, UpArrowSprite.transform.localPosition.y + num2, UpArrowSprite.transform.localPosition.z);
			}
			CalcConversions();
			PostChange(0f);
		}
	}

	private void OnDragPuck(GameObject go, Vector2 delta)
	{
		Vector3 mousePosition = Input.mousePosition;
		Vector3 position = PuckSprite.transform.position;
		Vector3 vector = UICamera.currentCamera.WorldToScreenPoint(position);
		PostChange(GetScrollAxis(mousePosition - vector) * mPixelToReal);
	}

	private void OnHoverUpArrow(GameObject go, bool over)
	{
		hoverUpArrow = over;
		if (!over)
		{
			hoverMouseDown = (hoverUpArrow = false);
			ResetHoverTime();
		}
	}

	private void OnHoverDownArrow(GameObject go, bool over)
	{
		hoverDownArrow = over;
		if (!over)
		{
			hoverMouseDown = (hoverDownArrow = false);
			ResetHoverTime();
		}
	}

	private void ResetHoverTime()
	{
		hoverTime = 0f;
		if (GameInput.Instance != null)
		{
			lastHoverFire = GameInput.Instance.KeyRepeatDelay;
		}
	}

	private void OnUpArrow(GameObject go)
	{
		hoverMouseDown = false;
		ResetHoverTime();
	}

	private void OnDownArrow(GameObject go)
	{
		hoverMouseDown = false;
		ResetHoverTime();
	}

	private void OnClickTrack(GameObject go)
	{
		Vector2 vector = GameInput.MousePosition;
		Vector3 point = InGameUILayout.NGUICamera.ScreenToWorldPoint(vector);
		Vector3 v = base.transform.worldToLocalMatrix.MultiplyPoint3x4(point);
		if (IsVertical)
		{
			if (UninvertedContent)
			{
				v.y = 0f - mMaxPixelRange + v.y - PuckInitialMin;
			}
			else
			{
				v.y -= PuckInitialMin;
			}
		}
		else if (UninvertedContent)
		{
			v.x = 0f - mMaxPixelRange + v.x - PuckInitialMin;
		}
		else
		{
			v.x -= PuckInitialMin;
		}
		float num = GetScrollAxis(v) * mPixelToReal;
		PostChange(num - mPuckOffset);
	}

	private void OnScroll(GameObject go, float delta)
	{
		if (IsVertical)
		{
			PostChange(10f * delta * (float)MousewheelIncrement);
		}
		else
		{
			PostChange(-10f * delta * (float)MousewheelIncrement);
		}
	}

	private void ResizeScrollPanelForScrollbarHidden(bool isScrollbarVisible)
	{
		if (ResizePanelTargetIfHidden == null || m_LastOffsetWasScrollBarVisible == isScrollbarVisible)
		{
			return;
		}
		if (IsVertical)
		{
			float num = (isScrollbarVisible ? (0f - ResizePanelSize) : ResizePanelSize);
			Vector4 clipRange = ResizePanelTargetIfHidden.clipRange;
			clipRange.x += num / 2f;
			clipRange.z += num;
			ResizePanelTargetIfHidden.clipRange = clipRange;
			for (int i = 0; i < ResizeLabelsOnScrollbarHide.Length; i++)
			{
				if (ResizeLabelsOnScrollbarHide[i].lineWidth != 0)
				{
					ResizeLabelsOnScrollbarHide[i].lineWidth += Mathf.RoundToInt(num);
				}
			}
		}
		else
		{
			float num2 = (isScrollbarVisible ? (0f - ResizePanelSize) : ResizePanelSize);
			Vector4 clipRange2 = ResizePanelTargetIfHidden.clipRange;
			clipRange2.y += num2 / 2f;
			clipRange2.w += num2;
			ResizePanelTargetIfHidden.clipRange = clipRange2;
		}
		m_LastOffsetWasScrollBarVisible = isScrollbarVisible;
	}

	protected void PostChange(float delta)
	{
		delta = Mathf.Floor(delta);
		Transform obj = ScrollTarget.transform;
		Vector3 localPosition = obj.localPosition;
		Vector3 position = obj.position;
		Vector3 position2 = UICamera.currentCamera.WorldToScreenPoint(position);
		if (IsVertical)
		{
			position2.y += delta;
		}
		else
		{
			position2.x += delta;
		}
		Vector3 position3 = UICamera.currentCamera.ScreenToWorldPoint(position2);
		Vector3 vector = obj.parent.InverseTransformPoint(position3);
		float num = 0f;
		num = ((!IsVertical) ? (vector.x - localPosition.x) : (vector.y - localPosition.y));
		if (UninvertedContent)
		{
			mPuckOffset = Mathf.Clamp(m_ScrollTargetComponent.GetScroll() + num, 0f - mMaxRealRange, 0f);
		}
		else
		{
			mPuckOffset = Mathf.Clamp(m_ScrollTargetComponent.GetScroll() + num, 0f, mMaxRealRange);
		}
		float x = PuckSprite.transform.localPosition.x;
		float y = PuckSprite.transform.localPosition.y;
		if (IsVertical)
		{
			y = mPuckStartPos + mPuckOffset * mRealToPixel;
		}
		else
		{
			x = mPuckStartPos + mPuckOffset * mRealToPixel;
		}
		PuckSprite.transform.localPosition = new Vector3(x, y, PuckSprite.transform.localPosition.z);
		m_ScrollTargetComponent.SetScroll(mPuckOffset);
		if (onChange != null)
		{
			onChange(mPuckOffset, NormalizedOffset);
		}
	}
}
