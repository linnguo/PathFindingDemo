using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class NavMeshTools
{
	private static int FindFirst(List<Vector3> list, Vector3 value)
	{
		for (int i = 0; i < list.Count; ++i)
		{
			if (Vector3.Distance(list[i], value) < 0.1f)
			{
				return i;
			}
		}
		return -1;
	}

	private static void Replace(int[] list, int oldValue, int newValue)
	{
		for (int i = 0; i < list.Length; ++i)
		{
			if (list[i] == oldValue)
			{
				list[i] = newValue;
			}
		}
	}

	//  合并离的很近的顶点
	public static void MergeVertex(Vector3[] vertices, int[] indices, out Vector3[] mergedVertices, out int[] mergedIndices)
	{
		List<Vector3> mergedVerticesList = new List<Vector3>();

		mergedIndices = new int[indices.Length];
		Array.Copy(indices, mergedIndices, indices.Length);

		for (int i = 0; i < vertices.Length; ++i)
		{
			Vector3 curVex = vertices[i];
			int idx = FindFirst(mergedVerticesList, curVex);
			if (idx >= 0)
			{
				Replace(mergedIndices, i, idx);
			}
			else
			{
				mergedVerticesList.Add(curVex);
				Replace(mergedIndices, i, mergedVerticesList.Count - 1);
			}
		}

		mergedVertices = mergedVerticesList.ToArray();
	}

	// 将顶点和三角形序列化成为obj格式
	public static string SerializeToObj(Vector3[] vertices, int[] indices)
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("o obj");
		for (int i = 0; i < vertices.Length; ++i)
		{
			sb.AppendFormat("v {0} {1} {2}", -vertices[i].x, vertices[i].y, vertices[i].z);
			sb.AppendLine();
		}

		int triangleCount = indices.Length / 3;
		for (int i = 0; i < triangleCount; ++i)
		{
			int index = i * 3;
			sb.AppendFormat("f {0} {1} {2}", indices[index] + 1, indices[index +1] + 1, indices[index +2] + 1);
			sb.AppendLine();
		}

		return sb.ToString();
	}
}
