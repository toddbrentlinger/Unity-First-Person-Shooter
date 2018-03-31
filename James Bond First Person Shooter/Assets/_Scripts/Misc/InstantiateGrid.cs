using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateGrid : MonoBehaviour {

    public GameObject prefab;
    public float gridX = 5f;
    public float gridZ = 5f;
    public float spacing = 2f;

	// Use this for initialization
	void Start () {
		for (int z = 0; z < gridZ; z++)
        {
            for (int x = 0; x < gridX; x++)
            {
                Vector3 pos = new Vector3(x, 0, z) * spacing + transform.position;
                pos.y = prefab.transform.position.y;
                Instantiate(prefab, pos, Quaternion.identity, transform);
            }
        }
	}
}
