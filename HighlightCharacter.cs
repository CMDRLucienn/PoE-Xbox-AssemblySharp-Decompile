using System;
using System.Collections.Generic;
using UnityEngine;

public class HighlightCharacter : MonoBehaviour
{
	private List<Material> m_materials = new List<Material>();

	private Faction m_Faction;

	private Container m_Container;

	private AutoLootContainer m_AutoLootContainer;

	private Health m_Health;

	private bool m_IsCharacter;

	private Door m_Door;

	private const float DOOR_ALPHA = 0.35f;

	private PE_Collider2D m_collider2d;

	private float m_TailAlpha;

	public bool LassoSelected;

	public bool LassoDeselected;

	public bool Targeted;

	public bool DesignOverride;

	public Color ColorOverride = Color.white;

	public bool OccludedByScene;

	private bool m_IsHighlighted;

	private int m_HighlightPropertyID;

	private int m_HighlightOcclusionID;

	public bool ShouldHighlight
	{
		get
		{
			if (!LassoSelected)
			{
				return Targeted;
			}
			return true;
		}
	}

	public bool ShouldTarget => Targeted;

	private void Awake()
	{
		m_HighlightPropertyID = Shader.PropertyToID("_Highlight");
		m_HighlightOcclusionID = Shader.PropertyToID("_HighlightOccludedAlpha");
	}

	private void Start()
	{
		m_collider2d = GetComponent<PE_Collider2D>();
		m_Container = GetComponent<Container>();
		m_AutoLootContainer = GetComponent<AutoLootContainer>();
		m_Faction = GetComponent<Faction>();
		m_Door = GetComponent<Door>();
		m_Health = GetComponent<Health>();
		m_IsCharacter = GetComponent<CharacterStats>();
		ChangeColor();
		NPCAppearance component = GetComponent<NPCAppearance>();
		if ((bool)component)
		{
			component.OnPostGenerate = (NPCAppearance.OnGenerate)Delegate.Combine(component.OnPostGenerate, new NPCAppearance.OnGenerate(FindMaterials));
		}
		Loot component2 = GetComponent<Loot>();
		if ((bool)component2)
		{
			component2.OnLootDropped += CheckForContainer;
		}
		Equipment component3 = GetComponent<Equipment>();
		if ((bool)component3)
		{
			component3.OnEquipmentChanged += HandleOnEquipmentChanged;
		}
		FindMaterials();
	}

	private void OnDestroy()
	{
		NPCAppearance component = GetComponent<NPCAppearance>();
		if ((bool)component)
		{
			component.OnPostGenerate = (NPCAppearance.OnGenerate)Delegate.Remove(component.OnPostGenerate, new NPCAppearance.OnGenerate(FindMaterials));
		}
		Loot component2 = GetComponent<Loot>();
		if ((bool)component2)
		{
			component2.OnLootDropped -= CheckForContainer;
		}
		Equipment component3 = GetComponent<Equipment>();
		if ((bool)component3)
		{
			component3.OnEquipmentChanged -= HandleOnEquipmentChanged;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		bool flag = m_TailAlpha > 0f;
		bool shouldHighlight = GetShouldHighlight();
		if (!shouldHighlight)
		{
			m_TailAlpha -= TimeController.sUnscaledDelta / InGameHUD.Instance.HighlightTailDur;
		}
		m_IsHighlighted = shouldHighlight;
		if (m_IsHighlighted || flag)
		{
			ChangeColor();
		}
	}

	private bool GetShouldHighlight()
	{
		bool flag = ShouldHighlight;
		if (!flag && !m_IsCharacter)
		{
			flag = (GameCursor.ObjectUnderCursor == base.gameObject && !GameState.s_playerCharacter.IsDragSelecting) || (InGameHUD.Instance.HighlightActive && !m_Faction);
		}
		if (!InGameHUD.Instance.ShowHUD)
		{
			flag = false;
		}
		if ((bool)m_collider2d)
		{
			flag = m_collider2d.ShouldRender;
		}
		if (!flag)
		{
			return false;
		}
		if ((bool)m_Faction && !m_Faction.CanBeTargeted)
		{
			return false;
		}
		if (m_IsCharacter && (bool)m_Health && !GameState.InCombat && !m_Health.ShowDead)
		{
			return false;
		}
		if ((bool)m_Door && !m_Door.IsUsable)
		{
			return false;
		}
		if ((bool)m_Container && (m_Container.IsEmptyDeadBody() || GameState.InCombat))
		{
			return false;
		}
		if ((bool)m_AutoLootContainer && GameState.InCombat)
		{
			return false;
		}
		return true;
	}

	private void CheckForContainer(GameObject go, GameEventArgs args)
	{
		m_Container = GetComponent<Container>();
	}

	private void ChangeColor()
	{
		if (m_IsHighlighted)
		{
			m_TailAlpha = 1f;
		}
		else
		{
			m_TailAlpha = Mathf.Clamp01(m_TailAlpha);
		}
		Color value = InGameHUD.Instance.HighlightInteractable;
		if (DesignOverride)
		{
			value = ColorOverride;
		}
		else if ((bool)m_collider2d)
		{
			value = m_collider2d.DrawColor;
		}
		else if ((bool)m_Container)
		{
			value = InGameHUD.Instance.HighlightInteractable;
		}
		else if ((bool)m_Faction)
		{
			switch (m_Faction.RelationshipToPlayer)
			{
			case Faction.Relationship.Friendly:
				value = ((!GameState.Option.GetOption(GameOption.BoolOption.COLORBLIND_MODE)) ? InGameHUD.Instance.HighlightFriend : InGameHUD.Instance.HighlightFriendColorblind);
				break;
			case Faction.Relationship.Hostile:
				value = InGameHUD.Instance.HighlightFoe;
				break;
			case Faction.Relationship.Neutral:
				value = InGameHUD.Instance.HighlightNeutral;
				break;
			}
			float num = (value.r + value.g + value.b) / 3f;
			value.r = Mathf.Clamp01(num + InGameHUD.Instance.HighlightSaturation * (value.r - num));
			value.g = Mathf.Clamp01(num + InGameHUD.Instance.HighlightSaturation * (value.g - num));
			value.b = Mathf.Clamp01(num + InGameHUD.Instance.HighlightSaturation * (value.b - num));
		}
		float num2 = value.a;
		if ((bool)m_Door)
		{
			num2 *= 0.35f;
		}
		value.a = m_TailAlpha * num2 * InGameHUD.Instance.HighlightTweenAlpha;
		for (int i = 0; i < m_materials.Count; i++)
		{
			m_materials[i].SetColor(m_HighlightPropertyID, value);
			m_materials[i].SetFloat(m_HighlightOcclusionID, OccludedByScene ? 0f : 1f);
		}
	}

	private void HandleOnEquipmentChanged(Equippable.EquipmentSlot slot, Equippable oldEq, Equippable newEq, bool swappingSummonedWeapon, bool enforceRecoveryPenalty)
	{
		FindMaterials();
	}

	private void FindMaterials()
	{
		m_materials.Clear();
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		foreach (Renderer obj in componentsInChildren)
		{
			Material[] collection = (obj.sharedMaterials = obj.materials);
			m_materials.AddRange(collection);
		}
		ChangeColor();
	}
}
