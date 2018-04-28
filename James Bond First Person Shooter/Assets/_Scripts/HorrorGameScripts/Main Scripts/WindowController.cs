using UnityEngine;
using System.Collections;

public class WindowController : MonoBehaviour {
    
    [SerializeField] private Transform windowBottom;

    private Collider m_windowBottomCollider;
    private float m_maxDistance = 2.0f;
    private float m_openHeight = 0.6f;
    private float m_duration = 1.0f;
    private float m_closedHeight;

    private bool m_enter;
    private bool m_isMoving;
    private bool m_open;

	// Use this for initialization
	void Start () {
        m_windowBottomCollider = windowBottom.GetComponent<Collider>();

        CreateBoxColliderTrigger(gameObject);

        m_closedHeight = windowBottom.localPosition.z;

        m_enter = false;
        m_isMoving = false;
        m_open = false;
	}
	
	// Update is called once per frame
	void Update () {
        if (!m_enter || m_isMoving)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;
            if (m_windowBottomCollider.Raycast(ray, out hit, m_maxDistance))
            {
                m_isMoving = true;
                StartCoroutine("MoveWindow");
            }
        }
    }

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player"))
            m_enter = true;
    }

    void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player"))
            m_enter = false;
    }

    IEnumerator MoveWindow()
    {
        float from, to;
        if (!m_open)
        {
            from = m_closedHeight;
            to = m_closedHeight + m_openHeight;
        }
        else
        {
            from = m_closedHeight + m_openHeight;
            to = m_closedHeight;
        }

        float startTime = Time.time;
        float timeRatio = 0f;
        float height;
        while (timeRatio < 1.0f)
        {
            timeRatio = (Time.time - startTime) / m_duration;

            height = Mathf.SmoothStep(from, to, timeRatio);

            windowBottom.localPosition = new Vector3(windowBottom.localPosition.x, windowBottom.localPosition.y, height);

            yield return null;
        }

        m_open = !m_open;
        m_isMoving = false;
    }

    private void CreateBoxColliderTrigger(GameObject targetGameObject)
    {
        BoxCollider collider = targetGameObject.AddComponent<BoxCollider>();
        Vector3 targetCenter = windowBottom.GetComponent<Renderer>().bounds.center;

        collider.center = gameObject.transform.InverseTransformPoint(targetCenter);
        collider.size = new Vector3(2.5f, 3.0f, 2.5f);
        collider.isTrigger = true;
    }
}
