using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* NOTES:
 * - In OnCollider___ series of functions, if door isMoving and collides with 
 * another collider, add horizontal force/velocity to other rigidbody in direction
 * normal to hit.point
 * _ Use Rotate(Vector3 axis, float angle) and RotateAround(Vector3 point, Vector3 axis, float angle)
 * Rotate(Vector3.up, angularSpeed * Time.deltaTime)
 * _ Use Quaternion.Slerp(from.rotation, to.rotation, t[0,1])
 * For parameter t, translate from current rotation between closed and open rotation (using InverseLerp or InverseSlerp?)
 * - Get door collider in awake with GetComponentsInChildren<Collider>() and choose first non-trigger collider
 */

public class DoorController : MonoBehaviour
{
    private enum DoorState { Open, Closed, Moving, Locked };

    [SerializeField] private bool m_lockDoor = false;

    [SerializeField] private float m_openAngle = 90f;
    [SerializeField] private float m_closedAngle = 0f;
    //[SerializeField] private float m_openTime = 2f;
    [SerializeField] private float m_swingSmoothFactor = 2f;
    [SerializeField] private float m_rayDistanceFromDoor = 2f;

    private DoorState m_currentDoorState;

    private bool m_open;
    private bool m_canInteract;
    private bool m_isMoving;

    //private Vector3 m_closedRotation;
    //private float m_closedRotationY;

    private Collider m_doorCollider;
    //private Rigidbody m_doorRigidbody;

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

    // Update is called once per frame
    private void Update()
    {
        // First in the function to ensure door finishes moving if Player exits trigger
        if (m_isMoving)
        {
            Quaternion targetRotation;

            if (m_open)
            {
                // Set door rotation parameters for opening door
                //from = m_closedRotation.y;
                //to = m_closedRotation.y - m_openAngle;

                targetRotation = Quaternion.Euler(0, m_openAngle, 0);
            }
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

        // Return if Player is not within trigger
        if (m_lockDoor || !m_canInteract)
            return;

        // Raycast from player camera to see if player is looking at door
        // If true, change to hand cursor
        // If false, make sure original cursor is shown

        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0));
            RaycastHit hit;

            // Cast Ray that ignores all Colliders except doorCollider
            if (m_doorCollider.Raycast(ray, out hit, m_rayDistanceFromDoor))
            {
                m_open = !m_open;
                if (!m_isMoving)
                    m_isMoving = true;
            }
        }

        //Debug.Log("CanInteract: " + m_canInteract + " - IsOpen: " + m_open + " - IsMoving: " + m_isMoving);
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
