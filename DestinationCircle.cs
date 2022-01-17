using System;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(LineRenderer))]
public class DestinationCircle : MonoBehaviour
{
	public TweenScale StartTween;

	public TweenScale LoopTween;

	public float TweenStart = 1f;

	public float TweenEnd = 0.95f;

	private float m_YAng;

	private MeshRenderer m_Pies;

	private Mesh m_PieMesh;

	private Material m_PieMaterial;

	private LineRenderer lineRenderer;

	private Material m_Material;

	private Vector3[] m_PieVerts;

	private PartyMemberAI m_Owner;

	private Faction m_OwnerFaction;

	public bool Visible;

	public bool m_Initted;

	public void StartAt(Vector3 position)
	{
		if (!lineRenderer)
		{
			lineRenderer = GetComponent<LineRenderer>();
		}
		Visible = true;
		lineRenderer.enabled = false;
		base.transform.position = position;
		StartTween.Reset();
		StartTween.Play(forward: true);
		LoopTween.Reset();
		LoopTween.enabled = false;
		UpdateColors();
	}

	private void OnIntroDone()
	{
		LoopTween.Reset();
		LoopTween.Play(forward: true);
	}

	private void Start()
	{
		Init();
		int num = 40;
		lineRenderer.positionCount = num;
		lineRenderer.startWidth = 0.02f;
		lineRenderer.endWidth = 0.02f;
		float num2 = (float)Math.PI * 2f / (float)(num - 1);
		for (int i = 0; i < num - 1; i++)
		{
			float f = num2 * (float)i;
			lineRenderer.SetPosition(i, new Vector3(Mathf.Cos(f) * 0.5f, 0f, Mathf.Sin(f) * 0.5f));
		}
		lineRenderer.SetPosition(num - 1, new Vector3(0.5f, 0f, 0f));
	}

	private void Init()
	{
		if ((bool)this && !m_Initted)
		{
			m_Initted = true;
			InitPies();
			if (!lineRenderer)
			{
				lineRenderer = GetComponent<LineRenderer>();
			}
			m_Material = new Material(Shader.Find("Trenton/UI/PE_InGameCircle"));
			lineRenderer.sharedMaterial = m_Material;
		}
	}

	private void InitPies()
	{
		if ((bool)this)
		{
			if (m_Pies == null)
			{
				GameObject gameObject = NGUITools.AddChild(base.gameObject);
				gameObject.name = "Pies";
				gameObject.transform.localPosition = new Vector3(0f, 0.01f, 0f);
				MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
				m_PieMesh = new Mesh();
				m_PieMesh.name = "PieMesh";
				meshFilter.mesh = m_PieMesh;
				m_Pies = gameObject.AddComponent<MeshRenderer>();
				m_Pies.receiveShadows = false;
				m_Pies.shadowCastingMode = ShadowCastingMode.Off;
				m_PieMaterial = new Material(Shader.Find("Trenton/UI/PE_InGameArrowClipping"));
				m_PieMaterial.SetFloat("_OccludedAlpha", 1f);
				m_PieMaterial.mainTexture = InGameHUD.Instance.MoveTargetArrowTexture;
				m_Pies.sharedMaterial = m_PieMaterial;
			}
			int num = 4;
			Vector2[] array = new Vector2[num * 4];
			int[] array2 = new int[num * 6];
			for (int i = 0; i < num; i++)
			{
				int num2 = i * 4;
				int num3 = i * 6;
				array[num2] = new Vector2(0f, 1f);
				array[num2 + 1] = new Vector2(1f, 1f);
				array[num2 + 2] = new Vector2(1f, 0f);
				array[num2 + 3] = new Vector2(0f, 0f);
				array2[num3] = num2;
				array2[num3 + 1] = num2 + 1;
				array2[num3 + 2] = num2 + 2;
				array2[num3 + 3] = num2 + 2;
				array2[num3 + 4] = num2 + 3;
				array2[num3 + 5] = num2;
			}
			m_PieVerts = new Vector3[num * 4];
			m_PieMesh.vertices = m_PieVerts;
			m_PieMesh.uv = array;
			m_PieMesh.triangles = array2;
			UpdatePieGeometry();
		}
	}

	private void UpdatePieGeometry()
	{
		int num = 4;
		float num2 = (float)InGameHUD.Instance.MoveTargetArrowTexture.height / (float)InGameHUD.Instance.MoveTargetArrowTexture.width;
		float num3 = InGameHUD.Instance.MovePieWidth / base.transform.localScale.x;
		float num4 = num3 * num2;
		float movePieOffset = InGameHUD.Instance.MovePieOffset;
		for (int i = 0; i < num; i++)
		{
			int num5 = i * 4;
			Quaternion quaternion = Quaternion.AngleAxis((float)i * (float)Math.PI * 2f / (float)num * 57.29578f, Vector3.up);
			m_PieVerts[num5] = quaternion * new Vector3((0f - num3) / 2f, 0f, movePieOffset);
			m_PieVerts[num5 + 1] = quaternion * new Vector3(num3 / 2f, 0f, movePieOffset);
			m_PieVerts[num5 + 2] = quaternion * new Vector3(num3 / 2f, 0f, movePieOffset - num4);
			m_PieVerts[num5 + 3] = quaternion * new Vector3((0f - num3) / 2f, 0f, movePieOffset - num4);
		}
		m_PieMesh.vertices = m_PieVerts;
	}

	private void Update()
	{
		m_YAng += Time.unscaledDeltaTime * InGameHUD.Instance.MovePieRotSpeed;
		base.transform.rotation = Quaternion.AngleAxis(m_YAng, Vector3.up);
		UpdateColors();
		lineRenderer.enabled = Visible && (m_Owner.Selected || GameCursor.CharacterUnderCursor == m_Owner.gameObject);
		m_Pies.enabled = lineRenderer.enabled;
		UpdatePieGeometry();
	}

	private void UpdateColors()
	{
		if ((bool)m_Material)
		{
			Color color = m_Material.color;
			if ((bool)StartTween)
			{
				color.a *= StartTween.tweenFactor;
			}
			m_Material.color = color;
			color.a *= InGameHUD.Instance.MovePieAlpha;
			m_PieMaterial.color = color;
		}
	}

	public void Set(PartyMemberAI owner)
	{
		float radius = owner.GetComponent<Mover>().Radius;
		m_Owner = owner;
		m_OwnerFaction = m_Owner.GetComponent<Faction>();
		float num = TweenStart * radius;
		base.transform.localScale = new Vector3(num, num, num);
		if ((bool)m_OwnerFaction)
		{
			m_OwnerFaction.OnSelectionCircleMaterialChanged += SetSharedMaterial;
		}
		if ((bool)LoopTween)
		{
			float num2 = TweenEnd * radius;
			LoopTween.from = new Vector3(num, num, num);
			LoopTween.to = new Vector3(num2, num2, num2);
		}
		if ((bool)StartTween)
		{
			StartTween.from *= radius;
			StartTween.to = new Vector3(num, num, num);
		}
	}

	public void SetSharedMaterial(Material mat)
	{
		if ((bool)mat)
		{
			Init();
			if ((bool)m_Material)
			{
				m_Material.color = mat.color;
			}
			if ((bool)m_PieMaterial)
			{
				m_PieMaterial.color = mat.color;
			}
		}
	}
}
