using UnityEngine;
using XGamingRuntime;

public class XboxOnePresenceManager
{
	private float m_Timer;

	private bool m_ShouldSend;

	private XblPresenceRichPresenceIds m_presenceIDs;

	private string m_presenceId;

	private string[] m_tokenIds;

	private const float TimeBeforeSend = 15f;

	public void Initialize()
	{
		Debug.Log("---------GAMEPASS: XboxOnePresenceManager Initialize");
		m_Timer = 3f;
	}

	public void SetPresence(string presenceId, string[] tokenIds)
	{
		if (GamePassManager.Initialized)
		{
			int num = XblPresenceRichPresenceIds.Create(GamePassManager.Instance.PrimaryServiceConfigID, presenceId, tokenIds, out m_presenceIDs);
			Debug.Log("---------GAMEPASS: XboxOnePresenceManager.SetPresence: Set presence: " + presenceId + ", scid: " + GamePassManager.Instance.PrimaryServiceConfigID + " ---> result : " + num.ToString("X8"));
			m_ShouldSend = true;
			if (m_presenceIDs != null)
			{
				if (m_presenceIDs.PresenceId != null)
				{
					Debug.Log("---------GAMEPASS: XboxOnePresenceManager.SetPresence: presence id to set: " + m_presenceIDs.PresenceId);
					return;
				}
				Debug.LogError("---------GAMEPASS: XboxOnePresenceManager.SetPresence: m_presenceIDs.PresenceId is null");
			}
			else
			{
				Debug.LogError("---------GAMEPASS: XboxOnePresenceManager.SetPresence: m_presenceIDs is null !");
			}
		}
		else
		{
			Debug.LogError("---------GAMEPASS: XboxOnePresenceManager.SetPresence : GamePassManager not initialized !");
		}
		m_presenceId = presenceId;
		m_tokenIds = tokenIds;
	}

	public void Update()
	{
		if (m_Timer > 0f)
		{
			m_Timer -= Time.unscaledDeltaTime;
			if (m_Timer <= 0f)
			{
				m_Timer = 0f;
			}
		}
		if (m_Timer <= 0f)
		{
			if (m_presenceId != null && GamePassManager.Initialized)
			{
				int num = XblPresenceRichPresenceIds.Create(GamePassManager.Instance.PrimaryServiceConfigID, m_presenceId, m_tokenIds, out m_presenceIDs);
				Debug.Log("---------GAMEPASS: XboxOnePresenceManager.Update: Set presence: " + m_presenceId + ", scid: " + GamePassManager.Instance.PrimaryServiceConfigID + " ---> result : " + num.ToString("X8"));
				m_ShouldSend = true;
				m_presenceId = null;
			}
			if (m_ShouldSend)
			{
				Debug.Log("---------GAMEPASS: XboxOnePresenceManager.Update : XblPresenceSetPresenceAsync m_presenceIDs: " + m_presenceIDs.PresenceId);
				_ = GamePassManager.Instance.ContextHandle;
				SDK.XBL.XblPresenceSetPresenceAsync(GamePassManager.Instance.ContextHandle, isUserActiveInTitle: true, m_presenceIDs, PresenceCompleted);
				m_Timer = 15f;
			}
		}
	}

	private void PresenceCompleted(int hr)
	{
		Debug.Log("---------GAMEPASS: XboxOnePresenceManager.SetPresence : Setting rich presence Status: " + ((hr >= 0) ? "Success" : "Failed") + ", " + hr.ToString("X8"));
		if (hr >= 0)
		{
			m_ShouldSend = false;
		}
	}
}
