using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioBank))]
public class AudioBankPlayer : MonoBehaviour
{
	public string ClipSet = "";

	public Gaussian IntervalSeconds = new Gaussian(3.0, 2.0);

	private float m_NextPlayInSeconds;

	private bool m_Stopped;

	public bool PlayAtSceneStart;

	public int MaximumPlays = -1;

	[SerializeField]
	[HideInInspector]
	private int m_CurrentPlays;

	private AudioBank m_AudioBank;

	private void Awake()
	{
		m_AudioBank = GetComponent<AudioBank>();
	}

	private void Start()
	{
		if (PlayAtSceneStart)
		{
			m_NextPlayInSeconds = 0f;
		}
		else
		{
			m_NextPlayInSeconds = (float)(IntervalSeconds.Mean * AudioBank.s_AudioRandom.NextDouble());
		}
		if (IntervalSeconds.Mean <= 0.0 && MaximumPlays < 0)
		{
			Debug.LogError(SceneManager.GetActiveScene().name + ": AudioBankPlayer '" + base.name + "' has Mean interval set to 0.");
		}
	}

	private void Update()
	{
		if (m_Stopped || !(TimeController.Instance != null))
		{
			return;
		}
		m_NextPlayInSeconds -= Time.unscaledDeltaTime;
		if (m_NextPlayInSeconds <= 0f && !TimeController.Instance.Paused && !Play())
		{
			m_CurrentPlays++;
			if (m_CurrentPlays < MaximumPlays || MaximumPlays == -1)
			{
				m_NextPlayInSeconds = (float)IntervalSeconds.RandomSample();
			}
			else
			{
				m_Stopped = true;
			}
		}
	}

	protected bool Play()
	{
		return m_AudioBank.PlayFrom(ClipSet);
	}
}
