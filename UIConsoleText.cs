using UnityEngine;

public class UIConsoleText : UITextList
{
	public float LineHeight => textLabel.transform.localScale.y + (float)textLabel.font.verticalSpacing;

	public int TotalLines => mTotalLines;

	public int ParagraphCount => mParagraphs.Count;

	public string GetParagraph(int index)
	{
		return mParagraphs[index].text;
	}

	public void Alter(int index, string text)
	{
		mParagraphs[index].text = text;
		RebuildLines(index, updateVisible: true);
	}

	public new void Clear()
	{
		base.Clear();
		mTotalLines = 0;
	}

	public void AlterColor(int index, string color)
	{
		if (index < 0 || index >= TotalLines)
		{
			return;
		}
		int num = -1;
		for (int i = 0; i < mParagraphs[index].text.Length; i++)
		{
			if (mParagraphs[index].text[i] == '[')
			{
				num = i;
			}
			else if (num >= 0 && mParagraphs[index].text[i] == ']')
			{
				mParagraphs[index].text = mParagraphs[index].text.Substring(0, num) + color + mParagraphs[index].text.Substring(i + 1);
				RebuildLines(index, updateVisible: true);
				break;
			}
		}
	}

	public int LineAt(Vector3 scrpos)
	{
		Camera nGUICamera = InGameUILayout.NGUICamera;
		Vector3 vector = base.transform.worldToLocalMatrix.MultiplyPoint3x4(nGUICamera.ScreenToWorldPoint(scrpos));
		if (vector.x < 0f || vector.x > maxWidth)
		{
			return -1;
		}
		if (textLabel.pivot == UIWidget.Pivot.Top || textLabel.pivot == UIWidget.Pivot.TopLeft || textLabel.pivot == UIWidget.Pivot.TopRight)
		{
			vector.y = 0f - vector.y;
		}
		int num = Mathf.FloorToInt(vector.y / LineHeight) + Mathf.RoundToInt(mScroll);
		int num2 = 0;
		int num3 = 0;
		if (style == Style.Chat)
		{
			for (int i = 0; i < mParagraphs.Count; i++)
			{
				num3 = num2 + mParagraphs[i].lines.Length;
				if (num >= num2 && num < num3)
				{
					if (vector.x <= (float)mParagraphs[i].maxLineWidth)
					{
						return i;
					}
					return -1;
				}
				num2 = num3;
			}
		}
		else
		{
			for (int num4 = mParagraphs.Count - 1; num4 >= 0; num4--)
			{
				num3 = num2 + mParagraphs[num4].lines.Length;
				if (num >= num2 && num < num3)
				{
					return num4;
				}
				num2 = num3;
			}
		}
		return -1;
	}

	public void RemoveLast()
	{
		if (mParagraphs.Count > 0)
		{
			mParagraphs.RemoveAt(mParagraphs.Count - 1);
		}
	}

	public void RebuildAllLines()
	{
		mTotalLines = 0;
		for (int i = 0; i < mParagraphs.Count; i++)
		{
			RebuildLines(i, updateVisible: false);
		}
		UpdateVisibleText();
	}

	protected void RebuildLines(int paragraphIndex, bool updateVisible)
	{
		UIDynamicFontSize.Guarantee(textLabel.gameObject);
		if (textLabel != null && textLabel.font != null)
		{
			int num = 0;
			for (int i = 0; i < paragraphIndex; i++)
			{
				if (mParagraphs[i] != null && mParagraphs[i].lines != null)
				{
					num += mParagraphs[i].lines.Length;
				}
			}
			UIFont.IndentationData indentData = textLabel.IndentData;
			indentData.indentFirst = Mathf.Clamp(indentData.indentFirst - num, 0, indentData.indentFirst);
			Paragraph paragraph = mParagraphs[paragraphIndex];
			paragraph.lines = textLabel.font.WrapText(paragraph.text, maxWidth / textLabel.transform.localScale.y, textLabel.maxLineCount, textLabel.supportEncoding, textLabel.symbolStyle, out paragraph.maxLineWidth, useAllCharacters: false, indentData).Split(mSeparator);
			mTotalLines = 0;
			int j = 0;
			for (int count = mParagraphs.Count; j < count; j++)
			{
				mTotalLines += mParagraphs[j].lines.Length;
			}
		}
		else
		{
			Debug.LogError("Font guarantee failed in UIConsoleText.RebuildLines");
		}
		if (updateVisible)
		{
			UpdateVisibleText();
		}
	}
}
