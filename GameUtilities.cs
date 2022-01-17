using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OEIFormats.FlowCharts;
using UnityEngine;
using UnityEngine.AI;

public class GameUtilities : MonoBehaviour
{
	public struct CapeColliderData
	{
		public float radius;

		public float height;

		public Vector3 center;

		public bool directionIsYAxis;
	}

	public enum CapeType
	{
		F_AUM,
		F_HUM,
		F_ORL,
		M_AUM,
		M_HUM,
		M_ORL
	}

	private static string[] s_sceneNames = null;

	private static GameUtilities s_instance = null;

	private static int s_wallLayerMask = 0;

	private static int s_walkLayerMask = 0;

	private static int s_doorLayerMask = 0;

	private static int s_dynamicsLayerMask = 0;

	private static int s_dynamicsNoShadowLayerMask = 0;

	private static int s_characterLayerMask = 0;

	private static List<Mover> s_potentialOverlap = new List<Mover>();

	private static List<GameObject> s_fadingEffects = new List<GameObject>();

	public const int MAX_PARTICLES = 400;

	public const float LOSDefaultHeight = 1f;

	private const float PositionScannerIncrement = 0.5f;

	private static AssetBundle m_defaultControllerAssetBundle = null;

	private static AssetBundle m_defaultShaderAssetBundle = null;

	private static AssetBundle m_defaultVFXAssetBundle = null;

	private static AssetBundle m_expansion1SceneAssetBundle = null;

	private static AssetBundle m_expansion2SceneAssetBundle = null;

	private static AssetBundle m_streamedSceneAssetBundle = null;

	public static bool ShowResolutionDebug = false;

	public static AssetBundle Expansion1SceneAssetBundle
	{
		get
		{
			return m_expansion1SceneAssetBundle;
		}
		set
		{
			m_expansion1SceneAssetBundle = value;
		}
	}

	public static AssetBundle Expansion2SceneAssetBundle
	{
		get
		{
			return m_expansion2SceneAssetBundle;
		}
		set
		{
			m_expansion2SceneAssetBundle = value;
		}
	}

	public static AssetBundle StreamedSceneAssetBundle
	{
		get
		{
			return m_streamedSceneAssetBundle;
		}
		set
		{
			m_streamedSceneAssetBundle = value;
		}
	}

	public Recipe[] RecipePrefabs { get; private set; }

	public static GameUtilities Instance
	{
		get
		{
			if ((bool)s_instance)
			{
				return s_instance;
			}
			CreateInGameGlobalPrefabObject();
			if (!s_instance)
			{
				s_instance = UnityEngine.Object.FindObjectOfType(typeof(GameUtilities)) as GameUtilities;
			}
			return s_instance;
		}
	}

	public static string[] SceneNames
	{
		get
		{
			return s_sceneNames;
		}
		set
		{
			s_sceneNames = value;
		}
	}

	private void Awake()
	{
		if (s_instance == null)
		{
			s_instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'GameUtilities' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
		s_wallLayerMask = 1 << LayerMask.NameToLayer("Wall");
		s_walkLayerMask = 1 << LayerMask.NameToLayer("Walkable");
		s_doorLayerMask = 1 << LayerMask.NameToLayer("Doors");
		s_dynamicsLayerMask = 1 << LayerMask.NameToLayer("Dynamics");
		s_dynamicsNoShadowLayerMask = 1 << LayerMask.NameToLayer("Dynamics No Shadow");
		s_characterLayerMask = 1 << LayerMask.NameToLayer("Character");
	}

	private void Start()
	{
		RecipePrefabs = GameResources.LoadAllPrefabsWithComponent<Recipe>();
	}

	private void Update()
	{
		if (ShowResolutionDebug)
		{
			DrawResolutionDebug();
		}
	}

	private void OnDestroy()
	{
		if (s_instance == this)
		{
			s_instance = null;
		}
		if (m_defaultControllerAssetBundle != null)
		{
			m_defaultControllerAssetBundle.Unload(unloadAllLoadedObjects: true);
			if (Application.isPlaying)
			{
				Destroy(m_defaultControllerAssetBundle);
			}
			m_defaultControllerAssetBundle = null;
		}
		if (m_defaultShaderAssetBundle != null)
		{
			m_defaultShaderAssetBundle.Unload(unloadAllLoadedObjects: true);
			if (Application.isPlaying)
			{
				Destroy(m_defaultShaderAssetBundle);
			}
			m_defaultShaderAssetBundle = null;
		}
		if (m_defaultVFXAssetBundle != null)
		{
			m_defaultVFXAssetBundle.Unload(unloadAllLoadedObjects: true);
			if (Application.isPlaying)
			{
				Destroy(m_defaultVFXAssetBundle);
			}
			m_defaultVFXAssetBundle = null;
		}
		if (m_expansion1SceneAssetBundle != null)
		{
			m_expansion1SceneAssetBundle.Unload(unloadAllLoadedObjects: true);
			if (Application.isPlaying)
			{
				Destroy(m_expansion1SceneAssetBundle);
			}
			m_expansion1SceneAssetBundle = null;
		}
		if (m_expansion2SceneAssetBundle != null)
		{
			m_expansion2SceneAssetBundle.Unload(unloadAllLoadedObjects: true);
			if (Application.isPlaying)
			{
				Destroy(m_expansion2SceneAssetBundle);
			}
			m_expansion2SceneAssetBundle = null;
		}
		if (m_streamedSceneAssetBundle != null)
		{
			m_streamedSceneAssetBundle.Unload(unloadAllLoadedObjects: true);
			if (Application.isPlaying)
			{
				Destroy(m_streamedSceneAssetBundle);
			}
			m_streamedSceneAssetBundle = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public static void RunBootstrapCommand()
	{
	}

	public new static void Destroy(UnityEngine.Object obj)
	{
		UnityEngine.Object.Destroy(obj);
	}

	public static void DestroyComponent(MonoBehaviour mono)
	{
		UnityEngine.Object.Destroy(mono);
	}

	public new static void Destroy(UnityEngine.Object obj, float time)
	{
		UnityEngine.Object.Destroy(obj, time);
	}

	public static void DestroyComponent(MonoBehaviour mono, float time)
	{
		UnityEngine.Object.Destroy(mono, time);
	}

	public new static void DestroyImmediate(UnityEngine.Object obj, bool allowDestroyingAssets)
	{
		UnityEngine.Object.DestroyImmediate(obj, allowDestroyingAssets);
	}

	public new static void DestroyImmediate(UnityEngine.Object obj)
	{
		UnityEngine.Object.DestroyImmediate(obj);
	}

	public static void DestroyComponentImmediate(MonoBehaviour mono)
	{
		UnityEngine.Object.DestroyImmediate(mono);
	}

	public static void BroadcastEvent(Type listenerType, GameEventArgs e)
	{
		object[] array = UnityEngine.Object.FindObjectsOfType(listenerType);
		object[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			if (array2[i] is IGameEventListener)
			{
				(array2[i] as IGameEventListener).OnEvent(e);
			}
		}
	}

	public static bool LineofSight(Vector3 source, GameObject target, float verticalOffset)
	{
		return LineofSight(source, target.transform.position, verticalOffset, includeDynamics: false, wallsOnly: false);
	}

	public static bool LineofSight(Vector3 source, Vector3 position, float verticalOffset, bool includeDynamics)
	{
		return LineofSight(source, position, verticalOffset, includeDynamics, wallsOnly: false);
	}

	public static bool LineofSight(Vector3 source, Vector3 position, float verticalOffset, bool includeDynamics, bool wallsOnly)
	{
		Vector3 vector = new Vector3(0f, verticalOffset, 0f);
		Vector3 vector2 = source + vector;
		Vector3 vector3 = position + vector - vector2;
		Ray ray = new Ray(vector2, vector3.normalized);
		int num = s_wallLayerMask;
		if (!wallsOnly)
		{
			num |= s_walkLayerMask;
			num |= s_doorLayerMask;
		}
		if (includeDynamics)
		{
			num |= s_dynamicsLayerMask;
			num |= s_dynamicsNoShadowLayerMask;
			num |= s_characterLayerMask;
		}
		if (Physics.Raycast(ray, out var hitInfo, vector3.magnitude, num))
		{
			if (hitInfo.rigidbody == null)
			{
				return false;
			}
			if ((hitInfo.rigidbody.gameObject.transform.position - position).sqrMagnitude < float.Epsilon)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public static GameObject[] CreaturesInRange(Vector3 center, float range, bool playerEnemiesOnly, bool includeUnconscious)
	{
		if (playerEnemiesOnly)
		{
			return CreaturesInRange(center, range, GameState.s_playerCharacter, includeUnconscious);
		}
		return CreaturesInRange(center, range, null, includeUnconscious);
	}

	public static GameObject[] CreaturesInRange(Vector3 center, float range, GameObject hostileTo, bool includeUnconscious)
	{
		List<GameObject> list = new List<GameObject>();
		float num = range * range;
		foreach (Faction activeFactionComponent in Faction.ActiveFactionComponents)
		{
			if (activeFactionComponent == null || (hostileTo != null && activeFactionComponent.GetRelationship(hostileTo) != Faction.Relationship.Hostile))
			{
				continue;
			}
			Health component = activeFactionComponent.GetComponent<Health>();
			if (!(component == null) && !component.Dead && (!component.Unconscious || includeUnconscious) && component.gameObject.activeInHierarchy)
			{
				float cachedRadius = activeFactionComponent.CachedRadius;
				num = (range + cachedRadius) * (range + cachedRadius);
				if (V3SqrDistance2D(center, activeFactionComponent.transform.position) < num)
				{
					list.Add(activeFactionComponent.gameObject);
				}
			}
		}
		if (list.Count > 0)
		{
			return list.ToArray();
		}
		return null;
	}

	public static float NearestPlayerSquaredDist(Vector3 point)
	{
		float num = float.MaxValue;
		for (int i = 0; i < PartyMemberAI.PartyMembers.Length; i++)
		{
			if (!(PartyMemberAI.PartyMembers[i] == null))
			{
				float num2 = V3SqrDistance2D(PartyMemberAI.PartyMembers[i].transform.position, point);
				if (num2 < num)
				{
					num = num2;
				}
			}
		}
		return num;
	}

	public static float NearestPlayerDist(Vector3 point)
	{
		return Mathf.Sqrt(NearestPlayerSquaredDist(point));
	}

	public static GameObject[] FriendsInRange(Vector3 center, float range, GameObject friendsWith, bool includeUnconscious)
	{
		List<GameObject> list = new List<GameObject>();
		float num = range * range;
		foreach (Faction activeFactionComponent in Faction.ActiveFactionComponents)
		{
			if (activeFactionComponent == null || activeFactionComponent.gameObject == friendsWith || (friendsWith != null && activeFactionComponent.GetRelationship(friendsWith) != Faction.Relationship.Friendly))
			{
				continue;
			}
			Health component = activeFactionComponent.GetComponent<Health>();
			if (!(component == null) && !component.Dead && (!component.Unconscious || includeUnconscious) && component.gameObject.activeInHierarchy && component.CanBeTargeted)
			{
				float cachedRadius = activeFactionComponent.CachedRadius;
				num = (range + cachedRadius) * (range + cachedRadius);
				if (V3SqrDistance2D(center, activeFactionComponent.transform.position) < num)
				{
					list.Add(activeFactionComponent.gameObject);
				}
			}
		}
		if (list.Count > 0)
		{
			return list.ToArray();
		}
		return null;
	}

	public static void GetEnemiesInRange(GameObject owner, AIController aiController, float range, List<GameObject> enemiesInRange, bool mustBeFowVisible = false)
	{
		float num = range * range;
		Faction component = owner.GetComponent<Faction>();
		if (component == null)
		{
			Debug.LogError(owner.name + " doesn't have a faction.", owner);
			return;
		}
		Vector3 position = owner.transform.position;
		bool isInPlayerFaction = component.IsInPlayerFaction;
		for (int i = 0; i < Faction.ActiveFactionComponents.Count; i++)
		{
			Faction faction = Faction.ActiveFactionComponents[i];
			if ((faction.IsInPlayerFaction || component.GetRelationship(faction) == Faction.Relationship.Hostile || faction.GetRelationship(component) == Faction.Relationship.Hostile || (isInPlayerFaction && faction.UnitHostileToPlayer)) && (component.IsHostile(faction) || faction.IsHostile(component) || (isInPlayerFaction && faction.UnitHostileToPlayer)))
			{
				AIController component2 = faction.GetComponent<AIController>();
				if (!(component2 == null) && !component2.IsPet && !component2.IsInvisible && (!mustBeFowVisible || faction.isFowVisible) && V3SqrDistance2D(position, faction.gameObject.transform.position) < num && aiController.IsTargetable(faction.gameObject))
				{
					enemiesInRange.Add(component2.gameObject);
				}
			}
		}
	}

	public static GameObject[] CreaturesAlongBeam(Vector3 start, Vector3 end, bool playerEnemiesOnly)
	{
		float num = Vector3.Distance(start, end) + 1f;
		List<GameObject> list = new List<GameObject>();
		foreach (Faction activeFactionComponent in Faction.ActiveFactionComponents)
		{
			if (!(activeFactionComponent == null) && (!playerEnemiesOnly || activeFactionComponent.RelationshipToPlayer == Faction.Relationship.Hostile))
			{
				Vector3 position = activeFactionComponent.transform.position;
				if (!(Vector3.Distance(start, position) > num) && DistanceLineSegmentToPoint(start, end, position) <= 1f)
				{
					list.Add(activeFactionComponent.gameObject);
				}
			}
		}
		if (list.Count > 0)
		{
			return list.ToArray();
		}
		return null;
	}

	public static Vector3 NearestUnoccupiedLocation(Vector3 start, float radius, float maxDistance, Mover ignoredMover)
	{
		s_potentialOverlap.Clear();
		bool flag = IsPositionOnNavMesh(start);
		Mover.GetMoversInRange(s_potentialOverlap, start, radius, maxDistance, ignoredMover);
		Mover overlappedMover = GetOverlappedMover(s_potentialOverlap, start, radius);
		NavMeshPath navMeshPath = new NavMeshPath();
		if (overlappedMover == null)
		{
			if (IsPositionOnNavMesh(start))
			{
				return start;
			}
		}
		else
		{
			Vector2 vector = V3Subtract2D(start, overlappedMover.transform.position);
			vector.Normalize();
			if (vector.sqrMagnitude < float.Epsilon)
			{
				vector.x = -1f;
			}
			Vector3 vector2 = overlappedMover.transform.position + V2ToV3(vector * (radius + overlappedMover.Radius + 0.05f));
			overlappedMover = GetOverlappedMover(s_potentialOverlap, vector2, radius);
			if (overlappedMover == null && IsPositionOnNavMesh(vector2))
			{
				if (!flag)
				{
					return vector2;
				}
				if (NavMesh.CalculatePath(start, vector2, int.MaxValue, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete)
				{
					return vector2;
				}
			}
		}
		float num = 0.5f;
		float num2 = (float)Math.PI * 2f;
		for (; num < maxDistance; num += 0.5f)
		{
			float num3 = num2 * num / 0.5f;
			float num4 = num2 / num3;
			for (float num5 = 0f; num5 < num2; num5 += num4)
			{
				Vector3 vector3;
				vector3.x = Mathf.Cos(num5);
				vector3.y = 0f;
				vector3.z = Mathf.Sin(num5);
				vector3 *= num;
				vector3 += start;
				overlappedMover = GetOverlappedMover(s_potentialOverlap, vector3, radius);
				if (overlappedMover == null && IsPositionOnNavMesh(vector3))
				{
					if (!flag)
					{
						return vector3;
					}
					if (NavMesh.CalculatePath(start, vector3, int.MaxValue, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete)
					{
						return vector3;
					}
				}
			}
		}
		IsPositionOnNavMesh(start);
		return start;
	}

	public static Mover GetOverlappedMover(List<Mover> movers, Vector3 start, float radius)
	{
		foreach (Mover mover in movers)
		{
			float num = V3SqrDistance2D(mover.transform.position, start);
			float num2 = mover.Radius + radius;
			if (num2 * num2 >= num)
			{
				return mover;
			}
		}
		return null;
	}

	public static GameObject FindAnimalCompanion(GameObject owner)
	{
		if (owner == null)
		{
			return null;
		}
		AIController aIController = FindActiveAIController(owner);
		if ((bool)aIController)
		{
			for (int i = 0; i < aIController.SummonedCreatureList.Count; i++)
			{
				GameObject gameObject = aIController.SummonedCreatureList[i];
				if (gameObject != null)
				{
					AIController component = gameObject.GetComponent<AIController>();
					if (component != null && component.SummonType == AIController.AISummonType.AnimalCompanion)
					{
						return gameObject;
					}
				}
			}
		}
		return null;
	}

	public static void RemoveAnimalCompanions(GameObject owner)
	{
		AIController aIController = FindActiveAIController(owner);
		if (!aIController)
		{
			return;
		}
		for (int num = aIController.SummonedCreatureList.Count - 1; num >= 0; num--)
		{
			GameObject gameObject = aIController.SummonedCreatureList[num];
			if (gameObject == null)
			{
				aIController.SummonedCreatureList.RemoveAt(num);
			}
			else
			{
				AIController aIController2 = FindActiveAIController(gameObject);
				if (aIController2 != null && aIController2.SummonType == AIController.AISummonType.AnimalCompanion)
				{
					aIController.SummonedCreatureList.RemoveAt(num);
					if (aIController is PartyMemberAI)
					{
						PartyMemberAI component = gameObject.GetComponent<PartyMemberAI>();
						if ((bool)component)
						{
							PartyMemberAI.RemoveFromActiveParty(component, purgePersistencePacket: true);
						}
					}
					Persistence component2 = gameObject.GetComponent<Persistence>();
					if ((bool)component2)
					{
						PersistenceManager.RemoveObject(component2);
					}
					Destroy(gameObject);
				}
			}
		}
	}

	public static bool IsPet(GameObject petObject)
	{
		if (petObject != null)
		{
			AIController component = petObject.GetComponent<AIController>();
			if (component != null && component.IsPet)
			{
				return true;
			}
		}
		return false;
	}

	public static GameObject FindMaster(GameObject animalCompanion)
	{
		if (animalCompanion != null)
		{
			AIController component = animalCompanion.GetComponent<AIController>();
			if (component != null && component.SummonType == AIController.AISummonType.AnimalCompanion)
			{
				return component.Summoner;
			}
		}
		return null;
	}

	public static bool IsAnimalCompanion(GameObject animalCompanion)
	{
		return FindMaster(animalCompanion) != null;
	}

	public static void KillAnimalCompanion(GameObject ranger)
	{
		GameObject gameObject = FindAnimalCompanion(ranger);
		if (gameObject != null)
		{
			Health component = gameObject.GetComponent<Health>();
			CharacterStats component2 = gameObject.GetComponent<CharacterStats>();
			if (component != null && component2 != null && component.m_isAnimalCompanion)
			{
				component.m_isAnimalCompanion = false;
				component2.ApplyAffliction(AfflictionData.Maimed);
				component.CanBeTargeted = true;
				component.ShouldDecay = true;
				component.ApplyHealthChangeDirectly((0f - component.CurrentHealth) * 10f, applyIfDead: false);
				component.ApplyDamageDirectly(component.MaxStamina * 100f);
			}
		}
	}

	public static bool ActiveAIControllerIsPartyMemberAI(GameObject obj)
	{
		if ((bool)obj)
		{
			PartyMemberAI component = obj.GetComponent<PartyMemberAI>();
			if ((bool)component)
			{
				return component.enabled;
			}
			return false;
		}
		return false;
	}

	public static AIController FindActiveAIController(GameObject obj)
	{
		if (obj == null)
		{
			return null;
		}
		AIPackageController component = obj.GetComponent<AIPackageController>();
		if ((bool)component && component.enabled)
		{
			return component;
		}
		PartyMemberAI component2 = obj.GetComponent<PartyMemberAI>();
		if ((bool)component2 && component2.enabled)
		{
			return component2;
		}
		AIControllerDummy component3 = obj.GetComponent<AIControllerDummy>();
		if ((bool)component3 && component3.enabled)
		{
			return component3;
		}
		AIController[] components = obj.GetComponents<AIController>();
		foreach (AIController aIController in components)
		{
			if (aIController.enabled)
			{
				return aIController;
			}
		}
		return null;
	}

	public static bool AreAttackersOnOppositeSidesOfTarget(GameObject attacker1, GameObject attacker2, GameObject enemy)
	{
		if (attacker1 == null)
		{
			return false;
		}
		if (attacker2 == null)
		{
			return false;
		}
		if (enemy == null)
		{
			return false;
		}
		Vector3 normalized = (enemy.transform.position - attacker1.transform.position).normalized;
		Vector3 normalized2 = (enemy.transform.position - attacker2.transform.position).normalized;
		if (Vector3.Dot(normalized, normalized2) < 0f)
		{
			return true;
		}
		return false;
	}

	public static float DistanceLineSegmentToPoint(Vector3 start, Vector3 end, Vector3 point)
	{
		float sqrMagnitude = (end - start).sqrMagnitude;
		if (sqrMagnitude == 0f)
		{
			return (point - start).magnitude;
		}
		float num = Vector3.Dot(point - start, end - start) / sqrMagnitude;
		if (num < 0f)
		{
			return (point - start).magnitude;
		}
		if (num > 1f)
		{
			return (point - end).magnitude;
		}
		Vector3 vector = start + num * (end - start);
		return (point - vector).magnitude;
	}

	public static void UpdateFadingEffects()
	{
		for (int num = s_fadingEffects.Count - 1; num >= 0; num--)
		{
			if (s_fadingEffects[num] == null)
			{
				s_fadingEffects.RemoveAt(num);
			}
		}
	}

	public static void ClearFadingEffects()
	{
		foreach (GameObject s_fadingEffect in s_fadingEffects)
		{
			if (s_fadingEffect != null)
			{
				DestroyImmediate(s_fadingEffect);
			}
		}
		s_fadingEffects.Clear();
	}

	public static void AddFadingEffect(GameObject effect)
	{
		if (effect != null)
		{
			s_fadingEffects.Add(effect);
		}
	}

	public static void LaunchEffect(GameObject effectBase, float scale, Vector3 position, GenericAbility abilityOrigin)
	{
		LaunchEffect(effectBase, position, Quaternion.identity, scale, null, abilityOrigin);
	}

	public static void LaunchEffect(GameObject effectBase, float scale, Vector3 position, Quaternion orientation, GenericAbility abilityOrigin)
	{
		LaunchEffect(effectBase, position, orientation, scale, null, abilityOrigin);
	}

	public static void LaunchEffect(GameObject effectBase, float scale, Transform parent, GenericAbility abilityOrigin)
	{
		if (!(parent == null))
		{
			LaunchEffect(effectBase, parent.position, parent.rotation, scale, parent, abilityOrigin);
		}
	}

	public static GameObject LaunchEffect(GameObject effectBase, Vector3 position, Quaternion orientation, float scale, Transform attachObj, GenericAbility abilityOrigin)
	{
		if (attachObj != null && attachObj.gameObject != null)
		{
			CharacterStats component = attachObj.gameObject.GetComponent<CharacterStats>();
			if (component != null && component.ImmuneToParticleEffects)
			{
				return null;
			}
		}
		if (abilityOrigin != null)
		{
			effectBase = abilityOrigin.CheckForReplacedParticleFX(effectBase);
		}
		if (effectBase != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(effectBase, position, orientation);
			Transform obj = gameObject.transform;
			obj.position = position;
			obj.localScale = Vector3.one * scale;
			obj.rotation = orientation;
			obj.parent = attachObj;
			Persistence component2 = gameObject.GetComponent<Persistence>();
			if ((bool)component2)
			{
				DestroyComponent(component2);
			}
			float num = float.MinValue;
			ParticleSystem[] componentsInChildren = gameObject.GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem obj2 in componentsInChildren)
			{
				ParticleSystem.EmissionModule emission = obj2.emission;
				emission.enabled = true;
				ParticleSystem.MainModule main = obj2.main;
				main.maxParticles = 400;
				if (main.duration + main.startDelay.constant > num)
				{
					num = main.duration + main.startDelay.constant;
				}
			}
			AudioSource[] componentsInChildren2 = gameObject.GetComponentsInChildren<AudioSource>();
			foreach (AudioSource audioSource in componentsInChildren2)
			{
				if (audioSource.clip != null)
				{
					if (!audioSource.playOnAwake && !audioSource.isPlaying)
					{
						GlobalAudioPlayer.Play(audioSource);
					}
					if (audioSource.clip.length > num)
					{
						num = audioSource.clip.length;
					}
				}
			}
			Destroy(gameObject, num);
			s_fadingEffects.Add(gameObject);
			return gameObject;
		}
		return null;
	}

	public static GameObject LaunchLoopingEffect(GameObject effectBase, Vector3 position, GenericAbility abilityOrigin)
	{
		return LaunchLoopingEffect(effectBase, position, Quaternion.identity, 1f, null, abilityOrigin);
	}

	public static GameObject LaunchLoopingEffect(GameObject effectBase, float scale, Transform parent, GenericAbility abilityOrigin)
	{
		return LaunchLoopingEffect(effectBase, parent.position, parent.rotation, scale, parent, abilityOrigin);
	}

	public static GameObject LaunchLoopingEffect(GameObject effectBase, Vector3 position, Quaternion orientation, float scale, Transform attachObj, GenericAbility abilityOrigin)
	{
		GameObject gameObject = null;
		if (attachObj != null)
		{
			CharacterStats component = attachObj.GetComponent<CharacterStats>();
			if (component != null && component.ImmuneToParticleEffects)
			{
				return gameObject;
			}
		}
		if (abilityOrigin != null)
		{
			effectBase = abilityOrigin.CheckForReplacedParticleFX(effectBase);
		}
		if (effectBase != null)
		{
			gameObject = UnityEngine.Object.Instantiate(effectBase, position, orientation);
			Transform obj = gameObject.transform;
			obj.position = position;
			obj.localScale = Vector3.one * scale;
			obj.rotation = orientation;
			obj.parent = attachObj;
			ParticleSystem[] componentsInChildren = gameObject.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				ParticleSystem.EmissionModule emission = componentsInChildren[i].emission;
				emission.enabled = true;
			}
		}
		return gameObject;
	}

	public static void ShutDownLoopingEffect(GameObject fx)
	{
		if (fx == null)
		{
			return;
		}
		List<ParticleSystem> list = new List<ParticleSystem>();
		list.AddRange(fx.GetComponents<ParticleSystem>());
		list.AddRange(fx.GetComponentsInChildren<ParticleSystem>());
		IgnoreParentRotation[] componentsInChildren = fx.GetComponentsInChildren<IgnoreParentRotation>();
		foreach (IgnoreParentRotation ignoreParentRotation in componentsInChildren)
		{
			if (ignoreParentRotation.AttachedChild != null)
			{
				list.AddRange(ignoreParentRotation.AttachedChild.GetComponentsInChildren<ParticleSystem>());
			}
		}
		foreach (ParticleSystem item in list)
		{
			ParticleSystem.EmissionModule emission = item.emission;
			emission.enabled = false;
		}
	}

	public static void RestartLoopingEffect(GameObject fx)
	{
		if (fx == null)
		{
			return;
		}
		List<ParticleSystem> list = new List<ParticleSystem>();
		list.AddRange(fx.GetComponents<ParticleSystem>());
		list.AddRange(fx.GetComponentsInChildren<ParticleSystem>());
		IgnoreParentRotation[] componentsInChildren = fx.GetComponentsInChildren<IgnoreParentRotation>();
		foreach (IgnoreParentRotation ignoreParentRotation in componentsInChildren)
		{
			if (ignoreParentRotation.AttachedChild != null)
			{
				list.AddRange(ignoreParentRotation.AttachedChild.GetComponentsInChildren<ParticleSystem>());
			}
		}
		foreach (ParticleSystem item in list)
		{
			ParticleSystem.EmissionModule emission = item.emission;
			emission.enabled = true;
		}
	}

	public static void CheckForExpansions()
	{
		ProductConfiguration.ActivePackage = ProductConfiguration.Package.BaseGame;
		if (File.Exists(Path.Combine(Application.dataPath, GameResources.PX1Path)))
		{
			ProductConfiguration.ActivePackage |= ProductConfiguration.Package.Expansion1;
		}
		if (File.Exists(Path.Combine(Application.dataPath, GameResources.PX2Path)))
		{
			ProductConfiguration.ActivePackage |= ProductConfiguration.Package.Expansion2;
		}
		if (File.Exists(Path.Combine(Application.dataPath, GameResources.PX4Path)))
		{
			ProductConfiguration.ActivePackage |= ProductConfiguration.Package.Expansion4;
		}
	}

	public static bool HasPX1()
	{
		if ((ProductConfiguration.ActivePackage & ProductConfiguration.Package.Expansion1) == ProductConfiguration.Package.Expansion1)
		{
			return true;
		}
		return false;
	}

	public static bool HasPX2()
	{
		if (HasPX1() && (ProductConfiguration.ActivePackage & ProductConfiguration.Package.Expansion2) == ProductConfiguration.Package.Expansion2)
		{
			return true;
		}
		return false;
	}

	public static bool HasPX4()
	{
		return HasPackage(ProductConfiguration.Package.Expansion4);
	}

	public static bool HasPackage(ProductConfiguration.Package package)
	{
		return (ProductConfiguration.ActivePackage & package) == package;
	}

	public static void CreateGlobalPrefabObject()
	{
		Debug.Log("Checking to instantiate global...");
		if (m_defaultControllerAssetBundle == null)
		{
			m_defaultControllerAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath, "assetbundles/prefabs/objectbundle/default_controller"));
		}
		if (m_defaultShaderAssetBundle == null)
		{
			m_defaultShaderAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath, "assetbundles/prefabs/objectbundle/shaders"));
		}
		if (m_defaultVFXAssetBundle == null)
		{
			m_defaultVFXAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath, "assetbundles/prefabs/objectbundle/common_vfx"));
		}
		if (GameCursor.Instance == null)
		{
			Debug.Log("Instantiating global.");
			CheckForExpansions();
			GameObject obj = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Global", typeof(GameObject)));
			obj.layer = LayerMask.NameToLayer("GUILayer");
			Persistence component = obj.GetComponent<Persistence>();
			if ((bool)component)
			{
				component.GlobalObject = true;
				component.UnloadsBetweenLevels = false;
			}
			GameState.PersistAcrossSceneLoadsUntracked(obj);
		}
	}

	public static void CreateInGameGlobalPrefabObject()
	{
		Time.timeScale = 0f;
		CreateGlobalPrefabObject();
		Debug.Log("Checking to instantiate in game global...");
		if (s_instance == null)
		{
			Debug.Log("Instantiating in game global.");
			GameObject obj = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("Prefabs/InGameGlobal", typeof(GameObject)));
			obj.layer = LayerMask.NameToLayer("GUILayer");
			Persistence component = obj.GetComponent<Persistence>();
			if ((bool)component)
			{
				component.GlobalObject = true;
				component.UnloadsBetweenLevels = false;
			}
			GameState.PersistAcrossSceneLoadsTracked(obj);
			obj.SetActive(value: true);
		}
	}

	public static void RecursiveSetLayer(GameObject obj, int layer)
	{
		obj.layer = layer;
		for (int i = 0; i < obj.transform.childCount; i++)
		{
			RecursiveSetLayer(obj.transform.GetChild(i).gameObject, layer);
		}
	}

	public static SkinnedMeshRenderer FindSkinnedMeshRenderer(GameObject obj)
	{
		SkinnedMeshRenderer result = null;
		if (obj != null)
		{
			result = obj.GetComponentInChildren<SkinnedMeshRenderer>();
		}
		return result;
	}

	public static SkinnedMeshRenderer[] FindSkinnedMeshRenderers(GameObject obj)
	{
		if (obj != null)
		{
			return obj.GetComponentsInChildren<SkinnedMeshRenderer>();
		}
		return null;
	}

	public static GameObject FindSkeleton(GameObject obj)
	{
		if (obj.CompareTag("Skeleton"))
		{
			return obj;
		}
		for (int i = 0; i < obj.transform.childCount; i++)
		{
			GameObject gameObject = FindSkeleton(obj.transform.GetChild(i).gameObject);
			if (gameObject != null)
			{
				return gameObject;
			}
		}
		return null;
	}

	public static Transform FindSkeletonTransform(GameObject obj)
	{
		GameObject gameObject = FindSkeleton(obj);
		if (!gameObject)
		{
			return null;
		}
		return gameObject.transform;
	}

	public static Animator FindAnimator(GameObject obj)
	{
		Animator result = null;
		if (obj != null)
		{
			result = obj.GetComponent<Animator>();
		}
		return result;
	}

	public static void SetAnimator(GameObject obj, Animator anim)
	{
		if (obj != null)
		{
			AnimationController component = obj.GetComponent<AnimationController>();
			if (component != null)
			{
				component.CurrentAvatar = anim;
			}
		}
	}

	public static void FastForwardAnimator(Animator animator, int seconds)
	{
		if ((bool)animator)
		{
			for (int i = 0; i < seconds; i++)
			{
				animator.Update(1f);
			}
		}
	}

	public static GameObject FindParentWithComponent<T>(GameObject child) where T : MonoBehaviour
	{
		if (!child)
		{
			return null;
		}
		Transform parent = child.transform;
		while ((bool)parent)
		{
			if ((bool)(UnityEngine.Object)parent.GetComponent<T>())
			{
				return parent.gameObject;
			}
			parent = parent.parent;
		}
		return null;
	}

	public static bool DoesMouseIntersect(GameObject obj)
	{
		if (!obj)
		{
			return false;
		}
		Ray ray = Camera.main.ScreenPointToRay(GameInput.MousePosition);
		Collider[] components = obj.transform.GetComponents<Collider>();
		if (components.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < components.Length; i++)
		{
			if (components[i].Raycast(ray, out var _, float.MaxValue))
			{
				return true;
			}
		}
		return false;
	}

	public static float ObjectDistance2D(GameObject a, GameObject b)
	{
		if (a == null || b == null)
		{
			return float.MaxValue;
		}
		float num = V3Distance2D(a.transform.position, b.transform.position);
		Mover component = a.GetComponent<Mover>();
		if (component != null)
		{
			num -= component.Radius;
		}
		Mover component2 = b.GetComponent<Mover>();
		if (component2 != null)
		{
			num -= component2.Radius;
		}
		if (num < 0f)
		{
			num = 0f;
		}
		return num;
	}

	public static float V3Distance2D(Vector3 a, Vector3 b)
	{
		return Mathf.Sqrt(V3SqrDistance2D(a, b));
	}

	public static float V3SqrDistance2D(Vector3 a, Vector3 b)
	{
		float num = b.x - a.x;
		float num2 = b.z - a.z;
		return num * num + num2 * num2;
	}

	public static float V3SqrDistance(Vector3 a, Vector3 b)
	{
		float num = b.x - a.x;
		float num2 = b.y - a.y;
		float num3 = b.z - a.z;
		return num * num + num2 * num2 + num3 * num3;
	}

	public static Vector2 V3Subtract2D(Vector3 a, Vector3 b)
	{
		return new Vector2(a.x - b.x, a.z - b.z);
	}

	public static float V3Dot2D(Vector3 a, Vector3 b)
	{
		return a.x * b.x + (a.z + b.z);
	}

	public static Vector3 V2ToV3(Vector2 a)
	{
		return new Vector3(a.x, 0f, a.y);
	}

	public static Vector2 V3ToV2(Vector3 a)
	{
		return new Vector2(a.x, a.z);
	}

	public static bool IsPositionOnNavMesh(Vector3 position)
	{
		if (NavMesh.SamplePosition(position, out var hit, 0.5f, -1) && V3SqrDistance2D(position, hit.position) < 0.001f)
		{
			return true;
		}
		return false;
	}

	public static Texture2D ResizeTexture(Texture2D tex, int width, int height)
	{
		Texture2D texture2D = new Texture2D(width, height);
		Texture2D texture2D2 = UnityEngine.Object.Instantiate(tex, Vector3.zero, Quaternion.identity);
		Color color = default(Color);
		Color color2 = default(Color);
		Color color3 = default(Color);
		Color color4 = default(Color);
		float num = (float)texture2D2.width / (float)width;
		float num2 = (float)texture2D2.height / (float)height;
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				int num3 = (int)Mathf.Floor((float)i * num);
				int num4 = (int)Mathf.Floor((float)j * num2);
				int num5 = num3 + 1;
				if (num5 >= texture2D2.width)
				{
					num5 = num3;
				}
				int num6 = num4 + 1;
				if (num6 >= texture2D2.height)
				{
					num6 = num4;
				}
				float num7 = (float)i * num - (float)num3;
				float num8 = (float)j * num2 - (float)num4;
				float num9 = 1f - num7;
				float num10 = 1f - num8;
				color = texture2D2.GetPixel(num3, num4);
				color2 = texture2D2.GetPixel(num5, num4);
				color3 = texture2D2.GetPixel(num3, num6);
				color4 = texture2D2.GetPixel(num5, num6);
				float num11 = num9 * color.b + num7 * color2.b;
				float num12 = num9 * color3.b + num7 * color4.b;
				float b = num10 * num11 + num8 * num12;
				num11 = num9 * color.g + num7 * color2.g;
				num12 = num9 * color3.g + num7 * color4.g;
				float g = num10 * num11 + num8 * num12;
				num11 = num9 * color.r + num7 * color2.r;
				num12 = num9 * color3.r + num7 * color4.r;
				float r = num10 * num11 + num8 * num12;
				texture2D.SetPixel(i, j, new Color(r, g, b));
			}
		}
		texture2D.Apply();
		Destroy(texture2D2);
		return texture2D;
	}

	public static void LogProgrammingDebug(string filename)
	{
	}

	public static string GetResolutionDebugText()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("-- Resolution Debug --");
		stringBuilder.AppendLine("Screen: Width = " + Screen.width + ", Height = " + Screen.height);
		stringBuilder.AppendLine("Screen.Resolution: Width = " + Screen.currentResolution.width + ", Height = " + Screen.currentResolution.height);
		if ((bool)InGameUILayout.Root)
		{
			stringBuilder.AppendLine("Root Scale (x100000): " + InGameUILayout.Root.transform.localScale * 100000f);
			stringBuilder.AppendLine("Root Height: Active = " + InGameUILayout.Root.activeHeight + ", Max = " + InGameUILayout.Root.maximumHeight);
			stringBuilder.AppendLine("Root Pixel Size = " + InGameUILayout.Root.pixelSizeAdjustment);
		}
		if ((bool)UIDynamicFontManager.Instance)
		{
			stringBuilder.AppendLine("Ngui Text Scale = " + UIDynamicFontManager.Instance.NguiAdjustment);
		}
		return stringBuilder.ToString();
	}

	private void DrawResolutionDebug()
	{
		UIDebug.Instance.SetText("Resolution Debug", GetResolutionDebugText(), Color.cyan);
		UIDebug.Instance.SetTextPosition("Resolution Debug", 0.95f, 0.95f, UIWidget.Pivot.TopRight);
	}

	public static AssetBundle GetDefaultControllerAssetBundle()
	{
		return m_defaultControllerAssetBundle;
	}

	public static string GetName(UnityEngine.Object go)
	{
		if ((bool)go)
		{
			return go.name;
		}
		return "*null*";
	}

	public static string GetDisplayName(GameObject go)
	{
		GenericAbility component = go.GetComponent<GenericAbility>();
		if ((bool)component)
		{
			return component.Name();
		}
		Item component2 = go.GetComponent<Item>();
		if ((bool)component2)
		{
			return component2.Name;
		}
		CharacterStats component3 = go.GetComponent<CharacterStats>();
		if ((bool)component3)
		{
			return component3.Name();
		}
		return "*NameError*";
	}

	public static bool GetBarkStringPersistsOnCombatStart(FlowChartNode node)
	{
		if (node == null)
		{
			return false;
		}
		string extendedPropertyValue = node.ClassExtender.GetExtendedPropertyValue("PersistOnCombatStart");
		bool result = false;
		if (bool.TryParse(extendedPropertyValue, out result))
		{
			return result;
		}
		return false;
	}

	public static bool IsOctoberHoliday()
	{
		if (DateTime.Today.Month == 10 && (DateTime.Today.Day == 31 || DateTime.Today.Day == 30))
		{
			return BigHeads.Enabled;
		}
		return false;
	}

	public static CapeColliderData GetCapeColliderData(CapeType capeType)
	{
		CapeColliderData result = default(CapeColliderData);
		result.radius = 0f;
		result.height = 0f;
		result.center = new Vector3(0f, 0f, 0f);
		result.directionIsYAxis = true;
		switch (capeType)
		{
		case CapeType.F_AUM:
			result.radius = 0.4f;
			result.height = 2f;
			result.center = new Vector3(0f, 0.5f, 0.25f);
			break;
		case CapeType.F_HUM:
			result.radius = 0.4f;
			result.height = 1.6f;
			result.center = new Vector3(0f, 0.65f, 0.2f);
			break;
		case CapeType.F_ORL:
			result.radius = 0.4f;
			result.height = 1.5f;
			result.center = new Vector3(0f, 0.3f, 0.23f);
			break;
		case CapeType.M_AUM:
			result.radius = 0.45f;
			result.height = 2f;
			result.center = new Vector3(0f, 0.75f, 0.2f);
			break;
		case CapeType.M_HUM:
			result.radius = 0.4f;
			result.height = 1.796f;
			result.center = new Vector3(0f, 0.6f, 0.25f);
			break;
		case CapeType.M_ORL:
			result.radius = 0.4f;
			result.height = 1.5f;
			result.center = new Vector3(0f, 0.3f, 0.2f);
			break;
		default:
			Debug.LogError("Invalid value passed into GetCapeColliderData");
			break;
		}
		return result;
	}
}
