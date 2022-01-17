using System;
using UnityEngine;

public class BackerUnlockTracker : MonoBehaviour
{
	[Persistent]
	private bool Tier1Unlocked;

	[Persistent]
	private bool Tier2Unlocked;

	[Persistent]
	private bool PreorderUnlocked;

	[Persistent]
	private bool Activated;

	public string Tier1EquippableUnlock;

	public bool AutoEquipTier1Equippable;

	public string Tier2EquippableUnlock;

	public bool AutoEquipTier2Equippable;

	public string Tier1PreOrderEquippableUnlock1;

	public string Tier1PreOrderEquippableUnlock2;

	public bool AutoEquipTier1PreOrderEquippable;

	public static BackerUnlockTracker Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'BackerUnlockTracker' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
	}

	private void Start()
	{
		GameState.OnLevelLoaded += OnLevelLoaded;
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		GameState.OnLevelLoaded -= OnLevelLoaded;
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnLevelLoaded(object sender, EventArgs e)
	{
		if (Activated)
		{
			HandleUnlocks();
		}
	}

	public void Activate()
	{
		if (!Activated)
		{
			HandleUnlocks();
		}
		Activated = true;
	}

	private void HandleTier1UnlockCheck()
	{
		if (Tier1Unlocked)
		{
			return;
		}
		bool flag = false;
		GameObject gameObject = GameResources.LoadPrefab<GameObject>("Tier1Backer", instantiate: false);
		if (gameObject != null)
		{
			InstanceID component = gameObject.GetComponent<InstanceID>();
			flag = component != null && component.UniqueID == "ad7305e9-7a5a-484b-87ee-ef7cd945f4f9";
		}
		if (flag && !string.IsNullOrEmpty(Tier1EquippableUnlock) && (bool)GameState.s_playerCharacter)
		{
			Player s_playerCharacter = GameState.s_playerCharacter;
			Inventory component2 = s_playerCharacter.GetComponent<Inventory>();
			Equippable equippable = GameResources.LoadPrefab<Equippable>(Tier1EquippableUnlock, instantiate: false);
			if (AutoEquipTier1Equippable)
			{
				Equippable newItem = s_playerCharacter.GetComponent<Equipment>().Equip(equippable);
				component2.AddItem(newItem, 1);
			}
			else
			{
				component2.AddItem(equippable, 1);
			}
			if ((bool)AchievementTracker.Instance)
			{
				AchievementTracker.Instance.IncrementTrackedStat(AchievementTracker.TrackedAchievementStat.BackedGame);
			}
			Tier1Unlocked = true;
		}
	}

	private void HandleTier2UnlockCheck()
	{
		if (Tier2Unlocked)
		{
			return;
		}
		bool flag = false;
		GameObject gameObject = GameResources.LoadPrefab<GameObject>("Tier2Backer", instantiate: false);
		if (gameObject != null)
		{
			InstanceID component = gameObject.GetComponent<InstanceID>();
			flag = component != null && component.UniqueID == "27a62e66-8e1b-4061-b710-135d5002298f";
		}
		if (flag && !string.IsNullOrEmpty(Tier2EquippableUnlock) && (bool)GameState.s_playerCharacter)
		{
			Player s_playerCharacter = GameState.s_playerCharacter;
			Inventory component2 = s_playerCharacter.GetComponent<Inventory>();
			Equippable equippable = GameResources.LoadPrefab<Equippable>(Tier2EquippableUnlock, instantiate: false);
			if (AutoEquipTier2Equippable)
			{
				Equippable newItem = s_playerCharacter.GetComponent<Equipment>().Equip(equippable);
				component2.AddItem(newItem, 1);
			}
			else
			{
				component2.AddItem(equippable, 1);
			}
			Tier2Unlocked = true;
		}
	}

	private void HandleTier1PreOrderUnlockCheck()
	{
		if (PreorderUnlocked)
		{
			return;
		}
		bool flag = false;
		GameObject gameObject = GameResources.LoadPrefab<GameObject>("Tier1Preorder", instantiate: false);
		if (gameObject != null)
		{
			InstanceID component = gameObject.GetComponent<InstanceID>();
			flag = component != null && component.UniqueID == "b3056e0f-b3de-43e2-be73-83566c687077";
		}
		if (!flag)
		{
			return;
		}
		if (!string.IsNullOrEmpty(Tier1PreOrderEquippableUnlock1) && (bool)GameState.s_playerCharacter)
		{
			Player s_playerCharacter = GameState.s_playerCharacter;
			Inventory component2 = s_playerCharacter.GetComponent<Inventory>();
			Equippable equippable = GameResources.LoadPrefab<Equippable>(Tier1PreOrderEquippableUnlock1, instantiate: false);
			if (AutoEquipTier1PreOrderEquippable)
			{
				Equippable newItem = s_playerCharacter.GetComponent<Equipment>().Equip(equippable);
				component2.AddItem(newItem, 1);
			}
			else
			{
				component2.AddItem(equippable, 1);
			}
		}
		if (!string.IsNullOrEmpty(Tier1PreOrderEquippableUnlock2) && (bool)GameState.s_playerCharacter)
		{
			Player s_playerCharacter2 = GameState.s_playerCharacter;
			Inventory component3 = s_playerCharacter2.GetComponent<Inventory>();
			Equippable equippable2 = GameResources.LoadPrefab<Equippable>(Tier1PreOrderEquippableUnlock2, instantiate: false);
			if (AutoEquipTier1PreOrderEquippable)
			{
				Equippable newItem2 = s_playerCharacter2.GetComponent<Equipment>().Equip(equippable2);
				component3.AddItem(newItem2, 1);
			}
			else
			{
				component3.AddItem(equippable2, 1);
			}
		}
		PreorderUnlocked = true;
	}

	private void HandleUnlocks()
	{
		HandleTier1UnlockCheck();
		HandleTier2UnlockCheck();
		HandleTier1PreOrderUnlockCheck();
	}
}
