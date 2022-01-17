using System.Collections.Generic;
using UnityEngine;

public class CullEnvironmentalVfx : MonoBehaviour
{
	private static List<CullEnvironmentalVfx> m_Instances = new List<CullEnvironmentalVfx>();

	private ParticleSystem[] m_ChildParticleSystems;

	private bool m_ChildrenEnabled = true;

	[Tooltip("The approximate size of the particle effect.")]
	public float Radius = 10f;

	private void Awake()
	{
		m_Instances.Add(this);
	}

	private void Start()
	{
		m_ChildParticleSystems = GetComponentsInChildren<ParticleSystem>();
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
		m_Instances.Remove(this);
	}

	private void Update()
	{
		Vector2 vector = Camera.main.WorldToViewportPoint(base.transform.position);
		float num = ((Vector2)Camera.main.WorldToViewportPoint(base.transform.position + new Vector3(Radius, 0f, 0f)) - vector).magnitude + 0.707107f;
		float sqrMagnitude = (new Vector2(0.5f, 0.5f) - vector).sqrMagnitude;
		SetActive(sqrMagnitude <= num * num);
	}

	public void SetActive(bool state)
	{
		if (state == m_ChildrenEnabled)
		{
			return;
		}
		for (int i = 0; i < m_ChildParticleSystems.Length; i++)
		{
			if ((bool)m_ChildParticleSystems[i])
			{
				m_ChildParticleSystems[i].gameObject.SetActive(state);
			}
		}
		m_ChildrenEnabled = state;
	}

	public static void EnableAll()
	{
		for (int i = 0; i < m_Instances.Count; i++)
		{
			m_Instances[i].SetActive(state: true);
		}
	}
}
