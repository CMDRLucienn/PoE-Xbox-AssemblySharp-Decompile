using System;
using UnityEngine;

[Serializable]
public class StrongholdHireling
{
	public int CostPerDay;

	public bool LeavesAfterFullPayCycle;

	public int PrestigeAdjustment;

	public int SecurityAdjustment;

	public CharacterStats HirelingPrefab;

	[GlobalVariableString]
	[Tooltip("This global is set to 1 when the hireling is hired and 0 when he is fired.")]
	public string HiredGlobalVariableName;

	[GlobalVariableString]
	[Tooltip("This global is set to 1 when the hireling can be hired and 0 when he can't.")]
	public string CanHireGlobalVariableName;

	private int m_SerializedNameId = -1;

	public bool Paid { get; set; }

	public bool IsLeaving { get; set; }

	[Obsolete]
	public string SerializedName { get; set; }

	public int NameId
	{
		get
		{
			if ((bool)HirelingPrefab)
			{
				return HirelingPrefab.DisplayName.StringID;
			}
			return -1;
		}
	}

	public int SerializedNameId
	{
		get
		{
			if (m_SerializedNameId < 0)
			{
				m_SerializedNameId = NameId;
			}
			return m_SerializedNameId;
		}
		set
		{
			m_SerializedNameId = value;
		}
	}

	public string Name
	{
		get
		{
			if ((bool)HirelingPrefab)
			{
				return HirelingPrefab.Name();
			}
			return "*HirelingNameError*";
		}
	}

	public void Restore(StrongholdHireling data)
	{
		Paid = data.Paid;
		IsLeaving = data.IsLeaving;
	}

	public void ResetState()
	{
		Paid = false;
		IsLeaving = false;
	}
}
