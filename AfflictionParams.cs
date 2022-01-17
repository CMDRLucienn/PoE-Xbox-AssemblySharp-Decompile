using System;
using System.Text;

[Serializable]
public class AfflictionParams
{
	public Affliction AfflictionPrefab;

	public float Duration;

	public string Keyword;

	public string GetString(CharacterStats owner, StatusEffectFormatMode mode)
	{
		return GetString(owner, null, showTime: true, mode);
	}

	public string GetString(CharacterStats owner, GenericAbility ability, StatusEffectFormatMode mode)
	{
		return GetString(owner, ability, showTime: true, mode);
	}

	public string GetString(CharacterStats owner, GenericAbility ability, bool showTime, StatusEffectFormatMode mode)
	{
		StringBuilder stringBuilder = new StringBuilder(AfflictionPrefab.DisplayName.GetText());
		if (!string.IsNullOrEmpty(Keyword))
		{
			stringBuilder.AppendGuiFormat(1731, KeywordData.GetAdjective(Keyword));
		}
		float num = Duration;
		if ((bool)ability && ability.DurationOverride > 0f)
		{
			num = ability.DurationOverride;
		}
		float num2 = num;
		if (owner != null && (!ability || !ability.IgnoreCharacterStats))
		{
			num2 *= owner.StatEffectDurationMultiplier;
		}
		string text = StatusEffectParams.FormatDuration(num, num2, mode);
		if (showTime && num > 0f && !string.IsNullOrEmpty(text))
		{
			return GUIUtils.Format(1665, stringBuilder.ToString(), text);
		}
		return stringBuilder.ToString();
	}

	public void AddStringEffects(string target, CharacterStats stats, GenericAbility ability, AttackBase attack, StatusEffectFormatMode mode, StringEffects stringEffects)
	{
		if (AfflictionPrefab.ThinUI)
		{
			AttackBase.AddStringEffect(target, new AttackBase.AttackEffect(StatusEffectParams.ListToString(AfflictionPrefab.StatusEffects, stats, ability, this), attack, hostile: true, secondary: true), stringEffects);
		}
		else
		{
			AttackBase.AddStringEffect(target, new AttackBase.AttackEffect(GetString(stats, ability, mode), attack, hostile: true, secondary: true), stringEffects);
		}
	}
}
