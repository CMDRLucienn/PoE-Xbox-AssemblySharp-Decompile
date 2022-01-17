using UnityEngine;

[AddComponentMenu("AI/Dragon Controller")]
public class DragonController : AIPackageController
{
	private Animator m_animator;

	public override void Start()
	{
		base.Start();
		m_animator = GetComponent<Animator>();
	}

	public override void Update()
	{
		base.Update();
		GameObject currentTarget = m_ai.CurrentTarget;
		if (currentTarget != null && (bool)m_animator)
		{
			float value = Vector3.Distance(currentTarget.transform.position, base.transform.position);
			m_animator.SetFloat("Target_Dist", value);
		}
	}
}
