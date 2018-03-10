using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    /* NOTES:
     * - If bullet hits enemy, they fall back at the same angle the bullet hits from
     * - Explosion affects enemy by killing them and throwing their body in relation to the explosion 
     * - Enemy skin changes color every time it gets hit
     */

    private Rigidbody enemyRigidbody;

	// Use this for initialization
	void Start () {
        enemyRigidbody = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Hit(Vector3 forceVector, Vector3 forcePosition)
    {
        // Set rigidbody to NOT isKinematic
        enemyRigidbody.isKinematic = false;

        // Add impulse force to enemy
        enemyRigidbody.AddForceAtPosition(forceVector, forcePosition, ForceMode.Impulse);
    }
}
