using UnityEngine;

[RequireComponent(typeof(UISprite))]
public class FanFillTimer : MonoBehaviour
{
	private UISprite m_fanFillSprite;

	private float m_current;

	private float m_max;

	public bool Invert
	{
		get
		{
			return FanFillSprite.invert;
		}
		set
		{
			FanFillSprite.invert = value;
		}
	}

	public bool IsRunning
	{
		get
		{
			if (m_max > 0f)
			{
				if (!Invert || !(m_current < m_max))
				{
					if (!Invert)
					{
						return m_current > 0f;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public UISprite FanFillSprite
	{
		get
		{
			if (!m_fanFillSprite)
			{
				m_fanFillSprite = GetComponent<UISprite>();
				m_fanFillSprite.type = UISprite.Type.Filled;
			}
			return m_fanFillSprite;
		}
	}

	private void Update()
	{
		m_current -= Time.deltaTime;
		UpdateFill();
	}

	public void StartFanFill(float start, float max)
	{
		m_current = start;
		m_max = max;
		UpdateFill();
	}

	public void Stop()
	{
		m_max = 0f;
		m_current = 0f;
		base.gameObject.SetActive(value: false);
	}

	public void SetFanFill(float current, float max)
	{
		m_current = current;
		m_max = max;
		UpdateFill();
		FanFillSprite.Update();
		base.gameObject.SetActive(value: true);
	}

	private void UpdateFill()
	{
		if (IsRunning)
		{
			float num = m_current / m_max;
			if (FanFillSprite.invert)
			{
				FanFillSprite.fillAmount = 1f - num;
			}
			else
			{
				FanFillSprite.fillAmount = num;
			}
		}
	}
}
