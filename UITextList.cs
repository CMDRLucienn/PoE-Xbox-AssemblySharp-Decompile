using System.Collections.Generic;
using System.Text;
using UnityEngine;

[AddComponentMenu("NGUI/UI/Text List")]
public class UITextList : MonoBehaviour
{
	public enum Style
	{
		Text,
		Chat
	}

	protected class Paragraph
	{
		public string text;

		public string[] lines;

		public int maxLineWidth;
	}

	[Tooltip("Style only controls the way scrolling behaves.")]
	public Style style;

	public UILabel textLabel;

	public float maxWidth;

	public float maxHeight;

	public int maxEntries = 50;

	public bool supportScrollWheel = true;

	public bool Invert;

	protected char[] mSeparator = new char[1] { '\n' };

	protected List<Paragraph> mParagraphs = new List<Paragraph>();

	protected float mScroll;

	protected bool mSelected;

	protected int mTotalLines;

	public void Clear()
	{
		mParagraphs.Clear();
		UpdateVisibleText();
	}

	public int Insert(string text, int fromEnd)
	{
		Paragraph paragraph = null;
		if (mParagraphs.Count < maxEntries)
		{
			paragraph = new Paragraph();
		}
		else
		{
			paragraph = mParagraphs[0];
			mParagraphs.RemoveAt(0);
		}
		int num = 0;
		for (int i = 0; i < mParagraphs.Count; i++)
		{
			fromEnd -= mParagraphs[mParagraphs.Count - 1 - i].lines.Length;
			if (fromEnd < 0)
			{
				num = i + 1;
				break;
			}
		}
		paragraph.text = text;
		mParagraphs.Insert(mParagraphs.Count - num, paragraph);
		UIDynamicFontSize.Guarantee(textLabel.gameObject);
		if (textLabel != null && textLabel.font != null)
		{
			int num2 = 0;
			for (int j = 0; j < mParagraphs.Count - num; j++)
			{
				if (mParagraphs[j] != null && mParagraphs[j].lines != null)
				{
					num2 += mParagraphs[j].lines.Length;
				}
			}
			UIFont.IndentationData indentData = textLabel.IndentData;
			indentData.indentFirst = Mathf.Clamp(indentData.indentFirst - num2, 0, indentData.indentFirst);
			paragraph.lines = textLabel.font.WrapText(paragraph.text, maxWidth / textLabel.transform.localScale.y, textLabel.maxLineCount, textLabel.supportEncoding, textLabel.symbolStyle, out paragraph.maxLineWidth, useAllCharacters: false, indentData).Split(mSeparator);
			mTotalLines = 0;
			int k = 0;
			for (int count = mParagraphs.Count; k < count; k++)
			{
				mTotalLines += mParagraphs[k].lines.Length;
			}
		}
		else
		{
			Debug.LogError("Font guarantee failed in UITextList.Add");
		}
		UpdateVisibleText();
		return paragraph.lines.Length;
	}

	public int Add(string text)
	{
		return Add(text, updateVisible: true);
	}

	protected int Add(string text, bool updateVisible)
	{
		Paragraph paragraph = null;
		if (mParagraphs.Count < maxEntries)
		{
			paragraph = new Paragraph();
		}
		else
		{
			paragraph = mParagraphs[0];
			mParagraphs.RemoveAt(0);
		}
		paragraph.text = text;
		mParagraphs.Add(paragraph);
		UIDynamicFontSize.Guarantee(textLabel.gameObject);
		if (textLabel != null && textLabel.font != null)
		{
			UIFont.IndentationData indentData = textLabel.IndentData;
			indentData.indentFirst = Mathf.Clamp(indentData.indentFirst - mTotalLines, 0, indentData.indentFirst);
			paragraph.lines = textLabel.font.WrapText(paragraph.text, maxWidth / textLabel.transform.localScale.y, textLabel.maxLineCount, textLabel.supportEncoding, textLabel.symbolStyle, out paragraph.maxLineWidth, useAllCharacters: false, indentData).Split(mSeparator);
			mTotalLines = 0;
			int i = 0;
			for (int count = mParagraphs.Count; i < count; i++)
			{
				mTotalLines += mParagraphs[i].lines.Length;
			}
		}
		else
		{
			Debug.LogError("Font guarantee failed in UITextList.Add");
		}
		if (updateVisible)
		{
			UpdateVisibleText();
		}
		return paragraph.lines.Length;
	}

	private void Awake()
	{
		if (textLabel == null)
		{
			textLabel = GetComponentInChildren<UILabel>();
		}
		if (textLabel != null)
		{
			textLabel.lineWidth = 0;
		}
		Collider component = GetComponent<Collider>();
		if (component != null)
		{
			if (maxHeight <= 0f)
			{
				maxHeight = component.bounds.size.y / base.transform.lossyScale.y;
			}
			if (maxWidth <= 0f)
			{
				maxWidth = component.bounds.size.x / base.transform.lossyScale.x;
			}
		}
	}

	private void OnSelect(bool selected)
	{
		mSelected = selected;
	}

	protected void UpdateVisibleText()
	{
		if (!(textLabel != null))
		{
			return;
		}
		UIDynamicFontSize.Guarantee(textLabel.gameObject);
		if (textLabel.font != null)
		{
			int num = 0;
			int num2 = ((maxHeight > 0f) ? Mathf.FloorToInt(maxHeight / textLabel.cachedTransform.localScale.y) : 100000);
			int num3 = Mathf.RoundToInt(mScroll);
			if (num2 + num3 > mTotalLines)
			{
				num3 = Mathf.Max(0, mTotalLines - num2);
				mScroll = num3;
			}
			if (style == Style.Chat)
			{
				num3 = Mathf.Max(0, mTotalLines - num2 - num3);
			}
			StringBuilder stringBuilder = new StringBuilder();
			int num4 = (Invert ? (mParagraphs.Count - 1) : 0);
			int count = mParagraphs.Count;
			while ((!Invert || num4 >= 0) && (Invert || num4 < count))
			{
				Paragraph paragraph = mParagraphs[num4];
				int i = 0;
				for (int num5 = paragraph.lines.Length; i < num5; i++)
				{
					string value = paragraph.lines[i];
					if (num3 > 0)
					{
						num3--;
						continue;
					}
					if (stringBuilder.Length > 0)
					{
						stringBuilder.Append("\n");
					}
					stringBuilder.Append(value);
					num++;
					if (num >= num2)
					{
						break;
					}
				}
				if (num >= num2)
				{
					break;
				}
				num4 = ((!Invert) ? (num4 + 1) : (num4 - 1));
			}
			textLabel.text = stringBuilder.ToString();
		}
		else
		{
			Debug.LogError("Font guarantee failed in UITextList.UpdateVisibleText");
		}
	}

	private void OnScroll(float val)
	{
		if (mSelected && supportScrollWheel)
		{
			val *= ((style == Style.Chat) ? 10f : (-10f));
			mScroll = Mathf.Max(0f, mScroll + val);
			UpdateVisibleText();
		}
	}
}
