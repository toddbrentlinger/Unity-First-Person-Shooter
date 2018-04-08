using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodySleep : MonoBehaviour {

    private Rigidbody[] m_rigidbodies;

	// Use this for initialization
	void Start ()
    {
        m_rigidbodies = GetComponentsInChildren<Rigidbody>();
        
        foreach(Rigidbody rb in m_rigidbodies)
        {
            rb.Sleep();
        }	
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
