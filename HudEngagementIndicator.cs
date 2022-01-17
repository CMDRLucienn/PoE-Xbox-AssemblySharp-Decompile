using System;
using UnityEngine;

public class HudEngagementIndicator : MonoBehaviour
{
	private GameObject m_Source;

	private Transform m_SourceTransform;

	private Faction m_SourceFaction;

	private GameObject m_Target;

	private Transform m_TargetTransform;

	private Faction m_TargetFaction;

	private float m_xrot;

	public GameObject LineRenderer;

	private Material m_Mat;

	private Mesh m_Mesh;

	private Vector3[] m_Verts;

	private const int arcVerts = 40;

	public bool Verified { get; set; }

	public GameObject Source
	{
		get
		{
			return m_Source;
		}
		private set
		{
			m_Source = value;
			m_SourceTransform = (m_Source ? m_Source.transform : null);
			m_SourceFaction = (m_Source ? m_Source.GetComponent<Faction>() : null);
		}
	}

	public GameObject Target
	{
		get
		{
			return m_Target;
		}
		private set
		{
			m_Target = value;
			m_TargetTransform = (m_Target ? m_Target.transform : null);
			m_TargetFaction = (m_Target ? m_Target.GetComponent<Faction>() : null);
		}
	}

	public bool TwoWay { get; set; }

	private void Awake()
	{
		MeshFilter component = LineRenderer.GetComponent<MeshFilter>();
		m_Mesh = new Mesh();
		m_Mesh.name = "ArrowMeshFlat";
		m_Verts = new Vector3[82];
		int[] array = new int[240];
		Vector2[] uv = new Vector2[m_Verts.Length];
		m_Mesh.vertices = m_Verts;
		m_Mesh.uv = uv;
		GenerateVertexData(m_Mesh, 1f, 1f, m_xrot);
		for (int i = 0; i < 40; i++)
		{
			int num = i * 2;
			int num2 = (i + 1) * 2;
			array[i * 2 * 3] = num2 + 1;
			array[i * 2 * 3 + 1] = num2;
			array[i * 2 * 3 + 2] = num;
			array[i * 2 * 3 + 3] = num + 1;
			array[i * 2 * 3 + 4] = num2 + 1;
			array[i * 2 * 3 + 5] = num;
		}
		m_Mesh.triangles = array;
		component.mesh = m_Mesh;
		m_Mat = LineRenderer.GetComponent<Renderer>().material;
		m_Mat.shader = HudEngagementManager.Instance.Shader;
		m_Mat.mainTexture = InGameHUD.Instance.EngageTexture;
	}

	private void GenerateVertexData(Mesh mesh, float lengthScale, float vertScale, float rot)
	{
		float num = (float)Math.PI * (lengthScale / 2f + vertScale / 2f);
		int num2 = Mathf.FloorToInt(num / 0.35f);
		Vector2[] uv = mesh.uv;
		float num3 = 0.18f;
		float num4 = InGameHUD.Instance.EngageTexture.height / InGameHUD.Instance.EngageTexture.width;
		int num5 = Mathf.CeilToInt(num / (num3 * num4));
		num3 = num / ((float)num5 * num4);
		for (int i = 0; i <= 40; i++)
		{
			int num6 = i * 2;
			float f = rot + (float)Math.PI * 2f * (float)i / 40f;
			m_Verts[num6] = new Vector3(num3 / 2f, Mathf.Cos(f) * 0.5f * vertScale, Mathf.Sin(f) * 0.5f * lengthScale);
			m_Verts[num6 + 1] = new Vector3((0f - num3) / 2f, Mathf.Cos(f) * 0.5f * vertScale, Mathf.Sin(f) * 0.5f * lengthScale);
			float y = (float)i / (40f / (float)num2);
			uv[num6] = new Vector2(0f, y);
			uv[num6 + 1] = new Vector2(1f, y);
		}
		mesh.vertices = m_Verts;
		mesh.uv = uv;
	}

	private void OnDestroy()
	{
		GameUtilities.Destroy(m_Mat);
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		if (!Target || !Source)
		{
			return;
		}
		LineRenderer.GetComponent<Renderer>().enabled = GameState.Paused || GameCursor.CharacterUnderCursor == Source || GameCursor.CharacterUnderCursor == Target;
		LineRenderer.GetComponent<Renderer>().enabled &= InGameHUD.Instance.ShowHUD;
		if (LineRenderer.GetComponent<Renderer>().enabled)
		{
			Vector3 vector = m_TargetTransform.position - m_SourceTransform.position;
			vector.y = 0f;
			base.transform.rotation = Quaternion.FromToRotation(Vector3.forward, vector.normalized);
			float vertScale = HudEngagementManager.Instance.ArrowScaleY;
			if (vector.sqrMagnitude > HudEngagementManager.Instance.ArrowMaxRange)
			{
				vertScale = HudEngagementManager.Instance.ArrowScaleY + (1f - HudEngagementManager.Instance.ArrowScaleY) * (vector.magnitude / HudEngagementManager.Instance.ArrowMaxRange);
			}
			float num = vector.magnitude - (m_SourceFaction.Mover.Radius + m_TargetFaction.Mover.Radius) / 3f;
			m_xrot += HudEngagementManager.Instance.ArrowRotSpeed * TimeController.sUnscaledDelta / num;
			GenerateVertexData(m_Mesh, num, vertScale, (float)Math.PI / 180f * m_xrot);
			base.transform.position = (m_SourceTransform.position + m_TargetTransform.position) / 2f;
			base.transform.position += new Vector3(0f, HudEngagementManager.Instance.ArrowElevation, 0f);
			if (TwoWay)
			{
				Quaternion quaternion = base.transform.rotation * Quaternion.AngleAxis(90f, Vector3.up);
				base.transform.position += quaternion * (Vector3.forward * HudEngagementManager.Instance.TwoWayOffset);
			}
			UpdateMaterial();
		}
	}

	private void UpdateMaterial()
	{
		if ((bool)m_SourceFaction)
		{
			Color selectionColor = m_SourceFaction.SelectionColor;
			selectionColor.a = 1f;
			m_Mat.color = selectionColor;
			m_Mat.shader = HudEngagementManager.Instance.Shader;
		}
	}

	public void SetTargets(GameObject source, GameObject target)
	{
		Source = source;
		Target = target;
		UpdateMaterial();
		Update();
	}
}
