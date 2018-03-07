using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunTest : MonoBehaviour {

    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletVelocity = 20f;
	
	// Update is called once per frame
	void Update ()
    {
		// Input handling
        if (Input.GetButtonDown("Fire1"))
        {
            Fire();
        }
	}

    void Fire()
    {
        // Create the bullet from the bullet prefab
        GameObject bullet = (GameObject)Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);

        // Add velocity to the bullet
        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * bulletVelocity;

        // Destroy the bullet after 2 seconds
        Destroy(bullet, 2.0f);
    }
}
