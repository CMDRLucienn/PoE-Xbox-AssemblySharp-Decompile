using System;
using UnityEngine;

[Serializable]
public class BoneMapping
{
	public string BoneName;

	public AttackBase.EffectAttachType AttachPoint;

	public Vector3 Offset;

	public Transform BoneTransform { get; set; }

	public BoneMapping()
	{
	}

	public BoneMapping(AttackBase.EffectAttachType attach, string name, Vector3 offset)
	{
		AttachPoint = attach;
		BoneName = name;
		offset = Offset;
	}

	public override string ToString()
	{
		return BoneName;
	}
}
