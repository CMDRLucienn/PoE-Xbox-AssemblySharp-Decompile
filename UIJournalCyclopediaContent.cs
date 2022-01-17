using UnityEngine;

public class UIJournalCyclopediaContent : MonoBehaviour
{
	public UILabel TitleLable;

	public UILabel DescriptionLabel;

	public UITexture Image;

	public int MaxWidthWithImage;

	private int m_DefaultMaxWidth;

	private void Start()
	{
		m_DefaultMaxWidth = DescriptionLabel.lineWidth;
	}

	public void Load(CyclopediaEntry entry)
	{
		TitleLable.text = entry.Title.GetText();
		DescriptionLabel.text = entry.Text.GetText();
		if ((bool)entry.Image)
		{
			Image.mainTexture = entry.Image;
			Image.alpha = 1f;
			DescriptionLabel.lineWidth = MaxWidthWithImage;
		}
		else
		{
			Image.alpha = 0f;
			DescriptionLabel.lineWidth = m_DefaultMaxWidth;
		}
	}
}
