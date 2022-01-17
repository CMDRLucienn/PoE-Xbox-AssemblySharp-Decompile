using UnityEngine;

public class PE_ColorGradeLUT : MonoBehaviour
{
	public Texture2D sourceTexture2D;

	public Texture3D converted3DLut;

	public string basedOnTempTex = "";

	public void Initialize()
	{
		if (null != sourceTexture2D)
		{
			Convert(sourceTexture2D, basedOnTempTex);
			return;
		}
		SetIdentityLut();
		Debug.LogError("PE_ColorGradeLUT::Initialize() - missing source texture 2D");
	}

	public void SetIdentityLut()
	{
		if (null != converted3DLut)
		{
			GameUtilities.DestroyImmediate(converted3DLut);
		}
		converted3DLut = PE_GameRender.CreateIdentityColorGradeLut();
		basedOnTempTex = "";
	}

	public bool ValidDimensions(Texture2D tex2d)
	{
		if (null == tex2d)
		{
			return false;
		}
		if (tex2d.height != Mathf.FloorToInt(Mathf.Sqrt(tex2d.width)))
		{
			return false;
		}
		return true;
	}

	public void Convert(Texture2D temp2DTex, string path)
	{
		if (!temp2DTex)
		{
			return;
		}
		int num = temp2DTex.width * temp2DTex.height;
		num = temp2DTex.height;
		if (!ValidDimensions(temp2DTex))
		{
			Debug.LogWarning("The given 2D texture " + temp2DTex.name + " cannot be used as a 3D LUT.");
			basedOnTempTex = "";
			return;
		}
		Color[] pixels = temp2DTex.GetPixels();
		Color[] array = new Color[pixels.Length];
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				for (int k = 0; k < num; k++)
				{
					int num2 = num - j - 1;
					array[i + j * num + k * num * num] = pixels[k * num + i + num2 * num * num];
				}
			}
		}
		if (null != converted3DLut)
		{
			GameUtilities.DestroyImmediate(converted3DLut);
		}
		converted3DLut = new Texture3D(num, num, num, TextureFormat.ARGB32, mipChain: false);
		converted3DLut.SetPixels(array);
		converted3DLut.Apply();
		basedOnTempTex = path;
		sourceTexture2D = temp2DTex;
	}
}
