using UnityEngine;

public class PE_Paperdoll : MonoBehaviour
{
	private static PE_Paperdoll s_Instance = null;

	private static Camera s_Camera;

	private static RenderTexture s_RenderTexture;

	private static Vector2 s_RenderTextureSize;

	private static GameObject s_Character;

	private static GameObject s_LoadedCharacter;

	public GameObject CharacterCreationSettings;

	public GameObject LevelUpSettings;

	public GameObject RecruitmentSettings;

	public GameObject InventorySettings;

	private static ParticleSystem[] systems = new ParticleSystem[0];

	private static TrailRenderer[] m_trailRenderers = new TrailRenderer[0];

	private static NPCAppearance oldAppearance;

	private static SkinnedMeshRenderer oldRenderer;

	private static GameObject oldSkeleton;

	private static Animator oldAnimator;

	private static SkinnedMeshRenderer newRenderer;

	private static GameObject newSkeleton;

	public static GameObject s_ModelOverride;

	public static Texture RenderImage => s_RenderTexture;

	public static GameObject PaperdollCharacter => s_Character;

	public static int PaperdollLayer => LayerMask.NameToLayer("Paperdoll");

	private void Update()
	{
		ParticleSystem[] array = systems;
		foreach (ParticleSystem particleSystem in array)
		{
			if (particleSystem != null)
			{
				particleSystem.Simulate(Time.unscaledDeltaTime, withChildren: true, restart: false);
			}
		}
		TrailRenderer[] trailRenderers = m_trailRenderers;
		foreach (TrailRenderer trailRenderer in trailRenderers)
		{
			if (trailRenderer != null)
			{
				trailRenderer.enabled = false;
			}
		}
		if ((bool)s_RenderTexture && !s_RenderTexture.IsCreated())
		{
			s_RenderTexture = new RenderTexture((int)s_RenderTextureSize.x, (int)s_RenderTextureSize.y, 32);
		}
	}

	public static bool IsObjectPaperdoll(GameObject character)
	{
		return character.layer == PaperdollLayer;
	}

	public static void CreateCameraCharacterCreation()
	{
		CreateCamera();
		s_Instance.RecruitmentSettings.SetActive(value: false);
		s_Instance.InventorySettings.SetActive(value: false);
		s_Instance.LevelUpSettings.SetActive(value: false);
		s_Instance.CharacterCreationSettings.SetActive(value: true);
		EnableCamera();
	}

	public static void CreateCameraLevelUp()
	{
		CreateCamera();
		s_Instance.CharacterCreationSettings.SetActive(value: false);
		s_Instance.RecruitmentSettings.SetActive(value: false);
		s_Instance.InventorySettings.SetActive(value: false);
		s_Instance.LevelUpSettings.SetActive(value: true);
		EnableCamera();
	}

	public static void CreateCameraRecruitment()
	{
		CreateCamera();
		s_Instance.CharacterCreationSettings.SetActive(value: false);
		s_Instance.InventorySettings.SetActive(value: false);
		s_Instance.LevelUpSettings.SetActive(value: false);
		s_Instance.RecruitmentSettings.SetActive(value: true);
		EnableCamera();
	}

	public static void CreateCameraInventory()
	{
		CreateCamera();
		s_Instance.CharacterCreationSettings.SetActive(value: false);
		s_Instance.RecruitmentSettings.SetActive(value: false);
		s_Instance.LevelUpSettings.SetActive(value: false);
		s_Instance.InventorySettings.SetActive(value: true);
		EnableCamera();
	}

	public static void CreateCamera()
	{
		if (s_Instance == null)
		{
			s_Instance = GameResources.LoadPrefab<GameObject>("Paper_Doll_Camera", instantiate: true).GetComponent<PE_Paperdoll>();
		}
		Light[] array = Object.FindObjectsOfType<Light>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].cullingMask &= ~(1 << PaperdollLayer);
		}
	}

	public static void ReloadTints()
	{
		NPCAppearance component = s_LoadedCharacter.GetComponent<NPCAppearance>();
		NPCAppearance component2 = s_Character.GetComponent<NPCAppearance>();
		MirrorCharacterUtils.CopyAppearance(component2, component);
		if ((bool)component2)
		{
			component2.ApplyTints();
		}
	}

	public static GameObject GetAlternateModel(GameObject character)
	{
		if ((bool)s_ModelOverride)
		{
			return s_ModelOverride;
		}
		foreach (GenericAbility activeAbility in character.GetComponent<CharacterStats>().ActiveAbilities)
		{
			Spiritshift spiritshift = activeAbility as Spiritshift;
			if (spiritshift != null && spiritshift.Activated)
			{
				return spiritshift.form;
			}
		}
		return null;
	}

	public static void HandleAlternateModelEnable(GameObject model)
	{
		if (model == null)
		{
			return;
		}
		GameObject gameObject = Object.Instantiate(model, s_Character.transform.position, s_Character.transform.rotation);
		Animator component = gameObject.GetComponent<Animator>();
		SkinnedMeshRenderer skinnedMeshRenderer = GameUtilities.FindSkinnedMeshRenderer(gameObject);
		GameObject gameObject2 = GameUtilities.FindSkeleton(gameObject);
		if (component == null || skinnedMeshRenderer == null || gameObject2 == null)
		{
			if (component == null)
			{
				Debug.LogError("--- Missing Animator component!");
			}
			if (skinnedMeshRenderer == null)
			{
				Debug.LogError("--- Cannot find skinned mesh renderer!");
			}
			if (gameObject2 == null)
			{
				Debug.LogError("--- Skeleton is missing or not tagged as skeleton!");
			}
			return;
		}
		oldAppearance = s_Character.GetComponent<NPCAppearance>();
		oldRenderer = GameUtilities.FindSkinnedMeshRenderer(s_Character);
		oldSkeleton = GameUtilities.FindSkeleton(s_Character);
		oldAnimator = GameUtilities.FindAnimator(s_Character);
		oldAppearance.enabled = false;
		oldRenderer.enabled = false;
		oldSkeleton.SetActive(value: false);
		newRenderer = oldRenderer.transform.parent.gameObject.AddComponent<SkinnedMeshRenderer>();
		newRenderer.material = skinnedMeshRenderer.material;
		newRenderer.rootBone = skinnedMeshRenderer.rootBone;
		newRenderer.bones = skinnedMeshRenderer.bones;
		newSkeleton = Object.Instantiate(gameObject2, s_Character.transform.position, s_Character.transform.rotation);
		newSkeleton.transform.parent = oldSkeleton.transform.parent;
		int layer = LayerUtility.FindLayerValue("Paperdoll");
		GameUtilities.RecursiveSetLayer(newSkeleton, layer);
		oldAnimator.runtimeAnimatorController = component.runtimeAnimatorController;
		oldAnimator.avatar = component.avatar;
		GameUtilities.SetAnimator(s_Character, oldAnimator);
		GameUtilities.Destroy(gameObject);
	}

	public static void HandleAlternateModelDisable()
	{
		if (!(newRenderer == null))
		{
			GameUtilities.DestroyImmediate(newRenderer);
			GameUtilities.DestroyImmediate(newSkeleton);
			oldAppearance.enabled = true;
			oldRenderer.enabled = true;
			oldSkeleton.SetActive(value: true);
			oldAnimator.runtimeAnimatorController = oldAppearance.controller;
			oldAnimator.avatar = oldAppearance.avatar;
			GameUtilities.SetAnimator(s_Character, oldAnimator);
			newRenderer = null;
			newSkeleton = null;
		}
	}

	public static void LoadCharacter(GameObject character)
	{
		s_LoadedCharacter = character;
		if (character == null)
		{
			return;
		}
		HandleAlternateModelDisable();
		CharacterStats component = character.GetComponent<CharacterStats>();
		NPCAppearance component2 = character.GetComponent<NPCAppearance>();
		Equipment component3 = character.GetComponent<Equipment>();
		Portrait component4 = character.GetComponent<Portrait>();
		if (!s_Character)
		{
			Object @object = GameResources.LoadPrefab("Paper_Doll", instantiate: false);
			if (@object is MonoBehaviour)
			{
				s_Character = GameResources.Instantiate<GameObject>((@object as MonoBehaviour).gameObject);
			}
			else
			{
				s_Character = GameResources.Instantiate<GameObject>(@object);
			}
			Persistence component5 = s_Character.GetComponent<Persistence>();
			if ((bool)component5)
			{
				GameUtilities.DestroyComponent(component5);
			}
			s_Character.name = "Paperdoll Character";
		}
		else
		{
			Cloth[] componentsInChildren = s_Character.GetComponentsInChildren<Cloth>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				GameUtilities.Destroy(componentsInChildren[i].gameObject);
			}
		}
		NPCAppearance component6 = s_Character.GetComponent<NPCAppearance>();
		CharacterStats component7 = s_Character.GetComponent<CharacterStats>();
		s_Character.GetComponent<NPCAppearance>().layer = PaperdollLayer;
		component7.Gender = component.Gender;
		component7.CharacterRace = component.CharacterRace;
		component7.CharacterSubrace = component.CharacterSubrace;
		component7.CharacterClass = component.CharacterClass;
		component7.BaseDexterity = component.BaseDexterity;
		component7.BaseMight = component.BaseMight;
		component7.BaseResolve = component.BaseResolve;
		component7.BaseIntellect = component.BaseIntellect;
		Portrait p = s_Character.AddComponent<Portrait>();
		if ((bool)component4)
		{
			component4.CopyTo(p);
		}
		MirrorCharacterUtils.LoadEquipment(s_Character, component3.CurrentItems);
		bool num = NPCAppearance.ConvertRace(component.RacialBodyType) != component6.racialBodyType || component6.gender != NPCAppearance.ConvertGender(component.Gender);
		MirrorCharacterUtils.CopyAppearance(component6, component2);
		if (num)
		{
			Transform transform = GameUtilities.FindSkeletonTransform(s_Character.gameObject);
			if (transform != null)
			{
				transform.name = "old_skel";
				GameUtilities.DestroyImmediate(transform.gameObject);
			}
			Animator component8 = s_Character.gameObject.GetComponent<Animator>();
			if ((bool)component8)
			{
				GameUtilities.DestroyImmediate(component8);
			}
		}
		component6.Generate();
		Animator component9 = s_Character.gameObject.GetComponent<Animator>();
		if ((bool)component9)
		{
			component9.updateMode = AnimatorUpdateMode.UnscaledTime;
		}
		AnimationController component10 = s_Character.gameObject.GetComponent<AnimationController>();
		if ((bool)component10)
		{
			component10.BindComponents();
		}
		MirrorCharacterUtils.LoadEquipment(s_Character, component3.CurrentItems);
		HandleAlternateModelEnable(GetAlternateModel(character));
		GameUtilities.RecursiveSetLayer(s_Character, PaperdollLayer);
		systems = s_Character.GetComponentsInChildren<ParticleSystem>();
		m_trailRenderers = s_Character.GetComponentsInChildren<TrailRenderer>();
	}

	public static void ReloadEquipment()
	{
		if ((bool)s_Character)
		{
			Equipment component = s_Character.GetComponent<Equipment>();
			MirrorCharacterUtils.LoadEquipment(s_Character, component.CurrentItems);
			systems = s_Character.GetComponentsInChildren<ParticleSystem>();
			m_trailRenderers = s_Character.GetComponentsInChildren<TrailRenderer>();
		}
	}

	public static void LookAt(float angle)
	{
		if ((bool)s_Character)
		{
			s_Character.transform.localRotation = Quaternion.AngleAxis(0f - angle, Vector3.up);
		}
	}

	public static void SetRenderSize(Rect dest)
	{
		float num = 2f / (float)Screen.height;
		float num2 = 2f / (float)InGameUILayout.Root.activeHeight;
		dest.width *= num2 / num;
		dest.height *= num2 / num;
		float num3 = dest.height / dest.width;
		if (dest.width > 2048f)
		{
			dest.width = 2048f;
			dest.height = dest.width * num3;
		}
		if (dest.height > 2048f)
		{
			dest.height = 2048f;
			dest.width = dest.height / num3;
		}
		if (s_RenderTexture == null || dest.width != (float)s_RenderTexture.width || dest.height != (float)s_RenderTexture.height)
		{
			if ((bool)s_RenderTexture)
			{
				s_RenderTexture.Release();
			}
			s_RenderTexture = new RenderTexture((int)dest.width, (int)dest.height, 32);
			s_RenderTextureSize = new Vector2(dest.width, dest.height);
		}
		s_Camera.targetTexture = s_RenderTexture;
		s_Camera.aspect = dest.width / dest.height;
	}

	public static void EnableCamera()
	{
		s_Camera = s_Instance.GetComponentInChildren<Camera>();
		s_Camera.gameObject.SetActive(value: true);
		GameCameraSettings componentInChildren = s_Camera.GetComponentInChildren<GameCameraSettings>();
		if ((bool)componentInChildren)
		{
			PE_LightEnvironment componentInChildren2 = s_Instance.GetComponentInChildren<PE_LightEnvironment>();
			if ((bool)componentInChildren2)
			{
				componentInChildren.LightEnvironment = componentInChildren2;
			}
		}
	}

	private void OnDestroy()
	{
		s_Camera = null;
		s_ModelOverride = null;
		if ((bool)s_Instance)
		{
			GameUtilities.Destroy(s_Instance.gameObject);
			s_Instance = null;
		}
		if ((bool)s_Character)
		{
			GameUtilities.Destroy(s_Character);
			s_Character = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public static void DisableCamera()
	{
		HandleAlternateModelDisable();
		s_ModelOverride = null;
		if (s_Camera != null)
		{
			s_Camera.gameObject.SetActive(value: false);
		}
		if ((bool)s_Instance)
		{
			GameUtilities.Destroy(s_Instance.gameObject);
			s_Instance = null;
		}
		if ((bool)s_Character)
		{
			GameUtilities.Destroy(s_Character);
			s_Character = null;
		}
	}
}
