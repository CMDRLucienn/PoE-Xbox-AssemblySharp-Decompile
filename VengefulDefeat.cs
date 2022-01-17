using UnityEngine;

[ClassTooltip("When the owner is knocked unconscious, automatically attacks each enemy within the Aoe Radius with the attached attack.")]
public class VengefulDefeat : GenericAbility
{
	protected const float AoeRadius = 2f;

	public GameObject OnGroundVisualEffect;

	private readonly AttackBase.FormattableTarget AOE_TARGET = new AttackBase.FormattableTarget(1606, 1603);

	protected override void Init()
	{
		if (!m_initialized)
		{
			base.Init();
			Health component = Owner.GetComponent<Health>();
			if (component != null)
			{
				component.OnUnconscious += HandleHealthOnUnconscious;
			}
			m_permanent = true;
		}
	}

	private void HandleHealthOnUnconscious(GameObject myObject, GameEventArgs args)
	{
		Equipment component = Owner.GetComponent<Equipment>();
		if (component == null)
		{
			return;
		}
		AttackBase primaryAttack = component.PrimaryAttack;
		AttackBase secondaryAttack = component.SecondaryAttack;
		bool flag = !(primaryAttack == null) && primaryAttack is AttackMelee;
		bool flag2 = !(secondaryAttack == null) && secondaryAttack is AttackMelee;
		if (!flag && !flag2)
		{
			return;
		}
		GameObject[] array = GameUtilities.CreaturesInRange(Owner.transform.position, 2f, Owner, includeUnconscious: false);
		if (array == null)
		{
			return;
		}
		GameUtilities.LaunchEffect(OnGroundVisualEffect, 1f, Owner.transform.position, this);
		GameObject[] array2 = array;
		foreach (GameObject enemy in array2)
		{
			if (flag)
			{
				primaryAttack.SkipAnimation = true;
				primaryAttack.LaunchingDirectlyToImpact = false;
				primaryAttack.Launch(enemy, this);
			}
			if (flag2)
			{
				secondaryAttack.SkipAnimation = true;
				secondaryAttack.LaunchingDirectlyToImpact = false;
				secondaryAttack.Launch(enemy, this);
			}
		}
	}

	public override string GetAdditionalEffects(StringEffects stringEffects, StatusEffectFormatMode mode, GenericAbility ability, GameObject character)
	{
		string text = AttackBase.FormatWC(GUIUtils.GetText(1590), GUIUtils.Format(1533, 2f.ToString("0.0#")));
		AttackBase.AddStringEffect(AOE_TARGET.GetText(AttackBase.TargetType.Hostile), new AttackBase.AttackEffect(GUIUtils.GetText(1615), null, hostile: true), stringEffects);
		return (text + "\n" + base.GetAdditionalEffects(stringEffects, mode, ability, character)).Trim();
	}
}
