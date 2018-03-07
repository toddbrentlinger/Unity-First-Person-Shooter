using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketOld : MonoBehaviour {

    public Transform[] explosionPrefabOptions; // Array of prefab explosion options
    public float fuseTime = 5.0f; // Time until rocket explodes if it has not hit collider
    // public float exhaustRate = 1f; // Rate of exhaust

    private Rigidbody rb; // Rigidbody reference
    private bool explodeRocket = false;
    private Vector3 explosionPosition;
    private Quaternion explosionRotation;
    // private ParticleSystem.EmissionModule exhaustEmission; // Particle system emission reference

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Use this for initialization
    void OnEnable()
    {
        // exhaustEmission = GetComponent<ParticleSystem>().emission;
        // Invoke Explode() method after fuseTime
        Invoke("SetExplode", fuseTime);
    }

    void Update()
    {
        // Set emission rate of exhaust particle system depending on rocket velocity
        // exhaustEmission.rateOverTime = rb.velocity.magnitude * exhaustRate;
    }

    void FixedUpdate()
    {
        if (explodeRocket)
        {
            Instantiate(GetRandomExplosion(), explosionPosition, explosionRotation);
            rb.ResetInertiaTensor();
            gameObject.SetActive(false);

            explodeRocket = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // If collision gameObject does NOT have "Player" tag
        if (collision.gameObject.tag != "Player")
        {
            // Set explosion at contact point
            ContactPoint contact = collision.contacts[0];
            explosionRotation = Quaternion.FromToRotation(Vector3.up, contact.normal);
            explosionPosition = contact.point;
            Instantiate(GetRandomExplosion(), explosionPosition, explosionRotation);

            // Destroy(gameObject);
            rb.ResetInertiaTensor();
            gameObject.SetActive(false);
        }
    }

    Transform GetRandomExplosion()
    {
        return explosionPrefabOptions[Random.Range(0, explosionPrefabOptions.Length)];
    }

    void SetExplode()
    {
        explodeRocket = true;

        Instantiate(GetRandomExplosion(), transform.position, transform.rotation);
        // Destroy(gameObject);
        rb.ResetInertiaTensor();
        gameObject.SetActive(false);
    }

    void Explode(Vector3 position, Quaternion rotation)
    {
        Instantiate(GetRandomExplosion(), position, rotation);
        rb.ResetInertiaTensor();
        gameObject.SetActive(false);
    }
}
