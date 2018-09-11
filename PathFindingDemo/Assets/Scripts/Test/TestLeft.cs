using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLeft : MonoBehaviour
{
	public GameObject redFrom;
	public GameObject blueTo;
	public GameObject greenFlag;
	
	void Start ()
	{
		
	}
	
	void Update ()
	{
		if (redFrom == null || blueTo == null || greenFlag == null)
		{
			return;
		}
		SetGameObjectY(redFrom, 0);
		SetGameObjectY(blueTo, 0);
		SetGameObjectY(greenFlag, 0);
	}

	void SetGameObjectY(GameObject go, float y)
	{
		if (go)
		{
			Vector3 lp = go.transform.position;
			lp.y = 0;
			go.transform.position = lp;
		}
	}
}
