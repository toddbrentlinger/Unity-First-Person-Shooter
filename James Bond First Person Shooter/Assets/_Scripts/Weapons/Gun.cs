using System.Collections;
using UnityEngine;

/* NOTES:
* X Add ammo limit before have to reload by pressing "R"
* - Add ability to aim down sights by holding, or toggling, Right-Mouse. Animate gun into position closer to camera with sights aligned to middle of screen. Move parent instead to use same firing animation?
* - If target of Raycast is enemy, get access to enemy script of target do change its behavior
* - Use Coroutine instead of Update. Check performance difference.
* - Eject physical objects or sprites of empty cartridge that disappear after a while?
* X Add firingMode as enum with choices "Semi-Auto", "Burst", "Full-Auto" to act as a dropdown of choices for the property
* - Put ammoCountText behavior in GameController and access GameController script from this class
* - Weapons have damage dropoff at a distance, and will disappear entirely if the shot travels too far
* - Put FireWeapon as coroutine when calculating damage dropoff, speed of bullet (not instantaneous), and bullet drop from gravity
* - ISSUE: When change weapon while reloading with bullets left in clip, gun won't fire when changing back until reload again even though it still has ammo in clip. Perhaps set ammo to zero when starting reload to signify current ammo clip being removed.
* - Implement reload time (change reload animation length) : animation["animName"].speed = animation["animName"].length/desiredLength;
* - Instead of adding impulse force at single rigidbody of enemy, perhaps use spherecast to affect other connected rigidbodies with force inversely proportional to distance from impact (refer to explosion script)
* - When aiming down sights, increase accuracy by reducing bulletScatter if any
* - Melee with weapon. Create animation and damage if enemy is within melee range
* - Perhaps have a WeaponController script that references camera, layermask, fpsController moveState and then pass info to active gun class functions.
* - Change accuracy/weapon spread with fpsController speed. Running decreases accuracy and crouching increases accuracy. Aiming down sights also increases accuracy.
* - Change WeaponBob to script rather than animation. Weapon moves direction to the same side as Player's initial movement from Idle. Adjust time of animation through script and horizontal bob limits.
* Can I adjust horizontal limits of animation through script? Would it be easier to have the curve in the editor under each weapon bob properties.
* - Use script to move weapon to aim down sights. Fire animation will play the same since script moves parent of weapon model. 
* - BulletLineRenderer: set first position in world space location so bullet line will stay in line wherever bullet originally fired from
*/

public enum GunState { Idle, HipFire, Reload, MoveBob };

public class Gun : MonoBehaviour {

    //[SerializeField] private GunState m_gunState = GunState.Idle;

    // Enumeration of FiringMode to choose weapon firing mode
    private enum FiringMode { SemiAuto, Burst, FullAuto };
    [SerializeField] private FiringMode m_firingMode = FiringMode.SemiAuto;

    [SerializeField] private int m_clipSize = 8; // Number of bullets in single clip before having to reload
    [SerializeField] [Tooltip("Rate of fire in rounds per minute (RPM)")]
    private float m_fireRate = 350f; // The rate of fire in rounds per minute (RPM)
    //[SerializeField] private float m_reloadDurationFactor = 1.0f; // multiplied to length of reload animation to adjust duration of animation
    [SerializeField] private float m_weaponRange = 50f; // Length of ray cast to test against colliders
    [SerializeField] private int m_gunDamage = 1; // How much damage is applied to target health. Interacts with Health script on target
    [SerializeField] private float m_bulletForce = 2.0f; // Force of bullet on rigidbodies
    [SerializeField] private float m_bulletScatter = 0; // Value of zero is NO scatter, perfect accuracy
    [SerializeField] private float m_bulletSpeed = 400f; // Speed of bullet (m/s)

    [Header("Audio")]
    [SerializeField] private AudioClip m_fireWeaponAudioClip; // AudioClip of weapon firing
    [SerializeField] private AudioClip m_emptyChamberAudioClip; // AudoClip of empty chamber click
    [SerializeField] private AudioClip m_weaponReloadAudioClip; // AudioClip of weapon reloading

    private bool m_canShoot = true; // Bool to see if weapon can be shot (Ex. should NOT fire when reloading or game is paused), initialized to true so weapon will fire
    private int m_ammoInClip; // Reference to current amount of ammo in clip
    private float m_nextShotTime = 0f; // Global time full-auto weapon can fire again between shots while button is held down

    //private bool m_isAiming = false;
    private bool m_isReloading = false;

    private Transform m_bulletSpawn; // Transform of point to spawn bullet firing effects
    private ParticleSystem m_muzzleFlash; // Muzzle flash particle system to play when gun fires
    private LineRenderer m_bulletTrail; // Bullet trail line renderer
    private ParticleSystem m_cartridgeEject; // Cartridge eject particle system

    //private FPSController m_fpsController;
    private Camera m_fpsCamera;

    // NOTE: Should I declare GameObject in FireWeapon() method or continue to use class property?
    private GameObject m_bulletHoleClone; // Reference to bulletHole from object pool.

    private bool m_moveTargetRigidbody = false; // Bool to check if target hit by raycast has rigidbody in update() and then change rigidbody in fixedupdate()
    private RaycastHit m_targetRaycastHit; // Reference to target of raycast hit with a rigidbody to be moved in FixedUpdate()

    private Animator m_gunAnimator; // Weapon animator
    private AudioSource m_gunAudioSource; // Weapon audioSource

    // Layer mask for raycast
    private int m_layerMask;

    // FPS Controller to reference MoveState for weaponBob
    //private FPSController m_fpsController;

    // BulletHoleController
    private BulletHoleController m_bulletHoleController = new BulletHoleController();

    private void Awake()
    {
        Transform gunModel = transform.Find("GunModel");
        m_gunAnimator = gunModel.GetComponent<Animator>();
        if (m_gunAnimator == null)
            m_gunAnimator = GetComponent<Animator>();
        m_gunAudioSource = gunModel.GetComponent<AudioSource>();

        m_ammoInClip = m_clipSize; // Initialize ammoInClip to a full clip

        m_bulletSpawn = gunModel.Find("BulletEffects");
        m_muzzleFlash = m_bulletSpawn.Find("MuzzleFlash").gameObject.GetComponent<ParticleSystem>();
        m_bulletTrail = transform.Find("BulletLineRenderer").GetComponent<LineRenderer>();
        m_cartridgeEject = m_bulletSpawn.Find("CartridgeEject").GetComponent<ParticleSystem>();

        //m_fpsController = GetComponentInParent<FPSController>();
        m_fpsCamera = Camera.main;

        // FPS Controller to reference MoveState for weaponBob
        //m_fpsController = GameObject.FindGameObjectWithTag("Player").GetComponent<FPSController>();

        // Bit shift the index of the layer (8) to get a bit mask
        m_layerMask = 1 << 8;
        // This would cast rays only against colliders in layer 8(Player).
        // But instead we want to collide against everything except layer 8(Player). 
        // The ~ operator does this, it inverts a bitmask.
        m_layerMask = ~m_layerMask;
        // NOTE: Could use IgnoreRaycast layer instead? Cannot since it's using Player layer instead
    }

    private void OnEnable()
    {
        // Set ammoCountText
        CanvasUI.sharedInstance.UpdateAmmoCount(m_ammoInClip, m_clipSize);

        // Reset bools in case weapon was changed in middle of reload
        m_canShoot = true;
        m_isReloading = false;
    }

    private void Update()
    {
        switch (m_firingMode)
        {
            case FiringMode.FullAuto:
                // If Fire1 button is held down AND current time is larger than nextShotTime
                if (Input.GetButton("Fire1") && Time.time > m_nextShotTime)
                    FireWeapon();
                break;

            case FiringMode.SemiAuto:
                // If Fire1 button is held down AND current time is larger than nextShotTime
                if (Input.GetButtonDown("Fire1") && Time.time > m_nextShotTime)
                    FireWeapon();
                break;
        }

        // If "R" button or MiddleMouseButton is pressed, reload weapon
        if (Input.GetKeyDown(KeyCode.R) || Input.GetMouseButtonDown(2))
        {
            if (!m_isReloading)
                StartCoroutine(ReloadWeapon());
        }
        /*
        // Weapon Bob depending on fpsController.CurrentMoveState
        if (m_fpsController.CurrentMoveState == MoveState.Walking ||
            m_fpsController.CurrentMoveState == MoveState.Running ||
            m_fpsController.CurrentMoveState == MoveState.Crouching)
        {
            m_gunAnimator.SetBool("WeaponBob", true);
        }
        else
            m_gunAnimator.SetBool("WeaponBob", false);
        */
    }

    private void FixedUpdate()
    {
        // moveTargetRgidbody is true if collider has rigidbody and is NOT kinematic
        if (m_moveTargetRigidbody)
        {
            // Normalized vector between hit point and bullet spawn multiplied by bulletForce factor
            Vector3 forceVec = (m_targetRaycastHit.point - m_bulletSpawn.transform.position).normalized * m_bulletForce;
            // Add impulse force to target rigidbody
            m_targetRaycastHit.rigidbody.AddForceAtPosition(forceVec, m_targetRaycastHit.point, ForceMode.Impulse);
            // Set moveTargetRigidbody bool to false
            m_moveTargetRigidbody = false;
        }
    }

    private void FireWeapon()
    {
        // If canShoot is false, return
        if (!m_canShoot)
            return;

        // Set nextShotTime of current shot before next shot can be fired by adding seconds to fire by converting RPM to seconds per round
        m_nextShotTime = Time.time + 60f / m_fireRate;

        // If there is no ammo in the clip
        if (m_ammoInClip <= 0)
        {
            // Play empty chamber click audio clip
            m_gunAudioSource.PlayOneShot(m_emptyChamberAudioClip);
            return;
        }

        // There is ammo in the clip
        // Decrement ammoInClip
        m_ammoInClip--;

        // Update ammoCountText
        CanvasUI.sharedInstance.UpdateAmmoCount(m_ammoInClip, m_clipSize);

        // Play gun barrel fire effects
        m_muzzleFlash.Play();

        // Play fire weapon sound effect
        m_gunAudioSource.PlayOneShot(m_fireWeaponAudioClip);

        // Play cartridge eject effect
        m_cartridgeEject.Play();

        // Set Fire animation parameter on animator to true
        m_gunAnimator.SetTrigger("Fire");

        StartCoroutine(FireBullet());
    }

    private IEnumerator FireBullet()
    {
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2, Screen.height / 2);
        // Apply bulletScatter
        if (m_bulletScatter != 0)
            screenCenterPoint += Random.insideUnitCircle * m_bulletScatter;
        Ray ray = m_fpsCamera.ScreenPointToRay(screenCenterPoint);
        RaycastHit hit;

        Physics.Raycast(ray, out hit, m_weaponRange, m_layerMask);
        Vector3 targetPoint;
        //float timeToHit;
        //float targetDistance;
        if (hit.collider != null)
        {
            targetPoint = hit.point;
            //targetDistance = hit.distance;
            //timeToHit = Time.time + hit.distance / m_bulletSpeed;
        }
        else
        {
            //targetPoint = m_fpsCamera.transform.position + m_fpsCamera.transform.forward * m_weaponRange;
            targetPoint = m_fpsCamera.ViewportToWorldPoint(new Vector3(.5f, .5f, m_weaponRange));
            //targetDistance = m_weaponRange;
            //timeToHit = Time.time + m_weaponRange / m_bulletSpeed;
        }

        // Set Bullet Position/Rotation(LookAt) from ObjectPool at m_bulletSpawn.position
        // Set Bullet Active
        // Call FireBullet() from Bullet

        // ---------------------------------------------------------------------------- //
        // Bullet Trail
        // ---------------------------------------------------------------------------- //

        //Debug.DrawLine(m_bulletTrail.transform.position, targetPoint, Color.green, .5f);

        // Adjust bulletTrail rotation to point at targetPoint
        m_bulletTrail.transform.LookAt(targetPoint);

        //Vector3 currPoint = m_bulletTrail.transform.position;
        float targetDistance = (targetPoint - m_bulletTrail.transform.position).magnitude;
        float currDistance = 0;
        //while (Time.time < timeToHit)
        while (currDistance < targetDistance)
        {
            currDistance += m_bulletSpeed * Time.deltaTime;
            currDistance = Mathf.Clamp(currDistance, 0, targetDistance);
            m_bulletTrail.SetPosition(1, Vector3.forward * currDistance);

            //currPoint = m_bulletTrail.GetPosition(1) + Vector3.forward * m_bulletSpeed * Time.deltaTime;
            //m_bulletTrail.SetPosition(1, m_bulletTrail.GetPosition(1) + Vector3.forward * m_bulletSpeed * Time.deltaTime);

            yield return null;
        }

        // Reset bulletTrail
        m_bulletTrail.SetPosition(1, Vector3.zero);

        // ---------------------------------------------------------------------------- //
        // Bullet Hole
        // ---------------------------------------------------------------------------- //

        SetBulletHole(hit);
    }

    private void SetBulletHole(RaycastHit hit)
    {
        // Return if PhysicsRaycast does NOT hit a collider, bullet does NOT hit anything
        if (hit.collider == null)
            return;

        // Get bulletHoleClone from ObjectPooler
        //m_bulletHoleClone = ObjectPooler.sharedInstance.GetPooledObject("BulletHole");
        m_bulletHoleClone = m_bulletHoleController.GetBulletHole(hit.collider.material);

        if (m_bulletHoleClone != null)
        {
            // Get bulletHolePosition a short distance above the hit point
            // Vector3 bulletHolePosition = hit.point + hit.normal * .01f;

            // Set bulletHolePosition a short distance above the hit point
            m_bulletHoleClone.transform.position = hit.point + hit.normal * .01f;
            // Set bulletHoleRotation by randomly spinning bulletHole and orient in line with hit point normal
            m_bulletHoleClone.transform.rotation = Quaternion.FromToRotation(Vector3.forward, hit.normal)
                * Quaternion.Euler(Vector3.forward * Random.Range(0, 360));

            // Set bullet hole parent to object that raycasthit hits
            m_bulletHoleClone.transform.SetParent(hit.transform);
            // Set bullet hole active
            m_bulletHoleClone.SetActive(true);

            // If collider gameobject has rigidbody AND is NOT kinematic, add force from bullet impact
            if (hit.rigidbody != null && !hit.rigidbody.isKinematic)
            {
                m_targetRaycastHit = hit;
                m_moveTargetRigidbody = true;
            }

            // IDamageable interface
            IDamageable m_damageInterface = hit.transform.GetComponentInParent<IDamageable>();
            if (m_damageInterface != null)
            {
                // Normalized vector between hit point and bullet spawn multiplied by bulletForce factor
                Vector3 hitForce = (hit.point - m_bulletSpawn.transform.position).normalized * m_bulletForce;

                // Call TakeDamage from IDamageable interface
                m_damageInterface.TakeDamage(hit.point, hitForce, m_gunDamage, hit.rigidbody);
            }
            /*
            // If collider is an Enemy AND isKinematic
            if (hit.transform.tag == "Enemy" && hit.rigidbody.isKinematic)
            {
                // Normalized vector between hit point and bullet spawn multiplied by bulletForce factor
                Vector3 forceVec = (hit.point - m_bulletSpawn.transform.position).normalized * m_bulletForce;

                // Call Damage() method on Enemy instance with forceVector and collider hit arguments
                hit.transform.GetComponentInParent<Enemy>().Damage(m_gunDamage, forceVec, hit);
            }
            */
        }
    }

    private IEnumerator ReloadWeapon()
    {
        // Set canShoot to false and isReloading to true
        m_canShoot = false;
        m_isReloading = true;

        // Play reload animation
        m_gunAnimator.SetTrigger("Reload");

        // Play reload audioClip
        m_gunAudioSource.PlayOneShot(m_weaponReloadAudioClip);

        // Wait until reload audio clip is done playing before adding to ammoInClip
        while (m_gunAudioSource.isPlaying)
            yield return null;

        // Reset ammoInClip
        m_ammoInClip = m_clipSize;

        // Update ammoCountText
        CanvasUI.sharedInstance.UpdateAmmoCount(m_ammoInClip, m_clipSize);

        // Set canShoot to true and isReloading to false
        m_isReloading = false;
        m_canShoot = true;
    }
}
