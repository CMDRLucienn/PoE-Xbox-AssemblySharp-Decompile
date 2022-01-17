using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class InteriorTriangulation
{
	public static Mesh Triangulate(IList<Vector3> verts, Quaternion rotation)
	{
		Mesh mesh = new Mesh();
		Vector3[] array = new Vector3[verts.Count];
		for (int i = 0; i < verts.Count; i++)
		{
			Vector2 vector = ((i > 0) ? verts[i - 1] : verts[verts.Count - 1]);
			Vector2 vector2 = ((i < verts.Count - 1) ? verts[i + 1] : verts[0]);
			array[i] = (vector + vector2) / -2f;
			array[i].Normalize();
		}
		IEnumerable<Vector3> source = verts.Select((Vector3 v3) => Quaternion.Inverse(rotation) * v3);
		Triangulator triangulator = new Triangulator(source.Select((Vector3 v3) => new Vector2(v3.x, v3.z)).ToArray());
		mesh.vertices = source.ToArray();
		mesh.triangles = triangulator.Triangulate();
		mesh.RecalculateNormals();
		return mesh;
	}
}
