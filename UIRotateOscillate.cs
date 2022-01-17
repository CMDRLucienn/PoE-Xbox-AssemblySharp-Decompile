using System;
using UnityEngine;

public class UIRotateOscillate : MonoBehaviour
{
	public float PeriodSeconds = 0.5f;

	public float ZeroAngle;

	public float Amplitude = 90f;

	private float m_CurrentTime;

	private void Update()
	{
		m_CurrentTime = (m_CurrentTime + Time.deltaTime) % PeriodSeconds;
		base.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, ZeroAngle + Amplitude * Mathf.Sin((float)Math.PI * 2f * m_CurrentTime / PeriodSeconds)));
	}
}
