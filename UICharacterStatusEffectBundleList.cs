using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UICharacterStatusEffectBundleList : UIPopulator
{
	private ISelectACharacter m_Owner;

	private UIGrid m_Grid;

	private CharacterStats m_LastCharacter;

	private bool m_NeedsReload;

	public GameObject DeactivateWhenEmpty;

	protected override void Awake()
	{
		base.Awake();
		m_Grid = GetComponent<UIGrid>();
	}

	private void Start()
	{
		m_Owner = UIWindowManager.FindParentISelectACharacter(base.transform);
		m_Owner.OnSelectedCharacterChanged += ReloadCharacter;
		ReloadCharacter(m_Owner.SelectedCharacter);
	}

	protected override void OnDestroy()
	{
		if (m_Owner != null)
		{
			m_Owner.OnSelectedCharacterChanged -= ReloadCharacter;
		}
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		if (m_NeedsReload)
		{
			RefreshContent();
		}
	}

	private void OnAddStatusEffect(GameObject sender, StatusEffect effect, bool fromAura)
	{
		m_NeedsReload = true;
	}

	private void OnClearStatusEffect(GameObject sender, StatusEffect effect)
	{
		m_NeedsReload = true;
	}

	private void ReloadCharacter(CharacterStats stats)
	{
		if ((bool)m_LastCharacter)
		{
			m_LastCharacter.OnAddStatusEffect -= OnAddStatusEffect;
			m_LastCharacter.OnClearStatusEffect -= OnClearStatusEffect;
		}
		m_LastCharacter = stats;
		if (!m_LastCharacter)
		{
			Populate(0);
			return;
		}
		m_LastCharacter.OnAddStatusEffect += OnAddStatusEffect;
		m_LastCharacter.OnClearStatusEffect += OnClearStatusEffect;
		RefreshContent();
	}

	private void RefreshContent()
	{
		m_NeedsReload = false;
		Populate(0);
		int num = 0;
		foreach (List<StatusEffect> item in UICharacterSheetStatusEffects.BundleEffectsForUI(m_LastCharacter.ActiveStatusEffects.Where((StatusEffect ef) => ShouldDisplayEffect(ef))))
		{
			UIStatusEffectBundleLine component = ActivateClone(num++).GetComponent<UIStatusEffectBundleLine>();
			if ((bool)component)
			{
				component.Icon.material = null;
				component.Load(item);
			}
			else
			{
				Debug.LogError("UICharacterStatusEffectBundleList populator root item is not UIStatusEffectBundleLine.");
			}
		}
		if ((bool)DeactivateWhenEmpty)
		{
			DeactivateWhenEmpty.SetActive(num > 0);
		}
		if ((bool)m_Grid)
		{
			m_Grid.Reposition();
		}
	}

	private bool ShouldDisplayEffect(StatusEffect se)
	{
		if ((bool)se.AbilityOrigin && se.AbilityOrigin.Passive && se.Target == se.Owner)
		{
			return false;
		}
		if ((bool)se.EquipmentOrigin && (se.Target == null || se.Target == se.Owner))
		{
			return false;
		}
		return true;
	}
}
