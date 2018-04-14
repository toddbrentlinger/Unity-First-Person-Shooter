using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Target : MonoBehaviour, IDamageable
{
    private float m_minAngularSpeed = 1f; // (radians/sec)
    private Rigidbody m_rigidbody;
    private Transform m_camera;
    private bool m_isSpinning = false;

	private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_camera = Camera.main.transform;
    }
	
    private void Update()
    {
        if (m_isSpinning)
        {
            // Check if rotational velocity is below certain limit, and then slerp from current rotation
            // to nearest LookAt rotatation (target can face Player from either side)

            if (m_rigidbody.angularVelocity.sqrMagnitude < m_minAngularSpeed * m_minAngularSpeed)
            {
                // Start LookAt behaviour
                m_rigidbody.isKinematic = true;
                m_isSpinning = false;
            }

            Debug.Log("Angular Speed: " + m_rigidbody.angularVelocity.magnitude);
        }
    }

	private void LateUpdate ()
    {
        if (m_camera != null && !m_isSpinning)
        {
            Vector3 target = m_camera.transform.position;
            //target.y = transform.position.y;
            transform.LookAt(target);
        }
	}

    // IDamageable interface members
    public void TakeDamage(Vector3 hitPoint, Vector3 hitForce)
    {
        // If NOT spinning
        if (!m_isSpinning)
        {
            // Stop LookAt behavior by setting m_isSpinning to true
            m_isSpinning = true;

            // Turn off rigidbody kinematic behaviour
            m_rigidbody.isKinematic = false;
        }

        // Add impulse force
        m_rigidbody.AddForceAtPosition(hitForce, hitPoint, ForceMode.Impulse);
    }
}
