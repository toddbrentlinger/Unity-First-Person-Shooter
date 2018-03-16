using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunCustom : MonoBehaviour {

    /* NOTES:
     * X Add ammo limit before have to reload by pressing "R"
     * - Add ability to aim down sights by holding, or toggling, Right-Mouse. Animate gun into position closer to camera with sights aligned to middle of screen.
     * - If target of Raycast is enemy, get access to enemy script of target do change its behavior
     * - Use Coroutine instead of Update. Check performance difference.
     * - Eject physical objects or sprites of empty cartridge that disappear after a while?
     * X Add firingMode as enum with choices "Semi-Auto", "Burst", "Full-Auto" to act as a dropdown of choices for the property
     * - Put ammoCountText behavior in GameController and access GameController script from this class
     * - Weapons have damage dropoff at a distance, and will disappear entirely if the shot travels too far
     * - Put FireWeapon as coroutine when calculating damage dropoff, speed of bullet (not instantaneous), and bullet drop from gravity
     */

    /* Public Properties */

    public int clipSize = 8; // Number of bullets in single clip before having to reload
    public int gunDamage = 1; // How much damage is applied to target health. Interacts with Health script on target
    [Tooltip("The rate of fire in rounds per minute (RPM)")]
    public float fireRate = 350f; // If fulAuto is true, time between rounds fired when button is held down
    // public bool fullAuto = true; // Bool representing if weapon is full-auto or semi-auto

    // Enumeration of FiringMode to choose weapon firing mode
    public enum FiringMode { SemiAuto, Burst, FullAuto };
    public FiringMode firingMode = FiringMode.SemiAuto; // firingMode initialized to semi-auto

    public float weaponRange = 50f; // Length of ray cast to test against colliders
    public float bulletForce = 2.0f; // Force of bullet on rigidbodies

    public AudioClip fireWeaponAudioClip; // AudioClip of weapon firing
    public AudioClip emptyChamberAudioClip; // AudoClip of empty chamber click
    public AudioClip weaponReloadAudioClip; // AudioClip of weapon reloading

    /* Private Properties */

    private bool canShoot = true; // Bool to see if weapon can be shot (Ex. should NOT fire when reloading), initialized to true so weapon will fire
    private int ammoInClip; // Reference to current amount of ammo in clip
    private float nextShotTime = 0f; // Global time full-auto weapon can fire again between shots while button is held down

    private bool isAiming = false;

    private Transform bulletSpawn; // Transform of point to spawn bullet firing effects
    private ParticleSystem muzzleFlash; // Muzzle flash particle system to play when gun fires
    private LineRenderer bulletTrail; // Bullet trail line renderer
    private ParticleSystem cartridgeEject; // Cartridge eject particle system

    private Camera fpsCamera;

    // NOTE: Should I declare GameObject in FireWeapon() method or continue to use class property?
    private GameObject bulletHoleClone; // Reference to bulletHole from object pool.

    private bool moveTarget = false; // Bool to check if target hit by raycast has rigidbody in update() and then change rigidbody in fixedupdate()
    private RaycastHit targetRaycastHit; // Reference to target of raycast hit with a rigidbody to be moved in FixedUpdate()

    private Animator gunAnimator; // Weapon animator
    private AudioSource gunAudioSource; // Weapon audioSource

    void Awake()
    {
        Transform gunModel = transform.Find("GunModel");
        gunAnimator = gunModel.GetComponent<Animator>();
        gunAudioSource = gunModel.GetComponent<AudioSource>();

        ammoInClip = clipSize; // Initialize ammoInClip to a full clip

        bulletSpawn = transform.Find("BulletEffects");
        muzzleFlash = bulletSpawn.Find("MuzzleFlash").gameObject.GetComponent<ParticleSystem>();
        bulletTrail = bulletSpawn.Find("BulletLineRenderer").GetComponent<LineRenderer>();
        cartridgeEject = bulletSpawn.Find("CartridgeEject").GetComponent<ParticleSystem>();

        fpsCamera = Camera.main;
    }

    void OnEnable()
    {
        // Set ammoCountText
        CanvasUI.sharedInstance.UpdateAmmoCount(ammoInClip, clipSize);
    }

    // Update is called once per frame
    void Update()
    {
        // If firingMode is FullAuto
        if (firingMode == FiringMode.FullAuto)
        {
            // If Fire1 button is held down AND current time is larger than nextShotTime
            if (Input.GetButton("Fire1") && Time.time > nextShotTime)
            {
                FireWeapon();
            }
        }

        // If firingMode is SemiAuto
        if (firingMode == FiringMode.SemiAuto)
        {
            // If Fire1 button is pressed AND current time is larger than nextShotTime
            if (Input.GetButtonDown("Fire1") && Time.time > nextShotTime)
            {
                FireWeapon();
            }
        }

        // If Fire2 button is held down AND NOT already aiming, aim down sights
        if (Input.GetButton("Fire2") && !isAiming)
        {
            isAiming = true;
            StartCoroutine("AimDownSights");
        }

        // If "R" button is pressed, reload weapon
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine("ReloadWeapon");
        }
    }

    void FireWeapon()
    {
        // If canShoot is false, return
        if (!canShoot)
            return;

        // Set nextShotTime of current shot before next shot can be fired by adding seconds to fire by converting RPM to seconds per round
        nextShotTime = Time.time + 60f / fireRate;

        // If there is no ammo in the clip
        if (ammoInClip <= 0)
        {
            // Play empty chamber click audio clip
            gunAudioSource.PlayOneShot(emptyChamberAudioClip);
            return;
        }

        // There is ammo in the clip
        // Decrement ammoInClip
        ammoInClip--;

        // Update ammoCountText
        CanvasUI.sharedInstance.UpdateAmmoCount(ammoInClip, clipSize);

        // Play gun barrel fire effects
        muzzleFlash.Play();

        // Play fire weapon sound effect
        gunAudioSource.PlayOneShot(fireWeaponAudioClip);

        // Play cartridge eject effect
        cartridgeEject.Play();

        // Set Fire animation parameter on animator to true
        gunAnimator.SetTrigger("Fire");

        Vector2 screenCenterPoint = new Vector2(Screen.width / 2, Screen.height / 2);
        Ray ray = fpsCamera.ScreenPointToRay(screenCenterPoint);
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
            // Vector3 bulletHolePosition = hit.point + hit.normal * .01f;

            // Get bulletHoleRotation by randomly spinning bulletHole and orient in line with hit point normal
            // Quaternion randRot = Quaternion.Euler(Vector3.forward * Random.Range(0, 360));
            // Quaternion bulletHoleRotation = Quaternion.FromToRotation(-Vector3.forward, hit.normal) * randRot;

            // Set bulletHolePosition a short distance above the hit point
            bulletHoleClone.transform.position = hit.point + hit.normal * .01f;
            // Set bulletHoleRotation by randomly spinning bulletHole and orient in line with hit point normal
            bulletHoleClone.transform.rotation = Quaternion.FromToRotation(-Vector3.forward, hit.normal)
                * Quaternion.Euler(Vector3.forward * Random.Range(0, 360));

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
                // Normalized vector between hit point and bullet spawn multiplied by bulletForce factor
                Vector3 forceVec = (hit.point - bulletSpawn.transform.position).normalized * bulletForce;

                // Call Damage() method on Enemy instance with forceVector and hit point arguments
                hit.transform.GetComponent<Enemy>().Damage(gunDamage, forceVec, hit.point);
            }
        }
    }

    IEnumerator AimDownSights()
    {
        gunAnimator.SetBool("Aim", true);

        while (Input.GetButton("Fire2"))
        {
            if (Input.GetButtonDown("Fire1"))
            {
                FireWeapon();
            }

            yield return null;
        }

        gunAnimator.SetBool("Aim", false);

        isAiming = false;
    }

    IEnumerator ReloadWeapon()
    {
        // Set canShoot to false
        canShoot = false;

        // Play reload animation
        gunAnimator.SetTrigger("Reload");

        // Play reload audioClip
        gunAudioSource.PlayOneShot(weaponReloadAudioClip);

        // Wait until reload audio clip is done playing before adding to ammoInClip
        while (gunAudioSource.isPlaying)
            yield return null;

        // Reset ammoInClip
        ammoInClip = clipSize;

        // Update ammoCountText
        CanvasUI.sharedInstance.UpdateAmmoCount(ammoInClip, clipSize);

        // Set canShoot to true
        canShoot = true;
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
