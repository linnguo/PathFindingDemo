using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum NodeState
{
	None,
	Open,
	Close,

	Path
}

public class Node
{
	public int[] indices = new int[3];
	public Node[] neightbors = new Node[3];

	public Vector3 center;
	public Vector3[] vertices = new Vector3[3];

	public NodeState state = NodeState.None;
	public Node parent;
	public float H;
	public float G;
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
			for (int j = i+1; j < nodes.Count; ++j)
			{
				CheckNeighbor(nodes[j], nodes[i]);
				CheckNeighbor(nodes[i], nodes[j]);
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
				return Color.gray;
			case NodeState.Open:
				return Color.green;
			case NodeState.Close:
				return Color.red;
			case NodeState.Path:
				return Color.blue;
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
				meshColors[index + j] = GetStateColor(nodes[i].state);
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

	public void ResetState()
	{
		foreach (var node in nodes)
		{
			node.state = NodeState.None;
			node.G = 0;
			node.H = 0;
			node.parent = null;
		}
		RefreshMesh();
	}

	public List<Node> pathNodes = new List<Node>();
	public IEnumerator FindCor(Vector3 srcPos, Vector3 dstPos)
	{
		int srcIdx = -1;
		int dstIdx = -1;
		if (!RayCast(srcPos, out srcIdx) || !RayCast(dstPos, out dstIdx))
		{
			yield break;
		}

		ResetState();
		pathNodes.Clear();

		Node dstNode = nodes[dstIdx];
		List<Node> open = new List<Node>();
		open.Add(nodes[srcIdx]);
		while (true)
		{
			Node minOpen = open.PopMin((Node a, Node b) => {
				return (a.G + a.H).CompareTo(b.G + b.H);
			});

			if (minOpen == null)
			{
				Debug.Log("Cant found path");
				yield break;
			}

			if (minOpen == dstNode)
			{
				while (minOpen != null)
				{
					pathNodes.Add(minOpen);
					minOpen.state = NodeState.Path;
					minOpen = minOpen.parent;
				}
				pathNodes.Reverse();
				RefreshMesh();
				Debug.Log("Path found Success");
				yield break;
			}

			minOpen.state = NodeState.Close;
			foreach (var nei in minOpen.neightbors)
			{
				if (nei != null)
				{
					if (nei.state == NodeState.None)
					{
						nei.state = NodeState.Open;
						nei.G = minOpen.G + Vector3.Distance(minOpen.center, nei.center);
						nei.H = Vector3.Distance(nei.center, dstNode.center);
						nei.parent = minOpen;
						open.Add(nei);
					}
					else if (nei.state == NodeState.Open)
					{
						var curG = minOpen.G + Vector3.Distance(minOpen.center, nei.center);
						if (curG < nei.G)
						{
							nei.G = curG;
							nei.parent = minOpen;
						}
					}
				}
			}

			RefreshMesh();
			yield return new WaitForSeconds(0.2f);
		}
	}

	public Vector3 curCorner;
	public Vector3 left;
	public Vector3 right;
	
	public List<Vector3> corners = new List<Vector3>();

	public struct Segment
	{
		public Vector3 left;
		public Vector3 right;
	}
	public List<Segment> gates = new List<Segment>();

	void InitGates()
	{
		gates.Clear();
		for (int i = 0; i < pathNodes.Count - 1; ++i)
		{
			var curNode = pathNodes[i];
			var nexNode = pathNodes[i + 1];

			for (int j = 0; j < 3; ++j)
			{
				var nei = curNode.neightbors[j];
				if (nei == nexNode)
				{
					Vector3 curLeft;
					Vector3 curRight;

					DivLeftRight(curNode.center, curNode.vertices[j], curNode.vertices[(j + 1) % 3], out curLeft, out curRight);
					Segment seg = new Segment();
					seg.left = curLeft;
					seg.right = curRight;
					gates.Add(seg);
					break;
				}
			}
		}
	}

	/// <summary>
	/// 拐角点计算路径
	/// </summary>
	/// <param name="srcPos"></param>
	/// <param name="dstPos"></param>
	/// <param name="nodeList">需要包含起点和终点的node</param>
	/// <returns></returns>
	public IEnumerator CalcCornersCor(Vector3 srcPos, Vector3 dstPos)
	{
		corners.Clear();
		corners.Add(srcPos);

		curCorner = srcPos;

		left = curCorner;
		right = curCorner;

		int leftIdx = 0;
		int rightIdx = 0;

		yield return new WaitForSeconds(0.5f);

		InitGates();
		
		for (int i = 0; i < gates.Count; ++i)
		{
			Vector3 curLeft = gates[i].left;
			Vector3 curRight = gates[i].right;
			
			yield return new WaitForSeconds(0.2f);
			
			if (left == curLeft)
			{
				// 如果这个left和当前的值一样，不用处理
			}
			else if (curCorner == left)
			{
				// 如果当前没有合理的left
				left = curLeft;
				leftIdx = i;
			}
			else if (IsLeft(curCorner, left, curLeft))
			{
				// 新的左边在旧的左边的左边，不用处理
			}
			else if (IsRight(curCorner, right, curLeft))
			{
				// 新的左边在旧的右边的右边，拐点
				curCorner = right;
				corners.Add(curCorner);

				// 回退
				i = rightIdx;

				left = curCorner;
				right = curCorner;
				leftIdx = i;
				rightIdx = i;
				continue;
			}
			else
			{
				// 新的左边在旧的左右两边之间
				left = curLeft;
				leftIdx = i;
			}

			if (right == curRight)
			{
				// 如果这个right和当前的值一样，不用处理
			}
			else if (curCorner == right)
			{
				// 如果当前没有合理的right
				right = curRight;
				rightIdx = i;
			}
			else if (IsRight(curCorner, right, curRight))
			{
				// 新的右边在旧的右边的右边，不用处理
			}
			else if (IsLeft(curCorner, left, curRight))
			{
				// 新的右边在旧的左边的左边，拐点
				curCorner = left;
				corners.Add(curCorner);

				// 回退
				i = leftIdx;

				left = curCorner;
				right = curCorner;
				leftIdx = i;
				rightIdx = i;

				continue;
			}
			else
			{
				// 新的右边在旧的左右两边之间
				right = curRight;
				rightIdx = i;
			}
		}

		corners.Add(dstPos);
		yield break;
	}

	// 区分左右
	void DivLeftRight(Vector3 center, Vector3 p0, Vector3 p1, out Vector3 left, out Vector3 right)
	{
		p0.y = 0;
		p1.y = 0;
		center.y = 0;

		float crossY = Vector3.Cross(p0 - center, p1 - center).y;

		if (crossY > 0)
		{
			left = p0;
			right = p1;
		}
		else
		{
			left = p1;
			right = p0;
		}
	}

	// 判断p是不是在center -> to的左边
	bool IsLeft(Vector3 center, Vector3 to, Vector3 p)
	{
		center.y = 0;
		to.y = 0;
		p.y = 0;
		return Vector3.Cross(to - center, p - center).y < 0;
	}

	// 判断p是不是在center -> to的左边
	bool IsRight(Vector3 center, Vector3 to, Vector3 p)
	{
		center.y = 0;
		to.y = 0;
		p.y = 0;
		return Vector3.Cross(to - center, p - center).y > 0;
	}
}