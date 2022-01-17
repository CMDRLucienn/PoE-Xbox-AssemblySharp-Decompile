using System;
using UnityEngine;

public class UILootTooltip : MonoBehaviour
{
	public UIPanel PanelMain;

	public UITexture TexItem;

	public UILabel LblTitle;

	private UIAnchorToWorld m_worldAnchor;

	private float m_showDuration;

	private bool m_triggeredDone;

	private float m_curDisplayTime;

	private float m_OffsetY;

	private Vector3 m_CachedPos;

	private const float FADE_TIME = 0.5f;

	private const float ITEM_NUM_OFFSET = -50f;

	public event Action<UILootTooltip> TooltipDone;

	public void Initialize(Item toLoad, int itemQty, GameObject spawnPos, float duration, int offsetCount)
	{
		PanelMain.alpha = 0f;
		m_triggeredDone = false;
		m_showDuration = duration;
		if (itemQty != 1)
		{
			LblTitle.text = StringUtility.Format("{0} (" + GUIUtils.Format(1278, itemQty) + ")", toLoad.Name);
		}
		else
		{
			LblTitle.text = toLoad.Name;
		}
		TexItem.mainTexture = toLoad.GetIconTexture();
		TexItem.color = Color.white;
		if (m_worldAnchor == null)
		{
			m_worldAnchor = GetComponent<UIAnchorToWorld>();
		}
		if (m_worldAnchor != null && spawnPos != null)
		{
			Vector3 position;
			if (spawnPos.GetComponent<Collider>() != null)
			{
				position = spawnPos.transform.position;
				position.y += spawnPos.GetComponent<Collider>().bounds.size.y;
				m_worldAnchor.AnchorPos = position;
			}
			else
			{
				position = spawnPos.transform.position;
			}
			m_worldAnchor.AnchorPos = position;
			m_OffsetY = -50f * (float)offsetCount;
			base.transform.localPosition = m_worldAnchor.Position;
		}
	}

	public void Show()
	{
		NGUITools.SetActive(base.gameObject, state: true);
		m_curDisplayTime = 0f;
	}

	public void Reset()
	{
		TexItem.mainTexture = null;
		PanelMain.alpha = 0f;
		m_OffsetY = 0f;
		NGUITools.SetActive(base.gameObject, state: false);
		m_curDisplayTime = 0f;
	}

	private void Update()
	{
		if (!InGameHUD.Instance.ShowHUD)
		{
			if (this.TooltipDone != null)
			{
				this.TooltipDone(this);
			}
			PanelMain.alpha = 0f;
			return;
		}
		if (m_curDisplayTime > 1f + m_showDuration)
		{
			if (!m_triggeredDone)
			{
				PanelMain.alpha = 0f;
				NGUITools.SetActive(base.gameObject, state: false);
				m_triggeredDone = true;
				if (this.TooltipDone != null)
				{
					this.TooltipDone(this);
				}
			}
			return;
		}
		if (m_curDisplayTime > 0.5f + m_showDuration)
		{
			PanelMain.alpha = Mathf.Lerp(1f, 0f, (m_curDisplayTime - (0.5f + m_showDuration)) / 0.5f);
		}
		else if (m_curDisplayTime > 0.5f)
		{
			if (PanelMain.alpha < 1f)
			{
				PanelMain.alpha = 1f;
			}
		}
		else
		{
			PanelMain.alpha = Mathf.Lerp(0f, 1f, m_curDisplayTime / 0.5f);
		}
		m_curDisplayTime += Time.unscaledDeltaTime;
		m_worldAnchor.UpdatePosition();
		if (!Mathf.Approximately(m_OffsetY, 0f))
		{
			m_CachedPos = m_worldAnchor.Position;
			m_CachedPos.y += m_OffsetY;
			base.transform.localPosition = m_CachedPos;
		}
		else
		{
			base.transform.localPosition = m_worldAnchor.Position;
		}
	}
}
