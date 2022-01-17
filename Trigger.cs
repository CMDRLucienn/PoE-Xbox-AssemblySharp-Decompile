using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Toolbox/Trigger")]
public class Trigger : MonoBehaviour
{
	public enum TriggerType
	{
		OnEnter,
		OnExit,
		OnEnterAndOnExit
	}

	protected bool m_RequireObjectInPolygon = true;

	public bool PlayerOrCompanionCanTrigger = true;

	public bool HostileCanTrigger;

	public bool NeutralCanTrigger;

	public TriggerType Type;

	[Persistent]
	public int NumberOfCharges;

	private List<GameObject> m_pendingTrigerers = new List<GameObject>();

	[Persistent]
	protected int m_triggeredCount;

	[Persistent]
	public bool IsEnabled = true;

	private ScriptEvent m_ScriptEvent;

	private bool m_restored;

	protected virtual void Start()
	{
		m_ScriptEvent = GetComponent<ScriptEvent>();
		if (GetComponent<Persistence>() == null)
		{
			m_restored = true;
		}
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	protected virtual void Update()
	{
		if (m_pendingTrigerers.Count == 0 || GameState.IsLoading || !m_restored)
		{
			return;
		}
		while (m_pendingTrigerers.Count > 0)
		{
			GameObject gameObject = m_pendingTrigerers[0];
			m_pendingTrigerers.RemoveAt(0);
			if (gameObject != null && CanTrigger(gameObject, onEnter: true))
			{
				m_triggeredCount++;
				HandleTriggerEnter(gameObject);
			}
		}
	}

	public virtual void Restored()
	{
		m_restored = true;
	}

	public virtual bool CanTrigger(GameObject obj, bool onEnter)
	{
		if (!IsEnabled)
		{
			return false;
		}
		if ((!onEnter && Type == TriggerType.OnEnter) || (onEnter && Type == TriggerType.OnExit))
		{
			return false;
		}
		if (NumberOfCharges > 0 && m_triggeredCount >= NumberOfCharges)
		{
			return false;
		}
		PE_Collider2D component = GetComponent<PE_Collider2D>();
		if (onEnter && m_RequireObjectInPolygon && component != null && !component.PointInPolygon(obj.transform.position))
		{
			return false;
		}
		PartyMemberAI component2 = obj.GetComponent<PartyMemberAI>();
		if (PlayerOrCompanionCanTrigger && component2 != null && component2.enabled)
		{
			if (component2.Slot >= 0)
			{
				return component2.Slot < 30;
			}
			return false;
		}
		Faction component3 = obj.GetComponent<Faction>();
		if (component3 != null)
		{
			return component3.RelationshipToPlayer switch
			{
				Faction.Relationship.Hostile => HostileCanTrigger, 
				Faction.Relationship.Neutral => NeutralCanTrigger, 
				_ => false, 
			};
		}
		return false;
	}

	protected virtual void HandleTriggerEnter(GameObject obj)
	{
		Faction component = obj.GetComponent<Faction>();
		if (!m_ScriptEvent || !component)
		{
			return;
		}
		SpecialCharacterInstanceID.Add(obj, SpecialCharacterInstanceID.SpecialCharacterInstance.User);
		if (component.RelationshipToPlayer == Faction.Relationship.Hostile)
		{
			m_ScriptEvent.ExecuteScript(ScriptEvent.ScriptEvents.OnHostileEnter);
		}
		else if ((bool)obj.GetComponent<PartyMemberAI>())
		{
			m_ScriptEvent.ExecuteScript(ScriptEvent.ScriptEvents.OnPartyMemberEnter);
			if (Stealth.IsInStealthMode(obj))
			{
				m_ScriptEvent.ExecuteScript(ScriptEvent.ScriptEvents.OnPartyMemberEnterWhileStealthed);
			}
			else
			{
				m_ScriptEvent.ExecuteScript(ScriptEvent.ScriptEvents.OnPartyMemberEnterWhileNonStealthed);
			}
		}
		else
		{
			m_ScriptEvent.ExecuteScript(ScriptEvent.ScriptEvents.OnNeutralEnter);
		}
		SpecialCharacterInstanceID.Add(obj, SpecialCharacterInstanceID.SpecialCharacterInstance.User);
		m_ScriptEvent.ExecuteScript(ScriptEvent.ScriptEvents.OnTriggered);
	}

	protected virtual void HandleTriggerExit(GameObject obj)
	{
		Faction component = obj.GetComponent<Faction>();
		if ((bool)m_ScriptEvent && (bool)component)
		{
			SpecialCharacterInstanceID.Add(obj, SpecialCharacterInstanceID.SpecialCharacterInstance.User);
			if (component.RelationshipToPlayer == Faction.Relationship.Hostile)
			{
				m_ScriptEvent.ExecuteScript(ScriptEvent.ScriptEvents.OnHostileExit);
			}
			else if ((bool)obj.GetComponent<PartyMemberAI>())
			{
				m_ScriptEvent.ExecuteScript(ScriptEvent.ScriptEvents.OnPartyMemberExit);
			}
			else
			{
				m_ScriptEvent.ExecuteScript(ScriptEvent.ScriptEvents.OnNeutralExit);
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		NotifyTriggerEnter(other);
	}

	private void OnTriggerExit(Collider other)
	{
		NotifyTriggerExit(other);
	}

	public void NotifyTriggerEnter(Collider other)
	{
		if (!GameState.IsLoading || !m_restored)
		{
			m_pendingTrigerers.Add(other.gameObject);
		}
		else if (CanTrigger(other.gameObject, onEnter: true))
		{
			m_triggeredCount++;
			HandleTriggerEnter(other.gameObject);
		}
	}

	public void NotifyTriggerExit(Collider other)
	{
		if (CanTrigger(other.gameObject, onEnter: false))
		{
			m_triggeredCount++;
			HandleTriggerExit(other.gameObject);
		}
	}

	public void ResetCharges()
	{
		m_triggeredCount = 0;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Collider component = GetComponent<Collider>();
		if (!(component == null))
		{
			DrawUtility.DrawCollider(component.transform, component, Color.yellow);
		}
	}
}
