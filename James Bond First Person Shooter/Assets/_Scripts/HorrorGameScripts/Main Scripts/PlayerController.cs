using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    [SerializeField] private float m_maxDistance = 4.0f;
    [SerializeField] private float m_distFromPlayer = 2.0f;

    private Camera m_cam;
    private GameObject m_object;
    private bool m_grabObj;

	// Use this for initialization
	void Start () {
        m_cam = Camera.main;
        m_grabObj = false;
        m_object = null;
	}

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!m_grabObj)
            {
                Ray ray = m_cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, m_maxDistance))
                {
                    if (!hit.collider.CompareTag("Player") &&
                        hit.rigidbody && !hit.rigidbody.isKinematic)
                    {
                        m_object = hit.transform.gameObject;
                        m_object.GetComponent<Rigidbody>().isKinematic = true;
                        m_object.GetComponent<Rigidbody>().useGravity = false;
                        m_grabObj = true;
                    }
                }
            }
            else
            {
                m_grabObj = false;
                m_object.GetComponent<Rigidbody>().useGravity = true;
                m_object.GetComponent<Rigidbody>().isKinematic = false;
                m_object.GetComponent<Rigidbody>().velocity = Vector3.zero;
                m_object = null;
            }
        }

        if (m_grabObj)
        {
            m_object.transform.position = m_cam.ViewportToWorldPoint(
                new Vector3(0.5f, 0.4f, m_distFromPlayer));
        }
    }
}
