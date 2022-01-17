using System.Collections.Generic;
using UnityEngine;

public class UIStealthIndicatorManager : UIPopulator
{
	private List<Stealth> m_stealthIndicators = new List<Stealth>();

	public static UIStealthIndicatorManager Instance { get; private set; }

	protected override bool ResetTransform => false;

	protected override void Awake()
	{
		Instance = this;
		base.Awake();
		Stealth[] array = Object.FindObjectsOfType<Stealth>();
		foreach (Stealth go in array)
		{
			AddIndicator(go);
		}
	}

	protected override void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		ResetAll();
		ComponentUtils.NullOutObjectReferences(this);
		base.OnDestroy();
	}

	public void AddIndicator(Stealth go)
	{
		if (!m_stealthIndicators.Contains(go))
		{
			m_stealthIndicators.Add(go);
			ActivateClone(m_stealthIndicators.Count - 1);
		}
	}

	public void RemoveIndicator(Stealth go)
	{
		if (m_stealthIndicators.Contains(go))
		{
			RemoveClone(m_stealthIndicators.IndexOf(go));
			m_stealthIndicators.Remove(go);
		}
	}

	protected override GameObject ActivateClone(int index)
	{
		GameObject gameObject = base.ActivateClone(index);
		if (gameObject != null)
		{
			gameObject.GetComponentInChildren<UIStealthIndicator>().Target = m_stealthIndicators[index];
		}
		return gameObject;
	}

	public void ResetAll()
	{
		if (m_stealthIndicators != null)
		{
			m_stealthIndicators.Clear();
		}
	}
}
