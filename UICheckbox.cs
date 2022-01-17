using AnimationOrTween;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Checkbox")]
public class UICheckbox : MonoBehaviour
{
	public delegate void OnStateChange(GameObject sender, bool state);

	public UISprite checkSprite;

	private UIImageButtonRevised checkSpriteButton;

	public Animation checkAnimation;

	public bool instantTween;

	public bool startsChecked = true;

	public Transform radioButtonRoot;

	public bool optionCanBeNone;

	public GameObject eventReceiver;

	public string functionName = "OnActivate";

	public OnStateChange onStateChange;

	public OnStateChange onStateChangeUser;

	[HideInInspector]
	[SerializeField]
	private bool option;

	private bool mChecked = true;

	private bool mStarted;

	private Transform mTrans;

	public bool isChecked
	{
		get
		{
			return mChecked;
		}
		set
		{
			if (radioButtonRoot == null || value || optionCanBeNone || !mStarted)
			{
				Set(value);
			}
		}
	}

	private void Awake()
	{
		mTrans = base.transform;
		if (checkSprite != null)
		{
			checkSpriteButton = checkSprite.GetComponent<UIImageButtonRevised>();
			if ((bool)checkSpriteButton)
			{
				checkSpriteButton.ForceHover(startsChecked);
			}
			else
			{
				checkSprite.alpha = (startsChecked ? 1f : 0f);
			}
		}
		if (option)
		{
			option = false;
			if (radioButtonRoot == null)
			{
				radioButtonRoot = mTrans.parent;
			}
		}
	}

	private void Start()
	{
		if (eventReceiver == null)
		{
			eventReceiver = base.gameObject;
		}
		mChecked = !startsChecked;
		mStarted = true;
		SetNoCallback(startsChecked);
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnClick()
	{
		if (base.enabled)
		{
			bool num = isChecked;
			isChecked = !isChecked;
			if (num != isChecked && onStateChangeUser != null)
			{
				onStateChangeUser(base.gameObject, isChecked);
			}
		}
	}

	public void SetNoCallback(bool state)
	{
		Set(state, callback: false);
	}

	private void Set(bool state)
	{
		Set(state, callback: true);
	}

	private void Set(bool state, bool callback)
	{
		if (!mStarted)
		{
			mChecked = state;
			startsChecked = state;
			checkSpriteButton = checkSprite.GetComponent<UIImageButtonRevised>();
			if ((bool)checkSpriteButton)
			{
				checkSpriteButton.ForceHover(startsChecked);
			}
			else
			{
				checkSprite.alpha = (startsChecked ? 1f : 0f);
			}
		}
		else
		{
			if (mChecked == state)
			{
				return;
			}
			if (radioButtonRoot != null && state)
			{
				UICheckbox[] componentsInChildren = radioButtonRoot.GetComponentsInChildren<UICheckbox>(includeInactive: true);
				int i = 0;
				for (int num = componentsInChildren.Length; i < num; i++)
				{
					UICheckbox uICheckbox = componentsInChildren[i];
					if (uICheckbox != this && uICheckbox.radioButtonRoot == radioButtonRoot)
					{
						uICheckbox.Set(state: false, callback);
					}
				}
			}
			mChecked = state;
			if (checkSprite != null)
			{
				if (instantTween || (bool)checkSpriteButton)
				{
					if ((bool)checkSpriteButton)
					{
						checkSpriteButton.ForceHover(mChecked);
					}
					else
					{
						checkSprite.alpha = (mChecked ? 1f : 0f);
					}
				}
				else
				{
					TweenAlpha.Begin(checkSprite.gameObject, 0.15f, mChecked ? 1f : 0f);
				}
			}
			if (callback)
			{
				if (onStateChange != null)
				{
					onStateChange(base.gameObject, mChecked);
				}
				if (eventReceiver != null && !string.IsNullOrEmpty(functionName))
				{
					eventReceiver.SendMessage(functionName, mChecked, SendMessageOptions.DontRequireReceiver);
				}
			}
			if (checkAnimation != null)
			{
				ActiveAnimation.Play(checkAnimation, state ? Direction.Forward : Direction.Reverse);
			}
		}
	}
}
