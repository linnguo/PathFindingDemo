using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public partial class NavMeshDemo : MonoBehaviour
{
	public Mesh manulNavMesh;
	public MeshFilter navMeshFilter;
	public Material material;
	
	// public LineRenderer

	NavMeshModel navMeshModel = null;

	public enum State
	{
		WaitSrc,
		WaitDst,
		Calcing,
	}
	State state = State.WaitSrc;
	Vector3 srcPos;
	Vector3 dstPos;

	private void OnGUI()
	{
		if (GUILayout.Button("Export"))
		{
			ExportNavMesh();
		}

		if (GUILayout.Button("Load"))
		{
			LoadNavMesh();
		}
		
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
			yield return StartCoroutine(navMeshModel.FindCor(srcPos, dstPos));
			yield return StartCoroutine(navMeshModel.CalcCornersCor(srcPos, dstPos));
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

	private void OnPostRender()
	{
		if (Camera.main != null && navMeshModel != null)
		{
			if (material != null)
			{
				material.SetPass(0);
			}

			GL.modelview = Camera.main.worldToCameraMatrix;
			GL.LoadProjectionMatrix(Camera.main.projectionMatrix);

			GL.Begin(GL.LINES);
			GL.Color(Color.red);
			
			GL.Color(Color.red);
			GL.Vertex(navMeshModel.curCorner);
			GL.Vertex(navMeshModel.left);

			GL.Color(Color.blue);
			GL.Vertex(navMeshModel.curCorner);
			GL.Vertex(navMeshModel.right);

			GL.Color(Color.white);
			for (int i = 0; i < navMeshModel.corners.Count - 1; ++i)
			{
				GL.Vertex(navMeshModel.corners[i]);
				GL.Vertex(navMeshModel.corners[i + 1]);
			}

			GL.End();
		}
	}
}
