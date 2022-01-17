using System;
using UnityEngine;

[ExecuteInEditMode]
public class PE_Water : MonoBehaviour
{
	public Spline FlowSpline;

	public float Cycle = 0.15f;

	public float Speed = 0.05f;

	private void Start()
	{
	}

	private void Update()
	{
		if ((bool)GetComponent<Renderer>())
		{
			if (Application.isEditor && !Application.isPlaying)
			{
				GetComponent<Renderer>().enabled = false;
			}
			else
			{
				GetComponent<Renderer>().enabled = true;
			}
			Material sharedMaterial = GetComponent<Renderer>().sharedMaterial;
			if ((bool)sharedMaterial)
			{
				Vector4 vector = sharedMaterial.GetVector("_WaveScaleAndDirection");
				float x = vector.x;
				float y = vector.y;
				float z = vector.z;
				float w = vector.w;
				Vector2 vectorFromAngle = GetVectorFromAngle(y);
				Vector2 vectorFromAngle2 = GetVectorFromAngle(z);
				Vector2 vectorFromAngle3 = GetVectorFromAngle(w);
				Vector4 vector2 = sharedMaterial.GetVector("_WaveSpeed");
				Vector4 vector3 = sharedMaterial.GetVector("_WaveSpeed2");
				Vector2 vector4 = new Vector2(vectorFromAngle.x * vector2.x, vectorFromAngle.y * vector2.x);
				Vector2 vector5 = new Vector2(vectorFromAngle2.x * vector2.y, vectorFromAngle2.y * vector2.y);
				Vector2 vector6 = new Vector2(vectorFromAngle3.x * vector2.z, vectorFromAngle3.y * vector2.z);
				Vector2 vector7 = new Vector2(vectorFromAngle.x * vector2.w, vectorFromAngle.y * vector2.w);
				Vector2 vector8 = new Vector2(vectorFromAngle2.x * vector3.x, vectorFromAngle2.y * vector3.x);
				float num = Time.time / 20f;
				Vector4 vector9 = new Vector4(vector4.x, vector4.y, vector5.x, vector5.y) * (num * x);
				Vector4 value = new Vector4(Mathf.Repeat(vector9.x, 1f), Mathf.Repeat(vector9.y, 1f), Mathf.Repeat(vector9.z, 1f), Mathf.Repeat(vector9.w, 1f));
				sharedMaterial.SetVector("_WaveOffset1", value);
				vector9 = new Vector4(vector6.x, vector6.y, vector7.x, vector7.y) * (num * x);
				value = new Vector4(Mathf.Repeat(vector9.x, 1f), Mathf.Repeat(vector9.y, 1f), Mathf.Repeat(vector9.z, 1f), Mathf.Repeat(vector9.w, 1f));
				sharedMaterial.SetVector("_WaveOffset2", value);
				vector9 = new Vector4(vector8.x, vector8.y, 0f, 0f) * (num * x);
				value = new Vector4(Mathf.Repeat(vector9.x, 1f), Mathf.Repeat(vector9.y, 1f), Mathf.Repeat(vector9.z, 1f), Mathf.Repeat(vector9.w, 1f));
				sharedMaterial.SetVector("_WaveOffset3", value);
				Vector3 vector10 = new Vector3(1f / x, 1f / x, 1f);
				Matrix4x4 value2 = Matrix4x4.TRS(new Vector3(value.x, value.y, 0f), Quaternion.identity, vector10);
				sharedMaterial.SetMatrix("_WaveMatrix", value2);
				value2 = Matrix4x4.TRS(new Vector3(value.z, value.w, 0f), Quaternion.identity, vector10 * 0.45f);
				sharedMaterial.SetMatrix("_WaveMatrix2", value2);
			}
		}
	}

	private Vector2 GetVectorFromAngle(float degrees)
	{
		Vector2 result = default(Vector2);
		result.x = Mathf.Cos(degrees * ((float)Math.PI / 180f));
		result.y = Mathf.Sin(degrees * ((float)Math.PI / 180f));
		return result;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = new Color(0.2f, 0.6f, 1f, 1f);
		if (!(GetComponent<Renderer>() == null))
		{
			Gizmos.DrawWireCube(GetComponent<Renderer>().bounds.center, GetComponent<Renderer>().bounds.size);
		}
	}

	private void CreateWaterMesh()
	{
		float num = 5f;
		int num2 = Mathf.CeilToInt(GetComponent<Renderer>().bounds.size.x / num);
		int num3 = Mathf.CeilToInt(GetComponent<Renderer>().bounds.size.z / num);
		Vector3[] array = new Vector3[4 * num2 * num3];
		Vector3[] array2 = new Vector3[4 * num2 * num3];
		Vector4[] array3 = new Vector4[4 * num2 * num3];
		Vector2[] array4 = new Vector2[4 * num2 * num3];
		Vector2[] array5 = new Vector2[4 * num2 * num3];
		int[] array6 = new int[6 * num2 * num3];
		Mesh mesh2 = (base.gameObject.GetComponent<MeshFilter>().mesh = new Mesh());
		int num4 = 0;
		for (int i = 0; i < num3; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				float num5 = (float)j * num - (float)num2 * num / 2f;
				float num6 = (float)i * num - (float)num3 * num / 2f;
				float x = j / num2;
				float x2 = (j + 1) / num2;
				float y = i / num3;
				float y2 = (i + 1) / num3;
				array[4 * num4].x = 0f + num5;
				array[4 * num4].y = 0f;
				array[4 * num4].z = 0f + num6;
				array[1 + 4 * num4].x = num + num5;
				array[1 + 4 * num4].y = 0f;
				array[1 + 4 * num4].z = 0f + num6;
				array[2 + 4 * num4].x = 0f + num5;
				array[2 + 4 * num4].y = 0f;
				array[2 + 4 * num4].z = num + num6;
				array[3 + 4 * num4].x = num + num5;
				array[3 + 4 * num4].y = 0f;
				array[3 + 4 * num4].z = num + num6;
				array2[4 * num4].x = 0f;
				array2[4 * num4].y = 1f;
				array2[4 * num4].z = 0f;
				array2[1 + 4 * num4].x = 0f;
				array2[1 + 4 * num4].y = 1f;
				array2[1 + 4 * num4].z = 0f;
				array2[2 + 4 * num4].x = 0f;
				array2[2 + 4 * num4].y = 1f;
				array2[2 + 4 * num4].z = 0f;
				array2[3 + 4 * num4].x = 0f;
				array2[3 + 4 * num4].y = 1f;
				array2[3 + 4 * num4].z = 0f;
				array3[4 * num4].x = 1f;
				array3[4 * num4].y = 0f;
				array3[4 * num4].z = 0f;
				array3[4 * num4].w = 1f;
				array3[1 + 4 * num4].x = 1f;
				array3[1 + 4 * num4].y = 0f;
				array3[1 + 4 * num4].z = 0f;
				array3[1 + 4 * num4].w = 1f;
				array3[2 + 4 * num4].x = 1f;
				array3[2 + 4 * num4].y = 0f;
				array3[2 + 4 * num4].z = 0f;
				array3[2 + 4 * num4].w = 1f;
				array3[3 + 4 * num4].x = 1f;
				array3[3 + 4 * num4].y = 0f;
				array3[3 + 4 * num4].z = 0f;
				array3[3 + 4 * num4].w = 1f;
				array4[4 * num4].x = x;
				array4[4 * num4].y = y;
				array4[1 + 4 * num4].x = x2;
				array4[1 + 4 * num4].y = y;
				array4[2 + 4 * num4].x = x;
				array4[2 + 4 * num4].y = y2;
				array4[3 + 4 * num4].x = x2;
				array4[3 + 4 * num4].y = y2;
				if (FlowSpline != null)
				{
					Vector3 p = array[4 * num4];
					Vector3 p2 = array[1 + 4 * num4];
					Vector3 p3 = array[2 + 4 * num4];
					Vector3 p4 = array[3 + 4 * num4];
					float d = 0f;
					Vector3 vector = -FlowSpline.GetVelocity(FlowSpline.GetClosestPoint(p, out d), d);
					Vector3 vector2 = -FlowSpline.GetVelocity(FlowSpline.GetClosestPoint(p2, out d), d);
					Vector3 vector3 = -FlowSpline.GetVelocity(FlowSpline.GetClosestPoint(p3, out d), d);
					Vector3 vector4 = -FlowSpline.GetVelocity(FlowSpline.GetClosestPoint(p4, out d), d);
					array5[4 * num4].x = vector.x;
					array5[4 * num4].y = vector.z;
					array5[1 + 4 * num4].x = vector2.x;
					array5[1 + 4 * num4].y = vector2.z;
					array5[2 + 4 * num4].x = vector3.x;
					array5[2 + 4 * num4].y = vector3.z;
					array5[3 + 4 * num4].x = vector4.x;
					array5[3 + 4 * num4].y = vector4.z;
				}
				array6[6 * num4] = 4 * num4;
				array6[1 + 6 * num4] = 2 + 4 * num4;
				array6[2 + 6 * num4] = 1 + 4 * num4;
				array6[3 + 6 * num4] = 1 + 4 * num4;
				array6[4 + 6 * num4] = 2 + 4 * num4;
				array6[5 + 6 * num4] = 3 + 4 * num4;
				num4++;
			}
		}
		mesh2.vertices = array;
		mesh2.uv = array4;
		mesh2.uv2 = array5;
		mesh2.triangles = array6;
		mesh2.normals = array2;
		mesh2.tangents = array3;
	}
}
