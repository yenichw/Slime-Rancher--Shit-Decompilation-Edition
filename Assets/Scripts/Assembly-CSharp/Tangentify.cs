using UnityEngine;

public class Tangentify
{
	public static void AddTangents(Mesh mesh)
	{
		int num = mesh.triangles.Length / 3;
		int num2 = mesh.vertices.Length;
		Vector3[] array = new Vector3[num2];
		Vector3[] array2 = new Vector3[num2];
		Vector4[] array3 = new Vector4[num2];
		for (long num3 = 0L; num3 < num; num3 += 3)
		{
			long num4 = mesh.triangles[num3];
			long num5 = mesh.triangles[num3 + 1];
			long num6 = mesh.triangles[num3 + 2];
			Vector3 vector = mesh.vertices[num4];
			Vector3 vector2 = mesh.vertices[num5];
			Vector3 vector3 = mesh.vertices[num6];
			Vector2 vector4 = mesh.uv[num4];
			Vector2 vector5 = mesh.uv[num5];
			Vector2 vector6 = mesh.uv[num6];
			float num7 = vector2.x - vector.x;
			float num8 = vector3.x - vector.x;
			float num9 = vector2.y - vector.y;
			float num10 = vector3.y - vector.y;
			float num11 = vector2.z - vector.z;
			float num12 = vector3.z - vector.z;
			float num13 = vector5.x - vector4.x;
			float num14 = vector6.x - vector4.x;
			float num15 = vector5.y - vector4.y;
			float num16 = vector6.y - vector4.y;
			float num17 = 1f / (num13 * num16 - num14 * num15);
			Vector3 vector7 = new Vector3((num16 * num7 - num15 * num8) * num17, (num16 * num9 - num15 * num10) * num17, (num16 * num11 - num15 * num12) * num17);
			Vector3 vector8 = new Vector3((num13 * num8 - num14 * num7) * num17, (num13 * num10 - num14 * num9) * num17, (num13 * num12 - num14 * num11) * num17);
			array[num4] += vector7;
			array[num5] += vector7;
			array[num6] += vector7;
			array2[num4] += vector8;
			array2[num5] += vector8;
			array2[num6] += vector8;
		}
		for (long num18 = 0L; num18 < num2; num18++)
		{
			Vector3 vector9 = mesh.normals[num18];
			Vector3 vector10 = array[num18];
			Vector3 normalized = (vector10 - vector9 * Vector3.Dot(vector9, vector10)).normalized;
			array3[num18] = new Vector4(normalized.x, normalized.y, normalized.z);
			array3[num18].w = ((Vector3.Dot(Vector3.Cross(vector9, vector10), array2[num18]) < 0f) ? (-1f) : 1f);
		}
		mesh.tangents = array3;
	}
}
