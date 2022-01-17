using System.Collections.Generic;
using System.Text;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Font")]
public class UIFont : MonoBehaviour
{
	public enum Alignment
	{
		Left,
		Center,
		Right
	}

	public enum SymbolStyle
	{
		None,
		Uncolored,
		Colored
	}

	public struct IndentationData
	{
		public int indentSubsequent;

		public int indentAmount;

		public int indentFirst;

		public IndentationData(int indentSubsequent, int indentAmount, int indentFirst)
		{
			this.indentSubsequent = indentSubsequent;
			this.indentAmount = indentAmount;
			this.indentFirst = indentFirst;
		}
	}

	[HideInInspector]
	[SerializeField]
	private Material mMat;

	[HideInInspector]
	[SerializeField]
	private Rect mUVRect = new Rect(0f, 0f, 1f, 1f);

	[HideInInspector]
	[SerializeField]
	private BMFont mFont = new BMFont();

	[HideInInspector]
	[SerializeField]
	private int mSpacingX;

	[HideInInspector]
	[SerializeField]
	private int mSpacingY;

	[HideInInspector]
	[SerializeField]
	private UIAtlas mAtlas;

	[HideInInspector]
	[SerializeField]
	private UIFont mReplacement;

	[HideInInspector]
	[SerializeField]
	private float mPixelSize = 1f;

	[HideInInspector]
	[SerializeField]
	private List<BMSymbol> mSymbols = new List<BMSymbol>();

	[HideInInspector]
	[SerializeField]
	private Font mDynamicFont;

	[HideInInspector]
	[SerializeField]
	private int mDynamicFontSize = 16;

	[HideInInspector]
	[SerializeField]
	private FontStyle mDynamicFontStyle;

	[HideInInspector]
	[SerializeField]
	private float mDynamicFontOffset;

	private UIAtlas.Sprite mSprite;

	private int mPMA = -1;

	private bool mSpriteSet;

	private List<Color> mColors = new List<Color>();

	protected char[] breakOn = new char[3] { ' ', '-', ',' };

	public BMFont bmFont
	{
		get
		{
			if (!(mReplacement != null))
			{
				return mFont;
			}
			return mReplacement.bmFont;
		}
	}

	public int texWidth
	{
		get
		{
			if (!(mReplacement != null))
			{
				if (mFont == null)
				{
					return 1;
				}
				return mFont.texWidth;
			}
			return mReplacement.texWidth;
		}
	}

	public int texHeight
	{
		get
		{
			if (!(mReplacement != null))
			{
				if (mFont == null)
				{
					return 1;
				}
				return mFont.texHeight;
			}
			return mReplacement.texHeight;
		}
	}

	public bool hasSymbols
	{
		get
		{
			if (!(mReplacement != null))
			{
				return mSymbols.Count != 0;
			}
			return mReplacement.hasSymbols;
		}
	}

	public List<BMSymbol> symbols
	{
		get
		{
			if (!(mReplacement != null))
			{
				return mSymbols;
			}
			return mReplacement.symbols;
		}
	}

	public UIAtlas atlas
	{
		get
		{
			if (!(mReplacement != null))
			{
				return mAtlas;
			}
			return mReplacement.atlas;
		}
		set
		{
			if (mReplacement != null)
			{
				mReplacement.atlas = value;
			}
			else
			{
				if (!(mAtlas != value))
				{
					return;
				}
				if (value == null)
				{
					if (mAtlas != null)
					{
						mMat = mAtlas.spriteMaterial;
					}
					if (sprite != null)
					{
						mUVRect = uvRect;
					}
				}
				mPMA = -1;
				mAtlas = value;
				MarkAsDirty();
			}
		}
	}

	public Material material
	{
		get
		{
			if (mReplacement != null)
			{
				return mReplacement.material;
			}
			if (mAtlas != null)
			{
				return mAtlas.spriteMaterial;
			}
			if (mMat != null)
			{
				if (mDynamicFont != null && mMat != mDynamicFont.material)
				{
					mMat.mainTexture = mDynamicFont.material.mainTexture;
				}
				return mMat;
			}
			if (mDynamicFont != null)
			{
				return mDynamicFont.material;
			}
			return null;
		}
		set
		{
			if (mReplacement != null)
			{
				mReplacement.material = value;
			}
			else if (mMat != value)
			{
				mPMA = -1;
				mMat = value;
			}
		}
	}

	public float pixelSize
	{
		get
		{
			if (mReplacement != null)
			{
				return mReplacement.pixelSize;
			}
			if (mAtlas != null)
			{
				return mAtlas.pixelSize;
			}
			return mPixelSize;
		}
		set
		{
			if (mReplacement != null)
			{
				mReplacement.pixelSize = value;
				return;
			}
			if (mAtlas != null)
			{
				mAtlas.pixelSize = value;
				return;
			}
			float num = Mathf.Clamp(value, 0.25f, 4f);
			if (mPixelSize != num)
			{
				mPixelSize = num;
				MarkAsDirty();
			}
		}
	}

	public bool premultipliedAlpha
	{
		get
		{
			if (mReplacement != null)
			{
				return mReplacement.premultipliedAlpha;
			}
			if (mAtlas != null)
			{
				return mAtlas.premultipliedAlpha;
			}
			if (mPMA == -1)
			{
				Material material = this.material;
				mPMA = ((material != null && material.shader != null && material.shader.name.Contains("Premultiplied")) ? 1 : 0);
			}
			return mPMA == 1;
		}
	}

	public Texture2D texture
	{
		get
		{
			if (mReplacement != null)
			{
				return mReplacement.texture;
			}
			Material material = this.material;
			if (!(material != null))
			{
				return null;
			}
			return material.mainTexture as Texture2D;
		}
	}

	public Rect uvRect
	{
		get
		{
			if (mReplacement != null)
			{
				return mReplacement.uvRect;
			}
			if (mAtlas != null && mSprite == null && sprite != null)
			{
				Texture texture = mAtlas.texture;
				if (texture != null)
				{
					mUVRect = mSprite.outer;
					if (mAtlas.coordinates == UIAtlas.Coordinates.Pixels)
					{
						mUVRect = NGUIMath.ConvertToTexCoords(mUVRect, texture.width, texture.height);
					}
					if (mSprite.hasPadding)
					{
						Rect rect = mUVRect;
						mUVRect.xMin = rect.xMin - mSprite.paddingLeft * rect.width;
						mUVRect.yMin = rect.yMin - mSprite.paddingBottom * rect.height;
						mUVRect.xMax = rect.xMax + mSprite.paddingRight * rect.width;
						mUVRect.yMax = rect.yMax + mSprite.paddingTop * rect.height;
					}
					if (mSprite.hasPadding)
					{
						Trim();
					}
				}
			}
			return mUVRect;
		}
		set
		{
			if (mReplacement != null)
			{
				mReplacement.uvRect = value;
			}
			else if (sprite == null && mUVRect != value)
			{
				mUVRect = value;
				MarkAsDirty();
			}
		}
	}

	public string spriteName
	{
		get
		{
			if (!(mReplacement != null))
			{
				return mFont.spriteName;
			}
			return mReplacement.spriteName;
		}
		set
		{
			if (mReplacement != null)
			{
				mReplacement.spriteName = value;
			}
			else if (mFont.spriteName != value)
			{
				mFont.spriteName = value;
				MarkAsDirty();
			}
		}
	}

	public int horizontalSpacing
	{
		get
		{
			if (!(mReplacement != null))
			{
				return mSpacingX;
			}
			return mReplacement.horizontalSpacing;
		}
		set
		{
			if (mReplacement != null)
			{
				mReplacement.horizontalSpacing = value;
			}
			else if (mSpacingX != value)
			{
				mSpacingX = value;
				MarkAsDirty();
			}
		}
	}

	public int verticalSpacing
	{
		get
		{
			if (!(mReplacement != null))
			{
				return mSpacingY;
			}
			return mReplacement.verticalSpacing;
		}
		set
		{
			if (mReplacement != null)
			{
				mReplacement.verticalSpacing = value;
			}
			else if (mSpacingY != value)
			{
				mSpacingY = value;
				MarkAsDirty();
			}
		}
	}

	public bool isValid
	{
		get
		{
			if (!(mDynamicFont != null))
			{
				return mFont.isValid;
			}
			return true;
		}
	}

	public int size
	{
		get
		{
			if (!(mReplacement != null))
			{
				if (!isDynamic)
				{
					return mFont.charSize;
				}
				return mDynamicFontSize;
			}
			return mReplacement.size;
		}
	}

	public UIAtlas.Sprite sprite
	{
		get
		{
			if (mReplacement != null)
			{
				return mReplacement.sprite;
			}
			if (!mSpriteSet)
			{
				mSprite = null;
			}
			if (mSprite == null)
			{
				if (mAtlas != null && !string.IsNullOrEmpty(mFont.spriteName))
				{
					mSprite = mAtlas.GetSprite(mFont.spriteName);
					if (mSprite == null)
					{
						mSprite = mAtlas.GetSprite(base.name);
					}
					mSpriteSet = true;
					if (mSprite == null)
					{
						mFont.spriteName = null;
					}
				}
				int i = 0;
				for (int count = mSymbols.Count; i < count; i++)
				{
					symbols[i].MarkAsDirty();
				}
			}
			return mSprite;
		}
	}

	public UIFont replacement
	{
		get
		{
			return mReplacement;
		}
		set
		{
			UIFont uIFont = value;
			if (uIFont == this)
			{
				uIFont = null;
			}
			if (mReplacement != uIFont)
			{
				if (uIFont != null && uIFont.replacement == this)
				{
					uIFont.replacement = null;
				}
				if (mReplacement != null)
				{
					MarkAsDirty();
				}
				mReplacement = uIFont;
				MarkAsDirty();
			}
		}
	}

	public bool isDynamic => mDynamicFont != null;

	public Font dynamicFont
	{
		get
		{
			if (!(mReplacement != null))
			{
				return mDynamicFont;
			}
			return mReplacement.dynamicFont;
		}
		set
		{
			if (mReplacement != null)
			{
				mReplacement.dynamicFont = value;
			}
			else if (mDynamicFont != value)
			{
				mDynamicFont = value;
			}
		}
	}

	public int dynamicFontSize
	{
		get
		{
			if (!(mReplacement != null))
			{
				return mDynamicFontSize;
			}
			return mReplacement.dynamicFontSize;
		}
		set
		{
			if (mReplacement != null)
			{
				mReplacement.dynamicFontSize = value;
				return;
			}
			value = Mathf.Clamp(value, 4, 256);
			if (mDynamicFontSize != value)
			{
				mDynamicFontSize = value;
			}
		}
	}

	public FontStyle dynamicFontStyle
	{
		get
		{
			if (!(mReplacement != null))
			{
				return mDynamicFontStyle;
			}
			return mReplacement.dynamicFontStyle;
		}
		set
		{
			if (mReplacement != null)
			{
				mReplacement.dynamicFontStyle = value;
			}
			else if (mDynamicFontStyle != value)
			{
				mDynamicFontStyle = value;
			}
		}
	}

	private Texture dynamicTexture
	{
		get
		{
			if ((bool)mReplacement)
			{
				return mReplacement.dynamicTexture;
			}
			if (isDynamic)
			{
				return mDynamicFont.material.mainTexture;
			}
			return null;
		}
	}

	protected int requestSize => Mathf.RoundToInt((float)mDynamicFontSize * (UIDynamicFontManager.Instance ? UIDynamicFontManager.Instance.NguiAdjustment : 1f));

	private void Trim()
	{
		Texture texture = mAtlas.texture;
		if (texture != null && mSprite != null)
		{
			Rect rect = NGUIMath.ConvertToPixels(mUVRect, this.texture.width, this.texture.height, round: true);
			Rect rect2 = ((mAtlas.coordinates == UIAtlas.Coordinates.TexCoords) ? NGUIMath.ConvertToPixels(mSprite.outer, texture.width, texture.height, round: true) : mSprite.outer);
			int xMin = Mathf.RoundToInt(rect2.xMin - rect.xMin);
			int yMin = Mathf.RoundToInt(rect2.yMin - rect.yMin);
			int xMax = Mathf.RoundToInt(rect2.xMax - rect.xMin);
			int yMax = Mathf.RoundToInt(rect2.yMax - rect.yMin);
			mFont.Trim(xMin, yMin, xMax, yMax);
		}
	}

	private bool References(UIFont font)
	{
		if (font == null)
		{
			return false;
		}
		if (font == this)
		{
			return true;
		}
		if (!(mReplacement != null))
		{
			return false;
		}
		return mReplacement.References(font);
	}

	public static bool CheckIfRelated(UIFont a, UIFont b)
	{
		if (a == null || b == null)
		{
			return false;
		}
		if (a.isDynamic && b.isDynamic && a.dynamicFont.fontNames[0] == b.dynamicFont.fontNames[0])
		{
			return true;
		}
		if (!(a == b) && !a.References(b))
		{
			return b.References(a);
		}
		return true;
	}

	public void MarkAsDirty()
	{
		if (mReplacement != null)
		{
			mReplacement.MarkAsDirty();
		}
		mSprite = null;
		UILabel[] array = NGUITools.FindActive<UILabel>();
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			UILabel uILabel = array[i];
			if (uILabel.enabled && NGUITools.GetActive(uILabel.gameObject) && CheckIfRelated(this, uILabel.font))
			{
				UIFont font = uILabel.font;
				uILabel.font = null;
				uILabel.font = font;
			}
		}
		int j = 0;
		for (int count = mSymbols.Count; j < count; j++)
		{
			symbols[j].MarkAsDirty();
		}
	}

	public void Request(string text)
	{
		if (!string.IsNullOrEmpty(text))
		{
			if (mReplacement != null)
			{
				mReplacement.Request(text);
			}
			else if (mDynamicFont != null)
			{
				mDynamicFont.RequestCharactersInTexture("j", requestSize, mDynamicFontStyle);
				MyGetCharacterInfo(mDynamicFont, 'j', out var info);
				mDynamicFontOffset = mDynamicFontSize + info.minY;
				mDynamicFont.RequestCharactersInTexture(text, requestSize, mDynamicFontStyle);
			}
		}
	}

	public string DoLigatureReplacement(string text, bool enableDiscretionary)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}
		UIFontLigatures component = GetComponent<UIFontLigatures>();
		if ((bool)component)
		{
			UIFontLigatures.Mapping[] mappings = component.Mappings;
			foreach (UIFontLigatures.Mapping mapping in mappings)
			{
				if (enableDiscretionary && (UIFontLigatures.LigaturesEnabled || !mapping.Discretionary))
				{
					text = text.ReplaceEncodingSafe(mapping.From, mapping.ToCharAsString);
				}
			}
		}
		return text;
	}

	public string DoLnumReplacement(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}
		UIFontLNums component = GetComponent<UIFontLNums>();
		StringBuilder stringBuilder = new StringBuilder(text);
		if ((bool)component)
		{
			for (int i = 0; i <= 9; i++)
			{
				char c = (char)(48 + i);
				char newChar = (char)(component.Start + i);
				bool flag = false;
				for (int num = text.Length - 1; num >= 0; num--)
				{
					if (text[num] == ']')
					{
						flag = true;
					}
					else if (text[num] == '[')
					{
						flag = false;
					}
					else if (!flag && text[num] == c)
					{
						stringBuilder.Replace(c, newChar, num, 1);
					}
				}
			}
		}
		return stringBuilder.ToString();
	}

	public Vector2 CalculatePrintedSize(string text, bool encoding, SymbolStyle symbolStyle)
	{
		if (mReplacement != null)
		{
			return mReplacement.CalculatePrintedSize(text, encoding, symbolStyle);
		}
		Vector2 zero = Vector2.zero;
		bool flag = isDynamic;
		if (flag || (mFont != null && mFont.isValid && !string.IsNullOrEmpty(text)))
		{
			if (encoding)
			{
				text = NGUITools.StripSymbols(text);
			}
			if (mDynamicFont != null)
			{
				mDynamicFont.RequestCharactersInTexture(text, requestSize);
			}
			int length = text.Length;
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			int num4 = 0;
			int num5 = size;
			int num6 = num5 + mSpacingY;
			bool flag2 = encoding && symbolStyle != 0 && hasSymbols;
			for (int i = 0; i < length; i++)
			{
				char c = text[i];
				if (c == '\n')
				{
					if (num > num3)
					{
						num3 = num;
					}
					num = 0f;
					num2 += (float)num6;
					num4 = 0;
				}
				else if (c < ' ')
				{
					num4 = 0;
				}
				else if (!flag)
				{
					BMSymbol bMSymbol = (flag2 ? MatchSymbol(text, i, length) : null);
					if (bMSymbol == null)
					{
						BMGlyph glyph = mFont.GetGlyph(c);
						if (glyph != null)
						{
							num += (float)(mSpacingX + ((num4 != 0) ? (glyph.advance + glyph.GetKerning(num4)) : glyph.advance));
							num4 = c;
						}
					}
					else
					{
						num += (float)(mSpacingX + bMSymbol.width);
						i += bMSymbol.length - 1;
						num4 = 0;
					}
				}
				else
				{
					num += (float)mSpacingX + GetCharacterWidth(mDynamicFont, c);
				}
			}
			float num7 = ((num5 > 0) ? (1f / (float)num5) : 1f);
			zero.x = num7 * ((num > num3) ? num : num3);
			zero.y = num7 * (num2 + (float)num6);
		}
		return zero;
	}

	private static void EndLine(ref StringBuilder s)
	{
		int num = s.Length - 1;
		if (num > 0 && s[num] == ' ')
		{
			s[num] = '\n';
		}
		else
		{
			s.Append('\n');
		}
		s.Append('\r');
	}

	public string GetEndOfLineThatFits(string text, float maxWidth, bool encoding, SymbolStyle symbolStyle)
	{
		if (mReplacement != null)
		{
			return mReplacement.GetEndOfLineThatFits(text, maxWidth, encoding, symbolStyle);
		}
		int num = Mathf.RoundToInt(maxWidth * (float)size);
		if (num < 1)
		{
			return text;
		}
		if (mDynamicFont != null)
		{
			mDynamicFont.RequestCharactersInTexture(text, requestSize);
		}
		int length = text.Length;
		float num2 = num;
		BMGlyph bMGlyph = null;
		int num3 = length;
		bool flag = encoding && symbolStyle != 0 && hasSymbols;
		bool flag2 = isDynamic;
		while (num3 > 0 && num2 > 0f)
		{
			char c = text[--num3];
			BMSymbol bMSymbol = (flag ? MatchSymbol(text, num3, length) : null);
			float num4 = mSpacingX;
			if (!flag2)
			{
				if (bMSymbol != null)
				{
					num4 += (float)bMSymbol.advance;
				}
				else
				{
					BMGlyph glyph = mFont.GetGlyph(c);
					if (glyph == null)
					{
						bMGlyph = null;
						continue;
					}
					num4 += (float)(glyph.advance + (bMGlyph?.GetKerning(c) ?? 0));
					bMGlyph = glyph;
				}
			}
			else
			{
				num4 += GetCharacterWidth(mDynamicFont, c);
			}
			num2 -= num4;
		}
		if (num2 < 0f)
		{
			num3++;
		}
		return text.Substring(num3, length - num3);
	}

	public string WrapText(string text, float maxWidth, int maxLineCount, bool encoding, SymbolStyle symbolStyle)
	{
		int maxLineWidth;
		return WrapText(text, maxWidth, maxLineCount, encoding, symbolStyle, out maxLineWidth);
	}

	public string WrapText(string text, float maxWidth, int maxLineCount, bool encoding, SymbolStyle symbolStyle, out int maxLineWidth)
	{
		return WrapText(text, maxWidth, maxLineCount, encoding, symbolStyle, out maxLineWidth, useAllCharacters: false, default(IndentationData));
	}

	public string WrapText(string text, float maxWidth, int maxLineCount, bool encoding, SymbolStyle symbolStyle, out int maxLineWidth, bool useAllCharacters, IndentationData indentData)
	{
		return WrapText(text, maxWidth, maxLineCount, encoding, symbolStyle, out maxLineWidth, useAllCharacters, indentData, ellipsis: true);
	}

	public string WrapText(string text, float maxWidth, int maxLineCount, bool encoding, SymbolStyle symbolStyle, out int maxLineWidth, bool useAllCharacters, IndentationData indentData, bool ellipsis)
	{
		maxLineWidth = 0;
		if (mReplacement != null)
		{
			return mReplacement.WrapText(text, maxWidth, maxLineCount, encoding, symbolStyle, out maxLineWidth, useAllCharacters, indentData, ellipsis);
		}
		int num = Mathf.RoundToInt(maxWidth * (float)size);
		if (num < 1)
		{
			return text;
		}
		if (mDynamicFont != null)
		{
			mDynamicFont.RequestCharactersInTexture(text, requestSize);
		}
		StringBuilder s = new StringBuilder();
		int length = text.Length;
		float num2 = num;
		int num3 = 0;
		int i = 0;
		int j = 0;
		bool flag = true;
		bool flag2 = maxLineCount != 1;
		int num4 = 1;
		bool flag3 = encoding && symbolStyle != 0 && hasSymbols;
		bool flag4 = isDynamic;
		string value;
		CharacterInfo info;
		if (!flag4)
		{
			if (mFont.GetGlyph(8230) != null)
			{
				value = '…'.ToString();
			}
			else
			{
				mFont.GetGlyph(46);
				value = "...";
			}
		}
		else if (MyGetCharacterInfo(mDynamicFont, '…', out info))
		{
			value = '…'.ToString();
		}
		else
		{
			MyGetCharacterInfo(mDynamicFont, '.', out info);
			value = "...";
		}
		if (indentData.indentFirst > 0)
		{
			num2 -= (float)indentData.indentAmount;
		}
		for (; j < length; j++)
		{
			char c = text[j];
			if (c == '\n')
			{
				if (!flag2 || num4 == maxLineCount)
				{
					break;
				}
				num2 = num;
				if (i < j)
				{
					s.Append(text.Substring(i, j - i + 1));
				}
				else
				{
					s.Append(c);
				}
				flag = true;
				num4++;
				i = j + 1;
				num3 = 0;
				if (num4 <= indentData.indentFirst)
				{
					num2 -= (float)indentData.indentAmount;
				}
				continue;
			}
			if (c == '\r')
			{
				num2 -= (float)indentData.indentSubsequent;
			}
			bool flag5 = false;
			bool flag6 = false;
			for (int num5 = breakOn.Length - 1; num5 >= 0; num5--)
			{
				if (c == breakOn[num5])
				{
					flag5 = true;
				}
				if (num3 == breakOn[num5])
				{
					flag6 = true;
				}
			}
			if (flag5 && !flag6 && i < j)
			{
				s.Append(text.Substring(i, j - i + 1));
				flag = false;
				i = j + 1;
				num3 = c;
			}
			int index = j;
			if (encoding && c == '[' && NGUITools.ParseSymbol(text, ref index, null, premultiply: false) != 0 && index > 0)
			{
				j += index - 1;
				continue;
			}
			BMSymbol bMSymbol = (flag3 ? MatchSymbol(text, j, length) : null);
			float num6 = mSpacingX;
			if (!flag4)
			{
				if (bMSymbol != null)
				{
					num6 += (float)bMSymbol.advance;
				}
				else
				{
					BMGlyph bMGlyph = ((bMSymbol == null) ? mFont.GetGlyph(c) : null);
					if (bMGlyph == null)
					{
						continue;
					}
					num6 += (float)((num3 != 0) ? (bMGlyph.advance + bMGlyph.GetKerning(num3)) : bMGlyph.advance);
				}
			}
			else
			{
				num6 += GetCharacterWidth(mDynamicFont, c);
			}
			num2 -= num6;
			if (num2 < 0f)
			{
				if (!flag && flag2 && num4 != maxLineCount)
				{
					for (; i < length && text[i] == ' '; i++)
					{
					}
					flag = true;
					num2 = num;
					j = i - 1;
					num3 = 0;
					if (!flag2 || num4 == maxLineCount)
					{
						break;
					}
					num4++;
					EndLine(ref s);
					num2 = ((num4 > indentData.indentFirst) ? (num2 - (float)indentData.indentSubsequent) : (num2 - (float)indentData.indentAmount));
					continue;
				}
				if (!useAllCharacters && ellipsis)
				{
					s.TrimEnd();
					s.Append(value);
				}
				else
				{
					s.Append(text.Substring(i, Mathf.Max(0, j - i)));
				}
				if (!flag2 || num4 == maxLineCount)
				{
					i = j;
					break;
				}
				EndLine(ref s);
				flag = true;
				num4++;
				if (c == ' ')
				{
					i = j + 1;
					num2 = num;
				}
				else
				{
					i = j;
					num2 = (float)num - num6;
				}
				num3 = 0;
				num2 = ((num4 > indentData.indentFirst) ? (num2 - (float)indentData.indentSubsequent) : (num2 - (float)indentData.indentAmount));
			}
			else
			{
				num3 = c;
			}
			if (!flag4 && bMSymbol != null)
			{
				j += bMSymbol.length - 1;
				num3 = 0;
			}
		}
		if (num4 > 1)
		{
			maxLineWidth = num;
		}
		else
		{
			maxLineWidth = (int)((float)num - num2);
		}
		if (i < j)
		{
			s.Append(text.Substring(i, j - i));
		}
		if (useAllCharacters)
		{
			i = j;
			if (i < text.Length)
			{
				s.Append(text.Substring(i, text.Length - i));
			}
		}
		return s.ToString();
	}

	private float GetCharacterWidth(Font font, char character)
	{
		if (MyGetCharacterInfo(font, character, out var info))
		{
			return info.advance;
		}
		return 0f;
	}

	private bool MyGetCharacterInfo(Font font, char character, out CharacterInfo info)
	{
		bool flag = false;
		if (font.GetCharacterInfo(character, out info, requestSize, mDynamicFontStyle))
		{
			flag = true;
		}
		else
		{
			CharacterInfo[] characterInfo = font.characterInfo;
			for (int i = 0; i < characterInfo.Length; i++)
			{
				CharacterInfo characterInfo2 = characterInfo[i];
				if (characterInfo2.index == character)
				{
					info = characterInfo2;
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			float num = (UIDynamicFontManager.Instance ? UIDynamicFontManager.Instance.NguiAdjustment : 1f);
			info.size = Mathf.RoundToInt((float)mDynamicFontSize / num);
			info.advance = (int)((float)info.advance / num);
			Rect rect = new Rect(info.minX, info.minY, info.maxX, info.maxY);
			rect.x /= num;
			rect.y /= num;
			rect.width /= num;
			rect.height /= num;
			info.minX = (int)rect.x;
			info.minY = (int)rect.y;
			info.maxX = (int)rect.width;
			info.maxY = (int)rect.height;
		}
		return flag;
	}

	public string WrapText(string text, float maxWidth, int maxLineCount, bool encoding)
	{
		return WrapText(text, maxWidth, maxLineCount, encoding, SymbolStyle.None);
	}

	public string WrapText(string text, float maxWidth, int maxLineCount)
	{
		return WrapText(text, maxWidth, maxLineCount, encoding: false, SymbolStyle.None);
	}

	private void Align(BetterList<Vector3> verts, int indexOffset, Alignment alignment, float x, int lineWidth, float offsetFactor)
	{
		if (alignment == Alignment.Left)
		{
			return;
		}
		int num = size;
		if (num <= 0)
		{
			return;
		}
		float num2 = 0f;
		if (alignment == Alignment.Right)
		{
			num2 = Mathf.RoundToInt((float)lineWidth - x);
			if (num2 < 0f)
			{
				num2 = 0f;
			}
			num2 /= offsetFactor;
		}
		else
		{
			num2 = Mathf.RoundToInt(((float)lineWidth - x) * 0.5f);
			if (num2 < 0f)
			{
				num2 = 0f;
			}
			num2 /= offsetFactor;
			if ((lineWidth & 1) == 1)
			{
				num2 += 0.5f / (float)num;
			}
		}
		for (int i = indexOffset; i < verts.size; i++)
		{
			Vector3 vector = verts.buffer[i];
			vector.x += num2;
			verts.buffer[i] = vector;
		}
	}

	public void Print(string text, Color32 color, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols, bool encoding, SymbolStyle symbolStyle, Alignment alignment, int lineWidth, bool premultiply)
	{
		Print(text, color, verts, uvs, cols, encoding, symbolStyle, alignment, lineWidth, premultiply, default(IndentationData));
	}

	public void Print(string text, Color32 color, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols, bool encoding, SymbolStyle symbolStyle, Alignment alignment, int lineWidth, bool premultiply, IndentationData indent)
	{
		if (mReplacement != null)
		{
			mReplacement.Print(text, color, verts, uvs, cols, encoding, symbolStyle, alignment, lineWidth, premultiply, indent);
		}
		else
		{
			if (text == null)
			{
				return;
			}
			if (!isValid)
			{
				Debug.LogError("Attempting to print using an invalid font!");
				return;
			}
			if (mDynamicFont != null)
			{
				mDynamicFont.RequestCharactersInTexture(text, requestSize);
			}
			bool flag = isDynamic;
			mColors.Clear();
			mColors.Add(color);
			int num = size;
			Vector2 vector = ((num > 0) ? new Vector2(1f / (float)num, 1f / (float)num) : Vector2.one);
			int num2 = verts.size;
			float num3 = 0f;
			float num4 = 0f;
			float num5 = 0f;
			int num6 = 0;
			int num7 = num + mSpacingY;
			Vector3 zero = Vector3.zero;
			Vector3 zero2 = Vector3.zero;
			Vector2 zero3 = Vector2.zero;
			Vector2 zero4 = Vector2.zero;
			float num8 = uvRect.width / (float)mFont.texWidth;
			float num9 = mUVRect.height / (float)mFont.texHeight;
			int length = text.Length;
			bool flag2 = encoding && symbolStyle != 0 && hasSymbols && sprite != null;
			if (indent.indentFirst > 0)
			{
				num3 += (float)indent.indentAmount;
			}
			for (int i = 0; i < length; i++)
			{
				char c = text[i];
				if (c == '\n')
				{
					if (num3 > num5)
					{
						num5 = num3;
					}
					if (alignment != 0)
					{
						Align(verts, num2, alignment, num3, lineWidth, num);
						num2 = verts.size;
					}
					num3 = 0f;
					num4 += (float)num7;
					num6 = 0;
					if (num4 < (float)indent.indentFirst)
					{
						num3 += (float)indent.indentAmount;
					}
					continue;
				}
				if (c == '\r')
				{
					num3 += (float)indent.indentSubsequent;
					continue;
				}
				if (c < ' ')
				{
					num6 = 0;
					continue;
				}
				if (encoding && c == '[')
				{
					int index = i;
					NGUITools.ParseSymbol(text, ref index, mColors, premultiply);
					if (index > 0)
					{
						color = mColors[mColors.Count - 1];
						i += index - 1;
						continue;
					}
				}
				CharacterInfo info;
				if (!flag)
				{
					BMSymbol bMSymbol = (flag2 ? MatchSymbol(text, i, length) : null);
					if (bMSymbol == null)
					{
						BMGlyph glyph = mFont.GetGlyph(c);
						if (glyph == null)
						{
							continue;
						}
						if (num6 != 0)
						{
							num3 += (float)glyph.GetKerning(num6);
						}
						zero.x = vector.x * (num3 + (float)glyph.offsetX);
						zero.y = (0f - vector.y) * (num4 + (float)glyph.offsetY);
						zero2.x = zero.x + vector.x * (float)glyph.width;
						zero2.y = zero.y - vector.y * (float)glyph.height;
						zero3.x = mUVRect.xMin + num8 * (float)glyph.x;
						zero3.y = mUVRect.yMax - num9 * (float)glyph.y;
						zero4.x = zero3.x + num8 * (float)glyph.width;
						zero4.y = zero3.y - num9 * (float)glyph.height;
						num3 += (float)(mSpacingX + glyph.advance);
						num6 = c;
						if (glyph.channel == 0 || glyph.channel == 15)
						{
							for (int j = 0; j < 4; j++)
							{
								cols.Add(color);
							}
						}
						else
						{
							Color color2 = color;
							color2 *= 0.49f;
							switch (glyph.channel)
							{
							case 1:
								color2.b += 0.51f;
								break;
							case 2:
								color2.g += 0.51f;
								break;
							case 4:
								color2.r += 0.51f;
								break;
							case 8:
								color2.a += 0.51f;
								break;
							}
							for (int k = 0; k < 4; k++)
							{
								cols.Add(color2);
							}
						}
					}
					else
					{
						zero.x = vector.x * (num3 + (float)bMSymbol.offsetX);
						zero.y = (0f - vector.y) * (num4 + (float)bMSymbol.offsetY);
						zero2.x = zero.x + vector.x * (float)bMSymbol.width;
						zero2.y = zero.y - vector.y * (float)bMSymbol.height;
						Rect rect = bMSymbol.uvRect;
						zero3.x = rect.xMin;
						zero3.y = rect.yMax;
						zero4.x = rect.xMax;
						zero4.y = rect.yMin;
						num3 += (float)(mSpacingX + bMSymbol.advance);
						i += bMSymbol.length - 1;
						num6 = 0;
						if (symbolStyle == SymbolStyle.Colored)
						{
							for (int l = 0; l < 4; l++)
							{
								cols.Add(color);
							}
						}
						else
						{
							Color32 item = Color.white;
							item.a = color.a;
							for (int m = 0; m < 4; m++)
							{
								cols.Add(item);
							}
						}
					}
					verts.Add(new Vector3(zero2.x, zero.y));
					verts.Add(new Vector3(zero2.x, zero2.y));
					verts.Add(new Vector3(zero.x, zero2.y));
					verts.Add(new Vector3(zero.x, zero.y));
					uvs.Add(new Vector2(zero4.x, zero3.y));
					uvs.Add(new Vector2(zero4.x, zero4.y));
					uvs.Add(new Vector2(zero3.x, zero4.y));
					uvs.Add(new Vector2(zero3.x, zero3.y));
				}
				else if (MyGetCharacterInfo(mDynamicFont, c, out info))
				{
					zero.x = vector.x * (num3 + (float)info.minX);
					zero.y = (0f - vector.y) * (num4 - (float)info.minY + mDynamicFontOffset);
					zero2.x = zero.x + vector.x * (float)info.glyphWidth;
					zero2.y = zero.y - vector.y * (float)(-info.glyphHeight);
					zero3.x = info.uvBottomLeft.x;
					zero3.y = info.uvBottomLeft.y;
					zero4.x = info.uvTopRight.x;
					zero4.y = info.uvTopRight.y;
					num3 += (float)(mSpacingX + info.advance);
					for (int n = 0; n < 4; n++)
					{
						cols.Add(color);
					}
					uvs.Add(info.uvBottomRight);
					uvs.Add(info.uvBottomLeft);
					uvs.Add(info.uvTopLeft);
					uvs.Add(info.uvTopRight);
					verts.Add(new Vector3(zero2.x, zero.y));
					verts.Add(new Vector3(zero.x, zero.y));
					verts.Add(new Vector3(zero.x, zero2.y));
					verts.Add(new Vector3(zero2.x, zero2.y));
				}
			}
			if (alignment != 0 && num2 < verts.size)
			{
				Align(verts, num2, alignment, num3, lineWidth, num);
				num2 = verts.size;
			}
		}
	}

	public void PrintExactCharacterPositions(string text, BetterList<Vector3> verts, BetterList<int> indices, bool encoding, SymbolStyle symbolStyle, Alignment alignment, int lineWidth, bool premultiply)
	{
		PrintExactCharacterPositions(text, verts, indices, encoding, symbolStyle, alignment, lineWidth, premultiply, default(IndentationData));
	}

	public void PrintExactCharacterPositions(string text, BetterList<Vector3> verts, BetterList<int> indices, bool encoding, SymbolStyle symbolStyle, Alignment alignment, int lineWidth, bool premultiply, IndentationData indent)
	{
		if (string.IsNullOrEmpty(text))
		{
			text = " ";
		}
		if (mDynamicFont != null)
		{
			mDynamicFont.RequestCharactersInTexture(text, requestSize);
		}
		bool flag = isDynamic;
		int num = size;
		float num2 = num + mSpacingY;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 0f;
		if (num4 < (float)indent.indentFirst)
		{
			num3 = indent.indentAmount;
		}
		int length = text.Length;
		int num6 = verts.size;
		int num7 = 0;
		bool flag2 = encoding && symbolStyle != 0 && hasSymbols && sprite != null;
		int num8 = 0;
		for (int i = 0; i < length; i++)
		{
			char c = text[i];
			if (c == '\n')
			{
				if (num3 > num5)
				{
					num5 = num3;
				}
				if (alignment != 0)
				{
					Align(verts, num6, alignment, num3, lineWidth, 1f);
					num6 = verts.size;
				}
				num3 = 0f;
				num4 += num2;
				num7 = 0;
				if (num4 < (float)indent.indentFirst)
				{
					num3 += (float)indent.indentAmount;
				}
			}
			else if (c == '\r')
			{
				num3 += (float)indent.indentSubsequent;
				num8++;
			}
			else if (c < ' ')
			{
				num7 = 0;
			}
			else if (encoding && c == '[')
			{
				int index = i;
				NGUITools.ParseSymbol(text, ref index, mColors, premultiply);
				if (index > 0)
				{
					i += index - 1;
				}
			}
			else if (!flag)
			{
				BMSymbol bMSymbol = (flag2 ? MatchSymbol(text, i, length) : null);
				if (bMSymbol == null)
				{
					BMGlyph glyph = mFont.GetGlyph(c);
					if (glyph != null)
					{
						if (num7 != 0)
						{
							num3 += (float)glyph.GetKerning(num7);
						}
						float num9 = glyph.width + mSpacingX;
						indices.Add(i);
						verts.Add(new Vector3((int)num3, 0f - num4 - (float)num));
						verts.Add(new Vector3((int)(num3 + num9), 0f - num4));
						num3 += (float)(mSpacingX + glyph.advance);
						num7 = c;
					}
				}
				else
				{
					int num10 = bMSymbol.advance * num + lineWidth;
					indices.Add(i);
					verts.Add(new Vector3((int)num3, 0f - num4 - (float)num));
					verts.Add(new Vector3((int)(num3 + (float)num10), 0f - num4));
					i += bMSymbol.sequence.Length - 1;
					num3 += (float)num10;
					num7 = 0;
				}
			}
			else
			{
				float characterWidth = GetCharacterWidth(mDynamicFont, c);
				if (characterWidth != 0f)
				{
					float num11 = characterWidth + (float)mSpacingX;
					indices.Add(i - num8);
					verts.Add(new Vector3((int)num3, 0f - num4 - (float)num));
					verts.Add(new Vector3((int)(num3 + num11), 0f - num4));
					num3 += num11;
					num7 = c;
				}
			}
		}
		if (alignment != 0 && num6 < verts.size)
		{
			Align(verts, num6, alignment, num3, lineWidth, 1f);
			num6 = verts.size;
		}
	}

	public static int GetExactCharacterIndex(BetterList<Vector3> verts, BetterList<int> indices, Vector2 pos)
	{
		for (int i = 0; i < indices.size; i++)
		{
			int num = i << 1;
			int i2 = num + 1;
			float x = verts[num].x;
			if (pos.x < x)
			{
				continue;
			}
			float x2 = verts[i2].x;
			if (pos.x > x2)
			{
				continue;
			}
			float y = verts[num].y;
			if (!(pos.y < y))
			{
				float y2 = verts[i2].y;
				if (!(pos.y > y2))
				{
					return indices[i];
				}
			}
		}
		return -1;
	}

	public static int GetCharacterCursorIndex(BetterList<Vector3> verts, BetterList<int> indices, Vector2 pos)
	{
		if (verts == null || indices == null || verts.size == 0 || indices.size == 0 || verts.size != indices.size * 2)
		{
			return -1;
		}
		for (int i = 0; i < indices.size; i++)
		{
			int num = i << 1;
			int i2 = num + 1;
			float x = verts[num].x;
			if (pos.x < x)
			{
				continue;
			}
			float x2 = verts[i2].x;
			if (pos.x > x2)
			{
				continue;
			}
			float y = verts[num].y;
			if (pos.y < y)
			{
				continue;
			}
			float y2 = verts[i2].y;
			if (!(pos.y > y2))
			{
				if (pos.x < x + (x2 - x) / 2f)
				{
					return indices[i];
				}
				return indices[i] + 1;
			}
		}
		return -1;
	}

	public static Bounds GetBoundsOfCharacterAtIndex(BetterList<Vector3> verts, BetterList<int> indices, int characterIndex)
	{
		for (int i = 0; i < indices.size; i++)
		{
			if (indices[i] == characterIndex)
			{
				int num = characterIndex << 1;
				int i2 = num + 1;
				Vector3 vector = verts[num];
				Vector3 vector2 = verts[i2];
				Vector3 vector3 = new Vector3(vector2.x - vector.x, vector2.y - vector.y, 0f);
				return new Bounds(new Vector3(vector.x + vector3.x / 2f, vector.y + vector3.y / 2f, 0f), vector3);
			}
		}
		return default(Bounds);
	}

	private BMSymbol GetSymbol(string sequence, bool createIfMissing)
	{
		int i = 0;
		for (int count = mSymbols.Count; i < count; i++)
		{
			BMSymbol bMSymbol = mSymbols[i];
			if (bMSymbol.sequence == sequence)
			{
				return bMSymbol;
			}
		}
		if (createIfMissing)
		{
			BMSymbol bMSymbol2 = new BMSymbol();
			bMSymbol2.sequence = sequence;
			mSymbols.Add(bMSymbol2);
			return bMSymbol2;
		}
		return null;
	}

	private BMSymbol MatchSymbol(string text, int offset, int textLength)
	{
		int count = mSymbols.Count;
		if (count == 0)
		{
			return null;
		}
		textLength -= offset;
		for (int i = 0; i < count; i++)
		{
			BMSymbol bMSymbol = mSymbols[i];
			int length = bMSymbol.length;
			if (length == 0 || textLength < length)
			{
				continue;
			}
			bool flag = true;
			for (int j = 0; j < length; j++)
			{
				if (text[offset + j] != bMSymbol.sequence[j])
				{
					flag = false;
					break;
				}
			}
			if (flag && bMSymbol.Validate(atlas))
			{
				return bMSymbol;
			}
		}
		return null;
	}

	public void AddSymbol(string sequence, string spriteName)
	{
		GetSymbol(sequence, createIfMissing: true).spriteName = spriteName;
		MarkAsDirty();
	}

	public void RemoveSymbol(string sequence)
	{
		BMSymbol symbol = GetSymbol(sequence, createIfMissing: false);
		if (symbol != null)
		{
			symbols.Remove(symbol);
		}
		MarkAsDirty();
	}

	public void RenameSymbol(string before, string after)
	{
		BMSymbol symbol = GetSymbol(before, createIfMissing: false);
		if (symbol != null)
		{
			symbol.sequence = after;
		}
		MarkAsDirty();
	}

	public bool UsesSprite(string s)
	{
		if (!string.IsNullOrEmpty(s))
		{
			if (s.Equals(spriteName))
			{
				return true;
			}
			int i = 0;
			for (int count = symbols.Count; i < count; i++)
			{
				BMSymbol bMSymbol = symbols[i];
				if (s.Equals(bMSymbol.spriteName))
				{
					return true;
				}
			}
		}
		return false;
	}
}
