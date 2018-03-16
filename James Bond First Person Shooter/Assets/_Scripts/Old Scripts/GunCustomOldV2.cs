using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunCustomOldV2 : MonoBehaviour {

    /* NOTES:
     * - Add ammo limit before have to reload by pressing "R"
     * - Add ability to aim down sights by holding, or toggling, Right-Mouse. Animate gun into position closer to camera with sights aligned to middle of screen.
     * - If target of Raycast is enemy, get access to enemy script of target do change its behavior
     */

    public int gunDamage = 1; // How much damage is applied to target health. Interacts with Health script on target
    public float bulletForce = 5.0f;
    // Time between rounds fired when button is held down
    public float cooldown = 0.1f;

    private Transform bulletSpawn;
    private ParticleSystem muzzleFlash;
    private GameObject bulletHoleClone;
    private LineRenderer bulletLineRenderer;

    // Bool to check if raycast hit target has rigidbody in update() and then change rigidbody in fixedupdate()
    private bool moveTarget = false;
    // Reference to raycast hit of target object with rigidbody
    private RaycastHit targetRaycastHit;

    // Animation
    private Animator animator; // Weapon animator
    private AudioSource audioSource; // Weapon audioSource

    // Reference to Enemy instance if target has enemy tag. Can I somehow use a static variable for Enemy class instead of using GetComponent<Enemy>() on each enemy hit?
    private Enemy targetEnemy;

    // Physics.Raycast() - Is this necessary or can I just create new RaycastHit and Ray each time weapon fires
    // private RaycastHit hit;
    // private Ray ray;

    // Recoil animation (simple)
    float lastShotTime;

    void Awake()
    {
        bulletSpawn = transform.Find("BulletSpawn");
        // muzzleFlash = bulletSpawn.GetComponent<ParticleSystem>();
        muzzleFlash = bulletSpawn.Find("MuzzleFlash").gameObject.GetComponent<ParticleSystem>();

        bulletLineRenderer = bulletSpawn.transform.Find("BulletLineRenderer").GetComponent<LineRenderer>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Fire1") && Time.time > lastShotTime + cooldown)
        {
            // Play gun barrel fire effects
            muzzleFlash.Play();

            // Play sound effect
            audioSource.Play();

            // Set Fire animation parameter on animator to true
            animator.SetTrigger("Fire");

            Vector2 screenCenterPoint = new Vector2(Screen.width / 2, Screen.height / 2);

            Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            RaycastHit hit;

            // Get bulletHoleClone from ObjectPooler
            bulletHoleClone = ObjectPooler.sharedInstance.GetPooledObject("BulletHole");

            if (bulletHoleClone != null && Physics.Raycast(ray, out hit, Camera.main.farClipPlane))
            {
                // Add bulletHole at hit point of raycast
                Vector3 bulletHolePosition = hit.point + hit.normal * .01f;

                // Randomly spin bulletHole and orient in line with hit point normal
                // Quaternion bulletHoleRotation = Quaternion.FromToRotation(-Vector3.forward, hit.normal);
                Quaternion randRot = Quaternion.Euler(Vector3.forward * Random.Range(0, 360));
                Quaternion bulletHoleRotation = Quaternion.FromToRotation(-Vector3.forward, hit.normal) * randRot;

                // Instantiate(bulletHole, bulletHolePosition, bulletHoleRotation, hit.transform);

                // Set bullet hole position
                bulletHoleClone.transform.position = bulletHolePosition;
                bulletHoleClone.transform.rotation = bulletHoleRotation;
                // Set bullet hole parent to object that raycasthit hits
                bulletHoleClone.transform.SetParent(hit.transform);
                // Set bullet hole active
                bulletHoleClone.SetActive(true);

                // If collider gameobject has rigidbody and NOT kinematic, add force from bullet impact
                if (hit.rigidbody != null && !hit.rigidbody.isKinematic)
                {
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

            // Set time of last shot to be added to cooldown timer and compared when button is held down
            lastShotTime = Time.time;
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

    float RandomCoordinate()
    {
        return Random.Range(-.02f, .02f);
    }
}
