using UnityEngine;

/* NOTES:
 * _ In OnCollider___ series of functions, if door isMoving and collides with 
 * another collider, add horizontal force/velocity to other rigidbody in direction
 * normal to hit.point
 * _ Use Rotate(Vector3 axis, float angle) and RotateAround(Vector3 point, Vector3 axis, float angle)
 * Rotate(Vector3.up, angularSpeed * Time.deltaTime)
 * _ Use Quaternion.Slerp(from.rotation, to.rotation, t[0,1])
 * For parameter t, translate from current rotation between closed and open rotation (using InverseLerp or InverseSlerp?)
 * _ Get door collider in awake with GetComponentsInChildren<Collider>() and choose first non-trigger collider
 * _ Use dot product to test door opening direction. Compare normal vector from camera and raycast hit point normal.
 * _ Door Swing - Raycast from player camera to see if player is looking at door.
 * If true, change to hand cursor. If false, make sure original cursor is shown.
 * _ ISSUE: isMoving is always true
 * SOLUTION: If isMoving is true, check if door swing is in final position depending on value of m_open
 */

public class DoorControllerOld : MonoBehaviour
{
    private enum DoorState { Open, Closed, Moving, Locked };

    [SerializeField] private bool m_lockDoor = false;

    [SerializeField] private float m_openAngle = 120f;
    [SerializeField] private float m_closedAngle = 0f;
    [SerializeField] private float m_doorWidth = .1f;
    [SerializeField] private float m_swingSmoothFactor = 2f;
    [SerializeField] private float m_rayDistanceFromDoor = 4f;

    private DoorState m_currentDoorState;

    private bool m_open;
    private bool m_canInteract;
    private bool m_isMoving = false;
    private bool m_positiveSwing = true;

    private Collider m_doorCollider;

    //private GameObject m_negativeSwingPivot;

    // Use this to assign references
    private void Awake()
    {
        // Set original rotation representing closed rotation
        //m_closedRotation = transform.rotation.eulerAngles;
        //m_closedRotationY = transform.rotation.eulerAngles.y;

        //m_doorRigidbody = m_doorCollider.GetComponent<Rigidbody>();
        Collider[] colliders = GetComponents<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            if (!colliders[i].isTrigger)
            {
                m_doorCollider = colliders[i];
                break;
            }
        }
    }

    // Use this for variable initialization
    private void Start()
    {
        if (m_lockDoor)
            m_currentDoorState = DoorState.Locked;
        else
            m_currentDoorState = DoorState.Closed;

        m_canInteract = false;
        m_open = false;
        m_isMoving = false;
    }
    //private float tempDotProduct;
    //private bool tempDoorHit;
    private void Update()
    {
        // Ensure door finishes moving if Player exits trigger
        if (m_isMoving)
        {
            SwingDoor();
        }

        // Return if Player is not within trigger
        if (m_lockDoor || !m_canInteract)
            return;

        //Debug.DrawRay(Camera.main.ViewportToWorldPoint(new Vector3(.5f, .5f, 0)),
        //    Camera.main.transform.forward * m_rayDistanceFromDoor, Color.white);
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleSwingDirection();
        }

        CheckDoorRotation();

        Debug.Log("IsOpen: " + m_open + " - IsMoving: " + m_isMoving + " - PositiveSwing: " + m_positiveSwing);
    }

    private void SwingDoor()
    {
        Quaternion targetRotation;
        // Set targetRotation
        // If door is to be opened
        if (m_open)
        {
            // Set door rotation parameters for opening door
            //from = m_closedRotation.y;
            //to = m_closedRotation.y - m_openAngle;
            if (!m_positiveSwing)
            {
                targetRotation = Quaternion.Euler(0, -m_openAngle, 0);

                // Dampen towards target rotation around axis shifted by doorWidth
                //Vector3 point = transform.position - transform.forward * m_doorWidth;
            }
            else
            {
                targetRotation = Quaternion.Euler(0, m_openAngle, 0);
            }
        }
        // Else door is to be closed
        else
        {
            // Set door rotation parameters for closing door
            //from = m_closedRotation.y - m_openAngle;
            //to = m_closedRotation.y;

            targetRotation = Quaternion.Euler(0, m_closedAngle, 0);
        }

        // Dampen towards target rotation
        transform.localRotation = Quaternion.Slerp(transform.localRotation,
            targetRotation, Time.deltaTime * m_swingSmoothFactor);

        //RotateDoor(to, from);
    }

    private void ToggleSwingDirection()
    {
        // Create center viewport point to ray
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0));
        RaycastHit hit;

        // Cast Ray that ignores all Colliders except doorCollider
        if (m_doorCollider.Raycast(ray, out hit, m_rayDistanceFromDoor))
        {
            // If door is NOT already moving, set isMoving to true
            if (!m_isMoving)
                m_isMoving = true;

            // If door is previously closed
            if (!m_open)
            {
                // Get dot product of normal vector from hit point and ray direction (from camera to hit point)
                float dotProduct = Vector3.Dot(transform.right, ray.direction);
                //tempDotProduct = dotProduct;
                // If dot product is negative, spin door reverse around position shifted away from origin by width of door
                if (dotProduct < 0)
                {
                    m_positiveSwing = false;
                }
                // Else dot product is positive or zero, spin door normally around origin
                else
                {
                    m_positiveSwing = true;
                }
                // Only applies to opening door, clicking from any direction closes door
            }

            // Toggle open bool
            m_open = !m_open;
        }
    }

    private void CheckDoorRotation()
    {
        if (!m_isMoving)
            return;

        if (m_open)
        {
            if (m_positiveSwing)
            {
                if (transform.localRotation.y < m_openAngle)
                    return;
            }
            else
            {
                if (transform.localRotation.y > -m_openAngle)
                    return;
            }
        }
        else
        {
            if (transform.localRotation.y > m_closedAngle || transform.localRotation.y < m_closedAngle)
                return;
        }

        m_isMoving = false;
    }

    /*
    private void RotateDoor(float to, float from)
    {
        float currentRotation = transform.rotation.eulerAngles.y;
        float parameter = Mathf.InverseLerp(to, from, currentRotation);
        float rotation = Mathf.SmoothStep(from, to, parameter);
        //transform.Rotate(Vector3.up * Mathf.SmoothStep(to, from, parameter));
        //transform.rotation = Quaternion.Euler(new Vector3(m_closedRotation.x,
        //    rotation, m_closedRotation.z));
    }
    */

    // ---------- OnTrigger ----------

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            m_canInteract = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            m_canInteract = false;
        }
    }
    /*
    private void CreateNegativeSwingPivot()
    {
        m_negativeSwingPivot = new GameObject("Negative Swing Pivot");
        m_negativeSwingPivot.transform.SetParent(transform);
        m_negativeSwingPivot.transform.localPosition = new Vector3();
    }
    */
}
