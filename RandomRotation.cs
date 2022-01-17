using UnityEngine;

public class RandomRotation : MonoBehaviour
{
	[Range(-360f, 360f)]
	public float MinX;

	[Range(-360f, 360f)]
	public float MaxX;

	[Range(-360f, 360f)]
	public float MinY;

	[Range(-360f, 360f)]
	public float MaxY;

	[Range(-360f, 360f)]
	public float MinZ;

	[Range(-360f, 360f)]
	public float MaxZ;

	public bool UseTimer;

	[Range(0f, 120f)]
	public float MinTimer;

	[Range(0f, 120f)]
	public float MaxTimer;

	private float m_timer;

	private void Start()
	{
		Rotate();
	}

	private void Update()
	{
		if (UseTimer)
		{
			m_timer -= Time.deltaTime;
			if (m_timer < 0f)
			{
				Rotate();
			}
		}
	}

	private void Rotate()
	{
		base.transform.rotation = Quaternion.Euler(OEIRandom.RangeInclusive(MinX, MaxX), OEIRandom.RangeInclusive(MinY, MaxY), OEIRandom.RangeInclusive(MinZ, MaxZ));
		m_timer = OEIRandom.RangeInclusive(MinTimer, MaxTimer);
	}
}
