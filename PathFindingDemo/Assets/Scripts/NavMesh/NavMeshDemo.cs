using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public partial class NavMeshDemo : MonoBehaviour
{
	public Material material;
	NavMeshTriangulation? navMeshTriangulation = null;
	NavMeshModel navMeshModel = null;

	private void OnGUI()
	{
		if (GUILayout.Button("Export"))
		{
			ExportNavMesh();
		}
	}

	public void ExportNavMesh()
	{
		navMeshTriangulation = NavMesh.CalculateTriangulation();
		navMeshModel = new NavMeshModel(navMeshTriangulation.Value);
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

				GL.Vertex(node.vertices[0]);
				GL.Vertex(node.vertices[1]);

				GL.Vertex(node.vertices[1]);
				GL.Vertex(node.vertices[2]);

				GL.Vertex(node.vertices[0]);
				GL.Vertex(node.vertices[2]);
			}
			
			GL.End();
		}
		
	}
}
