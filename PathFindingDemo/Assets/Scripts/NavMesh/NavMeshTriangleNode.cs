using System.Collections.Generic;
using UnityEngine;

// NavMesh的一个三角形节点
public class NavMeshTriangleNode
{
	// 三个顶点
	public Vector3[] triangles = new Vector3[3];

	// 中心点
	public Vector3 center;

	// 邻居
	// 固定三个邻居，分别是01边，12边，02边; 没有就置空
	public NavMeshTriangleNode[] neighbors = new NavMeshTriangleNode[3];
}