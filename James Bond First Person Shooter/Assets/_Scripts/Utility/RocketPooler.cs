using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketPooler : MonoBehaviour {

    // Setting public static allows other scripts to access ObjectPooler 
    // without getting a Component from a GameObject
    public static RocketPooler current;

    public GameObject pooledObject;
    public int pooledAmount = 10;
    public bool shouldExpand = true;

    private List<GameObject> pooledObjectsList;

    void Awake()
    {
        // Initialize public static ObjectPooler current instance reference
        current = this;
    }

    // Use this for initialization
    void Start()
    {
        pooledObjectsList = new List<GameObject>();
        for (int i = 0; i < pooledAmount; i++)
        {
            GameObject obj = (GameObject)Instantiate(pooledObject, transform);
            obj.SetActive(false);
            pooledObjectsList.Add(obj);
        }
    }

    public GameObject GetPooledObject()
    {
        for (int i = 0; i < pooledObjectsList.Count; i++)
        {
            if (!pooledObjectsList[i].activeInHierarchy)
            {
                return pooledObjectsList[i];
            }
        }

        // If no pooledObject is returned and list is allowed to grow
        if (shouldExpand)
        {
            GameObject obj = (GameObject)Instantiate(pooledObject, transform);
            pooledObjectsList.Add(obj);
            return obj;
        }
        // If no pooled Object was returned and list is NOT allowed to grow
        // return null since no more objects in object pool
        return null;
    }
}
