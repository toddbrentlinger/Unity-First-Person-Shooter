using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHoleController : MonoBehaviour {

    [SerializeField]
    [Tooltip("The prefab for the bullet hole")]
    private GameObject bulletHoleDecalPrefab;

    [SerializeField]
    [Tooltip("The number of decals to keep alive at a time. After this number are around, old ones will be replaced.")]
    private int maxConcurrentDecals = 30;

    private Queue<GameObject> decalsInPool;
    private Queue<GameObject> decalsActiveInWorld;

    private void Awake()
    {
        InitializeDecals();
    }

    private void InitializeDecals()
    {
        decalsInPool = new Queue<GameObject>();
        decalsActiveInWorld = new Queue<GameObject>();

        for (int i = 0; i < maxConcurrentDecals; i++)
        {
            InstantiateSingleDecal();
        }
    }

    private void InstantiateSingleDecal()
    {
        // Instantiate prefab decal as child of BulletHoleController gameobject
        GameObject spawned = (GameObject)Instantiate(bulletHoleDecalPrefab, transform);

        // Add decal to decalsInPool
        decalsInPool.Enqueue(spawned);

        // Deactivate decal
        spawned.SetActive(false);
    }

    // Public method to spawn decal using RaycastHit
    public void SpawnDecal(RaycastHit hit)
    {
        GameObject decal = GetNextAvailableDecal();
        if (decal != null)
        {
            decal.transform.position = hit.point + hit.normal * .01f;
            decal.transform.rotation = Quaternion.FromToRotation(-Vector3.forward, hit.normal);

            decal.SetActive(true);

            decalsActiveInWorld.Enqueue(decal);
        }
    }

    private GameObject GetNextAvailableDecal()
    {
        // If there is a decal in pool, return decal from decalsInPool
        if (decalsInPool.Count > 0)
            return decalsInPool.Dequeue();
        // Else there is no decal in pool, return decal from decalsActiveInWorld
        else
            return decalsActiveInWorld.Dequeue();
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
