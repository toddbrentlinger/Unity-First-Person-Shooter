using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* NOTES:
 * - Jumping state when presses Jump button and travelling upwards. Falling state begins as soon
 * as player starts travelling downward.
 */

// Enumeration of Movement to choose between state of movement
public enum MoveState { Idle, Walking, Running, Jumping, Falling, Crouching, Leaning, Sliding };

public class FPSController : MonoBehaviour {
    /*
    [System.Serializable]
    public class Movement
    {
        public float moveSpeed;
        [Range(0f, 1f)] float moveBackwardFactor ;
        [Range(0f, 1f)] float moveSideFactor;

        public Movement(float speed = 5f, float backwardFactor = .8f, float sideFactor = .8f)
        {
            moveSpeed = speed;
            moveBackwardFactor = backwardFactor;
            moveSideFactor = sideFactor;
        }
    }
    */
    [Header("General")]
    [SerializeField] private MoveState m_moveState = MoveState.Idle;
    public bool m_playerControl = true;

    [Header("Movement")]
    [SerializeField] [Range(0f, 1f)] private float m_moveBackwardFactor = .8f;
    [SerializeField] [Range(0f, 1f)] private float m_moveSideFactor = .8f;
    [SerializeField] private float m_antiBumpFactor = .75f;
    [SerializeField] private bool m_airControl = true;
    [SerializeField] private bool m_airAssist = false;
    [SerializeField] [Range(0f,1f)] private float m_airSpeed = 2f;
    private CharacterController m_characterController;
    private bool m_grounded = true;
    private float m_speed;
    private Vector3 m_moveDirection = Vector3.zero;
    private Vector3 m_moveVelocity = Vector3.zero;
    //private float m_stepCycle; // for step audio
    //private float m_nextStep; // for step audio

    [Header("Walking")]
    [SerializeField] private float m_walkSpeed = 5f;
    //[SerializeField] private float m_stepInterval = 5f; // for step audio

    [Header("Running")]
    [SerializeField] private float m_runSpeed = 8f;
    //[SerializeField] [Range(0f, 1f)] private float m_runstepLenghten = .7f; // for step audio and weaponBob

    [Header("Jumping")]
    [SerializeField] private float m_jumpSpeed = 7f;
    //Player must be grounded for at least this many physics frames before being able to jump again; set to 0 to allow bunny hopping
    [SerializeField] private float m_framesGroundedBetweenJumps = 1;
    private float m_jumpFrameCounter;

    [Header("Falling")]
    [SerializeField] private float m_fallingDamageThreshold = 10f;
    private float m_fallStartLevel;
    private bool m_falling = false;

    [Header("Crouching")]
    [SerializeField] private float m_crouchHeight = 1f;
    [SerializeField] private float m_crouchDropSpeed = 2f;
    [SerializeField] private float m_crouchSpeed = 3f;
    [SerializeField] [Range(0f, 1f)] private float m_crouchStepReduction = .8f;
    [SerializeField] bool m_startCrouched = false;
    private bool m_crouching;
    private float m_originalCameraLocalHeight;
    private float m_originalCharacterControllerHeight;
    private float m_originalCharacterControllerCenterY;

    [Header("Leaning")]

    [Header("Sliding")]

    [Header("Camera")]
    [SerializeField] private FPSMouseLook m_mouseLook;
    private Camera m_fpsCamera;

    [Header("Physics")]
    [SerializeField] private float m_gravity = 20f;
    [SerializeField] private float m_pushPower = .1f;

    [Header("Audio")]
    [SerializeField] private AudioClip[] m_footstepSounds; // array of footstep sounds that will be randomly selected from.
    [SerializeField] private AudioClip m_jumpSound; // sound played when character leaves the ground.
    [SerializeField] private AudioClip m_landSound; // sound played when character touches back on ground.
    private AudioSource m_audioSource;

    // Properties
    public float Speed
    {
        get { return m_speed; }
    }
    public MoveState CurrentMoveState
    {
        get { return m_moveState; }
    }

    private void Awake()
    {
        m_characterController = GetComponent<CharacterController>();
        m_fpsCamera = Camera.main; // GetComponentInChildren<Camera>() but there is another camera that renders only weapons
    }

	private void Start ()
    {
        m_mouseLook.Init(transform, m_fpsCamera.transform);
        m_speed = m_walkSpeed;
        m_jumpFrameCounter = m_framesGroundedBetweenJumps;

        // Crouching
        m_originalCameraLocalHeight = m_fpsCamera.transform.localPosition.y;
        m_originalCharacterControllerHeight = m_characterController.height;
        m_originalCharacterControllerCenterY = m_characterController.center.y;

        m_crouching = m_startCrouched ? true : false;
        CheckCrouch();
    }
	
	private void Update ()
    {
        if (!m_playerControl)
            return;

        // Rotate fpsCamera and CharacterController depending on MouseX/MouseY input
        m_mouseLook.LookRotation();

        if (m_grounded)
        {
            // Check if player was falling, and fell a vertical distance greater than the threshold, run a falling damage routine
            if (m_falling)
            {
                m_falling = false;
                if (transform.position.y < m_fallStartLevel - m_fallingDamageThreshold)
                    FallingDamageAlert(m_fallStartLevel - transform.position.y);
            }

            // Set speed (either walking, running, crouching)
            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (m_crouching)
                    m_crouching = false;

                m_speed = m_runSpeed;
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftControl))
                    m_crouching = !m_crouching;

                m_speed = m_crouching ? m_crouchSpeed : m_walkSpeed;
            }
        }
        // Else player is NOT grounded
        else
        {
            if (!m_falling)
            {
                m_falling = true;
                m_fallStartLevel = transform.position.y;
            }
        }

        // Move CharacterController depending on Horizontal/Vertical input and speed
        // Checks for jumping
        MovePosition();

        // Check crouch
        CheckCrouch();

        // Update moveState
        UpdateMoveState();

        //Debug.Log("Grounded: " + m_grounded + " - Falling: " + m_falling + " - Crouching: " + m_crouching);
    }

    // Move CharacterController depending on Horizontal/Vertical input and speed
    private void MovePosition()
    {
        // Get input Vector2
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (input.y < 0)
            input.y *= m_moveBackwardFactor;
        if (input.x != 0)
            input.x *= m_moveSideFactor;
        if (input.sqrMagnitude > 1)
            input.Normalize();

        if (m_grounded)
        {
            // Set desired move Vector3
            m_moveDirection = transform.forward * input.y + transform.right * input.x;

            // Get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position + m_characterController.center, m_characterController.radius, Vector3.down, out hitInfo,
                               m_characterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            // Get normalized Vector3 projected on plane of surface being touched
            m_moveDirection = Vector3.ProjectOnPlane(m_moveDirection, hitInfo.normal);
            // Set downward force to stick to ground when walking on slopes
            m_moveDirection.y = -m_antiBumpFactor;

            // Get move velocity Vector3 by multiplying speed to moveDirection Vector3
            m_moveVelocity = m_moveDirection * m_speed;

            /*
            // Forward/Backward
            m_moveVelocity.z = m_moveDirection.z * m_speed;
            if (m_moveDirection.z < 0)
                m_moveVelocity.z *= m_moveBackwardFactor;
            // Left/Right
            m_moveVelocity.x = m_moveDirection.x * m_speed;
            if (m_moveDirection.x < 0 || m_moveDirection.x > 0)
                m_moveVelocity.x *= m_moveSideFactor;
            // Up/Down (antiBumpFactor)
            // Set downward force to stick to ground when walking on slopes
            m_moveVelocity.y = -m_antiBumpFactor;
            */

            // Check jumping
            if (!Input.GetButton("Jump"))
                m_jumpFrameCounter++;
            else if (m_jumpFrameCounter >= m_framesGroundedBetweenJumps)
            {
                m_moveVelocity.y = m_jumpSpeed;
                m_jumpFrameCounter = 0;

                if (m_crouching)
                {
                    m_crouching = false;
                }
            }
        }
        // Else player is NOT grounded
        else
        {
            // If airControl is allowed (DON'T change y-velocity; let gravity accumulate)
            if (m_airControl)
            {   
                m_moveVelocity.x = input.x * m_speed;
                m_moveVelocity.z = input.y * m_speed;
                m_moveVelocity = transform.TransformDirection(m_moveVelocity);
            }
            else if (m_airAssist)
            {
                // NOTE: Should I use SimpleMove instead of Move? SimpleMove takes velocity argument and automatically applies gravity.
                m_moveVelocity.x = input.x * m_airSpeed;
                m_moveVelocity.z = input.y * m_airSpeed;
                m_moveVelocity = m_characterController.velocity + transform.TransformDirection(m_moveVelocity);
            }
        }

        // Apply gravity
        m_moveVelocity.y -= m_gravity * Time.deltaTime; // acceleration x time = velocity

        // Move character controller, and set grounded depending on whether player is standing on collider
        m_grounded = (m_characterController.Move(m_moveVelocity * Time.deltaTime) & CollisionFlags.Below) != 0;

        Debug.Log("Grounded: " + m_grounded + " - input: " + input + " - desiredMove: " + transform.InverseTransformDirection(m_moveDirection) + " - moveVelocity: " + transform.InverseTransformDirection(m_moveVelocity));
    }

    // Crouching
    private void CheckCrouch()
    {
        if (m_crouching)
        {
            // Set character controller height
            if (m_characterController.height != m_crouchHeight)
                m_characterController.height = Mathf.MoveTowards(m_characterController.height, m_crouchHeight, Time.deltaTime * m_crouchSpeed);

            // Set character controller center
            if (m_characterController.center.y != m_originalCharacterControllerCenterY - m_crouchHeight * .5f)
                m_characterController.center = new Vector3(m_characterController.center.x,
                    m_originalCharacterControllerCenterY - (m_originalCharacterControllerHeight - m_characterController.height) * .5f,
                    m_characterController.center.z);

            // Set camera height
            if (m_fpsCamera.transform.localPosition.y != m_crouchHeight)
                m_fpsCamera.transform.localPosition = new Vector3(m_fpsCamera.transform.localPosition.x,
                    Mathf.MoveTowards(m_fpsCamera.transform.localPosition.y, m_crouchHeight, Time.deltaTime * m_crouchSpeed),
                    m_fpsCamera.transform.localPosition.z);
        }
        else
        {
            // Reset character controller height
            if (m_characterController.height != m_originalCharacterControllerHeight)
                m_characterController.height = Mathf.MoveTowards(m_characterController.height, m_originalCharacterControllerHeight, Time.deltaTime * m_crouchSpeed);

            // Reset character controller center
            if (m_characterController.center.y != m_originalCharacterControllerCenterY)
                m_characterController.center = new Vector3(m_characterController.center.x,
                    m_originalCharacterControllerCenterY - (m_originalCharacterControllerHeight - m_characterController.height) * .5f,
                    m_characterController.center.z);

            // Reset camera height
            if (m_fpsCamera.transform.localPosition.y != m_originalCameraLocalHeight)
                m_fpsCamera.transform.localPosition = new Vector3(m_fpsCamera.transform.localPosition.x,
                    Mathf.MoveTowards(m_fpsCamera.transform.localPosition.y, m_originalCameraLocalHeight, Time.deltaTime * m_crouchSpeed),
                    m_fpsCamera.transform.localPosition.z);
        }
    }

    // Falling
    // If falling damage occured, this is the place to do something about it. You can make the player
    // have hitpoints and remove some of them based on the distance fallen, add sound effects, etc.
    private void FallingDamageAlert(float fallDistance)
    {
        print("Ouch! Fell " + fallDistance + " units!");
    }

    private void UpdateMoveState()
    {
        if (!m_grounded)
        {
            if (m_characterController.velocity.y < 0)
                m_moveState = MoveState.Falling;
            else
                m_moveState = MoveState.Jumping;
            return;
        }

        if (m_crouching)
        {
            m_moveState = MoveState.Crouching;
            return;
        }

        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Vector2 rotateInput = new Vector2(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));

        if (moveInput.sqrMagnitude == 0 && rotateInput.sqrMagnitude == 0)
        {
            m_moveState = MoveState.Idle;
            return;
        }

        if (m_speed == m_runSpeed)
            m_moveState = MoveState.Running;
        else if (m_speed == m_walkSpeed)
            m_moveState = MoveState.Walking;
    }
    /*
    private void OnGUI()
    {
        GUI.Label(new Rect(0, 0, 200, 20), "Grounded: " + m_grounded + " - desiredMove: " + transform.InverseTransformDirection(m_moveDirection) + " - moveVelocity: " + transform.InverseTransformDirection(m_moveVelocity));
    }
    */
}
