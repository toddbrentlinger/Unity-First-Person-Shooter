using UnityEngine;
using System.Collections;

public class HoldObject02 : MonoBehaviour {

    private float m_maxDistance = 2.0f;
    private float m_distFromPlayer = 1.0f;

    private float m_throwForce = 1.0f;

    private Camera m_mainCamera;
    private Transform m_jointTransform;
    private Rigidbody m_connectedBody;

    private bool m_attachCenterOfMass = true;

    private Ray m_ray;
    private RaycastHit m_hit;

    // Use this for initialization
    void Start () {
        m_mainCamera = Camera.main;
	}
	
	// Update is called once per frame
	void Update () {
        if (!Input.GetMouseButtonDown(0))
            return;

        m_ray = m_mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (!Physics.Raycast(m_ray, out m_hit, m_maxDistance))
            return;

        if (!m_hit.rigidbody || m_hit.rigidbody.isKinematic)
            return;

        // Set up joint
        if (!m_jointTransform)
        {
            GameObject joint = new GameObject("JointTarget") as GameObject;
            m_jointTransform = joint.GetComponent<Transform>();
            m_jointTransform.SetParent(m_mainCamera.transform);
        }

        // Move joint to front of mainCamera
        m_jointTransform.position = m_mainCamera.ViewportToWorldPoint(
            new Vector3(0.5f, 0.3f, m_distFromPlayer));

        // Connect rigidbody to joint
        m_hit.rigidbody.transform.position = m_jointTransform.position;

        // Set connectedBody data member
        m_connectedBody = m_hit.collider.GetComponent<Rigidbody>();

        if (m_attachCenterOfMass)
        {
            Vector3 anchor = transform.TransformDirection(m_hit.rigidbody.centerOfMass)
                + m_hit.rigidbody.transform.position;
            anchor = m_jointTransform.InverseTransformPoint(anchor);
            m_jointTransform.localPosition = anchor;
        }
        else
            m_jointTransform.localPosition = Vector3.zero;

        StartCoroutine("DragObject");
    }

    void LateUpdate()
    {

    }

    IEnumerator DragObject()
    {
        // connectedBody.useGravity = false;
        m_connectedBody.isKinematic = true;

        while (Input.GetMouseButton(0) && m_connectedBody)
        {
            m_jointTransform.position = m_mainCamera.ViewportToWorldPoint(
            new Vector3(0.5f, 0.3f, m_distFromPlayer));

            // Restrict connectedBody distance from camera to distFromPlayer

            // springJoint.transform.LookAt(mainCamera.transform.position);
            // springJoint.connectedBody.transform.LookAt(mainCamera.transform);

            yield return null;
        }
    }

    void ThrowObject()
    {
        Vector3 connectedBodyTransform = m_connectedBody.transform.position;
        Vector3 mainCamTransform = Camera.main.transform.position;
        Vector3 directionVector = (connectedBodyTransform - mainCamTransform).normalized;
        m_connectedBody.AddForce(directionVector * m_throwForce, ForceMode.Impulse);
    }
}
