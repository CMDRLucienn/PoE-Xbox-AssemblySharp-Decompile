using System;
using AnimationOrTween;
using UnityEngine;
using UnityEngine.Rendering;

public class SelectionCircle : MonoBehaviour
{
	public delegate void SharedMaterialChanged(Material mat);

	public enum Mode
	{
		Normal,
		Hidden,
		Hovered,
		Acting,
		Targeted
	}

	public TweenScale ScaleTween;

	public TweenColorIndependent ColorTween;

	public TweenScale ZoomTween;

	public TweenValue PieTween;

	private float m_PieTweenValue = 1f;

	public float ScaleTo = 0.78f;

	private float m_Radius = 0.5f;

	private GameObject m_Owner;

	private PartyMemberAI m_pai;

	private Faction m_faction;

	private CharacterStats m_stats;

	private HighlightCharacter m_highlight;

	private Health m_Health;

	private AlphaControl m_AlphaControl;

	private MeshRenderer[] m_Pies;

	private Material m_PieMaterial;

	private Mesh m_PieMesh;

	private MeshRenderer m_FlankRenderer;

	private Material m_FlankMaterial;

	private MeshRenderer m_EngageRenderer;

	private MeshRenderer m_TargetedEngageRenderer;

	private Vector3[] m_PieVerts;

	private Vector3[] m_CircleVerts;

	private Color[] m_CircleColors;

	private MeshRenderer m_Circle;

	private Mesh m_CircleMesh;

	private const int circleSegCount = 44;

	private const int pieSegCount = 16;

	private Material m_selectedMaterial;

	public Mode CurrentMode { get; set; }

	private bool OwnerEngaged
	{
		get
		{
			AIController aIController = GameUtilities.FindActiveAIController(m_Owner);
			if ((bool)aIController && aIController.EngagedBy != null)
			{
				return aIController.EngagedBy.Count > 0;
			}
			return false;
		}
	}

	private float GetCircleWidth
	{
		get
		{
			if (OwnerEngaged)
			{
				return InGameHUD.Instance.EngagedCircleWidth * (1f + m_Radius / 5f);
			}
			return InGameHUD.Instance.SelectionCircleWidth;
		}
	}

	public event SharedMaterialChanged OnSharedMaterialChanged;

	private void Start()
	{
		GameObject gameObject = NGUITools.AddChild(base.gameObject);
		gameObject.name = "FlankTexture";
		gameObject.AddComponent<MeshFilter>().mesh = InGameHUD.Instance.SelectionCircleMesh;
		m_FlankRenderer = gameObject.AddComponent<MeshRenderer>();
		m_FlankRenderer.shadowCastingMode = ShadowCastingMode.Off;
		m_FlankRenderer.receiveShadows = false;
		m_FlankRenderer.enabled = false;
		m_FlankMaterial = new Material(Shader.Find("Trenton/UI/PE_InGameTextureCircle"));
		m_FlankMaterial.mainTexture = InGameHUD.Instance.FlankTexture;
		m_FlankRenderer.sharedMaterial = m_FlankMaterial;
		GameObject gameObject2 = NGUITools.AddChild(base.gameObject);
		gameObject2.name = "EngagedSpikes";
		gameObject2.AddComponent<MeshFilter>().mesh = InGameHUD.Instance.EngagedSpikeMesh;
		m_EngageRenderer = gameObject2.AddComponent<MeshRenderer>();
		m_EngageRenderer.sharedMaterial = m_PieMaterial;
		m_EngageRenderer.shadowCastingMode = ShadowCastingMode.Off;
		m_EngageRenderer.receiveShadows = false;
		m_EngageRenderer.enabled = false;
		gameObject2 = NGUITools.AddChild(base.gameObject);
		gameObject2.name = "TargetedEngagedSpikes";
		gameObject2.transform.localRotation = Quaternion.AngleAxis(45f, Vector3.up);
		gameObject2.AddComponent<MeshFilter>().mesh = InGameHUD.Instance.EngagedSpikeMesh;
		m_TargetedEngageRenderer = gameObject2.AddComponent<MeshRenderer>();
		m_TargetedEngageRenderer.sharedMaterial = m_PieMaterial;
		m_TargetedEngageRenderer.shadowCastingMode = ShadowCastingMode.Off;
		m_TargetedEngageRenderer.receiveShadows = false;
		m_TargetedEngageRenderer.enabled = false;
	}

	private void OnEnable()
	{
		UpdateVisibility();
	}

	private void OnColorChanged(Color color)
	{
		if ((bool)m_FlankMaterial)
		{
			m_FlankMaterial.color = color;
		}
		if (ColorTween.enabled)
		{
			color += ColorTween.color;
		}
		for (int i = 0; i < m_Pies.Length; i++)
		{
			m_Pies[i].sharedMaterial.color = color;
		}
		if ((bool)m_EngageRenderer)
		{
			m_EngageRenderer.sharedMaterial.color = color;
		}
		if ((bool)m_TargetedEngageRenderer)
		{
			m_TargetedEngageRenderer.sharedMaterial.color = color;
		}
	}

	private void InitPies()
	{
		if (m_PieMesh == null)
		{
			m_Pies = new MeshRenderer[4];
			m_PieMesh = new Mesh();
			m_PieMesh.name = "PieMesh";
			if (m_PieMaterial == null)
			{
				m_PieMaterial = new Material(Shader.Find("Trenton/UI/PE_InGameTextureCircle"));
			}
			for (int i = 0; i < 4; i++)
			{
				GameObject obj = NGUITools.AddChild(base.gameObject);
				obj.name = "Pie";
				obj.transform.localPosition = new Vector3(0f, 0f, 0f);
				obj.transform.localRotation = Quaternion.AngleAxis(i * 90 + 45, Vector3.up);
				obj.AddComponent<MeshFilter>().mesh = m_PieMesh;
				MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
				meshRenderer.receiveShadows = false;
				if (!meshRenderer.receiveShadows)
				{
					meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
				}
				meshRenderer.sharedMaterial = m_PieMaterial;
				meshRenderer.enabled = false;
				m_Pies[i] = meshRenderer;
			}
		}
		int num = 36;
		int[] array = new int[108];
		for (int j = 0; j < 17; j++)
		{
			int num2 = j * 2;
			int num3 = j * 6;
			array[num3] = num2;
			array[num3 + 1] = num2 + 1;
			array[num3 + 2] = num2 + 3;
			array[num3 + 3] = num2;
			array[num3 + 4] = num2 + 3;
			array[num3 + 5] = num2 + 2;
		}
		int num4 = 34;
		int num5 = 102;
		array[num5] = num4;
		array[num5 + 1] = num4 + 1;
		array[num5 + 2] = 1;
		array[num5 + 3] = num4;
		array[num5 + 4] = 1;
		array[num5 + 5] = 0;
		m_PieVerts = new Vector3[num];
		m_PieMesh.vertices = m_PieVerts;
		m_PieMesh.triangles = array;
		m_PieMesh.uv = new Vector2[num];
		UpdateGeometry();
	}

	private void InitGeometry()
	{
		if (m_Circle == null)
		{
			m_CircleMesh = new Mesh();
			m_CircleMesh.name = "LineCircleMesh";
			m_CircleVerts = new Vector3[88];
			m_CircleColors = new Color[m_CircleVerts.Length];
			int[] array = new int[264];
			Vector2[] array2 = new Vector2[m_CircleVerts.Length];
			for (int i = 0; i < 44; i++)
			{
				int num = i * 2;
				int num2 = (i + 1) % 44 * 2;
				int num3 = i * 6;
				array2[num] = new Vector2((float)i / 43f, 0f);
				array2[num + 1] = new Vector2((float)i / 43f, 1f);
				array[num3] = num;
				array[num3 + 1] = num + 1;
				array[num3 + 2] = num2 + 1;
				array[num3 + 3] = num;
				array[num3 + 4] = num2 + 1;
				array[num3 + 5] = num2;
			}
			m_CircleMesh.vertices = m_CircleVerts;
			m_CircleMesh.colors = m_CircleColors;
			m_CircleMesh.triangles = array;
			m_CircleMesh.uv = array2;
			m_CircleMesh.normals = new Vector3[m_CircleVerts.Length];
			UpdateGeometry();
			base.gameObject.AddComponent<MeshFilter>().mesh = m_CircleMesh;
			m_Circle = base.gameObject.AddComponent<MeshRenderer>();
			m_Circle.receiveShadows = false;
			if (!m_Circle.receiveShadows)
			{
				m_Circle.shadowCastingMode = ShadowCastingMode.Off;
			}
		}
	}

	private void UpdateGeometry()
	{
		float x = base.transform.lossyScale.x;
		float num = GetCircleWidth / x;
		float num2 = InGameHUD.Instance.SelectionCircleWidth / x;
		m_PieTweenValue = PieTween.Value;
		if ((bool)m_Circle)
		{
			Color b = (ColorTween.enabled ? ColorTween.color : Color.black);
			b.a = 0.6f;
			Color a = (ColorTween.enabled ? ColorTween.color : Color.black);
			a.a = 1f;
			float num3 = 0.1461206f;
			float num4 = 22f;
			float num5 = m_Radius - num;
			float num6 = (float)Math.PI / 4f;
			for (int i = 0; i < 44; i++)
			{
				int num7 = i * 2;
				float f = num3 * (float)i - num6;
				int num8 = Mathf.Min(44 - i, i);
				float num9 = Mathf.Cos(f);
				float num10 = Mathf.Sin(f);
				m_CircleVerts[num7] = new Vector3(num9 * num5, 0f, num10 * num5);
				m_CircleVerts[num7 + 1] = new Vector3(num9 * m_Radius, 0f, num10 * m_Radius);
				m_CircleColors[num7] = Color.Lerp(a, b, (float)num8 / num4);
				m_CircleColors[num7 + 1] = m_CircleColors[num7];
			}
			m_CircleMesh.vertices = m_CircleVerts;
			m_CircleMesh.colors = m_CircleColors;
		}
		float num11 = InGameHUD.Instance.PieArc / 16f;
		float num12 = (PieTween.to - PieTween.from) * m_Radius;
		for (int j = 0; j < 17; j++)
		{
			int num13 = j * 2;
			Quaternion quaternion = Quaternion.AngleAxis(num11 * ((float)j - 8.5f), Vector3.up);
			float x2 = 0f;
			switch (j)
			{
			case 0:
				x2 = num2;
				break;
			case 16:
				x2 = 0f - num2;
				break;
			}
			Vector3 vector = new Vector3(0f, 0f, m_Radius * m_PieTweenValue);
			m_PieVerts[num13] = quaternion * new Vector3(x2, 0f, m_Radius - num - num12) + vector;
			m_PieVerts[num13 + 1] = quaternion * new Vector3(0f, 0f, m_Radius - num12) + vector;
		}
		m_PieVerts[35] = new Vector3(0f, 0f, m_PieTweenValue * m_Radius);
		m_PieVerts[34] = new Vector3(0f, 0f, m_PieTweenValue * m_Radius + Mathf.Abs(num2 / Mathf.Sin(InGameHUD.Instance.PieArc * ((float)Math.PI / 180f) / 2f)));
		m_PieMesh.vertices = m_PieVerts;
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnDisable()
	{
		ResetColorTween();
		ColorTween.enabled = false;
	}

	private void ResetColorTween()
	{
		ColorTween.Reset();
	}

	public bool IsVisible()
	{
		if ((bool)m_faction && !m_faction.isFowVisible)
		{
			return false;
		}
		if (InGameHUD.Instance.HudUserMode > 0)
		{
			return false;
		}
		if ((CurrentMode == Mode.Hidden || CurrentMode == Mode.Acting) && (CurrentMode != Mode.Acting || !m_faction.DrawConversationCircle))
		{
			return false;
		}
		if ((bool)m_stats && m_stats.InvisibilityState > 0 && !m_pai)
		{
			return false;
		}
		if ((bool)m_Health && m_Health.ShowDead)
		{
			return false;
		}
		return true;
	}

	private void UpdateVisibility()
	{
		float num = 1f;
		bool flag = CurrentMode == Mode.Targeted;
		bool flag2 = CurrentMode != Mode.Targeted;
		bool flag3 = IsVisible();
		if ((bool)m_AlphaControl)
		{
			num = m_AlphaControl.Alpha;
		}
		if (num < float.Epsilon)
		{
			flag = false;
			flag2 = false;
		}
		bool ownerEngaged = OwnerEngaged;
		if (m_Pies != null)
		{
			for (int i = 0; i < m_Pies.Length; i++)
			{
				m_Pies[i].enabled = flag && flag3;
			}
		}
		if ((bool)m_TargetedEngageRenderer)
		{
			m_TargetedEngageRenderer.enabled = ownerEngaged && (flag || flag2) && flag3;
		}
		if (m_Circle != null)
		{
			m_Circle.enabled = flag2 && flag3;
		}
		if ((bool)m_EngageRenderer)
		{
			m_EngageRenderer.enabled = ownerEngaged && flag2 && flag3;
		}
	}

	private void UpdateEngagementScale()
	{
		float num = 1f;
		if (CurrentMode == Mode.Targeted)
		{
			num = m_PieTweenValue + (1f - (PieTween.to - PieTween.from));
		}
		num *= m_Radius * 2f;
		num -= GetCircleWidth / base.transform.lossyScale.x * InGameHUD.Instance.SpikeCircleWidthInset - InGameHUD.Instance.SpikeAbsoluteInset;
		if ((bool)m_TargetedEngageRenderer)
		{
			m_TargetedEngageRenderer.transform.localScale = new Vector3(num, 1f, num);
		}
		if ((bool)m_EngageRenderer)
		{
			m_EngageRenderer.transform.localScale = new Vector3(num, 1f, num);
		}
	}

	private void Update()
	{
		if (PieTween != null && InGameHUD.Instance != null)
		{
			PieTween.from = InGameHUD.Instance.PieMinScale;
			PieTween.to = InGameHUD.Instance.PieMaxScale;
		}
		if (PE_GameRender.Instance != null)
		{
			base.transform.rotation = Quaternion.AngleAxis(PE_GameRender.Instance.transform.rotation.eulerAngles.y + 45f, Vector3.up);
		}
		UpdateVisibility();
		if ((bool)m_Circle && (bool)m_FlankRenderer)
		{
			if ((bool)m_stats)
			{
				m_FlankRenderer.enabled = m_stats.HasStatusEffectFromAffliction(AfflictionData.Flanked) && IsVisible();
			}
			else
			{
				m_FlankRenderer.enabled = false;
			}
		}
		if (((bool)m_pai && m_pai.Selected && m_pai.IsActiveInParty) || GameCursor.CharacterUnderCursor == m_Owner)
		{
			Faction faction = null;
			AIController aIController = GameUtilities.FindActiveAIController(m_Owner);
			if ((bool)aIController && (bool)aIController.CurrentTarget)
			{
				faction = aIController.CurrentTarget.GetComponent<Faction>();
			}
			if ((bool)faction)
			{
				faction.TargetSelectionCircle();
			}
		}
		if (ColorTween != null)
		{
			if (((GameCursor.CharacterUnderCursor == m_Owner && UIWindowManager.MouseInputAvailable) || CurrentMode == Mode.Hovered) && (!m_faction || m_faction.CanBeTargeted))
			{
				if (!ColorTween.enabled)
				{
					ZoomTween.Reset();
					ZoomTween.Play(forward: true);
					ResetColorTween();
					ColorTween.style = UITweener.Style.Loop;
					ColorTween.Play(forward: true);
				}
			}
			else
			{
				ColorTween.enabled = false;
			}
		}
		if (ScaleTween != null)
		{
			if (CurrentMode == Mode.Acting)
			{
				if (!ScaleTween.enabled)
				{
					ScaleTween.style = UITweener.Style.PingPong;
					ScaleTween.Play(forward: true);
				}
			}
			else if (ScaleTween.enabled && ScaleTween.direction == Direction.Reverse)
			{
				ScaleTween.style = UITweener.Style.Once;
			}
		}
		UpdateGeometry();
	}

	private void LateUpdate()
	{
		if (m_highlight != null && m_highlight.ShouldTarget)
		{
			CurrentMode = Mode.Targeted;
		}
		UpdateEngagementScale();
		UpdateVisibility();
		if (CurrentMode == Mode.Targeted)
		{
			if (!PieTween.enabled)
			{
				PieTween.Play(forward: true);
			}
		}
		else
		{
			PieTween.enabled = false;
			PieTween.Reset();
		}
		if (CurrentMode == Mode.Targeted)
		{
			CurrentMode = Mode.Normal;
		}
	}

	public void SetOwner(GameObject owner)
	{
		m_Owner = owner;
		m_pai = m_Owner.GetComponent<PartyMemberAI>();
		m_faction = m_Owner.GetComponent<Faction>();
		m_stats = m_Owner.GetComponent<CharacterStats>();
		m_highlight = m_Owner.GetComponent<HighlightCharacter>();
		m_Health = m_Owner.GetComponent<Health>();
		m_AlphaControl = m_Owner.GetComponent<AlphaControl>();
		base.transform.parent = m_Owner.transform;
	}

	public void SetRootScale(float scale)
	{
		m_Radius = scale / 2f;
		InitPies();
		InitGeometry();
		base.transform.localScale = Vector3.one;
		if ((bool)ScaleTween)
		{
			ScaleTween.from = new Vector3(1f, 1f, 1f);
			ScaleTween.to = new Vector3(ScaleTo, ScaleTo, ScaleTo);
		}
		if ((bool)ZoomTween)
		{
			ZoomTween.from = new Vector3(1.15f, 1.15f, 1.15f);
			ZoomTween.to = new Vector3(1f, 1f, 1f);
		}
	}

	public Color GetSelectedColor()
	{
		if ((bool)m_selectedMaterial)
		{
			return m_selectedMaterial.color;
		}
		return Color.clear;
	}

	public void SetMaterial(bool isFoe, bool isSelected, bool isStealthed, bool isDominated)
	{
		if (!(GetComponent<Renderer>() == null))
		{
			m_selectedMaterial = InGameHUD.Instance.CircleMaterials.Get(!isFoe, InGameHUD.Instance.UseColorBlindSettings, selected: true, isStealthed, isDominated);
			m_Circle.sharedMaterial = InGameHUD.Instance.CircleMaterials.Get(!isFoe, InGameHUD.Instance.UseColorBlindSettings, isSelected, isStealthed, isDominated);
			OnColorChanged(m_Circle.sharedMaterial.color);
			if (this.OnSharedMaterialChanged != null)
			{
				this.OnSharedMaterialChanged(m_Circle.sharedMaterial);
			}
		}
	}
}
