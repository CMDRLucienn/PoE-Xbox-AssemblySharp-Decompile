using AnimationOrTween;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Button Tween")]
public class UIButtonTween : MonoBehaviour
{
	public GameObject tweenTarget;

	public int tweenGroup;

	public AnimationOrTween.Trigger trigger;

	public Direction playDirection = Direction.Forward;

	public bool resetOnPlay;

	public EnableCondition ifDisabledOnPlay;

	public DisableCondition disableWhenFinished;

	public bool includeChildren;

	public GameObject eventReceiver;

	public string callWhenFinished;

	public UITweener.OnFinished onFinished;

	private UITweener[] mTweens;

	private bool mStarted;

	private bool mHighlighted;

	private void Start()
	{
		mStarted = true;
		if (tweenTarget == null)
		{
			tweenTarget = base.gameObject;
		}
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

	private void Update()
	{
		if (disableWhenFinished == DisableCondition.DoNotDisable || mTweens == null)
		{
			return;
		}
		bool flag = true;
		bool flag2 = true;
		int i = 0;
		for (int num = mTweens.Length; i < num; i++)
		{
			UITweener uITweener = mTweens[i];
			if (uITweener.tweenGroup == tweenGroup)
			{
				if (uITweener.enabled)
				{
					flag = false;
					break;
				}
				if (uITweener.direction != (Direction)disableWhenFinished)
				{
					flag2 = false;
				}
			}
		}
		if (flag)
		{
			if (flag2)
			{
				NGUITools.SetActive(tweenTarget, state: false);
			}
			mTweens = null;
		}
	}

	public void Play(bool forward)
	{
		GameObject gameObject = ((tweenTarget == null) ? base.gameObject : tweenTarget);
		if (!NGUITools.GetActive(gameObject))
		{
			if (ifDisabledOnPlay != EnableCondition.EnableThenPlay)
			{
				return;
			}
			NGUITools.SetActive(gameObject, state: true);
		}
		mTweens = (includeChildren ? gameObject.GetComponentsInChildren<UITweener>() : gameObject.GetComponents<UITweener>());
		if (mTweens.Length == 0)
		{
			if (disableWhenFinished != 0)
			{
				NGUITools.SetActive(tweenTarget, state: false);
			}
			return;
		}
		bool flag = false;
		if (playDirection == Direction.Reverse)
		{
			forward = !forward;
		}
		int i = 0;
		for (int num = mTweens.Length; i < num; i++)
		{
			UITweener uITweener = mTweens[i];
			if (uITweener.tweenGroup == tweenGroup)
			{
				if (!flag && !NGUITools.GetActive(gameObject))
				{
					flag = true;
					NGUITools.SetActive(gameObject, state: true);
				}
				if (playDirection == Direction.Toggle)
				{
					uITweener.Toggle();
				}
				else
				{
					uITweener.Play(forward);
				}
				if (resetOnPlay)
				{
					uITweener.Reset();
				}
				uITweener.onFinished = onFinished;
				if (eventReceiver != null && !string.IsNullOrEmpty(callWhenFinished))
				{
					uITweener.eventReceiver = eventReceiver;
					uITweener.callWhenFinished = callWhenFinished;
				}
			}
		}
	}
}
