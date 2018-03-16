using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletFire : MonoBehaviour {

    public float bulletVelocity = 20f;

    // Update is called once per frame
    void Update()
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
        GameObject bullet = ObjectPooler.sharedInstance.GetPooledObject("Bullet01");

        // If bullet is null, there is no bullet to fire
        if (bullet == null) return;

        bullet.transform.position = transform.position;
        bullet.transform.rotation = transform.rotation;
        bullet.SetActive(true);

        // Add velocity to the bullet
        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * bulletVelocity;
    }
}
