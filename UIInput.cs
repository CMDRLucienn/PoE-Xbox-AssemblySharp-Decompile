using System;
using UnityEngine;

[AddComponentMenu("NGUI/UI/Input (Basic)")]
public class UIInput : MonoBehaviour
{
	public delegate char Validator(string currentText, char nextChar);

	public enum KeyboardType
	{
		Default,
		ASCIICapable,
		NumbersAndPunctuation,
		URL,
		NumberPad,
		PhonePad,
		NamePhonePad,
		EmailAddress
	}

	public delegate void OnSubmit(string inputString);

	public static UIInput current;

	public UILabel label;

	[Tooltip("A string to use to identify what should go in this input.")]
	public DatabaseString IdentifierString = new DatabaseString(DatabaseString.StringTableType.Gui, -1);

	public int maxChars;

	public string caratChar = "|";

	public const float blinkPeriod = 0.5f;

	private UILabel mCaratLabel;

	private UIAnchor mCaratAnchor;

	private float mBlinker;

	private bool mBlinkState;

	private bool m_CalculateCaratPos;

	private Vector3 m_CaratPosition;

	private BetterList<Vector3> m_ProcessedTextVerts = new BetterList<Vector3>();

	private BetterList<int> m_ProcessedTextIndices = new BetterList<int>();

	private int m_CaratCharacterIndex;

	private float m_NextInputTimeStamp;

	private bool m_RepeatInput;

	public Validator validator;

	public KeyboardType type;

	public bool isPassword;

	public bool autoCorrect;

	public bool useLabelTextAtStart;

	public bool saveOnDeselect;

	public Color activeColor = Color.white;

	public GameObject selectOnTab;

	public GameObject eventReceiver;

	public string functionName = "OnSubmit";

	public OnSubmit onSubmit;

	private string mText = "";

	private string mDefaultText = "";

	private Color mDefaultColor = Color.white;

	private UIWidget.Pivot mPivot = UIWidget.Pivot.Left;

	private float mPosition;

	private string mLastIME = "";

	private bool mDoInit = true;

	public virtual string text
	{
		get
		{
			if (mDoInit)
			{
				Init();
			}
			return mText + mLastIME;
		}
		set
		{
			if (mDoInit)
			{
				Init();
			}
			mText = value;
			if (label != null)
			{
				if (string.IsNullOrEmpty(value))
				{
					value = mDefaultText;
				}
				label.supportEncoding = false;
				label.text = value;
				label.showLastPasswordChar = selected;
				label.color = ((selected || value != mDefaultText) ? activeColor : mDefaultColor);
				if (selected)
				{
					m_CalculateCaratPos = true;
					m_CaratCharacterIndex = label.processedText.Length;
				}
				UpdateLabel();
			}
		}
	}

	public bool selected
	{
		get
		{
			return UICamera.selectedObject == base.gameObject;
		}
		set
		{
			if (!value && UICamera.selectedObject == base.gameObject)
			{
				UICamera.selectedObject = null;
			}
			else if (value && UICamera.selectedObject != base.gameObject)
			{
				UICamera.selectedObject = base.gameObject;
			}
		}
	}

	public string defaultText
	{
		get
		{
			return mDefaultText;
		}
		set
		{
			if (label.text == mDefaultText)
			{
				label.text = value;
			}
			mDefaultText = value;
		}
	}

	public event Action<UIInput, bool> OnSelectedChanged;

	public event Action<UIInput, int> OnCaratMoved;

	public event Action<UIInput, string> OnInputRecieved;

	protected void Init()
	{
		if (!mDoInit)
		{
			return;
		}
		mDoInit = false;
		if (label == null)
		{
			label = GetComponentInChildren<UILabel>();
		}
		if (label != null)
		{
			if (useLabelTextAtStart)
			{
				mText = label.text;
			}
			mDefaultText = label.text;
			mDefaultColor = label.color;
			label.supportEncoding = false;
			label.password = isPassword;
			mPivot = label.pivot;
			mPosition = label.cachedTransform.localPosition.x;
			label.useLigatures = false;
		}
		else
		{
			base.enabled = false;
		}
		if (mCaratLabel == null)
		{
			mCaratLabel = NGUITools.AddChild<UILabel>(base.transform.parent.gameObject);
			if (mCaratLabel != null)
			{
				mCaratLabel.gameObject.name = "CaratLabel";
				mCaratLabel.transform.localPosition = label.transform.localPosition;
				mCaratLabel.transform.localScale = label.transform.localScale;
				mCaratLabel.font = label.font;
				mCaratLabel.depth = label.depth + 1;
				mCaratLabel.text = caratChar;
				mCaratLabel.color = label.color;
				mCaratLabel.pivot = UIWidget.Pivot.Center;
				mCaratAnchor = mCaratLabel.gameObject.AddComponent<UIAnchor>();
				mCaratAnchor.DisableX = (mCaratAnchor.DisableY = false);
				mCaratAnchor.widgetContainer = label;
				mCaratAnchor.side = UIAnchor.Side.Right;
				mCaratAnchor.enabled = false;
				mCaratAnchor.Update();
			}
		}
	}

	private void OnEnable()
	{
		if (UICamera.IsHighlighted(base.gameObject))
		{
			OnSelect(isSelected: true);
		}
		if (mCaratLabel != null)
		{
			mCaratLabel.gameObject.SetActive(value: true);
			mBlinkState = selected;
			mBlinker = 0.5f;
			UpdateLabel();
		}
	}

	private void OnDisable()
	{
		if (UICamera.IsHighlighted(base.gameObject))
		{
			OnSelect(isSelected: false);
		}
		if (mCaratLabel != null)
		{
			mCaratLabel.gameObject.SetActive(value: false);
		}
	}

	private void OnClick()
	{
		if (!selected)
		{
			selected = true;
			return;
		}
		Vector3 position = UICamera.lastTouchPosition;
		position.z = base.transform.position.z;
		Vector3 position2 = UICamera.mainCamera.ScreenToWorldPoint(position);
		Vector2 pos = label.transform.InverseTransformPoint(position2);
		pos.Scale(label.transform.localScale);
		if (label.font == null)
		{
			return;
		}
		CalculateTextVertices(label.text);
		if (m_ProcessedTextVerts == null || m_ProcessedTextIndices == null || m_ProcessedTextVerts.size != m_ProcessedTextIndices.size * 2)
		{
			m_CaratCharacterIndex = 0;
			return;
		}
		int num = UIFont.GetCharacterCursorIndex(m_ProcessedTextVerts, m_ProcessedTextIndices, pos);
		if (num < 0)
		{
			Vector3 vector = m_ProcessedTextVerts[0];
			Vector3 vector2 = m_ProcessedTextVerts[1];
			if (pos.x < vector.x)
			{
				num = m_ProcessedTextIndices[0];
			}
			vector = m_ProcessedTextVerts[(m_ProcessedTextIndices.size - 1) * 2];
			vector2 = m_ProcessedTextVerts[(m_ProcessedTextIndices.size - 1) * 2 + 1];
			if (pos.x > vector2.x)
			{
				num = m_ProcessedTextIndices[m_ProcessedTextIndices.size - 1] + 1;
			}
		}
		if (num >= 0 && num != m_CaratCharacterIndex)
		{
			m_CaratCharacterIndex = num;
			m_CalculateCaratPos = true;
			mBlinkState = true;
			mBlinker = 0.5f;
			UpdateLabel();
		}
	}

	private void OnSelect(bool isSelected)
	{
		if (mDoInit)
		{
			Init();
		}
		if (!(label != null) || !base.enabled || !NGUITools.GetActive(base.gameObject))
		{
			return;
		}
		m_CalculateCaratPos = true;
		if (isSelected)
		{
			mText = ((!useLabelTextAtStart && label.text == mDefaultText) ? "" : label.text);
			label.color = activeColor;
			if (isPassword)
			{
				label.password = true;
			}
			Input.imeCompositionMode = IMECompositionMode.On;
			Transform cachedTransform = label.cachedTransform;
			Vector3 position = label.pivotOffset;
			position.y += label.relativeSize.y;
			position = cachedTransform.TransformPoint(position);
			Input.compositionCursorPos = UICamera.currentCamera.WorldToScreenPoint(position);
			mBlinkState = true;
			mBlinker = 0.5f;
			UpdateLabel();
		}
		else
		{
			mText += mLastIME;
			if (string.IsNullOrEmpty(mText))
			{
				label.text = mDefaultText;
				label.color = mDefaultColor;
				if (isPassword)
				{
					label.password = false;
				}
			}
			else
			{
				label.text = mText;
			}
			label.showLastPasswordChar = false;
			Input.imeCompositionMode = IMECompositionMode.Off;
			RestoreLabel();
			mBlinkState = false;
			mBlinker = 0.5f;
			UpdateLabel();
			if (saveOnDeselect)
			{
				NotifySubmit();
			}
		}
		if (this.OnSelectedChanged != null)
		{
			this.OnSelectedChanged(this, isSelected);
		}
	}

	private void Update()
	{
		if (!selected)
		{
			return;
		}
		mBlinker -= TimeController.sUnscaledDelta;
		if (mBlinker <= 0f)
		{
			mBlinker = 0.5f;
			mBlinkState = !mBlinkState;
			UpdateLabel();
		}
		if (selectOnTab != null && Input.GetKeyDown(KeyCode.Tab))
		{
			UICamera.selectedObject = selectOnTab;
		}
		if (Input.GetKey(KeyCode.LeftArrow))
		{
			if (Time.realtimeSinceStartup > m_NextInputTimeStamp)
			{
				if (GameInput.GetControlkey())
				{
					NavigateCaratByWord(-1);
				}
				else
				{
					NavigateCarat(-1);
				}
				m_NextInputTimeStamp = Time.realtimeSinceStartup + (m_RepeatInput ? GameInput.Instance.KeyRepeatTime : GameInput.Instance.KeyRepeatDelay);
				m_RepeatInput = true;
			}
		}
		else if (Input.GetKey(KeyCode.RightArrow))
		{
			if (Time.realtimeSinceStartup > m_NextInputTimeStamp)
			{
				if (GameInput.GetControlkey())
				{
					NavigateCaratByWord(1);
				}
				else
				{
					NavigateCarat(1);
				}
				m_NextInputTimeStamp = Time.realtimeSinceStartup + (m_RepeatInput ? GameInput.Instance.KeyRepeatTime : GameInput.Instance.KeyRepeatDelay);
				m_RepeatInput = true;
			}
		}
		else if (Input.GetKey(KeyCode.Home))
		{
			MoveCaratToCharacter(0);
			m_NextInputTimeStamp = -1f;
			m_RepeatInput = false;
		}
		else if (Input.GetKey(KeyCode.End))
		{
			MoveCaratToCharacter(label.text.Length);
			m_NextInputTimeStamp = -1f;
			m_RepeatInput = false;
		}
		else
		{
			m_NextInputTimeStamp = -1f;
			m_RepeatInput = false;
		}
		if (Input.GetKeyDown(KeyCode.V) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
		{
			Append(NGUITools.clipboard);
		}
		if (mLastIME != Input.compositionString)
		{
			mLastIME = Input.compositionString;
			UpdateLabel();
			SendMessage("OnInputChanged", this, SendMessageOptions.DontRequireReceiver);
		}
	}

	private void NavigateCarat(int motion)
	{
		MoveCaratToCharacter(m_CaratCharacterIndex + motion);
	}

	private void NavigateCaratByWord(int motion)
	{
		MoveCaratToCharacter(m_CaratCharacterIndex + motion);
	}

	private void MoveCaratToCharacter(int index)
	{
		m_CaratCharacterIndex = Mathf.Clamp(index, 0, label.text.Length);
		m_CalculateCaratPos = true;
		mBlinkState = true;
		mBlinker = 0.5f;
		UpdateLabel();
		if (this.OnCaratMoved != null)
		{
			this.OnCaratMoved(this, m_CaratCharacterIndex);
		}
	}

	private void CalculateTextVertices(string textToProcess)
	{
		m_ProcessedTextIndices.Clear();
		m_ProcessedTextVerts.Clear();
		UIWidget.Pivot pivot = label.pivot;
		UIFont.Alignment alignment = UIFont.Alignment.Left;
		switch (pivot)
		{
		case UIWidget.Pivot.TopLeft:
		case UIWidget.Pivot.Left:
		case UIWidget.Pivot.BottomLeft:
			alignment = UIFont.Alignment.Left;
			break;
		case UIWidget.Pivot.TopRight:
		case UIWidget.Pivot.Right:
		case UIWidget.Pivot.BottomRight:
			alignment = UIFont.Alignment.Right;
			break;
		default:
			alignment = UIFont.Alignment.Center;
			break;
		}
		label.font.PrintExactCharacterPositions(textToProcess, m_ProcessedTextVerts, m_ProcessedTextIndices, label.supportEncoding, label.symbolStyle, alignment, (label.lineWidth != 0) ? label.lineWidth : Mathf.RoundToInt(label.relativeSize.x * (float)label.font.size), premultiply: false);
		label.ApplyOffset(m_ProcessedTextVerts, 0);
	}

	private void OnInput(string input)
	{
		if (mDoInit)
		{
			Init();
		}
		if (selected && base.enabled && NGUITools.GetActive(base.gameObject) && Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer)
		{
			mBlinkState = true;
			mBlinker = 0.5f;
			Append(input);
			if (this.OnInputRecieved != null)
			{
				this.OnInputRecieved(this, text);
			}
		}
	}

	protected void NotifySubmit()
	{
		current = this;
		if (onSubmit != null)
		{
			onSubmit(mText);
		}
		if (eventReceiver == null)
		{
			eventReceiver = base.gameObject;
		}
		eventReceiver.SendMessage(functionName, mText, SendMessageOptions.DontRequireReceiver);
		current = null;
		label.text = mText;
	}

	private void Append(string input)
	{
		int i = 0;
		for (int length = input.Length; i < length; i++)
		{
			char c = input[i];
			if (c == '\b')
			{
				if (mText.Length > 0 && m_CaratCharacterIndex > 0)
				{
					mText = mText.Remove(m_CaratCharacterIndex - 1, 1);
					m_CaratCharacterIndex--;
					m_CalculateCaratPos = true;
					SendMessage("OnInputChanged", this, SendMessageOptions.DontRequireReceiver);
				}
			}
			else if (c == '\r' || c == '\n')
			{
				if ((UICamera.current.submitKey0 == KeyCode.Return || UICamera.current.submitKey1 == KeyCode.Return) && (!label.multiLine || (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))))
				{
					NotifySubmit();
					selected = false;
					return;
				}
				if (validator != null)
				{
					c = validator(mText, c);
				}
				if (c != 0)
				{
					if (label.multiLine)
					{
						mText = mText.Insert(m_CaratCharacterIndex, "\n");
						m_CaratCharacterIndex++;
					}
					if (maxChars > 0 && mText.Length > maxChars)
					{
						mText = mText.Substring(0, maxChars);
					}
					m_CalculateCaratPos = true;
					SendMessage("OnInputChanged", this, SendMessageOptions.DontRequireReceiver);
				}
			}
			else
			{
				if (c < ' ')
				{
					continue;
				}
				if (validator != null)
				{
					c = validator(mText, c);
				}
				if (c != 0)
				{
					mText = mText.Insert(m_CaratCharacterIndex, c.ToString());
					if (maxChars > 0 && mText.Length > maxChars)
					{
						mText = mText.Substring(0, maxChars);
					}
					m_CaratCharacterIndex++;
					m_CalculateCaratPos = true;
					SendMessage("OnInputChanged", this, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
		UpdateLabel();
	}

	public void SetText(string text)
	{
		this.text = text;
		SendMessage("OnInputChanged", this, SendMessageOptions.DontRequireReceiver);
	}

	private void UpdateLabel()
	{
		if (mDoInit)
		{
			Init();
		}
		if (maxChars > 0 && mText.Length > maxChars)
		{
			mText = mText.Substring(0, maxChars);
		}
		if (!(label.font != null))
		{
			return;
		}
		string text;
		if (isPassword && selected)
		{
			text = "";
			int i = 0;
			for (int length = mText.Length; i < length; i++)
			{
				text += "*";
			}
			if (!string.IsNullOrEmpty(Input.compositionString))
			{
				text += Input.compositionString;
			}
		}
		else if (selected && !string.IsNullOrEmpty(Input.compositionString))
		{
			text = mText;
			text.Insert(m_CaratCharacterIndex, Input.compositionString);
			m_CalculateCaratPos = true;
		}
		else
		{
			text = mText;
		}
		label.supportEncoding = false;
		if (!label.shrinkToFit)
		{
			if (label.multiLine)
			{
				text = label.font.WrapText(text, (float)label.lineWidth / label.cachedTransform.localScale.x, 0, encoding: false, UIFont.SymbolStyle.None);
			}
			else
			{
				string endOfLineThatFits = label.font.GetEndOfLineThatFits(text, (float)label.lineWidth / label.cachedTransform.localScale.x, encoding: false, UIFont.SymbolStyle.None);
				if (endOfLineThatFits != text)
				{
					text = endOfLineThatFits;
					Vector3 localPosition = label.cachedTransform.localPosition;
					localPosition.x = mPosition + (float)label.lineWidth;
				}
				else
				{
					RestoreLabel();
				}
			}
		}
		label.text = text;
		label.showLastPasswordChar = selected;
		UpdateCursor();
	}

	private void UpdateCursor()
	{
		if (!(mCaratLabel != null))
		{
			return;
		}
		if (mBlinkState != mCaratLabel.gameObject.activeSelf)
		{
			mCaratLabel.gameObject.SetActive(mBlinkState);
		}
		if (!mBlinkState)
		{
			return;
		}
		if (mCaratLabel.font != label.font)
		{
			mCaratLabel.font = label.font;
		}
		if (mCaratLabel.color != label.color)
		{
			mCaratLabel.color = label.color;
		}
		if (mCaratLabel.transform.localScale != label.transform.localScale)
		{
			mCaratAnchor.transform.localScale = label.transform.localScale;
		}
		if (!(mCaratAnchor != null) || !m_CalculateCaratPos || !(label != null) || !(label.font != null))
		{
			return;
		}
		m_CaratCharacterIndex = Mathf.Min(label.processedText.Length, Mathf.Max(m_CaratCharacterIndex, 0));
		CalculateTextVertices(label.processedText);
		int num = m_CaratCharacterIndex;
		bool flag = false;
		if (label.processedText.Length > 0 && (m_CaratCharacterIndex == label.processedText.Length || label.processedText[m_CaratCharacterIndex] == '\n'))
		{
			num--;
			flag = true;
		}
		if (num >= 0 && label.processedText.Length > 0)
		{
			mCaratLabel.pivot = UIWidget.Pivot.Center;
			Bounds boundsOfCharacterAtIndex = UIFont.GetBoundsOfCharacterAtIndex(m_ProcessedTextVerts, m_ProcessedTextIndices, num);
			float num2 = 1f;
			if (label.shrinkToFit)
			{
				num2 = label.transform.localScale.x / (float)label.font.size;
			}
			Vector3 localPosition = label.transform.localPosition;
			if (flag)
			{
				m_CaratPosition.Set((localPosition.x + boundsOfCharacterAtIndex.max.x) * num2, (localPosition.y + boundsOfCharacterAtIndex.center.y) * num2, 0f);
			}
			else
			{
				m_CaratPosition.Set((localPosition.x + boundsOfCharacterAtIndex.min.x) * num2, (localPosition.y + boundsOfCharacterAtIndex.center.y) * num2, 0f);
			}
			mCaratAnchor.transform.localPosition = m_CaratPosition;
		}
		else
		{
			mCaratLabel.pivot = label.pivot;
			mCaratAnchor.transform.position = label.transform.position;
		}
		m_CalculateCaratPos = false;
	}

	private void RestoreLabel()
	{
		if (label != null)
		{
			label.pivot = mPivot;
			Vector3 localPosition = label.cachedTransform.localPosition;
			localPosition.x = mPosition;
			label.cachedTransform.localPosition = localPosition;
		}
	}
}
