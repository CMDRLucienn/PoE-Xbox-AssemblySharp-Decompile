using UnityEngine;

public class RandomStartFrame : MonoBehaviour
{
	private void Start()
	{
		Animator component = GetComponent<Animator>();
		if (!(component == null))
		{
			component.Play(component.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, OEIRandom.FloatValueInclusive());
		}
	}
}
