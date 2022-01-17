using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CompanyIntroductionManager : MonoBehaviour
{
	public MovieManager IntroMovieManager;

	public AudioSource IntroMusicSource;

	public UITexture LogoTexture;

	public UITexture KickstarterTexture;

	public UITexture ObsidianLogoTexture;

	public UITexture PoweredByUnityTexture;

	public float DelayInbetweenLogos;

	public float LogoShowTime;

	public bool FadeMusicOutAfterDelay = true;

	private bool m_skipped;

	private float m_fadeSpeed = 1.5f;

	private void Start()
	{
		Time.timeScale = 1f;
		LogoTexture.alpha = 0f;
		KickstarterTexture.alpha = 0f;
		ObsidianLogoTexture.alpha = 0f;
		if (!FadeMusicOutAfterDelay)
		{
			Object.DontDestroyOnLoad(IntroMusicSource.gameObject);
		}
		StartCoroutine(IntroCoroutine());
		GameUtilities.CheckForExpansions();
		GameUtilities.CreateGlobalPrefabObject();
	}

	private IEnumerator IntroCoroutine()
	{
		yield return new WaitForSeconds(1f);
		float introStartTime = Time.time;
		IntroMovieManager.Fade = false;
		IntroMovieManager.PlayMovieAtPath("Movies/ObsidianLogoMovieN", skippable: false);
		if (IntroMusicSource != null)
		{
			IntroMusicSource.volume = GameState.Option.GetVolume(MusicManager.SoundCategory.MUSIC);
			IntroMusicSource.Play();
		}
		yield return null;
		float movieDuration = IntroMovieManager.GetMovieDuration();
		yield return new WaitForSeconds(movieDuration - 1f / m_fadeSpeed);
		while (ObsidianLogoTexture.alpha < 1f)
		{
			ObsidianLogoTexture.alpha += Time.deltaTime * m_fadeSpeed;
			IntroMovieManager.PlayTexture.alpha -= Time.deltaTime * m_fadeSpeed;
			yield return null;
		}
		IntroMovieManager.StopMovie();
		ObsidianLogoTexture.alpha = 1f;
		yield return new WaitForSeconds(LogoShowTime);
		while (ObsidianLogoTexture.alpha > 0f)
		{
			ObsidianLogoTexture.alpha -= Time.deltaTime * m_fadeSpeed;
			yield return null;
		}
		ObsidianLogoTexture.alpha = 0f;
		yield return new WaitForSeconds(DelayInbetweenLogos);
		while (LogoTexture.alpha < 1f)
		{
			LogoTexture.alpha += Time.deltaTime * m_fadeSpeed;
			yield return null;
		}
		LogoTexture.alpha = 1f;
		yield return new WaitForSeconds(LogoShowTime);
		while (LogoTexture.alpha > 0f)
		{
			LogoTexture.alpha -= Time.deltaTime * m_fadeSpeed;
			yield return null;
		}
		LogoTexture.alpha = 0f;
		yield return new WaitForSeconds(DelayInbetweenLogos);
		while (PoweredByUnityTexture.alpha < 1f)
		{
			PoweredByUnityTexture.alpha += Time.deltaTime * m_fadeSpeed;
			yield return null;
		}
		PoweredByUnityTexture.alpha = 1f;
		yield return new WaitForSeconds(LogoShowTime);
		while (PoweredByUnityTexture.alpha > 0f)
		{
			PoweredByUnityTexture.alpha -= Time.deltaTime * m_fadeSpeed;
			yield return null;
		}
		PoweredByUnityTexture.alpha = 0f;
		yield return new WaitForSeconds(DelayInbetweenLogos);
		while (KickstarterTexture.alpha < 1f)
		{
			KickstarterTexture.alpha += Time.deltaTime * m_fadeSpeed;
			yield return null;
		}
		SceneManager.LoadScene("MainMenu");
		if (!FadeMusicOutAfterDelay)
		{
			float num = Time.time - introStartTime;
			Object.Destroy(IntroMusicSource.gameObject, IntroMusicSource.clip.length - num + 1f);
		}
		base.enabled = false;
	}

	private void Update()
	{
		if (!base.enabled)
		{
			return;
		}
		if (!m_skipped && Input.anyKeyDown)
		{
			StopAllCoroutines();
			m_skipped = true;
		}
		if (!m_skipped)
		{
			return;
		}
		if (IntroMovieManager.PlayTexture.alpha > 0f || LogoTexture.alpha > 0f || KickstarterTexture.alpha > 0f || ObsidianLogoTexture.alpha > 0f || (IntroMusicSource != null && IntroMusicSource.volume > 0f))
		{
			IntroMovieManager.PlayTexture.alpha -= Time.deltaTime;
			LogoTexture.alpha -= Time.deltaTime;
			KickstarterTexture.alpha -= Time.deltaTime;
			ObsidianLogoTexture.alpha -= Time.deltaTime;
			if ((bool)IntroMusicSource)
			{
				IntroMusicSource.volume -= Time.deltaTime;
			}
		}
		else
		{
			IntroMovieManager.StopMovie();
			if ((bool)IntroMusicSource)
			{
				GameUtilities.Destroy(IntroMusicSource.gameObject);
			}
			SceneManager.LoadScene("MainMenu");
			base.enabled = false;
		}
	}
}
