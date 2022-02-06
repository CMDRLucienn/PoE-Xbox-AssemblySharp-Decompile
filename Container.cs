using System.Collections.Generic;
using System.Text;
using AI.Plan;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Inventory))]
[AddComponentMenu("Toolbox/Container")]
public class Container : OCL
{
	public DatabaseString LabelName = new DatabaseString(DatabaseString.StringTableType.Interactables, 203);

	[HideInInspector]
	[Persistent]
	public string ManualLabelName;

	public GameObject NotEmptyVFX;

	private bool m_itemRemovedSinceClose;

	[Persistent]
	public bool DeleteMeIfEmpty;

	[Persistent]
	public bool AreaLootable;

	public FactionName StealingFactionID;

	public Reputation.ChangeStrength StealingFactionAdjustment;

	public bool AttackThief;

	[Tooltip("Whether or not allies of the owned faction will attack the thief.")]
	public bool AlliesAttackThief;

	public bool fireItemTakenEventOnlyOnce;

	private bool hasFiredEmptyContainerScript;

	protected Inventory m_inventory;

	private GameObject m_user;

	protected Animator m_anim;

	protected Faction m_faction;

	private PE_Collider2D m_collider2D;

	[Persistent]
	private int onContainerItemTakenCounter;

	private float m_LastTheftBarkTime;

	protected Vector3 m_navPosition = Vector3.zero;

	private static HashSet<Container> s_AllContainers = new HashSet<Container>();

	private bool m_mouseInsideContainer;

	private HighlightCharacter m_Highlighter;

	private bool m_deadBodyContainer;

	public virtual bool IsEmpty
	{
		get
		{
			if (!m_inventory || m_inventory.ItemList == null)
			{
				return true;
			}
			return m_inventory.ItemList.Count == 0;
		}
	}

	[Persistent]
	public bool HasInteracted { get; set; }

	[Persistent]
	public bool PlayerHasBeenCaughtStealingFrom { get; set; }

	public bool CanAreaLootContainer
	{
		get
		{
			if (AreaLootable)
			{
				return GameState.Option.AreaLootRange > 0f;
			}
			return false;
		}
	}

	public bool IsOwned
	{
		get
		{
			if (m_faction != null && !m_faction.IsInPlayerFaction)
			{
				return true;
			}
			return false;
		}
	}

	public override bool IsUsable
	{
		get
		{
			//!+ ADDED CODE
			if (IEModOptions.UnlockCombatInv)
			{
				if (GameState.InCombat
					&& (this.gameObject.name.Contains("DefaultDropItem") || this.gameObject.GetComponent<CharacterStats>() != null))
				// this is a check for ground loot or body loot
				{
					return false;
				}
				return !this.IsEmptyDeadBody() && base.IsUsable;
			}
			//!+ END ADD
			return !this.IsEmptyDeadBody() && !GameState.InCombat && base.IsUsable;

		}
	}

	private void OnEnable()
	{
		if ((bool)NotEmptyVFX)
		{
			NotEmptyVFX.SetActive(value: true);
		}
	}

	private void OnDisable()
	{
		if ((bool)NotEmptyVFX)
		{
			NotEmptyVFX.SetActive(value: false);
		}
	}

	protected override void Start()
	{
		base.Start();
		m_anim = GetComponent<Animator>();
		m_inventory = GetComponent<Inventory>();
		m_faction = GetComponent<Faction>();
		m_collider2D = GetComponent<PE_Collider2D>();
		m_inventory.OnRemoved += HandleItemRemoved;
		m_deadBodyContainer = ((GetComponent<DeadBody>() != null) ? true : false);
		s_AllContainers.Add(this);
		Vector3 position = base.transform.position;
		if (InteractionObject != null)
		{
			position = InteractionObject.transform.position;
		}
		m_Highlighter = GetComponent<HighlightCharacter>();
		if (NavMesh.SamplePosition(position, out var hit, 10f, int.MaxValue))
		{
			m_navPosition = hit.position;
		}
		else
		{
			m_navPosition = base.transform.position;
		}
		if ((bool)NotEmptyVFX)
		{
			GameObject gameObject = (NotEmptyVFX = Object.Instantiate(NotEmptyVFX));
			if ((bool)NotEmptyVFX)
			{
				NotEmptyVFX.gameObject.transform.localPosition = GetBestPositionForVFXTracking();
			}
		}
	}

	protected override void OnDestroy()
	{
		if (NotEmptyVFX != null)
		{
			GameUtilities.Destroy(NotEmptyVFX);
		}
		if (GameCursor.GenericUnderCursor == base.gameObject)
		{
			GameCursor.GenericUnderCursor = null;
		}
		s_AllContainers.Remove(this);
		m_deadBodyContainer = false;
		base.OnDestroy();
	}

	private Vector3 GetBestPositionForVFXTracking()
	{
		AnimationBoneMapper component = GetComponent<AnimationBoneMapper>();
		if ((bool)component && component.HasBone(base.gameObject, AttackBase.EffectAttachType.Chest))
		{
			return component[base.gameObject, AttackBase.EffectAttachType.Chest].position;
		}
		SkinnedMeshRenderer[] componentsInChildren = GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren)
		{
			if (skinnedMeshRenderer.name == "Mesh")
			{
				return skinnedMeshRenderer.bounds.center;
			}
		}
		return base.gameObject.transform.localPosition;
	}

	protected override void Update()
	{
		base.Update();
		if ((bool)NotEmptyVFX)
		{
			NotEmptyVFX.gameObject.transform.localPosition = GetBestPositionForVFXTracking();
		}
		if (!m_collider2D)
		{
			if (GameUtilities.DoesMouseIntersect(base.gameObject))
			{
				if (!m_mouseInsideContainer)
				{
					MouseOver();
				}
				m_mouseInsideContainer = true;
			}
			else if (m_mouseInsideContainer)
			{
				MouseExit();
				m_mouseInsideContainer = false;
			}
		}
		if (GameCursor.ObjectUnderCursor == base.gameObject && GameInput.GetControlUp(MappedControl.INTERACT, handle: true))
		{
			GameState.s_playerCharacter.ObjectClicked(this);
		}
	}

	public override bool Open(GameObject user, bool ignoreLock)
	{
		if (!base.Open(user, ignoreLock))
		{
			return false;
		}
		m_user = user;
		Loot component = GetComponent<Loot>();
		if ((bool)component && !HasInteracted)
		{
			component.PopulateInventory();
		}
		HasInteracted = true;
		if ((bool)user)
		{
			PartyMemberAI component2 = user.GetComponent<PartyMemberAI>();
			if ((bool)component2)
			{
				if (CanAreaLootContainer)
				{
					LootAllContainersInArea(component2, ignoreLock);
				}
				else
				{
					ShowSingleContainerLootUI(component2);
				}
			}
		}
		if ((bool)m_anim)
		{
			m_anim.SetInteger("CurrentState", (int)m_currentState);
		}
		return true;
	}

	public override bool Close(GameObject user)
	{
		if (!base.Close(user))
		{
			return false;
		}
		if ((bool)m_anim)
		{
			m_anim.SetInteger("CurrentState", (int)m_currentState);
		}
		if (m_inventory.Empty())
		{
			if (!hasFiredEmptyContainerScript)
			{
				ScriptEvent component = GetComponent<ScriptEvent>();
				if ((bool)component)
				{
					component.ExecuteScript(ScriptEvent.ScriptEvents.OnContainerEmpty);
					hasFiredEmptyContainerScript = true;
				}
			}
			if (m_deadBodyContainer && HasInteracted)
			{
				MouseExit();
			}
		}
		if (m_itemRemovedSinceClose)
		{
			Loot component2 = GetComponent<Loot>();
			if ((bool)component2)
			{
				component2.RefreshMeshCollider();
			}
			m_itemRemovedSinceClose = false;
		}
		return true;
	}

	public void CloseInventoryAreaLoot()
	{
		CombinedInventory component = UILootManager.Instance.gameObject.GetComponent<CombinedInventory>();
		if (component == null)
		{
			return;
		}
		foreach (InventoryItem item in m_inventory.ItemList)
		{
			item.AreaLootSource = false;
		}
		List<Inventory> combinedInventories = component.CombinedInventories;
		for (int i = 0; i < combinedInventories.Count; i++)
		{
			Container component2 = combinedInventories[i].GetComponent<Container>();
			if (component2 != null)
			{
				component2.CloseInventoryCB();
			}
		}
		GameUtilities.Destroy(component);
	}

	public void CloseInventoryCB()
	{
		if (DeleteMeIfEmpty && IsEmpty)
		{
			if (GetComponent<CharacterStats>() != null)
			{
				GameUtilities.Destroy(this);
			}
			else
			{
				Persistence component = GetComponent<Persistence>();
				if ((bool)component)
				{
					component.SetForDestroy();
				}
				GameUtilities.Destroy(base.gameObject);
			}
		}
		else
		{
			Close(null);
		}
		SetHighlightTarget(enabled: false);
	}

	private void MouseOver()
	{
		if (FogOfWar.PointVisibleInFog(base.transform.position))
		{
			if (!IsUsable)
			{
				GameCursor.UnusableUnderCursor = this;
				return;
			}
			GameCursor.GenericUnderCursor = base.gameObject;
			NotifyMouseOver(state: true);
		}
	}

	private void MouseExit()
	{
		if (GameCursor.UnusableUnderCursor == this)
		{
			GameCursor.UnusableUnderCursor = null;
		}
		if (GameCursor.GenericUnderCursor == base.gameObject)
		{
			GameCursor.GenericUnderCursor = null;
			NotifyMouseOver(state: false);
		}
	}

	public override void NotifyMouseOver(bool state)
	{
		if (CanAreaLootContainer)
		{
			HashSet<Container> hashSet = FindAreaLootableContainersInRangeOfUs(base.transform.position);
			{
				foreach (Container s_AllContainer in s_AllContainers)
				{
					s_AllContainer.SetHighlightTarget(state && hashSet != null && hashSet.Contains(s_AllContainer));
				}
				return;
			}
		}
		SetHighlightTarget(state);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.gray;
		Gizmos.DrawWireSphere(base.transform.position, UsableRadius);
	}

	public bool IsEmptyDeadBody()
	{
		if (m_deadBodyContainer && HasInteracted && IsEmpty)
		{
			return true;
		}
		return false;
	}

	private void HandleItemRemoved(BaseInventory sender, Item item, int removedQty, bool originalItem)
	{
		ScriptEvent component = GetComponent<ScriptEvent>();
		if ((bool)component)
		{
			if (onContainerItemTakenCounter < 1)
			{
				component.ExecuteScript(ScriptEvent.ScriptEvents.OnContainerItemTaken);
				onContainerItemTakenCounter++;
			}
			else if (!fireItemTakenEventOnlyOnce)
			{
				component.ExecuteScript(ScriptEvent.ScriptEvents.OnContainerItemTaken);
			}
		}
		if (!originalItem)
		{
			return;
		}
		if (item != null)
		{
			Equippable equippable = item as Equippable;
			Weapon weapon = item as Weapon;
			Equipment component2 = GetComponent<Equipment>();
			if ((bool)component2 && !weapon && (bool)equippable)
			{
				component2.UnEquip(equippable);
			}
			if ((bool)component2 && (bool)weapon)
			{
				component2.UnEquipWeapon(weapon);
			}
			NPCAppearance component3 = GetComponent<NPCAppearance>();
			if ((bool)component3)
			{
				component3.Generate();
			}
			m_itemRemovedSinceClose = true;
		}
		if (!IsOwned || m_user == null)
		{
			return;
		}
		float range = 13f;
		bool flag = false;
		GameObject[] array = GameUtilities.CreaturesInRange(base.transform.position, range, playerEnemiesOnly: false, includeUnconscious: false);
		if (array != null && StealingFactionID != 0)
		{
			GameObject[] array2 = array;
			foreach (GameObject gameObject in array2)
			{
				CharacterStats component4 = gameObject.GetComponent<CharacterStats>();
				Faction component5 = gameObject.GetComponent<Faction>();
				if (!(component4 != null) || !(component5 != null) || component5.IsInPlayerFaction)
				{
					continue;
				}
				Stealth stealthComponent = Stealth.GetStealthComponent(m_user.gameObject);
				if ((!(stealthComponent == null) && stealthComponent.IsInStealthMode() && !(stealthComponent.GetSuspicion(gameObject) >= 200f)) || !GameUtilities.LineofSight(gameObject.transform.position, m_user, 1f))
				{
					continue;
				}
				if ((AttackThief && component5.CurrentTeam == m_faction.CurrentTeam) || (AlliesAttackThief && component5.CurrentTeam.GetRelationship(m_faction.CurrentTeam) == Faction.Relationship.Friendly))
				{
					flag = true;
					component5.RelationshipToPlayer = Faction.Relationship.Hostile;
					AIPackageController component6 = gameObject.GetComponent<AIPackageController>();
					if (!(component6 != null))
					{
						break;
					}
					AttackBase primaryAttack = AIController.GetPrimaryAttack(gameObject);
					if (primaryAttack != null && !component6.IsConfused)
					{
						ApproachTarget approachTarget = AIStateManager.StatePool.Allocate<ApproachTarget>();
						component6.StateManager.PushState(approachTarget);
						approachTarget.TargetScanner = component6.GetTargetScanner();
						approachTarget.Target = m_user;
						if (approachTarget.TargetScanner == null)
						{
							approachTarget.Attack = primaryAttack;
						}
					}
					break;
				}
				if ((bool)component5.CurrentTeam && (bool)m_faction.CurrentTeam && (component5.CurrentTeam == m_faction.CurrentTeam || component5.CurrentTeam.GetRelationship(m_faction.CurrentTeam) == Faction.Relationship.Friendly))
				{
					flag = true;
				}
			}
		}
		if (!flag)
		{
			return;
		}
		if ((bool)component)
		{
			component.ExecuteScript(ScriptEvent.ScriptEvents.OnCaughtItemStealing);
		}
		if (!PlayerHasBeenCaughtStealingFrom)
		{
			PlayerHasBeenCaughtStealingFrom = true;
			CreateTheftNotice();
			if (StealingFactionID != 0)
			{
				ReputationManager.Instance.GetReputation(StealingFactionID)?.AddReputation(Reputation.Axis.Negative, StealingFactionAdjustment);
			}
		}
	}

	private HashSet<Container> FindAreaLootableContainersInRangeOfUs(Vector3 pos)
	{
		float num = GameState.Option.AreaLootRange * GameState.Option.AreaLootRange;
		HashSet<Container> hashSet = null;
		foreach (Container s_AllContainer in s_AllContainers)
		{
			if (!(s_AllContainer == null) && s_AllContainer.AreaLootable && !(GameUtilities.V3SqrDistance2D(base.transform.position, s_AllContainer.transform.position) > num))
			{
				if (hashSet == null)
				{
					hashSet = new HashSet<Container>();
				}
				hashSet.Add(s_AllContainer);
			}
		}
		return hashSet;
	}

	private void LootAllContainersInArea(PartyMemberAI partyMemberLooting, bool ignoreLock)
	{
		List<Inventory> list = new List<Inventory>();
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		HashSet<Container> hashSet = FindAreaLootableContainersInRangeOfUs(base.transform.position);
		if (hashSet == null)
		{
			return;
		}
		foreach (Container item in hashSet)
		{
			if (item == null || (item != this && !item.BaseOpen(partyMemberLooting.gameObject, ignoreLock)))
			{
				continue;
			}
			Loot component = item.GetComponent<Loot>();
			if ((bool)component && !item.HasInteracted)
			{
				component.PopulateInventory();
			}
			item.HasInteracted = true;
			Inventory component2 = item.GetComponent<Inventory>();
			if (!component2)
			{
				continue;
			}
			if (item == this)
			{
				list.Insert(0, component2);
			}
			else
			{
				list.Add(component2);
			}
			if (dictionary.ContainsKey(item.ManualLabelName))
			{
				dictionary[item.ManualLabelName]++;
			}
			else
			{
				dictionary.Add(item.ManualLabelName, 1);
			}
			foreach (InventoryItem item2 in component2.ItemList)
			{
				item2.AreaLootSource = this == item;
			}
		}
		if (list.Count <= 1)
		{
			ShowSingleContainerLootUI(partyMemberLooting);
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = true;
		foreach (string key in dictionary.Keys)
		{
			if (!flag)
			{
				stringBuilder.Append(", ");
			}
			if (dictionary[key] > 1)
			{
				stringBuilder.AppendFormat("{0}(x{1})", key, dictionary[key]);
			}
			else
			{
				stringBuilder.AppendFormat("{0}", key);
			}
			flag = false;
		}
		CombinedInventory combinedInventory = UILootManager.Instance.gameObject.AddComponent<CombinedInventory>();
		combinedInventory.Initialize(stringBuilder.ToString(), list.ToArray());
		UILootManager.Instance.SetData(partyMemberLooting, combinedInventory, base.gameObject);
		UILootManager.Instance.ShowWindow();
		combinedInventory.CloseInventoryCB = CloseInventoryAreaLoot;
	}

	private void ShowSingleContainerLootUI(PartyMemberAI user)
	{
		UILootManager.Instance.SetData(user, m_inventory, base.gameObject);
		UILootManager.Instance.ShowWindow();
		m_inventory.CloseInventoryCB = CloseInventoryCB;
	}

	private bool BaseOpen(GameObject user, bool ignoreLock)
	{
		return base.Open(user, ignoreLock);
	}

	private void SetHighlightTarget(bool enabled)
	{
		if (m_Highlighter == null)
		{
			m_Highlighter = GetComponent<HighlightCharacter>();
		}
		if ((bool)m_Highlighter)
		{
			m_Highlighter.Targeted = enabled;
		}
	}

	private void CreateTheftNotice()
	{
		if (m_LastTheftBarkTime != TimeController.Instance.RealtimeSinceStartupThisFrame)
		{
			string empty = string.Empty;
			if (m_user == GameState.s_playerCharacter.gameObject || m_user == null)
			{
				empty = GUIUtils.GetText(1646);
			}
			else
			{
				CharacterStats component = m_user.GetComponent<CharacterStats>();
				empty = ((!component) ? GUIUtils.GetText(1646) : GUIUtils.Format(1647, component.Name()));
			}
			Console.AddMessage(empty);
			UIHealthstringManager.Instance.ShowWarning(empty, m_user, 3f);
			m_LastTheftBarkTime = TimeController.Instance.RealtimeSinceStartupThisFrame;
		}
	}
}
