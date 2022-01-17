using UnityEngine;

public class FogOfWarRender : MonoBehaviour
{
	private Shader m_shader;

	private Material m_material;

	private FogOfWar m_fogOfWar;

	public static FogOfWarRender Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'FogOfWarRender' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
	}

	private void Start()
	{
		m_fogOfWar = FogOfWar.Instance;
		if (m_fogOfWar == null)
		{
			GameObject gameObject = new GameObject("FogOfWar");
			m_fogOfWar = gameObject.AddComponent<FogOfWar>();
			m_fogOfWar.CompletelyExplored = false;
		}
		m_fogOfWar.InitFogOfWar();
		m_shader = Shader.Find("Trenton/PE_FogOfWarGrid");
		m_material = new Material(m_shader);
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!(m_fogOfWar == null) && m_fogOfWar.FogAlphas != null)
		{
			m_fogOfWar.FogMesh.uv = m_fogOfWar.FogAlphas;
			m_material.SetPass(0);
			m_material.SetVector("_FogColor", m_fogOfWar.FogColor);
			RenderTexture active = RenderTexture.active;
			Graphics.SetRenderTarget(source);
			Graphics.DrawMeshNow(m_fogOfWar.FogMesh, Vector3.zero, Quaternion.identity);
			Graphics.SetRenderTarget(active);
			Graphics.Blit(source, destination);
		}
	}
}
