using UnityEngine;

public class DefensiveBond : GenericAbility
{
	public float DefenseBonus = 15f;

	protected override void Init()
	{
		if (m_initialized)
		{
			return;
		}
		base.Init();
		m_permanent = true;
		if (m_ownerStats != null)
		{
			m_ownerStats.DefensiveBondBonus += (int)DefenseBonus;
		}
		GameObject gameObject = GameUtilities.FindAnimalCompanion(Owner);
		if (gameObject != null)
		{
			CharacterStats component = gameObject.GetComponent<CharacterStats>();
			if (component != null)
			{
				component.DefensiveBondBonus += (int)DefenseBonus;
			}
		}
	}

	public override string GetAdditionalEffects(StringEffects stringEffects, StatusEffectFormatMode mode, GenericAbility ability, GameObject character)
	{
		AttackBase.AttackEffect effect = new AttackBase.AttackEffect(GUIUtils.Format(1226, TextUtils.NumberBonus(DefenseBonus), GUIUtils.GetText(1606)), null);
		AttackBase.AddStringEffect(GetSelfTarget().GetText(), effect, stringEffects);
		AttackBase.AddStringEffect(GUIUtils.GetText(2104), effect, stringEffects);
		return base.GetAdditionalEffects(stringEffects, mode, ability, character);
	}
}
