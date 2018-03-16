using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunCustomOld : MonoBehaviour {

    public float bulletForce = 5.0f;
    public GameObject bulletHole;
    public Transform bulletSpawn;
    public Transform muzzleFlash;
    public Transform impactDust;

    // Bool to check if raycast hit target has rigidbody in update() and then change rigidbody in fixedupdate()
    private bool hitTarget = false;
    // Reference to raycast hit of target object with rigidbody
    private RaycastHit targetRaycastHit;
    // Time between rounds fired when button is held down
    const float cooldown = 0.1f;

    // Weapon animator
    private Animator animator;

    // Reference to rigidbody of collider hit in raycast
    Rigidbody targetColliderRigidbody;

    // Physics.Raycast()
    RaycastHit hit;
    Ray ray;

    // Recoil animation (simple)
    float lastShotTime;
    Vector3 initialLocalPosition;

    void Awake()
    {
        animator = GetComponent<Animator>();
        initialLocalPosition = transform.localPosition;
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
            Instantiate(muzzleFlash, bulletSpawn.transform.position, bulletSpawn.transform.rotation, bulletSpawn);

            // Set Fire animation parameter on animator to true
            animator.SetTrigger("Fire");
            // animator.Play("Fire");

            Vector2 screenCenterPoint = new Vector2(Screen.width / 2, Screen.height / 2);

            ray = Camera.main.ScreenPointToRay(screenCenterPoint);

            if (Physics.Raycast(ray, out hit, Camera.main.farClipPlane) && bulletHole != null)
            {
                // Add bulletHole at hit point of raycast
                Vector3 bulletHolePosition = hit.point + hit.normal * .01f;

                // Randomly spin bulletHole and orient in line with hit point normal
                // Quaternion bulletHoleRotation = Quaternion.FromToRotation(-Vector3.forward, hit.normal);
                Quaternion randRot = Quaternion.Euler(Vector3.forward * Random.Range(0, 360));
                Quaternion bulletHoleRotation = Quaternion.FromToRotation(-Vector3.forward, hit.normal) * randRot;

                Instantiate(bulletHole, bulletHolePosition, bulletHoleRotation, hit.transform);

                // Add dust impact prefab at bullet hole position
                bulletHoleRotation = Quaternion.FromToRotation(Vector3.forward, hit.normal);

                Instantiate(impactDust, bulletHolePosition, bulletHoleRotation, hit.transform);

                // If collider gameobject has rigidbody, add force from bullet impact
                if (hit.rigidbody != null)
                {
                    targetRaycastHit = hit;
                    hitTarget = true;
                }
            }

            // Set time of last shot to be added to cooldown timer and compared when button is held down
            lastShotTime = Time.time;

            // Set recoil gun position
            transform.localPosition = initialLocalPosition + new Vector3(RandomCoordinate(), RandomCoordinate(), RandomCoordinate());
        }
        else if (transform.localPosition != initialLocalPosition)
        {
            // Reset gun position after recoil
            transform.localPosition = initialLocalPosition;
        }
    }

    void FixedUpdate()
    {
        if (hitTarget)
        {
            // Reference to raycast hit rigidbody
            targetColliderRigidbody = targetRaycastHit.rigidbody;
            // Normalized vector between hit point and bullet spawn multiplied by bulletForce factor
            Vector3 forceVec = (targetRaycastHit.point - bulletSpawn.transform.position).normalized * bulletForce;
            // Add impulse force to target rigidbody
            targetColliderRigidbody.AddForce(forceVec, ForceMode.Impulse);
            // Set hitTarget bool to false
            hitTarget = false;
        }
    }

    float RandomCoordinate()
    {
        return Random.Range(-.02f, .02f);
    }
}
