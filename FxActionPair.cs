using System;
using UnityEngine;

[Serializable]
public class FxActionPair
{
	public AnimationController.ActionType Action;

	public int Variation = 1;

	public AnimationController.ReactionType Reaction;

	public GameObject VisualEffect;

	public override bool Equals(object obj)
	{
		if (obj is AnimationController.Action)
		{
			AnimationController.Action action = obj as AnimationController.Action;
			if (action.m_actionType == Action)
			{
				return action.m_variation == Variation;
			}
			return false;
		}
		if (obj is AnimationController.ActionType && Action != 0)
		{
			return Action == (AnimationController.ActionType)obj;
		}
		if (obj is AnimationController.ReactionType && Reaction != 0)
		{
			return Reaction == (AnimationController.ReactionType)obj;
		}
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return Action.GetHashCode();
	}
}
