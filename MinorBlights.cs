using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MinorBlights : GenericSpell
{
	public List<Equippable> WeaponPrefab = new List<Equippable>();

	private int m_weapon_index;

	private float m_weapon_changed_time;

	private bool m_statusEffectCleared;

	protected override void Start()
	{
		StringTableManager.OnLanguageChanged += OnLanguageChanged;
		OnLanguageChanged(StringTableManager.CurrentLanguage);
		base.Start();
	}

	protected override void OnDestroy()
	{
		StringTableManager.OnLanguageChanged -= OnLanguageChanged;
		base.OnDestroy();
	}

	private void OnLanguageChanged(Language newlang)
	{
		StatusEffectParams[] statusEffects = StatusEffects;
		foreach (StatusEffectParams statusEffectParams in statusEffects)
		{
			if (statusEffectParams.AffectsStat == StatusEffect.ModifiedStat.CallbackAfterAttack)
			{
				statusEffectParams.Description = GUIUtils.Format(1159, string.Join(GUIUtils.Comma(), (from eq in WeaponPrefab
					where eq
					select eq into equippable
					select equippable.DisplayName.GetText()).ToArray()));
			}
		}
	}

	protected override void Update()
	{
		if (m_weapon_changed_time > 0f)
		{
			m_weapon_changed_time -= Time.deltaTime;
		}
		base.Update();
	}

	protected override void Apply(GameObject target)
	{
		m_statusEffectCleared = false;
		base.Apply(target);
		m_weapon_index = -1;
		SummonNewWeapon();
	}

	public override void HandleStatsOnPostDamageDealtCallback(GameObject source, CombatEventArgs args)
	{
		StartCoroutine(SummonNewWeaponDelay());
	}

	public override void HandleStatsOnPostDamageDealtCallbackComplete()
	{
		if (m_weapon_index >= 0)
		{
			Equipment component = Owner.GetComponent<Equipment>();
			if (component == null)
			{
				return;
			}
			component.PopSummonedWeapon(Equippable.EquipmentSlot.PrimaryWeapon);
		}
		m_weapon_index = -1;
		m_statusEffectCleared = true;
	}

	public IEnumerator SummonNewWeaponDelay()
	{
		yield return new WaitForSeconds(0.5f);
		SummonNewWeapon();
	}

	public void SummonNewWeapon()
	{
		if (m_statusEffectCleared || m_weapon_changed_time > 0f || WeaponPrefab.Count == 0)
		{
			return;
		}
		int num = OEIRandom.Index(WeaponPrefab.Count);
		if (num != m_weapon_index)
		{
			Equipment component = Owner.GetComponent<Equipment>();
			if (!(component == null))
			{
				component.PopSummonedWeapon(Equippable.EquipmentSlot.PrimaryWeapon);
				component.PushSummonedWeapon(WeaponPrefab[num], Equippable.EquipmentSlot.PrimaryWeapon, base.ActiveStatusEffects[0]);
				m_weapon_index = num;
				m_weapon_changed_time = 1f;
			}
		}
	}
}
