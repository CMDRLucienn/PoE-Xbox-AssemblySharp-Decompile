using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(LineRenderer))]
public class TargetCircle : MonoBehaviour
{
	private List<LineRenderer> renderers = new List<LineRenderer>();

	private void Start()
	{
		LineRenderer component = GetComponent<LineRenderer>();
		renderers.Add(component);
		int num2 = (component.positionCount = 40);
		component.startWidth = 0.015f;
		component.endWidth = 0.015f;
		float num3 = (float)Math.PI * 2f / (float)(num2 - 1);
		for (int i = 0; i < num2 - 1; i++)
		{
			float f = num3 * (float)i;
			component.SetPosition(i, new Vector3(Mathf.Cos(f) * 0.5f, 0f, Mathf.Sin(f) * 0.5f));
		}
		component.SetPosition(num2 - 1, new Vector3(0.5f, 0f, 0f));
		for (float num4 = 0f; num4 < (float)Math.PI * 2f; num4 += (float)Math.PI / 2f)
		{
			GameObject obj = new GameObject("LineCross");
			obj.transform.parent = base.transform;
			obj.transform.localScale = new Vector3(1f, 1f, 1f);
			obj.transform.localPosition = Vector3.zero;
			obj.layer = base.gameObject.layer;
			LineRenderer lineRenderer = obj.AddComponent<LineRenderer>();
			lineRenderer.positionCount = 2;
			lineRenderer.startWidth = 0.04f;
			lineRenderer.endWidth = 0f;
			lineRenderer.SetPosition(0, Vector3.zero);
			lineRenderer.SetPosition(1, new Vector3(Mathf.Cos(num4) * 0.8f, 0f, Mathf.Sin(num4) * 0.8f));
			lineRenderer.material = component.material;
			lineRenderer.useWorldSpace = false;
			lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
			lineRenderer.receiveShadows = false;
			renderers.Add(lineRenderer);
		}
	}

	public void Set(MonoBehaviour owner)
	{
		Faction component = owner.GetComponent<Faction>();
		if ((bool)component)
		{
			component.OnSelectionCircleMaterialChanged += SetSharedMaterial;
		}
	}

	public void SetSharedMaterial(Material mat)
	{
		if ((bool)mat)
		{
			for (int num = renderers.Count - 1; num >= 0; num--)
			{
				renderers[num].sharedMaterial = mat;
			}
		}
	}
}
