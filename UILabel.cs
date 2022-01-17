using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Label")]
public class UILabel : UIWidget
{
	public enum Effect
	{
		None,
		Shadow,
		Outline
	}

	public static List<UILabel> AllLabels = new List<UILabel>();

	[HideInInspector]
	[SerializeField]
	private UIFont mFont;

	[HideInInspector]
	[SerializeField]
	private string mText = "";

	[HideInInspector]
	[SerializeField]
	private int mMaxLineWidth;

	[HideInInspector]
	[SerializeField]
	private bool mEncoding = true;

	[HideInInspector]
	[SerializeField]
	private int mMaxLineCount;

	[HideInInspector]
	[SerializeField]
	private bool mPassword;

	[HideInInspector]
	[SerializeField]
	private bool mShowLastChar;

	[HideInInspector]
	[SerializeField]
	private Effect mEffectStyle;

	[HideInInspector]
	[SerializeField]
	private Color mEffectColor = Color.black;

	[HideInInspector]
	[SerializeField]
	private UIFont.SymbolStyle mSymbols = UIFont.SymbolStyle.Uncolored;

	[HideInInspector]
	[SerializeField]
	private Vector2 mEffectDistance = Vector2.one;

	[HideInInspector]
	[SerializeField]
	private bool mShrinkToFit;

	[HideInInspector]
	[SerializeField]
	private int mIndentSubsequent;

	[HideInInspector]
	[SerializeField]
	private int mIndentFirst;

	[HideInInspector]
	[SerializeField]
	private int mIndentFirstCount;

	[HideInInspector]
	[SerializeField]
	private bool mUseLnums = true;

	[HideInInspector]
	[SerializeField]
	private bool mUseLigatures = true;

	[HideInInspector]
	[SerializeField]
	private float mLineWidth;

	[HideInInspector]
	[SerializeField]
	private bool mMultiline = true;

	private bool mShouldBeProcessed = true;

	private string mProcessedText;

	private Vector3 mLastScale = Vector3.one;

	private string mLastText = "";

	private int mLastWidth;

	private bool mLastEncoding = true;

	private int mLastCount;

	private bool mLastPass;

	private bool mLastShow;

	private Effect mLastEffect;

	private Vector2 mSize = Vector2.zero;

	private bool mPremultiply;

	private static BetterList<Vector3> mTempVerts = new BetterList<Vector3>();

	private static BetterList<int> mTempIndices = new BetterList<int>();

	private bool hasChanged
	{
		get
		{
			if (!mShouldBeProcessed && !(mLastText != text) && mLastWidth == mMaxLineWidth && mLastEncoding == mEncoding && mLastCount == mMaxLineCount && mLastPass == mPassword && mLastShow == mShowLastChar)
			{
				return mLastEffect != mEffectStyle;
			}
			return true;
		}
		set
		{
			if (value)
			{
				mChanged = true;
				mShouldBeProcessed = true;
				return;
			}
			mShouldBeProcessed = false;
			mLastText = text;
			mLastWidth = mMaxLineWidth;
			mLastEncoding = mEncoding;
			mLastCount = mMaxLineCount;
			mLastPass = mPassword;
			mLastShow = mShowLastChar;
			mLastEffect = mEffectStyle;
		}
	}

	public UIFont.IndentationData IndentData => new UIFont.IndentationData(indentSubsequent, indentAmount, Mathf.CeilToInt((float)indentFirst / base.transform.localScale.y));

	public int indentSubsequent
	{
		get
		{
			return mIndentSubsequent;
		}
		set
		{
			if (mIndentSubsequent != value)
			{
				mIndentSubsequent = value;
				hasChanged = true;
			}
		}
	}

	public int indentFirst
	{
		get
		{
			return mIndentFirstCount;
		}
		set
		{
			if (mIndentFirstCount != value)
			{
				mIndentFirstCount = value;
				hasChanged = true;
			}
		}
	}

	public int indentAmount
	{
		get
		{
			return mIndentFirst;
		}
		set
		{
			if (mIndentFirst != value)
			{
				mIndentFirst = value;
				hasChanged = true;
			}
		}
	}

	public bool useLigatures
	{
		get
		{
			return mUseLigatures;
		}
		set
		{
			if (mUseLigatures != value)
			{
				mUseLigatures = value;
				hasChanged = true;
			}
		}
	}

	public bool useLnums
	{
		get
		{
			return mUseLnums;
		}
		set
		{
			if (mUseLnums != value)
			{
				mUseLnums = value;
				hasChanged = true;
			}
		}
	}

	public UIFont font
	{
		get
		{
			return mFont;
		}
		set
		{
			if (mFont != value)
			{
				mFont = value;
				material = ((mFont != null) ? mFont.material : null);
				mChanged = true;
				hasChanged = true;
				if (mFont != null && mFont.dynamicFont != null)
				{
					mFont.Request(processedText);
				}
				MarkAsChanged();
			}
		}
	}

	public string text
	{
		get
		{
			return mText;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				if (!string.IsNullOrEmpty(mText))
				{
					mText = "";
				}
				hasChanged = true;
				if (this.OnChanged != null)
				{
					this.OnChanged(value);
				}
			}
			else if (mText != value)
			{
				mText = value;
				hasChanged = true;
				ProcessText();
				if (mFont != null)
				{
					mFont.Request(processedText);
				}
				if (shrinkToFit)
				{
					MakePixelPerfect();
				}
				if (this.OnChanged != null)
				{
					this.OnChanged(value);
				}
			}
		}
	}

	public bool supportEncoding
	{
		get
		{
			return mEncoding;
		}
		set
		{
			if (mEncoding != value)
			{
				mEncoding = value;
				hasChanged = true;
				if (value)
				{
					mPassword = false;
				}
			}
		}
	}

	public UIFont.SymbolStyle symbolStyle
	{
		get
		{
			return mSymbols;
		}
		set
		{
			if (mSymbols != value)
			{
				mSymbols = value;
				hasChanged = true;
			}
		}
	}

	public int lineWidth
	{
		get
		{
			return mMaxLineWidth;
		}
		set
		{
			if (mMaxLineWidth != value)
			{
				mMaxLineWidth = value;
				hasChanged = true;
				if (shrinkToFit)
				{
					MakePixelPerfect();
				}
			}
		}
	}

	public bool multiLine
	{
		get
		{
			return mMaxLineCount != 1;
		}
		set
		{
			if (mMaxLineCount != 1 != value)
			{
				mMaxLineCount = ((!value) ? 1 : 0);
				hasChanged = true;
				if (value)
				{
					mPassword = false;
				}
			}
		}
	}

	public int maxLineCount
	{
		get
		{
			return mMaxLineCount;
		}
		set
		{
			if (mMaxLineCount != value)
			{
				mMaxLineCount = Mathf.Max(value, 0);
				hasChanged = true;
				if (value == 1)
				{
					mPassword = false;
				}
			}
		}
	}

	public bool password
	{
		get
		{
			return mPassword;
		}
		set
		{
			if (mPassword != value)
			{
				if (value)
				{
					mMaxLineCount = 1;
					mEncoding = false;
				}
				mPassword = value;
				hasChanged = true;
			}
		}
	}

	public bool showLastPasswordChar
	{
		get
		{
			return mShowLastChar;
		}
		set
		{
			if (mShowLastChar != value)
			{
				mShowLastChar = value;
				hasChanged = true;
			}
		}
	}

	public Effect effectStyle
	{
		get
		{
			return mEffectStyle;
		}
		set
		{
			if (mEffectStyle != value)
			{
				mEffectStyle = value;
				hasChanged = true;
			}
		}
	}

	public Color effectColor
	{
		get
		{
			return mEffectColor;
		}
		set
		{
			if (!mEffectColor.Equals(value))
			{
				mEffectColor = value;
				if (mEffectStyle != 0)
				{
					hasChanged = true;
				}
			}
		}
	}

	public Vector2 effectDistance
	{
		get
		{
			return mEffectDistance;
		}
		set
		{
			if (mEffectDistance != value)
			{
				mEffectDistance = value;
				hasChanged = true;
			}
		}
	}

	public bool shrinkToFit
	{
		get
		{
			return mShrinkToFit;
		}
		set
		{
			if (mShrinkToFit != value)
			{
				mShrinkToFit = value;
				hasChanged = true;
			}
		}
	}

	public string processedText
	{
		get
		{
			if (mLastScale != base.cachedTransform.localScale)
			{
				mLastScale = base.cachedTransform.localScale;
				mShouldBeProcessed = true;
			}
			if (hasChanged)
			{
				ProcessText();
			}
			return mProcessedText;
		}
	}

	public override Material material
	{
		get
		{
			Material material = base.material;
			if (material == null)
			{
				material = (this.material = ((mFont != null) ? mFont.material : null));
			}
			return material;
		}
	}

	public override Vector2 relativeSize
	{
		get
		{
			if (mFont == null)
			{
				return Vector3.one;
			}
			if (hasChanged)
			{
				ProcessText();
			}
			return mSize;
		}
	}

	public event Action<string> OnChanged;

	public static void MarkAllChanged(Font withfont)
	{
		foreach (UILabel allLabel in AllLabels)
		{
			if ((bool)allLabel && allLabel.enabled && allLabel.gameObject.activeSelf && (bool)allLabel.mFont && allLabel.mFont.dynamicFont == withfont)
			{
				allLabel.MarkAsChanged();
			}
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		MarkAsChanged();
	}

	protected override void OnDestroy()
	{
		AllLabels.Remove(this);
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	protected override void OnStart()
	{
		AllLabels.Add(this);
		if (mLineWidth > 0f)
		{
			mMaxLineWidth = Mathf.RoundToInt(mLineWidth);
			mLineWidth = 0f;
		}
		if (!mMultiline)
		{
			mMaxLineCount = 1;
			mMultiline = true;
		}
		mPremultiply = font != null && font.material != null && font.material.shader.name.Contains("Premultiplied");
		ProcessText();
		if (mFont != null)
		{
			mFont.Request(processedText);
		}
	}

	public override void MarkAsChanged()
	{
		hasChanged = true;
		base.MarkAsChanged();
	}

	private void ProcessText()
	{
		if (mFont == null)
		{
			return;
		}
		mChanged = true;
		hasChanged = false;
		mLastText = mText;
		float num = Mathf.Abs(base.cachedTransform.localScale.x);
		float num2 = mFont.size * mMaxLineCount;
		if (num > 0f)
		{
			string text = mText;
			text = font.DoLigatureReplacement(text, mUseLigatures);
			if (mUseLnums)
			{
				text = font.DoLnumReplacement(text);
			}
			while (true)
			{
				mProcessedText = WrapText(text, out var _);
				mSize = ((!string.IsNullOrEmpty(mProcessedText)) ? mFont.CalculatePrintedSize(mProcessedText, mEncoding, mSymbols) : Vector2.one);
				mSize *= mFont.pixelSize;
				if (!mShrinkToFit)
				{
					break;
				}
				if (mMaxLineCount > 0 && mSize.y * num > num2)
				{
					num = Mathf.Round(num - 1f);
					if (num > 1f)
					{
						continue;
					}
				}
				if (mMaxLineWidth > 0)
				{
					float num3 = (float)mMaxLineWidth / num;
					num = Mathf.Min((mSize.x * num > num3) ? (num3 / mSize.x * num) : num, num);
				}
				num = Mathf.Round(num);
				base.cachedTransform.localScale = new Vector3(num, num, 1f);
				break;
			}
			mSize.x = Mathf.Max(mSize.x, (num > 0f) ? ((float)lineWidth / num) : 1f);
		}
		else
		{
			mSize.x = 1f;
			num = mFont.size;
			base.cachedTransform.localScale = new Vector3(0.01f, 0.01f, 1f);
			mLastScale = base.cachedTransform.localScale;
			mProcessedText = "";
		}
		mSize.y = Mathf.Max(mSize.y, 1f);
		if (mFont != null)
		{
			mFont.Request(mProcessedText);
		}
	}

	public string WrapText(string useText, out int maxLineWidth, bool ignoreLineCount = false)
	{
		float num = Mathf.Abs(base.cachedTransform.localScale.x);
		int num2 = ((!ignoreLineCount) ? mMaxLineCount : 0);
		if (mFont == null)
		{
			Debug.LogWarning("UILabel.WrapText() - Font for label is null. Label: " + (base.gameObject ? base.gameObject.name : "NULL GameObject"));
			maxLineWidth = 0;
			return useText;
		}
		string text;
		if (mPassword)
		{
			text = "";
			if (mShowLastChar)
			{
				int i = 0;
				for (int num3 = useText.Length - 1; i < num3; i++)
				{
					text += "*";
				}
				if (useText.Length > 0)
				{
					text += useText[useText.Length - 1];
				}
			}
			else
			{
				int j = 0;
				for (int length = useText.Length; j < length; j++)
				{
					text += "*";
				}
			}
			text = mFont.WrapText(text, (float)mMaxLineWidth / num, num2, encoding: false, UIFont.SymbolStyle.None, out maxLineWidth, mShrinkToFit, IndentData);
		}
		else if (mMaxLineWidth > 0)
		{
			text = mFont.WrapText(useText, (float)mMaxLineWidth / num, num2, mEncoding, mSymbols, out maxLineWidth, mShrinkToFit, IndentData);
		}
		else if (!mShrinkToFit && num2 > 0)
		{
			text = mFont.WrapText(useText, 100000f, num2, mEncoding, mSymbols, out maxLineWidth, useAllCharacters: false, IndentData);
		}
		else
		{
			text = useText;
			maxLineWidth = 0;
		}
		return text;
	}

	public override void MakePixelPerfect()
	{
		if (mFont != null)
		{
			float pixelSize = font.pixelSize;
			Vector3 localScale = base.cachedTransform.localScale;
			localScale.x = (float)mFont.size * pixelSize;
			localScale.y = localScale.x;
			localScale.z = 1f;
			Vector3 localPosition = base.cachedTransform.localPosition;
			localPosition.x = Mathf.CeilToInt(localPosition.x / pixelSize * 4f) >> 2;
			localPosition.y = Mathf.CeilToInt(localPosition.y / pixelSize * 4f) >> 2;
			localPosition.z = Mathf.RoundToInt(localPosition.z);
			localPosition.x *= pixelSize;
			localPosition.y *= pixelSize;
			base.cachedTransform.localPosition = localPosition;
			base.cachedTransform.localScale = localScale;
			if (shrinkToFit)
			{
				ProcessText();
			}
		}
		else
		{
			base.MakePixelPerfect();
		}
	}

	private void ApplyShadow(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols, int start, int end, float x, float y)
	{
		Color color = mEffectColor;
		color.a *= base.alpha * mPanel.alpha;
		Color32 color2 = (font.premultipliedAlpha ? NGUITools.ApplyPMA(color) : color);
		for (int i = start; i < end; i++)
		{
			verts.Add(verts.buffer[i]);
			uvs.Add(uvs.buffer[i]);
			cols.Add(cols.buffer[i]);
			Vector3 vector = verts.buffer[i];
			vector.x += x;
			vector.y += y;
			verts.buffer[i] = vector;
			cols.buffer[i] = color2;
		}
	}

	public override void OnFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols)
	{
		if (mFont == null)
		{
			return;
		}
		Pivot pivot = base.pivot;
		int size = verts.size;
		Color color = base.color;
		color.a *= mPanel.alpha;
		if (font.premultipliedAlpha)
		{
			color = NGUITools.ApplyPMA(color);
		}
		UIFont.IndentationData indent = new UIFont.IndentationData(indentSubsequent, indentAmount, indentFirst);
		switch (pivot)
		{
		case Pivot.TopLeft:
		case Pivot.Left:
		case Pivot.BottomLeft:
			mFont.Print(processedText, color, verts, uvs, cols, mEncoding, mSymbols, UIFont.Alignment.Left, 0, mPremultiply, indent);
			break;
		case Pivot.TopRight:
		case Pivot.Right:
		case Pivot.BottomRight:
			mFont.Print(processedText, color, verts, uvs, cols, mEncoding, mSymbols, UIFont.Alignment.Right, Mathf.RoundToInt(relativeSize.x * (float)mFont.size), mPremultiply);
			break;
		default:
			mFont.Print(processedText, color, verts, uvs, cols, mEncoding, mSymbols, UIFont.Alignment.Center, Mathf.RoundToInt(relativeSize.x * (float)mFont.size), mPremultiply);
			break;
		}
		if (effectStyle != 0)
		{
			int size2 = verts.size;
			float num = 1f / ((float)mFont.size * mFont.pixelSize);
			float num2 = num * mEffectDistance.x;
			float num3 = num * mEffectDistance.y;
			ApplyShadow(verts, uvs, cols, size, size2, num2, 0f - num3);
			if (effectStyle == Effect.Outline)
			{
				size = size2;
				size2 = verts.size;
				ApplyShadow(verts, uvs, cols, size, size2, 0f - num2, num3);
				size = size2;
				size2 = verts.size;
				ApplyShadow(verts, uvs, cols, size, size2, num2, num3);
				size = size2;
				size2 = verts.size;
				ApplyShadow(verts, uvs, cols, size, size2, 0f - num2, 0f - num3);
			}
		}
	}

	public int GetCharacterIndexAtPosition(Vector3 worldPos)
	{
		Vector2 localPos = base.cachedTransform.InverseTransformPoint(worldPos);
		return GetCharacterIndexAtPosition(localPos);
	}

	public int GetCharacterIndexAtPosition(Vector2 localPos)
	{
		if (string.IsNullOrEmpty(processedText))
		{
			return 0;
		}
		UIFont.IndentationData indent = new UIFont.IndentationData(indentSubsequent, indentAmount, indentFirst);
		switch (base.pivot)
		{
		case Pivot.TopLeft:
		case Pivot.Left:
		case Pivot.BottomLeft:
			mFont.PrintExactCharacterPositions(processedText, mTempVerts, mTempIndices, mEncoding, mSymbols, UIFont.Alignment.Left, 0, mPremultiply, indent);
			break;
		case Pivot.TopRight:
		case Pivot.Right:
		case Pivot.BottomRight:
			mFont.PrintExactCharacterPositions(processedText, mTempVerts, mTempIndices, mEncoding, mSymbols, UIFont.Alignment.Right, Mathf.RoundToInt(relativeSize.x * (float)mFont.size), mPremultiply);
			break;
		default:
			mFont.PrintExactCharacterPositions(processedText, mTempVerts, mTempIndices, mEncoding, mSymbols, UIFont.Alignment.Center, Mathf.RoundToInt(relativeSize.x * (float)mFont.size), mPremultiply);
			break;
		}
		if (mTempVerts.size > 0)
		{
			ApplyOffset(mTempVerts, 0);
			int exactCharacterIndex = UIFont.GetExactCharacterIndex(mTempVerts, mTempIndices, localPos);
			mTempVerts.Clear();
			mTempIndices.Clear();
			return exactCharacterIndex;
		}
		return -1;
	}

	public Vector2 ApplyOffset(BetterList<Vector3> verts, int start)
	{
		Vector2 vector = base.pivotOffset;
		Vector2 vector2 = mFont.CalculatePrintedSize(processedText, mEncoding, mSymbols) * base.cachedTransform.localScale.x;
		float f = Mathf.Lerp(0f, 0f - vector2.x, 0f - vector.x);
		float f2 = Mathf.Lerp(0f, vector2.y, vector.y);
		f = Mathf.Round(f);
		f2 = Mathf.Round(f2);
		for (int i = start; i < verts.size; i++)
		{
			verts.buffer[i].x += f;
			verts.buffer[i].y += f2;
		}
		return new Vector2(f, f2);
	}

	public string GetUrlAtPosition(Vector3 worldPos)
	{
		return GetUrlAtCharacterIndex(GetCharacterIndexAtPosition(worldPos));
	}

	public string GetUrlAtPosition(Vector2 localPos)
	{
		return GetUrlAtCharacterIndex(GetCharacterIndexAtPosition(localPos));
	}

	public int GetUrlIndexAtCharacterIndex(int characterIndex)
	{
		if (characterIndex != -1 && characterIndex < mText.Length - 6)
		{
			int urlStart = GetUrlStart(mText, characterIndex);
			if (urlStart == -1)
			{
				return urlStart;
			}
			urlStart += 5;
			int num = mText.IndexOf(']', urlStart);
			if (num == -1)
			{
				return -1;
			}
			int num2 = mText.IndexOf("[/url]", num);
			if (num2 == -1 || characterIndex <= num2)
			{
				return urlStart;
			}
		}
		return -1;
	}

	public string GetUrlAtCharacterIndex(int characterIndex)
	{
		return GetUrlAtCharacterIndex(mText, characterIndex);
	}

	public static string GetUrlAtCharacterIndex(string text, int characterIndex)
	{
		if (characterIndex != -1 && characterIndex < text.Length - 6)
		{
			int urlStart = GetUrlStart(text, characterIndex);
			if (urlStart == -1)
			{
				return null;
			}
			urlStart += 5;
			int num = text.IndexOf(']', urlStart);
			if (num == -1)
			{
				return null;
			}
			int num2 = text.IndexOf("[/url]", num);
			if (num2 == -1 || characterIndex <= num2)
			{
				return text.Substring(urlStart, num - urlStart);
			}
		}
		return null;
	}

	private static int GetUrlStart(string text, int characterIndex)
	{
		if (text[characterIndex] == '[' && text[characterIndex + 1] == 'u' && text[characterIndex + 2] == 'r' && text[characterIndex + 3] == 'l' && text[characterIndex + 4] == '=')
		{
			return characterIndex;
		}
		return text.LastIndexOf("[url=", characterIndex);
	}
}
