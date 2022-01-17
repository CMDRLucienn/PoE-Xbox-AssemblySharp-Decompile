using System;
using System.Collections.Generic;
using UnityEngine;

public class UILootTooltipManager : MonoBehaviour
{
	private static UILootTooltipManager s_Instance;

	public GameObject PrefabLootTooltip;

	private List<UILootTooltip> m_TipPool = new List<UILootTooltip>();

	private const float DEFAULT_NOTIFICATION_DURATION = 2.5f;

	public static UILootTooltipManager Instance => s_Instance;

	private void Awake()
	{
		if (s_Instance != null && s_Instance != this)
		{
			GameUtilities.Destroy(this);
		}
		else
		{
			s_Instance = this;
		}
		UILootTooltip[] componentsInChildren = GetComponentsInChildren<UILootTooltip>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].TooltipDone += ReturnToolltipToPool;
			ReturnToolltipToPool(componentsInChildren[i]);
		}
		GameState.OnLevelUnload += OnLevelUnloaded;
	}

	private void OnDestroy()
	{
		if (m_TipPool != null)
		{
			for (int i = 0; i < m_TipPool.Count; i++)
			{
				m_TipPool[i].TooltipDone -= ReturnToolltipToPool;
			}
			m_TipPool.Clear();
		}
		GameState.OnLevelUnload -= OnLevelUnloaded;
	}

	public void ShowTip(Item item, GameObject spawnPos, int itemQty, int offset)
	{
		if (!(item == null) && !(spawnPos == null) && itemQty >= 0)
		{
			UILootTooltip tooltipFromPool = GetTooltipFromPool();
			tooltipFromPool.Initialize(item, itemQty, spawnPos, 2.5f, offset);
			tooltipFromPool.Show();
		}
	}

	public void ResetAll()
	{
		UILootTooltip[] componentsInChildren = GetComponentsInChildren<UILootTooltip>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			ReturnToolltipToPool(componentsInChildren[i]);
		}
	}

	private void OnLevelUnloaded(object sender, EventArgs e)
	{
		ResetAll();
	}

	private UILootTooltip GetTooltipFromPool()
	{
		UILootTooltip uILootTooltip = null;
		if (m_TipPool == null || m_TipPool.Count <= 0)
		{
			GameObject gameObject = NGUITools.AddChild(base.gameObject, PrefabLootTooltip);
			if (gameObject != null)
			{
				uILootTooltip = gameObject.GetComponent<UILootTooltip>();
				uILootTooltip.TooltipDone += ReturnToolltipToPool;
			}
		}
		else
		{
			uILootTooltip = m_TipPool[0];
			m_TipPool.RemoveAt(0);
		}
		return uILootTooltip;
	}

	private void ReturnToolltipToPool(UILootTooltip tooltip)
	{
		tooltip.Reset();
		m_TipPool.Add(tooltip);
	}
}
