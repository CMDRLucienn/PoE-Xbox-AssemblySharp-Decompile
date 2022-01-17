using System.Collections.Generic;
using UnityEngine;

public class UIParticleSystem : MonoBehaviour
{
	private List<ParticleSystem> m_Particles;

	private UIPanel m_ParentPanel;

	private float m_prevPanelAlpha;

	private bool m_childrenActive;

	private void Awake()
	{
		m_Particles = new List<ParticleSystem>();
		m_Particles.AddRange(GetComponentsInChildren<ParticleSystem>());
		IgnoreParentRotation[] componentsInChildren = GetComponentsInChildren<IgnoreParentRotation>();
		foreach (IgnoreParentRotation ignoreParentRotation in componentsInChildren)
		{
			if (ignoreParentRotation.AttachedChild != null)
			{
				m_Particles.AddRange(ignoreParentRotation.AttachedChild.GetComponentsInChildren<ParticleSystem>());
			}
		}
		m_childrenActive = true;
	}

	private void Start()
	{
		m_ParentPanel = GetComponentInParent<UIPanel>();
	}

	private void Update()
	{
		if (Time.timeScale == 0f && m_Particles != null)
		{
			foreach (ParticleSystem particle in m_Particles)
			{
				if (particle != null)
				{
					bool isPaused = particle.isPaused;
					if (particle.emission.enabled)
					{
						particle.Simulate(Time.unscaledDeltaTime, withChildren: false, restart: false);
					}
					if (!isPaused)
					{
						particle.Play(withChildren: false);
					}
				}
			}
		}
		if (!m_ParentPanel)
		{
			return;
		}
		if (m_ParentPanel.alpha != m_prevPanelAlpha)
		{
			if (m_childrenActive)
			{
				SetChildrenActive(active: false);
			}
		}
		else
		{
			bool flag = m_prevPanelAlpha > 0f;
			if (m_childrenActive != flag)
			{
				SetChildrenActive(flag);
			}
		}
		m_prevPanelAlpha = m_ParentPanel.alpha;
	}

	private void SetChildrenActive(bool active)
	{
		foreach (Transform item in base.transform)
		{
			item.gameObject.SetActive(active);
		}
		m_childrenActive = active;
	}
}
