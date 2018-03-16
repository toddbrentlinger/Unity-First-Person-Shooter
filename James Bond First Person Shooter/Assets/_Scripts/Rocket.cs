using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour {

    public float rocketSpeed = 10f; // Initial velocity of rocket
    public float fuseTime = 3.5f; // Time until rocket explodes if it has not hit collider
    public float blastRadius = 4f;
    public float blastForce = 130f;

    private GameObject rocketModel;
    private Rigidbody rb;
    private ParticleSystem exhaust;
    private ParticleSystem explosion;

    private float endTime;
    private bool rocketFired = false;
    // private bool rocketCollided = false;
    private bool rocketExplode = false;

    private Vector3 explosionPosition;
    private Quaternion explosionRotation;

    void Awake()
    {
        rocketModel = transform.Find("RocketModel").gameObject;
        rb = GetComponent<Rigidbody>();
        exhaust = transform.Find("Exhaust").GetComponent<ParticleSystem>();
        explosion = transform.Find("Explosion").GetComponent<ParticleSystem>();
    }

    // Rocket from ObjectPooler is position first before being SetActive, so
    // OnEnable() is called after rocket is positioned. 
    void OnEnable()
    {
        // Rigidbody transform should only be moved when isKinematic so OnEnable()
        // sets rigidbody back to NOT isKinematic to add forces
        // If rigidbody is kinematic, set to NOT kinematic
        // if (rb.isKinematic) rb.isKinematic = false;
    }

    // Use this for initialization
    void Start()
    {
        // Ignore collisions between rocket and player (NOTE: use short delay after firing and then allow collisions)
        // Physics.IgnoreCollision(GetComponent<Collider>(), GameObject.FindWithTag("Player").GetComponent<Collider>());
    }

    // Update is called once per frame
    void Update()
    {
        // If rocket has been fired and NOT set to explode
        if (rocketFired && !rocketExplode)
        {
            // If fuse time runs out (NOTE: perhaps compare to endTime initialized to Time.time+fuseTime when rocket is fired)
            if (Time.time > endTime)
            {
                explosionPosition = transform.position;
                explosionRotation = transform.rotation;

                // Set rocketExplode bool so FixedUpdate() will move surrounding objects
                // rocketExplode = true;
                // Explode
                Explode();
            }
        }
    }

    // Public function to fire rocket called from outside script
    public void Fire()
    {
        rocketFired = true;

        // Set time that rocket is fired
        endTime = Time.time + fuseTime;

        // Set velocity of rigidbody
        rb.velocity = transform.forward * rocketSpeed;

        // Start exhaust particle system
        exhaust.Play();
    }

    void OnCollisionEnter(Collision collision)
    {
        // If collision gameObject does NOT have "Player" tag
        if (collision.gameObject.tag != "Player")
        {
            // Set explosion at contact point
            ContactPoint contact = collision.contacts[0];
            explosionPosition = contact.point;
            explosionRotation = Quaternion.FromToRotation(Vector3.up, contact.normal);

            // Set explodeRocket bool
            // rocketCollided = true;
            // rocketExplode = true;
            // Explode
            Explode();
        }
    }

    void FixedUpdate()
    {
        // If rocketExplode is set from colliding or fuse time running down
        if (rocketExplode)
        {
            // Stop rigidbody from moving and detecting collisions
            rb.isKinematic = true;
            rb.detectCollisions = false;

            Collider[] colliders = Physics.OverlapSphere(explosionPosition, blastRadius);
            foreach (Collider hit in colliders)
            {
                Rigidbody rb = hit.GetComponent<Rigidbody>();

                if (rb != null)
                    rb.AddExplosionForce(blastForce, explosionPosition, blastRadius, 3.0f);
            }

            // Reset rocketExplode bool
            rocketExplode = false;
        }
    }

    void Explode()
    {
        // Set rocketExplode bool for FixedUpdate() to move surrounding rigidbodies
        rocketExplode = true;

        // Reset rocketFired bool to stop checking in Update()
        rocketFired = false;

        // Stop exhaust. Make sure particles already emitted run out their lives
        exhaust.Stop();

        // Stop rigidbody from moving and detecting collisions
        // rb.detectCollisions = false;
        // rb.isKinematic = true;

        // Let exhaust particle system current loop end by turning off looping
        // ParticleSystem.MainModule exhaustMain = exhaust.main;
        // exhaustMain.loop = false;
        // exhaust.Stop();

        // Deactivate model
        rocketModel.SetActive(false);

        // Set position of rocket to explosion (NOTE: parent transform is same as explosion child)
        explosion.transform.position = explosionPosition;
        explosion.transform.rotation = explosionRotation;

        // Play explosion
        explosion.Play();

        // Wait until end of explosion
        //yield return new WaitForSeconds(explosion.main.duration);

        // Change SetActive on rocket to false to return to pool
        // gameObject.SetActive(false);

        // Destory rocket after explosion loop ends
        Invoke("ResetRocket", explosion.main.duration);
    }

    // Reset rocket properties before returning to ObjectPool
    void ResetRocket()
    {
        // Change SetActive on rocket to false to return to pool and run OnDisable()
        gameObject.SetActive(false);

        // Reset bools
        if (rocketFired) rocketFired = false;
        if (rocketExplode) rocketExplode = false;
        // rocketCollided = false;

        // Reset explosion particle system
        if (explosion.transform.position != transform.position)
        {
            explosion.transform.position = transform.position;
            explosion.transform.position = transform.position;
        }

        // If rocketModel is NOT active in heirarchy, activate rocketModel
        if (!rocketModel.activeInHierarchy) rocketModel.SetActive(true);

        // Reset rigidbody to NOT isKinematic and to detect collisions
        if (rb.isKinematic) rb.isKinematic = false;
        if (!rb.detectCollisions) rb.detectCollisions = true;
    }
}