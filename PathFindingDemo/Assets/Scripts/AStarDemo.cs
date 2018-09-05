using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarDemo : MonoBehaviour
{
	public enum State
	{
		DrawBlock,
		DrawSource,
		DrawDestination,
		Searching
	}

	State _state;

	public GameObject nodeRoot;
	public MapNode nodeTemplate;
	public int cellCountX;
	public int cellCountY;

	MapNode[,] nodeGrid;

	MapNode srcNode;
	MapNode dstNode;
	

	void Start ()
	{
		nodeGrid = new MapNode[cellCountX, cellCountY];

		Vector3 cellSize = nodeTemplate.Size * 2.2f;

		nodeRoot = new GameObject("NodeRoot");

		// 初始化Grid
		for (int x = 0; x < cellCountX; ++x)
		{
			for (int y = 0; y < cellCountY; ++y)
			{
				var node = Instantiate<MapNode>(nodeTemplate);
				node.coord = new Int2(x, y);
				node.transform.SetParent(nodeRoot.transform);
				node.transform.position = new Vector3(cellSize.x * x, cellSize.y * y, 0);
				node.State = MapNode.MapNodeState.None;
				node.Flaged = false;
				nodeGrid[x, y] = node;
			}
		}
		nodeTemplate.gameObject.SetActive(false);
		_state = State.DrawBlock;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (_state == State.DrawBlock)
		{
			if (Input.GetMouseButton(0))
			{
				var node = GetHitedMapNode(Input.mousePosition);
				if (node)
				{
					HitGraphNode(node, true);
				}
			}
			else if (Input.GetMouseButton(1))
			{
				var node = GetHitedMapNode(Input.mousePosition);
				if (node)
				{
					HitGraphNode(node, false);
				}
			}
		}
		else
		{
			if (Input.GetMouseButtonDown(0))
			{
				var node = GetHitedMapNode(Input.mousePosition);
				if (node)
				{
					HitGraphNode(node, true);
				}
			}
		}
	}

	MapNode GetHitedMapNode(Vector3 mousePosition)
	{
		Camera camera = GetComponent<Camera>();
		if (camera)
		{
			Ray ray = camera.ScreenPointToRay(mousePosition);
			var hits = Physics.RaycastAll(ray);
			if (hits != null && hits.Length > 0)
			{
				for (int i = 0; i < hits.Length; ++i)
				{
					var mapNode = hits[i].collider.GetComponent<MapNode>();
					if (mapNode)
					{
						return mapNode;
					}
				}
			}
		}
		return null;
	}

	void HitGraphNode(MapNode mapNode, bool leftMouse)
	{
		switch (_state)
		{
			case State.DrawBlock:
				mapNode.State = leftMouse ? MapNode.MapNodeState.Block : MapNode.MapNodeState.None;
				break;
			case State.DrawSource:
				if (srcNode)
				{
					srcNode.Flaged = false;
				}
				srcNode = mapNode;
				srcNode.Flaged = true;
				_state = State.DrawDestination;
				break;
			case State.DrawDestination:
				if (dstNode)
				{
					dstNode.Flaged = false;
				}
				dstNode = mapNode;
				dstNode.Flaged = true;
				_state = State.Searching;
				StartCoroutine(PathfindingCor());
				break;
			case State.Searching:
				break;
		}
	}

	void OnGUI()
	{
		if (_state == State.DrawBlock)
		{
			GUILayout.Label("设置障碍");
			if (GUILayout.Button("设置障碍完成"))
			{
				_state = State.DrawSource;
			}
		}
		else if (_state == State.DrawSource)
		{
			GUILayout.Label("选择起始点");
			if (GUILayout.Button("重新设置障碍"))
			{
				ClearMapGrids();
				_state = State.DrawBlock;
			}
			if (GUILayout.Button("重新寻路"))
			{
				_state = State.Searching;
				StartCoroutine(PathfindingCor());
			}
		}
		else if (_state == State.DrawDestination)
		{
			GUILayout.Label("选择目标点");
		}
		else if (_state == State.Searching)
		{
			GUILayout.Label("正在寻路");
		}

		GUILayout.Space(10);
		HeuristicMethod = GUILayout.SelectionGrid(HeuristicMethod, HeuristicTitle, 1);

		GUILayout.Space(10);
		PopMinMethod = GUILayout.SelectionGrid(PopMinMethod, PopMinMethodTitle, 1);
	}

	int HeuristicMethod = 0;
	string[] HeuristicTitle = new string[]
	{
		"直线距离",
		"曼哈顿距离",
		"曼哈顿距离乘以1.2"
	};

	int PopMinMethod = 0;
	string[] PopMinMethodTitle = new string[]
	{
		"弹出F值最小",
		"弹出G值最小",
		"弹出H值最小"
	};
	
	void ClearMapGrids()
	{
		for (int x = 0; x < cellCountX; ++x)
		{
			for (int y = 0; y < cellCountY; ++y)
			{
				var node = nodeGrid[x, y];
				if (node.State != MapNode.MapNodeState.Block)
				{
					node.State = MapNode.MapNodeState.None;
					node.G = 0;
				}
			}
		}
	}

	IEnumerator PathfindingCor()
	{
		ClearMapGrids();

		if (srcNode == null || dstNode == null)
		{
			_state = State.DrawSource;
			yield break;
		}

		List<MapNode> openList = new List<MapNode>();
		openList.Add(srcNode);
		while (true)
		{
			var curNode = GetMinNode(openList);
			if (curNode == null)
			{
				Debug.LogError("Searching Error");
				_state = State.DrawSource;
				yield break;
			}

			if (curNode == dstNode)
			{
				// 到达终点
				while (curNode.parentNode != null)
				{
					curNode.State = MapNode.MapNodeState.Path;
					curNode = curNode.parentNode;
				}

				Debug.Log("Search Success");
				_state = State.DrawSource;
				yield break;
			}

			yield return new WaitForSeconds(0.2f);

			curNode.State = MapNode.MapNodeState.Close;
			for (int i = 0; i < neighborOffset.Length; ++i)
			{
				var neighborCoord = curNode.coord + neighborOffset[i];
				if (IsValidCoord(neighborCoord))
				{
					var neighborNode = nodeGrid[neighborCoord.x, neighborCoord.y];
					if (neighborNode)
					{
						if (neighborNode.State == MapNode.MapNodeState.Block || neighborNode.State == MapNode.MapNodeState.Close)
						{
							continue;
						}
						else
						{
							float newG = curNode.G + neighborStep[i];
							if (neighborNode.State == MapNode.MapNodeState.None)
							{
								neighborNode.H = Heuristic(neighborCoord, dstNode.coord);
								neighborNode.G = newG;
								neighborNode.parentNode = curNode;
								neighborNode.State = MapNode.MapNodeState.Open;
								
								openList.Add(neighborNode);
							}
							else if (neighborNode.State == MapNode.MapNodeState.Open)
							{
								if (newG < neighborNode.G)
								{
									neighborNode.G = newG;
									neighborNode.parentNode = curNode;
								}
							}
						}
					}
				}
			}
		}
	}

	Func<MapNode, MapNode, int> minH = (MapNode a, MapNode b) => { return a.H.CompareTo(b.H); };
	Func<MapNode, MapNode, int> minG = (MapNode a, MapNode b) => { return a.G.CompareTo(b.G); };
	Func<MapNode, MapNode, int> minF = (MapNode a, MapNode b) => { return a.F.CompareTo(b.F); };

	MapNode GetMinNode(List<MapNode> list)
	{
		if (PopMinMethod == 0)
		{
			return list.PopMin(minF);
		}
		else if (PopMinMethod == 1)
		{
			return list.PopMin(minG);
		}
		else if (PopMinMethod == 2)
		{
			return list.PopMin(minH);
		}

		return null;
	}

	// 邻居
	public static readonly Int2[] neighborOffset = new Int2[]
	{
			new Int2(-1, 0),
			new Int2(0, 1),
			new Int2(1, 0),
			new Int2(0, -1),

			new Int2(-1, -1),
			new Int2(-1, 1),
			new Int2(1, 1),
			new Int2(1, -1)
	};

	// 到邻居的距离
	public static float[] neighborStep = new float[]
	{
			1,
			1,
			1,
			1,

			Mathf.Sqrt(2),
			Mathf.Sqrt(2),
			Mathf.Sqrt(2),
			Mathf.Sqrt(2)
	};

	// 欧式距离
	public static float Distance(Int2 a, Int2 b)
	{
		int x = a.x - b.x;
		int y = a.y - b.y;

		return Mathf.Sqrt(x * x + y * y);
	}
	
	// 估值函数
	public float Heuristic(Int2 a, Int2 b)
	{
		if (HeuristicMethod == 0)
		{
			return Distance(a, b);
		}
		else
		{
			int x = a.x - b.x;
			int y = a.y - b.y;
			if (x < 0) x = -x;
			if (y < 0) y = -y;
			if (x > y)
			{
				int t = x;
				x = y;
				y = t;
			}
			float manha = Mathf.Sqrt(2) * x + y - x;
			if (HeuristicMethod == 2)
			{
				manha *= 1.2f;
			}

			return manha;
		}
	}

	// 坐标检查
	public bool IsValidCoord(Int2 coord)
	{
		return (coord.x >= 0 && coord.x < cellCountX && coord.y >= 0 && coord.y < cellCountY);
	}
}
