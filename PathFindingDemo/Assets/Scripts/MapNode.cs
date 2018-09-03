using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MissQ;

public class MapNode : MonoBehaviour
{
	public enum MapNodeState
	{
		Block,
		None,
		Open,
		Close
	}

	Color noneColor = Color.gray;
	Color openColor = Color.green;
	Color closeColor = Color.red;
	Color blockColor = Color.black;

	void SetCubeColor(Color color)
	{
		if (cubeRender != null)
		{
			cubeRender.material.color = color;
		}
	}

	public Renderer cubeRender;
	public GameObject flag;
	public Int2 coord;
	public float G;
	public float H;
	public float F
	{
		get
		{
			return G + H;
		}
	}
	public MapNode parentNode;
	public Vector3 Size
	{
		get
		{
			var collider = GetComponent<Collider>();
			if (collider)
			{
				return collider.bounds.extents;
			}
			return Vector3.one;
		}
	}

	public bool Flaged
	{
		set
		{
			if (flag != null)
			{
				flag.SetActive(value);
			}
		}
	}
	
	private MapNodeState _state = MapNodeState.None;
	public MapNodeState State
	{
		get
		{
			return _state;
		}
		set
		{
			_state = value;
			SetStateColor(value);
		}
	}

	void SetStateColor(MapNodeState state)
	{
		switch (state)
		{
			case MapNodeState.Block:
				SetCubeColor(blockColor);
				break;
			case MapNodeState.None:
				SetCubeColor(noneColor);
				break;
			case MapNodeState.Open:
				SetCubeColor(openColor);
				break;
			case MapNodeState.Close:
				SetCubeColor(closeColor);
				break;
		}
	}

	private void OnDrawGizmos()
	{
		if ((_state == MapNodeState.Open || _state == MapNodeState.Close) && parentNode != null)
		{
			Gizmos.matrix = Matrix4x4.identity;
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(transform.position, parentNode.transform.position);
		}
	}
}
