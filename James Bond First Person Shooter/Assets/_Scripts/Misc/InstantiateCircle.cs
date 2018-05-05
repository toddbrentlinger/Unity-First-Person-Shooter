using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateCircle : MonoBehaviour
{
    public GameObject prefab;
    public int numberOfObjects = 20;
    public float radius = 5f;

	private void Start()
    {
		for (int i = 0; i < numberOfObjects; i++)
        {
            float angle = i * Mathf.PI * 2 / numberOfObjects;
            Vector3 pos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius + transform.position;
            pos.y = transform.position.y;
            Instantiate(prefab, pos, Quaternion.identity, transform);
        }
	}
}
