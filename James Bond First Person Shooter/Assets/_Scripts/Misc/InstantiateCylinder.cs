using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateCylinder : MonoBehaviour {

    public GameObject block;
    public int columns = 30;
    public int rows = 10;
    public float radius = 6.0f;

	// Use this for initialization
	void Start () {
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                GameObject temp = (GameObject)Instantiate(block, new Vector3(0f, j, radius), Quaternion.identity);
                temp.transform.parent = transform;
            }
            transform.Rotate(0f, 360 / columns, 0f);
        }
	}
}
