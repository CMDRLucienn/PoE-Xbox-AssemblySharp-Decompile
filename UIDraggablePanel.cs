using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(UIPanel))]
[AddComponentMenu("NGUI/Interaction/Draggable Panel")]
public class UIDraggablePanel : IgnoreTimeScale, IUIPuckScrollable
{
	public enum DragEffect
	{
		None,
		Momentum,
		MomentumAndSpring
	}

	public enum ShowCondition
	{
		Always,
		OnlyIfNeeded,
		WhenDragging
	}

	public delegate void OnDragFinished();

	private bool m_restrictWithinBoundsEnabled = true;

	public bool restrictWithinPanel = true;

	public bool disableDragIfFits;

	public DragEffect dragEffect = DragEffect.MomentumAndSpring;

	public bool smoothDragStart = true;

	public Vector3 scale = Vector3.one;

	public float scrollWheelFactor;

	public float momentumAmount = 35f;

	public Vector2 relativePositionOnReset = Vector2.zero;

	public bool repositionClipping;

	public bool iOSDragEmulation = true;

	public UIScrollBar horizontalScrollBar;

	public UIScrollBar verticalScrollBar;

	public ShowCondition showScrollBars = ShowCondition.OnlyIfNeeded;

	public OnDragFinished onDragFinished;

	public UIEventListener.FloatDelegate onScrolled;

	public UIEventListener.VoidDelegate onScrollChanged;

	private Transform mTrans;

	private UIPanel mPanel;

	private Plane mPlane;

	private Vector3 mLastPos;

	private bool mPressed;

	private Vector3 mMomentum = Vector3.zero;

	private float mScroll;

	private Bounds mBounds;

	private bool mCalculatedBounds;

	private bool mShouldMove;

	private bool mIgnoreCallbacks;

	private int mDragID = -10;

	private Vector2 mDragStartOffset = Vector2.zero;

	private bool mDragStarted;

	public bool RestrictWithinBoundsEnabled
	{
		get
		{
			return m_restrictWithinBoundsEnabled;
		}
		set
		{
			m_restrictWithinBoundsEnabled = value;
		}
	}

	public UIPanel panel => mPanel;

	public bool IsDragging => mPressed;

	public Bounds bounds
	{
		get
		{
			if (!mCalculatedBounds)
			{
				mCalculatedBounds = true;
				mBounds = NGUIMath.CalculateRelativeWidgetBounds(mTrans, mTrans);
			}
			return mBounds;
		}
	}

	public bool shouldMoveHorizontally
	{
		get
		{
			float num = bounds.size.x;
			if (mPanel.clipping == UIDrawCall.Clipping.SoftClip)
			{
				num += mPanel.clipSoftness.x * 2f;
			}
			return num > mPanel.clipRange.z;
		}
	}

	public bool shouldMoveVertically
	{
		get
		{
			float num = bounds.size.y;
			if (mPanel.clipping == UIDrawCall.Clipping.SoftClip)
			{
				num += mPanel.clipSoftness.y * 2f;
			}
			return num > mPanel.clipRange.w;
		}
	}

	private bool shouldMove
	{
		get
		{
			if (!disableDragIfFits)
			{
				return true;
			}
			if (mPanel == null)
			{
				mPanel = GetComponent<UIPanel>();
			}
			Vector4 clipRange = mPanel.clipRange;
			Bounds bounds = this.bounds;
			float num = ((clipRange.z == 0f) ? ((float)Screen.width) : (clipRange.z * 0.5f));
			float num2 = ((clipRange.w == 0f) ? ((float)Screen.height) : (clipRange.w * 0.5f));
			if (!Mathf.Approximately(scale.x, 0f))
			{
				if (bounds.min.x < clipRange.x - num)
				{
					return true;
				}
				if (bounds.max.x > clipRange.x + num)
				{
					return true;
				}
			}
			if (!Mathf.Approximately(scale.y, 0f))
			{
				if (bounds.min.y < clipRange.y - num2)
				{
					return true;
				}
				if (bounds.max.y > clipRange.y + num2)
				{
					return true;
				}
			}
			return false;
		}
	}

	public Vector3 currentMomentum
	{
		get
		{
			return mMomentum;
		}
		set
		{
			mMomentum = value;
			mShouldMove = true;
		}
	}

	public void SetScroll(float scroll)
	{
		if (scale.x == 0f)
		{
			MoveRelative(new Vector3(0f, 0f - scroll - base.transform.localPosition.y, 0f));
		}
		else
		{
			MoveRelative(new Vector3(0f - scroll - base.transform.localPosition.x, 0f, 0f));
		}
	}

	public float GetScroll()
	{
		if (scale.x == 0f)
		{
			return 0f - base.transform.localPosition.y;
		}
		return 0f - base.transform.localPosition.x;
	}

	public float GetRealRange()
	{
		if (scale.x == 0f)
		{
			return panel.clipRange.w - ((panel.clipping == UIDrawCall.Clipping.SoftClip) ? panel.clipSoftness.y : 0f);
		}
		return panel.clipRange.z - ((panel.clipping == UIDrawCall.Clipping.SoftClip) ? panel.clipSoftness.x : 0f);
	}

	public float GetVisibleWidth()
	{
		return panel.clipRange.z;
	}

	public float GetVisibleHeight()
	{
		return panel.clipRange.w;
	}

	public float GetRealMax()
	{
		if (scale.x == 0f)
		{
			return bounds.size.y;
		}
		return bounds.size.x;
	}

	private void Awake()
	{
		mTrans = base.transform;
		mPanel = GetComponent<UIPanel>();
		UIPanel uIPanel = mPanel;
		uIPanel.onChange = (UIPanel.OnChangeDelegate)Delegate.Combine(uIPanel.onChange, new UIPanel.OnChangeDelegate(OnPanelChange));
	}

	private void OnDestroy()
	{
		if (mPanel != null)
		{
			UIPanel uIPanel = mPanel;
			uIPanel.onChange = (UIPanel.OnChangeDelegate)Delegate.Remove(uIPanel.onChange, new UIPanel.OnChangeDelegate(OnPanelChange));
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnPanelChange()
	{
		UpdateScrollbars(recalculateBounds: true);
	}

	private void Start()
	{
		UpdateScrollbars(recalculateBounds: true);
		if (horizontalScrollBar != null)
		{
			UIScrollBar uIScrollBar = horizontalScrollBar;
			uIScrollBar.onChange = (UIScrollBar.OnScrollBarChange)Delegate.Combine(uIScrollBar.onChange, new UIScrollBar.OnScrollBarChange(OnHorizontalBar));
			horizontalScrollBar.alpha = ((showScrollBars == ShowCondition.Always || shouldMoveHorizontally) ? 1f : 0f);
		}
		if (verticalScrollBar != null)
		{
			UIScrollBar uIScrollBar2 = verticalScrollBar;
			uIScrollBar2.onChange = (UIScrollBar.OnScrollBarChange)Delegate.Combine(uIScrollBar2.onChange, new UIScrollBar.OnScrollBarChange(OnVerticalBar));
			verticalScrollBar.alpha = ((showScrollBars == ShowCondition.Always || shouldMoveVertically) ? 1f : 0f);
		}
	}

	public bool RestrictWithinBounds(bool instant)
	{
		if (!RestrictWithinBoundsEnabled)
		{
			return false;
		}
		Vector3 vector = mPanel.CalculateConstrainOffset(bounds.min, bounds.max);
		if (vector.magnitude > 0.001f)
		{
			if (!instant && dragEffect == DragEffect.MomentumAndSpring)
			{
				SpringPanel.Begin(mPanel.gameObject, mTrans.localPosition + vector, 13f);
			}
			else
			{
				MoveRelative(vector);
				mMomentum = Vector3.zero;
				mScroll = 0f;
			}
			return true;
		}
		return false;
	}

	public void DisableSpring()
	{
		SpringPanel component = GetComponent<SpringPanel>();
		if (component != null)
		{
			component.enabled = false;
		}
	}

	public void UpdateScrollbars(bool recalculateBounds)
	{
		if (mPanel == null)
		{
			return;
		}
		if (onScrollChanged != null)
		{
			onScrollChanged(base.gameObject);
		}
		if (horizontalScrollBar != null || verticalScrollBar != null)
		{
			if (recalculateBounds)
			{
				mCalculatedBounds = false;
				mShouldMove = shouldMove;
			}
			Bounds bounds = this.bounds;
			Vector2 vector = bounds.min;
			Vector2 vector2 = bounds.max;
			if (mPanel.clipping == UIDrawCall.Clipping.SoftClip)
			{
				Vector2 clipSoftness = mPanel.clipSoftness;
				vector -= clipSoftness;
				vector2 += clipSoftness;
			}
			if (horizontalScrollBar != null && vector2.x > vector.x)
			{
				Vector4 clipRange = mPanel.clipRange;
				float num = clipRange.z * 0.5f;
				float num2 = clipRange.x - num - bounds.min.x;
				float num3 = bounds.max.x - num - clipRange.x;
				float num4 = vector2.x - vector.x;
				num2 = Mathf.Clamp01(num2 / num4);
				num3 = Mathf.Clamp01(num3 / num4);
				float num5 = num2 + num3;
				mIgnoreCallbacks = true;
				horizontalScrollBar.barSize = 1f - num5;
				horizontalScrollBar.scrollValue = ((num5 > 0.001f) ? (num2 / num5) : 0f);
				mIgnoreCallbacks = false;
			}
			if (verticalScrollBar != null && vector2.y > vector.y)
			{
				Vector4 clipRange2 = mPanel.clipRange;
				float num6 = clipRange2.w * 0.5f;
				float num7 = clipRange2.y - num6 - vector.y;
				float num8 = vector2.y - num6 - clipRange2.y;
				float num9 = vector2.y - vector.y;
				num7 = Mathf.Clamp01(num7 / num9);
				num8 = Mathf.Clamp01(num8 / num9);
				float num10 = num7 + num8;
				mIgnoreCallbacks = true;
				verticalScrollBar.barSize = 1f - num10;
				verticalScrollBar.scrollValue = ((num10 > 0.001f) ? (1f - num7 / num10) : 0f);
				mIgnoreCallbacks = false;
			}
		}
		else if (recalculateBounds)
		{
			mCalculatedBounds = false;
		}
	}

	public void SetDragAmount(float x, float y, bool updateScrollbars)
	{
		DisableSpring();
		Bounds bounds = this.bounds;
		if (bounds.min.x == bounds.max.x || bounds.min.y == bounds.max.y)
		{
			return;
		}
		Vector4 clipRange = mPanel.clipRange;
		float num = clipRange.z * 0.5f;
		float num2 = clipRange.w * 0.5f;
		float num3 = bounds.min.x + num;
		float num4 = bounds.max.x - num;
		float num5 = bounds.min.y + num2;
		float num6 = bounds.max.y - num2;
		if (mPanel.clipping == UIDrawCall.Clipping.SoftClip)
		{
			num3 -= mPanel.clipSoftness.x;
			num4 += mPanel.clipSoftness.x;
			num5 -= mPanel.clipSoftness.y;
			num6 += mPanel.clipSoftness.y;
		}
		float num7 = Mathf.Lerp(num3, num4, x);
		float num8 = Mathf.Lerp(num6, num5, y);
		if (!updateScrollbars)
		{
			Vector3 localPosition = mTrans.localPosition;
			if (scale.x != 0f)
			{
				localPosition.x += clipRange.x - num7;
			}
			if (scale.y != 0f)
			{
				localPosition.y += clipRange.y - num8;
			}
			mTrans.localPosition = localPosition;
		}
		if (scale.x != 0f)
		{
			clipRange.x = num7;
		}
		if (scale.y != 0f)
		{
			clipRange.y = num8;
		}
		mPanel.clipRange = clipRange;
		if (updateScrollbars)
		{
			UpdateScrollbars(recalculateBounds: false);
		}
	}

	public void ResetPosition()
	{
		if (!(mTrans == null))
		{
			InvalidateBounds();
			SetDragAmount(relativePositionOnReset.x, relativePositionOnReset.y, updateScrollbars: false);
			SetDragAmount(relativePositionOnReset.x, relativePositionOnReset.y, updateScrollbars: true);
		}
	}

	public void InvalidateBounds()
	{
		mCalculatedBounds = false;
	}

	private void OnHorizontalBar(UIScrollBar sb)
	{
		if (!mIgnoreCallbacks)
		{
			float x = ((horizontalScrollBar != null) ? horizontalScrollBar.scrollValue : 0f);
			float y = ((verticalScrollBar != null) ? verticalScrollBar.scrollValue : 0f);
			SetDragAmount(x, y, updateScrollbars: false);
		}
	}

	private void OnVerticalBar(UIScrollBar sb)
	{
		if (!mIgnoreCallbacks)
		{
			float x = ((horizontalScrollBar != null) ? horizontalScrollBar.scrollValue : 0f);
			float y = ((verticalScrollBar != null) ? verticalScrollBar.scrollValue : 0f);
			SetDragAmount(x, y, updateScrollbars: false);
		}
	}

	public void MoveRelative(Vector3 relative)
	{
		mTrans.localPosition += relative;
		Vector4 clipRange = mPanel.clipRange;
		clipRange.x -= relative.x;
		clipRange.y -= relative.y;
		mPanel.clipRange = clipRange;
		UpdateScrollbars(recalculateBounds: false);
	}

	public void MoveAbsolute(Vector3 absolute)
	{
		Vector3 vector = mTrans.InverseTransformPoint(absolute);
		Vector3 vector2 = mTrans.InverseTransformPoint(Vector3.zero);
		MoveRelative(vector - vector2);
	}

	public void ScrollObjectInView(GameObject target, float buffer)
	{
		if (panel.clipping != 0)
		{
			Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(base.transform, target.transform);
			float num = panel.clipRange.y + panel.clipRange.w / 2f - buffer;
			float num2 = panel.clipRange.y - panel.clipRange.w / 2f + buffer;
			if (panel.clipping == UIDrawCall.Clipping.SoftClip)
			{
				num -= panel.clipSoftness.y;
				num2 += panel.clipSoftness.y;
			}
			if (bounds.max.y > num)
			{
				MoveRelative(new Vector3(0f, num - bounds.max.y, 0f));
			}
			else if (bounds.min.y < num2)
			{
				MoveRelative(new Vector3(0f, num2 - bounds.min.y, 0f));
			}
		}
	}

	public void Press(bool pressed)
	{
		if (smoothDragStart && pressed)
		{
			mDragStarted = false;
			mDragStartOffset = Vector2.zero;
		}
		if (!base.enabled || !NGUITools.GetActive(base.gameObject))
		{
			return;
		}
		if (!pressed && mDragID == UICamera.currentTouchID)
		{
			mDragID = -10;
		}
		mCalculatedBounds = false;
		mShouldMove = shouldMove;
		if (!mShouldMove)
		{
			return;
		}
		mPressed = pressed;
		if (pressed)
		{
			mMomentum = Vector3.zero;
			mScroll = 0f;
			DisableSpring();
			mLastPos = UICamera.lastHit.point;
			mPlane = new Plane(mTrans.rotation * Vector3.back, mLastPos);
			return;
		}
		if (restrictWithinPanel && mPanel.clipping != 0 && dragEffect == DragEffect.MomentumAndSpring)
		{
			RestrictWithinBounds(instant: false);
		}
		if (onDragFinished != null)
		{
			onDragFinished();
		}
	}

	public void Drag()
	{
		if (!base.enabled || !NGUITools.GetActive(base.gameObject) || !mShouldMove)
		{
			return;
		}
		if (mDragID == -10)
		{
			mDragID = UICamera.currentTouchID;
		}
		UICamera.currentTouch.clickNotification = UICamera.ClickNotification.BasedOnDelta;
		if (smoothDragStart && !mDragStarted)
		{
			mDragStarted = true;
			mDragStartOffset = UICamera.currentTouch.totalDelta;
		}
		Ray ray = (smoothDragStart ? UICamera.currentCamera.ScreenPointToRay(UICamera.currentTouch.pos - mDragStartOffset) : UICamera.currentCamera.ScreenPointToRay(UICamera.currentTouch.pos));
		float enter = 0f;
		if (mPlane.Raycast(ray, out enter))
		{
			Vector3 point = ray.GetPoint(enter);
			Vector3 vector = point - mLastPos;
			mLastPos = point;
			if (vector.x != 0f || vector.y != 0f)
			{
				vector = mTrans.InverseTransformDirection(vector);
				vector.Scale(scale);
				vector = mTrans.TransformDirection(vector);
			}
			mMomentum = Vector3.Lerp(mMomentum, mMomentum + vector * (0.01f * momentumAmount), 0.67f);
			if (!iOSDragEmulation)
			{
				MoveAbsolute(vector);
			}
			else if (mPanel.CalculateConstrainOffset(bounds.min, bounds.max).magnitude > 0.001f)
			{
				MoveAbsolute(vector * 0.5f);
				mMomentum *= 0.5f;
			}
			else
			{
				MoveAbsolute(vector);
			}
			if (restrictWithinPanel && mPanel.clipping != 0 && dragEffect != DragEffect.MomentumAndSpring)
			{
				RestrictWithinBounds(instant: true);
			}
		}
	}

	public void Scroll(float delta)
	{
		if (base.enabled && NGUITools.GetActive(base.gameObject) && scrollWheelFactor != 0f)
		{
			DisableSpring();
			mShouldMove = shouldMove;
			if (mShouldMove)
			{
				if (Mathf.Sign(mScroll) != Mathf.Sign(delta))
				{
					mScroll = 0f;
				}
				mScroll += delta * scrollWheelFactor;
			}
		}
		if (onScrolled != null)
		{
			onScrolled(base.gameObject, delta);
		}
	}

	private void LateUpdate()
	{
		if (GameInput.GetControlDown(MappedControl.PAN_CAMERA))
		{
			mLastPos = GameInput.MousePosition;
			Press(pressed: true);
		}
		else if (GameInput.GetControlUp(MappedControl.PAN_CAMERA))
		{
			Press(pressed: false);
		}
		if (repositionClipping)
		{
			repositionClipping = false;
			mCalculatedBounds = false;
			SetDragAmount(relativePositionOnReset.x, relativePositionOnReset.y, updateScrollbars: true);
		}
		if (!Application.isPlaying)
		{
			return;
		}
		float num = UpdateRealTimeDelta();
		if (showScrollBars != 0)
		{
			bool flag = false;
			bool flag2 = false;
			if (showScrollBars != ShowCondition.WhenDragging || mDragID != -10 || mMomentum.magnitude > 0.01f)
			{
				flag = shouldMoveVertically;
				flag2 = shouldMoveHorizontally;
			}
			if ((bool)verticalScrollBar)
			{
				float alpha = verticalScrollBar.alpha;
				alpha += (flag ? (num * 6f) : ((0f - num) * 3f));
				alpha = Mathf.Clamp01(alpha);
				if (verticalScrollBar.alpha != alpha)
				{
					verticalScrollBar.alpha = alpha;
				}
			}
			if ((bool)horizontalScrollBar)
			{
				float alpha2 = horizontalScrollBar.alpha;
				alpha2 += (flag2 ? (num * 6f) : ((0f - num) * 3f));
				alpha2 = Mathf.Clamp01(alpha2);
				if (horizontalScrollBar.alpha != alpha2)
				{
					horizontalScrollBar.alpha = alpha2;
				}
			}
		}
		if (mShouldMove && !mPressed)
		{
			mMomentum -= scale * (mScroll * 0.05f);
			if (mMomentum.magnitude > 0.0001f)
			{
				mScroll = NGUIMath.SpringLerp(mScroll, 0f, 20f, num);
				Vector3 absolute = NGUIMath.SpringDampen(ref mMomentum, 9f, num);
				MoveAbsolute(absolute);
				if (restrictWithinPanel && mPanel.clipping != 0)
				{
					RestrictWithinBounds(instant: false);
				}
				if (mMomentum.magnitude < 0.0001f && onDragFinished != null)
				{
					onDragFinished();
				}
				return;
			}
			mScroll = 0f;
			mMomentum = Vector3.zero;
		}
		else
		{
			mScroll = 0f;
		}
		NGUIMath.SpringDampen(ref mMomentum, 9f, num);
	}
}
