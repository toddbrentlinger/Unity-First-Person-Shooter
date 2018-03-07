using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketOldV3 : MonoBehaviour {

    public float rocketSpeed = 10f; // Initial velocity of rocket
    public float fuseTime = 3.5f; // Time until rocket explodes if it has not hit collider
    public float blastRadius = 4f;
    public float blastForce = 130f;

    private GameObject rocketModel;
    private Rigidbody rb;
    private ParticleSystem exhaust;
    // private ParticleSystem.MainModule exhaustMain;
    private ParticleSystem explosion;

    private float startTime;
    private bool rocketFired = false;
    private bool rocketCollided = false;

    void Awake()
    {
        rocketModel = transform.Find("RocketModel").gameObject;
        rb = GetComponent<Rigidbody>();
        exhaust = transform.Find("Exhaust").GetComponent<ParticleSystem>();
        // exhaustMain = exhaust.main;
        explosion = transform.Find("Explosion").GetComponent<ParticleSystem>();
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // If rocket has been fired
        if (rocketFired && !rocketCollided)
        {
            if (Time.time > startTime + fuseTime)
            {
                // Set up explosion
                StartCoroutine(Explode(transform.position, transform.rotation));
            }
        }
    }

    public void Fire()
    {
        rocketFired = true;

        // Set time that rocket is fired
        startTime = Time.time;

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
            Vector3 contactPosition = contact.point;
            Quaternion contactRotation = Quaternion.FromToRotation(Vector3.up, contact.normal);

            // Set explodeRocket bool
            rocketCollided = true;

            // Set explosion at contact point
            // Explode(contactPosition, contactRotation);
            StartCoroutine(Explode(contactPosition, contactRotation));
        }
    }

    IEnumerator Explode(Vector3 explosionPosition, Quaternion explosionRotation)
    {
        // Reset rocketFired bool to stop checking in Update()
        rocketFired = false;

        exhaust.Stop();

        // Stop rigidbody from moving and detecting collisions
        rb.detectCollisions = false;
        rb.isKinematic = true;

        // Let exhaust particle system current loop end by turning off looping
        // ParticleSystem.MainModule exhaustMain = exhaust.main;
        // exhaustMain.loop = false;
        // exhaust.Stop();

        // Deactivate model
        rocketModel.SetActive(false);

        // Set position of rocket to explosion (NOTE: parent transform is same as explosion child)
        transform.position = explosionPosition;
        transform.rotation = explosionRotation;

        // Play explosion
        explosion.Play();

        // Move rigidbodies in explosion radius
        yield return new WaitForFixedUpdate();
        MoveSurroundingObjects();

        // Wait until end of explosion
        yield return new WaitForSeconds(explosion.main.duration);

        // Change SetActive on rocket to false to return to pool
        gameObject.SetActive(false);
    }

    void MoveSurroundingObjects()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, blastRadius);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if (rb != null)
                rb.AddExplosionForce(blastForce, transform.position, blastRadius, 3.0f);
        }
    }

    void OnDisable()
    {
        // Reset bools

        rocketFired = false;
        rocketCollided = false;

        // Reset rocket

        // If rocketModel is NOT active in heirarchy, activate rocketModel
        if (!rocketModel.activeInHierarchy) rocketModel.SetActive(true);

        // Reset exhaust particle system to loop
        ParticleSystem.MainModule exhaustMain = exhaust.main;
        if (!exhaustMain.loop) exhaustMain.loop = true;

        // Reset rigidbody
        if (rb.isKinematic)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }
    }
}
