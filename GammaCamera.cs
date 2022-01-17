using UnityEngine;

public class GammaCamera : MonoBehaviour
{
	private PE_PostProcessColorLevels m_colorLevels;

	public static void CreateCamera()
	{
		GameObject gameObject = GameResources.LoadPrefab<GameObject>("GammaCamera", instantiate: true);
		if ((bool)gameObject)
		{
			Persistence component = gameObject.GetComponent<Persistence>();
			if ((bool)component)
			{
				GameUtilities.Destroy(component);
			}
			gameObject.transform.parent = Camera.main.gameObject.transform;
		}
	}

	private void Start()
	{
		m_colorLevels = GetComponent<PE_PostProcessColorLevels>();
		m_colorLevels.FadeIn();
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		if ((bool)UIOptionsManager.Instance && UIOptionsManager.Instance.WindowActive())
		{
			m_colorLevels.inRedGamma = UIOptionsManager.Instance.GetGammaSetting();
			m_colorLevels.inGreenGamma = UIOptionsManager.Instance.GetGammaSetting();
			m_colorLevels.inBlueGamma = UIOptionsManager.Instance.GetGammaSetting();
		}
		else
		{
			m_colorLevels.inRedGamma = GameState.Mode.Option.Gamma;
			m_colorLevels.inGreenGamma = GameState.Mode.Option.Gamma;
			m_colorLevels.inBlueGamma = GameState.Mode.Option.Gamma;
		}
	}
}
