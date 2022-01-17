using System;
using UnityEngine;

[RequireComponent(typeof(UILabel))]
public class UIStrongholdHirelingCapacityLabel : MonoBehaviour
{
	private UILabel m_Label;

	private void OnEnable()
	{
		Reload();
	}

	private void Start()
	{
		m_Label = GetComponent<UILabel>();
		Stronghold instance = Stronghold.Instance;
		instance.OnHirelingStatusChanged = (Stronghold.HirelingStatusChanged)Delegate.Combine(instance.OnHirelingStatusChanged, new Stronghold.HirelingStatusChanged(HirelingsChanged));
		Reload();
	}

	private void OnDestroy()
	{
		if ((bool)Stronghold.Instance)
		{
			Stronghold instance = Stronghold.Instance;
			instance.OnHirelingStatusChanged = (Stronghold.HirelingStatusChanged)Delegate.Remove(instance.OnHirelingStatusChanged, new Stronghold.HirelingStatusChanged(HirelingsChanged));
		}
	}

	private void HirelingsChanged(StrongholdHireling hireling)
	{
		Reload();
	}

	private void Reload()
	{
		if ((bool)m_Label)
		{
			if (Stronghold.Instance.HasUpgrade(StrongholdUpgrade.Type.Barracks))
			{
				m_Label.text = GUIUtils.Format(451, Stronghold.Instance.HirelingsHired, Stronghold.Instance.MaxHirelings);
			}
			else
			{
				m_Label.text = "";
			}
		}
	}
}
