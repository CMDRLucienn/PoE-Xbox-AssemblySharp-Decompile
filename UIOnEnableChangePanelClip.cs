using UnityEngine;

public class UIOnEnableChangePanelClip : MonoBehaviour
{
	public UIDrawCall.Clipping Clip;

	private void OnEnable()
	{
		UIPanel component = GetComponent<UIPanel>();
		if ((bool)component)
		{
			component.clipping = Clip;
		}
	}
}
