using System;
using UnityEngine;

public class UISwayMotion : MonoBehaviour
{
	public float MaxMovementX;

	public float MaxMovementY;

	public float PeriodTimeX;

	public float PeriodTimeY;

	private float m_thetaX;

	private float m_thetaY;

	private void Update()
	{
		if (PeriodTimeX > 0f)
		{
			m_thetaX += Time.deltaTime / PeriodTimeX * (float)Math.PI;
			if (m_thetaX > (float)Math.PI * 2f)
			{
				m_thetaX -= (float)Math.PI * 2f;
			}
		}
		if (PeriodTimeY > 0f)
		{
			m_thetaY += Time.deltaTime / PeriodTimeY * (float)Math.PI;
			if (m_thetaY > (float)Math.PI * 2f)
			{
				m_thetaY -= (float)Math.PI * 2f;
			}
		}
		if (PeriodTimeX > 0f || PeriodTimeY > 0f)
		{
			base.gameObject.transform.localPosition = new Vector3((float)(Math.Sin(m_thetaX) * (double)MaxMovementX), (float)(Math.Sin(m_thetaY) * (double)MaxMovementY), base.gameObject.transform.localPosition.z);
		}
	}
}
