using System.Collections.Generic;
using AI.Achievement;
using AI.Player;
using UnityEngine;

public class Spiritshift : GenericAbility
{
	public GameObject form;

	public GenericAbility[] TempAbilities;

	public Equippable[] TempEquipment;

	public Equippable.EquipmentSlot[] LeaveEquipped;

	[Tooltip("If set, the character's base attribute scores will be overridden by those of the Form.")]
	public bool CopyAttributes;

	private NPCAppearance oldAppearance;

	private List<SkinnedMeshRenderer> oldRenderers = new List<SkinnedMeshRenderer>();

	private GameObject oldSkeleton;

	private Animator oldAnimator;

	private MoverData oldMoverData;

	private Cloth capeShroud;

	private bool hasCape;

	private SkinnedMeshRenderer newRenderer;

	private GameObject newSkeleton;

	private List<Equippable> m_ItemsToRestore = new List<Equippable>();

	private List<Equippable> m_ItemsToDestroy = new List<Equippable>();

	private List<GenericAbility> m_abilities = new List<GenericAbility>();

	private float m_activeTimer;

	private float m_commentDelay;

	protected override void Init()
	{
		base.Init();
		if (!CopyAttributes)
		{
			return;
		}
		CharacterStats characterStats = (form ? form.GetComponent<CharacterStats>() : null);
		if ((bool)characterStats)
		{
			for (CharacterStats.AttributeScoreType attributeScoreType = CharacterStats.AttributeScoreType.Resolve; attributeScoreType < CharacterStats.AttributeScoreType.Count; attributeScoreType++)
			{
				StatusEffectParams statusEffectParams = new StatusEffectParams();
				statusEffectParams.AffectsStat = StatusEffect.ModifiedStat.SetBaseAttribute;
				statusEffectParams.AttributeType = attributeScoreType;
				statusEffectParams.Value = characterStats.GetBaseAttribute(attributeScoreType);
				AddStatusEffect(statusEffectParams, AbilityType.Ability, DurationOverride);
			}
		}
	}

	public override void HandleStatsOnAdded()
	{
		base.HandleStatsOnAdded();
		AddSpiritshiftFormAbilities(m_ownerStats);
	}

	public void AddSpiritshiftFormAbilities(CharacterStats character)
	{
		GenericAbility[] tempAbilities = TempAbilities;
		foreach (GenericAbility ability in tempAbilities)
		{
			GenericAbility genericAbility = character.InstantiateAbility(ability, AbilityType.Ability);
			if (genericAbility != null)
			{
				genericAbility.IsVisibleOnUI = false;
				m_abilities.Add(genericAbility);
			}
		}
	}

	public override void HandleStatsOnRemoved()
	{
		base.HandleStatsOnRemoved();
		RemoveSpiritshiftFormAbilities(m_ownerStats);
	}

	public void RemoveSpiritshiftFormAbilities(CharacterStats character)
	{
		foreach (GenericAbility ability in m_abilities)
		{
			character.RemoveAbility(ability);
		}
		m_abilities.Clear();
	}

	public void RestoreTempAbilities()
	{
		if (m_abilities.Count > 0 || !m_ownerStats)
		{
			return;
		}
		for (int i = 0; i < TempAbilities.Length; i++)
		{
			for (int j = 0; j < m_ownerStats.ActiveAbilities.Count; j++)
			{
				if (TempAbilities[i].DisplayName.StringID == m_ownerStats.ActiveAbilities[j].DisplayName.StringID)
				{
					m_ownerStats.ActiveAbilities[j].IsVisibleOnUI = false;
					if (!m_abilities.Contains(m_ownerStats.ActiveAbilities[j]))
					{
						m_abilities.Add(m_ownerStats.ActiveAbilities[j]);
					}
				}
			}
		}
	}

	protected override void Update()
	{
		if (m_activeTimer > 0f)
		{
			m_activeTimer -= Time.deltaTime;
			if (m_activeTimer <= 0f)
			{
				Deactivate(Owner);
			}
		}
		if (m_activated && m_commentDelay > 0f)
		{
			m_commentDelay -= Time.deltaTime;
			if (m_commentDelay <= 0f && DisplayName.StringID == 1682 && DisplayName.StringTable == DatabaseString.StringTableType.Abilities)
			{
				Health.HaveRandomAlivePartyMemberSay(base.OwnerAI, base.OwnerAI ? SoundSet.SoundAction.PartyMemberPolymorphed : SoundSet.SoundAction.EnemyPolymorphed);
			}
		}
		base.Update();
	}

	public override void TriggerFromUI()
	{
		if (m_activated || Ready || base.WhyNotReady == NotReadyValue.InRecovery)
		{
			base.TriggerFromUI();
		}
	}

	public override void Activate(GameObject target)
	{
		base.Activate(target);
		if (m_activated)
		{
			ActivateHelper();
		}
	}

	public override void Activate(Vector3 target)
	{
		base.Activate(target);
		if (m_activated)
		{
			ActivateHelper();
		}
	}

	public override void Deactivate(GameObject target)
	{
		base.Deactivate(target);
		DeactivateHelper();
	}

	private void ActivateHelper()
	{
		AIController aIController = GameUtilities.FindActiveAIController(Owner);
		if (aIController != null)
		{
			if (aIController.StateManager.CurrentState is AI.Achievement.ReloadWeapon)
			{
				aIController.StateManager.PopCurrentState();
			}
			if (aIController.StateManager.CurrentState is AI.Player.ReloadWeapon)
			{
				aIController.StateManager.PopCurrentState();
			}
		}
		Equipment component = Owner.GetComponent<Equipment>();
		if (component == null)
		{
			return;
		}
		component.ClearSummonEffectInSlot(Equippable.EquipmentSlot.PrimaryWeapon);
		component.ClearSummonEffectInSlot(Equippable.EquipmentSlot.SecondaryWeapon);
		Stealth.SetInStealthMode(Owner, inStealth: false);
		if (component.CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Neck) != null)
		{
			Cloth componentInChildren = Owner.GetComponentInChildren<Cloth>();
			if (componentInChildren != null)
			{
				componentInChildren.gameObject.SetActive(value: false);
				hasCape = true;
				capeShroud = componentInChildren;
			}
		}
		for (Equippable.EquipmentSlot equipmentSlot = Equippable.EquipmentSlot.Head; equipmentSlot < Equippable.EquipmentSlot.Count; equipmentSlot++)
		{
			if (LeaveSlotEquipped(equipmentSlot))
			{
				continue;
			}
			Equippable itemInSlot = component.CurrentItems.GetItemInSlot(equipmentSlot);
			if (itemInSlot != null)
			{
				itemInSlot.HandleItemModsOnSpiritshift(Owner);
				Equippable equippable = component.UnEquip(itemInSlot, equipmentSlot);
				if (equippable != null)
				{
					m_ItemsToRestore.Add(equippable);
				}
			}
		}
		SwitchForm();
		Equippable[] tempEquipment = TempEquipment;
		foreach (Equippable equippable2 in tempEquipment)
		{
			Equippable equippable3 = GameResources.Instantiate<Equippable>(equippable2);
			equippable3.Prefab = equippable2;
			m_ItemsToDestroy.Add(equippable3);
			if ((bool)component.Equip(equippable3))
			{
				UIDebug.Instance.LogOnScreenWarning(string.Concat("Spiritshift '", base.name, "' tried to overwrite equipment at '", equippable2.GetPreferredSlot(), "' but there was already something there."), UIDebug.Department.Design, 10f);
			}
		}
		foreach (GenericAbility ability in m_abilities)
		{
			ability.IsVisibleOnUI = true;
		}
		m_ownerStats.LockEquipment();
		if (base.OwnerAI != null)
		{
			base.OwnerAI.StateManager.ClearQueuedStates();
			if (base.OwnerAI.StateManager.CurrentState is AI.Player.Attack attack)
			{
				base.OwnerAI.StateManager.PopAllStates();
				AI.Player.Attack attack2 = AIStateManager.StatePool.Allocate<AI.Player.Attack>();
				base.OwnerAI.StateManager.PushState(attack2);
				attack2.IsAutoAttack = false;
				attack2.Target = attack.CurrentTarget;
			}
		}
		EnableSkinnedMeshes(enable: false);
		m_ownerStats.RecoveryTimer = 0f;
		m_commentDelay = 2f;
		if (BigHeads.Enabled)
		{
			BigHeads.Apply(m_ownerStats.gameObject);
		}
		m_activeTimer = DurationOverride;
		if (m_ownerStats != null)
		{
			m_activeTimer *= m_ownerStats.StatEffectDurationMultiplier;
		}
	}

	private void EnableSkinnedMeshes(bool enable)
	{
		if (oldRenderers == null)
		{
			return;
		}
		foreach (SkinnedMeshRenderer oldRenderer in oldRenderers)
		{
			if (oldRenderer != null)
			{
				oldRenderer.gameObject.SetActive(enable);
			}
		}
	}

	private void ApplyBigHeads(Transform t)
	{
		if (BigHeads.Enabled && t != null)
		{
			BigHeads.Apply(t.gameObject, FindHeadBone(t));
		}
	}

	private void RemoveBigHeads(Transform t)
	{
		if (BigHeads.Enabled && t != null)
		{
			BigHeads.Remove(t.gameObject, FindHeadBone(t));
		}
	}

	private void DeactivateHelper()
	{
		Equipment component = Owner.GetComponent<Equipment>();
		if (component == null)
		{
			return;
		}
		RestoreForm();
		Stealth stealthComponent = Stealth.GetStealthComponent(Owner);
		if ((bool)stealthComponent)
		{
			stealthComponent.Refresh();
		}
		AlphaControl component2 = Owner.GetComponent<AlphaControl>();
		if ((bool)component2)
		{
			component2.Refresh();
		}
		m_ownerStats.UnlockEquipment();
		for (int i = 0; i < m_ItemsToDestroy.Count; i++)
		{
			component.UnEquip(m_ItemsToDestroy[i]);
			GameUtilities.Destroy(m_ItemsToDestroy[i].gameObject);
		}
		m_ItemsToDestroy.Clear();
		if (hasCape)
		{
			bool flag = false;
			foreach (Equippable item in m_ItemsToRestore)
			{
				if (item.Appearance.bodyPiece == AppearancePiece.BodyPiece.Cape)
				{
					flag = true;
				}
			}
			if (flag && capeShroud != null)
			{
				capeShroud.gameObject.SetActive(value: true);
				hasCape = false;
			}
		}
		foreach (Equippable item2 in m_ItemsToRestore)
		{
			component.Equip(item2);
		}
		m_ItemsToRestore.Clear();
		foreach (GenericAbility ability in m_abilities)
		{
			ability.IsVisibleOnUI = false;
		}
		if (base.OwnerAI != null)
		{
			AI.Player.Attack attack = base.OwnerAI.StateManager.CurrentState as AI.Player.Attack;
			if (base.OwnerAI.StateManager.CurrentState is AI.Achievement.Attack attack2)
			{
				attack = base.OwnerAI.StateManager.FindState(typeof(AI.Player.Attack)) as AI.Player.Attack;
				attack2.Interrupt();
			}
			base.OwnerAI.StateManager.ClearQueuedStates();
			if (attack != null)
			{
				base.OwnerAI.StateManager.PopAllStates();
				AI.Player.Attack attack3 = AIStateManager.StatePool.Allocate<AI.Player.Attack>();
				base.OwnerAI.StateManager.PushState(attack3);
				attack3.IsAutoAttack = attack.IsAutoAttack;
				attack3.Target = attack.CurrentTarget;
			}
		}
		m_ownerStats.ClearWildstrikeEffects();
		m_ownerStats.RecoveryTimer = 0f;
		m_activeTimer = 0f;
		UntriggerFromUI();
	}

	protected void SwitchForm()
	{
		if (!Activated || newRenderer != null)
		{
			return;
		}
		GameObject owner = Owner;
		newRenderer = null;
		oldAppearance = owner.GetComponent<NPCAppearance>();
		oldRenderers.Clear();
		SkinnedMeshRenderer[] array = GameUtilities.FindSkinnedMeshRenderers(owner);
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in array)
		{
			if (skinnedMeshRenderer.enabled && skinnedMeshRenderer.gameObject.activeInHierarchy)
			{
				oldRenderers.Add(skinnedMeshRenderer);
			}
		}
		oldSkeleton = GameUtilities.FindSkeleton(owner);
		oldAnimator = GameUtilities.FindAnimator(owner);
		if (oldAppearance == null || oldRenderers == null || oldSkeleton == null || oldAnimator == null)
		{
			return;
		}
		GameObject gameObject = Object.Instantiate(form, owner.transform.position, owner.transform.rotation);
		try
		{
			Animator component = gameObject.GetComponent<Animator>();
			SkinnedMeshRenderer skinnedMeshRenderer2 = GameUtilities.FindSkinnedMeshRenderer(gameObject);
			GameObject gameObject2 = GameUtilities.FindSkeleton(gameObject);
			if (component == null || skinnedMeshRenderer2 == null || gameObject2 == null)
			{
				Debug.LogError("Cannot spiritshift due to bad form! Reason(s) :");
				if (component == null)
				{
					Debug.LogError("--- Missing Animator component!");
				}
				if (skinnedMeshRenderer2 == null)
				{
					Debug.LogError("--- Cannot find skinned mesh renderer!");
				}
				if (gameObject2 == null)
				{
					Debug.LogError("--- Skeleton is missing or not tagged as skeleton!");
				}
				return;
			}
			oldAppearance.enabled = false;
			EnableSkinnedMeshes(enable: false);
			oldSkeleton.SetActive(value: false);
			GameObject gameObject3 = oldSkeleton.gameObject;
			if (oldSkeleton.transform.parent != null)
			{
				gameObject3 = oldSkeleton.transform.parent.gameObject;
			}
			newRenderer = gameObject3.AddComponent<SkinnedMeshRenderer>();
			if (newRenderer == null)
			{
				return;
			}
			newRenderer.material = skinnedMeshRenderer2.material;
			newRenderer.rootBone = skinnedMeshRenderer2.rootBone;
			newRenderer.bones = skinnedMeshRenderer2.bones;
			newSkeleton = Object.Instantiate(gameObject2, owner.transform.position, owner.transform.rotation);
			newSkeleton.transform.parent = oldSkeleton.transform.parent;
			Mover component2 = owner.GetComponent<Mover>();
			Mover component3 = gameObject.GetComponent<Mover>();
			if ((bool)component2 && (bool)component3)
			{
				oldMoverData = new MoverData(component2);
				component2.CopyDataFrom(component3);
				component2.Radius = oldMoverData.Radius;
				if (PartyHelper.IsPartyMember(Owner))
				{
					component2.RunSpeed = component3.RunSpeed + m_ownerStats.GetStatusEffectValueSum(StatusEffect.ModifiedStat.MovementRate);
				}
			}
			oldAnimator.runtimeAnimatorController = component.runtimeAnimatorController;
			oldAnimator.avatar = component.avatar;
			GameUtilities.SetAnimator(owner, oldAnimator);
			ApplyBigHeads(newSkeleton.transform);
		}
		finally
		{
			GameUtilities.Destroy(gameObject);
		}
	}

	protected void RestoreForm()
	{
		if (newRenderer == null)
		{
			return;
		}
		RemoveBigHeads(newSkeleton.transform);
		GameUtilities.Destroy(newRenderer);
		GameUtilities.Destroy(newSkeleton);
		oldAppearance.enabled = true;
		EnableSkinnedMeshes(enable: true);
		oldSkeleton.SetActive(value: true);
		Mover component = Owner.GetComponent<Mover>();
		if ((bool)component && oldMoverData != null)
		{
			oldMoverData.ApplyTo(component);
			oldMoverData = null;
			if (PartyHelper.IsPartyMember(Owner))
			{
				component.RunSpeed = 4f + m_ownerStats.GetStatusEffectValueSum(StatusEffect.ModifiedStat.MovementRate);
			}
		}
		oldAnimator.runtimeAnimatorController = oldAppearance.controller;
		oldAnimator.avatar = oldAppearance.avatar;
		GameUtilities.SetAnimator(Owner, oldAnimator);
		newRenderer = null;
		newSkeleton = null;
		oldRenderers.Clear();
	}

	private bool LeaveSlotEquipped(Equippable.EquipmentSlot slot)
	{
		if (LeaveEquipped == null)
		{
			return false;
		}
		for (int i = 0; i < LeaveEquipped.Length; i++)
		{
			if (LeaveEquipped[i] == slot)
			{
				return true;
			}
		}
		return false;
	}

	private Transform FindHeadBone(Transform t)
	{
		if (t.name.ToLower().Contains("head"))
		{
			return t;
		}
		for (int i = 0; i < t.childCount; i++)
		{
			Transform transform = FindHeadBone(t.GetChild(i));
			if (transform != null)
			{
				return transform;
			}
		}
		return null;
	}

	public override string GetAdditionalEffects(StringEffects stringEffects, StatusEffectFormatMode mode, GenericAbility ability, GameObject character)
	{
		float num = DurationOverride;
		if ((bool)character)
		{
			CharacterStats component = character.GetComponent<CharacterStats>();
			if (component != null)
			{
				num *= component.StatEffectDurationMultiplier;
			}
		}
		string text = TextUtils.FormatBase(DurationOverride, num, (float v) => GUIUtils.Format(211, v.ToString("#0.#")));
		text = AttackBase.FormatWC(GUIUtils.GetText(1634), text);
		return (text + "\n" + base.GetAdditionalEffects(stringEffects, mode, ability, character)).Trim();
	}
}
