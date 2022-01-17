using UnityEngine;

public class SoulMirror : GenericAbility
{
	private const float ReflectChance = 0.5f;

	public override bool ListenForDamageEvents => true;

	public override void HandleOnDamaged(GameObject myObject, GameEventArgs args)
	{
		GameObject gameObject = args.GameObjectData[0];
		if (gameObject == null)
		{
			return;
		}
		DamageInfo damageInfo = (DamageInfo)args.GenericData[0];
		if (damageInfo != null && damageInfo.IsMiss && !(damageInfo.Attack == null) && damageInfo.Attack is AttackRanged && !(damageInfo.Attack is AttackAOE))
		{
			AttackRanged attackRanged = damageInfo.Attack as AttackRanged;
			if (!(attackRanged.ProjectilePrefab == null) && attackRanged.DefendedBy == CharacterStats.DefenseType.Deflect && !(OEIRandom.FloatValue() < 0.5f))
			{
				Transform hitTransform = damageInfo.Attack.GetHitTransform(gameObject);
				attackRanged.ProjectileLaunch(Owner.transform.position, hitTransform.position, gameObject, 0, canHitOwner: true);
			}
		}
	}

	public override string GetAdditionalEffects(StringEffects stringEffects, StatusEffectFormatMode mode, GenericAbility ability, GameObject character)
	{
		AttackBase.AddStringEffect(GetSelfTarget(), new AttackBase.AttackEffect(GUIUtils.Format(1988, GUIUtils.Format(1277, 50f.ToString("#0"))), base.Attack), stringEffects);
		return base.GetAdditionalEffects(stringEffects, mode, ability, character);
	}
}
