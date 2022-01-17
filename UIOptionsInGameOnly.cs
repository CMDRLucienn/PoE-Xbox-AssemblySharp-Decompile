using UnityEngine;

public class UIOptionsInGameOnly : MonoBehaviour
{
	public GameObject Target;

	public bool Activate = true;

	public bool InvertActive;

	private Vector3 m_InitialPos;

	private bool m_InitialRecorded;

	public Vector3 Offset;

	public bool InvertOffset;

	private void OnEnable()
	{
		if (!m_InitialRecorded)
		{
			m_InitialRecorded = true;
			m_InitialPos = Target.transform.localPosition;
		}
		if ((bool)Target)
		{
			if (Activate)
			{
				Target.SetActive(InvertActive != (bool)InGameHUD.Instance);
			}
			if (InvertOffset != (bool)InGameHUD.Instance)
			{
				Target.transform.localPosition = m_InitialPos + Offset;
			}
			else
			{
				Target.transform.localPosition = m_InitialPos;
			}
		}
	}
}
