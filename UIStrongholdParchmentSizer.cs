using UnityEngine;

public abstract class UIStrongholdParchmentSizer : MonoBehaviour
{
	public int ParchmentNeedsReposition = 1;

	protected abstract float ContentHeight { get; }

	protected virtual void Update()
	{
		if (ParchmentNeedsReposition > 0)
		{
			ParchmentNeedsReposition--;
			UpdateParchmentSize();
		}
	}

	public void UpdateParchmentSize()
	{
		if ((bool)UIStrongholdManager.Instance && base.gameObject.activeSelf)
		{
			UIStrongholdManager.Instance.SetParchmentHeight(ContentHeight + 25f);
		}
	}
}
