using System.Collections.Generic;
using UnityEngine;

public static class BigHeads
{
	private const float s_ScaleAmount = 5f;

	private static bool s_Enabled = false;

	private static List<Transform> s_ScaledHeadTransforms = new List<Transform>();

	public static bool Enabled
	{
		get
		{
			return s_Enabled;
		}
		set
		{
			if (value)
			{
				Enable();
			}
			else
			{
				Disable();
			}
		}
	}

	public static void Enable()
	{
		if (!s_Enabled)
		{
			s_Enabled = true;
			if (!GameUtilities.IsOctoberHoliday())
			{
				Apply();
			}
		}
	}

	public static void Reset()
	{
		s_ScaledHeadTransforms.Clear();
	}

	public static void ValidateList()
	{
		for (int num = s_ScaledHeadTransforms.Count - 1; num >= 0; num--)
		{
			if (s_ScaledHeadTransforms[num] == null)
			{
				s_ScaledHeadTransforms.RemoveAt(num);
			}
		}
	}

	private static void Apply()
	{
		foreach (Faction activeFactionComponent in Faction.ActiveFactionComponents)
		{
			CharacterStats component = activeFactionComponent.gameObject.GetComponent<CharacterStats>();
			if (component != null)
			{
				if (component.CharacterClass != CharacterStats.Class.Druid)
				{
					Apply(activeFactionComponent.gameObject);
				}
				else
				{
					ApplyToDruid(activeFactionComponent.gameObject);
				}
			}
			else
			{
				Apply(activeFactionComponent.gameObject);
			}
		}
	}

	private static void ApplyToDruid(GameObject go)
	{
		ValidateList();
		Transform headTransform = GetHeadTransform(go);
		Apply(go);
		if (!headTransform.gameObject.activeInHierarchy)
		{
			headTransform = FindHeadBone(go.transform);
			if (!s_ScaledHeadTransforms.Contains(headTransform) && headTransform != null)
			{
				headTransform.localScale *= 5f;
				s_ScaledHeadTransforms.Add(headTransform);
			}
		}
	}

	private static void RemoveFromDruid(GameObject go)
	{
		ValidateList();
		Transform headTransform = GetHeadTransform(go);
		Remove(go);
		if (!headTransform.gameObject.activeInHierarchy)
		{
			headTransform = FindHeadBone(go.transform);
			if (headTransform != null)
			{
				headTransform.localScale /= 5f;
				s_ScaledHeadTransforms.Remove(headTransform);
			}
		}
	}

	private static Transform FindHeadBone(Transform t)
	{
		if (t.name.ToLower().Contains("head"))
		{
			if (t.gameObject.activeInHierarchy)
			{
				return t;
			}
		}
		else
		{
			for (int i = 0; i < t.childCount; i++)
			{
				Transform transform = FindHeadBone(t.GetChild(i));
				if (transform != null)
				{
					return transform;
				}
			}
		}
		return null;
	}

	public static void Apply(GameObject go, Transform tr = null)
	{
		if (!GameUtilities.IsOctoberHoliday())
		{
			ValidateList();
			Transform transform = tr ?? GetHeadTransform(go);
			if (!s_ScaledHeadTransforms.Contains(transform) && transform != null)
			{
				transform.localScale *= 5f;
				s_ScaledHeadTransforms.Add(transform);
			}
		}
	}

	public static void Disable()
	{
		if (s_Enabled)
		{
			s_Enabled = false;
			Remove();
		}
	}

	private static void Remove()
	{
		for (int num = s_ScaledHeadTransforms.Count - 1; num >= 0; num--)
		{
			if (s_ScaledHeadTransforms[num] != null)
			{
				s_ScaledHeadTransforms[num].localScale /= 5f;
			}
			s_ScaledHeadTransforms.RemoveAt(num);
		}
	}

	public static void Remove(GameObject go, Transform tr = null)
	{
		ValidateList();
		Transform transform = tr ?? GetHeadTransform(go);
		if (transform != null)
		{
			transform.localScale /= 5f;
			s_ScaledHeadTransforms.Remove(transform);
		}
	}

	private static Transform GetHeadTransform(GameObject go)
	{
		AnimationBoneMapper component = go.GetComponent<AnimationBoneMapper>();
		if (component != null)
		{
			component.Initialize();
			if (component.HasBone(go, AttackBase.EffectAttachType.Head))
			{
				Transform transform = component[go, AttackBase.EffectAttachType.Head];
				if (transform != null)
				{
					return transform.parent;
				}
			}
		}
		AnimationController component2 = go.GetComponent<AnimationController>();
		if (component2 != null)
		{
			Transform transform2 = null;
			if (component2.CurrentAvatar != null)
			{
				transform2 = component2.GetBoneTransform(HumanBodyBones.Head);
			}
			if (transform2 == null)
			{
				transform2 = component2.GetBoneTransform("head", component2.transform);
			}
			return transform2;
		}
		return null;
	}
}
