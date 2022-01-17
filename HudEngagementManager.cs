using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HudEngagementManager : MonoBehaviour
{
	private class GameObjectPair
	{
		public GameObject source;

		public GameObject target;

		public GameObjectPair Reverse => new GameObjectPair(target, source);

		public GameObjectPair(GameObject source, GameObject target)
		{
			this.source = source;
			this.target = target;
		}

		public override bool Equals(object obj)
		{
			if (obj is GameObjectPair gameObjectPair)
			{
				if (source == gameObjectPair.source)
				{
					return target == gameObjectPair.target;
				}
				return false;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return source.GetHashCode() + 13 * target.GetHashCode();
		}
	}

	private HudEngagementIndicator m_Prefab;

	public float ArrowRotSpeed = 150f;

	public float ArrowElevation;

	public float ArrowScaleY = 0.6f;

	public float ArrowMaxRange = 3f;

	public float TwoWayOffset = 0.09f;

	public float FlankAlpha = 0.8f;

	private Dictionary<GameObjectPair, HudEngagementIndicator> m_ActiveIndicators = new Dictionary<GameObjectPair, HudEngagementIndicator>();

	private Stack<HudEngagementIndicator> m_PooledIndicators = new Stack<HudEngagementIndicator>();

	private List<GameObjectPair> killList = new List<GameObjectPair>();

	public static HudEngagementManager Instance { get; private set; }

	public Shader Shader { get; private set; }

	private void Awake()
	{
		Instance = this;
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		m_Prefab = null;
		Shader = null;
		m_ActiveIndicators.Clear();
		m_PooledIndicators.Clear();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnLoadSceneCallback;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnLoadSceneCallback;
	}

	private void OnLoadSceneCallback(Scene scene, LoadSceneMode sceneMode)
	{
		m_ActiveIndicators.Clear();
		m_PooledIndicators.Clear();
	}

	private void Init()
	{
		if (!m_Prefab)
		{
			m_Prefab = Resources.Load<GameObject>("Prefabs/Graphics/EngagementIndicator").GetComponent<HudEngagementIndicator>();
			Shader = Shader.Find("Trenton/UI/PE_InGameArrow");
		}
	}

	public void Verify(GameObject source, GameObject target)
	{
		if (!source || (source.GetComponent<Faction>().ShowSelectionCircle(elevate: false) && base.enabled))
		{
			GameObjectPair key = new GameObjectPair(source, target);
			if (!m_ActiveIndicators.ContainsKey(key))
			{
				HudEngagementIndicator newIndicator = GetNewIndicator();
				newIndicator.gameObject.SetActive(value: true);
				newIndicator.SetTargets(source, target);
				m_ActiveIndicators[key] = newIndicator;
			}
			m_ActiveIndicators[key].Verified = true;
		}
	}

	private void LateUpdate()
	{
		killList.Clear();
		Dictionary<GameObjectPair, HudEngagementIndicator>.Enumerator enumerator = m_ActiveIndicators.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				if (!enumerator.Current.Value.Verified)
				{
					killList.Add(enumerator.Current.Key);
					continue;
				}
				enumerator.Current.Value.TwoWay = m_ActiveIndicators.ContainsKey(enumerator.Current.Key.Reverse);
				enumerator.Current.Value.Verified = false;
			}
		}
		finally
		{
			enumerator.Dispose();
		}
		for (int i = 0; i < killList.Count; i++)
		{
			GameObjectPair key = killList[i];
			m_PooledIndicators.Push(m_ActiveIndicators[key]);
			m_ActiveIndicators[key].gameObject.SetActive(value: false);
			m_ActiveIndicators.Remove(key);
		}
	}

	private HudEngagementIndicator GetNewIndicator()
	{
		Init();
		if (m_PooledIndicators.Count > 0)
		{
			return m_PooledIndicators.Pop();
		}
		HudEngagementIndicator component = Object.Instantiate(m_Prefab.gameObject).GetComponent<HudEngagementIndicator>();
		component.transform.parent = base.transform;
		return component;
	}
}
