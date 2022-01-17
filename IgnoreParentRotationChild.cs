using UnityEngine;

public class IgnoreParentRotationChild : MonoBehaviour
{
	[HideInInspector]
	public GameObject AttachedParent;

	private Transform m_Transform;

	private void Awake()
	{
		m_Transform = base.transform;
	}

	private void LateUpdate()
	{
		if (AttachedParent == null)
		{
			GameUtilities.Destroy(base.gameObject);
		}
		else
		{
			m_Transform.position = AttachedParent.transform.position;
		}
	}
}
