using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketOldV2 : MonoBehaviour {

    public float rocketSpeed = 10f;  // Initial velocity of rocket
    public float fuseTime = 5.0f; // Time until rocket explodes if it has not hit collider
    // public float exhaustRate = 1f; // Rate of exhaust

    private GameObject rocketModel;
    private Rigidbody rb;
    private ParticleSystem exhaust;
    private ParticleSystem.MainModule exhaustMain;
    private ParticleSystem explosion;

    private float startTime;
    private bool fireRocket = false;
    private bool rocketCollide = false;
    private bool explodeRocket = false;
    private Vector3 explosionPosition;
    private Quaternion explosionRotation;
    // private ParticleSystem.EmissionModule exhaustEmission; // Particle system emission reference

    void Awake()
    {
        Debug.Log("Rocket Awake");
        rocketModel = transform.Find("RocketModel").gameObject;
        rb = GetComponent<Rigidbody>();
        exhaust = transform.Find("Exhaust").GetComponent<ParticleSystem>();
        exhaustMain = exhaust.main;
        explosion = transform.Find("Explosion").GetComponent<ParticleSystem>();
        // exhaustEmission = GetComponent<ParticleSystem>().emission;
    }

    // Use this for initialization when object is enabled
    void OnEnable()
    {
        Debug.Log("Rocket OnEnable");
    }

    void Start()
    {
        Debug.Log("Rocket Start");
    }

    // Fire rocket
    public void Fire()
    {
        // Set fireRocket bool to change rigidbodies in FixedUpdate()
        fireRocket = true;
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

            explodeRocket = true;

            SetUpExplosion(explosionPosition, explosionRotation);
        }
    }

    void Update()
    {
        // Set emission rate of exhaust particle system depending on rocket velocity
        // exhaustEmission.rateOverTime = rb.velocity.magnitude * exhaustRate;
    }

    // Physics updates
    void FixedUpdate()
    {
        // Fire rocket
        if (fireRocket)
        {
            // Set time that rocket is fired
            startTime = Time.time;

            // Set velocity of rigidbody
            rb.velocity = transform.forward * rocketSpeed;

            // Start exhaust particle system
            exhaust.Play();

            // Change fireRocket bool to false since rocket has fired
            fireRocket = false;
        }

        if (explodeRocket) { }

        // If explodeRocket is true OR time since launch is greater than fuse time
        if (rocketCollide || Time.time > startTime + fuseTime)
        {
            if (!rocketCollide)
            {
                explosionPosition = transform.position;
                explosionRotation = transform.rotation;
            }

            // Explode();
            StartCoroutine("Explode");

            rocketCollide = false;
        }
    }

    void SetUpExplosion(Vector3 position, Quaternion rotation)
    {
        // Stop rigidbody from moving and detecting collisions
        rb.isKinematic = true;
        rb.detectCollisions = false;

        // Let exhaust particle system current loop end by turning off looping
        ParticleSystem.MainModule exhaustMain = exhaust.main;
        exhaustMain.loop = false;

        // Deactivate model
        rocketModel.SetActive(false);

        // Set position of rocket to explosion (NOTE: parent transform is same as explosion child)
        transform.position = explosionPosition;
        transform.rotation = explosionRotation;

        // Play explosion
        explosion.Play();

        // Move rigidbodies in explosion radius
        MoveObjects();

        // Wait until end of explosion
        // yield return new WaitForSeconds(explosion.main.duration);

        // Change SetActive on rocket to false to return to pool
        gameObject.SetActive(false);
    }

    IEnumerator Explode()
    {
        // Stop rigidbody from moving and detecting collisions
        rb.isKinematic = true;
        rb.detectCollisions = false;

        // Let exhaust particle system current loop end by turning off looping
        ParticleSystem.MainModule exhaustMain = exhaust.main;
        exhaustMain.loop = false;

        // Deactivate model
        rocketModel.SetActive(false);

        // Set position of rocket to explosion (NOTE: parent transform is same as explosion child)
        transform.position = explosionPosition;
        transform.rotation = explosionRotation;

        // Play explosion
        explosion.Play();

        // Move rigidbodies in explosion radius
        MoveObjects();

        // Wait until end of explosion
        yield return new WaitForSeconds(explosion.main.duration);

        // Change SetActive on rocket to false to return to pool
        gameObject.SetActive(false);
    }

    void MoveObjects()
    {

    }

    // Run when rocket is disabled (SetActive set to false) and returned to object pool
    void OnDisable()
    {
        Debug.Log("Rocket Disable");
        // Reset rocket

        // If rocketModel is NOT active in heirarchy, activate rocketModel
        if (!rocketModel.activeInHierarchy) rocketModel.SetActive(true);

        // Reset exhaust particle system to loop
        if (!exhaustMain.loop) exhaustMain.loop = true;

        // Reset rigidbody
        if (rb.isKinematic)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }
    }
}
