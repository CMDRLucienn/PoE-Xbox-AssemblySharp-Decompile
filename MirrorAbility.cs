using System;
using System.Collections.Generic;
using UnityEngine;

[ClassTooltip("Creates NumberOfCopies images of the caster when cast. One is destroyed each time a Hit or Crit is scored on the caster.")]
public class MirrorAbility : GenericAbility
{
	public int NumberOfCopies = 1;

	private List<GameObject> m_copy = new List<GameObject>();

	protected override void Apply(GameObject target)
	{
		if (m_ownerStats != null)
		{
			m_ownerStats.ClearEffectFromAbility(this);
		}
		base.Apply(target);
		MakeCopies();
		if (m_ownerHealth != null)
		{
			m_ownerHealth.OnDamaged += HandleOnDamaged;
		}
	}

	public override void HandleOnMyEffectRemoved()
	{
		DestroyCopies();
		if (m_ownerHealth != null)
		{
			m_ownerHealth.OnDamaged -= HandleOnDamaged;
		}
	}

	public override void HandleOnDamaged(GameObject myObject, GameEventArgs args)
	{
		DamageInfo damageInfo = (DamageInfo)args.GenericData[0];
		if (damageInfo != null && damageInfo.AttackIsHostile && (damageInfo.IsCriticalHit || damageInfo.IsPlainHit))
		{
			DestroyRandomCopy();
		}
	}

	private void MakeCopies()
	{
		if (NumberOfCopies != 0)
		{
			float num = 0f;
			float num2 = (float)Math.PI * 2f / (float)NumberOfCopies;
			for (int i = 0; i < NumberOfCopies; i++)
			{
				GameObject gameObject = MirrorCharacterUtils.MirrorCharacter(Owner, MirrorCharacterUtils.MirrorType.Image);
				gameObject.transform.parent = Owner.transform;
				gameObject.transform.localPosition = new Vector3(Mathf.Cos(num), 0f, Mathf.Sin(num));
				m_copy.Add(gameObject);
				num += num2;
			}
		}
	}

	private void DestroyCopies()
	{
		foreach (GameObject item in m_copy)
		{
			GameUtilities.Destroy(item);
		}
		m_copy.Clear();
	}

	private void DestroyRandomCopy()
	{
		if (m_copy.Count != 0)
		{
			int index = OEIRandom.Index(m_copy.Count);
			GameUtilities.Destroy(m_copy[index]);
			m_copy.RemoveAt(index);
			if (m_copy.Count == 0)
			{
				OnInactive();
			}
		}
	}
}
