public class UIStatBreakdownTrigger : UIParentSelectorListener
{
	public StatusEffect.ModifiedStat ModifiedStat = StatusEffect.ModifiedStat.NoEffect;

	public DamagePacket.DamageType DamageType = DamagePacket.DamageType.None;

	public bool Offhand;

	private UILabel m_Label;

	private void OnDisable()
	{
		if ((bool)UITextTooltip.Instance)
		{
			UITextTooltip.Instance.Hide();
		}
	}

	private void Awake()
	{
		m_Label = GetComponentInChildren<UILabel>();
	}

	private void OnClick()
	{
		if (ParentSelector != null)
		{
			UIItemInspectManager.Examine(ModifiedStat, ParentSelector.SelectedCharacter, DamageType, Offhand);
		}
	}

	private void OnTooltip(bool over)
	{
		if (!over)
		{
			UITextTooltip.Instance.Hide();
			return;
		}
		CharacterStats.SkillType skillType = StatusEffect.ModifiedStatToSkillType(ModifiedStat);
		CharacterStats.AttributeScoreType attributeScoreType = StatusEffect.ModifiedStatToAttributeScoreType(ModifiedStat);
		CharacterStats.DefenseType defenseType = StatusEffect.ModifiedStatToDefenseType(ModifiedStat);
		CharacterStats selectedCharacter = ParentSelector.SelectedCharacter;
		if (skillType != CharacterStats.SkillType.Count)
		{
			UITextTooltip.Instance.Show(m_Label, GUIUtils.GetSkillTypeString(skillType), UICharacterSheetContentManager.GetSkillEffectsInverted(selectedCharacter, skillType, "\n", UIGlobalColor.LinkStyle.WOOD));
		}
		else if (attributeScoreType != CharacterStats.AttributeScoreType.Count)
		{
			UITextTooltip.Instance.Show(m_Label, GUIUtils.GetAttributeScoreTypeString(attributeScoreType), UICharacterSheetContentManager.GetAttributeEffectsInverted(selectedCharacter, attributeScoreType, "\n", UIGlobalColor.LinkStyle.WOOD));
		}
		else if (defenseType != CharacterStats.DefenseType.None)
		{
			UITextTooltip.Instance.Show(m_Label, GUIUtils.GetDefenseTypeString(defenseType), UICharacterSheetContentManager.GetDefenseEffectsInverted(selectedCharacter, defenseType, "\n", UIGlobalColor.LinkStyle.WOOD));
		}
		else if (ModifiedStat == StatusEffect.ModifiedStat.InterruptBonus)
		{
			UITextTooltip.Instance.Show(m_Label, StringTableManager.GetText(DatabaseString.StringTableType.Cyclopedia, 173), UICharacterSheetContentManager.GetInterruptEffectsInverted(selectedCharacter, "\n", UIGlobalColor.LinkStyle.WOOD));
		}
		else if (ModifiedStat == StatusEffect.ModifiedStat.ConcentrationBonus)
		{
			UITextTooltip.Instance.Show(m_Label, StringTableManager.GetText(DatabaseString.StringTableType.Cyclopedia, 159), UICharacterSheetContentManager.GetConcentrationEffectsInverted(selectedCharacter, "\n", UIGlobalColor.LinkStyle.WOOD));
		}
		else if (ModifiedStat == StatusEffect.ModifiedStat.DamageThreshhold)
		{
			string text = StringTableManager.GetText(DatabaseString.StringTableType.Cyclopedia, 157);
			if (DamageType != DamagePacket.DamageType.All && DamageType != DamagePacket.DamageType.None)
			{
				text += GUIUtils.Format(1731, GUIUtils.GetDamageTypeString(DamageType));
			}
			UITextTooltip.Instance.Show(m_Label, text, UICharacterSheetContentManager.GetDamageThresholdEffectsInverted(selectedCharacter, DamageType, "\n", UIGlobalColor.LinkStyle.WOOD));
		}
		else if (ModifiedStat == StatusEffect.ModifiedStat.Damage)
		{
			Equipment component = selectedCharacter.GetComponent<Equipment>();
			AttackBase attack = ((!component) ? null : (Offhand ? component.SecondaryAttack : component.PrimaryAttack));
			UITextTooltip.Instance.Show(m_Label, GUIUtils.GetText(428), UICharacterSheetContentManager.GetDamageEffectsInverted(selectedCharacter, attack, "\n", UIGlobalColor.LinkStyle.WOOD));
		}
		else if (ModifiedStat == StatusEffect.ModifiedStat.Accuracy)
		{
			Equipment component2 = selectedCharacter.GetComponent<Equipment>();
			AttackBase attack2 = ((!component2) ? null : (Offhand ? component2.SecondaryAttack : component2.PrimaryAttack));
			UITextTooltip.Instance.Show(m_Label, GUIUtils.GetText(369), UICharacterSheetContentManager.GetAccuracyEffectsInverted(selectedCharacter, attack2, "\n", UIGlobalColor.LinkStyle.WOOD));
		}
	}
}
