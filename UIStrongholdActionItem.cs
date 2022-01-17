using UnityEngine;

public abstract class UIStrongholdActionItem : MonoBehaviour
{
	protected virtual void OnEnable()
	{
		Reload();
	}

	public abstract void Reload();
}
