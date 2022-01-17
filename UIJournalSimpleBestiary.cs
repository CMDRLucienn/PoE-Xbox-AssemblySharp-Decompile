using UnityEngine;

public class UIJournalSimpleBestiary : MonoBehaviour
{
	public UIDynamicLoadTexture Picture;

	public UILabel Description;

	public void Load(BestiaryParent content)
	{
		Picture.SetPath(content.Image);
		Description.text = content.Description.GetText();
	}
}
