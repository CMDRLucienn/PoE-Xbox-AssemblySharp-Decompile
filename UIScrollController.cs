using UnityEngine;

public class UIScrollController : MonoBehaviour
{
	public float DefaultScrollSpeedY;

	private int m_ScrollSpeedMultiplierIndexY;

	private float[] m_PossibleSpeeds = new float[15]
	{
		-32f, -16f, -8f, -4f, -2f, -1f, -0.5f, 0f, 0.5f, 1f,
		2f, 4f, 8f, 16f, 32f
	};

	private bool m_Paused = true;

	public event GameInputEventHandle OnSpeedChanged;

	public void Reset()
	{
		for (int i = 0; i < m_PossibleSpeeds.Length; i++)
		{
			if (m_PossibleSpeeds[i] == 1f)
			{
				m_ScrollSpeedMultiplierIndexY = i;
				if (this.OnSpeedChanged != null)
				{
					this.OnSpeedChanged(base.gameObject, null);
				}
				break;
			}
		}
	}

	public void SetPaused(bool paused)
	{
		m_Paused = paused;
	}

	private void Start()
	{
		Reset();
	}

	public void SetSpeed(float newSpeed)
	{
		int num = -1;
		for (int i = 0; i < m_PossibleSpeeds.Length; i++)
		{
			if (newSpeed < m_PossibleSpeeds[i] || Mathf.Approximately(newSpeed, m_PossibleSpeeds[i]))
			{
				num = i;
				break;
			}
		}
		if (num < 0)
		{
			m_ScrollSpeedMultiplierIndexY = m_PossibleSpeeds.Length - 1;
		}
		else
		{
			m_ScrollSpeedMultiplierIndexY = num;
		}
		this.OnSpeedChanged(base.gameObject, null);
	}

	public void IncreaseSpeed()
	{
		m_ScrollSpeedMultiplierIndexY = Mathf.Min(m_PossibleSpeeds.Length - 1, m_ScrollSpeedMultiplierIndexY + 1);
		if (this.OnSpeedChanged != null)
		{
			this.OnSpeedChanged(base.gameObject, null);
		}
	}

	public void DecreaseSpeed()
	{
		m_ScrollSpeedMultiplierIndexY = Mathf.Max(0, m_ScrollSpeedMultiplierIndexY - 1);
		if (this.OnSpeedChanged != null)
		{
			this.OnSpeedChanged(base.gameObject, null);
		}
	}

	public float GetYSpeedMultiplier()
	{
		return m_PossibleSpeeds[m_ScrollSpeedMultiplierIndexY];
	}

	private void Update()
	{
		if (!m_Paused)
		{
			base.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y + DefaultScrollSpeedY * m_PossibleSpeeds[m_ScrollSpeedMultiplierIndexY] * Time.deltaTime, base.transform.localPosition.z);
		}
	}
}
