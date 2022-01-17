using System;
using System.Collections;
using UnityEngine;

public class Loot : MonoBehaviour
{
	public LootList LootList;

	public bool DropInventory = true;

	public bool DropEquipment;

	[Tooltip("If this is checked and this is a character that dies, then the loot will be placed in the body. If this is false, the body will be faded out and loot bag will be dropped instead.")]
	public bool UseBodyAsLootBag = true;

	[Tooltip("If you don't want to use the default loot bag visuals, specify a different one here.")]
	public GameObject LootBagOverride;

	[Persistent]
	private bool m_populate;

	[Persistent]
	private bool m_lootDroppedOnBody;

	[Persistent]
	private bool m_dropLootOnDeathComplete;

	private MeshCollider m_lootMeshCollider;

	private IEnumerator m_creatingMeshColliderCorouting;

	public event GameInputEventHandle OnLootDropped;

	private void Awake()
	{
		Health component = GetComponent<Health>();
		if ((bool)component)
		{
			component.OnDeath += PrepareItemDrop;
			component.OnGibbed += OnGibbed;
		}
	}

	public void Restored()
	{
		if ((bool)GetComponent<Health>() && (m_lootDroppedOnBody || m_dropLootOnDeathComplete))
		{
			PopulateInventory();
			BaseInventory inventoryComponent = GetInventoryComponent();
			if (inventoryComponent != null && inventoryComponent.ItemList.Count > 0)
			{
				CreateLootBag(base.gameObject, null, dropInventory: true, dropEquipment: false, inventoryComponent);
			}
			m_lootDroppedOnBody = false;
			m_dropLootOnDeathComplete = false;
			PersistenceManager.SaveComponentForObject(base.gameObject, GetType());
		}
	}

	private bool DeathAnimationComplete()
	{
		if ((bool)GetComponent<FXAnimationController>())
		{
			return true;
		}
		Animator[] componentsInChildren = GetComponentsInChildren<Animator>();
		if (componentsInChildren == null || componentsInChildren.Length == 0)
		{
			return true;
		}
		Animator[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			AnimatorStateInfo currentAnimatorStateInfo = array[i].GetCurrentAnimatorStateInfo(0);
			float num = (UseBodyAsLootBag ? 1f : 0.1f);
			if (currentAnimatorStateInfo.IsTag("dead") && currentAnimatorStateInfo.normalizedTime >= num)
			{
				return true;
			}
		}
		return false;
	}

	public void RefreshMeshCollider()
	{
		if ((bool)m_lootMeshCollider)
		{
			if (m_creatingMeshColliderCorouting != null)
			{
				StopCoroutine(m_creatingMeshColliderCorouting);
			}
			m_creatingMeshColliderCorouting = CreateMeshForCollider();
			StartCoroutine(m_creatingMeshColliderCorouting);
		}
	}

	private IEnumerator CreateMeshForCollider()
	{
		if ((bool)m_lootMeshCollider)
		{
			GameUtilities.DestroyImmediate(m_lootMeshCollider);
		}
		MeshCollider meshCollider = base.gameObject.AddComponent<MeshCollider>();
		meshCollider.enabled = true;
		m_lootMeshCollider = meshCollider;
		SkinnedMeshRenderer[] componentsInChildren = GetComponentsInChildren<SkinnedMeshRenderer>();
		CombineInstance[] array = new CombineInstance[componentsInChildren.Length];
		int num = 0;
		Transform transform = base.transform;
		Vector3 localPosition = transform.localPosition;
		Quaternion localRotation = transform.localRotation;
		Vector3 localScale = transform.localScale;
		transform.position = Vector3.zero;
		transform.rotation = Quaternion.identity;
		transform.localScale = Vector3.one;
		AlphaControl component = GetComponent<AlphaControl>();
		if ((bool)component)
		{
			component.TempEnableRenderers();
		}
		Animator component2 = GetComponent<Animator>();
		AnimatorCullingMode cullingMode = AnimatorCullingMode.CullUpdateTransforms;
		if ((bool)component2)
		{
			cullingMode = component2.cullingMode;
			component2.cullingMode = AnimatorCullingMode.AlwaysAnimate;
			component2.Update(0f);
		}
		SkinnedMeshRenderer[] array2 = componentsInChildren;
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in array2)
		{
			if (skinnedMeshRenderer.enabled)
			{
				Mesh mesh = new Mesh();
				skinnedMeshRenderer.BakeMesh(mesh);
				if (!(mesh == null))
				{
					array[num].mesh = mesh;
					Matrix4x4 identity = Matrix4x4.identity;
					identity.SetTRS(skinnedMeshRenderer.transform.position, skinnedMeshRenderer.transform.rotation, Vector3.one);
					array[num].transform = identity;
					num++;
				}
			}
		}
		Mesh mesh2 = new Mesh();
		mesh2.CombineMeshes(array, mergeSubMeshes: true, useMatrices: true);
		meshCollider.sharedMesh = mesh2;
		transform.localPosition = localPosition;
		transform.localRotation = localRotation;
		transform.localScale = localScale;
		float num2 = 7f;
		int num3 = 9000;
		if (meshCollider.bounds.size.magnitude <= num2 && meshCollider.sharedMesh.triangles.Length < num3)
		{
			meshCollider.convex = true;
		}
		if ((bool)component)
		{
			component.Refresh();
		}
		if ((bool)component2)
		{
			component2.cullingMode = cullingMode;
		}
		m_creatingMeshColliderCorouting = null;
		yield return null;
	}

	private void Update()
	{
		if (m_dropLootOnDeathComplete && DeathAnimationComplete())
		{
			m_dropLootOnDeathComplete = false;
			DropAllItems();
		}
	}

	public void OnGibbed(GameObject parent, GameEventArgs args)
	{
		if (m_dropLootOnDeathComplete)
		{
			m_dropLootOnDeathComplete = false;
			DropAllItems();
			Persistence component = GetComponent<Persistence>();
			if ((bool)component)
			{
				PersistenceManager.SaveObject(component);
			}
		}
	}

	private void OnDestroy()
	{
		Health component = GetComponent<Health>();
		if ((bool)component)
		{
			component.OnDeath -= PrepareItemDrop;
			component.OnGibbed -= OnGibbed;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void CreateLootBag(GameObject parent, object[] lootList, bool dropInventory, bool dropEquipment, BaseInventory parentInventory)
	{
		GameObject gameObject = null;
		if (gameObject == null)
		{
			if ((bool)LootBagOverride)
			{
				gameObject = GameResources.Instantiate<GameObject>(LootBagOverride, base.transform.position, Quaternion.identity);
			}
			else
			{
				Container container = GameResources.LoadPrefab<Container>("DefaultDropItem", instantiate: false);
				if (container == null)
				{
					Debug.LogError("DefaultDropItem prefab not found!");
					return;
				}
				gameObject = GameResources.Instantiate<GameObject>(container.gameObject, base.transform.position, Quaternion.identity);
			}
			gameObject.tag = "DropItem";
			gameObject.layer = LayerUtility.FindLayerValue("Dynamics");
			gameObject.transform.localRotation = Quaternion.AngleAxis(OEIRandom.AngleDegrees(), Vector3.up);
			InstanceID component = gameObject.GetComponent<InstanceID>();
			component.Guid = Guid.NewGuid();
			component.UniqueID = component.Guid.ToString();
			gameObject.GetComponent<Persistence>().TemplateOnly = true;
		}
		Container component2 = gameObject.GetComponent<Container>();
		component2.ManualLabelName = CharacterStats.Name(parent);
		if ((bool)component2)
		{
			component2.DeleteMeIfEmpty = true;
			component2.AreaLootable = true;
		}
		CopyLootObjectsToInventory(parent, gameObject, lootList, dropInventory, dropEquipment, parentInventory);
		AlphaControl alphaControl = gameObject.AddComponent<AlphaControl>();
		if ((bool)alphaControl)
		{
			alphaControl.Alpha = 0f;
			alphaControl.FadeIn(1f);
		}
		parentInventory.ClearInventory(deleteItems: false);
	}

	public void PrepareItemDrop(GameObject parent, GameEventArgs args)
	{
		Health component = base.gameObject.GetComponent<Health>();
		if ((bool)component && component.ShouldGib)
		{
			DropAllItems();
		}
		else
		{
			m_dropLootOnDeathComplete = true;
		}
	}

	public void DropAllItems()
	{
		Health component = base.gameObject.GetComponent<Health>();
		if (component != null && !component.ShouldDecay)
		{
			return;
		}
		BaseInventory inventoryComponent = GetInventoryComponent();
		bool flag = false;
		object[] array = null;
		if (LootList != null)
		{
			SetSeed();
			array = LootList.Evaluate();
			ResetSeed();
		}
		if (array != null && array.Length != 0)
		{
			flag = true;
		}
		if (inventoryComponent != null && DropInventory && inventoryComponent.ItemList.Count > 0)
		{
			flag = true;
		}
		if (DropEquipment)
		{
			flag = true;
		}
		if (!flag)
		{
			if (!UseBodyAsLootBag)
			{
				BeginFade();
			}
			return;
		}
		if (component.ShouldGib || !UseBodyAsLootBag)
		{
			CreateLootBag(base.gameObject, array, DropInventory, DropEquipment, inventoryComponent);
			if (!UseBodyAsLootBag)
			{
				BeginFade();
			}
			Persistence component2 = GetComponent<Persistence>();
			if ((bool)component2)
			{
				PersistenceManager.SaveObject(component2);
			}
		}
		else
		{
			CreateLootOnBody(base.gameObject, array, DropInventory, DropEquipment, inventoryComponent);
		}
		if (this.OnLootDropped != null)
		{
			this.OnLootDropped(base.gameObject, null);
		}
	}

	private void BeginFade()
	{
		AlphaControl component = GetComponent<AlphaControl>();
		if ((bool)component)
		{
			float num = 1f;
			component.FadeOut(num);
			component.LockAlphaControl();
			GameUtilities.Destroy(base.gameObject, num + 0.5f);
		}
	}

	private void CopyLootObjectsToInventory(GameObject parent, GameObject dropObject, object[] lootList, bool dropInventory, bool dropEquipment, BaseInventory parentInventory)
	{
		Inventory component = dropObject.GetComponent<Inventory>();
		component.MaxItems = int.MaxValue;
		if ((bool)component)
		{
			if (parent == dropObject)
			{
				if (!dropInventory)
				{
					component.ClearInventory(deleteItems: true);
				}
			}
			else if (dropInventory && parentInventory != null)
			{
				foreach (InventoryItem item in parentInventory.ItemList)
				{
					DropItem(component, item);
				}
			}
		}
		if (lootList != null)
		{
			for (int i = 0; i < lootList.Length; i++)
			{
				if (lootList[i] is GameObject)
				{
					Item component2 = (lootList[i] as GameObject).GetComponent<Item>();
					if ((bool)component2)
					{
						DropItem(component, component2, 1);
					}
				}
			}
			m_populate = true;
		}
		if (dropEquipment)
		{
			Equipment component3 = parent.GetComponent<Equipment>();
			if ((bool)component3)
			{
				foreach (Equippable currentItem in component3.CurrentItems)
				{
					if (!(currentItem == null))
					{
						DropItem(component, currentItem, 1);
					}
				}
				if ((bool)component3.CurrentItems.PushedPrimaryWeapon)
				{
					DropItem(component, component3.CurrentItems.PushedPrimaryWeapon, 1);
				}
				if ((bool)component3.CurrentItems.PushedSecondaryWeapon)
				{
					DropItem(component, component3.CurrentItems.PushedSecondaryWeapon, 1);
				}
			}
			QuickbarInventory component4 = parent.GetComponent<QuickbarInventory>();
			if ((bool)component4)
			{
				foreach (InventoryItem item2 in component4)
				{
					if (item2 != null)
					{
						DropItem(component, item2);
					}
				}
			}
		}
		for (int num = component.ItemList.Count - 1; num >= 0; num--)
		{
			if (component.ItemList[num].BaseItem.NeverDropAsLoot)
			{
				component.ItemList.RemoveAt(num);
			}
		}
		foreach (InventoryItem item3 in component)
		{
			item3.Original = true;
		}
	}

	private void DropItem(BaseInventory targetInventory, InventoryItem item)
	{
		DropItem(targetInventory, item.baseItem, item.stackSize);
	}

	private void DropItem(BaseInventory targetInventory, Item baseItem, int quantity)
	{
		if ((bool)baseItem)
		{
			targetInventory.AddItem(baseItem, quantity);
			ScriptEvent component = baseItem.GetComponent<ScriptEvent>();
			if ((bool)component)
			{
				component.ExecuteScript(ScriptEvent.ScriptEvents.OnItemDroppedAsLoot);
			}
		}
	}

	private void CreateLootOnBody(GameObject parent, object[] lootList, bool dropInventory, bool dropEquipment, BaseInventory parentInventory)
	{
		GameObject gameObject = null;
		if (gameObject == null)
		{
			Container container = GameResources.LoadPrefab<Container>("DefaultDropItem", instantiate: false);
			if (container == null)
			{
				Debug.LogError("DefaultDropItem prefab not found!");
				return;
			}
			Container container2 = ComponentUtils.CopyComponent(container, base.gameObject);
			NPCDialogue component = base.gameObject.GetComponent<NPCDialogue>();
			if ((bool)component)
			{
				GameUtilities.Destroy(component);
			}
			container2.ManualLabelName = CharacterStats.Name(parent);
			container2.AreaLootable = true;
			gameObject = base.gameObject;
			gameObject.layer = LayerUtility.FindLayerValue("Dynamics");
		}
		CopyLootObjectsToInventory(parent, gameObject, lootList, dropInventory, dropEquipment, parentInventory);
		m_creatingMeshColliderCorouting = CreateMeshForCollider();
		StartCoroutine(m_creatingMeshColliderCorouting);
		m_lootDroppedOnBody = true;
	}

	public void PopulateInventory()
	{
		if (m_populate)
		{
			return;
		}
		m_populate = true;
		object[] array = null;
		if (LootList != null)
		{
			SetSeed();
			array = LootList.Evaluate();
			ResetSeed();
		}
		if (array == null || array.Length == 0)
		{
			return;
		}
		Inventory component = GetComponent<Inventory>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] is GameObject)
			{
				Item component2 = (array[i] as GameObject).GetComponent<Item>();
				if ((bool)component2)
				{
					component.AddItem(component2, 1, -1, original: true);
				}
			}
		}
	}

	private BaseInventory GetInventoryComponent()
	{
		BaseInventory[] components = base.gameObject.GetComponents<BaseInventory>();
		BaseInventory result = null;
		if (components.Length == 1)
		{
			if (!(components[0] is Store))
			{
				result = components[0];
			}
		}
		else if (components.Length > 1)
		{
			for (int i = 0; i < components.Length; i++)
			{
				if (!(components[i] is Store))
				{
					result = components[i];
					break;
				}
			}
		}
		return result;
	}

	private void SetSeed()
	{
		UnityEngine.Random.InitState((int)(base.transform.position.x + base.transform.position.z) * GameState.s_playerCharacter.name.GetHashCode() + WorldTime.Instance.CurrentDay);
	}

	private void ResetSeed()
	{
		UnityEngine.Random.InitState(Environment.TickCount);
	}
}
