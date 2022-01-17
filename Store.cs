using UnityEngine;

[RequireComponent(typeof(Vendor))]
public class Store : BaseInventory
{
	public GlobalVariableString GlobalVariableName = new GlobalVariableString();

	public RegenerationEntry[] RegenerationItemTable;

	public int RegenerationHours = 12;

	public CurrencyValue currencyStoreBank = new CurrencyValue();

	public Currency baseCurrency;

	[Persistent]
	public float sellMultiplier = 1.5f;

	[Persistent]
	public float buyMultiplier = 0.2f;

	public const float DefaultSellMultiplier = 1.5f;

	public const float DefaultBuyMultiplier = 0.2f;

	[Persistent]
	private int m_lastGlobalValue;

	[Persistent]
	private EternityDateTime m_timeStamp;

	[Persistent]
	private bool m_firstTime = true;

	public override bool InfiniteStacking => true;

	private void Awake()
	{
		if (!GetComponent<Vendor>())
		{
			base.gameObject.AddComponent<Vendor>();
			Debug.LogError("Store '" + base.gameObject.name + "' has no Vendor component (these are now required for Stores).");
		}
		MaxItems = int.MaxValue;
	}

	public override void Restored()
	{
		if (sellMultiplier >= 10f)
		{
			sellMultiplier /= 10f;
		}
		if (buyMultiplier >= 1f)
		{
			buyMultiplier /= 10f;
		}
		base.Restored();
	}

	public void NotifyOpened()
	{
		RegenerateItems();
	}

	private int FindRegenerationEntryIndex(int value)
	{
		int result = -1;
		if (RegenerationItemTable != null)
		{
			for (int i = 0; i < RegenerationItemTable.Length; i++)
			{
				if (RegenerationItemTable[i].GlobalVariableValue == value)
				{
					result = i;
					break;
				}
			}
		}
		return result;
	}

	private void RegenerateItems()
	{
		int num = 0;
		bool flag = false;
		if (GlobalVariableName != null && GlobalVariableName.Name.Length > 0)
		{
			num = GlobalVariables.Instance.GetVariable(GlobalVariableName.Name);
			if (num < 0)
			{
				Debug.LogError(string.Concat("Invalid global variable name", GlobalVariableName, " on Store script on object ", base.gameObject.name, ". Using entry zero."));
				num = 0;
			}
		}
		if (m_firstTime)
		{
			flag = true;
		}
		else if ((WorldTime.Instance.CurrentTime - m_timeStamp).TotalHours() >= RegenerationHours)
		{
			flag = true;
		}
		if (!(m_lastGlobalValue != num || flag))
		{
			return;
		}
		int num2 = FindRegenerationEntryIndex(m_lastGlobalValue);
		if (num2 >= 0)
		{
			foreach (RegeneratingItem regeneratingItem in RegenerationItemTable[num2].RegeneratingItems)
			{
				int num3 = ItemCount(regeneratingItem.baseItem);
				if (num3 > 0)
				{
					DestroyItem(regeneratingItem.baseItem, num3);
				}
			}
		}
		num2 = FindRegenerationEntryIndex(num);
		if (num2 >= 0)
		{
			foreach (RegeneratingItem regeneratingItem2 in RegenerationItemTable[num2].RegeneratingItems)
			{
				if (OEIRandom.FloatValue() < regeneratingItem2.Chance)
				{
					int addCount = OEIRandom.Range(regeneratingItem2.stackMin, regeneratingItem2.stackMax);
					AddItem(regeneratingItem2.baseItem, addCount);
				}
			}
		}
		m_firstTime = false;
		m_lastGlobalValue = num;
		if (flag)
		{
			m_timeStamp = new EternityDateTime(WorldTime.Instance.CurrentTime);
		}
	}
}
