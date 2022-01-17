using UnityEngine;

public class UIPartyMemberDTGetter : UIPopulator
{
	private CharacterStats m_NeedsReload;

	private UITable m_Table;

	private ISelectACharacter m_Owner;

	protected override void Awake()
	{
		base.Awake();
		m_Table = GetComponent<UITable>();
	}

	private void Start()
	{
		m_Owner = UIWindowManager.FindParentISelectACharacter(base.transform);
		m_Owner.OnSelectedCharacterChanged += MarkReload;
		m_NeedsReload = m_Owner.SelectedCharacter;
	}

	protected override void OnDestroy()
	{
		if (m_Owner != null)
		{
			m_Owner.OnSelectedCharacterChanged -= MarkReload;
		}
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void LateUpdate()
	{
		if ((bool)m_NeedsReload)
		{
			ReloadCharacter(m_NeedsReload);
			m_NeedsReload = null;
		}
	}

	private void MarkReload(CharacterStats pai)
	{
		m_NeedsReload = pai;
	}

	private void ReloadCharacter(CharacterStats stats)
	{
		Populate(0);
		int num = 0;
		BestiaryCertainty known;
		float perceivedDamThresh = stats.GetPerceivedDamThresh(DamagePacket.DamageType.All, isVeilPiercing: false, out known);
		if (known > BestiaryCertainty.Unknown)
		{
			for (DamagePacket.DamageType damageType = DamagePacket.DamageType.Slash; damageType < DamagePacket.DamageType.Count; damageType++)
			{
				if (stats.GetPerceivedDamThresh(damageType, isVeilPiercing: false, out known) != perceivedDamThresh && known > BestiaryCertainty.Unknown)
				{
					GameObject obj = ActivateClone(num++);
					UIPartyMemberStatGetter componentInChildren = obj.GetComponentInChildren<UIPartyMemberStatGetter>();
					if ((bool)componentInChildren)
					{
						componentInChildren.Damage = damageType;
					}
					UIPartyMemberStatIconGetter componentInChildren2 = obj.GetComponentInChildren<UIPartyMemberStatIconGetter>();
					if ((bool)componentInChildren2)
					{
						componentInChildren2.Damage = damageType;
					}
					UIStatBreakdownTrigger componentInChildren3 = obj.GetComponentInChildren<UIStatBreakdownTrigger>();
					if ((bool)componentInChildren3)
					{
						componentInChildren3.DamageType = damageType;
					}
				}
			}
		}
		if ((bool)m_Table)
		{
			m_Table.Reposition();
		}
	}
}
