using UnityEngine;

public class UIRotateByTicks : MonoBehaviour
{
	public float Increment = 25f;

	public float IntervalSeconds = 1f;

	private float m_CurrentTime;

	private float m_CurrentAngle;

	private void Update()
	{
		m_CurrentTime += Time.deltaTime;
		if (m_CurrentTime >= IntervalSeconds)
		{
			m_CurrentTime -= IntervalSeconds;
			m_CurrentAngle = (m_CurrentAngle + Increment) % 360f;
			base.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, m_CurrentAngle));
		}
	}
}
