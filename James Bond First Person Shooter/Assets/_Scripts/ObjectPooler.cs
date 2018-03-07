using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObjectPoolItem
{
    public GameObject objectToPool;
    public int amountToPool = 10;
    public bool shouldExpand = true;
}

public class ObjectPooler : MonoBehaviour {

    // Setting public static allows other scripts to access ObjectPooler 
    // without getting a Component from a GameObject
    public static ObjectPooler SharedInstance;
    public List<ObjectPoolItem> itemsToPool;

    private List<GameObject> pooledObjects;

    void Awake()
    {
        // Initialize public static ObjectPooler instance reference
        SharedInstance = this;
    }

	// Use this for initialization
	void Start () {
        pooledObjects = new List<GameObject>();
        foreach (ObjectPoolItem item in itemsToPool)
        {
            for (int i = 0; i < item.amountToPool; i++)
            {
                GameObject obj = (GameObject)Instantiate(item.objectToPool, transform);
                obj.SetActive(false);
                pooledObjects.Add(obj);
            }
        }
	}

    public GameObject GetPooledObject(string tag)
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].activeInHierarchy && pooledObjects[i].tag == tag)
            {
                return pooledObjects[i];
            }
        }
        foreach (ObjectPoolItem item in itemsToPool)
        {
            // If no pooledObject is returned and list is allowed to grow
            if (item.objectToPool.tag == tag && item.shouldExpand)
            {
                GameObject obj = (GameObject)Instantiate(item.objectToPool, transform);
                pooledObjects.Add(obj);
                return obj;
            }
        }

        // If no pooled Object was returned and list is NOT allowed to grow
        // return null since no more objects in object pool
        return null;
    }

    /*
     * Replace Instantiate with:
     * GameObject enemy1 = ObjectPooer.SharedInstance.GetPooledObject("EnemyShip01");
     * if (enemey1 != null) {
     *   enemy1.transform.position = spawnPosition;
     *   enemy1.transform.rotation = spawnRotation;
     *   enemy1.SetActive(true);
     * }
     * 
     * Replace Destroy with:
     * enemy1.SetActive(false);
     */
}
