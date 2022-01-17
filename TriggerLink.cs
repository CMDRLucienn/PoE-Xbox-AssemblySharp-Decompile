using System.Collections;
using UnityEngine;

[AddComponentMenu("Toolbox/Trigger")]
public class TriggerLink : MonoBehaviour
{
	public bool triggerEnabled = true;

	public bool PlayerTriggered = true;

	public bool CompanionTriggered = true;

	public bool AITriggered;

	public float delay;

	public int NumberOfCharges;

	protected int m_triggeredCount;

	public virtual bool CanTrigger(GameObject obj)
	{
		if (!triggerEnabled)
		{
			return false;
		}
		if (NumberOfCharges > 0 && m_triggeredCount >= NumberOfCharges)
		{
			return false;
		}
		if (PlayerTriggered && obj.GetComponent<Player>() != null)
		{
			return true;
		}
		if (CompanionTriggered && obj.GetComponent<PartyMemberAI>() != null)
		{
			return true;
		}
		if (AITriggered && obj.GetComponent<AIController>() != null)
		{
			return true;
		}
		return false;
	}

	protected void HandleTrigger(GameObject other)
	{
		StartCoroutine("SendWithDelay");
	}

	public IEnumerator SendWithDelay()
	{
		yield return new WaitForSeconds(delay);
	}

	public virtual void OnTriggerEnter(Collider other)
	{
		if (CanTrigger(other.gameObject))
		{
			m_triggeredCount++;
			HandleTrigger(other.gameObject);
		}
	}

	public virtual void OnTriggerExit(Collider other)
	{
		if (CanTrigger(other.gameObject))
		{
			m_triggeredCount++;
			HandleTrigger(other.gameObject);
		}
	}

	private void OnDrawGizmos()
	{
	}
}
