using UnityEngine;

public class UIStrongholdStatusItem : MonoBehaviour
{
	public UILabel Label;

	public void Set(string text)
	{
		Label.text = text;
	}
}
