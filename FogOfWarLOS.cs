using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FogOfWarLOS
{
	public class VertexData
	{
		public float MinAlpha;

		public byte[] DoorIndices;
	}

	public class BacksideVertexData
	{
		public VertexData[] VertexData;

		public Guid DoorID = Guid.Empty;
	}

	private const int MagicNumber = 235995888;

	public int Rows { get; set; }

	public int Columns { get; set; }

	public VertexData[][] Vertices { get; set; }

	public Dictionary<int, BacksideVertexData> BacksideVertices { get; set; }

	public Guid[] DoorIDs { get; set; }

	public static FogOfWarLOS Load(string filename)
	{
		if (!File.Exists(filename))
		{
			return null;
		}
		FogOfWarLOS fogOfWarLOS = new FogOfWarLOS();
		int num = 19 * 2 + 1;
		int maxVerts = num * num;
		try
		{
			using FileStream input = new FileStream(filename, FileMode.Open, FileAccess.Read);
			using BinaryReader binaryReader = new BinaryReader(input);
			int num2 = binaryReader.ReadInt32();
			int num3 = 1;
			if (num2 == 235995888)
			{
				num3 = binaryReader.ReadInt32();
				fogOfWarLOS.Rows = binaryReader.ReadInt32();
				fogOfWarLOS.Columns = binaryReader.ReadInt32();
			}
			else
			{
				fogOfWarLOS.Rows = num2;
				fogOfWarLOS.Columns = binaryReader.ReadInt32();
			}
			int num4 = binaryReader.ReadInt32();
			fogOfWarLOS.DoorIDs = new Guid[num4];
			for (int i = 0; i < num4; i++)
			{
				string g = binaryReader.ReadString();
				Guid guid = Guid.Empty;
				try
				{
					guid = new Guid(g);
				}
				catch
				{
				}
				fogOfWarLOS.DoorIDs[i] = guid;
			}
			int num5 = binaryReader.ReadInt32();
			int num6 = 0;
			if (num3 > 1)
			{
				num6 = binaryReader.ReadInt32();
			}
			fogOfWarLOS.Vertices = new VertexData[fogOfWarLOS.Rows * fogOfWarLOS.Columns][];
			fogOfWarLOS.BacksideVertices = new Dictionary<int, BacksideVertexData>();
			for (int j = 0; j < num5; j++)
			{
				ReadVertData(binaryReader, fogOfWarLOS, maxVerts, isBackside: false);
			}
			if (num3 > 1)
			{
				Guid empty = Guid.Empty;
				for (int k = 0; k < num6; k++)
				{
					try
					{
						empty = new Guid(binaryReader.ReadString());
					}
					catch
					{
						empty = Guid.Empty;
					}
					int key = ReadVertData(binaryReader, fogOfWarLOS, maxVerts, isBackside: true);
					fogOfWarLOS.BacksideVertices[key].DoorID = empty;
				}
			}
			return fogOfWarLOS;
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to read file " + filename + ". " + ex.Message);
			return null;
		}
	}

	public static void Save(FogOfWarLOS data, string filename)
	{
		string directoryName = Path.GetDirectoryName(filename);
		if (!Directory.Exists(directoryName))
		{
			try
			{
				Directory.CreateDirectory(directoryName);
			}
			catch (Exception ex)
			{
				Debug.LogError("Failed to create directory " + directoryName + ". " + ex.Message);
				return;
			}
		}
		if (File.Exists(filename))
		{
			try
			{
				File.Delete(filename);
			}
			catch (Exception ex2)
			{
				Debug.LogError("Failed to delete file " + filename + ". " + ex2.Message);
				return;
			}
		}
		int num = data.Rows * data.Columns;
		int num2 = 0;
		int num3 = 19 * 2 + 1;
		int maxVerts = num3 * num3;
		for (int i = 0; i < num; i++)
		{
			if (data.Vertices[i] != null)
			{
				num2++;
			}
		}
		try
		{
			using FileStream output = new FileStream(filename, FileMode.CreateNew, FileAccess.Write);
			using BinaryWriter binaryWriter = new BinaryWriter(output);
			binaryWriter.Write(235995888);
			binaryWriter.Write(2);
			binaryWriter.Write(data.Rows);
			binaryWriter.Write(data.Columns);
			binaryWriter.Write(data.DoorIDs.Length);
			Guid[] doorIDs = data.DoorIDs;
			for (int j = 0; j < doorIDs.Length; j++)
			{
				Guid guid = doorIDs[j];
				binaryWriter.Write(guid.ToString());
			}
			binaryWriter.Write(num2);
			binaryWriter.Write(data.BacksideVertices.Count);
			for (int k = 0; k < num; k++)
			{
				if (data.Vertices[k] != null)
				{
					WriteVertData(binaryWriter, data.Vertices[k], k, maxVerts);
				}
			}
			foreach (int key in data.BacksideVertices.Keys)
			{
				binaryWriter.Write(data.BacksideVertices[key].DoorID.ToString());
				WriteVertData(binaryWriter, data.BacksideVertices[key].VertexData, key, maxVerts);
			}
		}
		catch (Exception ex3)
		{
			Debug.LogError("Failed to write file " + filename + ". " + ex3.Message);
		}
	}

	private static void WriteVertData(BinaryWriter writer, VertexData[] vertexData, int index, int maxVerts)
	{
		writer.Write(index);
		ushort num = 0;
		ushort num2 = 0;
		for (ushort num3 = 0; num3 < maxVerts; num3 = (ushort)(num3 + 1))
		{
			float minAlpha = vertexData[num3].MinAlpha;
			if (minAlpha > float.Epsilon && minAlpha < 1f)
			{
				num = (ushort)(num + 1);
			}
			if (vertexData[num3].DoorIndices != null && vertexData[num3].DoorIndices.Length != 0)
			{
				num2 = (ushort)(num2 + 1);
			}
		}
		writer.Write(num);
		writer.Write(num2);
		int num4 = maxVerts / 32 + 1;
		int num5 = 0;
		int num6 = 0;
		uint[] array = new uint[num4];
		for (int i = 0; i < num4; i++)
		{
			array[i] = 0u;
		}
		for (int j = 0; j < vertexData.Length; j++)
		{
			if (vertexData[j].MinAlpha >= 1f)
			{
				array[num6] |= (uint)(1 << num5);
			}
			num5++;
			if (num5 > 31)
			{
				num5 = 0;
				num6++;
			}
		}
		uint[] array2 = array;
		for (int j = 0; j < array2.Length; j++)
		{
			int value = (int)array2[j];
			writer.Write(value);
		}
		for (ushort num7 = 0; num7 < maxVerts; num7 = (ushort)(num7 + 1))
		{
			float minAlpha2 = vertexData[num7].MinAlpha;
			if (minAlpha2 > float.Epsilon && minAlpha2 < 1f)
			{
				byte value2 = (byte)(((double)minAlpha2 + 0.001) * 20.0);
				writer.Write(num7);
				writer.Write(value2);
			}
		}
		for (ushort num8 = 0; num8 < maxVerts; num8 = (ushort)(num8 + 1))
		{
			byte[] doorIndices = vertexData[num8].DoorIndices;
			if (doorIndices != null && doorIndices.Length != 0)
			{
				writer.Write(num8);
				writer.Write((byte)doorIndices.Length);
				byte[] array3 = doorIndices;
				foreach (byte value3 in array3)
				{
					writer.Write(value3);
				}
			}
		}
	}

	private static int ReadVertData(BinaryReader reader, FogOfWarLOS data, int maxVerts, bool isBackside)
	{
		int num = reader.ReadInt32();
		ushort num2 = reader.ReadUInt16();
		ushort num3 = reader.ReadUInt16();
		VertexData[] array = new VertexData[maxVerts];
		if (isBackside)
		{
			BacksideVertexData backsideVertexData = new BacksideVertexData();
			backsideVertexData.VertexData = array;
			data.BacksideVertices.Add(num, backsideVertexData);
		}
		else
		{
			data.Vertices[num] = array;
		}
		int num4 = 0;
		int num5 = maxVerts / 32 + 1;
		for (int i = 0; i < num5; i++)
		{
			uint num6 = reader.ReadUInt32();
			for (int j = 0; j < 32; j++)
			{
				if ((num6 & (uint)(1 << j)) != 0)
				{
					array[num4] = new VertexData();
					array[num4].MinAlpha = 1f;
				}
				num4++;
				if (num4 >= maxVerts)
				{
					break;
				}
			}
		}
		for (ushort num7 = 0; num7 < num2; num7 = (ushort)(num7 + 1))
		{
			ushort num8 = reader.ReadUInt16();
			byte b = reader.ReadByte();
			array[num8] = new VertexData();
			array[num8].MinAlpha = (float)(int)b / 20f;
		}
		for (ushort num9 = 0; num9 < num3; num9 = (ushort)(num9 + 1))
		{
			uint num10 = reader.ReadUInt16();
			byte b2 = reader.ReadByte();
			if (array[num10] == null)
			{
				array[num10] = new VertexData();
				array[num10].MinAlpha = 0f;
			}
			try
			{
				array[num10].DoorIndices = new byte[b2];
			}
			catch
			{
				Debug.LogError("FAILED " + num3 + " " + num9 + " " + num + " " + num10 + " " + b2);
			}
			for (int k = 0; k < b2; k++)
			{
				array[num10].DoorIndices[k] = reader.ReadByte();
			}
		}
		return num;
	}
}
