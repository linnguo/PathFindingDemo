using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public partial class NavMeshDemo : MonoBehaviour
{
	public Mesh manulNavMesh;
	public MeshFilter navMeshFilter;
	public Material material;
	NavMeshModel navMeshModel = null;


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
							var node = navMeshModel.nodes[nodeClicked];
							node.nodeState = NodeState.Close;
							navMeshModel.RefreshMesh();
						}
					}
					
				}
			}
		}
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
			
			foreach (var node in navMeshModel.nodes)
			{
				foreach (var nei in node.neightbors)
				{
					if (nei != null)
					{
						GL.Vertex(node.center);
						GL.Vertex(nei.center);
					}
				}
			}
			
			GL.End();
		}
		
	}
}
