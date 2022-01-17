using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCAppearance : MonoBehaviour
{
	public delegate void OnGenerate();

	public enum Sex
	{
		Male,
		Female
	}

	public enum Race
	{
		HUM,
		ELF,
		DWA,
		AUM,
		ORL,
		GOD
	}

	public enum Subrace
	{
		Undefined,
		Meadow_Human,
		Ocean_Human,
		Savannah_Human,
		Wood_Elf,
		Snow_Elf,
		Mountain_Dwarf,
		Boreal_Dwarf,
		Death_Godlike,
		Fire_Godlike,
		Nature_Godlike,
		Moon_Godlike,
		Hearth_Orlan,
		Wild_Orlan,
		Coastal_Aumaua,
		Island_Aumaua,
		Avian_Godlike,
		Advanced_Construct
	}

	public bool _IsBlackShamrockDebug;

	private GameUtilities.CapeType capeType = GameUtilities.CapeType.M_HUM;

	public static string CHARACTER_PATH = "Art/Character/";

	public static string SKELETON_POSTFIX = "_Skeleton";

	private const string mPrimaryWeaponBone = "primaryWeapon";

	private const string mSecondaryWeaponBone = "secondaryWeapon";

	private const string mPrimaryScabbardBone = "primaryScabbard";

	private const string mSecondaryScabbardBone = "secondaryScabbard";

	private const string mBackScabbardBone = "backScabbard";

	private const string mShieldBone = "LeftForeArm_Att";

	private static readonly string[] mHDTextureFolders = new string[2] { "assets/data/art/character/hd/", "assets/data/art/weapons/hd/" };

	public OnGenerate OnPostGenerate;

	[Persistent]
	public Color primaryColor = new Color(0.38f, 0.27f, 0.19f);

	[Persistent]
	public Color secondaryColor = new Color(0.18f, 0.09f, 0.02f);

	[Persistent]
	public Color skinColor = new Color(0.88f, 0.83f, 0.78f);

	[Persistent]
	public Color hairColor = new Color(0.33f, 0.28f, 0.22f);

	[Persistent]
	public AppearancePiece headAppearance = new AppearancePiece();

	[Persistent]
	public AppearancePiece hairAppearance = new AppearancePiece();

	[Persistent]
	public AppearancePiece facialHairAppearance = new AppearancePiece();

	[Persistent]
	public string nudeModelOverride = "";

	[Persistent]
	public string skinOverride = "";

	[Persistent]
	public bool hasHead = true;

	[Persistent]
	public bool hasHair = true;

	[Persistent]
	public bool hasFacialHair;

	[Persistent]
	public bool ignoreDefaultNudeLegModel;

	[HideInInspector]
	public int layer = -1;

	private Mesh m_mesh;

	private GameObject m_capeMeshObject;

	private Equippable m_capeCachedEquippable;

	private bool m_isCreatingAppearance;

	public Avatar avatar;

	public RuntimeAnimatorController controller;

	[Persistent]
	public Sex gender;

	[Persistent]
	public Race race;

	[Persistent]
	public Subrace subrace;

	[Persistent]
	public Race racialBodyType;

	private static GameResources s_loader = new GameResources();

	private static List<UnityEngine.Object> s_cleanUpList = new List<UnityEngine.Object>();

	private static List<SkinnedMeshRenderer> s_skinnedParts = new List<SkinnedMeshRenderer>();

	private static List<Material> s_materials = new List<Material>();

	private static List<SkinnedMeshRenderer> s_newMeshes = new List<SkinnedMeshRenderer>();

	private static List<SkinnedMeshRenderer> s_subMeshList = new List<SkinnedMeshRenderer>();

	private static List<SkinnedMeshRenderer> s_subMeshBoots = new List<SkinnedMeshRenderer>();

	private static List<SkinnedMeshRenderer> s_subMeshGloves = new List<SkinnedMeshRenderer>();

	[Persistent]
	public bool HideHelmet { get; set; }

	public bool IsCreatingAppearance => m_isCreatingAppearance;

	public string GetGenderShortString()
	{
		if (gender != 0)
		{
			return "F";
		}
		return "M";
	}

	public string GetGenderFullString()
	{
		return gender.ToString();
	}

	public string GetRaceString()
	{
		return race.ToString();
	}

	public string GetSubRaceString()
	{
		return subrace switch
		{
			Subrace.Death_Godlike => "GODD", 
			Subrace.Fire_Godlike => "GODF", 
			Subrace.Moon_Godlike => "GODM", 
			Subrace.Nature_Godlike => "GODN", 
			_ => "", 
		};
	}

	public string GetHeadSubRaceFolder()
	{
		return subrace switch
		{
			Subrace.Death_Godlike => "GODD", 
			Subrace.Fire_Godlike => "GODF", 
			Subrace.Moon_Godlike => "GODM", 
			Subrace.Nature_Godlike => "GODN", 
			Subrace.Wild_Orlan => "WOR", 
			_ => "Default", 
		};
	}

	public string GetSubRaceFolder()
	{
		return subrace switch
		{
			Subrace.Death_Godlike => "GODD", 
			Subrace.Fire_Godlike => "GODF", 
			Subrace.Moon_Godlike => "GODM", 
			Subrace.Nature_Godlike => "GODN", 
			_ => "Default", 
		};
	}

	public string GetBodyString()
	{
		return racialBodyType.ToString();
	}

	public string GetSkeletonFileName()
	{
		return $"{GetGenderShortString()}_{GetBodyString()}{SKELETON_POSTFIX}";
	}

	public string GetSkeletonPath()
	{
		return $"{CHARACTER_PATH}{GetGenderFullString()}/{GetBodyString()}/";
	}

	public string GetSkeletonFullPath()
	{
		return $"{GetSkeletonPath()}{GetSkeletonFileName()}";
	}

	private void Reset()
	{
		headAppearance.bodyPiece = AppearancePiece.BodyPiece.Head;
		hairAppearance.bodyPiece = AppearancePiece.BodyPiece.Hair;
		facialHairAppearance.bodyPiece = AppearancePiece.BodyPiece.Facialhair;
	}

	private void Start()
	{
		if (race != Race.GOD)
		{
			racialBodyType = race;
		}
		else if (racialBodyType == Race.GOD)
		{
			racialBodyType = Race.HUM;
		}
		Generate();
	}

	private void OnDestroy()
	{
		if ((bool)m_mesh)
		{
			GameUtilities.DestroyImmediate(m_mesh);
			m_mesh = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private GameObject LoadReferenceSkeleton(GameResources loader)
	{
		string skeletonFullPath = GetSkeletonFullPath();
		loader.LoadBundle<GameObject>(skeletonFullPath);
		if (loader.obj == null)
		{
			Debug.LogError("Skeleton not found for appearance. Object: " + base.gameObject.name + " Skeleton: " + GetSkeletonFullPath());
			return null;
		}
		return loader.obj as GameObject;
	}

	private GameObject CreateMeshObject(Transform parent)
	{
		GameObject gameObject = null;
		Transform transform = base.gameObject.transform.Find("Mesh");
		if (transform == null)
		{
			gameObject = new GameObject("Mesh");
			gameObject.hideFlags = HideFlags.DontSave;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = Vector3.one;
			transform = gameObject.transform;
			gameObject.transform.parent = parent.transform;
		}
		gameObject = transform.gameObject;
		if (layer == -1)
		{
			gameObject.layer = LayerUtility.FindLayerValue("Dynamics");
		}
		else
		{
			gameObject.layer = layer;
		}
		return gameObject;
	}

	private SkinnedMeshRenderer CreateSkinnedMeshRenderer(GameObject parent)
	{
		SkinnedMeshRenderer skinnedMeshRenderer = parent.GetComponent("SkinnedMeshRenderer") as SkinnedMeshRenderer;
		if (skinnedMeshRenderer == null)
		{
			skinnedMeshRenderer = parent.AddComponent<SkinnedMeshRenderer>();
		}
		skinnedMeshRenderer.updateWhenOffscreen = true;
		return skinnedMeshRenderer;
	}

	private List<AppearancePiece> CollectAllAppearancePieces()
	{
		List<AppearancePiece> list = new List<AppearancePiece>();
		Equipment component = GetComponent<Equipment>();
		if ((bool)component)
		{
			if (component.CurrentItems != null)
			{
				list.AddRange(component.CurrentItems.GetAppearancePieces());
			}
			else
			{
				list.AddRange(component.DefaultEquippedItems.GetAppearancePieces());
			}
		}
		bool flag = false;
		foreach (AppearancePiece item in list)
		{
			if (item.bodyPiece == AppearancePiece.BodyPiece.Body)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			AppearancePiece appearancePiece = new AppearancePiece();
			appearancePiece.bodyPiece = AppearancePiece.BodyPiece.Body;
			appearancePiece.armorType = AppearancePiece.ArmorType.None;
			appearancePiece.modelVariation = 1;
			appearancePiece.materialVariation = 1;
			appearancePiece.specialOverride = nudeModelOverride;
			list.Add(appearancePiece);
		}
		if (hasHead)
		{
			if (!GameUtilities.IsOctoberHoliday())
			{
				list.Add(headAppearance);
			}
			else
			{
				AppearancePiece appearancePiece2 = new AppearancePiece();
				appearancePiece2.bodyPiece = AppearancePiece.BodyPiece.Head;
				appearancePiece2.armorType = AppearancePiece.ArmorType.None;
				appearancePiece2.modelVariation = 1;
				appearancePiece2.materialVariation = 1;
				appearancePiece2.specialOverride = GetPumpkinHeadString();
				list.Add(appearancePiece2);
			}
		}
		if (hasHair)
		{
			list.Add(hairAppearance);
		}
		if (hasFacialHair)
		{
			list.Add(facialHairAppearance);
		}
		return list;
	}

	private void OnDrawGizmosSelected()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		Gizmos.color = Color.white;
		Transform transform = base.gameObject.transform.Find("Reference");
		if (transform != null)
		{
			Transform[] componentsInChildren = transform.gameObject.GetComponentsInChildren<Transform>();
			foreach (Transform transform2 in componentsInChildren)
			{
				Gizmos.DrawSphere(transform2.position, 0.05f);
				Gizmos.DrawLine(transform2.position, transform2.transform.parent.position);
			}
		}
	}

	private void GetSubMeshes(AppearancePiece piece, GameObject parent, bool bodyOverride, bool partialOverride, ref List<SkinnedMeshRenderer> subMeshes)
	{
		string modelMeshName = piece.GetModelMeshName(this, bodyOverride, partialOverride);
		SkinnedMeshRenderer[] componentsInChildren = parent.GetComponentsInChildren<SkinnedMeshRenderer>();
		subMeshes.Clear();
		SkinnedMeshRenderer[] array = componentsInChildren;
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in array)
		{
			if (skinnedMeshRenderer.gameObject.name.Contains(modelMeshName))
			{
				subMeshes.Add(skinnedMeshRenderer);
			}
		}
	}

	private string GetSubMeshSuffix(AppearancePiece piece, GameObject parent)
	{
		string modelMeshName = piece.GetModelMeshName(this, bodyOverride: false, partialOverride: false);
		SkinnedMeshRenderer[] componentsInChildren = parent.GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren)
		{
			if (skinnedMeshRenderer.gameObject.name.Contains(modelMeshName))
			{
				string text = skinnedMeshRenderer.gameObject.name.Substring(modelMeshName.Length);
				text = text.Substring(Math.Max(3, text.Length) - 3);
				if (text != string.Empty)
				{
					return text;
				}
				return string.Empty;
			}
		}
		return string.Empty;
	}

	private void RemoveExtraSubmeshes(AppearancePiece piece, List<SkinnedMeshRenderer> subMeshList)
	{
		string modelMeshName = piece.GetModelMeshName(this, bodyOverride: false, partialOverride: false);
		for (int num = subMeshList.Count - 1; num >= 0; num--)
		{
			if (subMeshList[num].gameObject.name.Contains(modelMeshName))
			{
				string text = subMeshList[num].gameObject.name.Substring(modelMeshName.Length);
				if (text != string.Empty && char.ToUpper(text[1]) == AppearancePiece.EXTRA_MESH)
				{
					subMeshList.RemoveAt(num);
				}
			}
		}
	}

	public void ApplyTints()
	{
		SkinnedMeshRenderer[] componentsInChildren = GetComponentsInChildren<SkinnedMeshRenderer>();
		if (componentsInChildren == null)
		{
			return;
		}
		SkinnedMeshRenderer[] array = componentsInChildren;
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in array)
		{
			Material[] array2 = null;
			array2 = ((Application.isEditor && !Application.isPlaying) ? skinnedMeshRenderer.sharedMaterials : skinnedMeshRenderer.materials);
			Material[] array3 = array2;
			foreach (Material material in array3)
			{
				if (material == null)
				{
					Debug.LogError("NULL Material found on skinned mesh renderer object: " + base.gameObject.name);
				}
				else if (material.HasProperty("_TintMap") && (bool)material.GetTexture("_TintMap"))
				{
					if (material.name.Contains("Skin") || material.name.Contains("Head"))
					{
						material.SetColor("_TintColor1", skinColor);
						continue;
					}
					if (material.name.Contains("Hair"))
					{
						material.SetColor("_TintColor1", hairColor);
						continue;
					}
					material.SetColor("_TintColor1", primaryColor);
					material.SetColor("_TintColor2", secondaryColor);
				}
			}
		}
	}

	private void AttachEmissiveScroll(GameObject meshGameObject)
	{
		if (meshGameObject.GetComponent<Renderer>() == null)
		{
			return;
		}
		Material[] sharedMaterials = meshGameObject.GetComponent<Renderer>().sharedMaterials;
		if (sharedMaterials == null)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < sharedMaterials.Length; i++)
		{
			if ((bool)sharedMaterials[i] && sharedMaterials[i].HasProperty("_EmissiveScroll"))
			{
				flag = true;
				break;
			}
		}
		PE_EmissiveAlphaMesh component = meshGameObject.GetComponent<PE_EmissiveAlphaMesh>();
		if (!flag)
		{
			if (component != null)
			{
				GameUtilities.Destroy(component);
			}
		}
		else if (component == null)
		{
			component = meshGameObject.AddComponent<PE_EmissiveAlphaMesh>();
		}
	}

	private void AttachScaleWeaponBones(Transform skeleton)
	{
		string[] array = new string[6] { "primaryWeapon", "secondaryWeapon", "primaryScabbard", "secondaryScabbard", "backScabbard", "LeftForeArm_Att" };
		for (int i = 0; i < array.Length; i++)
		{
			Transform transform = FindBone(skeleton, array[i]);
			if (!(transform != null))
			{
				continue;
			}
			Transform transform2 = new GameObject(array[i] + "Scaled").transform;
			transform2.parent = transform;
			Transform[] array2 = new Transform[transform.childCount];
			for (int j = 0; j < transform.childCount; j++)
			{
				Transform transform3 = (array2[j] = transform.GetChild(j));
			}
			for (int k = 0; k < array2.Length; k++)
			{
				if (array2[k] != transform2)
				{
					array2[k].transform.parent = transform2;
				}
			}
			transform2.localPosition = Vector3.zero;
			transform2.localRotation = Quaternion.identity;
			Vector3 localScale = transform.localScale;
			transform.localScale = Vector3.one;
			transform2.localScale = localScale;
		}
	}

	private void AttachCape(Transform skeleton)
	{
		Equipment component = GetComponent<Equipment>();
		if (component == null)
		{
			return;
		}
		Equippable equippable = null;
		if (component.CurrentItems != null)
		{
			if (component.CurrentItems.Neck != null && component.CurrentItems.Neck.Appearance.bodyPiece == AppearancePiece.BodyPiece.Cape)
			{
				equippable = component.CurrentItems.Neck;
			}
		}
		else if (component.DefaultEquippedItems != null && component.DefaultEquippedItems.Neck != null && component.DefaultEquippedItems.Neck.Appearance.bodyPiece == AppearancePiece.BodyPiece.Cape)
		{
			equippable = component.DefaultEquippedItems.Neck;
		}
		if (equippable == null)
		{
			if (m_capeMeshObject != null)
			{
				GameUtilities.Destroy(m_capeMeshObject);
				m_capeMeshObject = null;
			}
			m_capeCachedEquippable = null;
		}
		else
		{
			if (m_capeCachedEquippable == equippable)
			{
				return;
			}
			if (m_capeMeshObject != null)
			{
				GameUtilities.Destroy(m_capeMeshObject);
				m_capeMeshObject = null;
			}
			m_capeCachedEquippable = equippable;
			Transform transform = FindBone(skeleton, "Spine3");
			_ = transform == null;
			string text = GetCapePrefabPath();
			bool flag = ((base.gameObject.layer == PE_Paperdoll.PaperdollLayer) ? true : false);
			if (flag)
			{
				text += "_Paperdoll";
			}
			s_loader.LoadBundle<GameObject>(text);
			if (!s_loader.obj)
			{
				return;
			}
			m_capeMeshObject = UnityEngine.Object.Instantiate(s_loader.obj) as GameObject;
			m_capeMeshObject.name = "Cape Mesh";
			m_capeMeshObject.transform.parent = transform;
			m_capeMeshObject.transform.localPosition = Vector3.zero;
			m_capeMeshObject.transform.localRotation = Quaternion.Euler(90f, 270f, 0f);
			Cloth component2 = m_capeMeshObject.transform.GetChild(0).GetComponent<Cloth>();
			if (component2 != null)
			{
				StartCoroutine(DampCloth(component2));
			}
			if (flag)
			{
				m_capeMeshObject.layer = PE_Paperdoll.PaperdollLayer;
				m_capeMeshObject.transform.GetChild(0).gameObject.layer = PE_Paperdoll.PaperdollLayer;
			}
			else
			{
				m_capeMeshObject.layer = LayerUtility.FindLayerValue("Dynamics");
				m_capeMeshObject.transform.GetChild(0).gameObject.layer = m_capeMeshObject.layer;
				GameUtilities.CapeColliderData capeColliderData = GameUtilities.GetCapeColliderData(capeType);
				Transform transform2 = skeleton.Find("Cape Collider");
				if (!(transform2 == null))
				{
					CapsuleCollider[] array2 = (component2.capsuleColliders = new CapsuleCollider[1] { transform2.GetComponent<CapsuleCollider>() });
				}
				else
				{
					GameObject obj = new GameObject("Cape Collider")
					{
						layer = 10
					};
					CapsuleCollider capsuleCollider = obj.AddComponent<CapsuleCollider>();
					capsuleCollider.radius = capeColliderData.radius;
					capsuleCollider.height = capeColliderData.height;
					capsuleCollider.center = capeColliderData.center;
					capsuleCollider.direction = (capeColliderData.directionIsYAxis ? 1 : 0);
					obj.transform.SetParent(skeleton);
					obj.transform.localPosition = new Vector3(0f, 0f, 0f);
					obj.transform.localRotation = Quaternion.identity;
					CapsuleCollider[] array4 = (component2.capsuleColliders = new CapsuleCollider[1] { capsuleCollider });
				}
			}
			SkinnedMeshRenderer componentInChildren = m_capeMeshObject.GetComponentInChildren<SkinnedMeshRenderer>();
			string text2 = "Art/Character/Textures/Cape/m_Cape01_V";
			text2 = ((equippable.Appearance.materialVariation >= 10) ? (text2 + equippable.Appearance.materialVariation) : (text2 + "0" + equippable.Appearance.materialVariation));
			s_loader.LoadBundle<Material>(text2);
			if (!(s_loader.obj != null))
			{
				Debug.LogError("Cape Material Asset could not be found using GameResources.LoadBundle() Try Search via Resources at: '" + text2 + "'");
				try
				{
					componentInChildren.material = Resources.Load(text2, typeof(Material)) as Material;
					Debug.Log("Successfully Loaded '" + text2 + "'");
					return;
				}
				catch (NullReferenceException ex)
				{
					Debug.LogError("Cape Material Asset could not be found! Searched for at: '" + text2 + "' " + ex.Message);
					return;
				}
			}
			componentInChildren.material = s_loader.obj as Material;
		}
	}

	private IEnumerator DampCloth(Cloth cloth)
	{
		cloth.damping = 1f;
		while (Time.timeScale == 0f)
		{
			yield return null;
		}
		StartCoroutine(ReleaseDampCloth(cloth));
		yield return 0;
	}

	private IEnumerator ReleaseDampCloth(Cloth cloth)
	{
		while (cloth.damping > 0.41f)
		{
			cloth.damping -= 0.01f;
			yield return null;
		}
		yield return 0;
	}

	private string GetCapePrefabPath()
	{
		string text = "";
		if (gender == Sex.Female)
		{
			switch (racialBodyType)
			{
			case Race.AUM:
				text = "Art/Character/Female/HUM/Cape/F_AUM_Cape01";
				capeType = GameUtilities.CapeType.F_AUM;
				break;
			case Race.DWA:
				text = "Art/Character/Female/HUM/Cape/F_ORL_DWA_Cape01";
				capeType = GameUtilities.CapeType.F_ORL;
				break;
			case Race.ORL:
				text = "Art/Character/Female/HUM/Cape/F_ORL_DWA_Cape01";
				capeType = GameUtilities.CapeType.F_ORL;
				break;
			case Race.HUM:
				text = "Art/Character/Female/HUM/Cape/F_HUM_Cape01";
				capeType = GameUtilities.CapeType.F_HUM;
				break;
			case Race.ELF:
				text = "Art/Character/Female/HUM/Cape/F_HUM_Cape01";
				capeType = GameUtilities.CapeType.F_HUM;
				break;
			default:
				text = "Art/Character/Female/HUM/Cape/F_HUM_Cape01";
				capeType = GameUtilities.CapeType.F_HUM;
				break;
			}
		}
		else
		{
			switch (racialBodyType)
			{
			case Race.AUM:
				text = "Art/Character/Male/HUM/Cape/M_AUM_Cape01";
				capeType = GameUtilities.CapeType.M_AUM;
				break;
			case Race.DWA:
				text = "Art/Character/Male/HUM/Cape/M_ORL_DWA_Cape01";
				capeType = GameUtilities.CapeType.M_ORL;
				break;
			case Race.ORL:
				text = "Art/Character/Male/HUM/Cape/M_ORL_DWA_Cape01";
				capeType = GameUtilities.CapeType.M_ORL;
				break;
			case Race.HUM:
				text = "Art/Character/Male/HUM/Cape/M_HUM_Cape01";
				capeType = GameUtilities.CapeType.M_HUM;
				break;
			case Race.ELF:
				text = "Art/Character/Male/HUM/Cape/M_HUM_Cape01";
				capeType = GameUtilities.CapeType.M_HUM;
				break;
			default:
				text = "Art/Character/Male/HUM/Cape/M_HUM_Cape01";
				capeType = GameUtilities.CapeType.M_HUM;
				break;
			}
		}
		return text;
	}

	private Transform AddCapeBone(Transform neck)
	{
		GameObject obj = new GameObject("bn_cloth_01");
		obj.transform.parent = neck;
		obj.transform.localPosition = new Vector3(0.02f, 0.15f, 0f);
		obj.transform.localRotation = Quaternion.Euler(70f, 270f, 0f);
		return obj.transform;
	}

	public static void ReplaceTexturesWithHDTextures(GameObject gameObject, bool createNewMaterials)
	{
		Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
		if (componentsInChildren == null)
		{
			return;
		}
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i] == null)
			{
				continue;
			}
			Material[] array = null;
			array = ((Application.isEditor && !Application.isPlaying) ? componentsInChildren[i].sharedMaterials : componentsInChildren[i].materials);
			for (int j = 0; j < array.Length; j++)
			{
				Material material = array[j];
				string[] array2 = new string[4] { "_MainTex", "_BumpMap", "_EmissiveMap", "_TintMap" };
				foreach (string text in array2)
				{
					if (!material.HasProperty(text))
					{
						continue;
					}
					Texture texture = null;
					Texture texture2 = null;
					texture = material.GetTexture(text);
					if ((bool)texture)
					{
						texture2 = GameResources.LoadTextureBundle(texture.name.Replace("(Clone)", "") + "_HD");
						if ((bool)texture2)
						{
							material = (array[j] = new Material(material));
							material.SetTexture(text, texture2);
						}
					}
				}
				if (!Application.isEditor || Application.isPlaying)
				{
					componentsInChildren[i].materials = array;
				}
				else
				{
					componentsInChildren[i].sharedMaterials = array;
				}
			}
		}
	}

	private void ReplaceSkin()
	{
		SkinnedMeshRenderer[] componentsInChildren = GetComponentsInChildren<SkinnedMeshRenderer>();
		if (componentsInChildren == null)
		{
			return;
		}
		int num = 1;
		if (race == Race.AUM)
		{
			num = 3;
		}
		else if (race == Race.ORL)
		{
			num = 4;
		}
		else if (race == Race.GOD)
		{
			if (subrace == Subrace.Death_Godlike)
			{
				num = 6;
			}
			else if (subrace == Subrace.Fire_Godlike)
			{
				num = 7;
			}
			else if (subrace == Subrace.Moon_Godlike)
			{
				num = 8;
			}
			else if (subrace == Subrace.Nature_Godlike)
			{
				num = 5;
			}
		}
		bool flag = false;
		if (!string.IsNullOrEmpty(skinOverride))
		{
			num = 1;
			flag = true;
		}
		else if (num == 1)
		{
			return;
		}
		SkinnedMeshRenderer[] array = componentsInChildren;
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in array)
		{
			Material[] array2 = null;
			array2 = ((Application.isEditor && !Application.isPlaying) ? skinnedMeshRenderer.sharedMaterials : skinnedMeshRenderer.materials);
			for (int j = 0; j < array2.Length; j++)
			{
				if (array2[j] == null)
				{
					Debug.LogError("NULL Material found on skinned mesh renderer object: " + base.gameObject.name);
				}
				else if (array2[j].name.Contains("Skin"))
				{
					string path = $"{CHARACTER_PATH}{GetGenderFullString()}/Textures/Skin/m_{GetGenderShortString()}_Skin_V{num:D2}";
					if (flag)
					{
						path = string.Format("{0}SpecialNPC/{1}/m_{1}_Skin", CHARACTER_PATH, skinOverride);
					}
					s_loader.LoadBundle<Material>(path);
					Material material = s_loader.obj as Material;
					if ((bool)material)
					{
						array2[j] = UnityEngine.Object.Instantiate(material);
					}
				}
			}
			if (!Application.isEditor || Application.isPlaying)
			{
				skinnedMeshRenderer.materials = array2;
			}
			else
			{
				skinnedMeshRenderer.sharedMaterials = array2;
			}
		}
	}

	private Transform FindBone(Transform current, string name)
	{
		if (current.name == name)
		{
			return current;
		}
		for (int i = 0; i < current.childCount; i++)
		{
			Transform transform = FindBone(current.GetChild(i), name);
			if (transform != null)
			{
				return transform;
			}
		}
		return null;
	}

	public void SetHelmetVisibility(bool visible)
	{
		HideHelmet = !visible;
		Generate();
	}

	public void Generate()
	{
		if (m_mesh == null)
		{
			m_mesh = new Mesh();
		}
		m_mesh.name = base.gameObject.name;
		Transform transform = base.transform;
		GameObject gameObject = CreateMeshObject(transform);
		SkinnedMeshRenderer skinnedMeshRenderer = CreateSkinnedMeshRenderer(gameObject);
		GameObject gameObject2 = LoadReferenceSkeleton(s_loader);
		if (gameObject2 == null)
		{
			return;
		}
		m_isCreatingAppearance = true;
		Transform transform2 = GameUtilities.FindSkeletonTransform(base.gameObject);
		if (transform2 == null)
		{
			GameObject gameObject3 = UnityEngine.Object.Instantiate(gameObject2, transform.position, transform.rotation);
			gameObject3.hideFlags = HideFlags.DontSave;
			gameObject3.name = "Skeleton";
			gameObject3.tag = "Skeleton";
			transform2 = gameObject3.transform.Find("Reference");
			gameObject3.transform.parent = transform;
			AttachScaleWeaponBones(gameObject3.transform);
			avatar = null;
		}
		GameObject gameObject4 = UnityEngine.Object.Instantiate(gameObject2);
		s_cleanUpList.Add(gameObject4);
		Transform refRootBone = gameObject4.transform.Find("Reference");
		List<AppearancePiece> list = CollectAllAppearancePieces();
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		bool flag6 = false;
		bool flag7 = false;
		bool flag8 = false;
		bool flag9 = false;
		if (GameUtilities.IsOctoberHoliday())
		{
			flag3 = true;
			flag4 = true;
			flag5 = true;
		}
		foreach (AppearancePiece item in list)
		{
			if (item.bodyPiece == AppearancePiece.BodyPiece.Boots)
			{
				flag8 = true;
			}
			if (item.bodyPiece == AppearancePiece.BodyPiece.Gloves)
			{
				flag9 = true;
			}
		}
		foreach (AppearancePiece item2 in list)
		{
			if (item2.bodyPiece == AppearancePiece.BodyPiece.None || (item2.bodyPiece == AppearancePiece.BodyPiece.Boots && flag) || (item2.bodyPiece == AppearancePiece.BodyPiece.Gloves && flag2) || (item2.bodyPiece == AppearancePiece.BodyPiece.Hair && flag3) || (item2.bodyPiece == AppearancePiece.BodyPiece.Facialhair && flag4) || (item2.bodyPiece == AppearancePiece.BodyPiece.Helm && HideHelmet) || (item2.bodyPiece == AppearancePiece.BodyPiece.Helm && flag5))
			{
				continue;
			}
			string modelFullPath = item2.GetModelFullPath(this, bodyOverride: false);
			s_loader.LoadBundle<GameObject>(modelFullPath);
			if (!s_loader.obj)
			{
				continue;
			}
			s_newMeshes.Clear();
			GameObject gameObject5 = (GameObject)UnityEngine.Object.Instantiate(s_loader.obj);
			if (item2.bodyPiece == AppearancePiece.BodyPiece.Body || item2.bodyPiece == AppearancePiece.BodyPiece.Helm)
			{
				string subMeshSuffix = GetSubMeshSuffix(item2, gameObject5);
				item2.ApplySuffixToAppearance(subMeshSuffix);
				if (item2.hideHair)
				{
					flag3 = true;
				}
				if (item2.hideFacialHair)
				{
					flag4 = true;
				}
				if (item2.hideGloves)
				{
					flag2 = true;
				}
				if (item2.hideBoots)
				{
					flag = true;
				}
				if (item2.partialHair)
				{
					flag6 = true;
				}
				if (item2.partialFacialHair)
				{
					flag7 = true;
				}
			}
			bool partialOverride = false;
			bool flag10 = false;
			if (item2.bodyPiece == AppearancePiece.BodyPiece.Hair && flag6)
			{
				partialOverride = true;
			}
			if (item2.bodyPiece == AppearancePiece.BodyPiece.Facialhair && flag7)
			{
				partialOverride = true;
			}
			if (item2.bodyPiece == AppearancePiece.BodyPiece.Head && flag3)
			{
				flag10 = true;
			}
			GetSubMeshes(item2, gameObject5, bodyOverride: false, partialOverride, ref s_subMeshList);
			if (flag10)
			{
				RemoveExtraSubmeshes(item2, s_subMeshList);
			}
			if (s_subMeshList.Count > 0)
			{
				s_newMeshes.AddRange(s_subMeshList);
				if (item2.materialVariation > 1)
				{
					string materialFullPath = item2.GetMaterialFullPath(this, bodyOverride: false);
					s_loader.LoadBundle<Material>(materialFullPath);
					if ((bool)s_loader.obj)
					{
						AppearancePiece appearancePiece = new AppearancePiece();
						appearancePiece.bodyPiece = item2.bodyPiece;
						appearancePiece.armorType = item2.armorType;
						appearancePiece.modelVariation = item2.modelVariation;
						appearancePiece.materialVariation = item2.materialVariation;
						string text = appearancePiece.GetMaterialFileName(this, bodyOverride: false).ToLower();
						for (int i = 0; i < s_subMeshList.Count; i++)
						{
							Material material = null;
							material = ((Application.isEditor && !Application.isPlaying) ? s_subMeshList[i].sharedMaterial : s_subMeshList[i].material);
							string subMeshMaterialFileName = appearancePiece.GetSubMeshMaterialFileName(this, bodyOverride: false, material);
							if (text != subMeshMaterialFileName)
							{
								s_loader.LoadBundle<Material>(subMeshMaterialFileName);
								if ((bool)s_loader.obj)
								{
									material = s_loader.obj as Material;
								}
							}
							if (material != null)
							{
								s_materials.Add(material);
								continue;
							}
							Debug.LogError("NULL Material found on skinned mesh renderer object: " + base.gameObject.name + " " + s_subMeshList[i].name);
							s_materials.Add(Resources.Load("Material/Missing", typeof(Material)) as Material);
						}
					}
				}
				else
				{
					for (int j = 0; j < s_subMeshList.Count; j++)
					{
						Material material2 = null;
						material2 = ((Application.isEditor && !Application.isPlaying) ? s_subMeshList[j].sharedMaterial : s_subMeshList[j].material);
						if (material2 != null)
						{
							s_materials.Add(material2);
							continue;
						}
						Debug.LogError("NULL Material found on skinned mesh renderer object: " + base.gameObject.name + " " + s_subMeshList[j].name);
						s_materials.Add(Resources.Load("Material/Missing", typeof(Material)) as Material);
					}
				}
				if (item2.bodyPiece == AppearancePiece.BodyPiece.Body)
				{
					if (!flag8)
					{
						AppearancePiece appearancePiece2 = new AppearancePiece();
						appearancePiece2.bodyPiece = AppearancePiece.BodyPiece.Boots;
						appearancePiece2.armorType = item2.armorType;
						appearancePiece2.modelVariation = item2.modelVariation;
						appearancePiece2.materialVariation = item2.materialVariation;
						GetSubMeshes(appearancePiece2, gameObject5, bodyOverride: true, partialOverride: false, ref s_subMeshBoots);
						if (s_subMeshBoots.Count == 0 && !ignoreDefaultNudeLegModel)
						{
							appearancePiece2.modelVariation = 1;
							modelFullPath = appearancePiece2.GetModelFullPath(this, bodyOverride: true);
							s_loader.LoadBundle<GameObject>(modelFullPath);
							if ((bool)s_loader.obj)
							{
								GameObject gameObject6 = (GameObject)UnityEngine.Object.Instantiate(s_loader.obj);
								GetSubMeshes(appearancePiece2, gameObject6, bodyOverride: true, partialOverride: false, ref s_subMeshBoots);
								s_cleanUpList.Add(gameObject6);
							}
						}
						if (s_subMeshBoots.Count > 0)
						{
							s_newMeshes.AddRange(s_subMeshBoots);
							string materialFullPath2 = appearancePiece2.GetMaterialFullPath(this, bodyOverride: true);
							s_loader.LoadBundle<Material>(materialFullPath2, suppressErrors: true);
							for (int k = 0; k < s_subMeshBoots.Count; k++)
							{
								Material material3 = null;
								material3 = ((Application.isEditor && !Application.isPlaying) ? s_subMeshBoots[k].sharedMaterial : s_subMeshBoots[k].material);
								if (material3 != null)
								{
									string subMeshMaterialFileName2 = appearancePiece2.GetSubMeshMaterialFileName(this, bodyOverride: true, material3);
									s_loader.LoadBundle<Material>(subMeshMaterialFileName2, suppressErrors: true);
									if ((bool)s_loader.obj)
									{
										UnityEngine.Object obj = s_loader.obj;
										s_materials.Add(obj as Material);
									}
									else
									{
										appearancePiece2.modelVariation = 1;
										materialFullPath2 = appearancePiece2.GetMaterialFullPath(this, bodyOverride: true);
										s_materials.Add(material3);
									}
								}
								else
								{
									Debug.LogError("NULL Material found on skinned mesh renderer object: " + base.gameObject.name + " " + s_subMeshBoots[k].name);
									s_materials.Add(Resources.Load("Material/Missing", typeof(Material)) as Material);
								}
							}
						}
					}
					if (!flag9)
					{
						AppearancePiece appearancePiece3 = new AppearancePiece();
						appearancePiece3.bodyPiece = AppearancePiece.BodyPiece.Gloves;
						appearancePiece3.armorType = item2.armorType;
						appearancePiece3.modelVariation = item2.modelVariation;
						appearancePiece3.materialVariation = item2.materialVariation;
						GetSubMeshes(appearancePiece3, gameObject5, bodyOverride: true, partialOverride: false, ref s_subMeshGloves);
						if (s_subMeshGloves.Count == 0)
						{
							appearancePiece3.modelVariation = 1;
							modelFullPath = appearancePiece3.GetModelFullPath(this, bodyOverride: true);
							s_loader.LoadBundle<GameObject>(modelFullPath, suppressErrors: true);
							if ((bool)s_loader.obj)
							{
								GameObject gameObject7 = (GameObject)UnityEngine.Object.Instantiate(s_loader.obj);
								GetSubMeshes(appearancePiece3, gameObject7, bodyOverride: true, partialOverride: false, ref s_subMeshGloves);
								s_cleanUpList.Add(gameObject7);
							}
						}
						if (s_subMeshGloves.Count > 0)
						{
							s_newMeshes.AddRange(s_subMeshGloves);
							string materialFullPath3 = appearancePiece3.GetMaterialFullPath(this, bodyOverride: true);
							s_loader.LoadBundle<Material>(materialFullPath3, suppressErrors: true);
							for (int l = 0; l < s_subMeshGloves.Count; l++)
							{
								Material material4 = null;
								material4 = ((Application.isEditor && !Application.isPlaying) ? s_subMeshGloves[l].sharedMaterial : s_subMeshGloves[l].material);
								if (material4 != null)
								{
									string subMeshMaterialFileName3 = appearancePiece3.GetSubMeshMaterialFileName(this, bodyOverride: true, material4);
									s_loader.LoadBundle<Material>(subMeshMaterialFileName3, suppressErrors: true);
									if ((bool)s_loader.obj)
									{
										UnityEngine.Object obj2 = s_loader.obj;
										s_materials.Add(obj2 as Material);
									}
									else
									{
										appearancePiece3.materialVariation = 1;
										materialFullPath3 = appearancePiece3.GetMaterialFullPath(this, bodyOverride: true);
										s_materials.Add(material4);
									}
								}
								else
								{
									Debug.LogError("NULL Material found on skinned mesh renderer object: " + base.gameObject.name + " " + s_subMeshGloves[l].name);
									s_materials.Add(Resources.Load("Material/Missing", typeof(Material)) as Material);
								}
							}
						}
					}
				}
			}
			else
			{
				string modelMeshName = item2.GetModelMeshName(this, bodyOverride: false, partialOverride);
				Debug.LogError("Could not find submesh: " + modelMeshName + " in " + modelFullPath);
			}
			s_skinnedParts.AddRange(s_newMeshes);
			s_cleanUpList.Add(gameObject5);
		}
		if (s_skinnedParts.Count > 0)
		{
			SkinnedMeshCombiner.CombineSkinnedMeshParts(skinnedMeshRenderer, m_mesh, s_skinnedParts, s_materials, transform2, refRootBone, atlasTextures: true);
		}
		AttachCape(transform2);
		ReplaceSkin();
		ApplyTints();
		if (PE_Paperdoll.IsObjectPaperdoll(base.gameObject))
		{
			ReplaceTexturesWithHDTextures(base.gameObject, createNewMaterials: true);
		}
		Animator animator = base.gameObject.GetComponent<Animator>();
		if (animator == null)
		{
			animator = base.gameObject.AddComponent<Animator>();
		}
		if ((bool)animator)
		{
			if (!PE_Paperdoll.IsObjectPaperdoll(base.gameObject))
			{
				animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
			}
			animator.applyRootMotion = false;
			if (animator.avatar == null || avatar == null)
			{
				if (avatar == null)
				{
					Animator component = gameObject2.GetComponent<Animator>();
					if ((bool)component)
					{
						avatar = component.avatar;
					}
				}
				animator.avatar = avatar;
			}
			if (animator.runtimeAnimatorController == null)
			{
				if (controller == null)
				{
					controller = (RuntimeAnimatorController)Resources.Load("Data/default", typeof(RuntimeAnimatorController));
				}
				animator.runtimeAnimatorController = controller;
			}
		}
		AnimationController component2 = GetComponent<AnimationController>();
		if ((bool)component2)
		{
			component2.BindComponents();
		}
		foreach (UnityEngine.Object s_cleanUp in s_cleanUpList)
		{
			GameUtilities.DestroyImmediate(s_cleanUp);
		}
		AttachEmissiveScroll(gameObject);
		AnimationBoneMapper component3 = base.gameObject.GetComponent<AnimationBoneMapper>();
		if ((bool)component3)
		{
			component3.Reinitialize();
		}
		if (BigHeads.Enabled)
		{
			BigHeads.Apply(base.gameObject);
		}
		skinnedMeshRenderer.transform.localPosition = Vector3.zero;
		skinnedMeshRenderer.transform.localRotation = Quaternion.identity;
		skinnedMeshRenderer.transform.localScale = Vector3.one;
		s_materials.Clear();
		s_cleanUpList.Clear();
		s_skinnedParts.Clear();
		s_newMeshes.Clear();
		s_subMeshList.Clear();
		s_subMeshBoots.Clear();
		s_subMeshGloves.Clear();
		s_loader.obj = null;
		m_isCreatingAppearance = false;
		AlphaControl component4 = base.gameObject.GetComponent<AlphaControl>();
		if (component4 != null)
		{
			component4.Refresh();
		}
		MaterialCache component5 = GetComponent<MaterialCache>();
		if ((bool)component5)
		{
			component5.Reapply();
		}
		if (OnPostGenerate != null)
		{
			OnPostGenerate();
		}
	}

	public static Sex ConvertGender(Gender gender)
	{
		return gender switch
		{
			Gender.Male => Sex.Male, 
			Gender.Female => Sex.Female, 
			_ => Sex.Male, 
		};
	}

	public static Race ConvertRace(CharacterStats.Race race)
	{
		return race switch
		{
			CharacterStats.Race.Human => Race.HUM, 
			CharacterStats.Race.Dwarf => Race.DWA, 
			CharacterStats.Race.Elf => Race.ELF, 
			CharacterStats.Race.Orlan => Race.ORL, 
			CharacterStats.Race.Aumaua => Race.AUM, 
			CharacterStats.Race.Godlike => Race.GOD, 
			_ => Race.HUM, 
		};
	}

	public static Subrace ConvertSubrace(CharacterStats.Subrace subrace)
	{
		return subrace switch
		{
			CharacterStats.Subrace.Meadow_Human => Subrace.Meadow_Human, 
			CharacterStats.Subrace.Ocean_Human => Subrace.Ocean_Human, 
			CharacterStats.Subrace.Savannah_Human => Subrace.Savannah_Human, 
			CharacterStats.Subrace.Wood_Elf => Subrace.Wood_Elf, 
			CharacterStats.Subrace.Snow_Elf => Subrace.Snow_Elf, 
			CharacterStats.Subrace.Mountain_Dwarf => Subrace.Mountain_Dwarf, 
			CharacterStats.Subrace.Boreal_Dwarf => Subrace.Boreal_Dwarf, 
			CharacterStats.Subrace.Death_Godlike => Subrace.Death_Godlike, 
			CharacterStats.Subrace.Fire_Godlike => Subrace.Fire_Godlike, 
			CharacterStats.Subrace.Nature_Godlike => Subrace.Nature_Godlike, 
			CharacterStats.Subrace.Moon_Godlike => Subrace.Moon_Godlike, 
			CharacterStats.Subrace.Hearth_Orlan => Subrace.Hearth_Orlan, 
			CharacterStats.Subrace.Wild_Orlan => Subrace.Wild_Orlan, 
			CharacterStats.Subrace.Coastal_Aumaua => Subrace.Coastal_Aumaua, 
			CharacterStats.Subrace.Island_Aumaua => Subrace.Island_Aumaua, 
			CharacterStats.Subrace.Avian_Godlike => Subrace.Avian_Godlike, 
			CharacterStats.Subrace.Advanced_Construct => Subrace.Advanced_Construct, 
			_ => Subrace.Undefined, 
		};
	}

	private string GetPumpkinHeadString()
	{
		if (gender == Sex.Male)
		{
			return race switch
			{
				Race.AUM => "M_AUM_Pumpkin", 
				Race.DWA => "M_DWA_Pumpkin", 
				Race.ORL => "M_ORL_Pumpkin", 
				Race.HUM => "M_HUM_Pumpkin", 
				Race.ELF => "M_ELF_Pumpkin", 
				_ => "M_HUM_Pumpkin", 
			};
		}
		return race switch
		{
			Race.AUM => "F_AUM_Pumpkin", 
			Race.DWA => "F_DWA_Pumpkin", 
			Race.ORL => "F_ORL_Pumpkin", 
			Race.HUM => "F_HUM_Pumpkin", 
			Race.ELF => "F_ELF_Pumpkin", 
			_ => "F_HUM_Pumpkin", 
		};
	}
}
