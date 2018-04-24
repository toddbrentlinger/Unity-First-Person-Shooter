using System.Collections.Generic;
using UnityEngine;

/* NOTES:
 * - Use BulletHoleController as reference for Queued behavior when shouldExpand is false.
 * - Use List.Find(x => x.isActive()) to find first occurrence where pooledObject is active
 *   or loop through each index stopping at first occurrence where object is active and then
 *   stopping at first occurence where object is inactive. Simulate queue using list. 
 */

[System.Serializable]
public class ObjectPoolItem
{
    [Tooltip("Item name")]
    public string groupName;
    [Tooltip("The prefab to be pooled.")]
    public GameObject objectToPool;
    [Tooltip("The number of objects to pool. Will grow if shouldExpand is true.")]
    public int amountToPool = 10;
    [Tooltip("Should the number of pooled objects expand. If false, objects are queued (first-in, first-out).")]
    public bool shouldExpand = true;

    private List<GameObject> singlePooledObjects = new List<GameObject>();

    public void AddPooledObject(GameObject obj)
    {
        singlePooledObjects.Add(obj);
    }

    public GameObject GetPooledObject()
    {
        // Loop through singlePooledObjects to find any available object
        for (int i = 0; i < singlePooledObjects.Count; i++)
        {
            // If nth pooled object is NOT active in heirarchy
            if (!singlePooledObjects[i].activeInHierarchy)
            {
                // Return nth pooled object
                return singlePooledObjects[i];
            }
        }

        // No object is available in pool

        // If should NOT expand, treat List like Queue
        if (!shouldExpand)
        {
            // Get first object in object pool (longest living object)
            GameObject obj = singlePooledObjects[0];

            // Remove object from beginning of list
            singlePooledObjects.RemoveAt(0);

            // Add object to end of List (shortest living object)
            singlePooledObjects.Add(obj);

            // Deactivate object
            obj.SetActive(false);

            // Return object
            return obj;
        }

        // No object available and pool should expand

        return null;
    }
}

public class ObjectPooler : MonoBehaviour {

    // Setting public static allows other scripts to access ObjectPooler 
    // without getting a Component from a GameObject
    public static ObjectPooler sharedInstance;

    public List<ObjectPoolItem> itemsToPool; // List of ObjectPoolItem instances with properties of object to pool
    // private List<GameObject> pooledObjects; // List of all objects in pool that are NOT active and waiting to be used

    void Awake()
    {
        // Initialize public static ObjectPooler instance reference
        sharedInstance = this;

        // Initialize object pools
        FillObjectPools();
    }

    // Method to fill each object pool
    private void FillObjectPools()
    {
        // For each ObjectPoolItem in itemsToPool, fill the individual object pool
        foreach (ObjectPoolItem item in itemsToPool)
        {
            // Add requested number of pooled objects
            for (int i = 0; i < item.amountToPool; i++)
            {
                // Instantiate prefab as child of ObjectPooler gameobject
                GameObject obj = (GameObject)Instantiate(item.objectToPool, transform);

                // Deactivate object
                obj.SetActive(false);

                // Add object to singlePooledObjects List variable in item
                item.AddPooledObject(obj);
            }
        }
    }

    // Public method to get pooled object given object tag
    public GameObject GetPooledObject(string name)
    {
        foreach (ObjectPoolItem item in itemsToPool)
        {
            //if (item.objectToPool.tag == name)
            if (item.groupName == name)
            {
                GameObject obj = item.GetPooledObject();

                // If object exists, NOT null
                if (obj)
                    return obj;

                // Object does NOT exist, equal to null
                // If object pool should expand, add new object instance to pool
                if (item.shouldExpand)
                {
                    // Instantiate prefab as child of ObjectPooler gameobject
                    obj = (GameObject)Instantiate(item.objectToPool, transform);

                    // Deactivate object
                    obj.SetActive(false);

                    // Add object to singlePooledObjects in item
                    item.AddPooledObject(obj);

                    // Return object
                    return obj;
                }

                // Object does NOT exist and should NOT expand, return null
                return null;
            }
        }

        // No item in itemsToPool matches tag argument, return null
        return null;
    }

    /* HOW TO USE:
     * 
     * Replace Instantiate with:
     * GameObject enemy1 = ObjectPooler.SharedInstance.GetPooledObject("EnemyShip01");
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
