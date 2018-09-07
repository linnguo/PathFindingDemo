using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum NodeState
{
	None,
	Open,
	Close
}

public class Node
{
	public int[] indices = new int[3];
	public Node[] neightbors = new Node[3];

	public Vector3 center;
	public Vector3[] vertices = new Vector3[3];

	public NodeState nodeState = NodeState.None;
}

public class NavMeshModel
{
	const int VertexPerNode = 3;

	// NavMesh给的原始资源, 经过了合并
	Vector3[] navMeshVertice;
	int[] navMeshIndice;
	
	// 显示用的模型
	public Mesh NavRenderMesh { get; private set; }
	Vector3[] meshVertices;
	int[] meshIndice;
	Color[] meshColors;
	
	// 节点
	public List<Node> nodes;
	
	public NavMeshModel(Vector3[] aNavMeshVertice, int[] aNavMeshIndice)
	{
		NavMeshTools.MergeVertex(aNavMeshVertice, aNavMeshIndice, out navMeshVertice, out navMeshIndice);

		InitNodes();
		InitNeighbors();

		RefreshMesh();
	}

	private void InitNodes()
	{
		nodes = new List<Node>();
		
		int nodesCount = navMeshIndice.Length / VertexPerNode;

		for (int nodeIndex = 0; nodeIndex < nodesCount; ++nodeIndex)
		{
			Node node = new Node();
			Vector3 center = Vector3.zero;
			for (int i = 0; i < VertexPerNode; ++i)
			{
				int index = navMeshIndice[nodeIndex * VertexPerNode + i];
				Vector3 vertex = navMeshVertice[index];
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

	private Vector3 Cut(Vector3 a, Vector3 b, float cutoffLength)
	{
		return a + (b - a).normalized * ((b - a).magnitude - cutoffLength);
	}

	Color GetStateColor(NodeState nodeState)
	{
		switch (nodeState)
		{
			case NodeState.None:
				return Color.white;
			case NodeState.Open:
				return Color.green;
			case NodeState.Close:
				return Color.red;
		}

		return Color.gray;
	}

	public void RefreshMesh()
	{
		if (meshVertices == null || meshIndice == null || meshColors == null)
		{
			meshVertices = new Vector3[nodes.Count * 3];
			for (int i = 0; i < nodes.Count; ++i)
			{
				int index = i * 3;
				for (int j = 0; j < 3; ++j)
				{
					meshVertices[index + j] = Cut(nodes[i].center, nodes[i].vertices[j], 0.1f);
				}
			}

			meshIndice = new int[nodes.Count * 3];
			for (int i = 0; i < meshIndice.Length; ++i)
			{
				meshIndice[i] = i;
			}

			meshColors = new Color[nodes.Count * 3];
		}

		for (int i = 0; i < nodes.Count; ++i)
		{
			int index = i * 3;
			
			for (int j = 0; j < 3; ++j)
			{
				meshColors[index + j] = GetStateColor(nodes[i].nodeState);
			}
		}

		if (NavRenderMesh == null)
		{
			NavRenderMesh = new Mesh();
			NavRenderMesh.vertices = meshVertices;
			NavRenderMesh.triangles = meshIndice;
		}

		NavRenderMesh.colors = meshColors;
	}

	private bool RayCastTriangle(Vector3 worldPos, Vector3 a, Vector3 b, Vector3 c)
	{
		worldPos.y = 0;
		a.y = 0;
		b.y = 0;
		c.y = 0;

		return Vector3.Dot(Vector3.Cross(b - a, worldPos - a), Vector3.Cross(worldPos - a, c - a)) >= 0 &&
			Vector3.Dot(Vector3.Cross(a - b, worldPos - b), Vector3.Cross(worldPos - b, c - b)) >= 0;
	}

	// 垂直检测
	public bool RayCast(Vector3 worldPos, out int nodeIndex)
	{
		for (int i = 0; i < nodes.Count; ++i)
		{
			if (RayCastTriangle(worldPos, nodes[i].vertices[0], nodes[i].vertices[1], nodes[i].vertices[2]))
			{
				nodeIndex = i;
				return true;
			}
		}
		nodeIndex = -1;
		return false;
	}
}