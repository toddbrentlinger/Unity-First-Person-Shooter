using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunCustomOldV3 : MonoBehaviour {

    /* NOTES:
     * - Add ammo limit before have to reload by pressing "R"
     * - Add ability to aim down sights by holding, or toggling, Right-Mouse. Animate gun into position closer to camera with sights aligned to middle of screen.
     * - If target of Raycast is enemy, get access to enemy script of target do change its behavior
     * - Use Coroutine instead of Update. Check performance difference.
     */

    public int clipSize = 8; // Number of bullets in single clip before having to reload
    public int gunDamage = 1; // How much damage is applied to target health. Interacts with Health script on target
    public float fireRate = 0.1f; // If fulAuto is true, time between rounds fired when button is held down
    public bool fullAuto = true; // Bool representing if weapon is full-auto or semi-auto

    public float weaponRange = 50f; // Length of ray cast
    public float bulletForce = 2.0f; // Force of bullet on rigidbodies

    private Transform bulletSpawn; // Transform of point to spawn bullet firing effects
    private ParticleSystem muzzleFlash; // Muzzle flash particle system to play when gun fires
    private LineRenderer bulletTrail; // Bullet trail line renderer

    private GameObject bulletHoleClone; // Reference to bulletHole from object pool

    private bool moveTarget = false; // Bool to check if target hit by raycast has rigidbody in update() and then change rigidbody in fixedupdate()
    private RaycastHit targetRaycastHit; // Reference to target of raycast hit with a rigidbody to be moved in FixedUpdate()

    private Animator gunAnimator; // Weapon animator
    private AudioSource gunAudioSource; // Weapon audioSource

    private Enemy targetEnemy; // Reference to Enemy instance if target has enemy tag
    // NOTE: Can I somehow use a static variable for Enemy class instead of using GetComponent<Enemy>() on each enemy hit?

    private float nextShotTime = 0f; // Global time full-auto weapon can fire again between shots while button is held down

    void Awake()
    {
        Transform gunModel = transform.Find("GunModel");
        gunAnimator = gunModel.GetComponent<Animator>();
        gunAudioSource = gunModel.GetComponent<AudioSource>();

        bulletSpawn = transform.Find("BulletEffects");
        muzzleFlash = bulletSpawn.Find("MuzzleFlash").gameObject.GetComponent<ParticleSystem>();
        bulletTrail = bulletSpawn.Find("BulletLineRenderer").GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        // If Fire1 button is held down and current time is larger than nextShotTime
        if (Input.GetButton("Fire1") && Time.time > nextShotTime)
        {
            // Set nextShotTime of current shot before next shot can be fired by adding cooldown timer
            nextShotTime = Time.time + fireRate;

            // Play gun barrel fire effects
            muzzleFlash.Play();

            // Play sound effect
            gunAudioSource.Play();

            // Play cartridge eject effect

            // Set Fire animation parameter on animator to true
            gunAnimator.SetTrigger("Fire");

            Vector2 screenCenterPoint = new Vector2(Screen.width / 2, Screen.height / 2);

            Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            RaycastHit hit;

            // Bit shift the index of the layer (8) to get a bit mask
            int layerMask = 1 << 8;
            // This would cast rays only against colliders in layer 8(Player).
            // But instead we want to collide against everything except layer 8(Player). 
            // The ~ operator does this, it inverts a bitmask.
            layerMask = ~layerMask;
            // NOTE: Could use IgnoreRaycast layer instead?

            // Get bulletHoleClone from ObjectPooler
            bulletHoleClone = ObjectPooler.sharedInstance.GetPooledObject("BulletHole");

            // If there is a bulletHoleClone and physics raycast hits a collider
            if (bulletHoleClone != null && Physics.Raycast(ray, out hit, weaponRange, layerMask))
            {
                // Get bulletHolePosition a short distance above the hit point
                Vector3 bulletHolePosition = hit.point + hit.normal * .01f;

                // Get bulletHoleRotation by randomly spinning bulletHole and orient in line with hit point normal
                Quaternion randRot = Quaternion.Euler(Vector3.forward * Random.Range(0, 360));
                Quaternion bulletHoleRotation = Quaternion.FromToRotation(-Vector3.forward, hit.normal) * randRot;

                // Set bullet hole position and rotation
                bulletHoleClone.transform.position = bulletHolePosition;
                bulletHoleClone.transform.rotation = bulletHoleRotation;
                // Set bullet hole parent to object that raycasthit hits
                bulletHoleClone.transform.SetParent(hit.transform);
                // Set bullet hole active
                bulletHoleClone.SetActive(true);

                // If collider gameobject has rigidbody AND is NOT kinematic, add force from bullet impact
                if (hit.rigidbody != null && !hit.rigidbody.isKinematic)
                {
                    // Set reference to 
                    targetRaycastHit = hit;
                    moveTarget = true;
                }

                // If collider is an Enemy AND isKinematic
                if (hit.transform.tag == "Enemy" && hit.rigidbody.isKinematic)
                {
                    targetEnemy = hit.transform.GetComponent<Enemy>();

                    // Normalized vector between hit point and bullet spawn multiplied by bulletForce factor
                    Vector3 forceVec = (hit.point - bulletSpawn.transform.position).normalized * bulletForce;

                    // Call Damage() method on Enemy instance with forceVector and hit point arguments
                    targetEnemy.Damage(gunDamage, forceVec, hit.point);
                }
            }
        }
    }

    void FixedUpdate()
    {
        // moveTarget is true if collider has rigidbody and is NOT kinematic
        if (moveTarget)
        {
            // Normalized vector between hit point and bullet spawn multiplied by bulletForce factor
            Vector3 forceVec = (targetRaycastHit.point - bulletSpawn.transform.position).normalized * bulletForce;
            // Add impulse force to target rigidbody
            targetRaycastHit.rigidbody.AddForceAtPosition(forceVec, targetRaycastHit.point, ForceMode.Impulse);
            // Set moveTarget bool to false
            moveTarget = false;
        }
    }
}
