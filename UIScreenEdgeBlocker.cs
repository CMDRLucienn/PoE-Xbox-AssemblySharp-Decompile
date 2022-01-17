using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class UIScreenEdgeBlocker : MonoBehaviour
{
	private static List<UIScreenEdgeBlocker> s_Blockers = new List<UIScreenEdgeBlocker>();

	private static Dictionary<GameObject, UIEventListener> s_Substitutes = new Dictionary<GameObject, UIEventListener>();

	public static int CongealDistance = 40;

	public static int TransitionSubSize = 200;

	public static float TransitionHideRatio = 0.75f;

	public UIWidget AnchorPoint;

	private UIWidget m_Widget;

	private static UICamera s_UiCamera;

	private bool IsLeft;

	private bool IsRight;

	private bool IsTop;

	private bool IsBottom;

	public bool IsActive
	{
		get
		{
			if ((bool)m_Widget)
			{
				if ((bool)m_Widget.panel && m_Widget.panel.alpha == 0f)
				{
					return false;
				}
				if (m_Widget.alpha == 0f)
				{
					return false;
				}
			}
			return base.gameObject.activeSelf;
		}
	}

	public BoxCollider Box { get; private set; }

	public static void Reset()
	{
		s_UiCamera = null;
	}

	private void Awake()
	{
		Box = GetComponent<BoxCollider>();
		s_Blockers.Add(this);
		m_Widget = GetComponent<UIWidget>();
	}

	private void Start()
	{
		if (s_UiCamera == null)
		{
			Camera nGUICamera = InGameUILayout.NGUICamera;
			if ((bool)nGUICamera)
			{
				s_UiCamera = nGUICamera.GetComponent<UICamera>();
			}
		}
	}

	private void OnDestroy()
	{
		s_Blockers.Remove(this);
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		if (s_UiCamera == null)
		{
			Camera nGUICamera = InGameUILayout.NGUICamera;
			if ((bool)nGUICamera)
			{
				s_UiCamera = nGUICamera.GetComponent<UICamera>();
			}
		}
		Vector3 vector = s_UiCamera.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(CongealDistance, CongealDistance, 0f));
		Vector3 vector2 = s_UiCamera.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(Screen.width - CongealDistance, Screen.height - CongealDistance, 0f));
		IsLeft = Box.bounds.min.x <= vector.x;
		IsRight = Box.bounds.max.x >= vector2.x;
		IsBottom = Box.bounds.min.y <= vector.y;
		IsTop = Box.bounds.max.y >= vector2.y;
		if (!IsLeft && !IsRight && !IsBottom && !IsTop)
		{
			IsLeft = Box.bounds.center.x < 0f;
			IsRight = Box.bounds.center.x > 0f;
			IsBottom = Box.bounds.center.y < 0f;
			IsTop = Box.bounds.center.y > 0f;
		}
		if ((double)Box.bounds.size.x * 0.5 > (double)Box.bounds.size.y)
		{
			if (IsTop || IsBottom)
			{
				IsLeft = (IsRight = false);
			}
		}
		else if ((double)Box.bounds.size.y > (double)Box.bounds.size.x * 0.5 && (IsLeft || IsRight))
		{
			IsTop = (IsBottom = false);
		}
	}

	private int BumpDirectionX()
	{
		bool isLeft = IsLeft;
		bool isRight = IsRight;
		if (isLeft && isRight)
		{
			return 0;
		}
		if (isLeft)
		{
			return 1;
		}
		if (isRight)
		{
			return -1;
		}
		return 0;
	}

	private int BumpDirectionY()
	{
		bool isBottom = IsBottom;
		bool isTop = IsTop;
		if (isBottom && isTop)
		{
			return 0;
		}
		if (isBottom)
		{
			return 1;
		}
		if (isTop)
		{
			return -1;
		}
		return 0;
	}

	private float MinBumpX(Bounds world)
	{
		return Mathf.Min(IsRight ? float.MaxValue : Mathf.Abs(Box.bounds.max.x - world.max.x), IsLeft ? float.MaxValue : Mathf.Abs(world.min.x - Box.bounds.min.x));
	}

	private float MinBumpY(Bounds world)
	{
		return Mathf.Min(IsTop ? float.MaxValue : Mathf.Abs(Box.bounds.max.y - world.max.y), IsBottom ? float.MaxValue : Mathf.Abs(world.min.y - Box.bounds.min.y));
	}

	private bool Contains(Bounds world)
	{
		world.center = new Vector3(world.center.x, world.center.y, Box.bounds.center.z);
		return Box.bounds.Intersects(world);
	}

	private float ContainsRatio(Bounds world)
	{
		float num = Mathf.Max(0f, Mathf.Min(world.max.x - Box.bounds.min.x, Box.bounds.max.x - world.min.x));
		float num2 = Mathf.Max(0f, Mathf.Min(world.max.y - Box.bounds.min.y, Box.bounds.max.y - world.min.y));
		return num * num2 / (world.size.x * world.size.y);
	}

	private float ContainsRatioY(Bounds world)
	{
		return Mathf.Max(0f, Mathf.Min(world.max.y - Box.bounds.min.y, Box.bounds.max.y - world.min.y)) / world.size.y;
	}

	private Vector3 BumpOrthogonal(Bounds world, bool nox, bool noy)
	{
		if (Contains(world))
		{
			int num = BumpDirectionX();
			int num2 = BumpDirectionY();
			if (nox && num2 != 0)
			{
				num = 0;
			}
			if (noy && num != 0)
			{
				num2 = 0;
			}
			if (num == 0 && num2 == 0)
			{
				return world.center;
			}
			if (num == 0)
			{
				return BumpY(world);
			}
			if (num2 == 0)
			{
				return BumpX(world);
			}
			if (MinBumpX(world) < MinBumpY(world))
			{
				return BumpX(world);
			}
			return BumpY(world);
		}
		return world.center;
	}

	private Vector3 BumpX(Bounds world)
	{
		float num = BumpDirectionX();
		if (num > 0f)
		{
			return new Vector3(Box.bounds.max.x + world.size.x / 2f, world.center.y, world.center.z);
		}
		if (num < 0f)
		{
			return new Vector3(Box.bounds.min.x - world.size.x / 2f, world.center.y, world.center.z);
		}
		return world.center;
	}

	private Vector3 BumpY(Bounds world)
	{
		float num = BumpDirectionY();
		if (num > 0f)
		{
			return new Vector3(world.center.x, Box.bounds.max.y + world.size.y / 2f, world.center.z);
		}
		if (num < 0f)
		{
			return new Vector3(world.center.x, Box.bounds.min.y - world.size.y / 2f, world.center.z);
		}
		return world.center;
	}

	private static Bounds ApproximateNguispaceBounds(Bounds world)
	{
		if (s_UiCamera == null || s_UiCamera.GetComponent<Camera>() == null)
		{
			return default(Bounds);
		}
		Vector3 vector = s_UiCamera.GetComponent<Camera>().ScreenToWorldPoint(Camera.main.WorldToScreenPoint(world.max));
		Vector3 vector2 = s_UiCamera.GetComponent<Camera>().ScreenToWorldPoint(Camera.main.WorldToScreenPoint(world.min));
		Vector3 vector3 = s_UiCamera.GetComponent<Camera>().ScreenToWorldPoint(Camera.main.WorldToScreenPoint(new Vector3(world.min.x, world.max.y, world.min.z)));
		Vector3 vector4 = s_UiCamera.GetComponent<Camera>().ScreenToWorldPoint(Camera.main.WorldToScreenPoint(new Vector3(world.max.x, world.min.y, world.min.z)));
		float num = Mathf.Min(vector.x, vector2.x, vector3.x, vector4.x);
		float num2 = Mathf.Max(vector.x, vector2.x, vector3.x, vector4.x);
		float num3 = Mathf.Min(vector.y, vector2.y, vector3.y, vector4.y);
		float num4 = Mathf.Max(vector.y, vector2.y, vector3.y, vector4.y);
		return new Bounds(new Vector3(num2 + num, num4 + num3, 0f) / 2f, new Vector3(num2 - num, num4 - num3, 1f));
	}

	public static void Avoid(UIScreenEdgeAvoider avoider)
	{
		AvoidOrthogonal(avoider);
	}

	public static void AvoidOrthogonal(UIScreenEdgeAvoider avoider)
	{
		int num = 5;
		while (num > 0)
		{
			bool flag = false;
			foreach (UIScreenEdgeBlocker s_Blocker in s_Blockers)
			{
				if (s_Blocker.IsActive && s_Blocker.Contains(avoider.Bounds))
				{
					avoider.transform.position = s_Blocker.BumpOrthogonal(avoider.Bounds, nox: false, noy: false);
					flag = true;
				}
			}
			if (flag)
			{
				num--;
				continue;
			}
			break;
		}
	}

	public static void AvoidAnchor(UIScreenEdgeAvoider avoider)
	{
		if (!avoider.Anchor)
		{
			return;
		}
		foreach (UIScreenEdgeBlocker s_Blocker in s_Blockers)
		{
			if (s_Blocker.IsActive && s_Blocker.Contains(avoider.Bounds))
			{
				if ((bool)s_Blocker.AnchorPoint)
				{
					avoider.Anchor.enabled = true;
					avoider.Anchor.widgetContainer = s_Blocker.AnchorPoint;
					return;
				}
				avoider.transform.position = s_Blocker.BumpOrthogonal(avoider.Bounds, nox: false, noy: false);
			}
		}
		avoider.Anchor.widgetContainer = null;
		avoider.Anchor.enabled = false;
	}

	public static UIEventListener SubstituteOrthogonal(GameObject sender, Collider avoider)
	{
		Bounds world = ApproximateNguispaceBounds(avoider.bounds);
		Vector3 center = world.center;
		foreach (UIScreenEdgeBlocker s_Blocker in s_Blockers)
		{
			if (s_Blocker.IsActive)
			{
				world.center = s_Blocker.BumpOrthogonal(world, nox: false, noy: false);
			}
		}
		if (center == world.center)
		{
			if (s_Substitutes.ContainsKey(sender))
			{
				GameUtilities.Destroy(s_Substitutes[sender].gameObject);
				s_Substitutes.Remove(sender);
			}
			return null;
		}
		UIEventListener uIEventListener = null;
		if (s_Substitutes.ContainsKey(sender))
		{
			uIEventListener = s_Substitutes[sender];
		}
		else
		{
			uIEventListener = UISceneTransitionSubstitutes.Instance.Create();
			s_Substitutes[sender] = uIEventListener;
		}
		BoxCollider component = uIEventListener.GetComponent<BoxCollider>();
		if ((bool)component)
		{
			UIRoot firstUIRoot = UIRoot.GetFirstUIRoot();
			component.transform.position = s_UiCamera.GetComponent<Camera>().ScreenToWorldPoint(Camera.main.WorldToScreenPoint(avoider.bounds.center));
			component.transform.position = new Vector3(component.transform.position.x, component.transform.position.y, 8f);
			component.transform.localScale = new Vector3(world.size.x / firstUIRoot.transform.localScale.x, world.size.y / firstUIRoot.transform.localScale.y, 1f);
			if (world.center.y != center.y)
			{
				component.transform.localScale = new Vector3(component.transform.localScale.x, TransitionSubSize, 1f);
			}
			if (world.center.x != center.x)
			{
				component.transform.localScale = new Vector3(TransitionSubSize, component.transform.localScale.y, 1f);
			}
			{
				foreach (UIScreenEdgeBlocker s_Blocker2 in s_Blockers)
				{
					if (s_Blocker2.IsActive)
					{
						component.transform.position = s_Blocker2.BumpOrthogonal(component.bounds, component.size.x > component.size.y, component.size.y > component.size.x);
					}
				}
				return uIEventListener;
			}
		}
		Debug.LogError("UIScreenEdgeBlocker: substitute object has no collider.");
		return uIEventListener;
	}

	public static bool UiDoesOverlap(Collider avoider)
	{
		Bounds world = ApproximateNguispaceBounds(avoider.bounds);
		for (int i = 0; i < s_Blockers.Count; i++)
		{
			if (s_Blockers[i].IsActive && s_Blockers[i].Contains(world))
			{
				return true;
			}
		}
		return false;
	}
}
