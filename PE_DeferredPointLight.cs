using UnityEngine;

public class PE_DeferredPointLight : MonoBehaviour
{
	public Color lightColor = new Color(1f, 1f, 1f, 1f);

	public float lightIntensity = 1f;

	public float lightRadius = 1f;

	public bool affectScene = true;

	public bool affectFog = true;

	public float lightAlpha = 1f;

	public float GetLightRadius()
	{
		return lightRadius;
	}

	public void Start()
	{
		if (PE_DeferredLightPass.Instance != null)
		{
			PE_DeferredLightPass.Instance.AddPointLight(this);
		}
	}

	public void OnEnable()
	{
		if (PE_DeferredLightPass.Instance != null)
		{
			PE_DeferredLightPass.Instance.AddPointLight(this);
		}
	}

	public void OnDisable()
	{
		if (PE_DeferredLightPass.Instance != null)
		{
			PE_DeferredLightPass.Instance.RemovePointLight(this);
		}
	}

	private void OnDrawGizmosSelected()
	{
		DrawLightGizmo();
	}

	private void OnDrawGizmos()
	{
		DrawLightGizmo();
	}

	private void DrawLightGizmo()
	{
		Color color = lightColor;
		Vector3 position = base.transform.position;
		float radius = GetLightRadius();
		Gizmos.color = color;
		GUIHelper.GizmoDrawCircle(position, radius);
	}
}
