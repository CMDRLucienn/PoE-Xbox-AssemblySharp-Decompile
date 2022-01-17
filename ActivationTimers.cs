using System;
using System.Collections.Generic;
using UnityEngine;

public class ActivationTimers : MonoBehaviour
{
	[Serializable]
	public class Timer
	{
		public GameObject Target;

		public float Delay { get; private set; }

		public bool State { get; private set; }

		public Guid TargetGuid { get; private set; }

		public bool Dead => Delay <= 0f;

		public Timer()
			: this(0f, state: true, null)
		{
		}

		public Timer(float delay, bool state, GameObject target)
		{
			Delay = delay;
			State = state;
			Target = target;
			if (!(Target == null))
			{
				InstanceID component = target.GetComponent<InstanceID>();
				if (!component)
				{
					throw new ArgumentException("Timer target must have an InstanceId.", "target");
				}
				TargetGuid = component.Guid;
			}
		}

		public void Update()
		{
			if (!Dead)
			{
				Delay -= Time.deltaTime;
				if (Delay <= 0f)
				{
					Scripts.ActivateObjectHelper(TargetGuid, null, State);
				}
			}
		}

		public void Restored()
		{
			Target = InstanceID.GetObjectByID(TargetGuid);
		}
	}

	[Persistent]
	private List<Timer> m_Timers = new List<Timer>();

	private const float DEFAULT_SPAWN_TIME = 0.3f;

	public static ActivationTimers Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'ActivationTimers' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	private void Update()
	{
		for (int num = m_Timers.Count - 1; num >= 0; num--)
		{
			m_Timers[num].Update();
			if (m_Timers[num].Dead)
			{
				m_Timers.RemoveAt(num);
			}
		}
	}

	public void Restored()
	{
		for (int i = 0; i < m_Timers.Count; i++)
		{
			m_Timers[i].Restored();
		}
	}

	public void StartTimer(GameObject target, GameObject vfx, bool state)
	{
		VfxData component = vfx.GetComponent<VfxData>();
		StartTimer(target, component ? component.SpawnTime : 0.3f, state);
	}

	public void StartTimer(GameObject target, float delay, bool state)
	{
		m_Timers.Add(new Timer(delay, state, target));
	}

	public static bool ActivateWithVfx(GameObject target, string vfxPrefab, bool state)
	{
		GameObject vfxPrefab2 = null;
		if (!string.IsNullOrEmpty(vfxPrefab))
		{
			vfxPrefab2 = GameResources.LoadPrefab<GameObject>(vfxPrefab, instantiate: false);
		}
		return ActivateWithVfx(target, vfxPrefab2, state);
	}

	public static bool ActivateWithVfx(GameObject target, GameObject vfxPrefab, bool state)
	{
		if ((bool)vfxPrefab)
		{
			GameUtilities.LaunchEffect(vfxPrefab, 1f, target.transform.position, target.transform.rotation, null);
			if (Instance == null)
			{
				throw new InvalidOperationException("ActivationTimers component not found.");
			}
			Instance.StartTimer(target, vfxPrefab, state);
			return false;
		}
		target.SetActive(state);
		return true;
	}
}
