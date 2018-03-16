using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObjectPoolItemOld
{
    public GameObject objectToPool;
    public int amountToPool = 10;
    public bool shouldExpand = true;
}

public class ObjectPoolerOld : MonoBehaviour {

    // Setting public static allows other scripts to access ObjectPooler 
    // without getting a Component from a GameObject
    public static ObjectPoolerOld SharedInstance;

    public List<ObjectPoolItemOld> itemsToPool; // List of ObjectPoolItem instances with properties of object to pool
    private List<GameObject> pooledObjects; // List of all objects in pool that are NOT active and waiting to be used

    void Awake()
    {
        // Initialize public static ObjectPooler instance reference
        SharedInstance = this;
    }

    // Use this for initialization
    void Start()
    {
        pooledObjects = new List<GameObject>();
        foreach (ObjectPoolItemOld item in itemsToPool)
        {
            // Add requested number of pooled objects to each object pool gameobject and list
            for (int i = 0; i < item.amountToPool; i++)
            {
                GameObject obj = (GameObject)Instantiate(item.objectToPool, transform);
                obj.SetActive(false);
                pooledObjects.Add(obj);
            }
        }
    }

    // Public method to get pooled object given object tag
    public GameObject GetPooledObject(string tag)
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            // If pooledObject is NOT active in heirarchy and it's tag matches tag parameter
            if (!pooledObjects[i].activeInHierarchy && pooledObjects[i].tag == tag)
            {
                // Return pooledObject
                return pooledObjects[i];
            }
        }

        // No pooledObject is available or matches tag parameter
        // If ObjectPoolItem should expand, add new object to pool and return object
        foreach (ObjectPoolItemOld item in itemsToPool)
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
