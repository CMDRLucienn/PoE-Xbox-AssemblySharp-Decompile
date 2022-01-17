using UnityEngine;

public class NotifyPostRender : MonoBehaviour
{
	public delegate void PostRenderCamera();

	public event PostRenderCamera OnPostRenderCamera;

	private void OnPostRender()
	{
		if (this.OnPostRenderCamera != null)
		{
			this.OnPostRenderCamera();
		}
	}
}
