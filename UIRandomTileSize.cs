using UnityEngine;

public class UIRandomTileSize : MonoBehaviour
{
	private bool m_StartSet;

	private Vector3 m_StartSize;

	private Vector3 m_StartPos;

	public bool AllowX = true;

	public bool AllowY;

	private void Start()
	{
		if (!m_StartSet)
		{
			m_StartSet = true;
			m_StartSize = base.transform.localScale;
			m_StartPos = base.transform.localPosition;
		}
		base.transform.localScale = new Vector3(AllowX ? (m_StartSize.x * (OEIRandom.FloatValue() + 1f)) : m_StartSize.x, AllowY ? (m_StartSize.y * (OEIRandom.FloatValue() + 1f)) : m_StartSize.y, m_StartSize.z);
		base.transform.localPosition = new Vector3(m_StartPos.x + (m_StartSize.x - base.transform.localScale.x), m_StartPos.y + (m_StartSize.y - base.transform.localScale.y), base.transform.localPosition.z);
	}
}
