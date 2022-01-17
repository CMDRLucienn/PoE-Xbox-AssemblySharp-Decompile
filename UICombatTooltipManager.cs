using System;
using System.Collections.Generic;
using UnityEngine;

public class UICombatTooltipManager : MonoBehaviour
{
	private static UICombatTooltipManager s_Instance;

	public bool ShowAbilityTooltips;

	public GameObject TooltipPrefab;

	private Dictionary<GameObject, UICombatTooltip> m_ActiveTips = new Dictionary<GameObject, UICombatTooltip>();

	private List<UICombatTooltip> m_TipPool = new List<UICombatTooltip>();

	private List<GameObject> m_DeleteTips = new List<GameObject>();

	private bool m_LevelUnloading = true;

	public static UICombatTooltipManager Instance => s_Instance;

	private void Awake()
	{
		s_Instance = this;
		GameState.OnLevelUnload += OnLevelUnload;
		GameState.OnLevelLoaded += OnLevelLoaded;
	}

	private void Start()
	{
		if (TooltipPrefab.GetComponent<UICombatTooltip>() == null)
		{
			Debug.LogError("UICombatTooltipManager prefab isn't a UICombatTooltip.");
		}
		NewToPool();
		TooltipPrefab.SetActive(value: false);
	}

	private void OnDestroy()
	{
		if (s_Instance == this)
		{
			s_Instance = null;
		}
		GameState.OnLevelUnload -= OnLevelUnload;
		GameState.OnLevelLoaded -= OnLevelLoaded;
		m_TipPool.Clear();
		m_DeleteTips.Clear();
		m_ActiveTips.Clear();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		if (GameState.InCombat && GameState.Option.GetOption(GameOption.BoolOption.COMBAT_TOOLTIPS_UI) && InGameHUD.Instance.ShowHUD)
		{
			UICombatTooltip.MasterFadeAlpha += 2f * TimeController.sUnscaledDelta;
		}
		else
		{
			UICombatTooltip.MasterFadeAlpha -= 2f * TimeController.sUnscaledDelta;
		}
		UICombatTooltip.MasterFadeAlpha = Mathf.Clamp(UICombatTooltip.MasterFadeAlpha, 0f, 1f);
		for (int i = 0; i < Faction.ActiveFactionComponents.Count; i++)
		{
			Faction faction = Faction.ActiveFactionComponents[i];
			if (faction == null || !faction.ShowTooltips || !faction.isFowVisible || !GameState.InCombat || faction.RelationshipToPlayer == Faction.Relationship.Neutral)
			{
				continue;
			}
			Health component = faction.gameObject.GetComponent<Health>();
			if (!(component == null) && !component.ShowDead && !component.Unconscious)
			{
				UICombatTooltip uICombatTooltip = Show(faction.gameObject);
				if (uICombatTooltip != null)
				{
					uICombatTooltip.activeThisFrame = true;
					uICombatTooltip.IsSelected = true;
				}
			}
		}
		m_DeleteTips.Clear();
		Dictionary<GameObject, UICombatTooltip>.Enumerator enumerator = m_ActiveTips.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				if (!enumerator.Current.Value.activeThisFrame)
				{
					m_DeleteTips.Add(enumerator.Current.Key);
				}
			}
		}
		finally
		{
			enumerator.Dispose();
		}
		for (int j = 0; j < m_DeleteTips.Count; j++)
		{
			Hide(m_DeleteTips[j]);
		}
	}

	private void OnLevelUnload(object sender, EventArgs e)
	{
		m_LevelUnloading = true;
		ResetAll();
	}

	private void OnLevelLoaded(object sender, EventArgs e)
	{
		m_LevelUnloading = false;
	}

	public void ResetAll()
	{
		List<GameObject> list = new List<GameObject>();
		list.AddRange(m_ActiveTips.Keys);
		foreach (GameObject item in list)
		{
			Hide(item);
		}
	}

	private void NewToPool()
	{
		GameObject obj = UnityEngine.Object.Instantiate(TooltipPrefab);
		obj.transform.parent = TooltipPrefab.transform.parent;
		obj.transform.localScale = new Vector3(1f, 1f, 1f);
		obj.SetActive(value: false);
		UICombatTooltip component = obj.GetComponent<UICombatTooltip>();
		m_TipPool.Add(component);
	}

	public UICombatTooltip Show(GameObject target)
	{
		if (target == null || m_LevelUnloading)
		{
			return null;
		}
		if (!m_ActiveTips.ContainsKey(target))
		{
			if (m_TipPool.Count <= 0)
			{
				NewToPool();
			}
			UICombatTooltip uICombatTooltip = m_TipPool[m_TipPool.Count - 1];
			m_TipPool.RemoveAt(m_TipPool.Count - 1);
			uICombatTooltip.gameObject.SetActive(value: true);
			uICombatTooltip.Set(target);
			m_ActiveTips.Add(target, uICombatTooltip);
			uICombatTooltip.Panel.alpha = 0f;
			uICombatTooltip.FadeIn();
			return uICombatTooltip;
		}
		return m_ActiveTips[target];
	}

	public void Hide(GameObject target)
	{
		if (m_ActiveTips.ContainsKey(target))
		{
			m_ActiveTips[target].FadeOut();
		}
	}

	public void Remove(GameObject target)
	{
		if (m_ActiveTips.ContainsKey(target))
		{
			UICombatTooltip uICombatTooltip = m_ActiveTips[target];
			uICombatTooltip.Reset();
			m_ActiveTips.Remove(target);
			m_TipPool.Add(uICombatTooltip);
			uICombatTooltip.gameObject.SetActive(value: false);
		}
	}
}
