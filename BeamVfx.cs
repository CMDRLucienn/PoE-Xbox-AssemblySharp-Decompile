using UnityEngine;

public class BeamVfx : MonoBehaviour
{
	public Transform Source;

	public Transform Target;

	public Vector3 TargetPos;

	public float MaxLifetime = float.MaxValue;

	private LineRenderer[] m_LineRenderers;

	private float m_LifeTime;

	private bool m_ShuttingDown;

	public static BeamVfx Create(GameObject vfxPrefab, GenericAbility abilityOrigin, Transform source, Transform target, bool loop)
	{
		BeamVfx beamVfx = Create(vfxPrefab, abilityOrigin, source, target.position, loop);
		beamVfx.Target = target;
		return beamVfx;
	}

	public static BeamVfx Create(GameObject vfxPrefab, GenericAbility abilityOrigin, Transform source, Vector3 target, bool loop)
	{
		CalcBeamParams(source, target, out var beamPosition, out var beamOrientation, out var beamLength);
		GameObject gameObject = ((!loop) ? GameUtilities.LaunchEffect(vfxPrefab, beamPosition, beamOrientation, beamLength, null, abilityOrigin) : GameUtilities.LaunchLoopingEffect(vfxPrefab, beamPosition, beamOrientation, beamLength, null, abilityOrigin));
		if (!gameObject)
		{
			return null;
		}
		BeamVfx beamVfx = gameObject.GetComponent<BeamVfx>();
		if (!beamVfx)
		{
			beamVfx = gameObject.AddComponent<BeamVfx>();
		}
		beamVfx.Source = source;
		beamVfx.TargetPos = target;
		return beamVfx;
	}

	private void Start()
	{
		m_LineRenderers = GetComponentsInChildren<LineRenderer>();
	}

	private void Update()
	{
		m_LifeTime += Time.deltaTime;
		Vector3 vector = ((!Target) ? TargetPos : Target.position);
		CalcBeamParams(Source, vector, out var beamPosition, out var beamOrientation, out var beamLength);
		Transform obj = base.transform;
		obj.position = beamPosition;
		obj.localScale = Vector3.one * beamLength;
		obj.rotation = beamOrientation;
		if (m_LineRenderers != null)
		{
			LineRenderer[] lineRenderers = m_LineRenderers;
			foreach (LineRenderer obj2 in lineRenderers)
			{
				obj2.SetPosition(0, beamPosition);
				obj2.SetPosition(1, vector);
			}
		}
		if (!m_ShuttingDown && m_LifeTime >= MaxLifetime)
		{
			ShutDown();
		}
	}

	public void ShutDown()
	{
		GameUtilities.ShutDownLoopingEffect(base.gameObject);
		GameUtilities.Destroy(base.gameObject, 0.5f);
		m_ShuttingDown = true;
	}

	private static void CalcBeamParams(Transform source, Vector3 target, out Vector3 beamPosition, out Quaternion beamOrientation, out float beamLength)
	{
		beamPosition = source.position;
		beamOrientation = Quaternion.FromToRotation(source.forward, (target - beamPosition).normalized);
		beamLength = (target - beamPosition).magnitude;
	}
}
