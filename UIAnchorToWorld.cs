using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIAnchorToWorld : MonoBehaviour
{
	public enum OffsetMode
	{
		None,
		AtEdge,
		AsContext
	}

	public UIWidget SizeTarget;

	public GameObject Anchor;

	public Vector3 AnchorPos;

	public Vector2 PixelOffset;

	public bool WholePixelsOnly = true;

	[Tooltip("Offset by the height of the renderer if any?")]
	public bool AboveHead = true;

	[Tooltip("Always update to match the animation, ignoring the usual threshold motion requirement?")]
	public bool DisableBobThreshold;

	public bool DoReposition;

	public bool CacheHead;

	private float m_headCachedOffset;

	private Vector2 m_Position;

	private List<Renderer> m_Renderers;

	private Transform m_Head;

	private const float DEFAULT_HEIGHT = -99999f;

	private float m_CachedHeight = -99999f;

	private float BobThreshold = 9999f;

	public OffsetMode RestrictPosition;

	public Vector2 OffsetMagnitudes = Vector2.zero;

	public Vector2 Position => m_Position;

	public Faction AnchorFaction { get; private set; }

	private void Awake()
	{
		UpdatePosition();
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void UpdatePosition()
	{
		Vector3 zero = Vector3.zero;
		if (Anchor != null)
		{
			zero = Anchor.transform.position;
			float entityHeight = GetEntityHeight();
			if (DisableBobThreshold || Mathf.Abs(m_CachedHeight - entityHeight) > BobThreshold)
			{
				m_CachedHeight = entityHeight;
			}
			if (AboveHead)
			{
				zero.y += m_CachedHeight;
			}
		}
		else
		{
			zero = AnchorPos;
		}
		if (Camera.main != null && UIRoot.GetFirstUIRoot() != null)
		{
			m_Position = InGameUILayout.ScreenToNgui(Camera.main.WorldToScreenPoint(zero));
		}
		Bounds bounds = ((!SizeTarget) ? NGUIMath.CalculateRelativeWidgetBounds(base.transform) : new Bounds(Vector3.Scale(new Vector2(UIWidgetUtils.PivotDirX(SizeTarget.pivot), UIWidgetUtils.PivotDirY(SizeTarget.pivot)) * 0.5f, (Vector2)SizeTarget.transform.localScale), SizeTarget.transform.localScale));
		if (RestrictPosition == OffsetMode.AsContext)
		{
			Vector2 vector = default(Vector2);
			if (m_Position.x + bounds.size.x < (float)Screen.width / 2f)
			{
				m_Position.x += bounds.extents.x;
				vector.x = 1f;
			}
			else
			{
				m_Position.x -= bounds.extents.x;
				vector.x = -1f;
			}
			if (m_Position.y - bounds.size.y > (float)(-Screen.height) / 2f)
			{
				m_Position.y -= bounds.extents.y;
				vector.y = -1f;
			}
			else
			{
				m_Position.y += bounds.extents.y;
				vector.y = 1f;
			}
			m_Position += new Vector2(OffsetMagnitudes.x * vector.x, OffsetMagnitudes.y * vector.y);
		}
		else if (RestrictPosition == OffsetMode.AtEdge)
		{
			float x = bounds.size.x;
			float y = bounds.size.y;
			if (m_Position.y + y > (float)Screen.height / 2f)
			{
				m_Position.y = (float)Screen.height / 2f - y;
			}
			else if (m_Position.y < (float)(-Screen.height) / 2f)
			{
				m_Position.y = (float)(-Screen.height) / 2f;
			}
			if (m_Position.x + x > (float)Screen.width / 2f)
			{
				m_Position.x = (float)Screen.width / 2f - x;
			}
			else if (m_Position.x < (float)(-Screen.width) / 2f)
			{
				m_Position.x = (float)(-Screen.width) / 2f;
			}
		}
		if (WholePixelsOnly)
		{
			m_Position.x = Mathf.Floor(m_Position.x);
			m_Position.y = Mathf.Floor(m_Position.y);
		}
		m_Position += PixelOffset;
		if (DoReposition)
		{
			base.gameObject.transform.localPosition = m_Position;
		}
	}

	public void SetAnchor(GameObject anchor)
	{
		m_headCachedOffset = 0f;
		Anchor = anchor;
		m_CachedHeight = -99999f;
		m_Head = null;
		if ((bool)Anchor)
		{
			AnchorFaction = Anchor.GetComponent<Faction>();
		}
		CacheRenderers();
		UpdatePosition();
	}

	public void SetAnchorPos(Vector3 pos)
	{
		m_headCachedOffset = 0f;
		AnchorPos = pos;
		Anchor = null;
		m_Head = null;
		m_CachedHeight = -99999f;
		UpdatePosition();
	}

	private void CacheRenderers()
	{
		m_Renderers = GetRelevantRenderersInChildren(Anchor).ToList();
	}

	public float GetEntityHeight()
	{
		if ((bool)AnchorFaction && AnchorFaction.TooltipHeightOverride > 0f)
		{
			return AnchorFaction.TooltipHeightOverride;
		}
		if (!m_Head || m_Head == Anchor.transform)
		{
			AnimationBoneMapper component = Anchor.GetComponent<AnimationBoneMapper>();
			if ((bool)component)
			{
				m_Head = component[Anchor, AttackBase.EffectAttachType.Head];
				m_CachedHeight = -99999f;
				m_headCachedOffset = 0f;
			}
		}
		if (CacheHead && m_headCachedOffset > 0f)
		{
			return m_headCachedOffset;
		}
		float num = 0f;
		if ((bool)m_Head)
		{
			num = m_Head.position.y - Anchor.transform.position.y;
		}
		if (m_Renderers != null)
		{
			foreach (Renderer renderer in m_Renderers)
			{
				if ((bool)renderer)
				{
					num = Mathf.Max(num, renderer.bounds.max.y - Anchor.transform.position.y);
				}
			}
		}
		m_headCachedOffset = num;
		return num;
	}

	public static float GetEntityMaxY(GameObject target)
	{
		if (target != null)
		{
			Faction component = target.GetComponent<Faction>();
			if ((bool)component && component.TooltipHeightOverride > 0f)
			{
				return component.transform.position.y + component.TooltipHeightOverride;
			}
			float num = 0f;
			AnimationBoneMapper component2 = target.GetComponent<AnimationBoneMapper>();
			if ((bool)component2)
			{
				Transform transform = component2[target, AttackBase.EffectAttachType.Head];
				if ((bool)transform)
				{
					num = transform.position.y;
				}
			}
			{
				foreach (Renderer relevantRenderersInChild in GetRelevantRenderersInChildren(target))
				{
					num = Mathf.Max(num, relevantRenderersInChild.bounds.max.y);
				}
				return num;
			}
		}
		return 0f;
	}

	public static IEnumerable<Renderer> GetRelevantRenderersInChildren(GameObject target)
	{
		if (!target)
		{
			yield break;
		}
		Renderer[] componentsInChildren = target.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if ((renderer is SkinnedMeshRenderer || renderer is MeshRenderer) && !renderer.gameObject.GetComponent<Cloth>())
			{
				yield return renderer;
			}
		}
	}
}
