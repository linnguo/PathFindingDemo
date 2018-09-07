using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshModel
{
	const int VertexPerNode = 3;

	// NavMesh给的原始资源
	Vector3[] navMeshVertices;
	int[] navMeshIndices;

	List<Vector3> vertices;
	// 显示用的模型
	Mesh mesh;

	public class Node
	{
		public int[] indices = new int[3];
		public Node[] neightbors = new Node[3];

		public Vector3 center;
		public Vector3[] vertices = new Vector3[3];
	}
	
	// 节点
	public List<Node> nodes;

	private int FindFirst(List<Vector3> list, Vector3 value)
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

	private void Replace(int[] list, int oldValue, int newValue)
	{
		for (int i = 0; i < list.Length; ++i)
		{
			if (list[i] == oldValue)
			{
				list[i] = newValue;
			}
		}
	}

	public NavMeshModel(NavMeshTriangulation navMeshTriangulation)
	{
		navMeshVertices = navMeshTriangulation.vertices;
		navMeshIndices = navMeshTriangulation.indices;

		vertices = new List<Vector3>();

		for (int i = 0; i < navMeshVertices.Length; ++i)
		{
			Vector3 curVex = navMeshVertices[i];
			int idx = FindFirst(vertices, curVex);
			if (idx >= 0)
			{
				Replace(navMeshIndices, i, idx);
			}
			else
			{
				vertices.Add(curVex);
				Replace(navMeshIndices, i, vertices.Count - 1);
			}
		}

		InitNodes();
		InitNeighbors();
	}

	private void InitNodes()
	{
		nodes = new List<Node>();
		
		int nodesCount = navMeshIndices.Length / VertexPerNode;

		for (int nodeIndex = 0; nodeIndex < nodesCount; ++nodeIndex)
		{
			Node node = new Node();
			Vector3 center = Vector3.zero;
			for (int i = 0; i < VertexPerNode; ++i)
			{
				int index = navMeshIndices[nodeIndex * VertexPerNode + i];
				Vector3 vertex = vertices[index];
				node.indices[i] = index;
				node.vertices[i] = vertex;
				center += vertex;
			}
			center /= VertexPerNode;
			node.center = center;

			nodes.Add(node);
		}
	}

	private void InitNeighbors()
	{
		for (int i = 0; i < nodes.Count - 1; ++i)
		{
			for (int j = 0; j < nodes.Count; ++j)
			{
				CheckNeighbor(nodes[i], nodes[j]);
				CheckNeighbor(nodes[j], nodes[i]);
			}
		}
	}

	private int FindFirst(int[] array, int value)
	{
		for (int i = 0; i < array.Length; ++i)
		{
			if (array[i] == value)
			{
				return i;
			}
		}

		return -1;
	}

	int[] aInb = new int[VertexPerNode];
	int[] bIna = new int[VertexPerNode];
	private void CheckNeighbor(Node a, Node b)
	{
		for (int i = 0; i < VertexPerNode; ++i)
		{
			int idx = FindFirst(b.indices, a.indices[i]);
			aInb[i] = idx;
		}

		for (int i = 0; i < VertexPerNode; ++i)
		{
			int nextI = (i + 1) % VertexPerNode;
			if (aInb[i] >= 0 && aInb[nextI] >= 0)
			{
				a.neightbors[i] = b;
			}
		}
	}
}