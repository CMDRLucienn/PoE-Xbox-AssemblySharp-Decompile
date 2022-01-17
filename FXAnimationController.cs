using System.Collections.Generic;
using UnityEngine;

public class FXAnimationController : AnimationController
{
	public List<FxActionPair> PairedActions = new List<FxActionPair>();

	private GameObject baseFX;

	private FxActionPair m_currentAP;

	protected override void Start()
	{
		base.Start();
		ParticleSystem componentInChildren = GetComponentInChildren<ParticleSystem>();
		if ((bool)componentInChildren)
		{
			baseFX = componentInChildren.gameObject;
		}
	}

	public override bool IsPerformingReaction(ReactionType reaction)
	{
		return false;
	}

	protected override void Update()
	{
		base.Update();
		if (base.Idle && m_currentAP != null)
		{
			baseFX.GetComponent<Renderer>().enabled = true;
			m_currentAP = null;
			return;
		}
		FxActionPair fxActionPair = FindCurrentReaction(CurrentReaction);
		if (fxActionPair == null)
		{
			fxActionPair = FindCurrentAction();
		}
		if (fxActionPair != null && fxActionPair != m_currentAP)
		{
			baseFX.GetComponent<Renderer>().enabled = false;
			GameUtilities.LaunchEffect(fxActionPair.VisualEffect, 1f, base.transform, null);
			m_currentAP = fxActionPair;
		}
	}

	public override void SetReaction(ReactionType reaction)
	{
		FxActionPair fxActionPair = FindCurrentReaction(reaction);
		if (fxActionPair != null)
		{
			baseFX.SetActive(value: false);
			m_currentAP = null;
			GameUtilities.LaunchEffect(fxActionPair.VisualEffect, 1f, base.transform, null);
			m_currentAP = fxActionPair;
		}
	}

	private FxActionPair FindCurrentAction()
	{
		if (CurrentAction.m_actionType == ActionType.None)
		{
			return null;
		}
		foreach (FxActionPair pairedAction in PairedActions)
		{
			if (pairedAction.Equals(CurrentAction))
			{
				return pairedAction;
			}
		}
		return null;
	}

	private FxActionPair FindCurrentReaction(ReactionType reaction)
	{
		if (reaction == ReactionType.None)
		{
			return null;
		}
		foreach (FxActionPair pairedAction in PairedActions)
		{
			if (pairedAction.Equals(reaction))
			{
				return pairedAction;
			}
		}
		return null;
	}
}
