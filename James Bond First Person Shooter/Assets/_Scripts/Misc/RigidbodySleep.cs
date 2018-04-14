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
}
