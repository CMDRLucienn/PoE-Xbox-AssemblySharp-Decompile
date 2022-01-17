using AnimationOrTween;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Button Play Animation")]
public class UIButtonPlayAnimation : MonoBehaviour
{
	public Animation target;

	public string clipName;

	public AnimationOrTween.Trigger trigger;

	public Direction playDirection = Direction.Forward;

	public bool resetOnPlay;

	public bool clearSelection;

	public EnableCondition ifDisabledOnPlay;

	public DisableCondition disableWhenFinished;

	public GameObject eventReceiver;

	public string callWhenFinished;

	public ActiveAnimation.OnFinished onFinished;

	private bool mStarted;

	private bool mHighlighted;

	private void Start()
	{
		mStarted = true;
	}

	private void OnEnable()
	{
		if (mStarted && mHighlighted)
		{
			OnHover(UICamera.IsHighlighted(base.gameObject));
		}
	}

	private void OnHover(bool isOver)
	{
		if (base.enabled)
		{
			if (trigger == AnimationOrTween.Trigger.OnHover || (trigger == AnimationOrTween.Trigger.OnHoverTrue && isOver) || (trigger == AnimationOrTween.Trigger.OnHoverFalse && !isOver))
			{
				Play(isOver);
			}
			mHighlighted = isOver;
		}
	}

	private void OnPress(bool isPressed)
	{
		if (base.enabled && (trigger == AnimationOrTween.Trigger.OnPress || (trigger == AnimationOrTween.Trigger.OnPressTrue && isPressed) || (trigger == AnimationOrTween.Trigger.OnPressFalse && !isPressed)))
		{
			Play(isPressed);
		}
	}

	private void OnClick()
	{
		if (base.enabled && trigger == AnimationOrTween.Trigger.OnClick)
		{
			Play(forward: true);
		}
	}

	private void OnDoubleClick()
	{
		if (base.enabled && trigger == AnimationOrTween.Trigger.OnDoubleClick)
		{
			Play(forward: true);
		}
	}

	private void OnSelect(bool isSelected)
	{
		if (base.enabled && (trigger == AnimationOrTween.Trigger.OnSelect || (trigger == AnimationOrTween.Trigger.OnSelectTrue && isSelected) || (trigger == AnimationOrTween.Trigger.OnSelectFalse && !isSelected)))
		{
			Play(forward: true);
		}
	}

	private void OnActivate(bool isActive)
	{
		if (base.enabled && (trigger == AnimationOrTween.Trigger.OnActivate || (trigger == AnimationOrTween.Trigger.OnActivateTrue && isActive) || (trigger == AnimationOrTween.Trigger.OnActivateFalse && !isActive)))
		{
			Play(isActive);
		}
	}

	private void Play(bool forward)
	{
		if (target == null)
		{
			target = GetComponentInChildren<Animation>();
		}
		if (!(target != null))
		{
			return;
		}
		if (clearSelection && UICamera.selectedObject == base.gameObject)
		{
			UICamera.selectedObject = null;
		}
		int num = 0 - playDirection;
		Direction direction = (forward ? playDirection : ((Direction)num));
		ActiveAnimation activeAnimation = ActiveAnimation.Play(target, clipName, direction, ifDisabledOnPlay, disableWhenFinished);
		if (!(activeAnimation == null))
		{
			if (resetOnPlay)
			{
				activeAnimation.Reset();
			}
			activeAnimation.onFinished = onFinished;
			if (eventReceiver != null && !string.IsNullOrEmpty(callWhenFinished))
			{
				activeAnimation.eventReceiver = eventReceiver;
				activeAnimation.callWhenFinished = callWhenFinished;
			}
			else
			{
				activeAnimation.eventReceiver = null;
			}
		}
	}
}
