using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AnimationController))]
public class AnimationBoneMapper : MonoBehaviour
{
	public BoneMapping[] Bones = new BoneMapping[13]
	{
		new BoneMapping(AttackBase.EffectAttachType.Chest, "Spine1", Vector3.zero),
		new BoneMapping(AttackBase.EffectAttachType.ElbowRight, "RightForeArm", Vector3.zero),
		new BoneMapping(AttackBase.EffectAttachType.ElbowLeft, "LeftForeArm", Vector3.zero),
		new BoneMapping(AttackBase.EffectAttachType.Head, "Head", Vector3.zero),
		new BoneMapping(AttackBase.EffectAttachType.Hips, "Hips", Vector3.zero),
		new BoneMapping(AttackBase.EffectAttachType.LeftEye, "Head", Vector3.zero),
		new BoneMapping(AttackBase.EffectAttachType.RightEye, "Head", Vector3.zero),
		new BoneMapping(AttackBase.EffectAttachType.RightFoot, "RightFoot", Vector3.zero),
		new BoneMapping(AttackBase.EffectAttachType.LeftFoot, "LeftFoot", Vector3.zero),
		new BoneMapping(AttackBase.EffectAttachType.RightHand, "primaryWeapon", Vector3.zero),
		new BoneMapping(AttackBase.EffectAttachType.LeftHand, "secondaryWeapon", Vector3.zero),
		new BoneMapping(AttackBase.EffectAttachType.Mouth, "Head", Vector3.zero),
		new BoneMapping(AttackBase.EffectAttachType.Tail, "w_tail", Vector3.zero)
	};

	private Dictionary<GameObject, Dictionary<AttackBase.EffectAttachType, BoneMapping>> m_bones = new Dictionary<GameObject, Dictionary<AttackBase.EffectAttachType, BoneMapping>>();

	private bool m_instantiated;

	public Transform this[GameObject obj, AttackBase.EffectAttachType bone] => GetBone(obj, bone);

	public void Reinitialize()
	{
		m_bones.Clear();
		m_instantiated = false;
		Initialize();
	}

	public void Initialize()
	{
		if (!m_instantiated)
		{
			CreateMapping(base.gameObject, Bones, allowMissingBones: true);
		}
	}

	private void Update()
	{
		if (!m_instantiated)
		{
			CreateMapping(base.gameObject, Bones, allowMissingBones: true);
		}
	}

	public void CreateFXMapping(GameObject parentObj)
	{
		if (!m_bones.ContainsKey(parentObj))
		{
			BoneMapping[] boneMap = new BoneMapping[10]
			{
				new BoneMapping(AttackBase.EffectAttachType.Fx_Bone_01, "fx_bone_01", Vector3.zero),
				new BoneMapping(AttackBase.EffectAttachType.Fx_Bone_02, "fx_bone_02", Vector3.zero),
				new BoneMapping(AttackBase.EffectAttachType.Fx_Bone_03, "fx_bone_03", Vector3.zero),
				new BoneMapping(AttackBase.EffectAttachType.Fx_Bone_04, "fx_bone_04", Vector3.zero),
				new BoneMapping(AttackBase.EffectAttachType.Fx_Bone_05, "fx_bone_05", Vector3.zero),
				new BoneMapping(AttackBase.EffectAttachType.Fx_Bone_06, "fx_bone_06", Vector3.zero),
				new BoneMapping(AttackBase.EffectAttachType.Fx_Bone_07, "fx_bone_07", Vector3.zero),
				new BoneMapping(AttackBase.EffectAttachType.Fx_Bone_08, "fx_bone_08", Vector3.zero),
				new BoneMapping(AttackBase.EffectAttachType.Fx_Bone_09, "fx_bone_09", Vector3.zero),
				new BoneMapping(AttackBase.EffectAttachType.Fx_Bone_10, "fx_bone_10", Vector3.zero)
			};
			CreateMapping(parentObj, boneMap, allowMissingBones: false);
		}
	}

	public void ClearMapping(GameObject parentObj)
	{
		if (m_bones.ContainsKey(parentObj))
		{
			m_bones.Remove(parentObj);
		}
	}

	private void CreateMapping(GameObject parentObj, BoneMapping[] boneMap, bool allowMissingBones)
	{
		if (m_bones.ContainsKey(parentObj))
		{
			return;
		}
		foreach (BoneMapping boneMapping in boneMap)
		{
			if (boneMapping == null)
			{
				continue;
			}
			if (boneMapping.BoneName == string.Empty)
			{
				boneMapping.BoneName = boneMapping.AttachPoint.ToString().ToLower();
			}
			if (boneMapping.BoneName.ToLower() == "origin")
			{
				CreateTransform(parentObj, boneMapping, parentObj.transform);
				continue;
			}
			Transform transform = AnimationController.SearchForBoneTransform(boneMapping.BoneName, parentObj.transform);
			if (transform != null || allowMissingBones)
			{
				CreateTransform(parentObj, boneMapping, transform);
				if (parentObj == base.gameObject && boneMapping.BoneTransform != parentObj.transform)
				{
					m_instantiated = true;
				}
				if (boneMapping.BoneTransform == null)
				{
					boneMapping.BoneName = "ORIGIN";
					CreateTransform(parentObj, boneMapping, parentObj.transform);
				}
			}
		}
	}

	private void CreateTransform(GameObject parentObj, BoneMapping bm, Transform parent)
	{
		if (parent == null)
		{
			parent = base.transform;
		}
		Vector3 localScale = new Vector3(1f / parent.lossyScale.x, 1f / parent.lossyScale.y, 1f / parent.lossyScale.z);
		string n = parent.name + "AttachmentPoint";
		Transform transform = parent.transform.Find(n);
		GameObject gameObject = ((!(transform == null)) ? transform.gameObject : new GameObject(n));
		if (bm.AttachPoint == AttackBase.EffectAttachType.Head || bm.AttachPoint == AttackBase.EffectAttachType.Mouth || bm.AttachPoint == AttackBase.EffectAttachType.LeftEye || bm.AttachPoint == AttackBase.EffectAttachType.RightEye)
		{
			gameObject.transform.rotation = base.transform.rotation;
			gameObject.transform.localScale = localScale;
			gameObject.transform.parent = parent;
			gameObject.transform.localPosition = bm.Offset;
		}
		else
		{
			gameObject.transform.parent = parent;
			gameObject.transform.localPosition = bm.Offset;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = localScale;
		}
		bm.BoneTransform = gameObject.transform;
		Dictionary<AttackBase.EffectAttachType, BoneMapping> dictionary;
		if (m_bones.ContainsKey(parentObj))
		{
			dictionary = m_bones[parentObj];
		}
		else
		{
			dictionary = new Dictionary<AttackBase.EffectAttachType, BoneMapping>();
			m_bones.Add(parentObj, dictionary);
		}
		if (!dictionary.ContainsKey(bm.AttachPoint))
		{
			dictionary.Add(bm.AttachPoint, bm);
		}
		else
		{
			Debug.LogError(parentObj.name + " has multiple mappings for bone " + bm.AttachPoint.ToString() + " please check the bone mapper settings!", parentObj);
		}
	}

	public Transform GetBone(GameObject obj, AttackBase.EffectAttachType bone)
	{
		if (m_bones.ContainsKey(obj))
		{
			Dictionary<AttackBase.EffectAttachType, BoneMapping> dictionary = m_bones[obj];
			if (dictionary.ContainsKey(bone))
			{
				return dictionary[bone].BoneTransform;
			}
			if (bone >= AttackBase.EffectAttachType.Fx_Bone_02 && bone <= AttackBase.EffectAttachType.Fx_Bone_10)
			{
				return this[obj, AttackBase.EffectAttachType.Fx_Bone_01];
			}
			if (bone == AttackBase.EffectAttachType.BothHands || bone == AttackBase.EffectAttachType.Fx_Bone_01)
			{
				return this[obj, AttackBase.EffectAttachType.RightHand];
			}
			if (obj != base.gameObject)
			{
				return this[base.gameObject, bone];
			}
		}
		return base.transform;
	}

	public bool HasBone(GameObject obj, AttackBase.EffectAttachType bone)
	{
		if (m_bones.ContainsKey(obj) && m_bones[obj].ContainsKey(bone))
		{
			return true;
		}
		return false;
	}
}
