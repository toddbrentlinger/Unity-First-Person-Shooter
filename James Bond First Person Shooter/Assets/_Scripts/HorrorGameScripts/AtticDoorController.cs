using UnityEngine;
using System.Collections;

public class AtticDoorController : MonoBehaviour {

    private Transform m_atticDoor;
    [SerializeField] private Transform m_ladderBottom;

    private Renderer m_atticDoorRenderer;
    private Collider m_atticDoorCollider;
    private float m_maxDistance = 2.0f;

    private Vector3 m_atticDoorOpenRotation;
    private Vector3 m_ladderBottomOpenRotation;

    private float m_atticDoorCloseAngle = 270f;
    private float m_ladderBottomCloseAngle = 90f;

    private float m_atticDoorDuration = 1.0f;
    private float m_ladderBottomDuration = 1.0f;

    private bool m_enter;
    private bool m_isMoving;
    private bool m_open;

	// Use this for initialization
	void Start () {
        m_atticDoor = gameObject.GetComponent<Transform>();

        m_atticDoorRenderer = gameObject.GetComponent<Renderer>();
        m_atticDoorCollider = gameObject.GetComponent<BoxCollider>();

        // Add BoxCollider component as trigger
        CreateBoxColliderTrigger(gameObject);

        m_atticDoorOpenRotation = m_atticDoor.rotation.eulerAngles;
        m_ladderBottomOpenRotation = m_ladderBottom.localRotation.eulerAngles;

        m_enter = false;
        m_isMoving = false;
        m_open = false;

        StartCoroutine("CloseDoor");
    }
	
	// Update is called once per frame
	void Update () {
        if (!m_enter || m_isMoving)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;
            if (m_atticDoorCollider.Raycast(ray, out hit, m_maxDistance))
            {
                m_isMoving = true;
                if (m_open)
                    StartCoroutine("CloseDoor");
                else
                    StartCoroutine("OpenDoor");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            m_enter = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            m_enter = false;
    }

    IEnumerator OpenDoor()
    {
        // Open atticDoor
        float from = m_atticDoorCloseAngle;
        float to = m_atticDoorOpenRotation.x;
        float startTime = Time.time;
        float duration = m_atticDoorDuration;
        float timeRatio = 0f;
        float angle;
        while (timeRatio < 1.0f)
        {
            timeRatio = (Time.time - startTime) / duration;

            angle = Mathf.SmoothStep(from, to, timeRatio);

            m_atticDoor.rotation = Quaternion.Euler(new Vector3(angle, m_atticDoorOpenRotation.y,
                m_atticDoorOpenRotation.z));

            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        // Open ladderBottom
        from = m_ladderBottomCloseAngle;
        to = m_ladderBottomOpenRotation.y;
        startTime = Time.time;
        duration = m_ladderBottomDuration;
        timeRatio = 0f;
        while (timeRatio < 1.0f)
        {
            timeRatio = (Time.time - startTime) / duration;

            angle = Mathf.SmoothStep(from, to, timeRatio);

            m_ladderBottom.localRotation = Quaternion.Euler(new Vector3(m_ladderBottomOpenRotation.x,
                angle, m_ladderBottomOpenRotation.z));

            yield return null;
        }

        m_open = true;
        m_isMoving = false;
    }

    IEnumerator CloseDoor()
    {
        // Close ladderBottom
        float from = m_ladderBottomOpenRotation.y;
        float to = m_ladderBottomCloseAngle;
        float startTime = Time.time;
        float duration = m_ladderBottomDuration;
        float timeRatio = 0f;
        float angle;
        while (timeRatio < 1.0f)
        {
            timeRatio = (Time.time - startTime) / duration;

            angle = Mathf.SmoothStep(from, to, timeRatio);

            m_ladderBottom.localRotation = Quaternion.Euler(new Vector3(m_ladderBottomOpenRotation.x,
                angle, m_ladderBottomOpenRotation.z));

            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        // Close atticDoor
        from = m_atticDoorOpenRotation.x;
        to = m_atticDoorCloseAngle;
        startTime = Time.time;
        duration = m_atticDoorDuration;
        timeRatio = 0f;
        while (timeRatio < 1.0f)
        {
            timeRatio = (Time.time - startTime) / duration;

            angle = Mathf.SmoothStep(from, to, timeRatio);

            m_atticDoor.rotation = Quaternion.Euler(new Vector3(angle, m_atticDoorOpenRotation.y,
                m_atticDoorOpenRotation.z));

            yield return null;
        }

        m_open = false;
        m_isMoving = false;
    }

    private void CreateBoxColliderTrigger(GameObject targetGameObject)
    {
        BoxCollider collider = targetGameObject.AddComponent<BoxCollider>();

        collider.center = gameObject.transform.InverseTransformPoint(m_atticDoorCollider.bounds.center);
        collider.size = new Vector3(4.5f, 2.5f, 6.0f);
        collider.isTrigger = true;
    }
}
