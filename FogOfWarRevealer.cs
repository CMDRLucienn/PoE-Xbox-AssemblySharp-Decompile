using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class FogOfWarRevealer : MonoBehaviour
{
	[Tooltip("If a trigger is set, the revealer will be visible if the trigger is revealed.")]
	public FogOfWarTrigger Trigger;

	[Tooltip("If true, line of sight will not be visible to the player, but the map will be revealed underneath.")]
	public bool RevealOnly;

	[Tooltip("If true, walls will block line of sight for the revealer. If false, the entire radius will be revealed.")]
	public bool RespectLOS;

	private FogOfWar.Revealer m_revealer;

	private SphereCollider m_sphereCollider;

	private void Start()
	{
		m_sphereCollider = GetComponent<SphereCollider>();
	}

	private void Update()
	{
		if (m_revealer == null)
		{
			CreateFogRevealer();
		}
	}

	private void CreateFogRevealer()
	{
		if (FogOfWar.Instance != null && m_sphereCollider != null)
		{
			if (m_revealer != null)
			{
				FogOfWar.Instance.RemoveRevealer(m_revealer);
			}
			m_revealer = FogOfWar.Instance.AddRevealer(triggersBoxColliders: false, m_sphereCollider.radius, base.gameObject.transform.position, Trigger, RevealOnly, RespectLOS);
		}
		else
		{
			m_revealer = null;
		}
	}

	private void OnDisable()
	{
		if (m_revealer != null)
		{
			if ((bool)FogOfWar.Instance)
			{
				FogOfWar.Instance.RemoveRevealer(m_revealer);
			}
			m_revealer = null;
		}
	}

	private void OnDestroy()
	{
		if (m_revealer != null)
		{
			if ((bool)FogOfWar.Instance)
			{
				FogOfWar.Instance.RemoveRevealer(m_revealer);
			}
			m_revealer = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnDrawGizmosSelected()
	{
		if (!(Trigger == null))
		{
			Gizmos.color = Color.white;
			Gizmos.DrawLine(base.gameObject.transform.position, Trigger.transform.position);
			Vector3 size = new Vector3(0.25f, 0.25f, 0.25f);
			Gizmos.color = Color.red;
			Gizmos.DrawCube(Trigger.transform.position, size);
			Gizmos.color = Color.green;
			Gizmos.DrawCube(base.transform.position, size);
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawLine(base.transform.position, base.transform.position + base.transform.forward);
	}
}
