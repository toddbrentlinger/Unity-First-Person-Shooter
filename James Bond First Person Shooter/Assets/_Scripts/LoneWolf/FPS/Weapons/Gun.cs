using System.Collections;
using UnityEngine;

namespace LoneWolf.FPS
{
    public class Gun : MonoBehaviour
    {
        // Enumeration for current gun state
        public enum GunState { Idle, Reload, Hipfire, AimDownSights, WalkBob, SprintBob }
        private GunState m_currentGunState = GunState.Idle;

        // Enumeration to choose weapon firing mode
        private enum FiringMode {  SemiAuto, Burst, FullAuto };

        // Gun specifications
        [Header("Gun Spefications")]
        [SerializeField] private FiringMode m_firingMode = FiringMode.SemiAuto; // Default firing mode

        [SerializeField]
        [Tooltip("Number of bullets in single clip")]
        private int m_clipSize = 8; // Number of bullets in single clip before having to reload

        [SerializeField]
        [Tooltip("Number of available bullets for reload")]
        private int m_ammoAvailable = 24; // Number of available bullets for reload

        [SerializeField]
        [Tooltip("Rate of fire in rounds per minute (RPM)")]
        private float m_fireRate = 350f; // The rate of fire in rounds per minute (RPM)

        [SerializeField] private float m_reloadDurationFactor = 1.0f; // multiplied to length of reload animation to adjust duration of animation
        [SerializeField] private float m_weaponRange = 50f; // Length of ray cast to test against colliders
        [SerializeField] private int m_gunDamage = 1; // How much damage is applied to target health. Interacts with Health script on target
        [SerializeField] private float m_bulletForce = 2.0f; // Force of bullet on rigidbodies
        [SerializeField] private float m_bulletScatter = 0; // Value of zero is NO scatter, perfect accuracy
        [SerializeField] private float m_bulletSpeed = 400f; // Speed of bullet (m/s)

        // Gun effects
        [Header("Gun Effects")]
        [SerializeField] private Transform m_bulletSpawn; // Transform of point to spawn bullet firing effects
        [SerializeField] private ParticleSystem m_muzzleFlash; // Muzzle flash particle system to play when gun fires
        [SerializeField] private LineRenderer m_bulletTrail; // Bullet trail line renderer
        [SerializeField] private ParticleSystem m_cartridgeEject; // Cartridge eject particle system

        // Animation
        [Header("Animation")]
        [SerializeField] private Animator m_gunAnimator;

        // Audio
        [Header("Audio")]
        [SerializeField] private AudioClip m_fireWeaponAudioClip; // AudioClip of weapon firing
        [SerializeField] private AudioClip m_emptyChamberAudioClip; // AudoClip of empty chamber click
        [SerializeField] private AudioClip m_weaponReloadAudioClip; // AudioClip of weapon reloading
        [SerializeField] private AudioSource m_gunAudioSource; // Weapon audioSource

        // Camera
        private Camera m_fpsCamera;

        // General variables
        private bool m_canShoot = true; // Bool to see if weapon can be shot (Ex. should NOT fire when reloading or game is paused), initialized to true so weapon will fire
        private int m_ammoInClip; // Reference to current amount of ammo in clip
        private float m_nextShotTime = 0f; // Global time full-auto weapon can fire again between shots while button is held down

        // Bullet hole
        private BulletHoleController m_bulletHoleController = new BulletHoleController();
        private GameObject m_bulletHoleClone; // Reference to bulletHole from object pool.

        // Physics
        private bool m_moveTargetRigidbody = false; // Bool to check if target hit by raycast has rigidbody in update() and then change rigidbody in fixedupdate()
        private RaycastHit m_targetRaycastHit; // Reference to target of raycast hit with a rigidbody to be moved in FixedUpdate()

        // Layer mask for raycast
        private int m_layerMask;

        private void Awake()
        {
            // Initialize ammo in clip to a full clip
            m_ammoInClip = m_clipSize;

            // Set reference to FPS camera
            m_fpsCamera = Camera.main;

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
            CanvasUI.sharedInstance.UpdateAmmoCount(m_ammoInClip, m_ammoAvailable);

            // Reset variables and states in case weapon was changed in middle of reload
            m_canShoot = true;
            m_currentGunState = GunState.Idle;
        }

        private void Update()
        {
            // If Fire1 button is held down AND current time is larger than nextShotTime
            if (Input.GetButton("Fire1") && Time.time > m_nextShotTime)
                FireWeapon();

            // If "R" button or MiddleMouseButton is pressed, reload weapon
            if (Input.GetKeyDown(KeyCode.R) || Input.GetMouseButtonDown(2))
            {
                // Check if there is ammo available to be reloaded
                if (m_currentGunState != GunState.Reload && (m_ammoAvailable > 0))
                    StartCoroutine(ReloadWeapon());
            }
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

            // Set gun state to hipfire
            m_currentGunState = GunState.Hipfire;

            // Set nextShotTime of current shot before next shot can be fired by adding seconds to fire by converting RPM to seconds per round
            m_nextShotTime = Time.time + 60f / m_fireRate;

            // If there is NO ammo in the clip
            if (m_ammoInClip <= 0)
            {
                // Play empty chamber click audio clip and return
                m_gunAudioSource.PlayOneShot(m_emptyChamberAudioClip);
                return;
            }

            // Since there is ammo in the clip, decrement ammoInClip
            m_ammoInClip--;

            // Update ammoCountText
            CanvasUI.sharedInstance.UpdateAmmoCount(m_ammoInClip, m_ammoAvailable);

            // Play gun barrel fire effects
            m_muzzleFlash.Play();

            // Play fire weapon sound effect
            m_gunAudioSource.PlayOneShot(m_fireWeaponAudioClip);

            // Play cartridge eject effect
            m_cartridgeEject.Play();

            // Set Fire animation parameter on animator to true
            m_gunAnimator.SetTrigger("Fire");

            StartCoroutine(FireBullet());

            // Set gun state
            m_currentGunState = GunState.Idle;
        }

        private IEnumerator FireBullet()
        {
            Vector2 screenCenterPoint = new Vector2(Screen.width / 2, Screen.height / 2);

            // Apply bulletScatter
            if (m_bulletScatter != 0)
                screenCenterPoint += Random.insideUnitCircle * m_bulletScatter;

            // Cast ray
            Ray ray = m_fpsCamera.ScreenPointToRay(screenCenterPoint);
            RaycastHit hit;
            Physics.Raycast(ray, out hit, m_weaponRange, m_layerMask, QueryTriggerInteraction.Ignore);

            // Set target point
            Vector3 targetPoint;
            if (hit.collider != null)
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = m_fpsCamera.ViewportToWorldPoint(new Vector3(.5f, .5f, m_weaponRange));
            }

            // ---------- Bullet Trail ----------

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

            // ---------- Bullet Hole ----------

            SetBulletHole(hit);

            yield return null;
        }

        // ---------- Reload ----------

        private IEnumerator ReloadWeapon()
        {
            m_canShoot = false;
            m_currentGunState = GunState.Reload;

            // Play reload animation
            m_gunAnimator.SetTrigger("Reload");

            // Play reload audioClip
            m_gunAudioSource.PlayOneShot(m_weaponReloadAudioClip);

            // Wait until reload audio clip is done playing before adding to ammoInClip
            while (m_gunAudioSource.isPlaying)
                yield return null;

            // Reset ammoInClip
            // There is one or more bullets available
            // If there is less ammo available than fits in the clip, 
            // set ammo in clip to ammo available 
            // and then set ammo available to zero
            if (m_ammoAvailable < m_ammoInClip)
            {
                m_ammoInClip = m_ammoAvailable;
                m_ammoAvailable = 0;
            }
            // Else there is more ammo available than fits in the clip,
            // set ammo in clip to the clip size
            // and reduce ammo available by the clip size
            else
            {
                m_ammoInClip = m_clipSize;
                m_ammoAvailable -= m_clipSize;
            } 

            // Update ammoCountText
            CanvasUI.sharedInstance.UpdateAmmoCount(m_ammoInClip, m_ammoAvailable);

            m_currentGunState = GunState.Idle;
            m_canShoot = true;
        }

        private void SetBulletHole(RaycastHit hit)
        {
            // Return if PhysicsRaycast does NOT hit a collider, bullet does NOT hit anything
            if (hit.collider == null)
                return;

            // Get bulletHoleClone from ObjectPooler
            //m_bulletHoleClone = ObjectPooler.sharedInstance.GetPooledObject("BulletHole");
            //m_bulletHoleClone = m_bulletHoleController.GetBulletHole(hit.collider.material);
            m_bulletHoleClone = m_bulletHoleController.GetBulletHole(hit);

            if (m_bulletHoleClone != null)
            {
                // Set bullet hole parent to object that raycastHit hits
                m_bulletHoleClone.transform.SetParent(hit.transform, false);

                // Set bulletHolePosition a short distance above the hit point
                m_bulletHoleClone.transform.position = hit.point + hit.normal * .001f;

                // Set bulletHoleRotation by randomly spinning bulletHole and orient in line with hit point normal
                m_bulletHoleClone.transform.rotation = Quaternion.FromToRotation(Vector3.forward, hit.normal)
                    * Quaternion.Euler(Vector3.forward * Random.Range(0, 360));

                // Randomize 2D scale, NOT depth
                float holeScale = Random.Range(.6f, .8f);
                m_bulletHoleClone.transform.localScale = new Vector3(holeScale, holeScale, 1f);

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
    }
}
