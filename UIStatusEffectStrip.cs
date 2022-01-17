using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIStatusEffectStrip : MonoBehaviour
{
	public static int IconCollapseThreshold = 6;

	public List<UIStatusEffectIcon> StatusEffectBundles = new List<UIStatusEffectIcon>();

	public GameObject IconPrefab;

	public Texture DefaultIcon;

	public UIGrid Grid;

	public UISprite More;

	private BoxCollider m_Collider;

	private bool m_TooltipActive;

	private List<StatusEffect> m_Effects = new List<StatusEffect>();

	private List<StatusEffect> m_EffectsFromAuras = new List<StatusEffect>();

	private IEnumerable<UIStatusEffectIcon> m_VisibleEffectBundles => StatusEffectBundles.Where((UIStatusEffectIcon b) => !b.Empty);

	private void Start()
	{
		if (DefaultIcon == null)
		{
			DefaultIcon = IconPrefab.GetComponent<UITexture>().mainTexture;
		}
		UIEventListener uIEventListener = UIEventListener.Get(More);
		uIEventListener.onTooltip = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onTooltip, new UIEventListener.BoolDelegate(OnChildTooltip));
		NotifyContentChanged();
	}

	private void Update()
	{
		if (!TimeController.Instance.Paused && m_TooltipActive)
		{
			OnTooltip(show: true);
		}
		UpdateEmptyBundles();
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnChildTooltip(GameObject sender, bool show)
	{
		OnTooltip(show);
	}

	private void OnTooltip(bool show)
	{
		m_TooltipActive = show;
		if (show && m_VisibleEffectBundles.Any())
		{
			UITexture texture = m_VisibleEffectBundles.First().Texture;
			ITooltipContent[] data = m_VisibleEffectBundles.ToArray();
			UIAbilityTooltip.GlobalShow(texture, 24, data);
		}
		else
		{
			UIAbilityTooltip.GlobalHide();
		}
	}

	public void RebundleEffects()
	{
		m_Effects.Clear();
		m_EffectsFromAuras.Clear();
		foreach (UIStatusEffectIcon statusEffectBundle in StatusEffectBundles)
		{
			m_Effects.AddRange(statusEffectBundle.StatusEffects);
			m_EffectsFromAuras.AddRange(statusEffectBundle.EffectsFromAuras);
		}
		Clear();
		foreach (StatusEffect effect in m_Effects)
		{
			AddStatusEffect(effect, isFromAura: false);
		}
		foreach (StatusEffect effectsFromAura in m_EffectsFromAuras)
		{
			AddStatusEffect(effectsFromAura, isFromAura: true);
		}
	}

	public void AddStatusEffect(StatusEffect effect, bool isFromAura)
	{
		if (effect.Params.AffectsStat == StatusEffect.ModifiedStat.NoEffect || ((bool)effect.AfflictionOrigin && effect.AfflictionOrigin.HideFromUI))
		{
			return;
		}
		foreach (UIStatusEffectIcon statusEffectBundle in StatusEffectBundles)
		{
			if (statusEffectBundle.CanBundle(effect))
			{
				statusEffectBundle.AddEffect(effect, isFromAura);
				return;
			}
		}
		GameObject obj = NGUITools.AddChild(base.gameObject, IconPrefab);
		obj.SetActive(value: true);
		UIStatusEffectIcon component = obj.GetComponent<UIStatusEffectIcon>();
		component.SetOwner(this);
		component.AddEffect(effect, isFromAura);
		component.transform.localScale = new Vector3(16f, 16f, 1f);
		StatusEffectBundles.Add(component);
		NotifyContentChanged();
	}

	public void RemoveStatusEffect(StatusEffect e)
	{
		for (int i = 0; i < StatusEffectBundles.Count; i++)
		{
			StatusEffectBundles[i].RemoveEffect(e);
		}
		NotifyContentChanged();
	}

	public void UpdateEmptyBundles()
	{
		bool flag = false;
		for (int num = StatusEffectBundles.Count - 1; num >= 0; num--)
		{
			if (StatusEffectBundles[num].Empty && StatusEffectBundles[num].gameObject.activeSelf)
			{
				StatusEffectBundles[num].gameObject.SetActive(value: false);
				flag = true;
			}
			else if (!StatusEffectBundles[num].Empty && !StatusEffectBundles[num].gameObject.activeSelf)
			{
				StatusEffectBundles[num].gameObject.SetActive(value: true);
				flag = true;
			}
		}
		if (flag)
		{
			NotifyContentChanged();
		}
	}

	public void Clear()
	{
		foreach (UIStatusEffectIcon statusEffectBundle in StatusEffectBundles)
		{
			statusEffectBundle.gameObject.SetActive(value: false);
			GameUtilities.Destroy(statusEffectBundle.gameObject);
		}
		StatusEffectBundles.Clear();
		NotifyContentChanged();
	}

	private void NotifyContentChanged()
	{
		int num;
		if (m_VisibleEffectBundles.Count() > IconCollapseThreshold)
		{
			if ((bool)More)
			{
				More.gameObject.SetActive(value: true);
			}
			num = IconCollapseThreshold - 1;
		}
		else
		{
			if ((bool)More)
			{
				More.gameObject.SetActive(value: false);
			}
			num = IconCollapseThreshold;
		}
		int num2 = 0;
		for (int i = 0; i < StatusEffectBundles.Count; i++)
		{
			if ((bool)StatusEffectBundles[i] && StatusEffectBundles[i].gameObject != IconPrefab && !StatusEffectBundles[i].Empty)
			{
				StatusEffectBundles[i].gameObject.SetActive(num2 < num);
				num2++;
			}
		}
		Grid.Reposition();
		if (!m_Collider)
		{
			m_Collider = GetComponent<BoxCollider>();
		}
		m_Collider.size = NGUIMath.CalculateRelativeWidgetBounds(base.transform).size;
		m_Collider.center = NGUIMath.CalculateRelativeWidgetBounds(base.transform).center;
	}
}
