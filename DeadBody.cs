using UnityEngine;

public class DeadBody : MonoBehaviour
{
	public AnimationClip DeathClip;

	private bool m_FastForwarded;

	private void Start()
	{
		int layer = LayerUtility.FindLayerValue("Dynamics No Occlusion");
		NPCAppearance component = GetComponent<NPCAppearance>();
		if ((bool)component)
		{
			component.layer = layer;
		}
		SkinnedMeshRenderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = layer;
		}
		if (!(DeathClip != null))
		{
			return;
		}
		Animator component2 = GetComponent<Animator>();
		if ((bool)component2)
		{
			AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController();
			if (component2.runtimeAnimatorController == null && (bool)component)
			{
				component2.runtimeAnimatorController = component.controller;
			}
			if (component2.runtimeAnimatorController != null)
			{
				animatorOverrideController.runtimeAnimatorController = component2.runtimeAnimatorController;
				animatorOverrideController[animatorOverrideController.animationClips[0]] = DeathClip;
				component2.runtimeAnimatorController = animatorOverrideController;
			}
		}
	}

	private void Update()
	{
		if (!m_FastForwarded)
		{
			GameUtilities.FastForwardAnimator(GetComponent<Animator>(), 5);
			m_FastForwarded = true;
		}
	}
}
