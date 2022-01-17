using UnityEngine;

[RequireComponent(typeof(UILabel))]
public class UIHealthstring : MonoBehaviour
{
	private UILabel m_Label;

	private float m_Lifetime;

	private float m_TotalLifetime;

	private bool m_UpdateWhilePaused;

	public float Rise = 0.5f;

	private float m_LastCheckVisibilityTimestamp = -1f;

	private const float VISIBILITY_CHECK_DUR = 0.33f;

	private bool isVisible = true;

	private Vector3 m_WorldPos;

	private void Start()
	{
		if (m_Label != null)
		{
			m_Label.alpha = 0f;
		}
	}

	private void Update()
	{
		if (m_UpdateWhilePaused)
		{
			m_Lifetime -= Time.unscaledDeltaTime;
		}
		else
		{
			m_Lifetime -= Time.deltaTime;
		}
		float num = 1f - m_Lifetime / m_TotalLifetime;
		float num2 = (num - 0.25f) / 0.75f;
		float num3 = 0f;
		num3 = ((!(num < 0.25f)) ? Mathf.SmoothStep(Rise * 0.75f, Rise, num2) : Mathf.SmoothStep(0f, Rise * 0.75f, num / 0.25f));
		m_Label.alpha = (isVisible ? Mathf.SmoothStep(1f, 0f, (num < 0.25f) ? 0f : num2) : 0f);
		Vector3 worldPos = m_WorldPos;
		worldPos.y += num3;
		SetWorldPosition(worldPos);
		CheckFogOfWarVisibility();
		if (m_Lifetime <= 0f)
		{
			GameUtilities.Destroy(base.gameObject);
		}
	}

	public void Set(DamageInfo damage)
	{
		if (!m_Label)
		{
			m_Label = GetComponent<UILabel>();
		}
		m_UpdateWhilePaused = false;
		if (damage.FinalAdjustedDamage > 0f)
		{
			m_Label.color = UIHealthstringManager.Instance.DamageColor;
		}
		else if (damage.FinalAdjustedDamage < 0f)
		{
			m_Label.color = UIHealthstringManager.Instance.HealingColor;
		}
		else
		{
			m_Label.color = Color.white;
		}
		if (damage.IsMiss)
		{
			m_Label.text = GUIUtils.GetText(54).ToUpper();
		}
		else
		{
			m_Label.text = ((damage.FinalAdjustedDamage < 1f) ? damage.FinalAdjustedDamage.ToString("#0.0") : damage.FinalAdjustedDamage.ToString("#0"));
		}
	}

	public void Set(float damage)
	{
		if (!m_Label)
		{
			m_Label = GetComponent<UILabel>();
		}
		m_UpdateWhilePaused = false;
		if (damage > 0f)
		{
			m_Label.color = UIHealthstringManager.Instance.DamageColor;
		}
		else if (damage < 0f)
		{
			m_Label.color = UIHealthstringManager.Instance.HealingColor;
		}
		else
		{
			m_Label.color = Color.white;
		}
		m_Label.text = Mathf.RoundToInt(Mathf.Abs(damage)).ToString();
	}

	public void Set(string warning, Color color)
	{
		if (!m_Label)
		{
			m_Label = GetComponent<UILabel>();
		}
		m_UpdateWhilePaused = true;
		m_Label.color = color;
		m_Label.text = warning;
	}

	public void Begin(GameObject target, float duration)
	{
		float entityMaxY = UIAnchorToWorld.GetEntityMaxY(target);
		m_WorldPos = new Vector3(target.transform.position.x, entityMaxY, target.transform.position.z) + Camera.main.transform.right * OEIRandom.RangeInclusive(-0.25f, 0.25f);
		SetWorldPosition(m_WorldPos);
		CheckFogOfWarVisibility();
		m_TotalLifetime = duration;
		m_Lifetime = m_TotalLifetime;
	}

	private void SetWorldPosition(Vector3 pos)
	{
		if ((bool)Camera.main)
		{
			Vector3 localPosition = InGameUILayout.ScreenToNgui(Camera.main.WorldToScreenPoint(pos));
			localPosition.z = base.transform.localPosition.z;
			base.transform.localPosition = localPosition;
		}
	}

	private void CheckFogOfWarVisibility()
	{
		if (m_LastCheckVisibilityTimestamp < 0f || Time.time - m_LastCheckVisibilityTimestamp > 0.33f)
		{
			isVisible = !FogOfWar.Instance || FogOfWar.Instance.PointVisible(m_WorldPos);
			m_LastCheckVisibilityTimestamp = Time.time;
		}
	}
}
