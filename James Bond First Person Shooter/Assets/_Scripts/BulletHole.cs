using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHole : MonoBehaviour {

    private ParticleSystem bulletImpact;

    void Awake()
    {
        bulletImpact = GetComponent<ParticleSystem>();
    }

	void OnEnable()
    {
        bulletImpact.Play();
    }
}
