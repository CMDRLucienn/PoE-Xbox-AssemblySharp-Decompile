using UnityEngine;

public class BS_AlwaysAnimate : MonoBehaviour
{
	private Animator animator;

	private bool isAnimator;

	private void Start()
	{
		animator = GetComponent<Animator>();
		if (animator != null)
		{
			isAnimator = true;
		}
	}

	private void Update()
	{
		if (isAnimator && !animator.enabled)
		{
			animator.enabled = true;
		}
	}
}
