using System.Globalization;
using UnityEngine;

public class ColorPicker : MonoBehaviour
{
	private enum ESTATE
	{
		Hidden,
		Showed,
		Showing,
		Hidding
	}

	public Texture2D colorSpace;

	public Texture2D alphaGradient;

	public string Title = "Color Picker";

	public Vector2 startPos = new Vector2(20f, 20f);

	public GameObject receiver;

	public string colorSetFunctionName = "OnSetNewColor";

	public string colorGetFunctionName = "OnGetColor";

	public string colorSetTempFunctionName = "OnSetTempColor";

	public bool useExternalDrawer;

	public int drawOrder;

	private Color TempColor;

	private Color SelectedColor;

	private static ColorPicker activeColorPicker;

	private ESTATE mState;

	private int sizeFull = 200;

	private int sizeHidden = 20;

	private float animTime = 0.25f;

	private float dt;

	private float sizeCurr;

	private float alphaGradientHeight = 16f;

	private Color textColor = Color.black;

	private Texture2D txColorDisplay;

	private string txtR;

	private string txtG;

	private string txtB;

	private string txtA;

	private float valR;

	private float valG;

	private float valB;

	private float valA;

	public void NotifyColor(Color color)
	{
		SetColor(color);
		SelectedColor = color;
		UpdateColorEditFields(isFocused: false);
		UpdateColorSliders(isFocused: false);
	}

	private void Start()
	{
		sizeCurr = sizeHidden;
		txColorDisplay = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
		if ((bool)receiver)
		{
			receiver.SendMessage(colorGetFunctionName, this, SendMessageOptions.DontRequireReceiver);
		}
	}

	private void OnDestroy()
	{
		if (activeColorPicker == this)
		{
			activeColorPicker = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnGUI()
	{
		if (!useExternalDrawer)
		{
			_DrawGUI();
		}
	}

	private void UpdateColorSliders(bool isFocused)
	{
		if (!isFocused)
		{
			valR = TempColor.r;
			valG = TempColor.g;
			valB = TempColor.b;
			valA = TempColor.a;
		}
		else
		{
			SetColor(new Color(valR, valG, valB, valA));
		}
	}

	private void UpdateColorEditFields(bool isFocused)
	{
		if (!isFocused)
		{
			txtR = (255f * TempColor.r).ToString();
			txtG = (255f * TempColor.g).ToString();
			txtB = (255f * TempColor.b).ToString();
			txtA = (255f * TempColor.a).ToString();
			return;
		}
		byte r = 0;
		byte g = 0;
		byte b = 0;
		byte a = 0;
		if (!string.IsNullOrEmpty(txtR))
		{
			r = byte.Parse(txtR, NumberStyles.Any);
		}
		if (!string.IsNullOrEmpty(txtG))
		{
			g = byte.Parse(txtG, NumberStyles.Any);
		}
		if (!string.IsNullOrEmpty(txtB))
		{
			b = byte.Parse(txtB, NumberStyles.Any);
		}
		if (!string.IsNullOrEmpty(txtA))
		{
			a = byte.Parse(txtA, NumberStyles.Any);
		}
		SetColor(new Color32(r, g, b, a));
	}

	public bool IsDeployed()
	{
		return mState != ESTATE.Hidden;
	}

	public void _DrawGUI()
	{
		GUIStyle gUIStyle = new GUIStyle(GUI.skin.label);
		gUIStyle.normal.textColor = textColor;
		Rect rect = new Rect(startPos.x + sizeCurr + 10f, startPos.y + 30f, 40f, 140f);
		Rect rect2 = new Rect(startPos.x + sizeCurr + 50f, startPos.y + 30f, 60f, 140f);
		GUI.Label(new Rect(startPos.x + sizeCurr + 60f, startPos.y, 200f, 30f), Title, gUIStyle);
		GUI.DrawTexture(new Rect(startPos.x + sizeCurr + 10f, startPos.y, 40f, 20f), txColorDisplay);
		if (mState == ESTATE.Showed)
		{
			txtR = GUI.TextField(new Rect(startPos.x + sizeCurr + 10f, startPos.y + 30f, 40f, 20f), txtR, 3);
			txtG = GUI.TextField(new Rect(startPos.x + sizeCurr + 10f, startPos.y + 60f, 40f, 20f), txtG, 3);
			txtB = GUI.TextField(new Rect(startPos.x + sizeCurr + 10f, startPos.y + 90f, 40f, 20f), txtB, 3);
			txtA = GUI.TextField(new Rect(startPos.x + sizeCurr + 10f, startPos.y + 120f, 40f, 20f), txtA, 3);
			valR = GUI.HorizontalSlider(new Rect(startPos.x + sizeCurr + 50f, startPos.y + 35f, 60f, 20f), valR, 0f, 1f);
			valG = GUI.HorizontalSlider(new Rect(startPos.x + sizeCurr + 50f, startPos.y + 65f, 60f, 20f), valG, 0f, 1f);
			valB = GUI.HorizontalSlider(new Rect(startPos.x + sizeCurr + 50f, startPos.y + 95f, 60f, 20f), valB, 0f, 1f);
			valA = GUI.HorizontalSlider(new Rect(startPos.x + sizeCurr + 50f, startPos.y + 125f, 60f, 20f), valA, 0f, 1f);
			if (GUI.Button(new Rect(startPos.x + sizeCurr + 10f, startPos.y + 150f, 60f, 20f), "Apply"))
			{
				ApplyColor();
				SelectedColor = TempColor;
				if ((bool)receiver)
				{
					receiver.SendMessage(colorSetFunctionName, SelectedColor, SendMessageOptions.DontRequireReceiver);
				}
			}
			else if ((bool)receiver)
			{
				receiver.SendMessage(colorSetTempFunctionName, TempColor, SendMessageOptions.DontRequireReceiver);
			}
			GUIStyle gUIStyle2 = new GUIStyle(GUI.skin.label);
			gUIStyle2.normal.textColor = Color.white;
			GUI.Label(new Rect(startPos.x + sizeCurr + 110f, startPos.y + 30f, 20f, 20f), "R", gUIStyle2);
			GUI.Label(new Rect(startPos.x + sizeCurr + 110f, startPos.y + 60f, 20f, 20f), "G", gUIStyle2);
			GUI.Label(new Rect(startPos.x + sizeCurr + 110f, startPos.y + 90f, 20f, 20f), "B", gUIStyle2);
			GUI.Label(new Rect(startPos.x + sizeCurr + 110f, startPos.y + 120f, 20f, 20f), "A", gUIStyle2);
		}
		if (mState == ESTATE.Showing)
		{
			sizeCurr = Mathf.Lerp(sizeHidden, sizeFull, dt / animTime);
			if (dt / animTime > 1f)
			{
				mState = ESTATE.Showed;
			}
			dt += Time.deltaTime;
		}
		if (mState == ESTATE.Hidding)
		{
			sizeCurr = Mathf.Lerp(sizeFull, sizeHidden, dt / animTime);
			if (dt / animTime > 1f)
			{
				mState = ESTATE.Hidden;
			}
			dt += Time.deltaTime;
		}
		Rect position = new Rect(startPos.x, startPos.y, sizeCurr, sizeCurr);
		GUI.DrawTexture(position, colorSpace);
		float num = alphaGradientHeight * (sizeCurr / (float)sizeFull);
		Vector2 vector = startPos + new Vector2(0f, sizeCurr);
		Rect position2 = new Rect(vector.x, vector.y, sizeCurr, num);
		GUI.DrawTexture(position2, alphaGradient);
		Rect rect3 = new Rect(startPos.x, startPos.y, sizeCurr, sizeCurr + num);
		Vector2 mousePosition = Event.current.mousePosition;
		Event current = Event.current;
		bool flag = current.type == EventType.MouseUp;
		bool flag2 = current.type == EventType.MouseDrag;
		bool num2 = rect3.Contains(current.mousePosition) && (current.type == EventType.MouseUp || current.type == EventType.MouseDrag || current.type == EventType.MouseMove) && current.isMouse;
		bool flag3 = flag || (!rect3.Contains(current.mousePosition) && current.isMouse && (current.type == EventType.MouseMove || current.type == EventType.MouseDown));
		if (num2 && (activeColorPicker == null || activeColorPicker.mState == ESTATE.Hidden) && mState == ESTATE.Hidden)
		{
			mState = ESTATE.Showing;
			activeColorPicker = this;
			dt = 0f;
		}
		if (flag3 && mState == ESTATE.Showed)
		{
			if (flag)
			{
				ApplyColor();
			}
			else
			{
				SetColor(SelectedColor);
				if ((bool)receiver)
				{
					receiver.SendMessage(colorSetTempFunctionName, TempColor, SendMessageOptions.DontRequireReceiver);
				}
			}
			mState = ESTATE.Hidding;
			dt = 0f;
		}
		if (mState != ESTATE.Showed)
		{
			return;
		}
		if (position.Contains(current.mousePosition))
		{
			float num3 = (float)colorSpace.width / sizeCurr;
			float num4 = (float)colorSpace.height / sizeCurr;
			Vector2 vector2 = mousePosition - startPos;
			Color pixel = colorSpace.GetPixel((int)(num3 * vector2.x), colorSpace.height - (int)(num4 * vector2.y) - 1);
			SetColor(pixel);
			if (flag2)
			{
				ApplyColor();
			}
			UpdateColorEditFields(isFocused: false);
			UpdateColorSliders(isFocused: false);
		}
		else if (position2.Contains(current.mousePosition))
		{
			float num5 = (float)alphaGradient.width / sizeCurr;
			float num6 = (float)alphaGradient.height / sizeCurr;
			Vector2 vector3 = mousePosition - vector;
			Color pixel2 = alphaGradient.GetPixel((int)(num5 * vector3.x), colorSpace.height - (int)(num6 * vector3.y) - 1);
			Color color = GetColor();
			color.a = pixel2.r;
			SetColor(color);
			if (flag2)
			{
				ApplyColor();
			}
			UpdateColorEditFields(isFocused: false);
			UpdateColorSliders(isFocused: false);
		}
		else if (rect.Contains(current.mousePosition))
		{
			UpdateColorEditFields(isFocused: true);
			UpdateColorSliders(isFocused: false);
		}
		else if (rect2.Contains(current.mousePosition))
		{
			UpdateColorEditFields(isFocused: false);
			UpdateColorSliders(isFocused: true);
		}
		else
		{
			SetColor(SelectedColor);
		}
	}

	public void SetColor(Color color)
	{
		TempColor = color;
		if (txColorDisplay != null)
		{
			txColorDisplay.SetPixel(0, 0, color);
			txColorDisplay.Apply();
		}
	}

	public Color GetColor()
	{
		return TempColor;
	}

	public void SetTitle(string title, int fontSize, Color textColor)
	{
		Title = title;
		this.textColor = textColor;
	}

	public void ApplyColor()
	{
		SelectedColor = TempColor;
		if ((bool)receiver)
		{
			receiver.SendMessage(colorSetFunctionName, SelectedColor, SendMessageOptions.DontRequireReceiver);
		}
	}
}
