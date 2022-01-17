using System;
using UnityEngine;

public class UICharacterCreationStage : MonoBehaviour
{
	public int Stage;

	public bool HideIfSkippable;

	public UICharacterCreationController ShowController;

	private void Start()
	{
		base.gameObject.SetActive(value: true);
		UICharacterCreationManager instance = UICharacterCreationManager.Instance;
		instance.OnStageChanged = (UICharacterCreationManager.StageChanged)Delegate.Combine(instance.OnStageChanged, new UICharacterCreationManager.StageChanged(OnStageChanged));
		OnStageChanged(UICharacterCreationManager.Instance.CurrentStage);
	}

	private void OnDestroy()
	{
		if ((bool)UICharacterCreationManager.Instance)
		{
			UICharacterCreationManager instance = UICharacterCreationManager.Instance;
			instance.OnStageChanged = (UICharacterCreationManager.StageChanged)Delegate.Remove(instance.OnStageChanged, new UICharacterCreationManager.StageChanged(OnStageChanged));
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnStageChanged(int stage)
	{
		if (Stage == stage)
		{
			base.gameObject.SetActive(value: true);
			if (Stage == stage && (bool)ShowController)
			{
				UILabel componentInChildren = GetComponentInChildren<UILabel>();
				if ((bool)componentInChildren)
				{
					componentInChildren.color = Color.white;
				}
			}
			BoxCollider[] componentsInChildren = GetComponentsInChildren<BoxCollider>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = true;
			}
		}
		else
		{
			UILabel componentInChildren2 = GetComponentInChildren<UILabel>();
			if ((bool)componentInChildren2)
			{
				componentInChildren2.color = new Color(0.7058824f, 0.7058824f, 0.7058824f);
			}
		}
		if (stage > Stage && (bool)ShowController)
		{
			TweenPosition[] componentsInChildren2 = GetComponentsInChildren<TweenPosition>();
			foreach (TweenPosition tweenPosition in componentsInChildren2)
			{
				if ((bool)tweenPosition)
				{
					tweenPosition.enabled = true;
					tweenPosition.Play(forward: true);
				}
			}
			TweenAlpha[] componentsInChildren3 = GetComponentsInChildren<TweenAlpha>();
			foreach (TweenAlpha tweenAlpha in componentsInChildren3)
			{
				if ((bool)tweenAlpha)
				{
					tweenAlpha.enabled = true;
				}
			}
		}
		UIPanel componentInChildren3 = GetComponentInChildren<UIPanel>();
		if ((bool)componentInChildren3)
		{
			componentInChildren3.alpha = ((HideIfSkippable && (bool)ShowController && ShowController.ShouldSkip()) ? 0f : 1f);
		}
		if (HideIfSkippable)
		{
			UILabel componentInChildren4 = GetComponentInChildren<UILabel>();
			if ((bool)componentInChildren4)
			{
				Color color = componentInChildren4.color;
				color.a = (((bool)ShowController && ShowController.ShouldSkip()) ? 0f : 1f);
				componentInChildren4.color = color;
			}
		}
	}
}
