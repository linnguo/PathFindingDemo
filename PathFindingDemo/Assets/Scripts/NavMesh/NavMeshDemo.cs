using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshDemo : MonoBehaviour
{
	public Mesh manulNavMesh;
	public MeshFilter navMeshFilter;
	public Material material;
	
	// public LineRenderer

	NavMeshModel navMeshModel = null;

	// 指示线
	public LineRenderer redLeft;
	public LineRenderer blueRight;
	public LineRenderer pathLine;

	public enum State
	{
		WaitSrc,
		WaitDst,
		Calcing,
	}
	State state = State.WaitSrc;
	Vector3 srcPos;
	Vector3 dstPos;

	private void Start()
	{
		LoadNavMesh();
	}

	private void Update()
	{
		UpdateLines();
	}

	private void UpdateLines()
	{
		if (navMeshModel != null)
		{
			if (redLeft != null)
			{
				redLeft.positionCount = 2;
				redLeft.SetPosition(0, navMeshModel.curCorner);
				redLeft.SetPosition(1, navMeshModel.left);
			}

			if (blueRight != null)
			{
				blueRight.positionCount = 2;
				blueRight.SetPosition(0, navMeshModel.curCorner);
				blueRight.SetPosition(1, navMeshModel.right);
			}

			if (pathLine != null)
			{
				pathLine.positionCount = navMeshModel.corners.Count;
				pathLine.SetPositions(navMeshModel.corners.ToArray());
			}
		}
	}
	
	private void OnGUI()
	{
		/*
		if (GUILayout.Button("Export"))
		{
			ExportNavMesh();
		}

		if (GUILayout.Button("Load"))
		{
			LoadNavMesh();
		}
		*/

		if (Event.current != null)
		{
			if (Event.current.type == EventType.MouseDown)
			{
				var screenPos = Event.current.mousePosition;
				screenPos.y = Screen.height - screenPos.y;
				var ray = Camera.main.ScreenPointToRay(screenPos);
				Plane p = new Plane(Vector3.up, 0);
				float point;
				if (p.Raycast(ray, out point))
				{
					var worldPos = ray.GetPoint(point);
					if (navMeshModel != null)
					{
						int nodeClicked = -1;
						if (navMeshModel.RayCast(worldPos, out nodeClicked))
						{
							OnClickedNavMesh(worldPos);
						}
					}
				}
			}
		}
	}

	void OnClickedNavMesh(Vector3 worldPos)
	{
		if (state == State.WaitSrc)
		{
			srcPos = worldPos;
			state = State.WaitDst;
		}
		else if (state == State.WaitDst)
		{
			dstPos = worldPos;
			StartCoroutine(PathFindingCor());
		}
	}

	IEnumerator PathFindingCor()
	{
		state = State.Calcing;

		if (navMeshModel != null)
		{
			navMeshModel.srcPos = srcPos;
			navMeshModel.dstPos = dstPos;
			yield return StartCoroutine(navMeshModel.FindCor());
			yield return StartCoroutine(navMeshModel.CalcCornersCor());
		}

		state = State.WaitSrc;
		yield break;
	}

	public void ExportNavMesh()
	{
		NavMeshTriangulation navMeshTriangulation = NavMesh.CalculateTriangulation();
		Vector3[] vertices = null;
		int[] indices = null;
		NavMeshTools.MergeVertex(navMeshTriangulation.vertices, navMeshTriangulation.indices, out vertices, out indices);
		string objContent = NavMeshTools.SerializeToObj(vertices, indices);
		string objPath = Path.Combine(Application.dataPath, "navmesh.obj");
		File.WriteAllText(objPath, objContent);
	}

	public void LoadNavMesh()
	{
		if (manulNavMesh != null)
		{
			navMeshModel = new NavMeshModel(manulNavMesh.vertices, manulNavMesh.triangles);
			if (navMeshFilter != null)
			{
				navMeshFilter.mesh = navMeshModel.NavRenderMesh;
			}
		}
	}
}
