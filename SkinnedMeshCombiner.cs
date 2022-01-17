using System;
using System.Collections.Generic;
using UnityEngine;

public static class SkinnedMeshCombiner
{
	private static List<CombineInstance> s_combineInstances = new List<CombineInstance>();

	private static List<Transform> s_currentBones = new List<Transform>();

	private static List<int> s_boneIndices = new List<int>();

	private static List<Matrix4x4> s_bindPoses = new List<Matrix4x4>();

	private static List<Material> s_atlasMaterials = new List<Material>();

	private static List<Texture2D> s_colorTextures = new List<Texture2D>();

	private static List<Texture2D> s_normalTextures = new List<Texture2D>();

	private static Dictionary<int, Queue<int>> s_refBoneTransformNameHashes = new Dictionary<int, Queue<int>>();

	private static Dictionary<int, Queue<int>> s_curBoneTransformNameHashes = new Dictionary<int, Queue<int>>();

	private static Dictionary<int, int> s_curBoneNameHashesIndices = new Dictionary<int, int>();

	private static ObjectPool<Queue<int>> s_hashListPool = new ObjectPool<Queue<int>>(256);

	public static void CombineSkinnedMeshParts(SkinnedMeshRenderer destMeshRenderer, Mesh destMesh, List<SkinnedMeshRenderer> meshParts, List<Material> materials, Transform destRootBone, Transform refRootBone, bool atlasTextures)
	{
		atlasTextures = false;
		Transform[] componentsInChildren = refRootBone.GetComponentsInChildren<Transform>();
		Transform[] componentsInChildren2 = destRootBone.GetComponentsInChildren<Transform>();
		CacheTransformIndicesPerNameHash(componentsInChildren, componentsInChildren2);
		MapBonesPosesAndMeshes(meshParts, componentsInChildren, componentsInChildren2);
		if (atlasTextures)
		{
			CreateAtlasTexture(materials);
		}
		else
		{
			s_atlasMaterials.AddRange(materials);
		}
		CreateCombinedMesh(destMeshRenderer, destMesh);
		ClearStaticData();
	}

	private static void CacheTransformIndicesPerNameHash(Transform[] referenceBoneTransforms, Transform[] currentBoneTransforms)
	{
		int num = currentBoneTransforms.Length;
		int num2 = referenceBoneTransforms.Length;
		for (int i = 0; i < num2; i++)
		{
			Transform transform = referenceBoneTransforms[i];
			Queue<int> value = null;
			if (!s_refBoneTransformNameHashes.TryGetValue(Animator.StringToHash(transform.name), out value))
			{
				value = s_hashListPool.Allocate();
				s_refBoneTransformNameHashes.Add(Animator.StringToHash(transform.name), value);
			}
			value.Enqueue(i);
		}
		for (int j = 0; j < num; j++)
		{
			Transform transform2 = currentBoneTransforms[j];
			Queue<int> value2 = null;
			if (!s_curBoneTransformNameHashes.TryGetValue(Animator.StringToHash(transform2.name), out value2))
			{
				value2 = s_hashListPool.Allocate();
				s_curBoneTransformNameHashes.Add(Animator.StringToHash(transform2.name), value2);
			}
			value2.Enqueue(j);
		}
	}

	public static void MapBonesPosesAndMeshes(List<SkinnedMeshRenderer> meshParts, Transform[] referenceBoneTransforms, Transform[] currentBoneTransforms)
	{
		int count = meshParts.Count;
		for (int i = 0; i < count; i++)
		{
			SkinnedMeshRenderer skinnedMeshRenderer = meshParts[i];
			if (!skinnedMeshRenderer)
			{
				continue;
			}
			Transform[] bones = skinnedMeshRenderer.bones;
			int num = bones.Length;
			for (int j = 0; j < num; j++)
			{
				int key = Animator.StringToHash(bones[j].name);
				Queue<int> value = null;
				if (s_refBoneTransformNameHashes.TryGetValue(key, out value))
				{
					int num2 = value.Dequeue();
					Transform obj = referenceBoneTransforms[num2];
					if (value.Count == 0)
					{
						s_refBoneTransformNameHashes.Remove(key);
						s_hashListPool.Free(value);
					}
					Matrix4x4 worldToLocalMatrix = obj.worldToLocalMatrix;
					s_bindPoses.Add(worldToLocalMatrix);
				}
				if (s_curBoneTransformNameHashes.TryGetValue(key, out value))
				{
					int num3 = value.Dequeue();
					Transform item = currentBoneTransforms[num3];
					if (value.Count == 0)
					{
						s_curBoneTransformNameHashes.Remove(key);
						s_hashListPool.Free(value);
					}
					s_currentBones.Add(item);
					s_curBoneNameHashesIndices.Add(key, s_currentBones.Count - 1);
				}
				int value2 = -1;
				if (!s_curBoneNameHashesIndices.TryGetValue(key, out value2))
				{
					value2 = -1;
				}
				s_boneIndices.Add(value2);
			}
			int subMeshCount = skinnedMeshRenderer.sharedMesh.subMeshCount;
			for (int k = 0; k < subMeshCount; k++)
			{
				CombineInstance item2 = default(CombineInstance);
				item2.mesh = skinnedMeshRenderer.sharedMesh;
				item2.subMeshIndex = k;
				s_combineInstances.Add(item2);
			}
		}
	}

	public static void CreateAtlasTexture(List<Material> materials)
	{
		Shader shader = Shader.Find("Trenton/BumpSpec");
		Material material = new Material(shader);
		foreach (Material material2 in materials)
		{
			if (material2.shader == shader)
			{
				Texture2D texture2D = material2.GetTexture("_MainTex") as Texture2D;
				if ((bool)texture2D)
				{
					s_colorTextures.Add(texture2D);
				}
				Texture2D texture2D2 = material2.GetTexture("_BumpMap") as Texture2D;
				if ((bool)texture2D2)
				{
					s_normalTextures.Add(texture2D2);
				}
			}
			else
			{
				Texture2D texture2D3 = material2.GetTexture("_MainTex") as Texture2D;
				if ((bool)texture2D3)
				{
					s_colorTextures.Add(texture2D3);
					s_normalTextures.Add(texture2D3);
				}
				Debug.LogWarning("Material/Shader Combo not supported on Character. " + material2.name);
			}
		}
		Texture2D texture2D4 = new Texture2D(2048, 2048);
		Rect[] array = texture2D4.PackTextures(s_colorTextures.ToArray(), 0, 2048);
		material.SetTexture("_MainTex", texture2D4);
		Texture2D texture2D5 = new Texture2D(2048, 2048);
		texture2D5.PackTextures(s_normalTextures.ToArray(), 0, 2048);
		material.SetTexture("_BumpMap", texture2D5);
		for (int i = 0; i < s_combineInstances.Count; i++)
		{
			Mesh mesh = s_combineInstances[i].mesh;
			int num = mesh.uv.Length;
			Vector2[] array2 = new Vector2[num];
			Array.Copy(mesh.uv, array2, num);
			if (i < array.Length)
			{
				for (int j = 0; j < num; j++)
				{
					array2[j].x = array[i].x + array[i].width * array2[j].x;
					array2[j].y = array[i].y + array[i].height * array2[j].y;
				}
			}
			mesh.uv = array2;
		}
		s_atlasMaterials.Add(material);
	}

	public static void CreateCombinedMesh(SkinnedMeshRenderer destMeshRenderer, Mesh destMesh)
	{
		destMesh.Clear();
		destMesh.CombineMeshes(s_combineInstances.ToArray(), mergeSubMeshes: false, useMatrices: false);
		destMesh.bindposes = s_bindPoses.ToArray();
		destMeshRenderer.bones = s_currentBones.ToArray();
		destMeshRenderer.materials = s_atlasMaterials.ToArray();
		destMesh.subMeshCount = s_combineInstances.Count;
		BoneWeight[] boneWeights = destMesh.boneWeights;
		int num = boneWeights.Length;
		BoneWeight[] array = new BoneWeight[num];
		for (int i = 0; i < num; i++)
		{
			if (boneWeights[i].boneIndex0 < s_boneIndices.Count)
			{
				array[i].boneIndex0 = s_boneIndices[boneWeights[i].boneIndex0];
			}
			if (boneWeights[i].boneIndex1 < s_boneIndices.Count)
			{
				array[i].boneIndex1 = s_boneIndices[boneWeights[i].boneIndex1];
			}
			if (boneWeights[i].boneIndex2 < s_boneIndices.Count)
			{
				array[i].boneIndex2 = s_boneIndices[boneWeights[i].boneIndex2];
			}
			if (boneWeights[i].boneIndex3 < s_boneIndices.Count)
			{
				array[i].boneIndex3 = s_boneIndices[boneWeights[i].boneIndex3];
			}
			array[i].weight0 = boneWeights[i].weight0;
			array[i].weight1 = boneWeights[i].weight1;
			array[i].weight2 = boneWeights[i].weight2;
			array[i].weight3 = boneWeights[i].weight3;
		}
		destMesh.boneWeights = array;
		destMeshRenderer.sharedMesh = destMesh;
	}

	public static void ClearStaticData()
	{
		foreach (KeyValuePair<int, Queue<int>> s_refBoneTransformNameHash in s_refBoneTransformNameHashes)
		{
			s_hashListPool.Free(s_refBoneTransformNameHash.Value);
			s_refBoneTransformNameHash.Value.Clear();
		}
		foreach (KeyValuePair<int, Queue<int>> s_curBoneTransformNameHash in s_curBoneTransformNameHashes)
		{
			s_hashListPool.Free(s_curBoneTransformNameHash.Value);
			s_curBoneTransformNameHash.Value.Clear();
		}
		s_combineInstances.Clear();
		s_currentBones.Clear();
		s_boneIndices.Clear();
		s_bindPoses.Clear();
		s_atlasMaterials.Clear();
		s_colorTextures.Clear();
		s_normalTextures.Clear();
		s_refBoneTransformNameHashes.Clear();
		s_curBoneTransformNameHashes.Clear();
		s_curBoneNameHashesIndices.Clear();
	}
}
