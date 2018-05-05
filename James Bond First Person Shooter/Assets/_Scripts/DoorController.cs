using System.Collections;
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

public class DoorController : MonoBehaviour
{
    private enum DoorState { Open, Closed, Moving, Locked };

    [SerializeField] private bool m_lockDoor;

    [SerializeField] private float m_openAngle = 120f;
    [SerializeField] private float m_closedAngle = 0f;
    [SerializeField] private float m_swingDuration = 1f;
    [SerializeField] private float m_doorWidth = .1f;
    [SerializeField] private float m_swingSmoothFactor = 2f;
    [SerializeField] private float m_rayDistanceFromDoor = 4f;

    [SerializeField] private AnimationCurve m_doorSwingCurve = new AnimationCurve(
        new Keyframe(0f, 0f), new Keyframe(1f, 120f));

    [SerializeField] private Transform m_positiveSwingPivot;
    [SerializeField] private Transform m_negativeSwingPivot;

    private DoorState m_currentDoorState;

    private bool m_open;
    private bool m_canInteract;
    private bool m_isMoving;
    private bool m_positiveSwing;

    private Collider m_doorCollider;

    //private GameObject m_negativeSwingPivot;

    // Use this to assign references
    private void Awake()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
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
        m_positiveSwing = true;
    }

    private void Update()
    {
        // Return if door is locked, Player is not within trigger, or door is moving
        if (m_lockDoor || !m_canInteract || m_isMoving)
            return;
        
        if (Input.GetKeyDown(KeyCode.E))
            ToggleSwingDirection();

        //Debug.DrawRay(Camera.main.ViewportToWorldPoint(new Vector3(.5f, .5f, 0)),
        //    Camera.main.transform.forward * m_rayDistanceFromDoor, Color.white);
        //Debug.Log("IsOpen: " + m_open + " - IsMoving: " + m_isMoving + " - PositiveSwing: " + m_positiveSwing);
    }

    private IEnumerator SwingDoor01()
    {
        float from, to;
        if (m_open)
        {
            from = m_closedAngle;
            to = m_positiveSwing ? m_openAngle : -m_openAngle;
        }
        else
        {
            from = m_positiveSwing ? m_openAngle : -m_openAngle;
            to = m_closedAngle;
        }

        Vector3 startRotation = transform.eulerAngles;
        float startTime = Time.time;
        float t, angle;
        do
        {
            t = (Time.time - startTime) / m_swingDuration;
            angle = Mathf.SmoothStep(from, to, t);

            if (m_positiveSwing)
                m_positiveSwingPivot.localRotation = Quaternion.Euler(startRotation.x, angle, startRotation.z);
            else
                m_negativeSwingPivot.localRotation = Quaternion.Euler(startRotation.x, angle, startRotation.z);

            yield return null;
        } while (t <= 1);

        // Set currentDoorState open / closed and isMoving to false
        m_isMoving = false;

        CheckDoorState();
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
            StartCoroutine(SwingDoor01());
        }
    }

    private void CheckDoorState()
    {
        if (m_isMoving)
        {
            m_currentDoorState = DoorState.Moving;
        }
        else
        {
            if (m_open)
                m_currentDoorState = DoorState.Open;
            else
            {
                if (m_lockDoor)
                    m_currentDoorState = DoorState.Locked;
                else
                    m_currentDoorState = DoorState.Closed;
            }
        }
    }

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
}
