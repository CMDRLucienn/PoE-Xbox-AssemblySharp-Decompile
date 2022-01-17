using System;
using System.Collections.Generic;
using UnityEngine;

public class Stealth : MonoBehaviour
{
	[Serializable]
	public class DetectionObject
	{
		public Guid m_obj;

		public float m_time;

		public Guid Object
		{
			get
			{
				return m_obj;
			}
			set
			{
				m_obj = value;
			}
		}

		public float Time
		{
			get
			{
				return m_time;
			}
			set
			{
				m_time = value;
			}
		}

		public DetectionObject()
		{
		}

		public DetectionObject(Guid obj, float time)
		{
			m_obj = obj;
			m_time = time;
		}
	}

	private static float s_stealthVOCooldown = 0f;

	[Persistent]
	private bool m_inStealth;

	private float m_spottingTimer;

	private static Dictionary<GameObject, Stealth> s_activeStealthComponents = new Dictionary<GameObject, Stealth>();

	private static List<Stealth> s_componentsWithStealthToggledOnThisFrame = new List<Stealth>();

	private static HashSet<GameObject> s_EnemiesSpottedInStealth = new HashSet<GameObject>();

	private List<GameObject> m_enemyObjectsDetected = new List<GameObject>();

	[Persistent]
	private List<DetectionObject> m_detectingMe = new List<DetectionObject>(5);

	[Persistent]
	private float m_suspicionDecayTimer;

	public const int INVESTIGATION_THRESHOLD = 100;

	public const int FULLY_DETECTED_THRESHOLD = 200;

	public float HighestSuspicion
	{
		get
		{
			float num = 0f;
			foreach (DetectionObject item in m_detectingMe)
			{
				if (item.m_time > num)
				{
					num = item.m_time;
				}
			}
			return num;
		}
	}

	public bool IsBeingDetected => m_suspicionDecayTimer > 0f;

	public static event EventHandler GlobalOnAnyStealthStateChanged;

	public event EventHandler OnStealthStateChanged;

	public static event EventHandler OnDetected;

	public static bool IsInStealthMode(GameObject gameObject)
	{
		Stealth value = null;
		if (s_activeStealthComponents.TryGetValue(gameObject, out value) && (bool)value)
		{
			return value.IsInStealthMode();
		}
		return false;
	}

	public static Stealth GetStealthComponent(GameObject gameObject)
	{
		if (!gameObject)
		{
			return null;
		}
		Stealth value = null;
		s_activeStealthComponents.TryGetValue(gameObject, out value);
		return value;
	}

	public static bool AnyStealthInStealthMode()
	{
		Dictionary<GameObject, Stealth>.Enumerator enumerator = s_activeStealthComponents.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.Value != null && enumerator.Current.Value.IsInStealthMode())
				{
					return true;
				}
			}
		}
		finally
		{
			enumerator.Dispose();
		}
		return false;
	}

	public static void GlobalSetInStealthMode(bool inStealth)
	{
		foreach (KeyValuePair<GameObject, Stealth> s_activeStealthComponent in s_activeStealthComponents)
		{
			if (!(s_activeStealthComponent.Value == null))
			{
				if (inStealth)
				{
					s_activeStealthComponent.Value.ActivateStealth();
				}
				else
				{
					s_activeStealthComponent.Value.DeactivateStealth();
				}
			}
		}
	}

	public static void SetInStealthMode(GameObject gameObject, bool inStealth)
	{
		if (!gameObject)
		{
			return;
		}
		Stealth stealthComponent = GetStealthComponent(gameObject);
		if ((bool)stealthComponent)
		{
			if (inStealth)
			{
				stealthComponent.ActivateStealth();
			}
			else
			{
				stealthComponent.DeactivateStealth();
			}
		}
	}

	public void AddSuspicion(GameObject key, float value, Faction.Relationship relationship)
	{
		AddSuspicion(key.GetComponent<InstanceID>().Guid, value, relationship);
	}

	private bool HasSuspicion(Guid key)
	{
		foreach (DetectionObject item in m_detectingMe)
		{
			if (item.m_obj == key)
			{
				return true;
			}
		}
		return false;
	}

	public void AddSuspicion(Guid key, float value, Faction.Relationship relationship)
	{
		if (!HasSuspicion(key))
		{
			DetectionObject item = new DetectionObject(key, 0f);
			m_detectingMe.Add(item);
		}
		for (int i = 0; i < m_detectingMe.Count; i++)
		{
			if (!(m_detectingMe[i].m_obj == key))
			{
				continue;
			}
			m_detectingMe[i].m_time = Mathf.Min(m_detectingMe[i].m_time + value, 200f);
			if (m_detectingMe[i].m_time >= 200f && relationship == Faction.Relationship.Hostile)
			{
				SetInStealthMode(base.gameObject, inStealth: false);
				if (Stealth.OnDetected != null)
				{
					Stealth.OnDetected(this, EventArgs.Empty);
				}
				GlobalAudioPlayer.SPlay(UIAudioList.UIAudioType.StealthForceBreak);
			}
			m_suspicionDecayTimer = AttackData.Instance.StealthDecayDelay;
			break;
		}
	}

	public void ClearAllSuspicion()
	{
		if (m_detectingMe != null)
		{
			m_detectingMe.Clear();
		}
		m_suspicionDecayTimer = 0f;
	}

	public float GetSuspicion(GameObject key)
	{
		return GetSuspicion(key.GetComponent<InstanceID>().Guid);
	}

	public float GetSuspicion(Guid key)
	{
		foreach (DetectionObject item in m_detectingMe)
		{
			if (item.m_obj == key)
			{
				return item.m_time;
			}
		}
		return 0f;
	}

	private bool CanActivateStealth()
	{
		return !GameState.InCombat;
	}

	private void SetSneakAnimation()
	{
		AnimationController component = GetComponent<AnimationController>();
		if ((bool)component)
		{
			component.Sneak = IsInStealthMode(base.gameObject);
		}
	}

	private void LevelLoaded(object sender, EventArgs e)
	{
		SetSneakAnimation();
	}

	public void Refresh()
	{
		SetSneakAnimation();
	}

	private void HandleOnStealthChangedAudio()
	{
		if (m_inStealth)
		{
			GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.StealthOn);
		}
		else
		{
			GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.StealthOff);
		}
	}

	private void HandleStealthStateChanged()
	{
		SetSneakAnimation();
		if (m_inStealth)
		{
			GameObject[] array = GameUtilities.CreaturesInRange(base.transform.position, 11f, playerEnemiesOnly: false, includeUnconscious: false);
			foreach (GameObject gameObject in array)
			{
				Faction component = gameObject.GetComponent<Faction>();
				if (!((gameObject.transform.position - base.transform.position).sqrMagnitude > 121f))
				{
					AIController component2 = gameObject.GetComponent<AIController>();
					if (!(component2 == null) && component2.m_detectsStealthedCharacters && component.GetRelationship(base.gameObject) == Faction.Relationship.Neutral && GameUtilities.LineofSight(gameObject.transform.position, base.gameObject, 1f))
					{
						AddSuspicion(gameObject, 200f, Faction.Relationship.Neutral);
					}
				}
			}
			if (s_componentsWithStealthToggledOnThisFrame != null)
			{
				s_componentsWithStealthToggledOnThisFrame.Add(this);
			}
		}
		HandleOnStealthChangedAudio();
	}

	private void ActivateStealth()
	{
		if (!CanActivateStealth())
		{
			UISystemMessager.Instance.PostMessage(GUIUtils.GetText(2366), Color.red);
			return;
		}
		bool inStealth = m_inStealth;
		m_inStealth = true;
		if (m_inStealth != inStealth)
		{
			HandleStealthStateChanged();
			if (this.OnStealthStateChanged != null)
			{
				this.OnStealthStateChanged(m_inStealth, null);
			}
			if (Stealth.GlobalOnAnyStealthStateChanged != null)
			{
				Stealth.GlobalOnAnyStealthStateChanged(m_inStealth, null);
			}
		}
	}

	private void DeactivateStealth()
	{
		bool inStealth = m_inStealth;
		m_inStealth = false;
		if (m_inStealth != inStealth)
		{
			HandleStealthStateChanged();
			if (this.OnStealthStateChanged != null)
			{
				this.OnStealthStateChanged(m_inStealth, null);
			}
			if (Stealth.GlobalOnAnyStealthStateChanged != null)
			{
				Stealth.GlobalOnAnyStealthStateChanged(m_inStealth, null);
			}
		}
	}

	public bool IsInStealthMode()
	{
		return m_inStealth;
	}

	private void OnUnconscious(GameObject myObject, GameEventArgs args)
	{
		DeactivateStealth();
	}

	private void Awake()
	{
		s_activeStealthComponents.Add(base.gameObject, this);
		if ((bool)UIStealthIndicatorManager.Instance)
		{
			UIStealthIndicatorManager.Instance.AddIndicator(this);
		}
	}

	private void Start()
	{
		GameState.OnLevelLoaded += LevelLoaded;
		Health component = GetComponent<Health>();
		if ((bool)component)
		{
			component.OnDeath += OnUnconscious;
			component.OnUnconscious += OnUnconscious;
		}
	}

	private void OnEnable()
	{
		HandleStealthStateChanged();
	}

	private void OnDestroy()
	{
		s_activeStealthComponents.Remove(base.gameObject);
		GameState.OnLevelLoaded -= LevelLoaded;
		if ((bool)UIStealthIndicatorManager.Instance)
		{
			UIStealthIndicatorManager.Instance.RemoveIndicator(this);
		}
		Health component = GetComponent<Health>();
		if ((bool)component)
		{
			component.OnUnconscious -= OnUnconscious;
			component.OnDeath -= OnUnconscious;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void Restored()
	{
		if (StoredCharacterInfo.RestoringPackedCharacter)
		{
			m_inStealth = false;
		}
	}

	private void HandleEnemySpotting()
	{
		if (m_spottingTimer > 0f)
		{
			m_spottingTimer -= Time.deltaTime;
		}
		if (!(m_spottingTimer <= 0f))
		{
			return;
		}
		if (m_inStealth)
		{
			GameUtilities.GetEnemiesInRange(base.gameObject, GetComponent<PartyMemberAI>(), 13f, m_enemyObjectsDetected);
			if (m_enemyObjectsDetected != null && m_enemyObjectsDetected.Count > 0)
			{
				for (int i = 0; i < m_enemyObjectsDetected.Count; i++)
				{
					if (!s_EnemiesSpottedInStealth.Contains(m_enemyObjectsDetected[i]))
					{
						SoundSet.TryPlayVoiceEffectWithLocalCooldown(base.gameObject, SoundSet.SoundAction.EnemySpottedInStealth, SoundSet.s_LongVODelay, forceInterrupt: false);
						s_EnemiesSpottedInStealth.Add(m_enemyObjectsDetected[i]);
					}
				}
			}
			m_enemyObjectsDetected.Clear();
		}
		m_spottingTimer = 1f;
	}

	private void HandleSuspicion()
	{
		if (m_suspicionDecayTimer > 0f)
		{
			m_suspicionDecayTimer -= Time.deltaTime;
		}
		if (!(m_suspicionDecayTimer <= 0f))
		{
			return;
		}
		for (int num = m_detectingMe.Count - 1; num >= 0; num--)
		{
			m_detectingMe[num].m_time -= (float)AttackData.Instance.StealthDecayRate * Time.deltaTime;
			if (m_detectingMe[num].m_time <= 0f)
			{
				m_detectingMe.RemoveAt(num);
			}
		}
	}

	private void Update()
	{
		HandleEnemySpotting();
		HandleSuspicion();
	}

	public static void UpdateStaticLogic()
	{
		if (!AnyStealthInStealthMode())
		{
			s_EnemiesSpottedInStealth.Clear();
		}
		if (s_stealthVOCooldown > 0f)
		{
			s_stealthVOCooldown -= Time.deltaTime;
		}
		if (s_stealthVOCooldown <= 0f)
		{
			while (s_componentsWithStealthToggledOnThisFrame != null && s_componentsWithStealthToggledOnThisFrame.Count > 0)
			{
				int index = OEIRandom.Index(s_componentsWithStealthToggledOnThisFrame.Count);
				Stealth stealth = s_componentsWithStealthToggledOnThisFrame[index];
				SoundSetComponent soundSetComponent = (stealth ? stealth.GetComponent<SoundSetComponent>() : null);
				if ((bool)soundSetComponent && SoundSet.TryPlayVoiceEffectWithLocalCooldown(soundSetComponent.gameObject, SoundSet.SoundAction.Scouting, 5f, forceInterrupt: false))
				{
					s_stealthVOCooldown = 5f;
					break;
				}
				s_componentsWithStealthToggledOnThisFrame.RemoveAt(index);
			}
		}
		if (s_componentsWithStealthToggledOnThisFrame != null)
		{
			s_componentsWithStealthToggledOnThisFrame.Clear();
		}
	}
}
